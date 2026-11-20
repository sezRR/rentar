using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rentar.Shared.Infrastructure.Messaging;

/// <summary>
/// Wakes the background delivery when an event was committed. Notifications that arrive while one is pending collapse
/// into it, since one pass delivers everything that is due.
/// </summary>
public sealed class OutboxSignal
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite
    });

    public void Notify()
    {
        _channel.Writer.TryWrite(true);
    }

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _channel.Reader.ReadAsync(cancellationToken);
    }
}
