using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] AudioClip pickedUpSound;
    private void Start()
    {
        PickupManager.Instance.RegisterPickup(this);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime); 
        //Time.deltaTime lets us move the object independent from the frame rate: ie. I want to rotate the cube this amount per second, rather than per frame
    }

    public void PickedUp()
    {
        SoundFXManager.Instance.PlaySoundFX(pickedUpSound, transform, 1);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (PickupManager.Instance != null)
        {
            PickupManager.Instance.UnregisterPickup(this);
        }
    }
}
