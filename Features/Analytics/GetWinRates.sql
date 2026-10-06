SELECT
    COUNT(*) FILTER (WHERE event_name = @victory) AS victories,
    COUNT(*) FILTER (WHERE event_name = @defeat) AS defeats,
    COUNT(*) FILTER (WHERE event_name = @surrender) AS surrenders
FROM events;