using System.ComponentModel;
using System.Linq.Expressions;

namespace HeadStart.SharedKernel.Models.Extensions;

public static class DescriptionAttributeExtensions
{
    public static string GetDescription(this Enum e)
    {
        var name = e.ToString();
        var memberInfo = e.GetType().GetMember(name)[0];
        var descriptionAttributes = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return descriptionAttributes.Any() ? ((DescriptionAttribute)descriptionAttributes.First()).Description : name;
    }

    public static string GetMemberDescription<T, TProperty>(this T t, Expression<Func<T, TProperty>> property)
        where T : class
    {
        Activator.CreateInstance<T>();

        var memberName = ((MemberExpression)property.Body).Member.Name;
        var memberInfo = typeof(T).GetMember(memberName).FirstOrDefault();
        if (memberInfo == null)
        {
            return memberName;
        }

        var descriptionAttributes = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return descriptionAttributes.Any() ? ((DescriptionAttribute)descriptionAttributes.First()).Description : memberName;
    }

    public static string GetClassDescription<T>(this T t) where T : class
    {
        var descriptionAttributes = t.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false);
        return descriptionAttributes.Any() ? (descriptionAttributes.First() as DescriptionAttribute)!.Description : nameof(t);
    }
}
