using UnityEngine;

namespace BaranKoc.RuntimeScriptableEdit.Demo
{
    /// <summary>
    /// On-screen instructions and a live readout of the values the mover is reading.
    ///
    /// The readout exists so the effect is provable rather than merely visible: the numbers
    /// shown here come from the same config reference the mover uses, so they change the
    /// instant a slider moves in the Runtime Scriptable Edit window.
    ///
    /// Uses IMGUI to stay free of uGUI, TextMeshPro and any package dependency.
    /// </summary>
    [AddComponentMenu("Runtime Scriptable Edit/Demo HUD")]
    public class DemoHud : MonoBehaviour
    {
        [Tooltip("Mover whose config values are displayed. Leave empty to find one in the scene automatically.")]
        [SerializeField] private DemoMover mover;

        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;

        private void Awake()
        {
            if (mover == null)
            {
                mover = FindAnyObjectByType<DemoMover>();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            GUILayout.BeginArea(new Rect(20f, 20f, 460f, 320f));
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Runtime Scriptable Edit — Demo", headingStyle);
            GUILayout.Space(6f);

            GUILayout.Label(
                "1.  Open  Tools > RuntimeScriptableEdit > Runtime Scriptable Edit Window\n" +
                "2.  Drag any slider below the DemoMovementConfig entry\n" +
                "3.  The capsule reacts on the next frame — no recompile, no restart\n" +
                "4.  Press Perma Save to keep the values when Play Mode ends",
                bodyStyle
            );

            GUILayout.Space(10f);
            DrawLiveValues();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawLiveValues()
        {
            if (mover == null || mover.Config == null)
            {
                GUILayout.Label("No DemoMover with a config assigned was found in the scene.", bodyStyle);
                return;
            }

            DemoMovementConfig config = mover.Config;

            GUILayout.Label("Values being read this frame:", headingStyle);
            GUILayout.Label($"    Asset            {config.name}", bodyStyle);
            GUILayout.Label($"    Move Speed       {config.moveSpeed:0.00}", bodyStyle);
            GUILayout.Label($"    Patrol Distance  {config.patrolDistance:0.00}", bodyStyle);
            GUILayout.Label($"    Bounce Height    {config.bounceHeight:0.00}", bodyStyle);
            GUILayout.Label($"    Gravity          {config.gravity:0.00}", bodyStyle);
        }

        /// <summary>
        /// GUIStyle construction is only legal inside OnGUI, so the styles are built on the
        /// first repaint rather than in Awake.
        /// </summary>
        private void EnsureStyles()
        {
            if (headingStyle != null) return;

            headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                richText = false,
                wordWrap = true
            };
        }
    }
}
