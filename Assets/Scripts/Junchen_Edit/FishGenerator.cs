using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Junchen_Edit
{
    public class FishGenerator : MonoBehaviour
    {
        [Header("Fish Prefabs")]
        public GameObject commonFishPrefab;
        public GameObject uncommonFishPrefab;

        [Header("Spawn Timing")]
        public float minSpawnTime = 1f;
        public float maxSpawnTime = 4f;
        public int minFishCount = 1;
        public int maxFishCount = 3;

        [Header("Camera Follow Settings")]
        public Camera mainCamera;
        public float sideSpawnXOffset = 2f;        // Horizontal distance from camera edge for side spawns
        public float bottomSpawnYOffset = 3f;      // Vertical distance below camera for bottom spawns
        public float sideYMinOffset = 1f;          // Y range for side spawn (bottom part of screen)
        public float bottomYRange = 2f;            // Vertical height of the bottom region

        private enum SpawnRegion
        {
            LeftBottom,
            RightBottom,
            BottomLeft,
            BottomRight
        }

        void Start()
        {
            StartCoroutine(GenerateFishLoop());
        }

        IEnumerator GenerateFishLoop()
        {
            while (true)
            {
                float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);

                int fishCount = Random.Range(minFishCount, maxFishCount + 1);

                for (int i = 0; i < fishCount; i++)
                {
                    SpawnFish();
                }
            }
        }

        void SpawnFish()
        {
            Vector3 camPos = mainCamera.transform.position;
            float camHeight = mainCamera.orthographicSize * 2f;
            float camWidth = camHeight * mainCamera.aspect;

            float camTop = camPos.y + camHeight / 2f;
            float camBottom = camPos.y - camHeight / 2f;
            float camLeft = camPos.x - camWidth / 2f;
            float camRight = camPos.x + camWidth / 2f;

            // Randomly choose a spawn region
            SpawnRegion region = (SpawnRegion)Random.Range(0, 4);

            GameObject prefabToSpawn = null;
            Vector3 spawnPos = Vector3.zero;

            switch (region)
            {
                case SpawnRegion.LeftBottom:
                    // Spawn to the left of the camera, lower part of screen
                    spawnPos = new Vector3(
                        camLeft - sideSpawnXOffset,
                        Random.Range(camBottom, camBottom + sideYMinOffset),
                        0f
                    );
                    prefabToSpawn = commonFishPrefab;
                    break;

                case SpawnRegion.RightBottom:
                    // Spawn to the right of the camera, lower part of screen
                    spawnPos = new Vector3(
                        camRight + sideSpawnXOffset,
                        Random.Range(camBottom, camBottom + sideYMinOffset),
                        0f
                    );
                    prefabToSpawn = uncommonFishPrefab;
                    break;

                case SpawnRegion.BottomLeft:
                    // Spawn below the camera, on left half of screen
                    spawnPos = new Vector3(
                        Random.Range(camLeft, camPos.x),
                        Random.Range(camBottom - bottomSpawnYOffset, camBottom - bottomSpawnYOffset + bottomYRange),
                        0f
                    );
                    prefabToSpawn = commonFishPrefab;
                    break;

                case SpawnRegion.BottomRight:
                    // Spawn below the camera, on right half of screen
                    spawnPos = new Vector3(
                        Random.Range(camPos.x, camRight),
                        Random.Range(camBottom - bottomSpawnYOffset, camBottom - bottomSpawnYOffset + bottomYRange),
                        0f
                    );
                    prefabToSpawn = uncommonFishPrefab;
                    break;
            }

            Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);

            Debug.Log($"[FishGenerator] Spawned {(prefabToSpawn == commonFishPrefab ? "Common" : "Uncommon")} Fish at {spawnPos} in region {region}");
        }
    }
}
