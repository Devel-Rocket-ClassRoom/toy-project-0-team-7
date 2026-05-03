using UnityEngine;
using System;
using System.Collections.Generic;

// --- Station 데이터 클래스 ---
public class Station : MonoBehaviour
{
    [SerializeField] private StationType shape;
    public StationType Shape => shape;

    public Passenger[] passengerPrefabs;
    public Transform waitingArea;
    public StationTimerUI timerUI;

    [SerializeField] private int capacity = 6; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    [SerializeField] private float overflowTimer = 5f; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    public List<Passenger> waitingPassengers = new List<Passenger>();
    public List<Line> lines = new List<Line>(); // 승강장에 연결된 노선을 저장할 리스트
    public static event Action<Vector3> OnTimeOver; // 추후 게임 오버될 때 GameManager가 구독할 이벤트

    private bool isOverflow = false;
    private float currentTimer = 0f;

    private void Update()
    {
        if (isOverflow)
        {
            currentTimer -= Time.deltaTime;

            if (timerUI != null) timerUI.UpdateFill(currentTimer, overflowTimer);

            if (currentTimer <= 0f)
            {
                isOverflow = false;

                if (timerUI != null) timerUI.SetFull();
                TimeOver();
            }
        }
    }

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
        passenger.transform.localPosition = new Vector3(index * 2.5f, 1f, 0f);

        waitingPassengers.Add(passenger);

        if (waitingPassengers.Count >= capacity && !isOverflow)
        {
            isOverflow = true;
            currentTimer = overflowTimer;

            if (timerUI != null) timerUI.gameObject.SetActive(true);

            Debug.Log("[수용인원 초과] 타이머 시작]");
        }
    }

    // --- 승객이 열차를 타서 역에서 없애야할 때 호출할 메서드 ---
    public void RemovePassenger(Passenger passenger)
    {
        waitingPassengers.Remove(passenger);

        if (waitingPassengers.Count <= capacity)
        {
            isOverflow = false;
            currentTimer = overflowTimer;
        }
    }

    public void TimeOver()
    {
        foreach (var station in GetComponentsInChildren<SpriteRenderer>())
        {
            station.sortingOrder = 20;
        }
        
        OnTimeOver?.Invoke(transform.position);
    }

}
