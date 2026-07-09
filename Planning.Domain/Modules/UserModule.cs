namespace Planning.Domain.Modules;

public class UserModule
{
    public Guid UserId { get; private set; }
    public AppModule Module { get; private set; }
    public bool IsEnabled { get; private set; }

    private UserModule()
    {
    }

    private UserModule(Guid userId, AppModule module, bool isEnabled)
    {
        UserId = userId;
        Module = module;
        IsEnabled = isEnabled;
    }

    public static UserModule Create(Guid userId, AppModule module, bool isEnabled = true)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return new UserModule(userId, module, isEnabled);
    }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
