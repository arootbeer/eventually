using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses;
using Eventually.Interfaces.DomainEvents;
using Eventually.Utilities.Extensions;
using Fasterflect;
using Microsoft.Extensions.Logging;
using NEventStore.Domain;
using NEventStore.Domain.Persistence;

namespace Eventually.Domain.Runtime
{
    public class DomainCommandExecutor : IDomainCommandExecutor
    {
        private readonly Dictionary<Type, (Type type, MethodInfo handler)> _changeCommandHandlerMapping = new();
        private readonly Dictionary<Type, MethodInfo> _createCommandHandlerMapping = new();

        private readonly IRepository _repository;
        private readonly IKnownAggregateTypeProvider _knownAggregateTypeProvider;
        private readonly ILogger _logger;

        public DomainCommandExecutor(
            IRepository repository,
            IKnownAggregateTypeProvider knownAggregateTypeProvider,
            ILoggerFactory loggerFactory
        )
        {
            _repository = repository;
            _knownAggregateTypeProvider = knownAggregateTypeProvider;
            _logger = loggerFactory.CreateLogger(nameof(DomainCommandExecutor));

            CatalogCommandHandlerMethods();
        }

        private void CatalogCommandHandlerMethods()
        {
            // Builds a mapping of command handlers methods to the types that expose them. It is an error to
            // have more than one aggregate type which handles a command type.
            // TODO: allow runtime updating/reloading of types?
            try
            {
                foreach (var aggregateType in _knownAggregateTypeProvider.GetTypes())
                {
                    var commandMethods = aggregateType.GetMethods(Flags.InstancePublic)
                        .Where(
                            mi => mi.Parameters().Count == 1 &&
                                  typeof(DomainCommand).IsAssignableFrom(mi.Parameters()[0].ParameterType)
                        )
                        .ToList();

                    foreach (var commandMethod in commandMethods)
                    {
                        var commandType = commandMethod.Parameters()[0].ParameterType;
                        if (!typeof(ChangeEntityCommand).IsAssignableFrom(commandType))
                        {
                            throw new Exception(
                                $"Aggregate type `{aggregateType.FullName}` exposes a command handler for `{commandType.FullName}`, which does not implement `{nameof(ChangeEntityCommand)}`."
                            );
                        }

                        _changeCommandHandlerMapping.Add(commandType, (aggregateType, commandMethod));
                    }

                    var baseType = aggregateType.BaseType ?? typeof(object);
                    do
                    {
                        if (baseType.IsGenericType && typeof(AggregateBase<,,>) == baseType.GetGenericTypeDefinition())
                        {
                            _createCommandHandlerMapping.Add(
                                baseType.GenericTypeArguments[1],
                                aggregateType.Method(
                                    nameof(ConvenientUninterestingAggregateBaseImplementation.Create),
                                    Flags.StaticPublic
                                )
                            );
                            break;
                        }

                        baseType = baseType.BaseType ?? typeof(object);
                    } while (baseType != typeof(object));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An exception was encountered while attempting to map event handlers", ex);
            }
        }

        public DomainCommandResponse Execute(DomainCommand message)
        {
            DomainCommandResponse response;
            try
            {
                switch (message)
                {
                    case CreateEntityCommand createCommand:
                        var entity = Handle(createCommand);
                        response = DomainCreateCommandResponseBuilder
                            .InResponseTo(createCommand)
                            .ResultingInCreationOf(entity)
                            .Build();
                        break;
                    case ChangeEntityCommand changeCommand:
                        Handle(changeCommand);
                        response = DomainCommandResponseBuilder
                            .InResponseTo(changeCommand)
                            .Successful
                            .Build();
                        break;
                    default:
                        throw new Exception(
                            $"An unknown command of type `{message.GetType().FullName}` was encountered. The command is: {Environment.NewLine}```{message.ToJson()}```"
                        );
                }
            }
            catch (Exception ex)
            {
                response = DomainCommandResponseBuilder
                    .InResponseTo(message)
                    .Failed(ex)
                    .Build();
            }

            return response;
        }

        private IAggregate Handle(CreateEntityCommand createCommand)
        {
            var commandType = createCommand.GetType();

            var isKnownEntityType = _createCommandHandlerMapping.TryGetValue(commandType, out var createMethod);

            if (!isKnownEntityType)
            {
                throw new Exception(
                    $"The command could not be handled because no aggregate type was found for `{commandType.FullName}`"
                );
            }

            var entity = (IAggregate)createMethod.Invoke(null, new object[] { createCommand });

            var commitId = Guid.NewGuid();
            _logger.LogInformation(
                $"Saving newly created `{entity!.GetType().Name}` (Id = `{entity.Id}`) to repository in commit id `{commitId}`."
            );
            _repository.Save(entity, commitId, null);

            return entity;
        }
        
        private static readonly MethodInfo ChangeCommandHandler = typeof(DomainCommandExecutor)
            .Methods(nameof(HandleChangeCommand))
            .Single();

        private void Handle(ChangeEntityCommand changeCommand)
        {
            var commandType = changeCommand.GetType();

            var isKnownEntityType = _changeCommandHandlerMapping.TryGetValue(commandType, out var value);
            if (isKnownEntityType)
            {
                var (entityType, handler) = value;
                ChangeCommandHandler.MakeGenericMethod(entityType, commandType)
                    .Invoke(this, new object[] { changeCommand, handler });
                return;
            }

            throw new Exception(
                $"A command of type `{commandType.FullName}` was received, but no suitable handler could be found. The command is: {Environment.NewLine}```{changeCommand.ToJson()}```"
            );
        }

        private void HandleChangeCommand<TAggregate, TCommand>(TCommand command, MethodInfo handler)
            where TAggregate : class, IAggregate
            where TCommand : ChangeEntityCommand
        {
            var entity = _repository.GetById<TAggregate>(command.EntityId);
            
            handler.Invoke(entity, new object[] { command });

            var commitId = Guid.NewGuid();
            _repository.Save(entity, commitId, null);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ConvenientUninterestingAggregateBaseImplementation : 
            AggregateBase<ConvenientUninterestingAggregateBaseImplementation, CreateEntityCommand, IDomainEvent>
        {
            public ConvenientUninterestingAggregateBaseImplementation() : base(Guid.Empty) { }

            protected override void InitializeFromCreateCommand(CreateEntityCommand command)
            {
                // no-op
            }
        }
    }
}