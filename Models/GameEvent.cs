using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameAnalytics.Models;

public record GameEvent(
    string PlayerId,
    string SessionId,
    string RunId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    GameEventType EventType,
    DateTime EventTime,
    JsonElement Payload
);