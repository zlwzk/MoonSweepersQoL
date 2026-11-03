using BepInEx.Logging;

namespace MoonSweepersMod.Core
{
    /// <summary>Thin wrapper around the BepInEx log so feature code stays readable.</summary>
    public static class Log
    {
        public static ManualLogSource S;

        public static void Init(ManualLogSource src) { S = src; }

        public static void Info(string m) { S?.LogInfo(m); }
        public static void Warn(string m) { S?.LogWarning(m); }
        public static void Error(string m) { S?.LogError(m); }
    }
}
