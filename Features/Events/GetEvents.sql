SELECT
    id,
    player_id,
    event_name,
    payload::text AS payload_json,
    event_time
FROM events
WHERE (@playerId::text IS NULL OR player_id = @playerId)
ORDER BY event_time DESC, id DESC
LIMIT @limit OFFSET @offset;