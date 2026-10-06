internal static class SqlFiles
{
    public static string Load(Type owner, string fileName)
    {
        var assembly = owner.Assembly;
        var names = assembly.GetManifestResourceNames();

        // Try the full namespace, then progressively shorter tails:
        // GameAnalytics.Features.Events -> Features.Events -> Events
        var segments = (owner.Namespace ?? "").Split('.');
        for (var i = 0; i < segments.Length; i++)
        {
            var suffix = $"{string.Join('.', segments[i..])}.{fileName}";
            var match = names.FirstOrDefault(n => n.EndsWith("." + suffix, StringComparison.Ordinal)
                                               || n == suffix);
            if (match is not null)
            {
                using var stream = assembly.GetManifestResourceStream(match)!;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        throw new InvalidOperationException(
            $"Embedded SQL '{fileName}' not found. Available: {string.Join(", ", names)}");
    }

    public static string Load(Type owner) => Load(owner, $"{owner.Name}.sql");
}