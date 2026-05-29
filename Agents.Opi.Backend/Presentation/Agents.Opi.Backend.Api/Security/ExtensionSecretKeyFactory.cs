using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Agents.Opi.Backend.Api.Security;

public static class ExtensionSecretKeyFactory
{
    public static SymmetricSecurityKey Create(string secret)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret.Trim()));
    }
}
