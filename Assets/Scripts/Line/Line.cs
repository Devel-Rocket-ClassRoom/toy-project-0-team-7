using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public bool isOnMaking = true;

    public int lineId;
    public List<Station> stations = new();  // 순서 중요
    public List<Train> trains = new();
    //public bool isCircular = false;

    public List<Vector3> waypoints = new();
    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void AddStationEnd(Station station)
    {
        if (stations.Count > 0)
        {
            // 이전 역과 새 역 사이의 꺾임점 계산해서 추가
            Vector3 bend = GetBendPoint(stations[^1].transform.position, station.transform.position);
            bend.z = 0f;
            waypoints.Add(bend);
        }

        var stationPos = station.transform.position;
        stationPos.z = 0f;

        waypoints.Add(stationPos);
        stations.Add(station);

        if (lr == null) lr = GetComponent<LineRenderer>(); // 방어 코드
        lr.positionCount = waypoints.Count + 2;

        for (int i = 0; i < waypoints.Count; i++)
        {
            lr.SetPosition(i, waypoints[i]);
        }

        lr.SetPosition(waypoints.Count, stationPos);
        lr.SetPosition(waypoints.Count + 1, stationPos);
    }

    public void Init(int id)
    {
        lineId = id;
        lr.positionCount = waypoints.Count;

        for (int i = 0; i < waypoints.Count; i++)
        {
            lr.SetPosition(i, waypoints[i]);
        }

        isOnMaking = false;
    }

    public void SetColor(Color color)
    {
        lr.startColor = color;
        lr.endColor = color;
    }

    public Vector3 GetBendPoint(Vector3 from, Vector3 to)
    {
        Vector3 diff = to - from;   // 방향
        float ax = Mathf.Abs(diff.x);
        float ay = Mathf.Abs(diff.y);

        if (ax > ay)
            return new Vector3(to.x - Mathf.Sign(diff.x) * ay, from.y, 0);
        else
            return new Vector3(from.x, to.y - Mathf.Sign(diff.y) * ax, 0);
    }
}