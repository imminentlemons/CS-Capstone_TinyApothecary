using UnityEngine;
using UnityEngine.UI;

public class Satisfaction_UI : MonoBehaviour
{
    [SerializeField] private DailyStats dailyStats;
    [SerializeField] private Image[] stars;

    private void OnEnable()
    {
        if (dailyStats != null)
        {
            dailyStats.StatsChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (dailyStats != null)
        {
            dailyStats.StatsChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        if (dailyStats == null ||
            stars == null)
        {
            return;
        }

        float filledStarAmount =
            dailyStats.AverageSatisfaction *
            stars.Length;

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].fillAmount =
                Mathf.Clamp01(
                    filledStarAmount - i
                );
        }
    }
}