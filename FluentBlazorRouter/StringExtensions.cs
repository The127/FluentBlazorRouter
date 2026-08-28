using System.Text.RegularExpressions;
using FluentBlazorRouter.Internal;

namespace FluentBlazorRouter;

public static class StringExtensions
{
    public static string ApplyRouteValues(this string route, Dictionary<string, object> routeValues)
    {
        if (routeValues is null)
        {
            throw new ArgumentNullException(nameof(routeValues));
        }

        // replace identifiers in route with values from routeValues
        // e.g. /user/{id:int} with routeValues = { id = 1 } => /user/1
        var regex = new Regex($@"\{{\s*(?<key>{SegmentSyntax.Identifier})\s*(?:\:\s*(?<type>{SegmentSyntax.Identifier})\s*)?\}}");
        var result = regex.Replace(route, m =>
        {
            var key = m.Groups["key"].Value;

            if (!routeValues.TryGetValue(key, out var value))
            {
                return m.Value;
            }

            if (value is null)
            {
                throw new InvalidOperationException($"Route value '{key}' is null.");
            }

            return value.ToString() ?? throw new InvalidOperationException($"Route value '{key}' has a null string representation.");
        });
        return result;
    }
    
    public static string ApplyRouteValues(this string route, params object[] routeValues)
    {
        if (routeValues is null)
        {
            throw new ArgumentNullException(nameof(routeValues));
        }

        // replace identifiers in route with values from routeValues
        // e.g. /user/{id:int} with routeValues = { 1 } => /user/1
        var regex = new Regex(@"\{\s*(?<key>\w+)\s*(?:\:\s*(?<type>\w+)\s*)?\}");
        var result = regex.Replace(route, m =>
        {
            var index = int.Parse(m.Groups["key"].Value);

            if (index >= routeValues.Length)
            {
                return m.Value;
            }

            var value = routeValues[index];

            if (value is null)
            {
                throw new InvalidOperationException($"Route value at index {index} is null.");
            }

            return value.ToString() ?? throw new InvalidOperationException($"Route value at index {index} has a null string representation.");
        });
        return result;
    }
}