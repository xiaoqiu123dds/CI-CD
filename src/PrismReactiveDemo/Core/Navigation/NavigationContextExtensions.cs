using Prism.Navigation;
using Prism.Navigation.Regions;

namespace PrismReactiveDemo.Core.Navigation;

/// <summary>
/// 导航上下文强类型读取扩展。
/// Strongly typed extraction extensions for navigation context.
/// </summary>
public static class NavigationContextExtensions
{
    public static T GetRequired<T>(this NavigationContext navigationContext, string key)
    {
        if (!navigationContext.Parameters.ContainsKey(key))
        {
            throw new InvalidOperationException($"Missing required navigation parameter: {key}");
        }

        var value = navigationContext.Parameters[key];
        if (value is T typedValue)
        {
            return typedValue;
        }

        throw new InvalidOperationException($"Navigation parameter '{key}' is not of type {typeof(T).Name}.");
    }

    public static T? GetValueOrDefault<T>(this NavigationContext navigationContext, string key)
    {
        if (!navigationContext.Parameters.ContainsKey(key))
        {
            return default;
        }

        var value = navigationContext.Parameters[key];
        return value is T typedValue ? typedValue : default;
    }

    public static NavigationParameters CreateParameter<T>(string key, T value)
        where T : notnull
    {
        return new NavigationParameters
        {
            { key, value }
        };
    }
}
