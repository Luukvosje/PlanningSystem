namespace Planning.Domain.Modules;

public class OrganizationModule
{
    public Guid OrganizationId { get; private set; }
    public AppModule Module { get; private set; }
    public bool IsEnabled { get; private set; }

    private OrganizationModule()
    {
    }

    private OrganizationModule(Guid organizationId, AppModule module, bool isEnabled)
    {
        OrganizationId = organizationId;
        Module = module;
        IsEnabled = isEnabled;
    }

    public static OrganizationModule Create(Guid organizationId, AppModule module, bool isEnabled = true)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id is required.", nameof(organizationId));
        }

        return new OrganizationModule(organizationId, module, isEnabled);
    }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
