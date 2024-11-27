class Class
{
    public string? Name;
    public Class? ClassOwner;
    public List<Class> NestedClasses = [];
    public List<Field> Fields = [];

    public string Path => (ClassOwner is not null ? $"{ClassOwner.Path}." : "") + (Name??"TOPNAME");
    public bool IsNested => ClassOwner is not null;
    public bool HasNestedClasses => NestedClasses.Count != 0;
    public bool HasFields => Fields.Count != 0;
    public bool HasMembers => NestedClasses.Count != 0 || Fields.Count != 0;

    public string GetSimplifiedPath(Class? toClass)
    {
        var path = Path;
        if (toClass is null)
            return Path;

        var clsPath = toClass.Path;
        if (path.StartsWith(clsPath))
            return path.Substring(clsPath.Length + 1);

        return Path;
    }

    public void AddNestedClass(Class cls)
    {
        NestedClasses.Add(cls);
        cls.ClassOwner = this;
    }

    public void AddField(Field field) => Fields.Add(field);

    public List<Class> GetAllNestedClasses()
    {
        var result = NestedClasses.ToList();
        foreach (var cls in NestedClasses)
            result.AddRange(cls.GetAllNestedClasses());
        return result;
    }

    public List<string> Serialize(Generator generator)
    {
        List<string> lines = [];
            
        var prefix = IsNested ? "internal " : "";
        var sufix = HasMembers ? "(" : ";";

        lines.Add($"{prefix}record {Name??"TOPNAME"}{sufix}");

        if (!HasMembers)
            return lines;
        
        if (HasFields)
        {
            for (var i = 0; i < Fields.Count; i++)
            {
                var field = Fields[i];
                lines.Add($"    [{generator.JsonPropertyClassName}(\"{field.JsonName}\")] {field.Type} {field.Name}{((i + 1) != Fields.Count ? "," : "")}");
            }
        }

        if (HasNestedClasses)
        {
            lines[^1] = lines[^1] + "){";
            //lines.Add("    ){");
            for (var i = 0; i < NestedClasses.Count; i++)
            {
                var cls = NestedClasses[i];
                foreach (var line in cls.Serialize(generator))
                    lines.Add("    " + line);
            }
            lines.Add("}");
        }
        else lines[^1] = lines[^1] + ");";


        return lines;
    }
}