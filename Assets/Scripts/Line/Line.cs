using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class Line : MonoBehaviour
{
    public bool isOnMaking = true;

    public int lineId;
    public List<Station> stations = new();  // 순서 중요
    public List<Train> trains = new();
    //public bool isCircular = false;

    public List<Vector3> waypoints = new();
    private LineRenderer lr;

    private EdgeCollider2D collider;

    public GameObject handlePrefab;
    private Handle handleStart;
    private Handle handleEnd;
    private Color color;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        collider = GetComponent<EdgeCollider2D>();
    }

    public void InsertStation(Station station, int index)
    {
        stations.Insert(index, station);
        UpdateWaypoints();
    }

    public void AddStation(Station station)
    {
        stations.Add(station);
        UpdateWaypoints();
    }

    //public void AddStationEnd(Station station)
    //{
    //    if (stations.Count > 0)
    //    {
    //        // 이전 역과 새 역 사이의 꺾임점 계산해서 추가
    //        Vector3 bend = GetBendPoint(stations[^1].transform.position, station.transform.position);
    //        bend.z = 0f;
    //        waypoints.Add(bend);
    //    }

    //    var stationPos = station.transform.position;
    //    stationPos.z = 0f;

    //    waypoints.Add(stationPos);
    //    stations.Add(station);

    //    if (lr == null) lr = GetComponent<LineRenderer>(); // 방어 코드
    //    lr.positionCount = waypoints.Count + 2;

    //    for (int i = 0; i < waypoints.Count; i++)
    //    {
    //        lr.SetPosition(i, waypoints[i]);
    //    }
    //    lr.SetPosition(waypoints.Count, stationPos);
    //    lr.SetPosition(waypoints.Count + 1, stationPos);
    //}

    public void Init(int id)
    {
        // 손잡이 생성
        var hs = Instantiate(handlePrefab, transform);
        handleStart = hs.GetComponent<Handle>();
        handleStart.line = this;
        handleStart.isStartHandle = true;
        var he = Instantiate(handlePrefab, transform);
        handleEnd = he.GetComponent<Handle>();
        handleEnd.line = this;
        handleEnd.isStartHandle = false;
        UpdateHandles();

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
        this.color = color;

        if (lr == null) lr = GetComponent<LineRenderer>();
        lr.startColor = color;
        lr.endColor = color;

        if (handleStart != null) handleStart.SetColor(color);
        if (handleEnd != null) handleEnd.SetColor(color);
    }

    public void UpdateHandles()
    {
        var dirStart = (waypoints[0] - waypoints[1]).normalized;
        handleStart.transform.position = stations[0].transform.position;
        handleStart.SetHandleDirection(dirStart);
        handleStart.SetColor(color);

        var dirEnd = (waypoints[^1] - waypoints[^2]).normalized;
        handleEnd.transform.position = stations[^1].transform.position;
        handleEnd.SetHandleDirection(dirEnd);
        handleEnd.SetColor(color);
    }

    public void UpdateWaypoints()
    {
        waypoints.Clear();

        for (int i = 0; i < stations.Count; i++)
        {
            if (i > 0)
            {
                Vector3 bend = GetBendPoint(
                    stations[i - 1].transform.position,
                    stations[i].transform.position);
                bend.z = 0f;    
                waypoints.Add(bend);
            }

            var pos = stations[i].transform.position;
            pos.z = 0f;
            waypoints.Add(pos);
        }

        if (isOnMaking) // 생성 중
        {
            lr.positionCount = waypoints.Count + 2;
            for (int i = 0; i < waypoints.Count; i++)
                lr.SetPosition(i, waypoints[i]);
            lr.SetPosition(waypoints.Count, waypoints[^1]);
            lr.SetPosition(waypoints.Count + 1, waypoints[^1]);
        }
        else
        {
            lr.positionCount = waypoints.Count;
            for (int i = 0; i < waypoints.Count; i++)
                lr.SetPosition(i, waypoints[i]);
        }
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