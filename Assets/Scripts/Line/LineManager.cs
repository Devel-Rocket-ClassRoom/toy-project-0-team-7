using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class LineManager : MonoBehaviour
{
    public Line linePrefab;
    private Line line_onMouse;
    public bool IsValidLine => line_onMouse.stations.Count > 1;

    private LineRenderer lr;

    private Line[] lines = new Line[MAX_LINES];
    private const int MAX_LINES = 7;
    private int availableLines = 3;
    private int lineCount = 0;

    public bool IsLinesFull => lineCount == availableLines;

    public Button[] lineButtons = new Button[MAX_LINES];

    private GameObject touchingHandle;

    private void Awake()
    {
        for (int i = 0; i < MAX_LINES; i++)
        {
            int index = i;
            lineButtons[i].onClick.AddListener(() => ClearLine(index));
        }
    }

    public void StartNewLine(RaycastHit2D hit, Vector3 pos)
    {
        line_onMouse = Instantiate(linePrefab, transform);

        UnityEngine.Color color = new();
        for (int i = 0; i < MAX_LINES; i++)
        {
            if (lines[i] == null)
            {
                color = Colors.colors[i];
                break;
            }
        }
        line_onMouse.SetColor(color);

        line_onMouse.AddStation(hit.collider.gameObject.GetComponent<Station>());

        lr = line_onMouse.GetComponent<LineRenderer>();
        lr.SetPosition(0, pos);
    }

    public void FixNewLine()   // 선 확정
    {
        int lineId = -1;

        for (int i = 0; i < MAX_LINES; i++)
        {
            if (lines[i] == null)
            {
                lineId = i;
                break;
            }
        }
        line_onMouse.Init(lineId);

        AddLine(line_onMouse);
        line_onMouse = null;
        lr = null;
    }

    public void CancelNewLine()    // 선 만들기 취소
    {
        Destroy(line_onMouse.gameObject);
        line_onMouse = null;
    }

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    public void StartExtendLine(RaycastHit2D handleHit, Vector3 pos)
    {
        line_onMouse = handleHit.collider.GetComponent<Handle>().line;
        lr = line_onMouse.GetComponent<LineRenderer>();

        touchingHandle = handleHit.collider.gameObject;
        touchingHandle.SetActive(false);
    }

    public void ExtendLineStart(Station station)
    {
        line_onMouse.InsertStation(station, 0);
        line_onMouse.UpdateHandles();
        line_onMouse = null;
        lr = null;
    }

    public void ExtendLineEnd(Station station)
    {
        line_onMouse.AddStation(station);
        line_onMouse.UpdateHandles();
        line_onMouse = null;
        lr = null;
    }

    public void FinishExtendLine()
    {
        line_onMouse.UpdateWaypoints();
        line_onMouse.UpdateHandles();
        lines[line_onMouse.lineId] = line_onMouse;
        line_onMouse = null;

        touchingHandle.SetActive(true);
    }

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ






    public void StartEditLine(RaycastHit2D lineHit, Vector3 pos)
    {
        line_onMouse = lineHit.collider.GetComponent<Line>();
    }

    public void AddStationInMakingLine(Station station)
    {
        if (!line_onMouse.stations.Contains(station))
        {
            line_onMouse.AddStation(station);
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
        lineCount--;
        lineButtons[index].interactable = false;
    }

    public void UnlockNextLine()
    {
        availableLines++;
        lineButtons[availableLines - 1].interactable = true;
    }

    public void UpdateStartPreviewPoint(Vector3 previewPoint)
    {
        previewPoint.z = 0f;
        var firstStation = line_onMouse.stations[0].transform.position;

        Vector3 bend = line_onMouse.GetBendPoint(firstStation, previewPoint);
        bend.z = 0f;

        lr.positionCount = line_onMouse.waypoints.Count + 2;
        lr.SetPosition(0, previewPoint);
        lr.SetPosition(1, bend);

        for (int i = 0; i < line_onMouse.waypoints.Count; i++)
            lr.SetPosition(i + 2, line_onMouse.waypoints[i]);
    }

    public void UpdateEndPreviewPoint(Vector3 previewPoint)
    {
        previewPoint.z = 0f;
        var lastStation = line_onMouse.stations[^1].transform.position;    // 마지막 역 위치

        Vector3 bend = line_onMouse.GetBendPoint(lastStation, previewPoint);
        bend.z = 0f;

        lr.positionCount = line_onMouse.waypoints.Count + 2;
        lr.SetPosition(line_onMouse.waypoints.Count, bend);
        lr.SetPosition(line_onMouse.waypoints.Count + 1, previewPoint);
    }
}