using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssetManager : MonoBehaviour
{
    public enum Assets { Line, InterChangeStation }

    public GameManager gm;

    public MouseInput inputManager;
    public CanvasGroup gameUIGroup;

    public LineManager lineManager;
    public const int MAX_LINE_COUNT = 7;

    // (추가) 다른 자산 관리자들 . . .
    public TrainManager trainManager;
    public StationManager stationManager;
    // (추가) 다른 자산 관리자들 . . .

    public GameObject rewardPanel;
    public TextMeshProUGUI week;
    public TextMeshProUGUI message;

    public Button newTrainButton;
    public Button newAssetButton1;
    public Button newAssetButton2;
    public Button newInterchangeButton;
    public TextMeshProUGUI assetButtonText1;
    public TextMeshProUGUI assetButtonText2;

    public GameObject interchangeDragButton;
    
    private float dailyTimer = 0f;
    private const float dayInterval = 2f;

    public bool isWeekend = false;

    public static int dayCount = 1;
    private int weekCount = 0;
    private int displayWeek = 1;

    private int rewardRemain = 0;

    private float savedTimeScale;

    // --- 교차역 관련 변수 ---
    private int interchangeCount = 0;

    private void Awake()
    {
        newTrainButton.onClick.AddListener(OnClickNewTrain);
        rewardPanel.SetActive(false);
        newTrainButton.gameObject.SetActive(false);
        newAssetButton1.gameObject.SetActive(false);
        newInterchangeButton.gameObject.SetActive(false);
        interchangeDragButton.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale != 0)
        {
            if (dailyTimer > dayInterval)   // 1일 지나면
            {
                if (dayCount % 7 == 0)
                {
                    weekCount++;
                    rewardRemain++;

                    if (inputManager.mode == MouseInput.Mode.None)
                    {
                        ShowNextReward();
                    }
                }

                dayCount++;
                gm.UpdateUIText();
                dailyTimer = 0f;
            }

            dailyTimer += Time.deltaTime;
        }
    }

    public void ShowNextReward()
    {
        if (rewardRemain <= 0) return;

        savedTimeScale = Time.timeScale; // 현재 속도 저장
        Time.timeScale = 0f;
        isWeekend = true;
        gameUIGroup.interactable = false;
        ActivePanel();

        // 선택 자산 세팅
        Array assets = Enum.GetValues(typeof(Assets));
        Assets asset1;
        Assets asset2;

        // 첫 번째 자산 뽑기
        while (true)
        {
            asset1 = (Assets)assets.GetValue(UnityEngine.Random.Range(0, assets.Length));
            if (asset1 == Assets.Line && lineManager.IsLinesFull) continue;
            break;
        }

        // 두 번째 자산 뽑기
        do
        {
            asset2 = (Assets)assets.GetValue(UnityEngine.Random.Range(0, assets.Length));
        }
        while (asset2 == asset1 || (asset2 == Assets.Line && lineManager.IsLinesFull));

        newAssetButton1.onClick.RemoveAllListeners();
        newAssetButton2.onClick.RemoveAllListeners();
        newAssetButton1.onClick.AddListener(() => OnClickNewAsset(asset1));
        newAssetButton2.onClick.AddListener(() => OnClickNewAsset(asset2));

        switch (asset1)
        {
            case Assets.Line:
                assetButtonText1.text = "노선";
                break;
            case Assets.InterChangeStation:
                assetButtonText1.text = "교차역";
                break;
        }

        switch (asset2)
        {
            case Assets.Line:
                assetButtonText2.text = "노선";
                break;
            case Assets.InterChangeStation:
                assetButtonText2.text = "교차역";
                break;
        }
    }

    public void ActivePanel()
    {
        week.text = $"{displayWeek}번째 주";
        message.text = $"당신의 지하철을 위한 새로운 기관차가 있습니다.";

        newTrainButton.gameObject.SetActive(true);
        rewardPanel.SetActive(true);
    }

    public void OnClickNewTrain()
    {
        IncreaseTrain();
        newTrainButton.gameObject.SetActive(false);

        message.text = $"지하철에 어떤 자산을 고르시겠습니까?";

        newAssetButton1.gameObject.SetActive(true);
        newAssetButton2.gameObject.SetActive(true);
    }

    public void OnClickNewAsset(Assets asset)
    {
        switch (asset)
        {
            case Assets.Line:
                IncreaseLine();
                
                break;
            case Assets.InterChangeStation:
                IncreaseInterchange();
                break;
        }

        InactivePanel();
        rewardRemain--;
        displayWeek++;

        if (rewardRemain > 0 && inputManager.mode == MouseInput.Mode.None)
        {            
            ActivePanel(); // 다음 리워드 표시
        }
        else
        {
            Time.timeScale = savedTimeScale; // 1f 대신 복구
            isWeekend = false;
            gameUIGroup.interactable = true;
        }
    }

    public void InactivePanel()
    {
        rewardPanel.SetActive(false);
        newAssetButton1.gameObject.SetActive(false);
        newAssetButton2.gameObject.SetActive(false);
    }

    public void IncreaseTrain()
    {
        trainManager.AddAvailableTrain();
    }

    public void IncreaseLine()  // if문 검사 필요
    {
        lineManager.AddAvailableLine();
    }

    public void IncreaseCarriage()
    {
        Debug.Log("객차 수 증가"); // CarriageManager.-----
    }

    public void IncreaseInterchange()
    {
        // InterchangeDragButton 생성 + 활성화 + 리스너 추가
        interchangeCount++;
        Debug.Log($"[교차역 획득] 사용 가능한 교차역 수: {interchangeCount}");
        interchangeDragButton.SetActive(true);
    }

    public void InterchangeUsed()
    {
        interchangeCount--;
        Debug.Log($"[교차역 사용] 남은 교차역 수: {interchangeCount}");
        if (interchangeCount <= 0)
        {
            interchangeDragButton.SetActive(false);
            Debug.Log("[교차역 사용] 모든 교차역이 사용되었습니다. 드래그 버튼 비활성화.");
        }
    }

    public void OnInputReleased()
    {
        if (rewardRemain > 0 && !isWeekend)
            ShowNextReward();
    }
}