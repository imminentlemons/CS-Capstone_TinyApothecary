using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NotificationPopup_UI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float displaySeconds = 2.25f;
    [SerializeField, Min(0f)] private float fadeSeconds = 0.25f;

    private static NotificationPopup_UI instance;
    private Coroutine displayRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsurePopupExists()
    {
        if (instance != null)
        {
            return;
        }

        NotificationPopup_UI existing =
            FindFirstObjectByType<NotificationPopup_UI>(
                FindObjectsInactive.Include);

        if (existing != null)
        {
            instance = existing;
            return;
        }

        Canvas targetCanvas = FindHighestSortingCanvas();

        if (targetCanvas == null)
        {
            Debug.LogWarning(
                "Notification popup could not find a Canvas.");
            return;
        }

        CreateDefaultPopup(targetCanvas);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (popupPanel == null)
        {
            popupPanel = gameObject;
        }

        if (messageText == null)
        {
            messageText = GetComponentInChildren<TMP_Text>(true);
        }

        if (canvasGroup == null)
        {
            canvasGroup = popupPanel.GetComponent<CanvasGroup>();
        }

        HideImmediately();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (instance == null)
        {
            EnsurePopupExists();
        }

        if (instance != null)
        {
            instance.ShowMessage(message);
        }
    }

    private void ShowMessage(string message)
    {
        if (popupPanel == null || messageText == null)
        {
            Debug.LogWarning(
                "Notification popup is missing its panel or text reference.");
            return;
        }

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        messageText.text = message;
        popupPanel.SetActive(true);
        popupPanel.transform.SetAsLastSibling();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        displayRoutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        yield return new WaitForSecondsRealtime(displaySeconds);

        if (canvasGroup != null && fadeSeconds > 0f)
        {
            float elapsed = 0f;

            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeSeconds);
                yield return null;
            }
        }

        HideImmediately();
        displayRoutine = null;
    }

    private void HideImmediately()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private static Canvas FindHighestSortingCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        Canvas bestCanvas = null;

        foreach (Canvas canvas in canvases)
        {
            if (!canvas.isRootCanvas)
            {
                continue;
            }

            if (bestCanvas == null ||
                canvas.sortingOrder > bestCanvas.sortingOrder)
            {
                bestCanvas = canvas;
            }
        }

        return bestCanvas;
    }

    private static void CreateDefaultPopup(Canvas canvas)
    {
        GameObject popup = new GameObject(
            "NotificationPopup",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup));

        popup.layer = canvas.gameObject.layer;

        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.SetParent(canvas.transform, false);
        popupRect.anchorMin = new Vector2(0.5f, 1f);
        popupRect.anchorMax = new Vector2(0.5f, 1f);
        popupRect.pivot = new Vector2(0.5f, 1f);
        popupRect.anchoredPosition = new Vector2(0f, -90f);
        popupRect.sizeDelta = new Vector2(760f, 86f);

        Image background = popup.GetComponent<Image>();
        background.color = new Color(0.12f, 0.08f, 0.16f, 0.94f);
        background.raycastTarget = false;

        CanvasGroup group = popup.GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        GameObject textObject = new GameObject(
            "MessageText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        textObject.layer = canvas.gameObject.layer;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(popupRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(28f, 10f);
        textRect.offsetMax = new Vector2(-28f, -10f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 32f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        popup.AddComponent<NotificationPopup_UI>();
    }
}
