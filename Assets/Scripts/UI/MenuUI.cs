using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public Button restartButton;
    public Button menuButton;
    public GameManager gameManager;

    private void Start()
    {
        restartButton.onClick.AddListener(OnClickRestart);
        menuButton.onClick.AddListener(OnClickMenu);
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        gameManager.gameResultText.text = "";
        AssetManager.dayCount = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 아직 메뉴 씬이 없어서 일단 주석 처리해둠
    public void OnClickMenu()
    {
        // Time.timeScale = 1f;
        // SceneManager.LoadScene("Menu");
    }
}
