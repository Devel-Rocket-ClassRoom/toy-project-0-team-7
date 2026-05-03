using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssetManager : MonoBehaviour
{
    public MouseInput inputManager;
    public CanvasGroup gameUIGroup;

    public LineManager lineManager;
    public const int MAX_LINE_COUNT = 7;

    public TrainManager trainManager;

    // (추가) 다른 자산 관리자들 . . .

    public GameObject rewardPanel;
    public TextMeshProUGUI week;
    public TextMeshProUGUI message;

    public Button newTrainButton;
    public Button newAssetButton1;
    public Button newAssetButton2;

    private float weeklyTimer = 0f;
    private const float weekInterval = 140f;

    public bool isWeekend = false;

    private int weekCount = 1;
    private int displayWeek = 2;

    private int rewardRemain = 0;

    private void Awake()
    {
        newTrainButton.onClick.AddListener(OnClickNewTrain);
        rewardPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isWeekend)
        {
            if (Input.GetKeyDown(KeyCode.Space))    // 시간 토글
            {
                if (Time.timeScale != 0)
                {
                    Time.timeScale = 0f; // 정지
                }
                else
                {
                    Time.timeScale = 1f; // 재개
                }
            }
        }

        if (Time.timeScale != 0)
        {
            if (weeklyTimer > weekInterval) // 1주일 지나면
            {
                rewardRemain++;
                weekCount++;
                weeklyTimer = 0f;

                if (inputManager.mode == MouseInput.Mode.None)
                    ShowNextReward();
            }

            weeklyTimer += Time.deltaTime;
        }
    }

    public void IncreaseLine()  // if문 검사 필요
    {
        lineManager.AddAvailableLine();
    }

    public void IncreaseTrain()
    {
        trainManager.AddAvailableTrain();
    }

    public void IncreaseCarriage()
    {
        Debug.Log("객차 수 증가"); // CarriageManager.-----
    }

    public void ShowNextReward()
    {
        if (rewardRemain <= 0) return;

        Time.timeScale = 0f;
        isWeekend = true;
        gameUIGroup.interactable = false;
        ActivePanel();
    }

    public void ActivePanel()
    {
        week.text = $"{displayWeek}번째 주";
        message.text = $"당신의 지하철을 위한 새로운 기관차가 있습니다.";

        newTrainButton.gameObject.SetActive(true);
        rewardPanel.SetActive(true);
    }

    public void InactivePanel()
    {
        rewardPanel.SetActive(false);
        newAssetButton1.gameObject.SetActive(false);
        newAssetButton2.gameObject.SetActive(false);
    }

    public void OnClickNewTrain()
    {
        IncreaseTrain();
        newTrainButton.gameObject.SetActive(false);

        message.text = $"지하철에 어떤 자산을 고르시겠습니까?";

        newAssetButton1.onClick.RemoveAllListeners();
        newAssetButton2.onClick.RemoveAllListeners();
        newAssetButton1.onClick.AddListener(() => OnClickNewAsset());
        newAssetButton2.onClick.AddListener(() => OnClickNewAsset());
        newAssetButton1.gameObject.SetActive(true);
        newAssetButton2.gameObject.SetActive(true);
    }

    public void OnClickNewAsset()
    {
        IncreaseLine(); // 테스트 가능한 자산이 하나뿐이라 일단 두 버튼 다 노선으로 통일함.
        InactivePanel();
        rewardRemain--;
        displayWeek++;

        if (rewardRemain > 0 && inputManager.mode == MouseInput.Mode.None)
        {            
            ActivePanel(); // 다음 리워드 표시
        }
        else
        {
            Time.timeScale = 1f;
            isWeekend = false;
            gameUIGroup.interactable = true;
        }
    }

    public void OnInputReleased()
    {
        if (rewardRemain > 0 && !isWeekend)
            ShowNextReward();
    }
}