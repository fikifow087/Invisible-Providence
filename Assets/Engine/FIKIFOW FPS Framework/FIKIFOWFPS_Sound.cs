using UnityEngine;

[AddComponentMenu("FIKIFOW FPS/ - Sound")]
[RequireComponent(typeof(AudioSource))]
public class FIKIFOWFPS_Sound : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip shootClip;
    public AudioClip reloadClip;
    public AudioClip emptyAmmoClip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayShootSound()
    {
        if (shootClip != null) audioSource.PlayOneShot(shootClip);
    }

    public void PlayReloadSound()
    {
        if (reloadClip != null) audioSource.PlayOneShot(reloadClip);
    }

    public void PlayEmptySound()
    {
        if (emptyAmmoClip != null) audioSource.PlayOneShot(emptyAmmoClip);
    }
}