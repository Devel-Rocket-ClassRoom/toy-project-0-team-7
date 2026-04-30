using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerManager : MonoBehaviour
{
    public float spawnInterval = 3f;

    //[수정 예정] 승객이 스폰될 역들의 리스트 > Station쪽에서 참조해야함
    public List<TestStation> allStations;

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
        TestStation startStation = allStations[startIndex];

        //출발역과 다른 모양의 목적지 선정
        StationType randomDest = GetRandomDestExcluding(startStation.shapeType);

        //승객 객체 생성
        Passenger p = new Passenger(randomDest);
        //테스트 로그
        Debug.Log($"<color=cyan>[데이터 생성]</color> 새로운 승객 발생! " +
            $"출발지: {startStation.name}, 목적지: {randomDest}");
        startStation.AddPassenger(p);
    }

    public StationType GetRandomDestExcluding(StationType type)
    {
        //현재 역들 중 모양 하나 랜덤 선정
        int randomIndex = Random.Range(0, allStations.Count);
        StationType selectedDest = allStations[randomIndex].shapeType;

        //뽑은 모양이 출발역 모양과 같으면 다시 뽑기
        int loopCount = 0;
        while (selectedDest == type && loopCount < 50)
        {
            randomIndex = Random.Range(0, allStations.Count);
            selectedDest = allStations[randomIndex].shapeType;
            loopCount++;
        }
        return selectedDest;
    }

    //일정 시간마다 승객 생성하는 코루틴
    public IEnumerator CoSpawnPassenger()
    {
        while (true)
        {
            SpawnPassenger();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
