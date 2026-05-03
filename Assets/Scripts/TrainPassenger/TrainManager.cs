using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public List<Train> activeTrains = new List<Train>();

    public int availableTrainCount = 3;
    
    public GameObject trainPrefab;
    public List<Station> testStations = new();

    private void Update()
    {
        //[체크] 일시정지 기능 추가시 여기서 체크
        if (activeTrains.Count != 0)
            foreach (var train in activeTrains)
                train.Move();
        
    }
    public Train SpawnTrain(int lineId)
    {
        if (testStations.Count == 0) return null;
        //[수정 예정] 실제 게임에선 0번 역이 아니라 사용자가 클릭한 노선의 시작점에 스폰
        GameObject trainObject = Instantiate(trainPrefab, testStations[0].transform.position, Quaternion.identity);
        Train train = trainObject.GetComponent<Train>();

        //[수정 예정] 실제 데이터로 변경
        train.SetPath(new List<Station>(testStations));
        train.lineId = lineId;
        activeTrains.Add(train);

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