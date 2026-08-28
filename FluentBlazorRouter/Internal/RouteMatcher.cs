using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace FluentBlazorRouter.Internal;

internal sealed class RouteMatcher
{
    private readonly List<SegmentMatcherHandler> _segmentMatchers;

    public RouteMatcher(List<SegmentMatcherHandler> segmentMatchers)
    {
        _segmentMatchers = segmentMatchers;
    }

    internal bool Matches(string relativeUri, Dictionary<string, object> routeValues)
    {
        Dictionary<string, object> tempRouteValues = new();
        var segments = relativeUri.Split("/");

        if (segments.Length != _segmentMatchers.Count)
        {
            return false;
        }

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            
            var segmentMatcherHandler = _segmentMatchers[i];
            var segmentMatcher = segmentMatcherHandler.Matcher;
            
            if (segmentMatcher is null)
            {
                // hacky but this is a special case => no matcher => the segment has to match
                if (segment != segmentMatcherHandler.SegmentPropertyName)
                {
                    return false;
                }
            }
            else
            {
                if (!segmentMatcher.MatchSegment(segment, out var segmentValue))
                {
                    return false;
                }

                tempRouteValues[segmentMatcherHandler.SegmentPropertyName] = segmentValue;
            }
        }
        
        foreach (var key in tempRouteValues.Keys)
        {
            routeValues[key] = tempRouteValues[key];
        }
        
        return true;
    }

    private static bool AcceptsMatchType(Type propertyType, Type matchType)
        => propertyType == matchType || Nullable.GetUnderlyingType(propertyType) == matchType;

    private static List<PropertyInfo> FindPublicInstanceProperties(Type pageType, string propertyName)
    {
        var properties = new List<PropertyInfo>();
        var seenDeclarations = new HashSet<MethodInfo>();

        for (var type = pageType; type is not null; type = type.BaseType)
        {
            var declared = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(candidate => candidate.Name == propertyName && candidate.GetIndexParameters().Length == 0);

            foreach (var candidate in declared)
            {
                var accessor = candidate.GetMethod ?? candidate.SetMethod;
                var declaration = accessor?.GetBaseDefinition();

                if (declaration is not null && !seenDeclarations.Add(declaration))
                {
                    continue;
                }

                properties.Add(candidate);
            }
        }

        return properties;
    }

    private static string Describe(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        return underlying is null ? type.FullName ?? type.Name : (underlying.FullName ?? underlying.Name) + "?";
    }

    internal void Validate(Type pageType)
    {
        foreach (var segmentMatcherHandler in _segmentMatchers)
        {
            if (segmentMatcherHandler.Matcher is null)
            {
                continue;
            }

            var propertyName = segmentMatcherHandler.SegmentPropertyName;
            var matchType = segmentMatcherHandler.Matcher.MatchType;

            if (matchType is null)
            {
                throw new Exception($"Segment matcher for '{propertyName}' on page '{pageType.FullName}' has no MatchType.");
            }

            var candidates = FindPublicInstanceProperties(pageType, propertyName);
            var parameters = candidates
                .Where(candidate => candidate.GetCustomAttribute<ParameterAttribute>() is not null)
                .ToList();

            if (parameters.Count > 1)
            {
                throw new Exception($"Property '{propertyName}' is declared as a parameter on both '{parameters[0].DeclaringType?.FullName}' and '{parameters[1].DeclaringType?.FullName}'. Blazor requires parameter names to be unique.");
            }

            var propertyInfo = parameters.FirstOrDefault() ?? candidates.FirstOrDefault();
            if (propertyInfo is null)
            {
                var declaredAnywhere = pageType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .Any(property => property.Name == propertyName && property.GetIndexParameters().Length == 0);
                throw new Exception(declaredAnywhere
                    ? $"Property '{propertyName}' on page '{pageType.FullName}' is not a public instance property."
                    : $"No property '{propertyName}' found on page '{pageType.FullName}'.");
            }

            if (!AcceptsMatchType(propertyInfo.PropertyType, matchType))
            {
                throw new Exception($"Property '{propertyName}' on page '{pageType.FullName}' is of type '{Describe(propertyInfo.PropertyType)}' but the route segment requires '{Describe(matchType)}'.");
            }

            if (propertyInfo.GetCustomAttribute<ParameterAttribute>() is null)
            {
                throw new Exception($"Property '{propertyName}' on page '{pageType.FullName}' is missing [Parameter].");
            }
        }
    }
}