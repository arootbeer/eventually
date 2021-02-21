using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Infrastructure.Transport.CommandBus.Configuration;
using Eventually.Interfaces.Common.Messages;
using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses;
using Eventually.Utilities.Extensions;
using Microsoft.Extensions.Logging;

namespace Eventually.Infrastructure.Transport.CommandBus
{
    public class HTTPDomainCommandBus : IDomainCommandBus
    {
        private readonly BlockingCollection<IMessage> _outboundQueue = new();

        private readonly ConcurrentDictionary<Guid, BlockingCollection<DomainCommandResponse>> _responseBucket = new();

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpDomainCommandBusConfiguration _configuration;
        private readonly ILogger _logger;
        
        private CancellationToken CancellationToken => _cancellationTokenSource.Token;


        public HTTPDomainCommandBus(
            IHttpClientFactory httpClientFactory,
            IHttpDomainCommandBusConfiguration configuration,
            ILogger<HTTPDomainCommandBus> logger
        )
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            Task.Factory
                .StartNew(
                    SendCommand,
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                );
        }

        private void SendCommand()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
                try
                {
                    var command = _outboundQueue.Take(CancellationToken);
                    
                    _logger.LogDebug($"Got command with ID {command.Identity}");

                    var message = new CommandWrapper
                    {
                        CommandType = command.GetType().FullName,
                        CommandData = Type<Dictionary<string, object>>.From(command)
                    };
                    
                    var client = _httpClientFactory.CreateClient();
                    _configuration.ApplyTo(client);

                    _logger.LogDebug($"Domain API client created. Sending...{Environment.NewLine}" +
                                     $"{message.ToJson()} to {Environment.NewLine}{client.BaseAddress}");

                    client.PostAsJsonAsync(
                        (Uri) null,
                        message,
                        new JsonSerializerOptions(JsonSerializerDefaults.General),
                        CancellationToken
                    ).ContinueWith(HandleCommandResponse, command, CancellationToken);
                }
                catch (OperationCanceledException ocex)
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        throw new Exception(
                            "Caught an OperationCanceledException but the cancellation " +
                            "token is not canceled. Did someone call " +
                            "_responseBucket[commandIdentity].CompleteAdding() by accident?",
                            ocex
                        );
                    }

                    throw;
                }
        }

        private async void HandleCommandResponse(Task<HttpResponseMessage> requestTask, object state)
        {
            var command = (DomainCommand) state;
            DomainCommandResponse commandResponse;
            if (requestTask.IsFaulted)
            {
                commandResponse = DomainCommandResponseBuilder.InResponseTo(command)
                    .Failed(requestTask.Exception)
                    .Build();
            }
            else
            {
                _logger.LogDebug("Received response from Domain API");
                var httpResponse = requestTask.Result;
                var responseContent = await httpResponse.Content
                    .ReadAsStringAsync(CancellationToken);

                commandResponse = Type<DomainCreateCommandResponse>.FromJson(responseContent);
                if (commandResponse.Succeeded && ((DomainCreateCommandResponse)commandResponse).CreatedEntityId == default)
                {
                    commandResponse = Type<DomainCommandResponse>.FromJson(responseContent);
                }

                if (commandResponse?.CommandId != command.Identity)
                {
                    throw new Exception(
                        "The received response did not match the command which " +
                        $"was sent. The received response is {commandResponse}."
                    );
                }
            }

            _logger.LogDebug($"Command response is {commandResponse}");
            _responseBucket.TryGetValue(commandResponse.CommandId, out var responses);
            responses?.Add(commandResponse, CancellationToken);
        }

        public async Task ExecuteCommand(DomainCommand command, CancellationToken cancellationToken)
        {
            var exception = await ExecuteCommand(
                    command,
                    (response, _) => response.ToException(),
                    cancellationToken
                );

            if (exception != null)
            {
                throw exception;
            }
        }

        public async Task<TResult> ExecuteCommand<TResult>(
            DomainCommand command,
            Func<DomainCommandResponse, CancellationToken, TResult> handler,
            CancellationToken cancellationToken
        )
        {
            _responseBucket.TryAdd(
                command.Identity,
                new BlockingCollection<DomainCommandResponse>(1)
            );

            _outboundQueue.Add(command, cancellationToken);

            return await Task.Factory
                .StartNew(
                    () =>
                    {
                        DomainCommandResponse response = null;
                        if (_responseBucket.ContainsKey(command.Identity))
                        {
                            response = _responseBucket[command.Identity].Take(cancellationToken);
                        }

                        if (_responseBucket.Remove(command.Identity, out var collection))
                        {
                            collection.Dispose();
                        }

                        return handler == null || response == null ? default : handler(response, cancellationToken);
                    },
                    cancellationToken
                );
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cancellationTokenSource.Cancel();
        }
    }
}