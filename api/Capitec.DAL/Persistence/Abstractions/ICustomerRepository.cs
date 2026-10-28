using Capitec.DAL.Data.Entities;

namespace Capitec.DAL.Persistence.Abstractions
{
    public interface ICustomerRepository
    {
        Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);
        Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Customer?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
        void Add(Customer customer);
    }
}
