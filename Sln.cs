
using System.Text.RegularExpressions;

static class SlnSerializer
{
    // regex to match csproj lines
    private static readonly Regex _csprojRegex = new Regex(
        @"Project\(""\{(?<typeGuid>.*)\}""\) = ""(?<name>.*)"", ""(?<path>.*)\.csproj"", ""\{(?<projectGuid>.*)\}""",
        RegexOptions.Compiled);

    public static IEnumerable<CsProj> DeserializeCsProjs(IEnumerable<string> rawSln)
    {
        return rawSln
            .Where(l => l.StartsWith("Project("))
            .Select(l => _csprojRegex.Match(l))
            .Where(m => m.Success)
            .Select(m => new CsProj(m.Groups["name"].Value, m.Groups["path"].Value));
    }
}

record CsProj(string Name, string Path);