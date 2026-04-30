using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音效配置")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _kickSound;
    [SerializeField] private AudioClip _coinSound;
    [SerializeField] private AudioClip _brickSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayKickSound()
    {
        if (_audioSource != null && _kickSound != null)
        {
            _audioSource.PlayOneShot(_kickSound);
        }
    }

    public void PlayCoinSound()
    {
        if (_audioSource != null && _coinSound != null)
        {
            _audioSource.PlayOneShot(_coinSound);
        }
    }

    public void PlayBrickSound()
    {
        if (_audioSource != null && _brickSound != null)
        {
            _audioSource.PlayOneShot(_brickSound);
        }
    }
}