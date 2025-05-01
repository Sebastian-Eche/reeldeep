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

        // Called when the scene starts. Waits one frame before generating habitats to ensure DiffusionGrid is initialized.
        private void Start()
        {
            Debug.Log("[HabitatGenerator] Start() called — delaying habitat generation by 1 frame.");
            StartCoroutine(DelayedGenerate());
        }

        // Waits one frame before running GenerateHabitats()
        private IEnumerator DelayedGenerate()
        {
            yield return null;
            Debug.Log("[HabitatGenerator] Executing DelayedGenerate()");
            GenerateHabitats();
        }

        // Main entry point for generating habitats
        public void GenerateHabitats()
        {
            // Try to fetch the DiffusionGrid reference
            if (diffusionGrid == null)
            {
                if (background != null)
                    diffusionGrid = background.GetComponent<DiffusionGrid>();
                else
                    diffusionGrid = Object.FindFirstObjectByType<DiffusionGrid>();
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

            float shallowMin = gridBottom;
            float shallowMax = shallowMin + layerHeight;

            float midMin = shallowMax;
            float midMax = midMin + layerHeight;

            float deepMin = midMax;
            float deepMax = gridTop;

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

                // Instantiate a random habitat prefab
                GameObject prefab = prefabsByDepth[depth][Random.Range(0, prefabsByDepth[depth].Count)];
                Instantiate(prefab, worldPos, Quaternion.identity);

                // Mark this grid cell as occupied
                diffusionGrid.obstacles[x, y] = true;

                placed++;
            }

            Debug.Log($"[HabitatGenerator] Placed {placed} {depth} habitats.");
        }
    }
}