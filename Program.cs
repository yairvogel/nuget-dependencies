
// get the package to search for as a command line argument
string packageToSearchFor = args[0];
string versionToSearchFor = args.Length > 1 ? args[1] : string.Empty;

var slnFile = Directory.GetFiles(Environment.CurrentDirectory, "*.sln").Single();
var rawSln = File.ReadLines(slnFile);
IEnumerable<CsProj> csprojs = SlnSerializer.DeserializeCsProjs(rawSln);

foreach (var csproj in csprojs)
{
    var rawCsProj = File.ReadAllText($"{csproj.Path}.csproj");
    var toplevelPackages = CsProjSerializer.DeserializeToplevelPackages(rawCsProj).ToHashSet();

    Dictionary<string, List<Package>> targetPackages = null;
    try {
    var projectAssetsFile = Path.Combine(Path.GetDirectoryName(csproj.Path), "obj", "project.assets.json");
    var rawJson = File.ReadAllText(projectAssetsFile);
    targetPackages = ProjectAssets.DeserializePackages(rawJson);
    }
    catch (Exception e)
    {
        throw new Exception($"Failed to deserialize {csproj.Path}", e);
    }

    foreach ((var target, var packages) in targetPackages)
    {
        foreach (var path in GetPaths(packages))
        {
            var pathString = string.Join("->", path.Select(p => $"{p.Name}({p.Version})"));
            Console.WriteLine($"{csproj.Name}({target}) -> {pathString}");
        }
    }
}

IEnumerable<List<Package.Key>> GetPaths(IEnumerable<Package> packages)
{
    foreach (var package in packages)
    {
        if (Matches(package))
        {
            yield return new List<Package.Key> { Package.ExtractKey(package) };
        }
        else
        {
            foreach (var path in GetPaths(package.Dependencies.Values))
            {
                path.Insert(0, Package.ExtractKey(package));
                yield return path;
            }
        }
    }
}

bool Matches(Package package)
{
    var nameMatches = package.Name.Equals(packageToSearchFor, StringComparison.OrdinalIgnoreCase);
    var versionMatches = (string.IsNullOrEmpty(versionToSearchFor) || package.Version == versionToSearchFor);
    return nameMatches && versionMatches;
}
