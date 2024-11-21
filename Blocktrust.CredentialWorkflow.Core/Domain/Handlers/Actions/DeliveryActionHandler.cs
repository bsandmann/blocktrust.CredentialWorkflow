using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Handlers.Actions;

public class DeliveryActionHandler : IActionHandler
{
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger<DeliveryActionHandler> _logger;
    private readonly IConfiguration _configuration;

    public DeliveryActionHandler(
        IDeliveryService deliveryService,
        ILogger<DeliveryActionHandler> logger,
        IConfiguration configuration)
    {
        _deliveryService = deliveryService;
        _logger = logger;
        _configuration = configuration;
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
            var deliveryType = typedInput.DeliveryType.ResolveValue(context, _configuration);
            var destination = typedInput.Destination.ResolveValue(context, _configuration);

            if (string.IsNullOrEmpty(deliveryType) || string.IsNullOrEmpty(destination))
            {
                return Result.Fail<ActionResult>("Delivery parameters not available");
            }

            var deliveryResult = deliveryType.ToLower() switch
            {
                "email" => await _deliveryService.DeliverViaEmail(destination, credential),
                "didcomm" => await _deliveryService.DeliverViaDIDComm(destination, credential),
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