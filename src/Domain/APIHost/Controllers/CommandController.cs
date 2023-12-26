using Eventually.Domain.Runtime;
using Eventually.Interfaces.DomainCommands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.APIHost.Controllers
{
    [ApiController]
    [Route("/domain/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly IDomainCommandExecutor _commandExecutor;
        private readonly ILogger _logger;

        public CommandController(IDomainCommandExecutor commandExecutor, ILoggerFactory loggerFactory)
        {
            _commandExecutor = commandExecutor;
            _logger = loggerFactory.CreateLogger(nameof(CommandController));
        }

        [HttpPost]
        public IActionResult Execute([FromBody] DomainCommand command)
        {
            if (command is null)
            {
                return BadRequest();
            }

            var response = _commandExecutor.Execute(command);
            _logger.LogDebug("{response}", response);
            
            // Even if the command handling succeeds, return "Ok" so that the calling code
            // is assured that the command was handled correctly. The response object will
            // contain details about any failures that occurred.
            return Ok(response);

        }
    }
}