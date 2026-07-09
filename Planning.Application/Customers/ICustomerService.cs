using Planning.Application.Common;

namespace Planning.Application.Customers;

public interface ICustomerService
{
    Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CustomerResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CustomerResponse>>> GetByOrganizationAsync(CancellationToken cancellationToken = default);
}
