SELECT
    COUNT(*) FILTER (WHERE event_type = @victory) AS victories,
    COUNT(*) FILTER (WHERE event_type = @defeat) AS defeats,
    COUNT(*) FILTER (WHERE event_type = @surrender) AS surrenders
FROM events;