using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundEffectSource;

    [Header("Buttons With Custom Sounds")]
    [FormerlySerializedAs("buttonsWithCustomSounds")]
    [SerializeField] private List<Button> pageTurnButtons = new();

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip uiButtonClick;
    [SerializeField] private AudioClip pageTurn;
    [SerializeField] private AudioClip closeUI;
    [SerializeField] private AudioClip dropItem;
    [SerializeField] private AudioClip coinSale;
    [SerializeField] private AudioClip hoeImpact;
    [SerializeField] private AudioClip waterCrop;    
    [SerializeField] private AudioClip potionBrewing;
    [SerializeField] private AudioClip potionComplete;
    [SerializeField] private AudioClip collectPotion;
    [SerializeField] private AudioClip axeSwing;
    [SerializeField] private AudioClip playerDamage;
    [SerializeField] private AudioClip monsterDamage;
    [SerializeField] private AudioClip batDeath;
    [SerializeField] private AudioClip slimeDeath;
    [FormerlySerializedAs("buyItem")]
    [SerializeField] private AudioClip sellBuyItem;
    [SerializeField] private AudioClip dayEndStats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartMusic();
        RegisterButtonSounds();
    }

    private void StartMusic()
    {
        if (musicSource == null || backgroundMusic == null)
        {
            return;
        }

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private void RegisterButtonSounds()
    {
        Button[] buttons = FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            UIButtonPressSound pressSound =
                button.GetComponent<UIButtonPressSound>();

            if(pressSound == null)
            {
                pressSound =
                    button.gameObject.AddComponent<UIButtonPressSound>();
            }

            pressSound.SetPageTurnSound(
                pageTurnButtons.Contains(button));
        }
    }

    public static void PlayUIButtonPress()
    {
        Instance?.PlayClip(Instance.uiButtonClick);
    }

    private void PlayClip(AudioClip clip)
    {
        if (soundEffectSource != null && clip != null)
        {
            soundEffectSource.PlayOneShot(clip);
        }
    }

    public static void PlayPageTurn()
    {
        Instance?.PlayClip(Instance.pageTurn);
    }

    public static void PlayCloseUI()
    {
        Instance?.PlayClip(Instance.closeUI);
    }

    public static void PlayDropItem()
    {
        Instance?.PlayClip(Instance.dropItem);
    }

    public static void PlayHoeImpact()
    {
        Instance?.PlayClip(Instance.hoeImpact);
    }   

    public static void PlayWaterCrop()
    {
        Instance?.PlayClip(Instance.waterCrop);
    }

    public static void PlayPotionBrewing()
    {
        Instance?.PlayClip(Instance.potionBrewing);
    }

    public static void PlayPotionComplete()
    {
        Instance?.PlayClip(Instance.potionComplete);
    }

    public static void PlayCollectPotion()
    {
        Instance?.PlayClip(Instance.collectPotion);
    }

    public static void PlayAxeSwing()
    {
        Instance?.PlayClip(Instance.axeSwing);
    }  

    public static void PlayPlayerDamage()
    {
        Instance?.PlayClip(Instance.playerDamage);
    }

    public static void PlayMonsterDamage()
    {
        Instance?.PlayClip(Instance.monsterDamage);
    }

    public static void PlayBatDeath()
    {
        Instance?.PlayClip(Instance.batDeath);
    }

    public static void PlaySlimeDeath()
    {
        Instance?.PlayClip(Instance.slimeDeath);
    }

    public static void PlaySellBuyItem()
    {
        Instance?.PlayClip(Instance.sellBuyItem);
    }

    public static void PlayCoinSale()
    {
        Instance?.PlayClip(Instance.coinSale);
    }
    public static void PlayDayEndStats()
    {
        Instance?.PlayClip(Instance.dayEndStats);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
