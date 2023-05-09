
public class Package {
    public string Name { get; set; }

    public string Version { get; set; }

    public Dictionary<Key, Package> Dependencies { get; set; }

    // equality by name and version, name is case insensitive
    public record struct Key(string Name, string Version) : IEquatable<Key> {
        public bool Equals(Key other) => Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) && Version.Equals(other.Version);
        public override int GetHashCode() => HashCode.Combine(Name, Version);
    }

    public static Package.Key ExtractKey(Package package) => new Package.Key(package.Name, package.Version);
}