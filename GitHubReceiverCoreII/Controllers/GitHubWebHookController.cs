using System;
using Microsoft.AspNetCore.Http;
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

        [AzureAlertWebHook]
        public IActionResult AzureAlert(string receiverName, string id, AzureAlertNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(0, "Receiver {ReceiverName} '{ReceiverId}' received something.", receiverName, id);
            _logger.LogInformation(
                1,
                "Alert {AlertName} / {AlertId} reached status {AlertStatus} at {AlertTime}.",
                data.Context.Name,
                data.Context.Id,
                data.Status,
                data.Context.Timestamp);

            return Ok();
        }

        [BitbucketWebHook]
        public IActionResult Bitbucket(string receiverName, string id, string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [DropboxWebHook]
        public IActionResult Dropbox(string receiverName, string id, string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [DynamicsCRMWebHook]
        public IActionResult DynamicsCRM(string receiverName, string id, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var messageName = data.Value<string>(DynamicsCRMConstants.EventRequestPropertyName);
            _logger.LogInformation(
                2,
                "Receiver {ReceiverName} / {ReceiverId} received message {MessageName} with {PropertyCount} properties.",
                receiverName,
                id,
                messageName,
                data.Count);

            return Ok();
        }

        [GitHubWebHook]
        public IActionResult GitHub(string receiver, string receiverId, string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(3, "Receiver {ReceiverName} '{ReceiverId}' received something.", receiver, receiverId);

            if (@event.Equals("push", StringComparison.OrdinalIgnoreCase))
            {
                var branch = data["ref"].ToString();
                _logger.LogInformation(4, "Received notification of push to '{Branch}'.", branch);

                foreach (var commit in data["commits"])
                {
                    var id = commit["id"].ToString();
                    var message = commit["message"].ToString();
                    _logger.LogInformation(5, "\t{Id}: {Message}.", id, message);

                    foreach (var added in commit["added"])
                    {
                        var name = added.ToString();
                        _logger.LogInformation(6, "Added '{FileName}'.", name);
                    }

                    foreach (var modified in commit["modified"])
                    {
                        var name = modified.ToString();
                        _logger.LogInformation(7, "Modified '{FileName}'.", name);
                    }

                    foreach (var removed in commit["removed"])
                    {
                        var name = removed.ToString();
                        _logger.LogInformation(8, "Removed '{FileName}'.", name);
                    }
                }
            }

            return Ok();
        }

        [KuduWebHook]
        public IActionResult Kudu(string receiverName, string id, KuduNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(9, "Receiver {ReceiverName} '{ReceiverId}' received something.", receiverName, id);
            _logger.LogInformation(
                10,
                "Kudu deployment {KuduId} for site {SiteName} reached status {Status} ({StatusText}).",
                data.Id,
                data.SiteName,
                data.Status,
                data.StatusText);

            return Ok();
        }

        [MailChimpWebHook]
        public IActionResult MailChimp(string receiverName, string id, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the event name.
            var eventNames = data[MailChimpConstants.EventRequestPropertyName];
            _logger.LogInformation(
                11,
                "Receiver {ReceiverName} / {ReceiverId} received message {EventNames} with {PropertyCount} properties.",
                receiverName,
                id,
                eventNames.ToString(),
                data.Count);

            return Ok();
        }
    }
}
