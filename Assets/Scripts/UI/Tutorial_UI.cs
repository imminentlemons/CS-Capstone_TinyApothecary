using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tutorial_UI : MonoBehaviour
{
    [Serializable]
    private class TutorialPage
    {
        public string title;

        [TextArea(3, 6)]
        public string message;

        [Header("Optional Arrow Targets")]
        public Transform firstTarget;
        public Camera firstTargetCamera;

        public Transform secondTarget;
        public Camera secondTargetCamera;
    }

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text pageText;

    [Header("Arrows")]
    [SerializeField] private RectTransform arrowOne;
    [SerializeField] private RectTransform arrowTwo;
    [SerializeField] private Vector2 arrowOffset = new Vector2(0f, 70f);
       

    [Header("Tutorial Pages")]
    [SerializeField] private List<TutorialPage> pages = new();

    private int currentPage;

    private void OnEnable()
    {
        Begin();
    }

    public void Begin()
    {
        currentPage = 0;
        ShowCurrentPage();
    }

    // returns true if another tutorial page was shown
    // returns false when the tutorial is finished
    public bool Advance()
    {
        if (pages.Count == 0 || currentPage >= pages.Count - 1)
        {
            return false;
        }

        currentPage++;
        ShowCurrentPage();
        return true;
    }

    private void ShowCurrentPage()
    {
        if (pages.Count == 0)
        {
            if (titleText != null)
            {
                titleText.text = "Tutorial";
            }

            if (messageText != null)
            {
                messageText.text = "No tutorial pages have been assigned.";
            }

            if (pageText != null)
            {
                pageText.text = "";
            }

            HideArrow(arrowOne);
            HideArrow(arrowTwo);
            return;
        }

        TutorialPage page = pages[currentPage];

        if (titleText != null)
        {
            titleText.text = page.title;
        }

        if (messageText != null)
        {
            messageText.text = page.message;
        }

        if (pageText != null)
        {
            pageText.text = $"{currentPage + 1} / {pages.Count}";
        }

        UpdateArrow(arrowOne, page.firstTarget, page.firstTargetCamera);

        UpdateArrow(arrowTwo, page.secondTarget, page.secondTargetCamera);
    }

    private void LateUpdate()
    {
        if (pages.Count == 0)
        {
            return;
        }

        TutorialPage page = pages[currentPage];

        UpdateArrow(arrowOne, page.firstTarget, page.firstTargetCamera);

        UpdateArrow(arrowTwo, page.secondTarget, page.secondTargetCamera);
    }

    private void UpdateArrow(RectTransform arrow, Transform target, Camera targetCamera)
    {
        if (arrow == null)
        {
            return;
        }

        RectTransform arrowParent =
            arrow.parent as RectTransform;

        if (target == null ||
           targetCamera == null ||
           arrowParent == null)
        {
            HideArrow(arrow);
            return;
        }

        Vector3 viewportPosition =
            targetCamera.WorldToViewportPoint(target.position);

        bool targetIsVisible =
            viewportPosition.z > 0f &&
            viewportPosition.x >= 0f &&
            viewportPosition.x <= 1f &&
            viewportPosition.y >= 0f &&
            viewportPosition.y <= 1f;

        if (!targetIsVisible)
        {
            HideArrow(arrow);
            return;
        }

        Vector3 screenPosition =
            targetCamera.WorldToScreenPoint(target.position);

        Canvas canvas = arrow.GetComponentInParent<Canvas>();
        Camera uiCamera = null;

        if (canvas != null &&
           canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.rootCanvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            arrowParent,
            screenPosition,
            uiCamera,
            out Vector2 localPosition))
        {
            arrow.gameObject.SetActive(true);
            arrow.anchoredPosition = localPosition + arrowOffset;
        }
        else
        {
            HideArrow(arrow);
        }
    }

    private void HideArrow(RectTransform arrow)
    {
        if (arrow != null)
        {
            arrow.gameObject.SetActive(false);
        }
    }
}