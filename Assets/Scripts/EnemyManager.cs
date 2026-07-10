using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private int currentPoints = 0; // Points awarded for eliminating an enemy
    private List<EnemyController> enemies = new List<EnemyController>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        enemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyController enemy)
    {
        enemies.Remove(enemy);
    }

    public void AddPoints()
    {
        currentPoints += 100;
        if(currentPoints >= 500)
        {
            PlayerStatusManager.Instance.EnableDoomAura();
        }
    }

    public void RemovePoints(int points)
    {
        currentPoints -= points;
        if(currentPoints < 500)
        {
            PlayerStatusManager.Instance.DisableDoomAura();
        }
    }

    public int GetCurrentPoints()
    {
        return currentPoints;
    }

    public int GetEnemyCount()
    {
        return enemies.Count;
    }

    public void DestroyAllEnemies()
    {
        foreach (EnemyController enemy in enemies.ToArray())
        {
            if (enemy != null)
            {
                enemy.DoomedEnemy();
            }

        }
    }
}
