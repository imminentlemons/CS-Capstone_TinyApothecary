using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NotificationPopup_UI : MonoBehaviour
{
    [Header("Player 1")]
    [SerializeField] private GameObject player1Panel;
    [SerializeField] private TMP_Text player1Text;
    [SerializeField] private CanvasGroup player1CanvasGroup;

    [Header("Player 2")]
    [SerializeField] private GameObject player2Panel;
    [SerializeField] private TMP_Text player2Text;
    [SerializeField] private CanvasGroup player2CanvasGroup;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float displaySeconds = 2.25f;
    [SerializeField, Min(0f)] private float fadeSeconds = 0.25f;

    private static NotificationPopup_UI instance;
    private Coroutine player1Routine;
    private Coroutine player2Routine;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        PreparePanel(player1Panel, ref player1Text, ref player1CanvasGroup);

        PreparePanel(player2Panel, ref player2Text, ref player2CanvasGroup);        
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void PreparePanel( GameObject panel, ref TMP_Text text, ref CanvasGroup group)
    {
        if(panel == null)
        {
            return;
        }

        if(text == null)
        {
            text = panel.GetComponentInChildren<TMP_Text>(true);
        }

        if(group == null)
        {
            group = panel.GetComponentInChildren<CanvasGroup>();
        }

        HidePanel(panel, group);
    }

    public static void Show(Player player, string message)
    {
        if(player == null)
        {
            Show(message);
            return;
        }

        if(!FindManager() || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        bool isPlayer1 = player.notificationSide == Player.PlayerSide.Left;

        instance.ShowMessage(isPlayer1, message);
    }

    public static void Show(string message)
    {
        if(!FindManager() || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        instance.ShowMessage(true, message);
    }

    private static bool FindManager()
    {
        if(instance == null)
        {
            instance = FindFirstObjectByType<NotificationPopup_UI>(
                FindObjectsInactive.Include);
        }

        if(instance == null)
        {
            Debug.LogWarning("No notification popup manager was found in scene");
            return false;
        }

        return true;
    }

    private void ShowMessage(bool isPlayer1, string message)
    {
        GameObject panel = isPlayer1 ? player1Panel : player2Panel;

        TMP_Text text = isPlayer1 ? player1Text : player2Text;

        CanvasGroup group = isPlayer1 ? player1CanvasGroup : player2CanvasGroup;

        if(panel == null || text == null)
        {
            Debug.LogWarning("a notification panel or text is missing");

            return;
        }

        if(isPlayer1 && player1Routine != null)
        {
            StopCoroutine(player1Routine);
        }
        else if(!isPlayer1 && player2Routine != null)
        {
            StopCoroutine(player2Routine);
        }

        text.text = message;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        if(group != null)
        {
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        Coroutine routine = StartCoroutine(
            DisplayRoutine(panel, group, isPlayer1));

        if(isPlayer1)
        {
            player1Routine = routine;
        }
        else
        {
            player2Routine = routine;
        }
    }

    private IEnumerator DisplayRoutine(GameObject panel, CanvasGroup group, bool isPlayer1)
    {
        yield return new WaitForSeconds(displaySeconds);

        if (group != null && fadeSeconds > 0f)
        {
            float elapsed = 0f;

            while(elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;

                group.alpha = 1f - Mathf.Clamp01(elapsed / fadeSeconds);

                yield return null;
            }
        }

        HidePanel(panel, group);

        if(isPlayer1)
        {
            player1Routine = null;
        }
        else
        {
            player2Routine = null;
        }
    }

    private void HidePanel(GameObject panel, CanvasGroup group)
    {
        if(group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        if(panel != null)
        {
            panel.SetActive(false);
        }
    } 
}
