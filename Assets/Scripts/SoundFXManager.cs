using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    [SerializeField] private AudioSource audioSourcePrefab;

    void Awake()
    {
        Instance = this;
    }

    public void PlaySoundFX(AudioClip audioClip, Transform pos, float volume = 1f)
    {
        // Instantiate a new AudioSource at the specified position
        AudioSource audioSource = Instantiate(audioSourcePrefab, pos.position, Quaternion.identity);

        // Set the volume and clip, then play the sound
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        // Destroy the AudioSource after the clip has finished playing to clean up
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFX(AudioClip[] audioClip, Transform pos, float volume = 1f)
    {
        int randomIndex = Random.Range(0, audioClip.Length);

        // Instantiate a new AudioSource at the specified position
        AudioSource audioSource = Instantiate(audioSourcePrefab, pos.position, Quaternion.identity);

        // Set the volume and clip, then play the sound
        audioSource.clip = audioClip[randomIndex];
        audioSource.volume = volume;
        audioSource.Play();

        // Destroy the AudioSource after the clip has finished playing to clean up
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public AudioSource PlayLoopingSoundFX(AudioClip clip, Transform pos, float volume = 1f)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, pos.position, Quaternion.identity);

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true; // enable looping
        audioSource.Play();

        return audioSource; // IMPORTANT: return so we can stop it later
    }

    public void StopLoopingSoundFX(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
            Destroy(source.gameObject);
        }
    }
}
