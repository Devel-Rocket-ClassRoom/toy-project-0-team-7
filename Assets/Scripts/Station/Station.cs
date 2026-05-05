using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

// --- Station 데이터 클래스 ---
public class Station : MonoBehaviour
{
    [SerializeField] private StationType shape;
    public StationType Shape => shape;

    public Passenger[] passengerPrefabs;
    public Transform waitingArea;
    public StationTimerUI timerUI;
    private PassengerManager pm; 
    
    [Header("역 수용인원 및 초과 타이머 설정")] 
    [SerializeField] private int capacity = 6; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    [SerializeField] private float overflowTimer = 30f; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정

    [Header("승객 스폰 시간 가격")]
    [SerializeField] private float minSpawnTime = 5f;
    [SerializeField] private float maxSpawnTime = 15f;
   

    public List<Passenger> waitingPassengers = new List<Passenger>();
    public List<Line> lines = new List<Line>(); // 승강장에 연결된 노선을 저장할 리스트
    public static event Action<Vector3> OnTimeOver; // 추후 게임 오버될 때 GameManager가 구독할 이벤트
    private bool isOverflow = false;
    private float currentTimer = 0f;

    private void Awake()
    {
        pm = GameObject.FindWithTag("PassengerManager").GetComponent<PassengerManager>();
    }   
    private void Start()
    {
        if (timerUI != null) timerUI.gameObject.SetActive(false);
        StartCoroutine(CoSpawnPassengers(pm));  
    }

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

    public void StartSpawningPassengers(PassengerManager pm)
    {
        StartCoroutine(CoSpawnPassengers(pm));
    }

    private IEnumerator CoSpawnPassengers(PassengerManager pm)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnTime, maxSpawnTime)); // 역이 생성되고 나서 잠깐 기다렸다가 승객 스폰 시작
        
        while (true)
        {
            int spawnCount = UnityEngine.Random.value < 0.7f ? 1 : 2;
            for (int i = 0; i < spawnCount; i++)
            {
                StationType dest = pm.GetRandomDestExcluding(this.shape);
                AddPasssenger(dest);  
                //Debug.Log($"[승객 스폰] {shape} 역에서 {spawnCount}명 승객 스폰");
            }
              
            float spawnInterval = UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(spawnInterval);
        }
    }


    public void AddPasssenger(StationType destination)
    {
        if (waitingPassengers.Count >= capacity)
        {
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

        }
    }

    // --- 승객이 열차를 타서 역에서 없애야할 때 호출할 메서드 ---
    public void RemovePassenger(Passenger passenger)
    {
        waitingPassengers.Remove(passenger);

        for (int i = 0; i < waitingPassengers.Count; i++)
        {
            waitingPassengers[i].transform.localPosition = new Vector3(i * 2.5f, 1f, 0f);
        }

        if (waitingPassengers.Count <= capacity && isOverflow)
        {
            isOverflow = false;
            currentTimer = overflowTimer;
            
            timerUI.gameObject.SetActive(false);
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
