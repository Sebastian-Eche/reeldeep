using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Junchen_Edit
{
    public class HabitatGenerator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Drag in the Background GameObject that contains the DiffusionGrid script")]
        public GameObject background; // Reference to the object holding DiffusionGrid

        [Header("Habitat Prefabs (any zone, scriptable info will sort them)")]
        public List<GameObject> allHabitatPrefabs; // Prefabs for all depth zones

        [Header("Spawn Settings")]
        public int minPerZone = 10; // Minimum habitats per depth layer
        public int maxPerZone = 20; // Maximum habitats per depth layer

        private Dictionary<HabitatInfo.HabitatDepth, List<GameObject>> prefabsByDepth = new();
        private DiffusionGrid diffusionGrid;

        private List<GameObject> spawnedHabitats = new(); // Track spawned habitat instances

        void OnDisable()
        {
            GameManager.Instance.OnBackgroundChange -= RegenerateHabitats;
        }

        // Called when the scene starts. Waits one frame before generating habitats to ensure DiffusionGrid is initialized.
        private void Start()
        {
            GameManager.Instance.OnBackgroundChange += RegenerateHabitats;
            Debug.Log("[HabitatGenerator] Start() called — delaying habitat generation by 1 frame.");
            StartCoroutine(DelayedGenerate());
        }

        // Waits one frame before running GenerateHabitats()
        private IEnumerator DelayedGenerate()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            Debug.Log("[HabitatGenerator] Executing DelayedGenerate()");
            GenerateHabitats(GameManager.Instance.currentBackground);
        }

        // Main entry point for generating habitats
        public void GenerateHabitats(SpriteRenderer sr)
        {
            background = sr.gameObject;
            Debug.Log("SPAWING HABITATS ON: " + background.name);
            // Try to fetch the DiffusionGrid reference
            if (diffusionGrid == null)
            {
                if (background != null)
                    diffusionGrid = background.GetComponent<DiffusionGrid>();
                // else
                    // diffusionGrid = Object.FindFirstObjectByType<DiffusionGrid>();
            }

            // Abort if the grid is missing or uninitialized
            if (diffusionGrid == null || diffusionGrid.grid == null)
            {
                Debug.LogError("[HabitatGenerator] DiffusionGrid is missing or not initialized.");
                return;
            }

            // Group prefabs by their defined depth zone
            CategorizePrefabsByScriptableObject();

            // Determine vertical range for each depth zone based on grid height
            float gridBottom = diffusionGrid.gridOrigin.y;
            float gridTop = gridBottom + diffusionGrid.height;
            float layerHeight = (gridTop - gridBottom) / 3f;

            // Reversed zone order: Shallow is the top third, Deep is the bottom third
            float deepMin = gridBottom;
            float deepMax = gridBottom + layerHeight;

            float midMin = deepMax;
            float midMax = midMin + layerHeight;

            float shallowMin = midMax;
            float shallowMax = gridTop;

            // Generate for each depth zone
            GenerateForDepth(HabitatInfo.HabitatDepth.Shallow, shallowMin, shallowMax);
            GenerateForDepth(HabitatInfo.HabitatDepth.Mid, midMin, midMax);
            GenerateForDepth(HabitatInfo.HabitatDepth.Deep, deepMin, deepMax);
        }

        // Sorts the list of prefabs into depth zones based on their HabitatInfo setting
        private void CategorizePrefabsByScriptableObject()
        {
            prefabsByDepth.Clear();

            foreach (HabitatInfo.HabitatDepth depth in System.Enum.GetValues(typeof(HabitatInfo.HabitatDepth)))
            {
                prefabsByDepth[depth] = new List<GameObject>();
            }

            foreach (var prefab in allHabitatPrefabs)
            {
                var data = prefab.GetComponent<HabitatData>();
                if (data != null && data.habitatInfo != null)
                {
                    var zone = data.habitatInfo.depthZone;
                    prefabsByDepth[zone].Add(prefab);
                }
                else
                {
                    Debug.LogWarning($"[HabitatGenerator] Prefab {prefab.name} missing HabitatData or HabitatInfo!");
                }
            }

            Debug.Log($"[HabitatGenerator] Categorized prefabs — Shallow: {prefabsByDepth[HabitatInfo.HabitatDepth.Shallow].Count}, Mid: {prefabsByDepth[HabitatInfo.HabitatDepth.Mid].Count}, Deep: {prefabsByDepth[HabitatInfo.HabitatDepth.Deep].Count}");
        }

        // Attempts to place a number of habitats randomly within a vertical slice of the grid
        private void GenerateForDepth(HabitatInfo.HabitatDepth depth, float minY, float maxY)
        {
            if (!prefabsByDepth.ContainsKey(depth) || prefabsByDepth[depth].Count == 0)
            {
                Debug.LogWarning($"[HabitatGenerator] No prefabs found for {depth} layer.");
                return;
            }

            int width = diffusionGrid.width;
            int height = diffusionGrid.height;

            int spawnCount = Random.Range(minPerZone, maxPerZone + 1);
            int attempts = 0;
            int maxAttempts = spawnCount * 10;
            int placed = 0;

            HashSet<int> usedX = new(); // Prevent placing on same X axis
            HashSet<int> usedY = new(); // Prevent placing on same Y axis

            while (placed < spawnCount && attempts < maxAttempts)
            {
                attempts++;
                int x = Random.Range(0, width);
                int y = Random.Range(0, height);
                Vector3 worldPos = diffusionGrid.GridToWorld(x, y);

                // Skip if outside depth range
                if (worldPos.y < minY || worldPos.y > maxY)
                    continue;

                // Skip if already occupied
                if (diffusionGrid.obstacles[x, y])
                    continue;

                // Reject if this X or Y is already used (prevent axis-aligned alignment)
                if (usedX.Contains(x) || usedY.Contains(y))
                    continue;

                // Instantiate a random habitat prefab
                GameObject prefab = prefabsByDepth[depth][Random.Range(0, prefabsByDepth[depth].Count)];
                GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity);
                spawnedHabitats.Add(instance); // Track instance

                // Mark this grid cell as occupied
                diffusionGrid.obstacles[x, y] = true;

                // Track used axis values
                usedX.Add(x);
                usedY.Add(y);

                placed++;
            }

            Debug.Log($"[HabitatGenerator] Placed {placed} {depth} habitats.");
        }

        // Clears all previously spawned habitats and resets the obstacle map
        public void ClearHabitats()
        {
            foreach (var obj in spawnedHabitats)
            {
                if (obj != null)
                    Destroy(obj);
            }
            spawnedHabitats.Clear();

            if (diffusionGrid != null && diffusionGrid.obstacles != null)
            {
                int width = diffusionGrid.width;
                int height = diffusionGrid.height;
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        diffusionGrid.obstacles[x, y] = false;
            }

            Debug.Log("[HabitatGenerator] Cleared previous habitats.");
        }

        // Public method to clear and regenerate habitats
        public void RegenerateHabitats(SpriteRenderer sr)
        {
            Debug.Log("REGENERATING HABITATS");
            ClearHabitats();
            StartCoroutine(DelayedGenerate());
        }
    }
}