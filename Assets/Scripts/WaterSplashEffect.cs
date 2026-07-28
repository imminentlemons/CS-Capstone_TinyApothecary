using UnityEngine;

public class WaterSplashEffect : MonoBehaviour
{
    [SerializeField] private Animator splashAnimator;

    public void Play(Vector2 direction)
    {
        if(splashAnimator == null)
        {
            return;
        }

        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if(direction.x < 0f)
            {
                splashAnimator.Play("SplashLeft", 0, 0f);
            }

            else
            {
                splashAnimator.Play("SplashRight", 0, 0f);
            }
        }

        else
        {
            splashAnimator.Play("SplashUpDown", 0, 0f);
        }
    }

    public void DestroyEffect()
    {
        Destroy(gameObject);
    }
}
