using Capitec.Business.Services.Abstractions;
using Capitec.Business.Contracts;
using Capitec.Business.Extensions;
using Capitec.DAL.CustomExceptions;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Persistence.Abstractions;

namespace Capitec.Business.Services;

public sealed class CustomerService(IUnitOfWorkFactory unitOfWorkFactory, TimeProvider clock) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var customers = await unitOfWork.Customers.GetAllAsync(cancellationToken);
        return customers.Select(customer => customer.ToDto()).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var customer = await unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        return customer?.ToDto();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();
        var email = command.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken))
        {
            var existing = await unitOfWork.Customers.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (existing is not null)
                return ExistingOrConflict(existing, name);

            var customer = new Customer
            {
                Name = name,
                Email = email,
                NormalizedEmail = normalizedEmail,
                Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
                CreatedAtUtc = clock.GetUtcNow().UtcDateTime
            };
            unitOfWork.Customers.Add(customer);
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return customer.ToDto();
            }
            catch (PersistenceConflictException)
            {
                // A simultaneous request may have inserted the same normalized email.
            }
        }

        await using var retryUnitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var duplicate = await retryUnitOfWork.Customers.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (duplicate is null)
            throw new PersistenceException("The customer could not be saved.", new InvalidOperationException());
        return ExistingOrConflict(duplicate, name);
    }

    private static CustomerDto ExistingOrConflict(Customer existing, string requestedName)
    {
        if (!string.Equals(existing.Name, requestedName, StringComparison.OrdinalIgnoreCase))
            throw new CustomerEmailConflictException("A customer with this email already exists.");
        return existing.ToDto();
    }
}