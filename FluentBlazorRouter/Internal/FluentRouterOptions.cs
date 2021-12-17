namespace FluentBlazorRouter.Internal;

internal record FluentRouterOptions(IReadOnlyDictionary<string, ISegmentMatcher> SegmentMatchers);