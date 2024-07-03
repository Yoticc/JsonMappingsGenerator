class ClassFile
{
    public Class Class;

    public List<string> Serialize(Generator generator)
    {
        if (Class is null)
            throw new Exception("this.Class is null");

        List<string> lines = [];
        var allNestedClasses = Class.GetAllNestedClasses();
        var usages = allNestedClasses.add(Class).select(cls => cls.Path);

        if (usages.Count != 0)
            lines.Add("");

        lines.AddRange(Class.Serialize(generator));

        return lines;
    }
}