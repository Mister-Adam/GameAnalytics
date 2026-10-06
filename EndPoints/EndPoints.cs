namespace GameAnalytics.Endpoints;

using GameAnalytics.Features.Analytics;
using GameAnalytics.Features.Events;

public static class EndpointExtensions
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/analytics").WithTags("Analytics");

        GetRunDuration.Map(group);
        GetWinRates.Map(group);
    }

    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/events").WithTags("Events");

        TrackEvent.Map(group);
    }
}