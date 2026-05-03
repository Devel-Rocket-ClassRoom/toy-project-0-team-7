using UnityEngine;
using UnityEngine.UI;

public class GameSpeedController : MonoBehaviour
{
    public Toggle pauseToggle;
    public Toggle resumeToggle;
    public Toggle doubleSpeedToggle;

    private void Awake()
    {
        pauseToggle.onValueChanged.AddListener(on => {
            if (on)
            {
                Time.timeScale = 0f;
                pauseToggle.interactable = false;
                resumeToggle.interactable = true;
                doubleSpeedToggle.interactable = true;
            }
        });

        resumeToggle.onValueChanged.AddListener(on => {
            if (on)
            {
                Time.timeScale = 1f;
                pauseToggle.interactable = true;
                resumeToggle.interactable = false;
                doubleSpeedToggle.interactable = true;
            }
        });

        doubleSpeedToggle.onValueChanged.AddListener(on => {
            if (on)
            {
                Time.timeScale = 2f;
                pauseToggle.interactable = true;
                resumeToggle.interactable = true;
                doubleSpeedToggle.interactable = false;
            }
        });
    }

    private void Start()
    {
        resumeToggle.isOn = true;
        resumeToggle.interactable = false;
        resumeToggle.graphic.color = resumeToggle.colors.selectedColor;
    }
}
