using System.IO;

namespace MoonSweepersMod.Core
{
    /// <summary>Resolved filesystem paths used by the mod at runtime.</summary>
    public static class Paths
    {
        public static string PluginDir = "";
        public static string DataDir = "";   // BepInEx root
        public static string SteamInputDir = "";
    }
}
