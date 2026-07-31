using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameFlow_UI : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject controlScreen;
    [SerializeField] private GameObject endOfDayScreen;
    [SerializeField] private GameObject exitConfirmation;

    [Header("GameSystems")]
    [SerializeField] private DayCycleManager dayCycle;
    [SerializeField] private DailyStats dailyStats;

    [Header("End-Of-Day Values")]
    [SerializeField] private TMP_Text customerServedText;
    [SerializeField] private TMP_Text orderssFailedText;
    [SerializeField] private TMP_Text moneyEarnedText;
    [SerializeField] private TMP_Text monstersKilledText;
    [SerializeField] private TMP_Text satisfactionText;

    private enum FlowState
    {
        Title,
        Controls, 
        Playing,
        EndOfDay
    }

    private FlowState state;
    private int stateChangedFrame;
    private static bool startImmediatelyAfterReload;

    private void Awake()
    {
        if(exitConfirmation != null)
        {
            exitConfirmation.SetActive(false);
        }

        if(startImmediatelyAfterReload)
        {
            startImmediatelyAfterReload = false;
            StartGame();
        }
        else
        {
            ShowTitle();
        }
    }

    private void OnEnable()
    {
        if(dayCycle != null)
        {
            dayCycle.DayEnded += ShowEndOfDay;
        }
    }

    private void Update()
    {
        //prevent one press from advancing through two screens
        if(Time.frameCount == stateChangedFrame)
        {
            return;
        }

        //confirmation popup gets input priority
        if(exitConfirmation != null && exitConfirmation.activeSelf)
        {
            bool gamepadYes =
                Gamepad.current != null &&
                Gamepad.current.buttonSouth.wasPressedThisFrame;

            bool cancel =
                (Gamepad.current != null &&
                Gamepad.current.buttonEast.wasPressedThisFrame) ||
                (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame);

            if(gamepadYes)
            {
                QuitToDesktop();
            }
            else if(cancel)
            {
                CloseExitConfirmation();
            }

            return;
        }

        if(BackPressed())
        {
            if(state == FlowState.Controls)
            {
                ShowTitle();
            }
            else if(state == FlowState.Title)
            {
                OpenExitConfirmation();
            }

            return;
        }

        if(!ConfirmPressed())
        {
            return;
        }

        switch(state)
        {
            case FlowState.Title:
                ShowControls();
                break;

            case FlowState.Controls:
                StartGame();
                break;

            case FlowState.EndOfDay:
                PlayAgain();
                break;
        }
    }

    public void OpenExitConfirmation()
    {
        if(exitConfirmation == null)
        {
            return;
        }

        exitConfirmation.SetActive(true);
        exitConfirmation.transform.SetAsLastSibling();
        stateChangedFrame = Time.frameCount;
    }

    public void CloseExitConfirmation()
    {
        if(exitConfirmation != null)
        {
            exitConfirmation.SetActive(false);
        }

        stateChangedFrame = Time.frameCount;
    }

    public void QuitToDesktop()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void ExitToTitle()
    {
        startImmediatelyAfterReload = false;
        ReloadScene();
    }

    public void ShowTitle()
    {
        ApplyState(FlowState.Title);
    }

    public void ShowControls()
    {
        ApplyState(FlowState.Controls);
    }

    public void StartGame()
    {
        ApplyState(FlowState.Playing);
    }

    public void ShowEndOfDay()
    {
        if (dailyStats != null)
        {
            customerServedText.text =
                dailyStats.CustomersServed.ToString();

            orderssFailedText.text =
                dailyStats.CustomersTimedOut.ToString();

            moneyEarnedText.text = 
                dailyStats.MoneyEarnedToday.ToString();

            monstersKilledText.text = 
                dailyStats.EnemiesDefeated.ToString();

            satisfactionText.text =
                $"{dailyStats.SatisfactionPercentage:0}%";
        }

        ApplyState(FlowState.EndOfDay);
    }

    public void PlayAgain()
    {
        startImmediatelyAfterReload = true;
        ReloadScene();
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ApplyState(FlowState newState)
    {
        if(exitConfirmation != null)
        {

            exitConfirmation.SetActive(false);
        }

        state = newState;
        stateChangedFrame = Time.frameCount;

        titleScreen.SetActive(state == FlowState.Title);
        controlScreen.SetActive(state == FlowState.Controls);
        endOfDayScreen.SetActive(state == FlowState.EndOfDay);

        bool gameplayActive = state == FlowState.Playing;

        SetGameplayInputEnabled(gameplayActive);
        Time.timeScale = gameplayActive ? 1f : 0f;
    }

    private void SetGameplayInputEnabled(bool enabled)
    {
        foreach(PlayerInput input in FindObjectsByType<PlayerInput>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            input.enabled = enabled;
        }

        foreach(Player player in FindObjectsByType<Player>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            player.enabled = enabled;
        }

        foreach(PlayerMovement movement in FindObjectsByType<PlayerMovement>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            movement.enabled = enabled;
        }

        foreach(Toolbar_UI toolbar in FindObjectsByType<Toolbar_UI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            toolbar.enabled = enabled;
        }
    }

    private bool ConfirmPressed()
    {
        bool keyboardConfirm =
            Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame);

        bool gamepadConfirm =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        return keyboardConfirm || gamepadConfirm;
    }

    private bool BackPressed()
    {
        bool keyboardBack =
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame;

        bool gamepadBack =
            Gamepad.current != null &&
            Gamepad.current.buttonEast.wasPressedThisFrame;

        return keyboardBack || gamepadBack;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void OnDisable()
    {
        if(dayCycle != null)
        {
            dayCycle.DayEnded -= ShowEndOfDay;
        }
    }
}
