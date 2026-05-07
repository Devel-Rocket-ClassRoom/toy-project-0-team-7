using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public List<Train> activeTrains = new List<Train>();

    public int availableTrainCount = 3;
    public int availableHighSpeedTrain = 0;
    public int activeBasicTrainCount = 0;
    public int activeHighSpeedTrainCount = 0;

    public GameObject trainPrefab;
    public GameObject highTrainPrefab;
    public GameObject carriagePrefab;
    public List<Station> Stations = new();

    private void Update()
    {
        //[체크] 일시정지 기능 추가시 여기서 체크
        if (activeTrains.Count != 0)
            foreach (var train in activeTrains)
                train.Move();
        
    }
    public Train SpawnTrain(int lineId, List<Vector3> waypoints, Line line, bool isHighTrain = false)
    {
        GameObject trainObject;

        if (!isHighTrain)
        {
            trainObject = Instantiate(trainPrefab, Stations[0].transform.position, Quaternion.identity);
            activeBasicTrainCount++;
        }
        else
        {
            trainObject = Instantiate(highTrainPrefab, Stations[0].transform.position, Quaternion.identity);
            activeHighSpeedTrainCount++;
        }

        Train train = trainObject.GetComponent<Train>();

        train.SetPath(Stations, waypoints, true);
        train.lineId = lineId;
        train.myLine = line;
        train.GetComponentInChildren<SpriteRenderer>().color = Colors.colors[lineId];
        activeTrains.Add(train);
        train.isHighSpeedTrain = isHighTrain;
        train.Init();

        return train;
    }

    public void AddAvailableTrain()
    {
        availableTrainCount++;
        Debug.Log($"availableTrainCount: {availableTrainCount}");

        // 열차 버튼 생성 (화면 왼쪽 UI)
    }

    public void AddHighSpeedTrain()
    {
        availableHighSpeedTrain++;
        Debug.Log($"고속 열차 +1: {availableHighSpeedTrain}");
    }

    public void RemoveTrain(Train train)
    {
        foreach (var carriage in train.AttachedCarriages)
        {
            Destroy(carriage);
        }

        if (train.isHighSpeedTrain)
        {
            activeHighSpeedTrainCount--;
            Debug.Log($"[열차 반환] 고속 열차 1대가 제거되었습니다. activeHighSpeedTrainCount: {activeHighSpeedTrainCount}");
        }
        else
        {
            activeBasicTrainCount--;
            Debug.Log($"[열차 반환] 일반 열차 1대가 제거되었습니다. activeBasicTrainCount: {activeBasicTrainCount}");
        }
        
        Destroy(train.gameObject);
        activeTrains.Remove(train);
    }

    public void AddCarriage(Train train)
    {
        GameObject carriage = Instantiate(carriagePrefab, train.transform.position, train.transform.rotation);
        Train carriageTrain = carriage.GetComponent<Train>();
        Transform[] carriageSlots = carriageTrain?.passengerSlots;

        if (carriageTrain != null)
        {
            Destroy(carriageTrain);
        }

        Destroy(carriage.GetComponent<Collider2D>()); 

        carriage.GetComponentInChildren<SpriteRenderer>().color = Colors.colors[train.lineId];
        train.AttachCarriage(carriage, carriageSlots);
    }
}