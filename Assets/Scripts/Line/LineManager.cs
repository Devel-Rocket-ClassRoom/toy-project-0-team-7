using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class LineManager : MonoBehaviour
{
    public Line linePrefab;
    private Line line_onMouse;
    public bool IsValidLine => line_onMouse.stations.Count > 1;

    private LineRenderer lr;

    private Line[] lines = new Line[AssetManager.MAX_LINE_COUNT];
    private int lineCount = 0;
    private int availableLineCount = 3;

    public bool IsLinesFull => lineCount == availableLineCount;

    public Button[] lineButtons = new Button[AssetManager.MAX_LINE_COUNT];

    private GameObject touchingHandle;
    public Station stationUnderMouse;
    private int segmentIndex;

    public bool isStartHandle;

    public TrainManager trainManager;

    private void Awake()
    {
        for (int i = 0; i < AssetManager.MAX_LINE_COUNT; i++)
        {
            int index = i;
            lineButtons[i].onClick.AddListener(() => ClearLine(index));

            var colors = lineButtons[i].colors;
            colors.normalColor = new Color(0f, 0f, 0f, 0.5f);
            colors.disabledColor = colors.normalColor;
            lineButtons[i].colors = colors;
        }

        for (int i = 0; i < availableLineCount; i++)
        {
            var colors = lineButtons[i].colors;
            colors.normalColor = new Color(1, 1, 1);
            colors.disabledColor = colors.normalColor;
            lineButtons[i].colors = colors;
        }
    }

    public void StartNewLine(RaycastHit2D hit, Vector3 pos)
    {
        line_onMouse = Instantiate(linePrefab, transform);

        UnityEngine.Color color = new();
        for (int i = 0; i < AssetManager.MAX_LINE_COUNT; i++)
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

        for (int i = 0; i < AssetManager.MAX_LINE_COUNT; i++)
        {
            if (lines[i] == null)
            {
                lineId = i;
                break;
            }
        }
        line_onMouse.Init(lineId);

        if (trainManager.activeTrains.Count < trainManager.availableTrainCount)
        {
            trainManager.testStations = line_onMouse.stations;
            //trainManager.waypoints = line_onMouse.waypoints;
            line_onMouse.trains.Add(trainManager.SpawnTrain(lineId));
        }

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

    public void ToggleStationInExtendLine(Station station, bool isStart)
    {
        if (station == stationUnderMouse) return;
        stationUnderMouse = station;

        // 있던 역 제외
        if (isStart && station == line_onMouse.stations[0] && line_onMouse.stations.Count > 1)
            line_onMouse.stations.RemoveAt(0);
        else if (!isStart && station == line_onMouse.stations[^1] && line_onMouse.stations.Count > 1)
            line_onMouse.stations.RemoveAt(line_onMouse.stations.Count - 1);
        
        // 없던 역 추가
        else if (isStart && !line_onMouse.stations.Contains(station))   // 시작 핸들
            line_onMouse.InsertStation(station, 0);
        else if (!isStart && !line_onMouse.stations.Contains(station))   // 끝 핸들
            line_onMouse.AddStation(station);

        line_onMouse.UpdateWaypoints();
        line_onMouse.UpdateHandles();
    }

    public void FinishExtendLine()
    {
        if (line_onMouse == null) return;

        if (line_onMouse.stations.Count < 2)
            ClearLine(line_onMouse.lineId);
        else
        {
            line_onMouse.UpdateWaypoints();
            line_onMouse.UpdateHandles();
        }

        foreach (var train in trainManager.activeTrains)
        {
            if (train.lineId == line_onMouse.lineId)
                train.SetPath(line_onMouse.stations);
        }
        line_onMouse = null;

        if (touchingHandle != null)
        {
            touchingHandle.SetActive(true);
            touchingHandle = null;
        }

        stationUnderMouse = null;
        lr = null;
    }

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    public void StartEditLine(RaycastHit2D lineHit, Vector3 pos)
    {
        line_onMouse = lineHit.collider.GetComponent<Line>();
        lr = line_onMouse.GetComponent<LineRenderer>();
        segmentIndex = line_onMouse.GetSegmentIndex(pos);
    }

    public bool ToggleStationInEditLine(Station station)
    {
        if (station == stationUnderMouse) return false;
        stationUnderMouse = station;

        if (line_onMouse.stations.Contains(station) && line_onMouse.stations.Count > 1)    // 포함된 역
        {
            int index = line_onMouse.stations.IndexOf(station);
            bool isEndStation = (index == 0 || index == line_onMouse.stations.Count - 1);

            line_onMouse.stations.RemoveAt(index);
            line_onMouse.UpdateWaypoints();

            if (index <= segmentIndex) segmentIndex--;
            segmentIndex = Mathf.Clamp(segmentIndex, 0, line_onMouse.stations.Count - 2);

            if (isEndStation)
            {
                isStartHandle = (index == 0); // LineManager 멤버 변수
                return true; // ExtendLine으로 전환 신호
            }
        }
        else if (!line_onMouse.stations.Contains(station))
        {
            line_onMouse.InsertStation(station, segmentIndex + 1);
            segmentIndex++;
        }

        return false;
    }

    public void FinishEditLine()
    {
        if (line_onMouse == null) return;

        if (line_onMouse.stations.Count < 2)
            ClearLine(line_onMouse.lineId);
        else
        {
            line_onMouse.UpdateWaypoints();
            line_onMouse.UpdateHandles();
        }

        foreach (var train in trainManager.activeTrains)
        {
            if (train.lineId == line_onMouse.lineId)
                train.SetPath(line_onMouse.stations);
        }
        line_onMouse = null;
        stationUnderMouse = null;
        lr = null;
    }

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

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
        
        var rt = lineButtons[line.lineId].GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);
    }

    public void ClearLine(int index)
    {
        if (lines[index] == null) return;   // 방어 코드

        foreach(var train in lines[index].trains)
        {
            trainManager.RemoveTrain(train);
        }

        Destroy(lines[index].gameObject);       
        lines[index] = null;
        lineCount--;
        lineButtons[index].interactable = false;

        var rt = lineButtons[index].GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(30, 30);
    }

    public void AddAvailableLine()
    {
        availableLineCount++;

        var colors = lineButtons[availableLineCount - 1].colors;
        colors.normalColor = new Color(1, 1, 1);
        colors.disabledColor = colors.normalColor;
        lineButtons[availableLineCount - 1].colors = colors;  
    }

    public void HideHandle(bool isStart)
    {
        var handle = isStart ? line_onMouse.handleStart.gameObject : line_onMouse.handleEnd.gameObject;
        touchingHandle = handle;
        touchingHandle.SetActive(false);
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

    public void UpdateEditPreviewPoint(Vector3 previewPoint)
    {
        previewPoint.z = 0f;

        var from = line_onMouse.stations[segmentIndex].transform.position;
        var to = line_onMouse.stations[segmentIndex + 1].transform.position;

        Vector3 bend1 = line_onMouse.GetBendPoint(from, previewPoint);
        Vector3 bend2 = line_onMouse.GetBendPoint(previewPoint, to);
        bend1.z = 0f;
        bend2.z = 0f;

        var tempWaypoints = new List<Vector3>();
        for (int i = 0; i < line_onMouse.stations.Count; i++)
        {
            if (i > 0)
            {
                if (i == segmentIndex + 1)  // 추가 변곡점(3점)이 필요한 구간
                {
                    tempWaypoints.Add(bend1);
                    tempWaypoints.Add(previewPoint);
                    tempWaypoints.Add(bend2);
                }

                else    // 일반 waypoints 구할 때처럼
                {
                    var a = line_onMouse.stations[i - 1].transform.position;
                    var b = line_onMouse.stations[i].transform.position;
                    tempWaypoints.Add(line_onMouse.GetBendPoint(a, b));
                }
            }

            // 승강장 위치 추가
            var pos = line_onMouse.stations[i].transform.position;
            pos.z = 0f;
            tempWaypoints.Add(pos);
        }

        // 실제 선에 적용
        lr.positionCount = tempWaypoints.Count;
        for (int i = 0; i < tempWaypoints.Count; i++)
            lr.SetPosition(i, tempWaypoints[i]);
    }
}