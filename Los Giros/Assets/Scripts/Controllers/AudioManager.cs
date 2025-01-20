using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---AUDIO SOURCE---")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("---AUDIO CLIP---")]
    [SerializeField] AudioClip background;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip shoot;
    [SerializeField] AudioClip doubleShoot;
    [SerializeField] AudioClip heal;
    [SerializeField] AudioClip dodge;

    public static AudioManager instance;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    /*
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    */
}
