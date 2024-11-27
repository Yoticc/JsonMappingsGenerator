var input = "";
string? line;
while (!string.IsNullOrEmpty(line = Console.ReadLine()))
    input += line;
Console.Clear();

var generator = new Generator();
var output = generator.Generate(string.Concat(input));
Console.WriteLine(string.Join('\n', output));
Console.ReadLine();