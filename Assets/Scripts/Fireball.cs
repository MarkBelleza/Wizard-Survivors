using UnityEngine;

public class Fireball : MonoBehaviour
{
    private int targetLayer;
    void Awake()
    {
        targetLayer = LayerMask.NameToLayer("Target");
    }

    //allows the object to handle collisions with multiple enemies
    private void OnTriggerEnter(Collider col) // need to enable is Trigger on the fireball's collider for this to work
    {
        if (col.gameObject.layer == targetLayer)
        {
            // The collider may be a child of an enemy (eg. head). Get any EnemyController on the collider or its parents.
            EnemyController enemy = col.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.eliminateEnemy();
            }

        }
    }

}
