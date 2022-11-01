using System.Text.RegularExpressions;

namespace FluentBlazorRouter;

public static class StringExtensions
{
    public static string ApplyRouteValues(this string route, Dictionary<string, object> routeValues)
    {
        // replace identifiers in route with values from routeValues
        // e.g. /user/{id:int} with routeValues = { id = 1 } => /user/1
        var regex = new Regex(@"\{\s*(?<key>\w+)\s*(?:\:\s*(?<type>\w+)\s*)?\}");
        var result = regex.Replace(route, m =>
        {
            var key = m.Groups["key"].Value;
            return (routeValues.TryGetValue(key, out var value) ? value.ToString() : m.Value) 
                   ?? throw new InvalidOperationException($"Route value {m.Groups["key"].Name} not found");
        });
        return result;
    }
    
    public static string ApplyRouteValues(this string route, params object[] routeValues)
    {
        // replace identifiers in route with values from routeValues
        // e.g. /user/{id:int} with routeValues = { 1 } => /user/1
        var regex = new Regex(@"\{\s*(?<key>\w+)\s*(?:\:\s*(?<type>\w+)\s*)?\}");
        var result = regex.Replace(route, m =>
        {
            var key = m.Groups["key"].Value;
            var index = int.Parse(key);
            return (index < routeValues.Length ? routeValues[index].ToString() : m.Value) 
                   ?? throw new InvalidOperationException($"Route value {m.Groups["key"].Name} not found");
        });
        return result;
    }
}