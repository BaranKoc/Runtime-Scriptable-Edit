using UnityEngine;

namespace BaranKoc.RuntimeScriptableEdit.Demo
{
    /// <summary>
    /// Sample tuning data for the demo scene. This is an ordinary ScriptableObject with no
    /// dependency on Runtime Scriptable Edit — that is the point. Any config asset you already
    /// have works the same way; you only need to list it on a RuntimeScriptableEditProfile.
    /// </summary>
    [CreateAssetMenu(fileName = "DemoMovementConfig", menuName = "Runtime Scriptable Edit/Demo/Movement Config")]
    public class DemoMovementConfig : ScriptableObject
    {
        [Header("Horizontal Movement")]
        [Tooltip("Speed in units per second at which the demo mover travels along its patrol path.")]
        [Range(0f, 20f)]
        public float moveSpeed = 4f;

        [Tooltip("Distance in units the mover travels to either side of its starting position.")]
        [Range(0f, 15f)]
        public float patrolDistance = 5f;

        [Header("Bounce")]
        [Tooltip("Peak height in units the mover reaches at the top of each bounce.")]
        [Range(0f, 10f)]
        public float bounceHeight = 2f;

        [Tooltip("Downward acceleration in units per second squared. Higher values give a snappier bounce.")]
        [Range(1f, 60f)]
        public float gravity = 20f;

        [Header("Appearance")]
        [Tooltip("Colour applied to the mover every frame. Changing this in Play Mode is the quickest way to confirm live tuning is working.")]
        public Color bodyColor = new Color(0.25f, 0.70f, 1f);
    }
}
