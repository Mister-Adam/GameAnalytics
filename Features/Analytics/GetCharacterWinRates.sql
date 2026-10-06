SELECT
    character,

    COUNT(*) FILTER (
        WHERE event_name = @victory
    ) AS victories,

    COUNT(*) FILTER (
        WHERE event_type = @defeat
    ) AS defeats,

    COUNT(*) FILTER (
        WHERE event_type = @surrender
    ) AS surrenders

FROM events,
     json_array_elements_text(payload -> 'party') AS character

GROUP BY character;