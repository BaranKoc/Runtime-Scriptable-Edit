using UnityEngine;

namespace BaranKoc.RuntimeScriptableEdit.Demo
{
    /// <summary>
    /// Drives the demo capsule entirely from <see cref="DemoMovementConfig"/>.
    ///
    /// Every value is read from the config on the frame it is used and never copied into a
    /// field. That is what makes live tuning work: when Play Mode starts, Runtime Scriptable
    /// Edit swaps this component's <c>config</c> reference for a runtime copy, so any edit made
    /// in the editor window takes effect on the very next frame.
    ///
    /// Caching <c>config.moveSpeed</c> into a local field in Awake would silently defeat the
    /// whole system — this class is the worked example of what not to do.
    ///
    /// The mover drives itself rather than reading input, so the demo behaves identically
    /// whether the host project uses the legacy Input Manager, the Input System package, or
    /// both. It has no dependency outside UnityEngine.
    /// </summary>
    [AddComponentMenu("Runtime Scriptable Edit/Demo Mover")]
    public class DemoMover : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        /// <summary>Y position at which the capsule rests on the demo floor.</summary>
        private const float GroundHeight = 1f;

        [Tooltip("Config asset this mover reads every frame. Add this same asset to your RuntimeScriptableEditProfile to tune it during Play Mode.")]
        [SerializeField] private DemoMovementConfig config;

        private Vector3 startPosition;
        private float patrolTime;
        private float verticalVelocity;
        private Renderer bodyRenderer;
        private MaterialPropertyBlock propertyBlock;

        public DemoMovementConfig Config => config;

        private void Awake()
        {
            startPosition = transform.position;
            bodyRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (config == null) return;

            UpdatePatrol();
            UpdateBounce();
            UpdateColor();
        }

        private void UpdatePatrol()
        {
            patrolTime += Time.deltaTime * config.moveSpeed;

            Vector3 position = transform.position;
            position.x = startPosition.x + CalculatePatrolOffset();
            transform.position = position;
        }

        private float CalculatePatrolOffset()
        {
            // Mathf.PingPong divides by its length argument, so a zero patrol distance would
            // produce NaN and throw the capsule out of the scene.
            if (config.patrolDistance <= 0f) return 0f;

            return Mathf.PingPong(patrolTime, config.patrolDistance * 2f) - config.patrolDistance;
        }

        private void UpdateBounce()
        {
            verticalVelocity -= config.gravity * Time.deltaTime;

            Vector3 position = transform.position;
            position.y += verticalVelocity * Time.deltaTime;

            if (position.y <= GroundHeight)
            {
                position.y = GroundHeight;
                verticalVelocity = CalculateBounceVelocity();
            }

            transform.position = position;
        }

        /// <summary>
        /// Launch speed that peaks at exactly <c>bounceHeight</c>, from v = sqrt(2 * g * h).
        /// </summary>
        private float CalculateBounceVelocity()
        {
            float g = Mathf.Max(0f, config.gravity);
            float h = Mathf.Max(0f, config.bounceHeight);

            return Mathf.Sqrt(2f * g * h);
        }

        private void UpdateColor()
        {
            if (bodyRenderer == null) return;

            // Both property names are set so the demo looks right under the Built-in pipeline
            // (_Color) and URP/HDRP (_BaseColor). A MaterialPropertyBlock ignores names the
            // active shader does not declare, so setting both is safe.
            propertyBlock.SetColor(BaseColorId, config.bodyColor);
            propertyBlock.SetColor(ColorId, config.bodyColor);
            bodyRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
