using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public static class RuntimeScriptableEditReferenceReplacer
{
    private static int replacementCount = 0;

    public static void ReplaceAllReferences()
    {
        if (!RuntimeScriptableEditRegistry.IsInitialized)
        {
            Debug.LogWarning("[Reference Replacer] Registry not initialized");
            return;
        }

        replacementCount = 0;

        MonoBehaviour[] allMonoBehaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        
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
            replacementCount++;
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
                replacementCount++;
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

        System.Type elementType = field.FieldType.GetGenericArguments()[0];
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
                replacementCount++;
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
}
