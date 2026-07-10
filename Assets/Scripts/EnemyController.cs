using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class EnemyController : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent navMeshAgent;
    private bool isFrozen = false;
    private float health = 100;
    private Rigidbody rb;
    [SerializeField] GameObject destroyedImpactPrefab;
    [SerializeField] GameObject damagePlayerImpactPrefab;
    [SerializeField] AudioClip damagePlayerSound;
    [SerializeField] AudioClip destroyedSound;

    [SerializeField] GameObject[] doomedImpactPrefabs; // Array of different impact VFX for variety when the enemy is destroyed
    [SerializeField] AudioClip[] doomedDestroyedSounds;

    private Coroutine knockbackCoroutine;
    private Animator anim;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        EnemyManager.Instance.RegisterEnemy(this); //Keeps track of this enemy in the EnemyManager for global management (like mass elimination)
    }

    // Update is called once per frame
    void Update()
    {
        // Do not set a destination while frozen or when taking damage 
        if (player != null && navMeshAgent != null && !isFrozen && knockbackCoroutine == null)
        {
            navMeshAgent.SetDestination(player.position);

            float speed = navMeshAgent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
       
    }

    public bool TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            knockbackCoroutine = null;
            eliminateEnemy();
            EnemyManager.Instance.AddPoints(); // Award points for eliminating this enemy
            return true;
        }
        return false;
    }

    public void TakeDamage(float damage, float knockbackForce, float knockbackDuration)
    {
        if (TakeDamage(damage)) return; // Enemy is already eliminated, no need to apply knockback

        // Compute knockback direction away from the player (where the damage came from)
        Vector3 direction = (transform.position - player.position);

        // Start knockback coroutine (ensure only one runs at a time)
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction.normalized, knockbackForce, knockbackDuration));

    }
    public void dealDamageToPlayerAndExplode()
    {
        SoundFXManager.Instance.PlaySoundFX(damagePlayerSound, transform, 1f);

        GameObject impactVFX = Instantiate(damagePlayerImpactPrefab, transform.position, Quaternion.identity);
        Destroy(impactVFX, 2);
        Destroy(gameObject);

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this); // Unregister from EnemyManager to ensure accurate tracking
        }
    }
    public void eliminateEnemy()
    {
        GameObject impactVFX = Instantiate(destroyedImpactPrefab, transform.position, Quaternion.identity);

        SoundFXManager.Instance.PlaySoundFX(destroyedSound, transform, 1f);

        Destroy(impactVFX, 2);
        Destroy(gameObject);
       
        if (EnemyManager.Instance != null) 
        { 
            EnemyManager.Instance.UnregisterEnemy(this); // Unregister from EnemyManager to ensure accurate tracking
        }
    }

    public void DoomedEnemy()
    {
        // Lightning strick effect
        int randomIndex = Random.Range(0, doomedImpactPrefabs.Length);
        GameObject impactVFX = Instantiate(doomedImpactPrefabs[randomIndex], transform.position, Quaternion.identity);

        // Sound effect
        SoundFXManager.Instance.PlayRandomSoundFX(doomedDestroyedSounds, transform, 1f);

        Destroy(impactVFX, 2);
        Destroy(gameObject);

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this); // Unregister from EnemyManager to ensure accurate tracking
        }
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
    {
        // Ensure we are not considered frozen while being knocked back
        bool previousFrozen = isFrozen;
        isFrozen = false;

        //Disable NavMesh agent to allow physics-based knockback without interference
        navMeshAgent.enabled = false;
        rb.isKinematic = false; // Make rigidbody non-kinematic to respond to physics

        direction.y = 0f; // Keep knockback on the horizontal plane
        direction = direction.normalized; // Ensure consistent knockback force regardless of distance

        // If Rigidbody present, apply an impulse and wait
        rb.AddForce(direction * force, ForceMode.VelocityChange);
        yield return new WaitForSeconds(duration);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Clear coroutine marker and restore frozen state if it was frozen before
        knockbackCoroutine = null;
        isFrozen = previousFrozen;

        navMeshAgent.enabled = true;
        navMeshAgent.Warp(transform.position);
        navMeshAgent.SetDestination(player.position); // Makes sure the agent resumes pathfinding immediately after knockback
    }

    public void Freeze(float damage, float duration)
    {
        if (TakeDamage(damage)) return; // Enemy is already eliminated, no need to apply freeze

        if (!isFrozen)
        {
            StartCoroutine(FreezeRoutine(duration));
        }
    }
    private IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;

        if (navMeshAgent != null)
        {
            // Stop navigation immediately and clear the current path so the agent doesn't keep moving.
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();

            // Temporarily set speed to zero to avoid physics/velocity residual movement.
            float savedSpeed = navMeshAgent.speed;
            navMeshAgent.speed = 0f;
            anim.SetFloat("Speed", navMeshAgent.speed);

            yield return new WaitForSeconds(duration);

            // Restore agent
            navMeshAgent.speed = savedSpeed;
            navMeshAgent.isStopped = false;
            float speed = navMeshAgent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
        else
        {
            // Fallback wait if there's no NavMeshAgent
            yield return new WaitForSeconds(duration);
        }

        isFrozen = false;
    }
}
