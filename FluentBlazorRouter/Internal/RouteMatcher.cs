﻿using System.Reflection;
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

    internal void Validate(Type pageType)
    {
        foreach (var segmentMatcherHandler in _segmentMatchers)
        {
            if (segmentMatcherHandler.Matcher is null)
            {
                continue;
            }

            var propertyInfo = pageType.GetProperty(segmentMatcherHandler.SegmentPropertyName);
            if (propertyInfo?.PropertyType != segmentMatcherHandler.Matcher.MatchType || propertyInfo.GetCustomAttribute<ParameterAttribute>() is null)
            {
                throw new Exception($"No property '{segmentMatcherHandler.SegmentPropertyName}' with a parameter attribute in page '{pageType.FullName}' of type '{segmentMatcherHandler.Matcher.MatchType.FullName}' has been found.");
            }
        }
    }
}