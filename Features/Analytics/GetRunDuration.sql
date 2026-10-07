WITH ordered_events AS (
    SELECT
        event_time,
        event_name,
        LEAD(event_time) OVER (
            ORDER BY event_time
        ) AS next_event_time,
        LEAD(event_name) OVER (
            ORDER BY event_time
        ) AS next_event_name
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
    WHERE event_name IN (
        'OnRunCreated',
        'OnRunResumed'
    )
    AND next_event_name IN (
        'OnRunPaused',
        'Victory',
        'Defeat',
        'Surrender'
    )
)
SELECT
    @runId AS run_id,
    MIN(start_time) AS start_time,
    MAX(end_time) AS end_time,
    SUM(end_time - start_time) AS run_duration
FROM run_segments
GROUP BY run_id;