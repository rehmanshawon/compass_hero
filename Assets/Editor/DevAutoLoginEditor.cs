using UnityEditor;
using UnityEngine;

// Editor helper to toggle DevAutoLogin.DevMode
[InitializeOnLoad]
public static class DevAutoLoginEditor
{
    private const string PrefKey = "DevAutoLogin.DevMode";

        static DevAutoLoginEditor()
    {
        // Initialize static DevMode from EditorPrefs on load
        bool enabled = EditorPrefs.GetBool(PrefKey, false);
        SetDevMode(enabled);
    }

    [MenuItem("Dev/Toggle Dev Auto-Login")] 
    public static void ToggleDevMode()
    {
        bool current = EditorPrefs.GetBool(PrefKey, false);
        bool next = !current;
        EditorPrefs.SetBool(PrefKey, next);
        SetDevMode(next);
        Debug.Log("DevAutoLogin: Dev mode " + (next ? "ENABLED" : "DISABLED"));
    }

    [MenuItem("Dev/Toggle Dev Auto-Login", true)]
    public static bool ToggleDevModeValidate()
    {
        Menu.SetChecked("Dev/Toggle Dev Auto-Login", EditorPrefs.GetBool(PrefKey, false));
        return true;
    }

    private static void SetDevMode(bool on)
    {
        // Set static field on the runtime script type if loaded in editor domain
        var type = typeof(UnityEngine.Object).Assembly.GetType("DevAutoLogin");
        // The runtime type may not be available in the editor script domain; set via PlayerSettings or EditorPrefs instead
        // We'll set EditorPrefs only; the runtime script reads its static field at Awake based on EditorPrefs.
        // For simplicity, set the public static on the script if available via reflection.
        var runtimeType = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in runtimeType)
        {
            var t = asm.GetType("DevAutoLogin");
            if (t != null)
            {
                var field = t.GetField("DevMode");
                if (field != null)
                {
                    field.SetValue(null, on);
                }
            }
        }
    }
}
