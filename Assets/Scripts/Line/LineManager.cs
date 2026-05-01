using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    public Line linePrefab;
    private Line line_onMaking;
    public bool IsValidLine => line_onMaking.stations.Count > 1;

    private LineRenderer lr;

    private List<Line> lines = new();
    private int capacity = 7;
    public bool IsLinesFull => lines.Count == capacity;

    private Color color;

    public void StartLine(RaycastHit2D hit, Vector3 pos)
    {
        line_onMaking = Instantiate(linePrefab, transform);
        line_onMaking.SetColor(Colors.colors[lines.Count]);
        line_onMaking.AddStationEnd(hit.collider.gameObject.GetComponent<Station>());

        lr = line_onMaking.GetComponent<LineRenderer>();
        lr.SetPosition(0, pos);
    }

    public void FixLine()
    {
        line_onMaking.Init();
        AddLine(line_onMaking);
        line_onMaking = null;
        lr = null;
    }

    public void CancelLine()
    {
        Destroy(line_onMaking.gameObject);
        line_onMaking = null;
    }

    public void UpdatePreviewPoint(Vector3 previewPoint)
    {
        previewPoint.z = 0f;
        var lastStation = line_onMaking.stations[^1].transform.position;    // 마지막 역 위치

        Vector3 bend = line_onMaking.GetBendPoint(lastStation, previewPoint);
        bend.z = 0f;

        lr.positionCount = line_onMaking.waypoints.Count + 2;
        lr.SetPosition(line_onMaking.waypoints.Count, bend);
        lr.SetPosition(line_onMaking.waypoints.Count + 1, previewPoint);
    }

    public void AddStationInMakingLine(Station station)
    {
        if (!line_onMaking.stations.Contains(station))
        {
            line_onMaking.AddStationEnd(station);
        }
    }

    public void AddLine(Line line)
    {
        lines.Add(line);
    }
}