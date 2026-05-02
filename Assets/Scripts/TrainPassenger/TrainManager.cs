using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public List<Train> activeTrains = new List<Train>();
    //public Train train;
    public GameObject trainPrefab;
    

    // [수정 예정] 나중에 LineManager가 완성되면 testStations 대신 
    // 실제 Line이 보유한 Station 리스트를 가져와야 함
    public List<Station> testStations = new();

    private void Start()
    {
        //SpawnTrain();
    }
    private void Update()
    {
        //[체크] 일시정지 기능 추가시 여기서 체크
        if (activeTrains.Count != 0)
            foreach (var train in activeTrains)
                train.Move();
        
    }
    public Train SpawnTrain(int lineId)
    {
        //[수정 예정] 실제 게임에선 0번 역이 아니라 사용자가 클릭한 노선의 시작점에 스폰
        GameObject trainObject = Instantiate(trainPrefab, testStations[0].transform.position, Quaternion.identity);
        Train train = trainObject.GetComponent<Train>();

        //[수정 예정] 실제 데이터로 변경
        train.SetPath(testStations);
        train.lineId = lineId;
        activeTrains.Add(train);

        return train;
    }
}
