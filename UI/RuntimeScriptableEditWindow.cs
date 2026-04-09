using UnityEngine;
using UnityEditor;

public class RuntimeScriptableEditWindow : EditorWindow
{
    private RuntimeScriptableEditProfile activeProfile;
    private Vector2 scrollPosition;
    private Editor[] cachedEditors;

    private const string CONFIRM_APPLY_KEY = "RuntimeScriptableEdit_ConfirmApply";
    private const string CONFIRM_PERMA_SAVE_KEY = "RuntimeScriptableEdit_ConfirmPermaSave";
    private const string CONFIRM_REVERT_KEY = "RuntimeScriptableEdit_ConfirmRevert";

    private bool confirmOnApply;
    private bool confirmOnPermaSave;
    private bool confirmOnRevert;

    [MenuItem("Tools/RuntimeScriptableEdit/Runtime Scriptable Edit Window")]
    public static void ShowWindow()
    {
        GetWindow<RuntimeScriptableEditWindow>("Runtime Scriptable Edit");
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        LoadConfirmationPreferences();
        RefreshActiveProfile();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        CleanupEditors();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            RefreshActiveProfile();
            Repaint();
        }
    }

    private void RefreshActiveProfile()
    {
        activeProfile = RuntimeScriptableEditBootstrap.GetActiveProfile();
        CleanupEditors();
    }

    private void LoadConfirmationPreferences()
    {
        confirmOnApply = EditorPrefs.GetBool(CONFIRM_APPLY_KEY, true);
        confirmOnPermaSave = EditorPrefs.GetBool(CONFIRM_PERMA_SAVE_KEY, true);
        confirmOnRevert = EditorPrefs.GetBool(CONFIRM_REVERT_KEY, true);
    }

    private void SaveConfirmationPreferences()
    {
        EditorPrefs.SetBool(CONFIRM_APPLY_KEY, confirmOnApply);
        EditorPrefs.SetBool(CONFIRM_PERMA_SAVE_KEY, confirmOnPermaSave);
        EditorPrefs.SetBool(CONFIRM_REVERT_KEY, confirmOnRevert);
    }

    private void CleanupEditors()
    {
        if (cachedEditors != null)
        {
            foreach (var editor in cachedEditors)
            {
                if (editor != null)
                {
                    DestroyImmediate(editor);
                }
            }
            cachedEditors = null;
        }
    }

    private void OnGUI()
    {
        DrawHeader();

        if (!Application.isPlaying)
        {
            DrawEditModeUI();
            return;
        }

        if (!RuntimeScriptableEditRegistry.IsInitialized)
        {
            DrawNotInitializedUI();
            return;
        }

        DrawPlayModeUI();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Runtime Scriptable Edit System", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
    }

    private void DrawEditModeUI()
    {
        EditorGUILayout.HelpBox(
            "Runtime Scriptable Edit is only available in Play Mode.\n\n" +
            "Set your active profile below, then enter Play Mode to edit ScriptableObjects.",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        EditorGUI.BeginChangeCheck();
        RuntimeScriptableEditProfile newProfile = EditorGUILayout.ObjectField(
            "Active Profile",
            activeProfile,
            typeof(RuntimeScriptableEditProfile),
            false
        ) as RuntimeScriptableEditProfile;

        if (EditorGUI.EndChangeCheck())
        {
            RuntimeScriptableEditBootstrap.SetActiveProfile(newProfile);
            activeProfile = newProfile;
        }

        if (activeProfile != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Tunable Assets:", EditorStyles.boldLabel);

            if (activeProfile.tunableAssets == null || activeProfile.tunableAssets.Count == 0)
            {
                EditorGUILayout.HelpBox("No tunable assets assigned to this profile.", MessageType.Warning);
            }
            else
            {
                foreach (var asset in activeProfile.tunableAssets)
                {
                    if (asset != null)
                    {
                        EditorGUILayout.ObjectField(asset.name, asset, typeof(ScriptableObject), false);
                    }
                }
            }

        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Confirmation Options:", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        confirmOnApply = EditorGUILayout.Toggle("Confirm Temporary Apply", confirmOnApply);
        confirmOnPermaSave = EditorGUILayout.Toggle("Confirm Perma Save", confirmOnPermaSave);
        confirmOnRevert = EditorGUILayout.Toggle("Confirm Revert", confirmOnRevert);
        
        if (EditorGUI.EndChangeCheck())
        {
            SaveConfirmationPreferences();
        }
    }

    private void DrawNotInitializedUI()
    {
        EditorGUILayout.HelpBox(
            "Runtime Scriptable Edit System not initialized.\n\n" +
            "Make sure you have:\n" +
            "1. Created a RuntimeScriptableEditProfile\n" +
            "2. Assigned tunable ScriptableObjects to it\n" +
            "3. Set it as the active profile",
            MessageType.Warning
        );
    }

    private void DrawPlayModeUI()
    {
        EditorGUILayout.LabelField($"Active Profile: {activeProfile.name}", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        DrawGlobalActions();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Tunable Assets", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var kvp in RuntimeScriptableEditRegistry.Entries)
        {
            DrawTuningEntry(kvp.Key, kvp.Value);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawGlobalActions()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✓ Temporary Apply All", GUILayout.Height(30)))
        {
            if (ShouldConfirmAction(confirmOnApply, "Temporary apply all runtime changes?"))
            {
                RuntimeScriptableEditRegistry.ApplyAll();
            }
        }

        GUI.backgroundColor = new Color(0.2f, 0.8f, 1f);
        if (GUILayout.Button("💾 Perma Save All", GUILayout.Height(30)))
        {
            if (ShouldConfirmAction(confirmOnPermaSave, "Perma save all changes and update revert points?"))
            {
                RuntimeScriptableEditRegistry.PermaSaveAll();
            }
        }

        GUI.backgroundColor = new Color(1f, 0.5f, 0.3f);
        if (GUILayout.Button("↺ Revert All", GUILayout.Height(30)))
        {
            if (ShouldConfirmAction(confirmOnRevert, "Revert all changes to snapshot?"))
            {
                RuntimeScriptableEditRegistry.RevertAll();
            }
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (GUILayout.Button("🔄 Re-patch References", GUILayout.Height(25)))
        {
            RuntimeScriptableEditReferenceReplacer.ReplaceAllReferences();
        }
    }

    private void DrawTuningEntry(ScriptableObject originalAsset, RuntimeScriptableEditRegistry.ScriptableEditEntry entry)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField(originalAsset.name, EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField("Original", originalAsset, typeof(ScriptableObject), false);
        EditorGUILayout.ObjectField("Runtime", entry.runtimeCopy, typeof(ScriptableObject), false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        Editor editor = Editor.CreateEditor(entry.runtimeCopy);
        if (editor != null)
        {
            editor.OnInspectorGUI();
            DestroyImmediate(editor);
        }

        EditorGUILayout.Space(5);

        DrawEntryActions(entry);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    private void DrawEntryActions(RuntimeScriptableEditRegistry.ScriptableEditEntry entry)
    {
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✓ Temporary Apply", GUILayout.Height(25)))
        {
            if (ShouldConfirmAction(confirmOnApply, "Temporary apply runtime changes?"))
            {
                entry.Apply();
            }
        }

        GUI.backgroundColor = new Color(0.2f, 0.8f, 1f);
        if (GUILayout.Button("💾 Perma Save", GUILayout.Height(25)))
        {
            if (ShouldConfirmAction(confirmOnPermaSave, "Perma save changes and update revert point?"))
            {
                entry.PermaSave();
            }
        }

        GUI.backgroundColor = new Color(1f, 0.5f, 0.3f);
        if (GUILayout.Button("↺ Revert", GUILayout.Height(25)))
        {
            if (ShouldConfirmAction(confirmOnRevert, "Revert to snapshot?"))
            {
                entry.Revert();
            }
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    private bool ShouldConfirmAction(bool requiresConfirmation, string message)
    {
        if (!requiresConfirmation)
            return true;

        return EditorUtility.DisplayDialog("Confirm Action", message, "Yes", "No");
    }
}
