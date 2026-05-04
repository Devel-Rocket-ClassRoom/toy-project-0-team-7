using System.Collections.Generic;
using UnityEngine;

public class PassengerManager : MonoBehaviour
{

    //[수정 예정] 승객이 스폰될 역들의 리스트 > Station쪽에서 참조해야함
    public List<Station> allStations = new List<Station>();

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

}
