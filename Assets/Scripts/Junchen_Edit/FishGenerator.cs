using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Junchen_Edit
{
    public class FishGenerator : MonoBehaviour
    {
        [Header("References")]
        public Camera mainCamera;               // Reference to the camera used for position
        public Transform background;            // Background used to determine depth zones

        [Header("Fish Prefabs")]
        public GameObject commonFishPrefab;     // Prefab for common fish
        public GameObject uncommonFishPrefab;   // Prefab for uncommon fish

        [Header("Generation Settings")]
        public float minSpawnTime = 1f;         // Minimum wait time between spawns (seconds)
        public float maxSpawnTime = 5f;         // Maximum wait time between spawns (seconds)
        public int minFishCount = 1;            // Minimum number of fish per batch
        public int maxFishCount = 2;            // Maximum number of fish per batch
        public int spawnSegments = 3;           // Number of horizontal subdivisions per side
        public float verticalSpawnRange = 3f;   // Y-axis randomness per fish spawn

        private float backgroundTopY;
        private float backgroundBottomY;

        void Start()
        {
            // Calculate the top and bottom Y of the background based on its center + scale
            float bgHeight = background.localScale.y;
            backgroundTopY = background.position.y + (bgHeight / 2f);
            backgroundBottomY = background.position.y - (bgHeight / 2f);

            // Start the timed generation loop
            StartCoroutine(GenerateFishLoop());
        }

        IEnumerator GenerateFishLoop()
        {
            while (true)
            {
                // Wait for a random interval before spawning
                float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);

                // Decide how many fish to spawn this time
                int fishCount = Random.Range(minFishCount, maxFishCount + 1);
                GenerateFishBatch(fishCount);
            }
        }

        void GenerateFishBatch(int count)
        {
            // Cache camera position and screen width
            Vector3 camPos = mainCamera.transform.position;
            float camY = camPos.y;
            float camX = camPos.x;
            float camWidth = mainCamera.orthographicSize * mainCamera.aspect;

            // Split background into 3 vertical zones: shallow, mid, deep
            float regionHeight = (backgroundTopY - backgroundBottomY) / 3f;

            string zone = "";
            GameObject prefabToUse = null;

            // Determine current depth zone and which fish to spawn
            if (camY > backgroundTopY - regionHeight)
            {
                zone = "Shallow";
                prefabToUse = commonFishPrefab;
            }
            else if (camY > backgroundTopY - regionHeight * 2)
            {
                zone = "Mid";
                prefabToUse = (Random.value < 0.5f) ? commonFishPrefab : uncommonFishPrefab;
            }
            else
            {
                zone = "Deep";
                prefabToUse = uncommonFishPrefab;
            }

            // Determine spawn side and range
            bool isCommon = (prefabToUse == commonFishPrefab);
            float xStart = isCommon ? camX - camWidth : camX;
            float xEnd = isCommon ? camX : camX + camWidth;

            // Divide the selected side into horizontal segments to avoid overlap
            List<float> possibleXPositions = new List<float>();
            float segmentWidth = (xEnd - xStart) / spawnSegments;

            for (int i = 0; i < spawnSegments; i++)
            {
                float segmentMin = xStart + i * segmentWidth;
                float segmentMax = segmentMin + segmentWidth;
                float xPos = Random.Range(segmentMin, segmentMax);
                possibleXPositions.Add(xPos);
            }

            // Shuffle segment positions to randomize
            Shuffle(possibleXPositions);

            // Spawn up to 'count' fish in different positions
            for (int i = 0; i < Mathf.Min(count, possibleXPositions.Count); i++)
            {
                float spawnX = possibleXPositions[i];
                float spawnY = Random.Range(camY - verticalSpawnRange, camY + verticalSpawnRange);
                Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

                Instantiate(prefabToUse, spawnPos, prefabToUse.transform.rotation);

                Debug.Log($"[FishGenerator] Spawned {(isCommon ? "Common" : "Uncommon")} Fish in {zone} zone at {spawnPos}");
            }
        }

        // Fisher-Yates shuffle for randomizing a list
        void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
