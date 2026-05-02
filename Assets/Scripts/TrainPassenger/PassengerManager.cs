using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerManager : MonoBehaviour
{
    public float spawnInterval = 5f;

    //[수정 예정] 승객이 스폰될 역들의 리스트 > Station쪽에서 참조해야함
    public List<Station> allStations = new List<Station>();

    private void Start()
    {
        //3초마다 승객 생성
        StartCoroutine(CoSpawnPassenger());
    }
    private void Update()
    {

    }
    public void SpawnPassenger()
    {
        if (allStations.Count < 3) return;
  
        //출발역 랜덤 선정
        int startIndex = Random.Range(0, allStations.Count);
        Station startStation = allStations[startIndex];

        //출발역과 다른 모양의 목적지 선정
        StationType randomDest = GetRandomDestExcluding(startStation.Shape);

        //승객 객체는 Station에서 생성. 여기서는 모양만 전달
        startStation.AddPasssenger(randomDest);
    }

    public StationType GetRandomDestExcluding(StationType type)
    {
        //현재 역들 중 모양 하나 랜덤 선정
        int randomIndex = Random.Range(0, allStations.Count);
        StationType selectedDest = allStations[randomIndex].Shape;

        //뽑은 모양이 출발역 모양과 같으면 다시 뽑기
        int loopCount = 0;
        while (selectedDest == type && loopCount < 50)
        {
            randomIndex = Random.Range(0, allStations.Count);
            selectedDest = allStations[randomIndex].Shape;
            loopCount++;
        }
        return selectedDest;
    }

    //일정 시간마다 승객 생성하는 코루틴
    public IEnumerator CoSpawnPassenger()
    {
        //처음 시작할때만 5초 뒤에 생성
        yield return new WaitForSeconds(5f);

        while (true)
        {
            SpawnPassenger();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
