using Avalonia.Controls;
using Avalonia.Styling;

namespace TinCan.NET.Helpers;

public static class ResourceHelpers
{
    /// <summary>
    /// Similar to <see cref="IResourceNode.TryGetResource"/>, but handles the theme and casting
    /// automatically.
    /// </summary>
    /// <param name="node">The themed element in question.</param>
    /// <param name="key">A resource key.</param>
    /// <param name="value">Variable to write the resource to.</param>
    /// <typeparam name="T">Type to cast the result to. Needs not be explicitly speciifed.</typeparam>
    /// <returns></returns>
    public static bool TryGetResource<T>(this IThemeVariantHost node, object key, out T? value) where T: class
    {
        bool res = node.TryGetResource(key, node.ActualThemeVariant, out var valObject);
        value = (T?) valObject;
        return res;
    }
}