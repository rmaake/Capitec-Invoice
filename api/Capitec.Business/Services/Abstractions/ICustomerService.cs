using Capitec.Business.Contracts;

namespace Capitec.Business.Services.Abstractions
{
    public interface ICustomerService
    {
        Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<CustomerDto> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken);
    }
}
