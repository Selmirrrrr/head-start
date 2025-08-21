using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HeadStart.SharedKernel.Extensions;

public static class EnumExtensions
{
    private static readonly Lazy<Dictionary<Type, MemberInfo[]>> Cache = new();

    /// <summary>
    /// Retrieves all display names for a given enumeration."/>
    /// </summary>
    /// <param name="enum">The enumeration.</param>
    /// <returns>All display name values from <see cref="DisplayAttribute"/> or the default identifier names.</returns>
    public static string[] GetEnumDisplayNames(this Enum @enum)
    {
        var enumType = @enum.GetType();
        return CacheMembers(enumType).Select(m => m.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? m.Name).ToArray();
    }

    /// <summary>
    /// Retrieves the display name of a given <see cref="Enum"/> member.
    /// </summary>
    /// <param name="enum">The enum member.</param>
    /// <returns>The display name value from a <see cref="DisplayAttribute"/> or the default identifier name.</returns>
    public static string GetEnumDisplayName(this Enum @enum)
    {
        var enumType = @enum.GetType();
        return (CacheMembers(enumType)
                .SingleOrDefault(x => x.Name == @enum.ToString()) ?? throw new InvalidOperationException())
            .GetCustomAttribute<DisplayAttribute>()
            ?.GetName() ?? @enum.ToString();
    }

    private static MemberInfo[] CacheMembers(Type @type)
    {
        if (Cache.Value.TryGetValue(@type, out var members))
        {
            return members;
        }

        members = @type.GetMembers();
        Cache.Value.Add(@type, members);

        return members;
    }
}
