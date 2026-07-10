using System.Collections.Generic;
using UnityEngine;

public class FreezeSplashDetector : MonoBehaviour
{
    List<GameObject> enemiesInFreezeRange = new List<GameObject>();

    void LateUpdate()
    {
        enemiesInFreezeRange.RemoveAll(e => e == null); // Clean up any destroyed enemies from the list
    }

    public List<GameObject> GetEnemiesInRange()
    {
        return enemiesInFreezeRange;
    }

    public void OnTriggerEnter(Collider other) //Add enemies to list when they enter the trigger (in range)
    {
        if (other.CompareTag("FreezeSplash"))
        {
            if (enemiesInFreezeRange.Count == 0)
            {
                enemiesInFreezeRange.Add(other.gameObject);
            }
            else if (!enemiesInFreezeRange.Contains(other.gameObject))
            {
                enemiesInFreezeRange.Add(other.gameObject);
            }
        }
    }

    public void OnTriggerExit(Collider other) //Remove enemies from list when they exit the trigger (out of range)
    {
        if (other.CompareTag("FreezeSplash"))
        {
            if (enemiesInFreezeRange.Count > 0)
            {
                enemiesInFreezeRange.Remove(other.gameObject);
            }
        }
    }
}
