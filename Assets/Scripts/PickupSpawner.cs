using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject pickupPrefab;

    [SerializeField]
    private float spawnInterval = 4f;

    [SerializeField]
    private Transform spawnPointTransform; // Optional: assign the point where enemies will spawn. If null, it will use the spawner's position.

    [SerializeField]
    private float spawnRadius = 5f;


    void Start()
    {
        StartCoroutine(spawnPickup(spawnInterval, pickupPrefab));
    }

    private IEnumerator spawnPickup(float interval, GameObject pickupPref)
    {
        while (true)
        {
            if (PickupManager.Instance.GetCurrentPickupCount() < 10)
            {
                // Choose a random offset on the XZ plane within spawnRadius
                Vector3 offset = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
                Vector3 basePosition = spawnPointTransform != null ? spawnPointTransform.position : transform.position;
                Vector3 spawnPos = basePosition + offset;

                //Make sure the spawn position is on the NavMesh
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(spawnPos, out navHit, 2f, NavMesh.AllAreas))
                {
                    spawnPos = navHit.position;
                }

                spawnPos.y += 0.5f; // Raise the spawn position slightly to avoid spawning inside the ground
                //Instantiate the enemy and set its target to the player
                Instantiate(pickupPref, spawnPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
