using System.Reflection;

namespace SessionPlanner.Infrastructure.Auth;

public static class PermissionHelper
{
    public static IEnumerable<string> GetAllPermissions(Type permissionsType)
    {
        return permissionsType
            .GetNestedTypes(BindingFlags.Public)
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue()?.ToString())
            .Where(v => !string.IsNullOrWhiteSpace(v))!
            .Distinct()!;
    }
}