using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Domain.ServiceHost.Configuration;
using Eventually.Infrastructure.Transport.Extensions;
using Eventually.Infrastructure.Transport.Messages;
using Eventually.Infrastructure.Transport.Sockets;
using Eventually.Interfaces.Common;
using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses;
using Eventually.Interfaces.DomainEvents;
using Eventually.Utilities.Extensions;
using Fasterflect;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using NEventStore.Domain;
using NEventStore.Domain.Persistence;

namespace Eventually.Domain.ServiceHost
{
    public class DomainServiceHost : IUniquelyIdentified, IHostedService, IDisposable
    {
        private readonly IRepository _repository;
        private readonly IWireMessageFactory _wireMessageFactory;
        private readonly ILogger _logger;

        private readonly NetMQPoller _poller;
        private readonly RouterSocket _mySocket;

        public Guid Identity { get; }

        private readonly Dictionary<Type, (Type type, MethodInfo handler)> _changeCommandHandlerMapping = new Dictionary<Type, (Type, MethodInfo)>();
        private readonly Dictionary<Type, MethodInfo> _createMethodMapping = new Dictionary<Type, MethodInfo>();

        public DomainServiceHost(
            IDomainServiceHostConfiguration hostConfiguration,
            ISocketFactory socketFactory,
            IRepository repository,
            IWireMessageFactory wireMessageFactory,
            ILoggerFactory loggerFactory
        )
        {
            _repository = repository;
            _wireMessageFactory = wireMessageFactory;

            Identity = hostConfiguration.Identity;
            _logger = loggerFactory.CreateLogger(typeof(DomainServiceHost).Name);

            PopulateMappings();

            _poller = new NetMQPoller();
            _mySocket = socketFactory.GetServerSocket(Identity, _poller, MessageReceived, hostConfiguration.Server);
        }

        // Builds a static mapping of command handlers methods to the types that expose them. It is an error to
        // have more than one aggregate type which handles a command type.
        private void PopulateMappings()
        {
            try
            {
                var assembly = typeof(AggregateBase<,,>).Assembly;
                var aggregateTypes = assembly.GetTypes()
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

        private void MessageReceived(object sender, NetMQSocketEventArgs e)
        {
            var netMQMessage = e.Socket.ReceiveMultipartMessage();
            var wireMessage = _wireMessageFactory.Create(netMQMessage);
            var message = wireMessage.Message;

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
                            throw new Exception($"An unknown command of type `{command.GetType().FullName}` was encountered. The command is: {Environment.NewLine}```{message.ToJson()}```");
                    }
                }
                catch (Exception ex)
                {
                    response = DomainCommandResponseBuilder
                        .InResponseTo(command)
                        .Failed(ex)
                        .Build();
                }

                try
                {
                    e.Socket.SendResponse(_wireMessageFactory.Create(response, this), wireMessage.SenderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to send response for {(response.Succeeded ? "successful" : "failed")} `{command.GetType().FullName}`. The response is: {Environment.NewLine}```{response.ToJson()}```", ex);
                }

                return;
            }

            throw new Exception($"An unknown message of type `{message.GetType().FullName}` was encountered. The message is: {Environment.NewLine}```{message.ToJson()}```");
        }

        private static readonly MethodInfo ChangeCommandHandler = typeof(DomainServiceHost)
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _poller.RunAsync();

            _logger.LogInformation("Domain Service Host started.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Service Host stopping.");

            _poller?.StopAsync();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogDebug("Domain Service Host is being disposed.");
            GC.SuppressFinalize(this);

            _poller?.Remove(_mySocket);
            _poller?.Dispose();
            _mySocket?.Dispose();
            
            _logger.LogInformation("DomainServiceHost has been disposed.");
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