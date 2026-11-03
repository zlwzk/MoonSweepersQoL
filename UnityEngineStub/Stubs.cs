// Compile-time stub for UnityEngine. NOT shipped with the mod.
// Il2CppInterop provides the real IL2CPP-backed UnityEngine types at runtime.
// This stub exists only so the BepInEx IL2CPP managed assemblies can be
// resolved by the C# compiler when building the plugin.

using System;

namespace UnityEngine
{
    public class Object { public string name; public int GetInstanceID() => 0; public virtual void SetActive(bool v) { } }
    public class YieldInstruction { }
    public class Coroutine : YieldInstruction { }
    public class WaitForSeconds : YieldInstruction { public WaitForSeconds(float s) { } }
    public class GameObject : Object { public GameObject() { } public GameObject(string n) { } public Transform transform => null; public T AddComponent<T>() where T : Component => null; public Component AddComponent(System.Type t) => null; public bool activeSelf => false; public new void SetActive(bool v) { } public static GameObject CreatePrimitive(PrimitiveType t) => null; public static void Destroy(Object o) { } }
    public class Component : Object { public Transform transform => null; public GameObject gameObject => null; }
    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour { public void Invoke(string m, float t) { } public Coroutine StartCoroutine(System.Collections.IEnumerator e) => null; public virtual void Awake() { } public virtual void Start() { } public virtual void Update() { } public virtual void FixedUpdate() { } public virtual void OnEnable() { } public virtual void OnDisable() { } public virtual void OnGUI() { } }
    public class Transform : Component { public Vector3 position; public Quaternion rotation; public Vector3 localPosition; public Vector3 eulerAngles; public Transform parent; public void Translate(Vector3 v) { } public void Rotate(Vector3 v) { } }
    public struct Vector2 { public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; } public static Vector2 zero => new Vector2(0,0); }
    public struct Vector3 { public float x, y, z; public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; } public static Vector3 zero => new Vector3(0,0,0); public static Vector3 forward => new Vector3(0,0,1); public static float Distance(Vector3 a, Vector3 b) => 0; public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a; }
    public struct Vector4 { public float x, y, z, w; public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; } }
    public struct Quaternion { public float x, y, z, w; public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; } public static Quaternion identity => new Quaternion(0,0,0,1); }
    public struct Vector2Int { public int x, y; }
    public struct Vector3Int { public int x, y, z; }
    public struct Color { public float r, g, b, a; public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; } public static Color white => new Color(1,1,1,1); public static Color black => new Color(0,0,0,1); public static Color red => new Color(1,0,0,1); public static Color green => new Color(0,1,0,1); }
    public struct Color32 { public byte r, g, b, a; }
    public struct Rect { public float x, y, width, height; public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; this.width = w; this.height = h; } }
    public struct RectOffset { }
    public struct Bounds { public Vector3 center; public Vector3 size; }
    public struct Ray { public Ray(Vector3 o, Vector3 d) { } }
    public struct RaycastHit { public Vector3 point; public Collider collider; public float distance; }
    public struct Matrix4x4 { }
    public struct Plane { }
    public struct Touch { public int fingerId; public Vector2 position; }

    public class Mathf { public const float PI = 3.14159265f; public static float Clamp(float v, float a, float b) => v; public static float Lerp(float a, float b, float t) => a; public static float Abs(float v) => v; public static float Sqrt(float v) => v; public static float Sin(float v) => v; public static float Cos(float v) => v; public static float Atan2(float y, float x) => 0; public static float Deg2Rad => 0.01745329f; }
    public static class Time { public static float deltaTime => 0; public static float time => 0; public static float unscaledDeltaTime => 0; public static int frameCount => 0; }
    public static class Application { public static bool isPlaying => false; public static string dataPath => ""; public static string persistentDataPath => ""; public static void Quit() { } public static bool isBatchMode => false; }
    public static class Debug { public static void Log(object o) { } public static void LogWarning(object o) { } public static void LogError(object o) { } }
    public static class Screen { public static int width => 0; public static int height => 0; public static float dpi => 96; }
    public static class Input { public static bool GetKey(KeyCode k) => false; public static bool GetKeyDown(KeyCode k) => false; public static bool GetKeyUp(KeyCode k) => false; public static bool GetMouseButton(int b) => false; public static bool GetMouseButtonDown(int b) => false; public static Vector3 mousePosition => Vector3.zero; }
    public static class Physics { public static RaycastHit Raycast(Ray r) { return new RaycastHit(); } public static bool Raycast(Ray r, out RaycastHit h) { h = new RaycastHit(); return false; } }
    public static class Physics2D { }
    public static class Random { public static float Range(float a, float b) => a; }
    public static class PlayerPrefs { public static int GetInt(string k, int d = 0) => d; public static void SetInt(string k, int v) { } public static float GetFloat(string k, float d = 0) => d; public static void SetFloat(string k, float v) { } public static string GetString(string k, string d = "") => d; public static void SetString(string k, string v) { } public static void Save() { } }
    public static class JsonUtility { public static string ToJson(object o) => ""; public static T FromJson<T>(string s) => default; }
    public static class Resources { public static Object Load(string p) => null; }
    public static class AssetBundle { }
    public static class Cursor { public static bool visible { get; set; } public static void SetCursor(Texture2D t, Vector2 v, int m) { } }
    public static class QualitySettings { public static int masterTextureLimit { get; set; } }
    public static class RenderTexture { }
    public static class GUILayout { public static void Label(string s) { } public static void Label(string s, GUILayoutOption[] o) { } public static void Box(Rect r, string s) { } public static bool Button(string s) => false; public static Rect Window(int id, Rect r, Action<int> f, string t) => r; public static void BeginArea(Rect r) { } public static void EndArea() { } }
    public class GUILayoutOption { }
    public class GUIStyle { public GUIStyle() { } public GUIStyle(string n) { } }
    public class GUIContent { public GUIContent(string t) { } }
    public static class GUI { public static void Label(Rect r, string s) { } public static bool Button(Rect r, string s) => false; public static void Box(Rect r, string s) { } public static void DrawTexture(Rect r, Texture t) { } public static string TextField(Rect r, string s) => s; public static bool Toggle(Rect r, bool v, string s) => v; public static void BeginGroup(Rect r) { } public static void EndGroup() { } public static Color color { get; set; } public static bool enabled { get; set; } }
    public class Event { public static Event current => null; public Vector2 mousePosition => Vector2.zero; public EventType type => EventType.Repaint; public int button => 0; public bool isMouse => false; public bool isKey => false; public KeyCode keyCode => KeyCode.None; }
    public class Texture { }
    public class Texture2D : Texture { public Texture2D(int w, int h) { } }
    public class Sprite : Object { }
    public class Material : Object { public Material(Shader s) { } }
    public class Shader : Object { public static Shader Find(string n) => null; }
    public class Mesh : Object { }
    public class Renderer : Component { }
    public class MeshRenderer : Renderer { }
    public class Collider : Component { }
    public class BoxCollider : Collider { public Vector3 size; public Vector3 center; }
    public class SphereCollider : Collider { }
    public class CapsuleCollider : Collider { }
    public class MeshCollider : Collider { }
    public class Rigidbody : Component { public Vector3 velocity; public float mass; }
    public class CharacterController : Collider { public bool isGrounded; }
    public class AudioClip : Object { }
    public class AudioSource : Component { }
    public class Animation : Component { }
    public class Animator : Component { }
    public class Light : Component { }
    public class Camera : Component { public static Camera main => null; public Ray ScreenPointToRay(Vector3 p) => new Ray(Vector3.zero, Vector3.forward); }
    public class Scene { }
    public static class SceneManager { public static Scene GetActiveScene() => null; }
    public enum KeyCode { None, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, UpArrow, DownArrow, LeftArrow, RightArrow, LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt, Space, Tab, Return, Escape, Backspace, Delete, Mouse0, Mouse1, Mouse2, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, Comma, Period, Slash, Backslash, LeftBracket, RightBracket, Equals, Minus, Semicolon, Quote }
    public enum EventType { Layout, MouseDown, MouseUp, MouseMove, KeyDown, KeyUp, Repaint }
    public enum HideFlags { None, HideInHierarchy }
    public enum Space { World, Self }
    public enum PrimitiveType { Sphere, Cube, Cylinder, Plane }
    public enum WrapMode { Once, Loop }
}
