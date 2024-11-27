using Newtonsoft.Json.Linq;
using System.Reflection;
class Generator
{
    public string JsonPropertyClassName = "JsonProperty";
    public bool AppendWatermark = true;

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
            resultClass.Name = GetNoSnakeCaseName(GetUpperName(propToken.Name)) + "Data";
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

            var upperName = GetNoSnakeCaseName(GetUpperName(name));

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
    string GetNoSnakeCaseName(string name)
    {
        var result = "";
        var parts = name.Split('_');
        foreach (var part in parts)
        {
            if (part == "")
                result += '_';
            else result += GetUpperName(part);
        }

        return result;
    }

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