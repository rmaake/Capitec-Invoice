namespace Capitec.DAL.Persistence.Abstractions
{
    public interface IUnitOfWorkFactory
    {
        Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken);
    }
}
