using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Junchen_Edit
{
    public class AdaptiveLightController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Drag in the Background GameObject that contains the DiffusionGrid script")]
        public GameObject background; // Reference to the object holding DiffusionGrid

        [Header("Light Settings")]
        public float shallowIntensity = 1f; // Brightest at top
        public float midIntensity = 0.5f;   // Medium brightness in mid-layer
        public float deepIntensity = 0.2f;  // Darkest at the bottom
        public float transitionSpeed = 3f;  // Controls speed of brightness change

        private Light2D light2D;
        private Transform cameraTransform;

        private float shallowMin, shallowMax;
        private float midMin, midMax;
        private float deepMin, deepMax;

        private enum DepthZone { Shallow, Mid, Deep }
        private DepthZone currentZone;

        private float targetIntensity;
        private bool isTransitioning = false;

        // Called when the scene starts. Waits one frame to ensure DiffusionGrid is initialized
        private void Start()
        {
            light2D = GetComponent<Light2D>();
            cameraTransform = Camera.main.transform;
            StartCoroutine(DelayedInitialize());
        }

        // Waits one frame before running initialization logic
        private System.Collections.IEnumerator DelayedInitialize()
        {
            yield return null;

            if (background == null)
            {
                Debug.LogError("[AdaptiveLightController] Background not assigned!");
                yield break;
            }

            var grid = background.GetComponent<DiffusionGrid>();
            if (grid == null || grid.grid == null)
            {
                Debug.LogError("[AdaptiveLightController] Missing or uninitialized DiffusionGrid.");
                yield break;
            }

            // Match zone logic to HabitatGenerator.cs — Y increases as depth decreases
            float gridBottom = grid.gridOrigin.y;
            float gridTop = gridBottom + grid.height;
            float layerHeight = (gridTop - gridBottom) / 3f;

            deepMin = gridBottom;
            deepMax = gridBottom + layerHeight;

            midMin = deepMax;
            midMax = midMin + layerHeight;

            shallowMin = midMax;
            shallowMax = gridTop;

            // Set initial light intensity based on camera's starting Y position
            float camY = cameraTransform.position.y;
            currentZone = GetZone(camY);
            targetIntensity = GetTargetIntensity(currentZone);
            light2D.intensity = targetIntensity;

            Debug.Log($"[AdaptiveLightController] Init — Camera Y: {camY}, Zone: {currentZone}, Intensity: {targetIntensity}");
        }

        // Called every frame to detect zone changes and transition lighting
        private void Update()
        {
            if (cameraTransform == null)
                return;

            float camY = cameraTransform.position.y;
            DepthZone newZone = GetZone(camY);

            // If camera enters a new zone, update target light intensity
            if (newZone != currentZone)
            {
                currentZone = newZone;
                targetIntensity = GetTargetIntensity(currentZone);
                isTransitioning = true;

                Debug.Log($"[AdaptiveLightController] Transition — Camera Y: {camY}, Zone: {currentZone}, Target Intensity: {targetIntensity}");
            }

            // Gradually interpolate toward the target brightness
            if (isTransitioning)
            {
                light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * transitionSpeed);

                // Stop interpolating once brightness is close enough
                if (Mathf.Abs(light2D.intensity - targetIntensity) < 0.01f)
                {
                    light2D.intensity = targetIntensity;
                    isTransitioning = false;
                }
            }
        }

        // Determines the correct zone based on camera Y position
        private DepthZone GetZone(float y)
        {
            if (y >= shallowMin)
                return DepthZone.Shallow;
            else if (y >= midMin)
                return DepthZone.Mid;
            else
                return DepthZone.Deep;
        }

        // Returns target brightness for a given zone
        private float GetTargetIntensity(DepthZone zone)
        {
            switch (zone)
            {
                case DepthZone.Shallow: return shallowIntensity;
                case DepthZone.Mid:     return midIntensity;
                case DepthZone.Deep:    return deepIntensity;
                default:                return shallowIntensity;
            }
        }
    }
}