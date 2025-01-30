using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private List<AudioClip> audios;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayShotSound()
    {
        if (audios != null && audios.Count > 0)
            audioSource.PlayOneShot(audios[0]);
    }

    public void PlayHealSound()
    {
        if (audios != null && audios.Count > 0)
            audioSource.PlayOneShot(audios[1]);
    }

    public void PlayDodgeSound()
    {
        if (audios != null && audios.Count > 0)
            audioSource.PlayOneShot(audios[2]);
    }

    public void PlayReloadSound()
    {
        if (audios != null && audios.Count > 0)
            audioSource.PlayOneShot(audios[3]);
    }

    public void PlayDoubleShotSound()
    {
        if (audios != null && audios.Count > 0)
            audioSource.PlayOneShot(audios[4]);
    }

    public void PlayRifleSound()
    {
        if (audios != null && audios.Count > 0)
            audioSource.PlayOneShot(audios[5]);
    }

    public void PlayDynamiteSound()
    {
        if (audios != null && audios.Count > 0)
            audioSource.PlayOneShot(audios[6]);
    }
}
