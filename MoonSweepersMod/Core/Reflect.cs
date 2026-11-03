using System;
// Reference IL2CPP reflection types through fully-qualified names or these
// aliases, so unqualified Object/Type do not collide with System.Object/System.Type.
using Object = Il2CppSystem.Object;
using Type = Il2CppSystem.Type;

namespace MoonSweepersMod.Core
{
    /// <summary>
    /// Runtime reflection helpers over IL2CPP objects. Everything is defensive:
    /// a failure simply logs and returns null so the mod never crashes the game.
    /// All heavy lifting happens through Il2CppSystem.Reflection so we never need
    /// the game's compile-time type definitions.
    /// </summary>
    public static class Reflect
    {
        public static Il2CppSystem.Type GetType(string name)
        {
            try { return Type.GetType(name); }
            catch (Exception e) { Log.Error("[Reflect] GetType(" + name + "): " + e.Message); return null; }
        }

        public static Il2CppSystem.Reflection.MethodInfo Method(object target, string name)
        {
            try { return (target as Object)?.GetType()?.GetMethod(name); }
            catch (Exception e) { Log.Error("[Reflect] Method " + name + ": " + e.Message); return null; }
        }

        public static Il2CppSystem.Reflection.MethodInfo StaticMethod(string typeName, string name)
        {
            try { return GetType(typeName)?.GetMethod(name); }
            catch (Exception e) { Log.Error("[Reflect] Static " + typeName + "." + name + ": " + e.Message); return null; }
        }

        public static object Call(object target, string name, params object[] args)
        {
            try { return Method(target, name)?.Invoke(target, args); }
            catch (Exception e) { Log.Error("[Reflect] Call " + name + ": " + e.Message); return null; }
        }

        public static object CallStatic(string typeName, string name, params object[] args)
        {
            try { return StaticMethod(typeName, name)?.Invoke(null, args); }
            catch (Exception e) { Log.Error("[Reflect] CallStatic " + typeName + "." + name + ": " + e.Message); return null; }
        }

        public static object GetField(object target, string name)
        {
            try { return (target as Object)?.GetType()?.GetField(name)?.GetValue(target); }
            catch (Exception e) { Log.Error("[Reflect] GetField " + name + ": " + e.Message); return null; }
        }

        public static void SetField(object target, string name, object value)
        {
            try { (target as Object)?.GetType()?.GetField(name)?.SetValue(target, value); }
            catch (Exception e) { Log.Error("[Reflect] SetField " + name + ": " + e.Message); }
        }

        public static object GetProp(object target, string name)
        {
            try { return (target as Object)?.GetType()?.GetProperty(name)?.GetValue(target, null); }
            catch (Exception e) { Log.Error("[Reflect] GetProp " + name + ": " + e.Message); return null; }
        }

        public static T GetFieldT<T>(object target, string name)
        {
            var v = GetField(target, name);
            if (v == null) return default;
            try { return (T)Convert.ChangeType(v, typeof(T)); }
            catch { return default; }
        }

        /// <summary>Build a real IL2CPP UnityEngine.Vector3 so it can be passed to game methods.</summary>
        public static object MakeVec3(float x, float y, float z)
        {
            try
            {
                var vt = GetType("UnityEngine.Vector3, UnityEngine");
                if (vt == null) return null;
                return Il2CppSystem.Activator.CreateInstance(vt, (object)x, (object)y, (object)z);
            }
            catch (Exception e) { Log.Error("[Reflect] MakeVec3: " + e.Message); return null; }
        }

        /// <summary>Set an object's transform world position using a freshly built IL2CPP Vector3.</summary>
        public static void SetPosition(object target, float x, float y, float z)
        {
            try
            {
                var t = (target as Object)?.GetType();
                var transform = t?.GetField("transform")?.GetValue(target);
                var vec = MakeVec3(x, y, z);
                if (transform != null && vec != null)
                {
                    (transform as Object)?.GetType()?.GetMethod("set_position")?.Invoke(transform, new object[] { vec });
                }
            }
            catch (Exception e) { Log.Error("[Reflect] SetPosition: " + e.Message); }
        }

        public static UnityEngine.Vector3 Vec3(object il2cppVec)
        {
            if (il2cppVec == null) return UnityEngine.Vector3.zero;
            try
            {
                float x = Convert.ToSingle(GetField(il2cppVec, "x"));
                float y = Convert.ToSingle(GetField(il2cppVec, "y"));
                float z = Convert.ToSingle(GetField(il2cppVec, "z"));
                return new UnityEngine.Vector3(x, y, z);
            }
            catch (Exception e) { Log.Error("[Reflect] Vec3: " + e.Message); return UnityEngine.Vector3.zero; }
        }

        /// <summary>Find a loaded IL2CPP type by simple or full name (optionally within an assembly).</summary>
        public static Il2CppSystem.Type FindType(string simpleName, string assembly = null)
        {
            try
            {
                var asm = Il2CppSystem.Reflection.Assembly.GetExecutingAssembly();
                if (asm == null) return null;
                var types = asm.GetTypes();
                if (types == null) return null;
                foreach (var t in types)
                {
                    if (t.Name == simpleName || t.FullName == simpleName)
                    {
                        if (assembly == null || (t.Assembly?.FullName ?? "").Contains(assembly)) return t;
                    }
                }
            }
            catch (Exception e) { Log.Error("[Reflect] FindType " + simpleName + ": " + e.Message); }
            return null;
        }
    }
}
