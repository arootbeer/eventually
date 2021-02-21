using System;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Interfaces.DomainCommands;

namespace Eventually.Infrastructure.Transport.CommandBus
{
    public interface IDomainCommandBus : IDisposable
    {
        Task ExecuteCommand(DomainCommand command, CancellationToken cancellationToken);

        Task<TResult> ExecuteCommand<TResult>(DomainCommand command, Func<DomainCommandResponse, CancellationToken, TResult> handler, CancellationToken cancellationToken);
    }
}