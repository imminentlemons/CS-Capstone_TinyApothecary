using System.Collections;
using TMPro;
using UnityEngine;

public class ShopFunds_UI : MonoBehaviour
{
    [Header("Funds")]
    [SerializeField] private ShopFunds shopFunds;
    [SerializeField] private TMP_Text moneyAmountText;

    [Header("Sale Feedback")]
    [SerializeField] private GameObject salePopup;
    [SerializeField] private TMP_Text saleAmountText;
    [SerializeField] private CanvasGroup salePopupCanvasGroup;

    [SerializeField] private float visibleDuration = 1f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine feedbackRoutine;

    private void Awake()
    {
        if (salePopupCanvasGroup != null)
        {
            salePopupCanvasGroup.alpha = 0f;
        }

        if (salePopup != null)
        {
            salePopup.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if(shopFunds == null)
        {
            return;
        }

        shopFunds.CoinsChanged += UpdateMoneyDisplay;
        shopFunds.CoinsAdded += ShowSaleFeedback;

        UpdateMoneyDisplay(shopFunds.Coins);
    }

    private void OnDisable()
    {
        if(shopFunds == null)
        {
            return;
        }

        shopFunds.CoinsChanged -= UpdateMoneyDisplay;
        shopFunds.CoinsAdded -= ShowSaleFeedback;
    }

    private void UpdateMoneyDisplay(int totalCoins)
    {
        if(moneyAmountText != null)
        {
            moneyAmountText.text = totalCoins.ToString();
        }
    }

    private void ShowSaleFeedback(int amount)
    {
        if(salePopup == null ||
            saleAmountText == null ||
            salePopupCanvasGroup == null)
        {
            return;
        }

        if(feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        saleAmountText.text = amount.ToString();

        salePopup.SetActive(true);
        salePopupCanvasGroup.alpha = 1f;

        feedbackRoutine = StartCoroutine(FadeSaleFeedback());
    }

    private IEnumerator FadeSaleFeedback()
    {
        yield return new WaitForSecondsRealtime(visibleDuration);

        float elapsed = 0f;

        while(elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            salePopupCanvasGroup.alpha =
                1f - elapsed / fadeDuration;

            yield return null;
        }

        salePopupCanvasGroup.alpha = 0f;
        salePopup.SetActive(false);
        feedbackRoutine = null;
    }
}
