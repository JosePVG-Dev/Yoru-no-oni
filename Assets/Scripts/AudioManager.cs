using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    private AudioClip attackSlash;
    private AudioClip attackHit;
    private AudioClip jump;
    private AudioClip dash;
    private AudioClip shrineDamage;
    private AudioClip newWave;
    private AudioClip menuBGM;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        LoadClips();
    }

    private void LoadClips()
    {
        attackSlash = Resources.Load<AudioClip>("Sounds/Attack");
        attackHit = Resources.Load<AudioClip>("Sounds/Attack hit");
        jump = Resources.Load<AudioClip>("Sounds/jump");
        dash = Resources.Load<AudioClip>("Sounds/dash");
        shrineDamage = Resources.Load<AudioClip>("Sounds/Take damage shrine");
        newWave = Resources.Load<AudioClip>("Sounds/new wave");
        menuBGM = Resources.Load<AudioClip>("Sounds/Background menu");
    }

    public void PlayAttackSlash() => PlaySFX(attackSlash);
    public void PlayAttackHit() => PlaySFX(attackHit);
    public void PlayJump() => PlaySFX(jump);
    public void PlayDash() => PlaySFX(dash);
    public void PlayShrineDamage() => PlaySFX(shrineDamage);
    public void PlayNewWave() => PlaySFX(newWave);
    public void PlayMenuBGM() => PlayBGM(menuBGM);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}
