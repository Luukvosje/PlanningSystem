using Planning.Domain.Customers;

namespace Planning.Application.Customers;

internal static class CustomerMapper
{
    public static CustomerResponse ToResponse(Customer customer) =>
        new(
            customer.Id,
            customer.OrganizationId,
            customer.Name,
            customer.Email,
            customer.Address,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc);
}
