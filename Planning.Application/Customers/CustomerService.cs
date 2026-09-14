using Planning.Application.Common;
using Planning.Domain.Customers;

namespace Planning.Application.Customers;

public class CustomerService : TenantServiceBase, ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(
        ICustomerRepository customerRepository,
        ICurrentUserContext currentUserContext)
        : base(currentUserContext) =>
        _customerRepository = customerRepository;

    public async Task<Result<CustomerResponse>> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<CustomerResponse>();
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            var customer = Customer.Create(
                organizationId,
                request.Name,
                request.Email,
                request.Address,
                request.Color,
                DateTime.UtcNow);

            await _customerRepository.AddAsync(customer, cancellationToken);
            return Result<CustomerResponse>.Success(CustomerMapper.ToResponse(customer));
        });
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null || !Owns(customer))
        {
            return Failures.NotFoundFor<CustomerResponse>("Customer");
        }

        return await TranslateDomainErrorsAsync(async () =>
        {
            customer.Update(request.Name, request.Email, request.Address, request.Color, DateTime.UtcNow);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
            return Result<CustomerResponse>.Success(CustomerMapper.ToResponse(customer));
        });
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null || !Owns(customer))
        {
            return Failures.NotFoundFor("Customer");
        }

        await _customerRepository.DeleteAsync(customer, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null || !Owns(customer))
        {
            return Failures.NotFoundFor<CustomerResponse>("Customer");
        }

        return Result<CustomerResponse>.Success(CustomerMapper.ToResponse(customer));
    }

    public async Task<Result<IReadOnlyList<CustomerResponse>>> GetByOrganizationAsync(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetOrganizationId(out var organizationId))
        {
            return Failures.NoOrganizationContext<IReadOnlyList<CustomerResponse>>();
        }

        var customers = await _customerRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
        var response = customers.Select(CustomerMapper.ToResponse).ToList();

        return Result<IReadOnlyList<CustomerResponse>>.Success(response);
    }
}
