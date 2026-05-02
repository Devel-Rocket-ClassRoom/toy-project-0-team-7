using UnityEngine;
using UnityEngine.UI;

public class AssetManager : MonoBehaviour
{
    public LineManager lineManager;
    public const int MAX_LINE_COUNT = 7;

    public TrainManager trainManager;

    // + 객차 관리자 (있다면) 참조

    void Awake()
    {

    }

    private void Update()
    {
        // TEST
        if (Input.GetKeyDown(KeyCode.Alpha1))   
        {
            IncreaseLine();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            IncreaseTrain();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            IncreaseCarriage();
        }
        // TEST
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
}