using UnityEngine;
using System.Collections.Generic;

namespace BaranKoc.RuntimeScriptableEdit
{
    [CreateAssetMenu(fileName = "RuntimeScriptableEditProfile", menuName = "Runtime Scriptable Edit/Profile")]
    public class RuntimeScriptableEditProfile : ScriptableObject
    {
        [Header("Tunable Assets")]
        [Tooltip("List of ScriptableObjects that will have runtime copies created for live tuning")]
        public List<ScriptableObject> tunableAssets = new List<ScriptableObject>();

        [Header("Settings")]
        [Tooltip("Automatically re-patch references when new objects are instantiated")]
        public bool autoRepatchOnInstantiate = true;

        // No OnValidate stripping of nulls or duplicates. Unity's list "+" button adds an
        // empty slot on an empty list, and clones the last element on a populated one, so
        // stripping either on validate deleted the new row the moment it appeared and made
        // "+" do nothing. Both cases are handled without side effects where they matter, in
        // RuntimeScriptableEditRegistry.Initialize, which skips them and warns once on entry
        // to play mode.
    }
}
