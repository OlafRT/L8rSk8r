using System.Collections;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Tooltip("The prefab to spawn.")]
    public GameObject prefabToSpawn;

    [Tooltip("The plane GameObject used to determine the spawn area. Must have a Renderer component.")]
    public GameObject plane;

    [Tooltip("Interval in seconds between spawns.")]
    public float spawnInterval = 2f;

    private Renderer planeRenderer;

    void Start()
    {
        if (plane != null)
        {
            planeRenderer = plane.GetComponent<Renderer>();
        }
        else
        {
            Debug.LogError("Plane GameObject is not assigned.");
        }

        // Start the spawning coroutine.
        StartCoroutine(SpawnPrefabRoutine());
    }

    IEnumerator SpawnPrefabRoutine()
    {
        while (true)
        {
            SpawnPrefab();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPrefab()
    {
        if (prefabToSpawn == null || planeRenderer == null)
        {
            return;
        }

        // Get the world-space bounds of the plane.
        Bounds bounds = planeRenderer.bounds;

        // Generate random x and z positions within the bounds.
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        // Set the spawn position. Using the bounds center's y to place it on the plane.
        Vector3 spawnPosition = new Vector3(randomX, bounds.center.y, randomZ);

        // Instantiate the prefab at the random position.
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }
}

