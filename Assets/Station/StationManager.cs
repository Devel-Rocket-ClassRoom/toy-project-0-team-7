using System.Collections.Generic;
using UnityEngine;

// --- Station Prefab을 랜덤하게 생성하는 스포너 역할 클래스 ---
public class StationManager : MonoBehaviour
{
    public Station[] prefabs;
    [SerializeField] private float minRandomTime = 5f;
    [SerializeField] private float maxRandomTime = 15f;
    [SerializeField] private float minRadius;
    [SerializeField] private float maxRadius;
    
    private float timer = 0f;
    private float offset = 0.5f;
    private List<Station> exisitingStations;
    public List<Station> ExisitingStations => exisitingStations;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        exisitingStations = new List<Station>();
    } 
    private void Start()
    {
        RandomSpawn();
        timer = Random.Range(minRandomTime, maxRandomTime);
    }

    private void Update()
    {
       timer -= Time.deltaTime;
       
       if (timer <= 0f)
        {
            Debug.Log("[시간 초과] 역 생성됨");
            RandomSpawn();
            timer = Random.Range(minRandomTime, maxRandomTime);
        }
    }

    // --- 한 번 실행될 때 승강장 하나만 랜덤 생성 ---
    private void RandomSpawn()
    {
        if (prefabs == null || prefabs.Length == 0) 
        {
            Debug.Log("[Null] 프리팹 없음");
            return;
        }

        int range = prefabs.Length;
        var station = Instantiate(prefabs[Random.Range(0, range)]);
        station.transform.position = GetRandomScreenPos();
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

// 이미 생성된 스테이션 리스트에서 minRadius ~ maxRadius 범위 밖이면 다시 생성하도록 해야함
/*
    private bool IsValidPosition()
    {
        
    }
*/
}
