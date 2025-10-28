using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---Audio Source---")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("---Audio Clip---")]
    public AudioClip swap;
    public AudioClip disappear;

    public void PlaySwapSFX()
    {
        SFXSource.PlayOneShot(swap);
    }

    public void PlayDisappearSFX()
    {
        SFXSource.PlayOneShot(disappear);
    }
}
