// Compile-time stub for BepInEx.Unity.IL2CPP.
// Its shipped metadata cannot be parsed by the managed compiler, so we provide
// a minimal identically-named assembly. At runtime the real BepInEx-provided
// BepInEx.Unity.IL2CPP satisfies these references.

using BepInEx.Configuration;
using BepInEx.Logging;

namespace BepInEx.IL2CPP
{
    public class BasePlugin
    {
        public ManualLogSource Log { get { return null; } }
        public ConfigFile Config { get { return null; } }

        public virtual void Load()
        {
        }
    }
}
