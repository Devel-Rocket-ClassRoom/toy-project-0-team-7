using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    public Button playButton;
    public Button optionsButton;
    public Button backButton;

    public GameObject creditPanel;

    private void Start()
    {
        creditPanel.SetActive(false);
        playButton.onClick.AddListener(OnClickPlay);
        optionsButton.onClick.AddListener(OnClickCredit);
        backButton.onClick.AddListener(OnClickBack);
    }

    private void OnClickPlay()
    {
        SceneManager.LoadScene("Sky");
    }

    private void OnClickCredit()
    {
        creditPanel.SetActive(true);
    }

    private void OnClickBack()
    {
        creditPanel.SetActive(false);
        SceneManager.LoadScene("MainTitleScene");
    }
}
