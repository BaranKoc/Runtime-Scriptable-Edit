using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace BaranKoc.RuntimeScriptableEdit
{
    public static class RuntimeScriptableEditReferenceReplacer
    {
        /// <summary>
        /// One field this system overwrote, and what it held beforehand, so play mode can be
        /// unwound without relying on Unity reloading the scene.
        /// </summary>
        private struct PatchedReference
        {
            public Object owner;
            public FieldInfo field;
            public int index;                  // -1 when the field holds a single reference
            public ScriptableObject originalValue;
        }

        private static readonly List<PatchedReference> patchedReferences = new List<PatchedReference>();
        private static int replacementCount = 0;

        public static int ReplacementCount => replacementCount;

        public static void ReplaceAllReferences()
        {
            if (!RuntimeScriptableEditRegistry.IsInitialized)
            {
                Debug.LogWarning("[Reference Replacer] Registry not initialized");
                return;
            }

            replacementCount = 0;

            MonoBehaviour[] allMonoBehaviours = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (var mb in allMonoBehaviours)
            {
                if (mb == null) continue;
                ReplaceReferencesInObject(mb);
            }

            ScriptableObject[] allScriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();

            foreach (var so in allScriptableObjects)
            {
                if (so == null) continue;
                if (AssetDatabase.Contains(so)) continue;
                if (RuntimeScriptableEditRegistry.IsOriginalAsset(so)) continue;

                ReplaceReferencesInObject(so);
            }
        }

        private static void ReplaceReferencesInObject(Object obj)
        {
            if (obj == null) return;

            System.Type type = obj.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            FieldInfo[] fields = type.GetFields(flags);

            foreach (var field in fields)
            {
                if (field.IsInitOnly) continue;

                if (field.FieldType.IsSubclassOf(typeof(ScriptableObject)) || field.FieldType == typeof(ScriptableObject))
                {
                    ReplaceScriptableObjectField(obj, field);
                }
                else if (field.FieldType.IsArray && field.FieldType.GetElementType().IsSubclassOf(typeof(ScriptableObject)))
                {
                    ReplaceScriptableObjectArray(obj, field);
                }
                else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    System.Type elementType = field.FieldType.GetGenericArguments()[0];
                    if (elementType.IsSubclassOf(typeof(ScriptableObject)) || elementType == typeof(ScriptableObject))
                    {
                        ReplaceScriptableObjectList(obj, field);
                    }
                }
            }
        }

        private static void ReplaceScriptableObjectField(Object obj, FieldInfo field)
        {
            ScriptableObject currentValue = field.GetValue(obj) as ScriptableObject;

            if (currentValue == null) return;

            ScriptableObject runtimeCopy = RuntimeScriptableEditRegistry.GetRuntimeCopy(currentValue);

            if (runtimeCopy != null)
            {
                field.SetValue(obj, runtimeCopy);
                RecordPatch(obj, field, -1, currentValue);
            }
        }

        private static void ReplaceScriptableObjectArray(Object obj, FieldInfo field)
        {
            System.Array array = field.GetValue(obj) as System.Array;

            if (array == null || array.Length == 0) return;

            bool replacedAny = false;

            for (int i = 0; i < array.Length; i++)
            {
                ScriptableObject element = array.GetValue(i) as ScriptableObject;

                if (element == null) continue;

                ScriptableObject runtimeCopy = RuntimeScriptableEditRegistry.GetRuntimeCopy(element);

                if (runtimeCopy != null)
                {
                    array.SetValue(runtimeCopy, i);
                    RecordPatch(obj, field, i, element);
                    replacedAny = true;
                }
            }

            if (replacedAny)
            {
                field.SetValue(obj, array);
            }
        }

        private static void ReplaceScriptableObjectList(Object obj, FieldInfo field)
        {
            object listObj = field.GetValue(obj);

            if (listObj == null) return;

            var list = listObj as System.Collections.IList;

            if (list == null || list.Count == 0) return;

            bool replacedAny = false;

            for (int i = 0; i < list.Count; i++)
            {
                ScriptableObject element = list[i] as ScriptableObject;

                if (element == null) continue;

                ScriptableObject runtimeCopy = RuntimeScriptableEditRegistry.GetRuntimeCopy(element);

                if (runtimeCopy != null)
                {
                    list[i] = runtimeCopy;
                    RecordPatch(obj, field, i, element);
                    replacedAny = true;
                }
            }

            if (replacedAny)
            {
                field.SetValue(obj, list);
            }
        }

        public static void RepatchSingleObject(Object obj)
        {
            if (obj == null) return;
            if (!RuntimeScriptableEditRegistry.IsInitialized) return;

            ReplaceReferencesInObject(obj);
        }

        private static void RecordPatch(Object owner, FieldInfo field, int index, ScriptableObject originalValue)
        {
            patchedReferences.Add(new PatchedReference
            {
                owner = owner,
                field = field,
                index = index,
                originalValue = originalValue
            });

            replacementCount++;
        }

        /// <summary>
        /// Points every patched field back at the asset it referenced before play mode began.
        /// Unity normally reloads the scene on exit and makes this redundant, but under Fast
        /// Enter Play Mode with Reload Scene disabled nothing resets those fields, so without
        /// this they keep referencing runtime copies the registry is about to destroy.
        /// </summary>
        public static void RestoreAllReferences()
        {
            for (int i = patchedReferences.Count - 1; i >= 0; i--)
            {
                RestoreSingleReference(patchedReferences[i]);
            }

            patchedReferences.Clear();
            replacementCount = 0;
        }

        private static void RestoreSingleReference(PatchedReference patch)
        {
            if (patch.owner == null) return;

            if (patch.index < 0)
            {
                patch.field.SetValue(patch.owner, patch.originalValue);
                return;
            }

            object container = patch.field.GetValue(patch.owner);

            if (container is System.Array array)
            {
                if (patch.index < array.Length)
                {
                    array.SetValue(patch.originalValue, patch.index);
                }
                return;
            }

            if (container is System.Collections.IList list)
            {
                if (patch.index < list.Count)
                {
                    list[patch.index] = patch.originalValue;
                }
            }
        }
    }
}
