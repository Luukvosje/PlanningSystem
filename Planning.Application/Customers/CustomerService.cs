using Planning.Application.Common;
using Planning.Domain.Customers;
using Planning.Domain.Organizations;

namespace Planning.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public CustomerService(
        ICustomerRepository customerRepository,
        ICurrentUserContext currentUserContext)
    {
        _customerRepository = customerRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<CustomerResponse>> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<CustomerResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        try
        {
            var customer = Customer.Create(
                _currentUserContext.OrganizationId!.Value,
                request.Name,
                request.Email,
                request.Address,
                DateTime.UtcNow);

            await _customerRepository.AddAsync(customer, cancellationToken);
            return Result<CustomerResponse>.Success(CustomerMapper.ToResponse(customer));
        }
        catch (ArgumentException ex)
        {
            return Result<CustomerResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null || !BelongsToCurrentOrganization(customer))
        {
            return Result<CustomerResponse>.Failure("Customer not found.", "NOT_FOUND");
        }

        try
        {
            customer.Update(request.Name, request.Email, request.Address, DateTime.UtcNow);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
            return Result<CustomerResponse>.Success(CustomerMapper.ToResponse(customer));
        }
        catch (ArgumentException ex)
        {
            return Result<CustomerResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null || !BelongsToCurrentOrganization(customer))
        {
            return Result.Failure("Customer not found.", "NOT_FOUND");
        }

        await _customerRepository.DeleteAsync(customer, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null || !BelongsToCurrentOrganization(customer))
        {
            return Result<CustomerResponse>.Failure("Customer not found.", "NOT_FOUND");
        }

        return Result<CustomerResponse>.Success(CustomerMapper.ToResponse(customer));
    }

    public async Task<Result<IReadOnlyList<CustomerResponse>>> GetByOrganizationAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<IReadOnlyList<CustomerResponse>>.Failure(
                "Organization context is required.",
                "NO_ORGANIZATION");
        }

        var customers = await _customerRepository.GetByOrganizationIdAsync(
            _currentUserContext.OrganizationId!.Value,
            cancellationToken);

        var response = customers.Select(CustomerMapper.ToResponse).ToList();
        return Result<IReadOnlyList<CustomerResponse>>.Success(response);
    }

    private bool BelongsToCurrentOrganization(Customer customer) =>
        _currentUserContext.HasOrganization &&
        customer.OrganizationId == _currentUserContext.OrganizationId;
}
