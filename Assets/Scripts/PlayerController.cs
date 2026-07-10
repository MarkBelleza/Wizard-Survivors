using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] int playerHealth = 100;
    [SerializeField] Transform respawnPoint;
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] ShootFreeze shootFreeze;

    void takeDamage(int damage)
    {
        playerHealth -= damage;
        if (playerHealth <= 0)
        {
            Debug.Log("Player has been defeated!");
           
            EnemyManager.Instance.DestroyAllEnemies(); // Eliminate all existing enemies when the player is defeated
            enemySpawner.DisableSpawner(); // Disable enemy spawning while the player is respawning
            playerHealth = 100; // Reset health to full

            //Teleport the player to the respawn point while maintaining the same offset from the camera to prevent disorientation
            Transform rig = transform.root;
            Vector3 offset = rig.position - Camera.main.transform.position;
            rig.position = respawnPoint.position + offset;

            //Reset points and spawn rate to the initial state when the player is defeated to give them a fresh start
            //Make sure this occurs AFTER enemySpawner.DisableSpawner(), to prevent points/FURY count UI from resetting to 0
            EnemyManager.Instance.RemovePoints(EnemyManager.Instance.GetCurrentPoints()); // Reset points to 0

        }
    }

    public void OnTriggerEnter(Collider other)
    {
        // Check if the player collided with an enemy, then deal damage to the player and trigger the enemy's explosion effect
        if (other.CompareTag("Enemy"))
        {
            other.GetComponentInParent<EnemyController>().dealDamageToPlayerAndExplode();
            takeDamage(40); 
        }

        // Check if the player collided with Pickup gameobject
        if (other.CompareTag("Pickup"))
        {
            other.GetComponent<Rotator>().PickedUp();
            shootFreeze.SetRandomFireMode();
        }
    }
}
