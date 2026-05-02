using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

// --- Station 데이터 클래스 ---
public class Station : MonoBehaviour
{
    [SerializeField] private StationType shape;
    public StationType Shape => shape;

    public Passenger[] passengerPrefabs;
    public Transform waitingArea;

    [SerializeField] private int capacity = 6; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    [SerializeField] private float overflowTimer = 180f; // 3분으로 설정, 조절하면서 게임 실행하다가 나중에 고정하든가 수정

    public List<Passenger> waitingPassengers = new List<Passenger>();
    public List<Line> lines = new List<Line>(); // 승강장에 연결된 노선을 저장할 리스트

    public void AddPasssenger(StationType destination)
    {
        if (waitingPassengers.Count >= capacity)
        {
            Debug.Log($"[역 수용인원 초과] {waitingPassengers.Count} 더 이상 수용할 수 없습니다. 게임오버 타이머 스타트");
            return;
        }

        Passenger prefab = passengerPrefabs[(int)destination];
        Passenger passenger = Instantiate(prefab, waitingArea);

        passenger.Init(destination);

        int index = waitingPassengers.Count;
        passenger.transform.localPosition = new Vector3(index * 2.5f, 0f, 0f);

        waitingPassengers.Add(passenger);
    }
}
