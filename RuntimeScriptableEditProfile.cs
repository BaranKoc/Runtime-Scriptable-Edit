using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RuntimeScriptableEditProfile", menuName = "Config/Runtime Scriptable Edit Profile")]
public class RuntimeScriptableEditProfile : ScriptableObject
{
    [Header("Tunable Assets")]
    [Tooltip("List of ScriptableObjects that will have runtime copies created for live tuning")]
    public List<ScriptableObject> tunableAssets = new List<ScriptableObject>();

    [Header("Settings")]
    [Tooltip("Automatically re-patch references when new objects are instantiated")]
    public bool autoRepatchOnInstantiate = true;

    public void OnValidate()
    {
        if (tunableAssets == null) return;

        HashSet<ScriptableObject> seen = new HashSet<ScriptableObject>();
        for (int i = tunableAssets.Count - 1; i >= 0; i--)
        {
            if (tunableAssets[i] == null)
            {
                tunableAssets.RemoveAt(i);
                continue;
            }

            if (seen.Contains(tunableAssets[i]))
            {
                Debug.LogWarning($"Duplicate asset found in RuntimeScriptableEditProfile: {tunableAssets[i].name}. Removing duplicate.");
                tunableAssets.RemoveAt(i);
            }
            else
            {
                seen.Add(tunableAssets[i]);
            }
        }
    }
}
