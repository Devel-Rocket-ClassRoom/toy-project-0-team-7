using UnityEngine;
using System.Collections.Generic;
using Unity.VectorGraphics;
using Unity.VisualScripting;
// --- Station Prefab을 랜덤하게 생성하는 스포너 역할 클래스 ---
public class StationManager : MonoBehaviour
{
    public Station[] prefabs;

    [Header("스폰 시간 조정")] // To Do: 추후 실제 게임에 맞게 느리게 조정해야함
    [SerializeField] private float minRandomTime = 20f;
    [SerializeField] private float maxRandomTime = 30f;
    
    [Header("역 오른쪽 공간 offset")]
    private float rightOffset = 2.2f;
    private float minRadius;
    private float timer = 0f;
    //private float gameTime = 0f;
    private float offset = 0.5f;
    private int initialCount = 3; // 처음 시작할 때 생성할 역의 수 (각각 다른 모양으로 3개) 
    private int spawnedCount = 0;   // 처음 역이 세 개 생성된 후, 랜덤 스폰되는 횟수
    private float initialSpawnTime = 15f;
    private List<Station> exisitingStations;
    public List<Station> ExisitingStations => exisitingStations;
    public PassengerManager pm;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        exisitingStations = new List<Station>();
        pm = GameObject.FindWithTag("PassengerManager").GetComponent<PassengerManager>();
        float screenHeight = cam.orthographicSize * 2f;
        minRadius = screenHeight * 0.15f;
    } 
    private void Start()
    {
        InitialRandomSpawn(initialCount);
        timer = initialSpawnTime;
        Debug.Log($"[시작] 초기 스폰 완료 | 다음 스폰 시간 타이머: {timer:F2}");
        spawnedCount++;    
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Debug.Log("[시간 초과] 역 생성됨");
            RandomSpawn();
            timer = GetNextInterval();
            spawnedCount++;
            Debug.Log($"[스폰] {spawnedCount}번째 스폰 | 다음 스폰 시간 타이머: {timer:F2}");
        }
    }

    private float GetNextInterval()
    {
        if (spawnedCount < 4)
        {
            return 27f;
        }

        else
        {
            return Random.Range(minRandomTime, maxRandomTime);  
        }
    }

    // --- 처음 시작할 때 각각 다른 모양으로 3개의 역 랜덤 스폰하는 메서드 ---
    private void InitialRandomSpawn(int count)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        var shuffeld = new List<Station>(prefabs);
        for (int i = shuffeld.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffeld[i], shuffeld[j]) = (shuffeld[j], shuffeld[i]);
        }

        for (int i = 0; i < Mathf.Min(count, shuffeld.Count); i++)
        {
            var station = Instantiate(shuffeld[i]);
            Vector3 stationPos = GetRandomScreenPos();

            int attempts = 0;
            while (!IsValidPosition(stationPos) && attempts < 100)
            {
                stationPos = GetRandomScreenPos();
                attempts++;
            }

            station.transform.position = stationPos;
            exisitingStations.Add(station);
            pm.allStations.Add(station);
        }
    }

    // --- 한 번 실행될 때 승강장 하나만 랜덤 생성 ---
    private void RandomSpawn()
    {
        if (prefabs == null || prefabs.Length == 0) return;

        int range = prefabs.Length;
        var station = Instantiate(prefabs[Random.Range(0, range)]);
        Vector3 stationPos = GetRandomScreenPos();

        int attempts = 0;
        while (!IsValidPosition(stationPos) && attempts < 100)
        {
            stationPos = GetRandomScreenPos();
            attempts++;
        }
        station.transform.position = stationPos;
        exisitingStations.Add(station);
        pm.allStations.Add(station);

    }

    // --- 승강장 위치 랜덤하게 생성 ---
    private Vector3 GetRandomScreenPos()
    {
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        float x = Random.Range(bottomLeft.x + offset, topRight.x - rightOffset);
        float y = Random.Range(bottomLeft.y + offset, topRight.y - offset);

        return new Vector3(x, y, 0f);
    }

    // --- 이미 생성된 역 리스트를 순회하면서 최소 및 최대 거리 검사 메서드 ---
    private bool IsValidPosition(Vector3 stationPos)
    {
        if (ExisitingStations == null || ExisitingStations.Count == 0)
        {
            return true;
        }

        foreach (var station in exisitingStations)
        {
            //Debug.Log($"[포지션 검사] {stationPos}가 유효한 포지션인지 검사");
            float dist = Vector2.Distance(stationPos, station.transform.position);
            if (dist < minRadius)
            {
                //Debug.Log($"[포지션 검사] {dist}가 생성 거리의 범위를 충족하지 못했습니다.");
                return false;
            }
        }
        
        return true;
    }

}
