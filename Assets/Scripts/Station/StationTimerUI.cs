using UnityEngine;
using UnityEngine.UI;

public class StationTimerUI : MonoBehaviour
{
    public Image fillImage;

    public void UpdateFill(float current, float max)
    {
        fillImage.fillAmount = 1f - (current / max);
    }

    public void SetFull()
    {
        fillImage.fillAmount = 1f;
    }
}
