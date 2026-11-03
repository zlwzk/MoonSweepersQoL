// Compile-time stub for Il2CppInterop.Runtime.
// The shipped Il2CppInterop.Runtime.dll ships with metadata that the managed
// compiler/metadata reader cannot parse, so we provide a minimal, identically
// named assembly that declares only the surface this mod uses. At runtime the
// real BepInEx-provided Il2CppInterop.Runtime satisfies these references.

namespace Il2CppSystem
{
    public class Object
    {
        // Il2CppInterop declares this as `public new Type GetType()` returning the
        // Il2CppSystem.Type (not System.Type). We mirror that signature.
        public new Type GetType() => null;
    }

    public sealed class String : Object
    {
        public static implicit operator String(string s) => null;
        public static implicit operator string(String s) => null;
    }

    public class Type
    {
        public static Type GetType(string typeName) => null;

        public string FullName => null;
        public string Name => null;
        public string Namespace => null;
        public Type BaseType => null;

        public Il2CppSystem.Reflection.Assembly Assembly => null;
        public Type[] GetInterfaces() => null;
        public Il2CppSystem.Reflection.MethodInfo GetMethod(string name) => null;
        public Il2CppSystem.Reflection.MethodInfo GetMethod(string name, Type[] types) => null;
        public Il2CppSystem.Reflection.FieldInfo GetField(string name) => null;
        public Il2CppSystem.Reflection.PropertyInfo GetProperty(string name) => null;
    }

    public class Activator
    {
        public static object CreateInstance(Type type) => null;
        public static object CreateInstance(Type type, params object[] args) => null;
        public static object CreateInstance(string assemblyName, string typeName) => null;
    }

    namespace Reflection
    {
        public class MemberInfo
        {
            public string Name => null;
        }

        public class MethodInfo : MemberInfo
        {
            public object Invoke(object obj, object[] parameters) => null;
            public object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, object binder, object[] parameters, System.Globalization.CultureInfo culture) => null;
        }

        public class FieldInfo : MemberInfo
        {
            public object GetValue(object obj) => null;
            public void SetValue(object obj, object value) { }
        }

        public class PropertyInfo : MemberInfo
        {
            public object GetValue(object obj, object[] index) => null;
            public void SetValue(object obj, object value, object[] index) { }
        }

        public class Assembly
        {
            public static Assembly GetExecutingAssembly() => null;
            public Type[] GetTypes() => null;
            public Type GetType(string name) => null;
            public string FullName => null;
        }
    }
}

namespace Il2CppInterop.Runtime.Injection
{
    public class RegisterTypeOptions
    {
        public static RegisterTypeOptions Options => null;
    }

    public static class ClassInjector
    {
        public static void RegisterTypeInIl2Cpp(System.Type type, RegisterTypeOptions options) { }
        public static void RegisterTypeInIl2Cpp(System.Type type) { }
        public static void RegisterTypeInIl2Cpp<T>(RegisterTypeOptions options) where T : class { }
    }
}
