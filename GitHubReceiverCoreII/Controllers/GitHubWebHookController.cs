using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace GitHubReceiverCoreII.Controllers
{
    public class GitHubWebHookController : ControllerBase
    {
        private readonly ILogger _logger;

        public GitHubWebHookController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GitHubWebHookController>();
        }

        // ??? Should we provide an abstract base GitHub controller containing this method?
        // ??? [FromRoute] not required but makes parameters consistent and explicit.
        [GitHubWebHookAction]
        public IActionResult Handler(
            [FromRoute(Name = "webHookReceiver")] string receiver,
            [FromRoute(Name = "id")] string receiverId,
            [FromHeader(Name = "X-Github-Event")] string action,
            [FromBody] JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(0, "Receiver {ReceiverName} '{ReceiverId}' received something.", receiver, receiverId);

            if (action.Equals("push", StringComparison.OrdinalIgnoreCase))
            {
                var branch = data["ref"].ToString();
                _logger.LogInformation(1, $"Received notification of push to '{branch}'.");

                foreach (var commit in data["commits"])
                {
                    var id = commit["id"].ToString();
                    var message = commit["message"].ToString();
                    _logger.LogInformation(2, $"\t{id}: {message}");

                    foreach (var added in commit["added"])
                    {
                        var name = added.ToString();
                        _logger.LogInformation(3, $"Added '{name}'");
                    }

                    foreach (var modified in commit["modified"])
                    {
                        var name = modified.ToString();
                        _logger.LogInformation(4, $"Modified '{name}'");
                    }

                    foreach (var removed in commit["removed"])
                    {
                        var name = removed.ToString();
                        _logger.LogInformation(5, $"Removed '{name}'");
                    }
                }
            }

            return Ok();
        }
    }
}
