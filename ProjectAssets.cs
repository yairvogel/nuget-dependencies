using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

public static class ProjectAssets
{
    /// <summary>
    /// returns a dictionary from target name to packages
    /// </summary>
    public static Dictionary<string, List<Package>> DeserializePackages(string rawJson)
    {
        dynamic assets = JObject.Parse(rawJson);
        var targets = assets.targets as JObject ?? Enumerable.Empty<dynamic>();
        var result = new Dictionary<string, List<Package>>();
        foreach (var target in targets)
        {
            List<Package> targetPackages = GetPackagesForTarget(target);
            result.Add(target.Name, targetPackages);
        }
        return result;
    }

    private static List<Package> GetPackagesForTarget(dynamic target)
    {
        var packages = target.Value as JObject ?? Enumerable.Empty<dynamic>();
        var targetPackages = new List<Package>();
        foreach (var package in packages)
        {
            List<Package> packageDependencies = GetPackageDependencies(package);
            string rawName = package.Name.ToString();
            (string name, string version) = ParsePackageName(rawName);
            targetPackages.Add(new Package
            {
                Name = name,
                Version = version,
                Dependencies = packageDependencies.ToDictionary(Package.ExtractKey, p => p)
            });
        }

        return CreateDependencyGraph(targetPackages);
    }

    private static (string name, string version) ParsePackageName(string name)
    {
        var parts = name.Split("/");
        return (parts[0], parts[1]);
    }

    /// <summary>
    /// modify the packages such that they're dependencies reference the toplevel packages
    /// </summary>
    private static List<Package> CreateDependencyGraph(List<Package> targetPackages)
    {
        var packages = targetPackages.ToDictionary(Package.ExtractKey, p => p);
        foreach (var package in targetPackages)
        {
            package.Dependencies = package.Dependencies.ToDictionary(d => d.Key, d => packages.TryGetValue(d.Key, out var p) ? p : d.Value);
        }

        return targetPackages;
    }

    private static List<Package> GetPackageDependencies(dynamic package)
    {
        var dependencies = package.Value["dependencies"] as JObject ?? Enumerable.Empty<dynamic>();
        var packageDependencies = new List<Package>();
        foreach (var dependency in dependencies)
        {
            packageDependencies.Add(new Package
            {
                Name = dependency.Name,
                Version = dependency.Value.ToString(),
                Dependencies = new Dictionary<Package.Key, Package>()
            });
        }

        return packageDependencies;
    }
}