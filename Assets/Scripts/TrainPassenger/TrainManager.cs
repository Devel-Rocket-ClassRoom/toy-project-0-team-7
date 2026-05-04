using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public List<Train> activeTrains = new List<Train>();

    public int availableTrainCount = 3;
    
    public GameObject trainPrefab;
    public List<Station> Stations = new();

    private void Update()
    {
        //[체크] 일시정지 기능 추가시 여기서 체크
        if (activeTrains.Count != 0)
            foreach (var train in activeTrains)
                train.Move();
        
    }
    public Train SpawnTrain(int lineId, List<Vector3> waypoints)
    {
        GameObject trainObject = Instantiate(trainPrefab, Stations[0].transform.position, Quaternion.identity);
        Train train = trainObject.GetComponent<Train>();

        train.SetPath(Stations, waypoints, true);
        train.lineId = lineId;
        train.GetComponentInChildren<SpriteRenderer>().color = Colors.colors[lineId];
        activeTrains.Add(train);
        train.Init();

        return train;
    }

    public void AddAvailableTrain()
    {
        availableTrainCount++;
        Debug.Log($"availableTrainCount: {availableTrainCount}");

        // 열차 버튼 생성 (화면 왼쪽 UI)
    }

    public void RemoveTrain(Train train)
    {
        Destroy(train.gameObject);
        activeTrains.Remove(train);
    }
}