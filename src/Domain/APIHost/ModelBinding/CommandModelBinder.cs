using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventually.Infrastructure.Transport;
using Eventually.Interfaces.DomainCommands;
using Eventually.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.APIHost.ModelBinding
{
    public class CommandModelBinder : IModelBinder
    {
        private readonly IList<IInputFormatter> _formatters;
        private readonly IHttpRequestStreamReaderFactory _readerFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly MvcOptions _options;
        private readonly ModelMetadata _commandWrapperMetadata;
        private readonly ILogger<CommandModelBinder> _logger;

        public CommandModelBinder(
            IList<IInputFormatter> formatters,
            IHttpRequestStreamReaderFactory readerFactory,
            MvcOptions options,
            ModelMetadata commandWrapperMetadata,
            ILoggerFactory loggerFactory
        )
        {
            _formatters = formatters;
            _readerFactory = readerFactory;
            _loggerFactory = loggerFactory;
            _options = options;
            _commandWrapperMetadata = commandWrapperMetadata;

            _logger = loggerFactory.CreateLogger<CommandModelBinder>();
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var binder = new BodyModelBinder(_formatters, _readerFactory, _loggerFactory, _options);
            bindingContext.ModelMetadata = _commandWrapperMetadata;

            await binder.BindModelAsync(bindingContext);

            var payload = (CommandWrapper) bindingContext.Result.Model;
            try
            {
                var commandType = typeof(DomainCommand)
                    .Assembly
                    .GetType(payload.CommandType);

                var command = commandType.HydrateFrom(payload.CommandData);

                bindingContext.Result = ModelBindingResult.Success(command);
                _logger.LogDebug(command.ToString());
            }
            catch (Exception ex)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                bindingContext.ModelState.AddModelError(
                    "Command",
                    $"An exception occurred while reading the command: {ex}");
            }
        }
    }
}