using ROMulus.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ROMulus.Infrastructure.SystemRegistry;

/// <summary>
/// Loads the system registry from one or more YAML files.
/// Mirrors <c>romulus.models.system</c>'s YAML-loading logic.
///
/// YAML schema (mirrors <c>systems/builtin.yaml</c>):
/// <code>
/// systems:
///   - id: snes
///     display_name: Super Nintendo Entertainment System
///     short_name: SNES
///     manufacturer: Nintendo
///     generation: 4
///     extensions: [.sfc, .smc]
///     folder_aliases: [snes, sfc, supernintendo]
///     header_rule: smc_512
///     libretro_name: "Nintendo - Super Nintendo Entertainment System"
///     dat_name: "Nintendo - Super Nintendo Entertainment System"
/// </code>
/// </summary>
public static class SystemRegistryLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Loads all systems from every <c>*.yaml</c> file in <paramref name="directory"/>.
    /// Returns an empty list (not an exception) if the directory doesn't exist.
    /// </summary>
    public static IReadOnlyList<SystemInfo> LoadFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return [];

        var systems = new List<SystemInfo>();
        foreach (var yamlFile in Directory.EnumerateFiles(directory, "*.yaml"))
        {
            systems.AddRange(LoadFromFile(yamlFile));
        }
        return systems;
    }

    /// <summary>Loads systems from a single YAML file.</summary>
    public static IReadOnlyList<SystemInfo> LoadFromFile(string yamlPath)
    {
        var yaml = File.ReadAllText(yamlPath);
        return LoadFromYaml(yaml);
    }

    /// <summary>Loads systems from a YAML string (useful for tests).</summary>
    public static IReadOnlyList<SystemInfo> LoadFromYaml(string yaml)
    {
        var doc = Deserializer.Deserialize<YamlSystemDocument>(yaml);
        return doc?.Systems?.Select(ToSystemInfo).ToList() ?? [];
    }

    // ── YAML DTO (private) ────────────────────────────────────────────────────

    private sealed class YamlSystemDocument
    {
        public List<YamlSystemDef>? Systems { get; set; }
    }

    private sealed class YamlSystemDef
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string ShortName { get; set; } = "";
        public string? Manufacturer { get; set; }
        public int? Generation { get; set; }
        public List<string> Extensions { get; set; } = [];
        public string? HeaderRule { get; set; }
        public string? LibretroName { get; set; }
        public List<string> FolderAliases { get; set; } = [];
        public string? DatName { get; set; }
        // Fields present in the YAML but not in the core model — ignored.
        // (logo_dark, logo_light, gamedb_file, dat_name_aliases)
    }

    private static SystemInfo ToSystemInfo(YamlSystemDef d) => new()
    {
        Id = d.Id,
        DisplayName = d.DisplayName,
        ShortName = d.ShortName,
        Manufacturer = d.Manufacturer,
        Generation = d.Generation,
        Extensions = d.Extensions,
        HeaderRule = d.HeaderRule,
        LibretroName = d.LibretroName,
        FolderAliases = d.FolderAliases,
        DatName = d.DatName,
    };
}
