using Capitec.DAL.Data;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Capitec.DAL.Persistence.Repository
{
    internal sealed class CustomerRepository(CapitecDbContext database) : ICustomerRepository
    {
        public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken) =>
            await database.Customers.AsNoTracking().OrderBy(customer => customer.Name).ToListAsync(cancellationToken);

        public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            database.Customers.AsNoTracking().SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

        public Task<Customer?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
            database.Customers.AsNoTracking().SingleOrDefaultAsync(customer => customer.NormalizedEmail == normalizedEmail, cancellationToken);

        public Task<int> CountAsync(CancellationToken cancellationToken) => database.Customers.CountAsync(cancellationToken);

        public void Add(Customer customer) => database.Customers.Add(customer);
    }
}
