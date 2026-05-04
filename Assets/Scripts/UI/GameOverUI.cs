using UnityEngine;
using UnityEngine.UI;   
public class GameOverUI : MonoBehaviour
{
    public GameObject menuPanel;
    public Button arrowButton;

    private void Awake()
    {
        menuPanel.SetActive(false);    
    }
    private void Start()
    {
        arrowButton.onClick.AddListener(OnClickMenu);
    }

    public void OnClickMenu()
    {
        Time.timeScale = 1f; // 게임 오버 UI에서 메뉴로 돌아갈 때 시간 정지 해제
        menuPanel.SetActive(true);
    }   
}
