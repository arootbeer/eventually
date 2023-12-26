using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses;
using Eventually.Interfaces.DomainEvents;
using Eventually.Utilities.Extensions;
using Fasterflect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NEventStore.Domain;
using NEventStore.Domain.Persistence;

namespace Eventually.Domain.APIHost.Controllers
{
    [ApiController]
    [Route("/domain/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly ILogger _logger;

        private readonly Dictionary<Type, (Type type, MethodInfo handler)> _changeCommandHandlerMapping = new();
        private readonly Dictionary<Type, MethodInfo> _createMethodMapping = new();

        public CommandController(
            IRepository repository,
            ILoggerFactory loggerFactory
        )
        {
            _repository = repository;
            _logger = loggerFactory.CreateLogger(nameof(CommandController));

            PopulateMappings();
        }

        // Builds a static mapping of command handlers methods to the types that expose them. It is an error to
        // have more than one aggregate type which handles a command type.
        // TODO: replace this with something like MEF, and definitely don't execute it for every request 
        private void PopulateMappings()
        {
            try
            {
                var aggregateTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(type => type.IsAssignableToGenericType(typeof(AggregateBase<,,>)))
                    .Where(type => !type.IsAbstract);
                foreach (var aggregateType in aggregateTypes)
                {
                    var commandMethods = aggregateType.GetMethods(Flags.InstancePublic)
                        .Where(mi => mi.Parameters().Count == 1 && typeof(DomainCommand).IsAssignableFrom(mi.Parameters()[0].ParameterType))
                        .ToList();

                    foreach (var commandMethod in commandMethods)
                    {
                        var commandType = commandMethod.Parameters()[0].ParameterType;
                        if (!typeof(ChangeEntityCommand).IsAssignableFrom(commandType))
                        {
                            throw new Exception($"Aggregate type `{aggregateType.FullName}` exposes a command handler for `{commandType.FullName}`, which does not implement `{nameof(ChangeEntityCommand)}`.");
                        }

                        _changeCommandHandlerMapping.Add(commandType, (aggregateType, commandMethod));
                    }

                    var baseType = aggregateType.BaseType ?? typeof(object);
                    do
                    {
                        if (baseType.IsGenericType && typeof(AggregateBase<,,>) == baseType.GetGenericTypeDefinition())
                        {
                            _createMethodMapping.Add(baseType.GenericTypeArguments[1], aggregateType.Method(nameof(AggregateHelper.Create), Flags.StaticPublic));
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
        
        [HttpPost]
        public IActionResult Execute([FromBody] DomainCommand message)
        {
            if (message is DomainCommand command)
            {
                DomainCommandResponse response;

                try
                {
                    switch (command)
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
                                $"An unknown command of type `{command.GetType().FullName}` was encountered. The command is: {Environment.NewLine}```{message.ToJson()}```");
                    }
                }
                catch (Exception ex)
                {
                    response = DomainCommandResponseBuilder
                        .InResponseTo(command)
                        .Failed(ex)
                        .Build();
                }
                
                _logger.LogDebug(response.ToString());
                return Ok(response);
            }

            return BadRequest();
        }
        
        private static readonly MethodInfo ChangeCommandHandler = typeof(CommandController)
            .Methods(nameof(HandleChangeCommand))
            .Single();
        private void Handle(ChangeEntityCommand changeCommand)
        {
            var commandType = changeCommand.GetType();

            if (_changeCommandHandlerMapping.ContainsKey(commandType))
            {
                var (entityType, handler) = _changeCommandHandlerMapping[commandType];
                ChangeCommandHandler.MakeGenericMethod(entityType, commandType)
                    .Invoke(this, new object[] {changeCommand, handler});
                return;
            }

            throw new Exception($"A command of type `{commandType.FullName}` was received, but no suitable handler could be found. The command is: {Environment.NewLine}```{changeCommand.ToJson()}```");
        }

        private void HandleChangeCommand<TAggregate, TCommand>(TCommand command, MethodInfo handler)
            where TAggregate : class, IAggregate
            where TCommand : ChangeEntityCommand
        {
            var entity = _repository.GetById<TAggregate>(command.EntityId);

            handler.Invoke(entity, new object[] {command});

            var commitId = Guid.NewGuid();
            _repository.Save(entity, commitId, null);
        }

        private IAggregate Handle(CreateEntityCommand createCommand)
        {
            var commandType = createCommand.GetType();

            if (_createMethodMapping.ContainsKey(commandType))
            {
                var createMethod = _createMethodMapping[commandType];
                var entity = (IAggregate) createMethod.Invoke(null, new object[] {createCommand});

                var commitId = Guid.NewGuid();
                _logger.LogInformation($"Saving newly created `{entity.GetType().Name}` (Id = `{entity.Id}`) to repository in commit id `{commitId}`.");
                _repository.Save(entity, commitId, null);

                return entity;
            }

            throw new Exception($"The command could not be handled because no aggregate type was found for `{commandType.FullName}`");
        }

        private class AggregateHelper : AggregateBase<AggregateHelper, CreateEntityCommand, IDomainEvent>
        {
            public AggregateHelper(Guid id) : base(id)
            {
            }

            public AggregateHelper(Guid id, IRouteEvents handler) : base(id, handler)
            {
            }

            protected override void InitializeFromCreateCommand(CreateEntityCommand command)
            {
                // no-op
            }
        }
    }
}