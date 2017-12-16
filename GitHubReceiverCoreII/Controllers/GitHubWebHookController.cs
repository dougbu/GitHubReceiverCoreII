using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace GitHubReceiverCoreII.Controllers
{
    public class GitHubWebHookController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISalesforceResultCreator _resultCreator;

        public GitHubWebHookController(ILoggerFactory loggerFactory, ISalesforceResultCreator resultCreator)
        {
            _logger = loggerFactory.CreateLogger<GitHubWebHookController>();
            _resultCreator = resultCreator;
        }

        [AzureAlertWebHook]
        public IActionResult AzureAlert(string receiverName, string id, string @event, AzureAlertNotification data)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    if (state.Value.ValidationState == ModelValidationState.Invalid)
                    {
                        _logger.LogWarning(
                            0,
                            "ModelState not valid. Key '{Key}' has {Count} errors.",
                            state.Key,
                            state.Value.Errors.Count);
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning(
                                1,
                                "Error: {ErrorMessage}",
                                error.ErrorMessage);
                        }
                    }
                }

                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                2,
                "Receiver {ReceiverName} '{ReceiverId}' received '{Event}'.",
                receiverName,
                id,
                @event);
            _logger.LogInformation(
                3,
                "Alert {AlertName} / {AlertId} reached status {AlertStatus} of {MetricName} at {AlertTime}.",
                data.Context.Name,
                data.Context.Id,
                data.Status,
                data.Context.Condition.MetricName,
                data.Context.Timestamp);

            return Ok();
        }

        [BitbucketWebHook]
        public IActionResult Bitbucket(string receiverName, string id, string webHookId, string @event, JObject data)
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
        public IActionResult DynamicsCRM(string receiverName, string id, string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                4,
                "Receiver {ReceiverName} / {ReceiverId} received '{Event}' with {PropertyCount} properties.",
                receiverName,
                id,
                @event,
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

            _logger.LogInformation(
                5,
                "Receiver {ReceiverName} '{ReceiverId}' received '{Event}'.",
                receiver,
                receiverId,
                @event);

            if (@event.Equals("push", StringComparison.OrdinalIgnoreCase))
            {
                var branch = data["ref"].ToString();
                _logger.LogInformation(6, "Received notification of push to '{Branch}'.", branch);

                foreach (var commit in data["commits"])
                {
                    var id = commit["id"].ToString();
                    var message = commit["message"].ToString();
                    _logger.LogInformation(7, "\t{Id}: {Message}.", id, message);

                    foreach (var added in commit["added"])
                    {
                        var name = added.ToString();
                        _logger.LogInformation(8, "Added '{FileName}'.", name);
                    }

                    foreach (var modified in commit["modified"])
                    {
                        var name = modified.ToString();
                        _logger.LogInformation(9, "Modified '{FileName}'.", name);
                    }

                    foreach (var removed in commit["removed"])
                    {
                        var name = removed.ToString();
                        _logger.LogInformation(10, "Removed '{FileName}'.", name);
                    }
                }
            }

            return Ok();
        }

        [KuduWebHook]
        public IActionResult Kudu(string receiverName, string id, string @event, KuduNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                11,
                "Receiver {ReceiverName} '{ReceiverId}' received '{Event}'.",
                receiverName,
                id,
                @event);
            _logger.LogInformation(
                12,
                "Kudu deployment {KuduId} for site {SiteName} reached status {Status} ({StatusText}).",
                data.Id,
                data.SiteName,
                data.Status,
                data.StatusText);

            return Ok();
        }

        [MailChimpWebHook]
        public IActionResult MailChimp(string receiverName, string id, string @event, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the event name.
            _logger.LogInformation(
                13,
                "Receiver {ReceiverName} / {ReceiverId} received '{Event}' with {PropertyCount} properties.",
                receiverName,
                id,
                @event,
                data.Count);

            return Ok();
        }

        [PusherWebHook]
        public IActionResult Pusher(string receiverName, string id, string[] events, PusherNotifications data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the notification creation timestamp.
            var createdAtUnix = data.CreatedAt;
            var createdAt = DateTimeOffset.FromUnixTimeMilliseconds(createdAtUnix);
            _logger.LogInformation(
                14,
                "{ControllerName} received {Count} notifications and {EventCount} events created at '{CreatedAt}'.",
                nameof(GitHubWebHookController),
                data.Events.Count,
                events.Length,
                createdAt.ToString("o"));
            for (var i = 0; i < events.Length; i++)
            {
                _logger.LogInformation(15, "Event {Index}: '{Event}'.", i, events[i]);
            }

            // Get details of the individual notifications.
            var index = 0;
            foreach (var @event in data.Events)
            {
                if (@event.TryGetValue(PusherConstants.EventNamePropertyName, out var eventName))
                {
                    if (@event.TryGetValue(PusherConstants.ChannelNamePropertyName, out var channelName))
                    {
                        _logger.LogInformation(
                            16,
                            "Event {EventNumber} has {Count} properties, including name '{EventName}' and channel " +
                            "'{ChannelName}'.",
                            index,
                            @event.Count,
                            eventName,
                            channelName);
                    }
                    else
                    {
                        _logger.LogInformation(
                            17,
                            "Event {EventNumber} has {Count} properties, including name '{EventName}'.",
                            index,
                            @event.Count,
                            eventName);
                    }
                }
                else
                {
                    _logger.LogError(
                        18,
                        "Event {EventNumber} has {Count} properties but does not contain a {PropertyName} property.",
                        index,
                        @event.Count,
                        PusherConstants.EventNamePropertyName);
                }

                index++;
            }

            return Ok();
        }

        [SalesforceWebHook]
        public async Task<IActionResult> Salesforce(string id, string @event, SalesforceNotifications data)
        {
            if (!ModelState.IsValid)
            {
                return await _resultCreator.GetFailedResultAsync("Model binding failed.");
            }

            _logger.LogInformation(
                19,
                "{ControllerName} / '{ReceiverId}' received {Count} notifications with ActionId '{ActionId}' (event " +
                "'{Event}').",
                nameof(GitHubWebHookController),
                id,
                data.Notifications.Count(),
                data.ActionId,
                @event);
            _logger.LogInformation(
                10,
                "Data contains OrganizationId '{OrganizationId}' and SessionId '{SessionId}'.",
                data.OrganizationId,
                data.SessionId);
            _logger.LogInformation(
                21,
                "Contained URLs include EnterpriseUrl '{EnterpriseUrl}' and PartnerUrl '{PartnerUrl}'.",
                data.EnterpriseUrl,
                data.PartnerUrl);

            var index = 0;
            foreach (var notification in data.Notifications)
            {
                _logger.LogInformation(
                    22,
                    "Notification #{Number} contained {Count} values.",
                    index,
                    notification.Count);
                index++;
            }

            return await _resultCreator.GetSuccessResultAsync();
        }

        [SlackWebHook]
        public IActionResult Slack(string id, string @event, string subtext, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                23,
                "{ControllerName} / '{ReceiverId}' received {Count} properties with event '{Event}').",
                nameof(GitHubWebHookController),
                id,
                data.Count,
                @event);

            var channel = data[SlackConstants.ChannelRequestFieldName];
            var command = data[SlackConstants.CommandRequestFieldName];
            var trigger = data[SlackConstants.TriggerRequestFieldName];
            _logger.LogInformation(
                24,
                "Data contains channel '{ChannelName}', command '{Command}', and trigger '{Trigger}'.",
                channel,
                command,
                trigger);

            var text = data[SlackConstants.TextRequestFieldName];
            _logger.LogInformation(
                25,
                "Data contains text '{Text}' and subtext '{Subtext}'.",
                text,
                subtext);

            if (!StringValues.IsNullOrEmpty(command) && text.ToString().Contains("="))
            {
                var parsedCommand = SlackCommand.ParseActionWithValue(text);
                var parsedParameters = SlackCommand.TryParseParameters(parsedCommand.Value, out var errorMessage);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    _logger.LogWarning("Error while parsing command parameters: {ErrorMessage}", errorMessage);
                    return new BadRequestObjectResult("Error while parsing command parameters");
                }

                var reply = string.Format(
                    "Received command '{0}' with action '{1}' and parameters ({2}).",
                    command,
                    parsedCommand.Key,
                    SlackCommand.GetNormalizedParameterString(parsedParameters));

                // Slash responses can be augmented with attachments containing data, images, and more.
                var attachment = new SlackAttachment("Attachment Text", "Fallback description")
                {
                    Color = "#439FE0",
                    Pretext = "Hello from ASP.NET WebHooks!",
                    Title = "Attachment title",
                };

                // Slash attachments can contain tabular data as well
                attachment.Fields.Add(new SlackField("Field1", "1234"));
                attachment.Fields.Add(new SlackField("Field2", "5678"));

                return new JsonResult(new SlackSlashResponse(reply, attachment));
            }

            if (!StringValues.IsNullOrEmpty(command) && text.ToString().Contains(" "))
            {
                var parsedCommand = SlackCommand.ParseActionWithValue(text);
                var reply = string.Format(
                    "Received command '{0}' with action '{1}' and value '{2}'",
                    command,
                    parsedCommand.Key,
                    parsedCommand.Value);

                return new JsonResult(new SlackResponse(reply));
            }

            if (!StringValues.IsNullOrEmpty(trigger) && subtext.ToString().Contains(" "))
            {
                var triggerCommand = SlackCommand.ParseActionWithValue(subtext);
                var reply = string.Format(
                    "Received trigger '{0}' with action '{1}' and value '{2}'",
                    trigger,
                    triggerCommand.Key,
                    triggerCommand.Value);

                return new JsonResult(new SlackResponse(reply));
            }

            return Ok();
        }

        [StripeWebHook]
        public IActionResult Stripe(string id, string @event, string notificationId, StripeEvent data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                26,
                "{ControllerName} / '{ReceiverId}' received a '{EventType}' notification (event '{EventName}').",
                nameof(GitHubWebHookController),
                id,
                data.EventType,
                @event);

            _logger.LogInformation(
                27,
                "Data created at '{Created}' and contains Notification ID '{Id}' / '{NotificationId}', Live mode " +
                "'{DetailsLiveMode}', and Request ID '{RequestId}'.",
                data.Created,
                data.Id,
                notificationId,
                data.LiveMode,
                data.Request);

            var details = data.Data.Object;
            var created = DateTimeOffset.FromUnixTimeMilliseconds(
                details.Value<long>(StripeConstants.CreatedPropertyName));
            _logger.LogInformation(
                28,
                "Event detail created at '{DetailsCreated}' and contains {PropertyCount} properties, including " +
                "Account '{Account}', Id '{DetailsId}', Live mode '{DetailsLiveMode}', and Name '{Name}'.",
                created,
                details.Count,
                details.Value<string>("account"),
                details.Value<string>("id"),
                details.Value<string>(StripeConstants.LiveModePropertyName),
                details.Value<string>("name"));

            return Ok();
        }
    }
}
