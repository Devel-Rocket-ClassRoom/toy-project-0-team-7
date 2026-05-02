using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public List<Train> activeTrains = new List<Train>();
    //public Train train;
    public GameObject trainPrefab;


    private void Update()
    {
        //[체크] 일시정지 기능 추가시 여기서 체크
        foreach (var train in activeTrains)
        {
            if (train != null)
            {
                train.Move();
            }
        }
    }
    //Line 데이터 받아서 열차 생성
    public void SpawnTrain(Line targetLine)
    {
        if (targetLine == null || targetLine.stations == null || targetLine.stations.Count == 0)
        {
            Debug.LogWarning("노선 데이터가 비어있어 열차를 생성할 수 없습니다.");
            return;
        }

        GameObject trainObject = Instantiate(trainPrefab, targetLine.stations[0].transform.position, Quaternion.identity);
        Train train = trainObject.GetComponent<Train>();

        if (train != null)
        {
            //Line에서 가지고 있는 노선
            train.SetPath(targetLine.stations);
            activeTrains.Add(train);
            targetLine.trains.Add(train);
        }
    }
}
