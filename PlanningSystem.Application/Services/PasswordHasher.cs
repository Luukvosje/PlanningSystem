using System.Security.Cryptography;

namespace PlanningSystem.Application.Services;

public class PasswordHasher : Interfaces.IPasswordHasher
{
    private static byte[] GetSalt()
    {
        // Same salt as original Base64Helper.From("YWFw")
        var str = "YWFw";
        var paddingLength = 4 - (str.Length % 4);
        var padding = (paddingLength < 4 && paddingLength > 0) ? new string('=', paddingLength) : "";
        return Convert.FromBase64String(str + padding);
    }

    public string HashPassword(string password)
    {
        var salt = GetSalt();
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var hash = HashPassword(password);
            return hash == hashedPassword;
        }
        catch
        {
            return false;
        }
    }
}
