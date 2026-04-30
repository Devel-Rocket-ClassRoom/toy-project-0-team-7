using UnityEngine;
using System.Collections.Generic;

// --- Station 데이터 클래스 ---
public class Station : MonoBehaviour
{
    private StationType shape;
    public StationType Shape => shape;

    [SerializeField] private int capacity = 6; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    [SerializeField] private float overflowTimer = 180f; // 3분으로 설정, 조절하면서 게임 실행하다가 나중에 고정하든가 수정

    public List<Passenger> waitingPassengers = new List<Passenger>();
    public List<Line> lines = new List<Line>(); // 승강장에 연결된 노선을 저장할 리스트

    public void AddPasssenger(Passenger p)
    {
        if (waitingPassengers.Count > 6)
        {
            Debug.Log($"[역 수용인원 초과] {waitingPassengers.Count} 더 이상 수용할 수 없습니다. 게임오버 타이머 스타트");
            return;
        }
        
        waitingPassengers.Add(p);
    }
}
