using System.Threading.Channels;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public class WorkflowQueue : IWorkflowQueue
{
    private readonly Channel<Guid> _channel;

    public WorkflowQueue()
    {
        _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    }

    public async Task EnqueueAsync(Guid outcomeId, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(outcomeId, cancellationToken);
    }

    public ChannelReader<Guid> Reader => _channel.Reader;

}