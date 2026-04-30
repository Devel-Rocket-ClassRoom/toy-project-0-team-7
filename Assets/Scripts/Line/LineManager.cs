using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    public Line linePrefab;
    private Line line_onMaking;
    public bool IsValidLine => line_onMaking.stations.Count > 1;

    private LineRenderer lineRenderer;

    private List<Line> lines = new();

    public void StartLine(RaycastHit2D hit, Vector3 pos)
    {
        line_onMaking = Instantiate(linePrefab, transform);
        line_onMaking.AddStationEnd(hit.collider.gameObject.GetComponent<Station>());

        lineRenderer = line_onMaking.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, pos);
    }

    public void FixLine()
    {
        line_onMaking.Init();
        AddLine(line_onMaking);
        line_onMaking = null;
    }

    public void CancelLine()
    {
        Destroy(line_onMaking.gameObject);
        line_onMaking = null;
    }

    public void UpdatePreviewPoint(Vector3 previewPoint)
    {
        lineRenderer.SetPosition(line_onMaking.stations.Count, previewPoint);
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