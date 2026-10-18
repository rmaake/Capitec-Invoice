using Capitec.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions
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
