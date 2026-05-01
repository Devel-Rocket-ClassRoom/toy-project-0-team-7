using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class LineManager : MonoBehaviour
{
    public Line linePrefab;
    private Line line_onMaking;
    public bool IsValidLine => line_onMaking.stations.Count > 1;

    private LineRenderer lr;

    private Line[] lines = new Line[MAX_LINES];
    private const int MAX_LINES = 7;
    private int availableLines = 3;
    private int lineCount = 0;

    public bool IsLinesFull => lineCount == availableLines;

    private List<bool> isUsedColor = new();
    private List<bool> isUsedLine = new();

    public Button[] lineButtons = new Button[MAX_LINES];

    private void Awake()
    {
        for (int i = 0; i < MAX_LINES; i++)
        {
            isUsedColor.Add(false);
            isUsedLine.Add(false);

            int index = i;
            lineButtons[i].onClick.AddListener(() => ClearLine(index));
        }
    }

    public void StartLine(RaycastHit2D hit, Vector3 pos)
    {
        line_onMaking = Instantiate(linePrefab, transform);

        UnityEngine.Color color = new();
        for (int i = 0; i < MAX_LINES; i++)
        {
            if (!isUsedColor[i])
            {
                color = Colors.colors[i];
                isUsedColor[i] = true;
                break;
            }
        }
        line_onMaking.SetColor(color);

        line_onMaking.AddStationEnd(hit.collider.gameObject.GetComponent<Station>());

        lr = line_onMaking.GetComponent<LineRenderer>();
        lr.SetPosition(0, pos);
    }

    public void FixLine()
    {
        int lineId = -1;

        for (int i = 0; i < MAX_LINES; i++)
        {
            if (!isUsedLine[i])
            {
                lineId = i;
                isUsedLine[i] = true;
                break;
            }
        }
        line_onMaking.Init(lineId);

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
        lines[line.lineId] = line;
        lineCount++;
        lineButtons[line.lineId].interactable = true;
    }

    public void ClearLine(int index)
    {
        if (lines[index] == null) return;   // 방어 코드

        Destroy(lines[index].gameObject);       
        lines[index] = null;
        isUsedColor[index] = false;
        isUsedLine[index] = false;
        lineCount--;
        lineButtons[index].interactable = false;
    }

    public void UnlockNextLine()
    {
        availableLines++;
        lineButtons[availableLines - 1].interactable = true;
    }
}