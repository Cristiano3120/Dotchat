using System.Reflection;

namespace DotchatClient.src.Core.Enums;

public static class GoogleScopes
{
    public const string Email = "openid email profile";

    public static IEnumerable<string> GetAllScopes()
    {
        Type gs = typeof(GoogleScopes);
        foreach (FieldInfo scope in gs.GetFields())
        {
            string? val = scope.GetValue(null)?.ToString();
            if (val is null)
            {
                continue;
            }

            yield return val;
        }
    }
}