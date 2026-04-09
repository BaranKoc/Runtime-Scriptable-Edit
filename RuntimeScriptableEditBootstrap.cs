using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class RuntimeScriptableEditBootstrap
{
    private const string ACTIVE_PROFILE_PREF_KEY = "RuntimeScriptableEdit_ActiveProfilePath";

    static RuntimeScriptableEditBootstrap()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                InitializeScriptableEditSystem();
                break;

            case PlayModeStateChange.ExitingPlayMode:
                CleanupScriptableEditSystem();
                break;
        }
    }

    private static void InitializeScriptableEditSystem()
    {
        RuntimeScriptableEditProfile profile = GetActiveProfile();

        if (profile == null)
        {
            return;
        }

        if (profile.tunableAssets == null || profile.tunableAssets.Count == 0)
        {
            Debug.LogWarning($"[Runtime Scriptable Edit] Profile '{profile.name}' has no tunable assets assigned.");
            return;
        }

        RuntimeScriptableEditRegistry.Initialize(profile);
        RuntimeScriptableEditReferenceReplacer.ReplaceAllReferences();
    }

    private static void CleanupScriptableEditSystem()
    {
        RuntimeScriptableEditRegistry.RevertTemporaryChanges();
        RuntimeScriptableEditRegistry.Clear();
    }

    public static RuntimeScriptableEditProfile GetActiveProfile()
    {
        string path = EditorPrefs.GetString(ACTIVE_PROFILE_PREF_KEY, "");
        
        if (string.IsNullOrEmpty(path))
            return null;

        return AssetDatabase.LoadAssetAtPath<RuntimeScriptableEditProfile>(path);
    }

    public static void SetActiveProfile(RuntimeScriptableEditProfile profile)
    {
        if (profile == null)
        {
            EditorPrefs.DeleteKey(ACTIVE_PROFILE_PREF_KEY);
            Debug.Log("[Runtime Scriptable Edit] Active profile cleared");
        }
        else
        {
            string path = AssetDatabase.GetAssetPath(profile);
            EditorPrefs.SetString(ACTIVE_PROFILE_PREF_KEY, path);
            Debug.Log($"[Runtime Scriptable Edit] Active profile set to: {profile.name}");
        }
    }
}
