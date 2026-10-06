WITH ordered_events AS (
    SELECT
        player_id,
        run_id,
        event_time,
        event_name,
        LEAD(event_time) OVER (
            PARTITION BY player_id, run_id
            ORDER BY event_time
        ) AS next_event_time,
        LEAD(event_name) OVER (
            PARTITION BY player_id, run_id
            ORDER BY event_time
        ) AS next_event_name
    FROM events
    WHERE player_id = @playerId
),
run_segments AS (
    SELECT
        player_id,
        run_id,
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
),
completed_runs AS (
    SELECT
        player_id,
        run_id,
        SUM(end_time - start_time) AS run_duration
    FROM run_segments
    GROUP BY player_id, run_id
    HAVING EXISTS (
        SELECT 1
        FROM events completed
        WHERE completed.player_id = run_segments.player_id
        AND completed.run_id = run_segments.run_id
        AND completed.event_name IN (
            'Victory',
            'Defeat',
            'Surrender'
        )
    )
)
SELECT
    AVG(run_duration) AS average_duration
FROM completed_runs;