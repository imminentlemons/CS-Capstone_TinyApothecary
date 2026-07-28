using System;
using UnityEngine;

public enum DayPhase
{
    Preparation,
    Open,
    Closed
}

public class DayCycleManager : MonoBehaviour
{
    [Header("Day Schedule")]
    [SerializeField, Range(0, 23)] private int startingHour = 7;
    [SerializeField, Range(0, 23)] private int openingHour = 9;
    [SerializeField, Range(0, 23)] private int closingHour = 17;
    [SerializeField, Range(0, 23)] private int endingHour= 18;

    [Header("Clock Speed")]
    [Tooltip("How many real seconds it takes for one game minute to pass.")]
    [SerializeField] private float secondsPerGameMinute = 1f;

    public event Action<int, int> TimeChanged;
    public event Action<DayPhase> PhaseChanged;
    public event Action ShopOpened;
    public event Action ShopClosed;
    public event Action DayEnded;

    public int CurrentHour => currentTotalMinutes / 60;
    public int CurrentMinute => currentTotalMinutes % 60;
    public DayPhase CurrentPhase => currentPhase;
    public bool IsShopOpen => currentPhase == DayPhase.Open;
    public bool IsDayRunning => isDayRunning;

    private int currentTotalMinutes;
    private float minuteTimer;
    private DayPhase currentPhase;
    private bool isDayRunning;

    private void Awake()
    {
        currentTotalMinutes = startingHour * 60;
        currentPhase = DeterminePhase();
        isDayRunning = true;
    }

    private void Start()
    {
        TimeChanged?.Invoke(CurrentHour, CurrentMinute);
        PhaseChanged?.Invoke(currentPhase);
    }

    private void Update()
    {
        if(!isDayRunning)
        {
            return;
        }

        minuteTimer += Time.deltaTime;

        float minuteLength = Mathf.Max(0.01f, secondsPerGameMinute);

        while(minuteTimer >= minuteLength && isDayRunning)
        {
            minuteTimer -= minuteLength;
            AdvanceOneMinute();
        }
    }

    private void AdvanceOneMinute()
    {
        currentTotalMinutes++;

        int endingTime = endingHour * 60;

        if(currentTotalMinutes >= endingTime)
        {
            currentTotalMinutes = endingTime;
            isDayRunning = false;
        }

        TimeChanged?.Invoke(CurrentHour, CurrentMinute);

        UpdatePhase();

        if(!isDayRunning)
        {
            DayEnded?.Invoke();
        }
    }

    private void UpdatePhase()
    {
        DayPhase newPhase = DeterminePhase();

        if(newPhase == currentPhase)
        {
            return;
        }

        currentPhase = newPhase;
        PhaseChanged?.Invoke(currentPhase);

        if(currentPhase == DayPhase.Open)
        {
            ShopOpened?.Invoke();
        }
        else if( currentPhase == DayPhase.Closed)
        {
            ShopClosed?.Invoke();
        }
    }

    private DayPhase DeterminePhase()
    {
        int openingTime = openingHour * 60;
        int closingTime = closingHour * 60;

        if(currentTotalMinutes < openingTime)
        {
            return DayPhase.Preparation;
        }

        if(currentTotalMinutes < closingTime)
        {
            return DayPhase.Open;
        }

        return DayPhase.Closed;

    }

}
