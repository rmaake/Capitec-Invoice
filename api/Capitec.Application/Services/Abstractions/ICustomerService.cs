using Capitec.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Services.Abstractions
{
    public interface ICustomerService
    {
        Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<CustomerDto> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken);
    }
}
