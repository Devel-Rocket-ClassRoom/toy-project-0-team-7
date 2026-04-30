using System.Collections.Generic;
using UnityEngine;

//테스트 스크립트로 나중에 삭제예정
public class TestStation : MonoBehaviour
{
    public StationType shapeType;
    public List<Passenger> waitingPassengers = new List<Passenger>(); // 대기 중인 승객들

    // 승객이 추가될 때 호출될 함수
    public void AddPassenger(Passenger p)
    {
        waitingPassengers.Add(p);

        //테스트로그
        Debug.Log($"[승객 입성] {gameObject.name}(역 모양: {shapeType}) -> " +
            $"목적지: {p.destination}행 승객 추가됨 (현재 대기: {waitingPassengers.Count}명)");
    }

}
