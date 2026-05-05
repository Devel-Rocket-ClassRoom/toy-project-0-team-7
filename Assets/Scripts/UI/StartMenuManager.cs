using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    public Button playButton;
    public Button optionsButton;
    public Button backButton;

    private void Start()
    {
        playButton.onClick.AddListener(OnClickPlay);
        optionsButton.onClick.AddListener(OnClickOptions);
        backButton.onClick.AddListener(OnClickBack);
    }

    private void OnClickPlay()
    {
        SceneManager.LoadScene("Sky");
    }

    private void OnClickOptions()
    {
        // To do: 추후 구현 예정 or 아예 생략
    }

    private void OnClickBack()
    {
        SceneManager.LoadScene("MainTitleScene");
    }
}
