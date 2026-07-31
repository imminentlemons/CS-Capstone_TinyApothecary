using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonPressSound : MonoBehaviour,
    IPointerDownHandler,
    ISubmitHandler
{
    private bool usePageTurnSound;

    public void SetPageTurnSound(bool enabled)
    {
        usePageTurnSound = enabled;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            PlaySound();
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlaySound();
    }

    private void PlaySound()
    {
        if(usePageTurnSound)
        {
            AudioManager.PlayPageTurn();
        }
        else
        {
            AudioManager.PlayUIButtonPress();
        }
    }
}
