using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BaranKoc.RuntimeScriptableEdit
{
    public static class RuntimeScriptableEditRegistry
    {
        public class ScriptableEditEntry
        {
            public ScriptableObject originalAsset;
            public ScriptableObject runtimeCopy;
            public ScriptableObject revertSnapshot;
            public bool hasUnsavedChanges;
            public bool isPermaSaved;

            public ScriptableEditEntry(ScriptableObject original)
            {
                originalAsset = original;
                CreateRuntimeCopy();
                CreateRevertSnapshot();
            }

            private void CreateRuntimeCopy()
            {
                if (originalAsset == null) return;

                runtimeCopy = Object.Instantiate(originalAsset);
                runtimeCopy.name = $"{originalAsset.name} (Runtime)";

                // These copies are not assets and must outlive any scene load that happens
                // during play mode. Without DontSave, loading a second scene destroys them
                // and every patched reference goes null mid-session.
                runtimeCopy.hideFlags = HideFlags.DontSave;
            }

            private void CreateRevertSnapshot()
            {
                if (originalAsset == null) return;

                revertSnapshot = Object.Instantiate(originalAsset);
                revertSnapshot.name = $"{originalAsset.name} (Snapshot)";
                revertSnapshot.hideFlags = HideFlags.DontSave;
            }

            public void Apply()
            {
                if (runtimeCopy == null || originalAsset == null)
                {
                    Debug.LogWarning("Cannot apply: missing runtime or asset reference");
                    return;
                }

                // CopySerialized overwrites every serialized field, including the name and the
                // hide flags. Both belong to the destination object rather than to the values
                // being copied, so they are captured and put back. Letting the copy's
                // DontSave flag through would stop the asset writing to disk entirely.
                string originalName = originalAsset.name;
                HideFlags originalHideFlags = originalAsset.hideFlags;

                Undo.RecordObject(originalAsset, "Apply Runtime Changes");
                EditorUtility.CopySerialized(runtimeCopy, originalAsset);
                originalAsset.name = originalName;
                originalAsset.hideFlags = originalHideFlags;
                EditorUtility.SetDirty(originalAsset);
                AssetDatabase.SaveAssets();

                hasUnsavedChanges = false;
                isPermaSaved = false;
            }

            public void PermaSave()
            {
                if (runtimeCopy == null || originalAsset == null || revertSnapshot == null)
                {
                    Debug.LogWarning("Cannot perma save: missing references");
                    return;
                }

                // See Apply() — name and hide flags survive the copy deliberately.
                string originalName = originalAsset.name;
                HideFlags originalHideFlags = originalAsset.hideFlags;

                Undo.RecordObject(originalAsset, "Perma Save Runtime Changes");
                EditorUtility.CopySerialized(runtimeCopy, originalAsset);
                originalAsset.name = originalName;
                originalAsset.hideFlags = originalHideFlags;
                EditorUtility.SetDirty(originalAsset);
                AssetDatabase.SaveAssets();

                string snapshotName = revertSnapshot.name;
                EditorUtility.CopySerialized(runtimeCopy, revertSnapshot);
                revertSnapshot.name = snapshotName;
                revertSnapshot.hideFlags = HideFlags.DontSave;

                hasUnsavedChanges = false;
                isPermaSaved = true;
            }

            public void Revert()
            {
                if (runtimeCopy == null || revertSnapshot == null)
                {
                    Debug.LogWarning("Cannot revert: missing runtime or snapshot reference");
                    return;
                }

                // Without this the runtime copy inherits the snapshot's "(Snapshot)" name and
                // shows up under the wrong label in the tuning window.
                string runtimeName = runtimeCopy.name;

                EditorUtility.CopySerialized(revertSnapshot, runtimeCopy);
                runtimeCopy.name = runtimeName;
                runtimeCopy.hideFlags = HideFlags.DontSave;

                Apply();
                isPermaSaved = false;
            }

            public void Cleanup()
            {
                if (runtimeCopy != null)
                {
                    Object.DestroyImmediate(runtimeCopy);
                    runtimeCopy = null;
                }

                if (revertSnapshot != null)
                {
                    Object.DestroyImmediate(revertSnapshot);
                    revertSnapshot = null;
                }
            }
        }

        private static Dictionary<ScriptableObject, ScriptableEditEntry> registry = new Dictionary<ScriptableObject, ScriptableEditEntry>();
        private static RuntimeScriptableEditProfile activeProfile;

        public static bool IsInitialized => registry.Count > 0;
        public static RuntimeScriptableEditProfile ActiveProfile => activeProfile;
        public static IReadOnlyDictionary<ScriptableObject, ScriptableEditEntry> Entries => registry;

        public static void Initialize(RuntimeScriptableEditProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("Cannot initialize RuntimeScriptableEditRegistry: profile is null");
                return;
            }

            Clear();

            activeProfile = profile;

            foreach (var asset in profile.tunableAssets)
            {
                if (asset == null)
                {
                    Debug.LogWarning("Skipping null asset in scriptable edit profile");
                    continue;
                }

                if (registry.ContainsKey(asset))
                {
                    Debug.LogWarning($"Asset already registered: {asset.name}");
                    continue;
                }

                var entry = new ScriptableEditEntry(asset);
                registry[asset] = entry;
            }
        }

        public static ScriptableEditEntry GetEntry(ScriptableObject asset)
        {
            if (asset == null) return null;
            return registry.TryGetValue(asset, out var entry) ? entry : null;
        }

        public static ScriptableObject GetRuntimeCopy(ScriptableObject originalAsset)
        {
            var entry = GetEntry(originalAsset);
            return entry?.runtimeCopy;
        }

        public static bool IsOriginalAsset(ScriptableObject asset)
        {
            return registry.ContainsKey(asset);
        }

        public static void ApplyAll()
        {
            foreach (var entry in registry.Values)
            {
                entry.Apply();
            }
        }

        public static void PermaSaveAll()
        {
            foreach (var entry in registry.Values)
            {
                entry.PermaSave();
            }
        }

        public static void RevertAll()
        {
            foreach (var entry in registry.Values)
            {
                entry.Revert();
            }
        }

        public static void RevertTemporaryChanges()
        {
            foreach (var entry in registry.Values)
            {
                if (!entry.isPermaSaved)
                {
                    entry.Revert();
                }
            }
        }

        public static void Clear()
        {
            foreach (var entry in registry.Values)
            {
                entry.Cleanup();
            }

            registry.Clear();
            activeProfile = null;
        }
    }
}
