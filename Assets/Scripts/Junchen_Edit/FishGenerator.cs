using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Junchen_Edit
{
    public class FishGenerator : MonoBehaviour 
    {
        [Header("Fish Prefabs")]
        public List<GameObject> fishPrefabs; // List of all fish prefabs (manual swimStyle setup, names end with _Left, _Right, or _Random)

        [Header("Spawn Timing")]
        public float minSpawnTime = 1f;
        public float maxSpawnTime = 3f;
        public int minFishCount = 1;
        public int maxFishCount = 5;

        [Header("Camera Settings")]
        public Camera mainCamera;
        public float sideOffsetX = 3f; // Horizontal offset from left/right screen edge
        public float bottomOffsetY = 3f; // Vertical offset below screen bottom
        public float sideYBottomFixedHeight = 0.1f; // Fixed vertical height above camera bottom for side fish generation
        public float bottomYRange = 2f;

        [Header("Spawn Position Constraints")]
        public float minDistanceBetweenFish = 1.2f; // Prevent overlapping fish
        public float fishColliderRadius = 0.6f;
        public int maxAttempts = 10; // Max attempts to find valid position

        [Header("Cleanup Settings")]
        public float cleanupOffsetY = 7f; // Vertical offset above camera after which fish will be destroyed

        private DiffusionGrid diffusionGrid; // Reference to the background diffusion grid

        void Start()
        {
            GameObject bg = GameObject.Find("Background");
            if (bg != null)
            {
                diffusionGrid = bg.GetComponent<DiffusionGrid>();
                if (diffusionGrid != null)
                {
                StartCoroutine(GenerateFishLoop());
                }
                else
                {
                    Debug.LogError("[FishGenerator] Background GameObject found, but DiffusionGrid component is missing.");
                }
            }
            else
            {
                Debug.LogError("[FishGenerator] No GameObject named 'Background' found in the scene.");
            }

            StartCoroutine(GenerateFishLoop());
        }

        void Update()
        {
            CleanupFishAboveScreen();
        }

        IEnumerator GenerateFishLoop()
        {
            while (true)
            {
                if (GameManager.Instance != null && GameManager.Instance.minigameStart)
                {
                    yield return null;
                    continue;
                }

                float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);

                if (GameManager.Instance != null && GameManager.Instance.minigameStart)
                {
                    continue;
                }

                int fishCount = Random.Range(minFishCount, maxFishCount + 1);
                List<Vector3> usedPositions = new List<Vector3>();

                for (int i = 0; i < fishCount; i++)
                {
                    GameObject prefab = fishPrefabs[Random.Range(0, fishPrefabs.Count)];
                    Vector3? spawnPos = GetSpawnPositionFromName(prefab.name, usedPositions);
                    if (spawnPos == null) continue;

                    GameObject fish = Instantiate(prefab);
                    fish.transform.position = spawnPos.Value;

                    SmartFishMovement instanceFm = fish.GetComponent<SmartFishMovement>();
                    instanceFm.diffusionGrid = diffusionGrid;
                    if (instanceFm.swimStyle == SmartFishMovement.SwimStyle.Straight)
                    {
                        Transform wp = fish.transform.GetChild(0);
                        instanceFm.endPoint = wp;
                    }
                    if (instanceFm.head == null)
                        instanceFm.head = fish.transform;

                    usedPositions.Add(spawnPos.Value);

                    Debug.Log($"[FishGenerator] Spawned {fish.name} at {spawnPos.Value}, SwimStyle: {instanceFm.swimStyle}");
                }
            }
        }

        Vector3? GetSpawnPositionFromName(string prefabName, List<Vector3> usedPositions)
        {
            Vector3 camPos = mainCamera.transform.position;
            float camHeight = mainCamera.orthographicSize * 2f;
            float camWidth = camHeight * mainCamera.aspect;

            float camBottom = camPos.y - camHeight / 2f;
            float camLeft = camPos.x - camWidth / 2f;
            float camRight = camPos.x + camWidth / 2f;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 newPos = Vector3.zero;

                if (prefabName.EndsWith("_Left"))
                {
                    // Fish swims right → spawn on far left, at fixed vertical height close to bottom
                    float xJitter = Random.Range(-0.5f, 0.5f);
                    float y = camBottom + sideYBottomFixedHeight;
                    newPos = new Vector3(camLeft - sideOffsetX + xJitter, y, 0f);
                }
                else if (prefabName.EndsWith("_Right"))
                {
                    // Fish swims left → spawn on far right, at fixed vertical height close to bottom
                    float xJitter = Random.Range(-0.5f, 0.5f);
                    float y = camBottom + sideYBottomFixedHeight;
                    newPos = new Vector3(camRight + sideOffsetX + xJitter, y, 0f);
                }
                else if (prefabName.EndsWith("_Random"))
                {
                    float x = Random.Range(camLeft, camRight);
                    float y = camBottom - bottomOffsetY;
                    newPos = new Vector3(x, y, 0f);
                }
                else
                {
                    Debug.LogWarning($"[FishGenerator] Unknown prefab naming convention: {prefabName}");
                    return null;
                }

                bool overlaps = false;
                foreach (Vector3 existing in usedPositions)
                {
                    if (Vector3.Distance(existing, newPos) < minDistanceBetweenFish)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                    return newPos;
            }

            return null;
        }

        void CleanupFishAboveScreen()
        {
            float thresholdY = mainCamera.transform.position.y + mainCamera.orthographicSize + cleanupOffsetY;
            GameObject[] allFish = GameObject.FindGameObjectsWithTag("Fish");
            foreach (GameObject fish in allFish)
            {
                if (fish.transform.position.y > thresholdY)
                {
                    Destroy(fish);
                }
            }
        }
    }
}