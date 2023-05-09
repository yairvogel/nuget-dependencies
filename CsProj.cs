using System.Xml.Serialization;

internal static class CsProjSerializer
{
    public static List<Package.Key> DeserializeToplevelPackages(string rawCsProj)
    {
        var serializer = new XmlSerializer(typeof(Project));
        var project = (Project)serializer.Deserialize(new StringReader(rawCsProj));
        return project.ItemGroup.PackageReference.Select(p => new Package.Key(p.Name, p.Version)).ToList();
    }
}

[XmlRoot(ElementName = "Project")]
public class Project
{
    [XmlElement(ElementName = "ItemGroup")]
    public ItemGroup ItemGroup { get; set; }
}

[XmlRoot(ElementName = "ItemGroup")]
public class ItemGroup
{
    [XmlElement(ElementName = "PackageReference")]
    public List<PackageReference> PackageReference { get; set; }
}

[XmlRoot(ElementName = "PackageReference")]
public class PackageReference
{
    [XmlAttribute(AttributeName = "Version")]
    public string Version { get; set; }

    [XmlAttribute(AttributeName = "Include")]
    public string Name { get; set; }
}