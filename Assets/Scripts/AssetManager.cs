using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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
    // (추가) 다른 자산 관리자들 . . .

    public GameObject rewardPanel;
    public TextMeshProUGUI week;
    public TextMeshProUGUI message;

    public Button newTrainButton;
    public Button newAssetButton1;
    public Button newAssetButton2;
    public TextMeshProUGUI assetButtonText1;
    public TextMeshProUGUI assetButtonText2;

    public GameObject interchangeDragButton;
    public GameObject trainDragButton;
    public Image interchangeAssetUI;
    public Image trainAssetUI;
    
    private float dailyTimer = 0f;
    private const float dayInterval = 20f;

    public bool isWeekend = false;

    public static int dayCount = 1;
    private int weekCount = 0;
    private int displayWeek = 1;

    private int rewardRemain = 0;

    private float savedTimeScale;
    
    private List<Sprite> sprites = new();

// --- 자산 개수 관리 관련 변수 ---
    private int interchangeCount = 0;
    private int remainingTrainCount = 0; 
    public int RemainingTrainCount => remainingTrainCount;  

    private void Awake()
    {
        sprites.Add(Resources.Load<Sprite>("line"));
        sprites.Add(Resources.Load<Sprite>("interchangeStation"));

        newTrainButton.onClick.AddListener(OnClickNewTrain);
        rewardPanel.SetActive(false);
        newTrainButton.gameObject.SetActive(false);
        newAssetButton1.gameObject.SetActive(false);
        newAssetButton2.gameObject.SetActive(false);

        interchangeDragButton.SetActive(false);
        Debug.Log($"[초기화] 기관차 수: {trainManager.availableTrainCount}");
        UpdateTrainUI();
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
        var max = 100;
        var a = 1;
        do
        {
            asset1 = (Assets)assets.GetValue(UnityEngine.Random.Range(0, assets.Length));
            a++;

            if (a == max) break;
        }
        while (asset1 == Assets.Line && lineManager.CantAddLine);
        Debug.Log(asset1);
        Debug.Log($"a = {a}");

        // 두 번째 자산 뽑기
        a = 1;
        do
        {
            asset2 = (Assets)assets.GetValue(UnityEngine.Random.Range(0, assets.Length));
            a++;

            if (a == max) break;
        }
        while (asset2 == asset1 || (asset2 == Assets.Line && lineManager.CantAddLine));
        Debug.Log(asset2);
        Debug.Log($"a = {a}");

        SetAssetButton(newAssetButton1, assetButtonText1, asset1);
        SetAssetButton(newAssetButton2, assetButtonText2, asset2);
    }

    public void SetAssetButton(Button button, TextMeshProUGUI text, Assets asset)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnClickNewAsset(asset));

        switch (asset)
        {
            case Assets.Line:
                text.text = "노선";
                button.image.sprite = sprites[0];
                break;
            case Assets.InterChangeStation:
                text.text = "교차역";
                button.image.sprite = sprites[1];
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
        Debug.Log("[기관차 획득] 기관차 1대가 추가되었습니다.");    
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
        UpdateTrainUI();
        remainingTrainCount = trainManager.availableTrainCount - trainManager.activeTrains.Count;
        Debug.Log($"[기관차 획득] 사용 가능한 기관차 수: {remainingTrainCount}");
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
        if (interchangeCount == 1) // 처음 획득했을 때만 버튼 활성화    
        {
            interchangeDragButton.SetActive(true);
        }

        UpdateAssetUI(interchangeAssetUI, interchangeDragButton.GetComponent<Button>(), interchangeCount);
    }

    public void InterchangeUsed()
    {
        interchangeCount--;
        Debug.Log($"[교차역 사용] 남은 교차역 수: {interchangeCount}");
        UpdateAssetUI(interchangeAssetUI, interchangeDragButton.GetComponent<Button>(), interchangeCount);
    }

    public void OnInputReleased()
    {
        if (rewardRemain > 0 && !isWeekend)
            ShowNextReward();
    }

    // --- 열차 UI 업데이트 ---
    // 열차 개수 0개 -> 회색 처리 
    public void UpdateTrainUI()
    {
        remainingTrainCount = trainManager.availableTrainCount - trainManager.activeTrains.Count;
        UpdateAssetUI(trainAssetUI, trainDragButton.GetComponent<Button>(), remainingTrainCount);
        Debug.Log($"[기관차 UI 업데이트] 사용 가능한 기관차 수: {remainingTrainCount}");
    }

    // --- 자산 UI 업데이트 ---
    private void UpdateAssetUI(Image image, Button button, int count)
    {
        image.color = count > 0 ? Color.white : Color.gray;
        button.interactable = count > 0;
    }
}