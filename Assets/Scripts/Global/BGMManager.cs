using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource audioSource;

    [Header("Battle BGM Clips")]
    public AudioClip battleStartClip;
    public AudioClip inBattleLoopClip;

    [Header("End Game Clips")]
    public AudioClip winClip;
    public AudioClip loseClip;

    private bool isBattleStarted = false;
    private bool isPlayingBattleStart = false;

    private const float MinVolume = 0f;
    private const float MaxVolume = 0.2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Ensure nothing plays on start
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        // Wait for battle start clip to finish, then transition to looped battle music
        if (isPlayingBattleStart && !audioSource.isPlaying)
        {
            isPlayingBattleStart = false;
            PlayInBattleLoop();
        }
    }

    public void OnBattleStarted()
    {
        if (isBattleStarted) return;

        isBattleStarted = true;
        PlayBattleStart();
    }

    private void PlayBattleStart()
    {
        if (battleStartClip != null)
        {
            audioSource.clip = battleStartClip;
            audioSource.loop = false;
            audioSource.Play();
            isPlayingBattleStart = true;
        }
        else
        {
            // If no start clip, jump to in-battle loop
            PlayInBattleLoop();
        }
    }

    private void PlayInBattleLoop()
    {
        if (inBattleLoopClip != null)
        {
            audioSource.clip = inBattleLoopClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PlayWinBGM()
    {
        PlayEndClip(winClip);
    }

    public void PlayLoseBGM()
    {
        PlayEndClip(loseClip);
    }

    private void PlayEndClip(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.loop = false;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }


    #region Volume Control
    public void SetVolume(float value)
    {
        audioSource.volume = Mathf.Clamp(value, MinVolume, MaxVolume);
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }

    public void Mute()
    {
        audioSource.volume = 0f;
    }

    public void Unmute()
    {
        audioSource.volume = MaxVolume;
    }
    #endregion
}
