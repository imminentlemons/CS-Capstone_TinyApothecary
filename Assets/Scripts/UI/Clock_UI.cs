using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Clock_UI : MonoBehaviour
{
    [SerializeField] private DayCycleManager dayCycle;
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private TMP_Text phaseText;

    [Header("Phase Background")]
    [SerializeField] private Image phaseBackground;
    [SerializeField] private Sprite preparationBackground;
    [SerializeField] private Sprite openBackground;
    [SerializeField] private Sprite closedBackground;

    private void OnEnable()
    {
        if(dayCycle ==  null)
        {
            return;
        }

        dayCycle.TimeChanged += UpdateClock;
        dayCycle.PhaseChanged += UpdatePhase;

        UpdateClock(dayCycle.CurrentHour, dayCycle.CurrentMinute);
        UpdatePhase(dayCycle.CurrentPhase);
    }

    private void OnDisable()
    {
        if(dayCycle == null)
        {
            return;
        }

        dayCycle.TimeChanged -= UpdateClock;
        dayCycle.PhaseChanged -= UpdatePhase;
    }

    private void UpdateClock(int hour, int minute)
    {
        if(clockText == null)
        {
            return;
        }

        int displayHour = hour % 12;

        if(displayHour == 0)
        {
            displayHour = 12;
        }

        string period = hour < 12 ? "AM" : "PM";

        clockText.text = $"{displayHour}:{minute:00}{period}";
    }

    private void UpdatePhase(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Preparation:
                if (phaseText != null)
                {
                    phaseText.text = "PREP";
                }

                if (phaseBackground != null)
                {
                    phaseBackground.sprite = preparationBackground;
                }
                break;

            case DayPhase.Open:
                if (phaseText != null)
                {
                    phaseText.text = "OPEN";
                }

                if (phaseBackground != null)
                {
                    phaseBackground.sprite = openBackground;
                }
                break;

            case DayPhase.Closed:
                if (phaseText != null)
                {
                    phaseText.text = "CLOSED";
                }

                if (phaseBackground != null)
                {
                    phaseBackground.sprite = closedBackground;
                }
                break;
        }
    }
}
