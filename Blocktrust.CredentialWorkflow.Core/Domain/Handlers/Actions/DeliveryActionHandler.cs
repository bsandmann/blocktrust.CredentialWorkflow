using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Handlers.Actions;

public class DeliveryActionHandler : IActionHandler
{
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger<DeliveryActionHandler> _logger;

    public DeliveryActionHandler(
        IDeliveryService deliveryService,
        ILogger<DeliveryActionHandler> logger)
    {
        _deliveryService = deliveryService;
        _logger = logger;
    }

    public async Task<Result<ActionResult>> ExecuteAsync(
        ActionInput input,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var typedInput = (ActionInputDelivery)input;
            var previousAction = context.GetActionResult("issuance");
            
            if (previousAction?.OutputJson == null)
            {
                return Result.Fail<ActionResult>("No credential available for delivery");
            }

            var credential = JsonSerializer.Deserialize<dynamic>(previousAction.OutputJson)?.credential?.ToString();

            var deliveryResult = typedInput.DeliveryType switch
            {
                EDeliveryType.Email => await _deliveryService.DeliverViaEmail(typedInput.Destination, credential),
                EDeliveryType.DIDComm => await _deliveryService.DeliverViaDIDComm(typedInput.Destination, credential),
                _ => Result.Fail("Unsupported delivery type")
            };

            return deliveryResult.IsSuccess 
                ? Result.Ok(new ActionResult { Success = true })
                : Result.Fail<ActionResult>("Delivery failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute delivery");
            return Result.Fail<ActionResult>("Delivery failed: " + ex.Message);
        }
    }
}