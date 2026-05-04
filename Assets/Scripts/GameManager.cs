using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CameraController cameraController;

    public GameObject gameOverWindow;
    public TextMeshProUGUI gameResultText;

    public bool isGameOver = false;

    private void Awake()
    {
        gameOverWindow.SetActive(false);
    }


    private void OnEnable()
    {
        Station.OnTimeOver += HandleGameOver;
    }

    private void OnDisable()
    {
        Station.OnTimeOver -= HandleGameOver;
    }

    private void HandleGameOver(Vector3 stationPos)
    {
        if (isGameOver) return;

        isGameOver = true;
        cameraController.targetStation = stationPos;

        Invoke(nameof(LoadGameOverWindow), cameraController.transitionDelay);
    }

    private void LoadGameOverWindow()
    {
        Debug.Log("[게임 오버]");
        gameResultText.text = $"본 역은 너무 혼잡해 지하철이 폐쇄조치 되었습니다.\n당신의 철도에서 {AssetManager.dayCount}일 동안 {Score.score}명의 승객이 여행했습니다.";
        gameOverWindow.SetActive(true);
        Time.timeScale = 0f;
    }
}