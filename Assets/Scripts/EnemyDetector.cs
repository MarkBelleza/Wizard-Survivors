using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    List<GameObject> enemiesInRange = new List<GameObject>();

    void LateUpdate()
    {
        enemiesInRange.RemoveAll(e => e == null); // Clean up any destroyed enemies from the list
    }

    public GameObject GetClosestEnemy()
    {
        if (enemiesInRange.Count == 0)
        {
            return null; // No enemies in range
        }

        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject enemy in enemiesInRange)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }


    public List<GameObject> GetEnemiesInRange()
    {
        return enemiesInRange;
    }

    public void OnTriggerEnter(Collider other) //Add enemies to list when they enter the trigger (in range)
    {
        if (other.CompareTag("LightningTarget"))
        {
            if(enemiesInRange.Count == 0)
            {
                enemiesInRange.Add(other.gameObject);
            }
            else if (!enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Add(other.gameObject);
            }
        }
    }

    public void OnTriggerExit(Collider other) //Remove enemies from list when they exit the trigger (out of range)
    {
        if (other.CompareTag("LightningTarget"))
        {
            if (enemiesInRange.Count > 0)
            {
                enemiesInRange.Remove(other.gameObject);
            }
        }
    }
}
