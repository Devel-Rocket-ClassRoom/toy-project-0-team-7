using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public bool isOnMaking = true;

    public int lineId;
    public bool isCircular = false;

    public List<Station> stations = new();  // 순서 중요
    public List<Train> trains = new();

    public List<Vector3> waypoints = new();
    private LineRenderer lr;

    public GameObject handlePrefab;
    public Handle handleStart;
    public Handle handleEnd;
    private Color color;
    public const int MAX_TRAIN_COUNT = 4; // 노선당 최대 열차 수

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        GetComponent<EdgeCollider2D>().edgeRadius = 0.2f;
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

    public void RemoveStation(int index)
    {
        stations.RemoveAt(index);
        UpdateWaypoints();
    }

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
        if (handleStart == null || handleEnd == null) return;
        if (stations.Count < 1 || waypoints.Count < 2) return;

        var dirStart = (waypoints[0] - waypoints[1]).normalized;
        handleStart.transform.position = stations[0].transform.position;
        handleStart.SetHandleDirection(dirStart);
        handleStart.SetColor(color);

        Vector3 dirEnd;
        if (isCircular)
        {
            dirEnd = (waypoints[0] - waypoints[^2]).normalized;
            handleEnd.transform.position = handleStart.transform.position;
        }
        else
        {
            dirEnd = (waypoints[^1] - waypoints[^2]).normalized;
            handleEnd.transform.position = stations[^1].transform.position;
        }
        handleEnd.SetHandleDirection(dirEnd);
        handleEnd.SetColor(color);

        handleStart.GetComponent<Collider2D>().enabled = false;
        handleStart.GetComponent<Collider2D>().enabled = true;
        handleEnd.GetComponent<Collider2D>().enabled = false;
        handleEnd.GetComponent<Collider2D>().enabled = true;
    }

    public void UpdateWaypoints()
    {
        waypoints.Clear();

        int stationCount = stations.Count;

        for (int i = 0; i < stationCount; i++)
        {
            int nextIndex = (i + 1) % stationCount;
            bool isLast = i == stationCount - 1;

            var pos = stations[i].transform.position;
            pos.z = 0f;
            waypoints.Add(pos);

            if (isLast && !isCircular) break;

            var bendPoint = GetBendPoint(stations[i].transform.position, stations[nextIndex].transform.position);
            bendPoint.z = 0f;
            waypoints.Add(bendPoint);
        }

        if (isCircular)
        {
            var firstPos = stations[0].transform.position;
            firstPos.z = 0f;
            waypoints.Add(firstPos);
        }

        if (!isOnMaking || isCircular)
        {
            lr.positionCount = waypoints.Count;
            for (int i = 0; i < waypoints.Count; i++)
                lr.SetPosition(i, waypoints[i]);
        }
        else
        {
            lr.positionCount = waypoints.Count + 2;
            for (int i = 0; i < waypoints.Count; i++)
                lr.SetPosition(i, waypoints[i]);
            lr.SetPosition(waypoints.Count, waypoints[^1]);
            lr.SetPosition(waypoints.Count + 1, waypoints[^1]);
        }

        var points = new Vector2[waypoints.Count];
        for (int i = 0; i < waypoints.Count; i++)
            points[i] = waypoints[i];

        GetComponent<EdgeCollider2D>().points = points;
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

    public int GetSegmentIndex(Vector3 clickPos)   // 클릭 지점으로 구간 인덱스 찾기
    {
        float minDist = float.MaxValue;
        int waypointSegmentIndex = 0;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            float dist = DistancePointToSegment(
                clickPos, waypoints[i], waypoints[i + 1]);

            if (dist < minDist)
            {
                minDist = dist;
                waypointSegmentIndex = i;
            }
        }

        return waypointSegmentIndex / 2;    // 역은 짝수 인덱스에 위치함 (0, 2, 4...)
    }

    private float DistancePointToSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 ap = p - a;
        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab));
        return (a + ab * t - p).magnitude;
    }
}