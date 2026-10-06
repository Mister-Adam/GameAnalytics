WITH ordered_events AS (
    SELECT
        event_time,
        event_type,
        LEAD(event_time) OVER (
            ORDER BY event_time
        ) AS next_event_time,
        LEAD(event_type) OVER (
            ORDER BY event_time
        ) AS next_event_type
    FROM events
    WHERE player_id = @playerId
    AND session_id = @sessionId
    AND run_id = @runId
),
run_segments AS (
    SELECT
        event_time AS start_time,
        next_event_time AS end_time
    FROM ordered_events
    WHERE event_type IN (
        'OnRunCreated',
        'OnRunResumed'
    )
    AND next_event_type IN (
        'OnRunPaused',
        'Victory',
        'Defeat',
        'Surrender'
    )
)
SELECT
    MIN(start_time) AS start_time,
    MAX(end_time) AS end_time,
    SUM(end_time - start_time) AS run_duration
FROM run_segments;