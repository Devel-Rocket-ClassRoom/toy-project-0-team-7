using UnityEngine;
using System.Collections.Generic;
// --- Station Prefab을 랜덤하게 생성하는 스포너 역할 클래스 ---
public class StationManager : MonoBehaviour
{
    public Station[] prefabs;

    [Header("스폰 시간 조정")] // To Do: 추후 실제 게임에 맞게 느리게 조정해야함
    [SerializeField] private float minRandomTime = 5f;
    [SerializeField] private float maxRandomTime = 15f;
    [SerializeField] private float maxScaleTime = 300f; // 스폰 시간이 빨라질 게임 시간 기준 (5분)
    [SerializeField] private float minIntervalAtPeak = 2f; // 5분 지났을 때 최소 스폰 간격
    [SerializeField] private float maxIntervalAtPeak = 5f; // 5분 지났을 때 최대 스폰 간격
    private float minRadius;
    private float timer = 0f;
    private float gameTime = 0f;
    private float offset = 0.5f;
    private List<Station> exisitingStations;
    public List<Station> ExisitingStations => exisitingStations;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        exisitingStations = new List<Station>();

        float screenHeight = cam.orthographicSize * 2f;
        minRadius = screenHeight * 0.15f;
    } 
    private void Start()
    {
        RandomSpawn();
        timer = Random.Range(minRandomTime, maxRandomTime);
    }

    private void Update()
    {
        gameTime += Time.deltaTime;
        
        // 게임 초반에는 더 빨리 생성되도록 함 (존재하는 역이 3개 미만일 때만)
        float t = Mathf.Clamp01(exisitingStations.Count / 3f);
        float speedOffset = Mathf.Lerp(10f, 1f, t);

        timer -= Time.deltaTime * speedOffset;

        if (timer <= 0f)
        {
            //Debug.Log("[시간 초과] 역 생성됨");
            RandomSpawn();
            float difficultyT = Mathf.Clamp01(gameTime / maxScaleTime);
            float scaleMin = Mathf.Lerp(minRandomTime, minIntervalAtPeak, difficultyT);
            float scaleMax = Mathf.Lerp(maxRandomTime, maxIntervalAtPeak, difficultyT);
            timer = Random.Range(scaleMin, scaleMax);
            //Debug.Log($"[스폰] gameTime = {gameTime:F1}s | diffucultyT = {difficultyT:F2} | 다음 간격 = {timer:F2}");
        }
    }

    // --- 한 번 실행될 때 승강장 하나만 랜덤 생성 ---
    private void RandomSpawn()
    {
        if (prefabs == null || prefabs.Length == 0) 
        {
            //Debug.Log("[Null] 프리팹 없음");
            return;
        }

        int range = prefabs.Length;
        var station = Instantiate(prefabs[Random.Range(0, range)]);

        Vector3 stationPos = GetRandomScreenPos();
        while (!IsValidPosition(stationPos))
        {
            stationPos = GetRandomScreenPos();
        }

        station.transform.position = stationPos;
        exisitingStations.Add(station);
    }

    // --- 승강장 위치 랜덤하게 생성 ---
    private Vector3 GetRandomScreenPos()
    {
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        float x = Random.Range(bottomLeft.x + offset, topRight.x - offset);
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
