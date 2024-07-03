using Newtonsoft.Json.Linq;
class Generator
{
    public string JsonPropertyClassName = "JsonProperty";

    public string Generate(string input)
    {
        var cls = new ClassFile();

        if (input[0] != '{')
            throw new Exception("No bracket at the start of input text"); //

        var root = JToken.Parse(input);
        cls.Class = Parse(null, root);    

        var lines = cls.Serialize(this);
        return string.Join("\n", lines);
    }

    Class Parse(Class? owner, JToken token, bool isArray = false)
    {
        var resultClass = new Class();
        if (owner is not null)
            owner.AddNestedClass(resultClass);

        if (token is JProperty propToken)
        {
            resultClass.Name = GetUpperName(propToken.Name) + "Data";
            token = propToken.Value;
        }

        if (isArray)
            token.Children().First().Children().each(HandleToken);
        else token.Children().each(HandleToken);

        void HandleToken(JToken childToken)
        {
            if (childToken is not JProperty propChildToken)
                return;

            var value = propChildToken.Value;
            var type = value.Type;
            var name = propChildToken.Name;

            if (name.Length == 0)
                return;

            if (name.Contains('/'))
                return;

            var upperName = GetUpperName(name);

            var field = type switch
            {
                JTokenType.Object => value is JObject objectValue ? GetField(Parse(resultClass, propChildToken).GetSimplifiedPath(owner)) : GetField("object"),
                JTokenType.Array =>
                    value is JArray arrayValue
                     ? (
                        arrayValue.Count != 0
                         ? GetField(Parse(resultClass, propChildToken, isArray: true).GetSimplifiedPath(owner).ex(path => path.EndsWith('s') ? path[0..^2] : path) + "[]")
                         : GetField("object[]"))
                     : null!,
                _ => GetField(TokenTypeToType[type])
            }; ;

            if (field.Type is null)
                return; 

            resultClass.AddField(field);

            Field GetField(string type) => new(type, name, upperName);
        }

        return resultClass;
    }

    string GetUpperName(string name) => new string(name.ToCharArray().ex(a => { a[0] = char.ToUpper(a[0]); return a; }));

    static readonly Dictionary<JTokenType, string> TokenTypeToType = new()
    {
        { JTokenType.Integer , "long" },
        { JTokenType.Float , "double" },
        { JTokenType.String , "string" },
        { JTokenType.Boolean , "bool" },
        { JTokenType.Date , "DateTime" },
        { JTokenType.TimeSpan , "TimeSpan" },
        { JTokenType.Null , "object" },
    };
}