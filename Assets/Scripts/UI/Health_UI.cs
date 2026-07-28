using UnityEngine;
using UnityEngine.UI;

public class Health_UI : MonoBehaviour
{
    [Header("Heart Images")]
    [SerializeField] private Image[] hearts;

    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    public void SetHealth(int currentHealth)
    {
      int maximumDisplayedHealth = hearts.Length * 2;

        currentHealth = Mathf.Clamp(currentHealth, 0, maximumDisplayedHealth);

        for(int i = 0; i < hearts.Length; i++)
        {
            int healthForThisHeart = currentHealth - (i * 2);

            if(healthForThisHeart >= 2)
            {
                hearts[i].sprite = fullHeartSprite;
            }
            else if(healthForThisHeart == 1)
            {
                hearts[i].sprite = halfHeartSprite;
            }
            else
            {
                hearts[i].sprite = emptyHeartSprite;
            }
        }
    }
}
