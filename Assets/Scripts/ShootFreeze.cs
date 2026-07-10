using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using RangeAttribute = UnityEngine.RangeAttribute;

public class ShootFreeze : MonoBehaviour
{

    [Header("References")]
    [Tooltip("The left controller transform (e.g., from XR Rig).")]
    public Transform controller;
    public Transform controllerLeft;
    public Transform controllerTip;
    public Transform controllerTipLeft;

    public float triggerThreshold = 0.5f; // Adjust this value to set the trigger sensitivity

    [Header("Laser (LineRenderer)")]
    [Tooltip("Optional: assign an existing LineRenderer. If null, one will be created in Awake().")]
    public LineRenderer laser = null;
    public Material laserMaterial;
    public Material laserMaterialLightning;
    public Material laserMaterialFreeze;
    public float laserWidth = 0.01f;
    public float laserMaxDistance = 100f;

    [Tooltip("Layers the laser can hit.")]
    private LayerMask laserMask;
    private LayerMask pickupMask;

    [Header("Firing")]
    [Tooltip("Time (seconds) between allowed shots.")]
    [SerializeField] private float fireCooldown = 0.2f;

    [Tooltip("How long the laser is visible when firing (seconds).")]
    [SerializeField] private float laserPulseDuration = 0.05f;
    [SerializeField] private float defaultDamageAmount = 40f;

    private float nextFireTime = 0f;
    private bool triggerHeldLastFrame = false;
    private bool triggerHeldLastFrameLeft = false;
    [SerializeField] GameObject headshotImpactPrefab;

    // Added as component to controller. turn off world space of line , set beginning and end points (tip of controller)
    private LineRenderer lineRenderer;

    private int fireMode = 1; // 1 shoot, 2 freeze, 3 fireball, 4 lightingChain. toggled by X button on left controller
    [Header("Fireball")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private float fireBallSpeed = 10f;
    [SerializeField] private float fireBallLifetime = 5f;
    [SerializeField] private int fireBallLimit = 4;
    private int currentFireBalls = 0;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 1f;
    [SerializeField] GameObject knockbackImpactPrefab;

    [Header("Freeze")]
    [SerializeField] private float freezeDamage = 25f;
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] GameObject freezeImpactPrefab;
    [SerializeField] GameObject headshotFreezeImpactPrefab;

    [Header("Lighting Chain")]
    [SerializeField] float refreshRate = 0.01f; // How often to update the lightning line renderer positions
    [SerializeField] [Range(1, 10)] int maximumChains = 3; // Maximum number of chained lightning jumps
    [SerializeField] float delayBetweenChain = 0.01f; // Delay between each chain jump
    [SerializeField] EnemyDetector playerEnemyDetector;
    [SerializeField] GameObject lineRendererLightningPrefab;
    [SerializeField] GameObject lightningImpactPrefab;
    [SerializeField] private float lightningDamage = 50f;
    [SerializeField] private float lightningStunDuration = 1.5f;
    [SerializeField] float lightningCooldown = 0.2f;
    bool lightningOnCooldown = false;

    bool lightningChainActive;
    bool shot;
    int counter = 1;
    GameObject curentClosestEnemy;
    List<GameObject> spawnedlightningPrefabs = new List<GameObject>();
    List<GameObject> enemiesInChain = new List<GameObject>();

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] shootSounds;
    [SerializeField] private AudioClip[] freezeSounds;
    [SerializeField] private AudioClip[] fireBallSounds;
    [SerializeField] private AudioClip headshotSound;
    [SerializeField] private AudioClip headshotFreezeSound;
    [SerializeField] private AudioClip lightningChainSound;
    [SerializeField] private AudioClip doomSound;


    private bool triggerActive;
    bool triggerActiveLeft;
    void Awake()
    {
        // Create Laser
        if (laser == null)
        {
            laser = gameObject.AddComponent<LineRenderer>();
            ConfigureLaser(laser);
        }
        else
        {
            // Make sure it's configured for our use
            ConfigureLaser(laser, keepMaterialIfAssigned: true);
        }

        laser.enabled = false;
        // Laser can only hit objects with "Target" Layer Mask
        laserMask = LayerMask.GetMask("Target");
        pickupMask = LayerMask.GetMask("Pickup");
    }

    void Update()
    {
        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch); //Right Trigger
        float sideTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch); //Right side grip
        bool aButton = OVRInput.GetDown(OVRInput.RawButton.A, OVRInput.Controller.RTouch); //A button

        float triggerValueLeft = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch); //Left Trigger
        bool xButton = OVRInput.GetDown(OVRInput.RawButton.X, OVRInput.Controller.LTouch); //X button

        // Fire like a gun: fire once when trigger crosses threshold from up -> down.
        triggerActive = triggerValue > triggerThreshold; 
        triggerActiveLeft = triggerValueLeft > triggerThreshold;

        //Right trigger
        //lightning chain (NEED TO HOLD DOWN TRIGGER)

        if (triggerActive && !triggerHeldLastFrame)
        {
            if (Time.time >= nextFireTime)
            {
                // Single shot
                if (fireMode == 1 || fireMode == 2)
                {
                    StartCoroutine(Fire(controllerTip));
                }
                else if(fireMode == 3 && currentFireBalls < fireBallLimit)
                {
                    // Implement fireball mode
                    FireBall(controllerTip);

                    SoundFXManager.Instance.PlayRandomSoundFX(fireBallSounds, controllerTip, 1f);

                    currentFireBalls++;
                }
                nextFireTime = Time.time + fireCooldown;
            }
        }
        if (fireMode == 4 && triggerActive) //lightning chain mode
        {
            GameObject closest = playerEnemyDetector.GetClosestEnemy();
            if (closest != null && playerEnemyDetector.GetEnemiesInRange().Count > 0)
            {
                if (!lightningChainActive && !lightningOnCooldown)
                {
                    StartLightning(controllerTip);

                }
            }
            else
            {
                StopLightning();
            }
        }
        else
        {
            StopLightning();
        }

        //Left trigger
        if (triggerActiveLeft && !triggerHeldLastFrameLeft)
        {
            if(Time.time >= nextFireTime)
            {
                // Single shot
                if (fireMode == 1 || fireMode == 2)
                {
                    StartCoroutine(Fire(controllerTipLeft));
                }
                else if (fireMode == 3 && currentFireBalls < fireBallLimit)
                {
                    // Implement fireball mode if needed
                    FireBall(controllerTipLeft);
                    currentFireBalls++;
                }
                nextFireTime = Time.time + fireCooldown;
            }
        }

        triggerHeldLastFrame = triggerActive;
        triggerHeldLastFrameLeft = triggerActiveLeft;

        //Change back to FireMode 1
        if (xButton)
        {
            fireMode = 1; // Change to shoot mode
            ConfigureLaser(laser, keepMaterialIfAssigned: true);
            PlayerStatusManager.Instance.DisableMode();
            PlayerStatusManager.Instance.EnableFireMode();
        }

        if(aButton && EnemyManager.Instance.GetCurrentPoints() >= 500)
        {
            DoomAllEnemies();
            EnemyManager.Instance.RemovePoints(500); // Subtract points after using nuke
        }

        //Change FireMode back to 1 (shoot) if fireball limit reached
        if (currentFireBalls >= fireBallLimit)
        {
            fireMode = 1; // Revert to shoot mode if fireball limit reached
            ConfigureLaser(laser, keepMaterialIfAssigned: true);
            currentFireBalls = 0; // Reset fireball count after reaching limit

            PlayerStatusManager.Instance.DisableMode();
            PlayerStatusManager.Instance.EnableFireMode();
        }
    }


    private AudioSource lightningAudioSource;
    void StartLightning(Transform controllerFirePoint)
    {
        lightningOnCooldown = true;
        lightningChainActive = true;
        if(playerEnemyDetector != null && controllerFirePoint != null && lineRendererLightningPrefab != null)
        {
            if (!shot)
            {
                shot = true;

                GameObject firstEnemy = playerEnemyDetector.GetClosestEnemy();
                if (firstEnemy == null) StopLightning();

                if (lightningChainSound != null)
                {
                    lightningAudioSource = SoundFXManager.Instance.PlayLoopingSoundFX(lightningChainSound, controllerFirePoint, 1f);
                }

                NewLightningLineRenderer(controllerFirePoint, firstEnemy.transform, firstEnemy, true); //

                if(maximumChains > 1)
                {
                    StartCoroutine(ChainReaction(firstEnemy));
                }
            }
        }
    }

    void NewLightningLineRenderer(Transform startPos, Transform endPos, GameObject enemy, bool fromPlayer = false) //
    {
        // Lightning Line Renderer
        GameObject lineR = Instantiate(lineRendererLightningPrefab);
        spawnedlightningPrefabs.Add(lineR);

        EnemyController enemyController = enemy.transform.parent.GetComponentInChildren<EnemyController>(); //
        StartCoroutine(UpdateLineRendererLightning(lineR, startPos, endPos, enemyController, fromPlayer)); //

        // Impact Effects
        GameObject impactVFX = Instantiate(lightningImpactPrefab, endPos.position, Quaternion.identity);
        Destroy(impactVFX, 5);
    }


    //Keep displaying line renderer and updating its position until lightningChainActive is false (e.g. player stops shooting or reaches max chains)
    IEnumerator UpdateLineRendererLightning(GameObject lineR, Transform startPos, Transform endPos, EnemyController enemy, bool fromPlayer = false) //
    {
        while (lightningChainActive && lineR != null && enemy != null)
        {
            lineR.GetComponent<LineRendererController>().SetPosition(startPos, endPos);
            enemy.Freeze(lightningDamage, lightningStunDuration); //Deal damage and stun to enemy hit by lightning line renderer

            if (startPos == null || endPos == null || enemy == null)
            {
                StopLightning();
                yield break;
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    //Adds chains of lightning jumps to closest enemy until reaches max chains or player stops shooting. Each jump goes to closest enemy that is not already in the chain.
    IEnumerator ChainReaction (GameObject closestEnemy)
    {
        yield return new WaitForSeconds(delayBetweenChain);

        if (counter >= maximumChains || !lightningChainActive) 
        {
            yield break; // Stop the chain reaction if we've reached the maximum number of chains or if the lightning chain is no longer active
        }

        counter++;
        enemiesInChain.Add(closestEnemy);

        EnemyDetector detector = closestEnemy.GetComponent<EnemyDetector>();

        if (detector == null) 
        {
            yield break;
        }

        GameObject nextEnemy = detector.GetClosestEnemy();

        if (nextEnemy == null || enemiesInChain.Contains(nextEnemy))
        {
            yield break;
        }

        NewLightningLineRenderer(closestEnemy.transform, nextEnemy.transform, nextEnemy, false);
        StartCoroutine(ChainReaction(nextEnemy));
    }
    void StopLightning()
    {
        lightningChainActive = false;
        shot = false;
        counter = 1;

        for (int i = 0; i < spawnedlightningPrefabs.Count; i++)
        {
            Destroy(spawnedlightningPrefabs[i]);
        }

        spawnedlightningPrefabs.Clear();
        enemiesInChain.Clear();

        if (lightningAudioSource != null)
        {
            SoundFXManager.Instance.StopLoopingSoundFX(lightningAudioSource);
            lightningAudioSource = null;
        }

        StartCoroutine(LightningCooldown());
    }

    IEnumerator LightningCooldown()
    {
        lightningOnCooldown = true;
        yield return new WaitForSeconds(lightningCooldown);
        lightningOnCooldown = false;
    }

    private void DoomAllEnemies()
    {
        EnemyManager.Instance.DestroyAllEnemies();
    }

    private void FireBall(Transform tip)
    {
        GameObject fireball = Instantiate(fireBallPrefab, tip.position, Quaternion.identity);
        Destroy(fireball, fireBallLifetime); // Destroy fireball after 5 seconds

        Vector3 velocity = tip.forward.normalized * fireBallSpeed;
        StartCoroutine(MoveKinematicFireball(fireball, velocity, fireBallLifetime));
    }

    private IEnumerator MoveKinematicFireball(GameObject fireball, Vector3 velocity, float lifetime)
    {

        Rigidbody fbRb = fireball.GetComponent<Rigidbody>();
        float elapsed = 0f;

        // Use FixedUpdate steps for physics-consistent movement
        while (elapsed < lifetime && fireball != null)
        {
            // Use physics-friendly step duration
            float dt = Time.fixedDeltaTime;
             fireball.transform.position += velocity * dt;
            elapsed += dt;
            yield return new WaitForFixedUpdate();
        }
    }

    /*
    * Fire a single pulse: show laser briefly, perform a raycast and apply damage/freeze.
    */
    private IEnumerator Fire(Transform controllerT)
    {
        if (controllerT == null || laser == null) yield break;

        Vector3 origin = controllerT.position;
        Vector3 direction = controllerT.forward;
        Vector3 endPosition = origin + (direction * laserMaxDistance);

        // Enable laser and set laser (lineRenderer) end points
        laser.enabled = true;
        laser.SetPosition(0, origin);
        laser.SetPosition(1, endPosition);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, laserMaxDistance, laserMask))
        {
            // If we hit something, shorten the laser to the hit point and apply effects
            laser.SetPosition(1, hit.point);

            EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();

            if (enemy != null)
            { 
                if (fireMode == 1) //Default fire
                {
                    if (hit.collider.CompareTag("headshot")) 
                    {
                        // Play headshot sound effect at the hit location
                        SoundFXManager.Instance.PlaySoundFX(headshotSound, hit.transform, 1f);

                        // Instantiate headshot impact VFX at the hit point
                        GameObject impactVFX = Instantiate(headshotImpactPrefab, hit.point, Quaternion.identity);
                        Destroy(impactVFX, 2);

                        // Apply massive damage for headshot
                        enemy.TakeDamage(150f);

                        yield return new WaitForSeconds(laserPulseDuration);
                        laser.enabled = false;
                    }
                    else
                    {
                        SoundFXManager.Instance.PlayRandomSoundFX(shootSounds, hit.transform, 1f);

                        enemy.TakeDamage(defaultDamageAmount, knockbackForce, knockbackDuration);

                        GameObject impactVFX = Instantiate(knockbackImpactPrefab, hit.point, Quaternion.identity);
                        Destroy(impactVFX, 2);
                    }
                }
                else if (fireMode == 2) //Freeze
                {
                    if (hit.collider.CompareTag("headshot"))
                    {
                        // Play headshot freeze sound effect at the hit location
                        SoundFXManager.Instance.PlaySoundFX(headshotFreezeSound, hit.transform, 1f);

                        // Instantiate headshot freeze impact VFX at the hit point
                        GameObject impactVFX = Instantiate(headshotFreezeImpactPrefab, hit.point, Quaternion.identity);
                        Destroy(impactVFX, 2);

                        // Deal damage and AOE freeze for headshot
                        FreezeSplash(enemy.gameObject);
                        enemy.Freeze(90f, freezeDuration);

                        yield return new WaitForSeconds(laserPulseDuration);
                        laser.enabled = false;
                    }
                    else 
                    {
                        SoundFXManager.Instance.PlayRandomSoundFX(freezeSounds, hit.transform, 1f);

                        enemy.Freeze(freezeDamage, freezeDuration);

                        GameObject impactVFX = Instantiate(freezeImpactPrefab, hit.point, Quaternion.identity);
                        Destroy(impactVFX, 2);
                    }
                }
            }
        }
        else if (Physics.Raycast(origin, direction, out hit, laserMaxDistance, pickupMask)) // Pickup raycast
        {
            // If we hit a pickup, shorten the laser to the hit point and apply effects
            laser.SetPosition(1, hit.point);
            Rotator pickup = hit.collider.GetComponentInParent<Rotator>();

            if (pickup != null)
            {
                pickup.PickedUp(); 
                SetRandomFireMode();
            }
        }
        // Keep the laser visible for a short pulse, then hide it.
        yield return new WaitForSeconds(laserPulseDuration);
        laser.enabled = false;
    }

    public void SetRandomFireMode()
    {
        // Randomly change fireMode just for fun.
        int newMode = Random.Range(1, 5);
        while (newMode == fireMode) // Ensure new mode is different from current mode
        {
            newMode = Random.Range(1, 5);
        }
        fireMode = newMode;
        PlayerStatusManager.Instance.DisableMode();

        // Change laser material based on new fireMode
        if (fireMode == 1 || fireMode == 2)
        {
            ConfigureLaser(laser, keepMaterialIfAssigned: true);

            if (fireMode == 1)
            {
                PlayerStatusManager.Instance.EnableFireMode();
            }
            else
            {
                PlayerStatusManager.Instance.EnableFreezeMode();
            }
        }
        else if (fireMode == 3) // Fireball mode 
        {
            PlayerStatusManager.Instance.EnableFireBallMode();
        }
        else // Lightning mode
        {
            PlayerStatusManager.Instance.EnableLightningMode();
        }
    }

    void FreezeSplash(GameObject enemy)
    {
        FreezeSplashDetector detector = enemy.GetComponentInChildren<FreezeSplashDetector>();
        if (detector != null)
        {
            List<GameObject> enemiesToFreeze = detector.GetEnemiesInRange();
            foreach (GameObject e in enemiesToFreeze)
            {
                EnemyController ec = e.GetComponentInParent<EnemyController>();
                if (ec != null)
                {
                    GameObject impactVFX = Instantiate(freezeImpactPrefab, ec.transform.position, Quaternion.identity);
                    Destroy(impactVFX, 5);

                    ec.Freeze(freezeDamage - 5, freezeDuration);
                }
            }
        }
    }

    // ------------ Laser Setup ------------
    private void ConfigureLaser(LineRenderer lr, bool keepMaterialIfAssigned = false)
    {
        // Set all the line renderer (lr) paramters
        UnityEngine.Debug.Log("Laser Created");
        lr.positionCount = 2; //since we only want a laser coming from our controller tip
        lr.useWorldSpace = true;
        lr.startWidth = laserWidth;
        lr.endWidth = laserWidth;

        lr.alignment = LineAlignment.View;

        lr.textureMode = LineTextureMode.Tile;
        lr.material.mainTextureScale = new Vector2(1, 1);

        // If you made a LineRenderer via the Editor, then need to re-assign the laser material to the new LineRenderer component
        // By default a LineRenderer component is made via the Awake() section of this script
        if (keepMaterialIfAssigned)
        {
            //Change material of laser based on shootFreeze
            if (fireMode == 1)
            {
                lr.material = laserMaterial;
            }
            else if (fireMode == 2)
            {
                lr.material = laserMaterialFreeze;
            }
            else
            {
                lr.material = laserMaterialLightning; // Default material for other modes (e.g., lightning)
            }
        }
    }
}
