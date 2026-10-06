internal static class SqlFiles
{
    public static string Load(Type owner, string fileName)
    {
        var name = $"{owner.Namespace}.{fileName}";
        using var stream = owner.Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded SQL not found: {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // Convention: the file is named after the slice class
    public static string Load(Type owner) => Load(owner, $"{owner.Name}.sql");
}