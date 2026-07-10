using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]private GameObject enemyPrefabs; 

    [SerializeField] private float spawnInterval = 2f; 

    [SerializeField] private Transform playerTransform; // player's position to set as target for enemies

    [SerializeField] private Transform spawnPointTransform; // Optional: assign the point where enemies will spawn. If null, it will use the spawner's position.

    [SerializeField] private float spawnRadius = 9f;

    [Header("Dynamically Change spawn interval the more points the player has")]
    [SerializeField] private float baseSpawnInterval = 2f;
    [SerializeField] private float minimumSpawnInterval = 0.4f;
    [SerializeField] private float difficultyScaling = 500f; //the higher this is, the slower the spawn rate increases with points
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(spawnEnemy(GetSpawnInterval(), enemyPrefabs));
    }

    public bool GetisSpawning()
    {
         return isSpawning;
    }

    public void EnableSpawner()
    {
        //gameObject.SetActive(true);
        isSpawning = true;
    }

    public void DisableSpawner()
    {
        //gameObject.SetActive(false);
        isSpawning = false;
    }

    float GetSpawnInterval()
    {
        int points = EnemyManager.Instance.GetCurrentPoints();

        float interval = baseSpawnInterval - (points / difficultyScaling);

        return Mathf.Clamp(interval, minimumSpawnInterval, baseSpawnInterval);
    }
    private IEnumerator spawnEnemy(float interval, GameObject enemyPref)
    {
        while (true)
        {
            if(isSpawning && EnemyManager.Instance.GetEnemyCount() < 20) // Limit the number of enemies in the scene to prevent overwhelming the player and maintain performance
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


                //Instantiate the enemy and set its target to the player
                GameObject instantiatedEnemy = Instantiate(enemyPref, spawnPos, Quaternion.identity);
                EnemyController controller = instantiatedEnemy.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.player = playerTransform;
                }

            }
            yield return new WaitForSeconds(GetSpawnInterval());
        }

    }

}
