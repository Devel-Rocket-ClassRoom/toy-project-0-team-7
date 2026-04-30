using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public bool isOnMaking = true;

    public int lineId;
    public Color color;
    public List<Station> stations = new();  // 순서 중요
    public List<Train> trains = new();
    //public bool isCircular = false;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void AddStationEnd(Station station)
    {
        if (lr == null) lr = GetComponent<LineRenderer>(); // 방어 코드
        
        stations.Add(station);
        lr.positionCount = stations.Count + 1;

        var pos = station.transform.position;
        pos.z = 0f;
        lr.SetPosition(stations.Count - 1, pos);
        lr.SetPosition(stations.Count, pos);
    }

    public void Init()
    {
        lr.positionCount = stations.Count;

        for (int i = 0; i < stations.Count; i++)
        {
            lr.SetPosition(i, stations[i].transform.position);
        }

        isOnMaking = false;
    }
}