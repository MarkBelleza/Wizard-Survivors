using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    public static PlayerStatusManager Instance;
    [SerializeField] Transform doomAuraLoc;
    [SerializeField] Transform controllerTipLeft;
    [SerializeField] Transform controllerTipRight;

    [SerializeField] GameObject doomAuraPrefab;
    [SerializeField] GameObject fireModePrefab;
    [SerializeField] GameObject freezeModePrefab;
    [SerializeField] GameObject lightningModePrefab;
    [SerializeField] GameObject fireBallModePrefab;

    [SerializeField] AudioClip auraActivateSound;

    private GameObject currentModeEffectLeft;
    private GameObject currentModeEffectRight;
    private bool isDoomAuraActive = false;



    void Awake()
    {
        Instance = this;
    }

    public void EnableDoomAura()
    {
        if (!isDoomAuraActive)
        {
            Instantiate(doomAuraPrefab, doomAuraLoc.position, Quaternion.identity, doomAuraLoc);
            isDoomAuraActive = true;

            SoundFXManager.Instance.PlaySoundFX(auraActivateSound, doomAuraLoc, 1f);
        }

    }

    public void DisableDoomAura()
    {
        foreach (Transform child in doomAuraLoc)
        {
            Destroy(child.gameObject);
        }
        isDoomAuraActive = false;
    }

    public void EnableFireMode()
    {
        if (fireModePrefab != null)
        {
            Instantiate(fireModePrefab, controllerTipLeft.position, Quaternion.identity, controllerTipLeft);
            Instantiate(fireModePrefab, controllerTipRight.position, Quaternion.identity, controllerTipRight);
        }
    }

    public void EnableFreezeMode()
    {
        currentModeEffectLeft = Instantiate(freezeModePrefab, controllerTipLeft.position, Quaternion.identity, controllerTipLeft);
        currentModeEffectRight = Instantiate(freezeModePrefab, controllerTipRight.position, Quaternion.identity, controllerTipRight);
    }

    public void EnableLightningMode()
    {
        currentModeEffectRight = Instantiate(lightningModePrefab, controllerTipRight.position, Quaternion.identity, controllerTipRight);
    }

    public void EnableFireBallMode()
    {
        currentModeEffectLeft = Instantiate(fireBallModePrefab, controllerTipLeft.position, Quaternion.identity, controllerTipLeft);
        currentModeEffectRight = Instantiate(fireBallModePrefab, controllerTipRight.position, Quaternion.identity, controllerTipRight);
    }

    public void DisableMode() 
    {
        Destroy(currentModeEffectLeft);
        Destroy(currentModeEffectRight);

        currentModeEffectLeft = null;
        currentModeEffectRight = null;  
    }
}
