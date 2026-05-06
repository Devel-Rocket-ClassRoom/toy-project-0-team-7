using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LineManager : MonoBehaviour
{
    public Line linePrefab;
    private Line line_onMouse;
    public bool IsValidLine => line_onMouse.stations.Count > 1;
    public bool IsCircular => line_onMouse.isCircular;

    private LineRenderer lr;

    private Line[] lines = new Line[AssetManager.MAX_LINE_COUNT];
    private int lineCount = 0;
    private int availableLineCount = 3;

    public bool IsLinesFull => lineCount == availableLineCount;
    public bool CantAddLine => availableLineCount == AssetManager.MAX_LINE_COUNT;

    public Button[] lineButtons = new Button[AssetManager.MAX_LINE_COUNT];

    private GameObject touchingHandle;
    public Station stationUnderMouse;
    private int segmentIndex;

    public bool isStartHandle;

    // 자산
    public TrainManager trainManager;
    public StationManager stationManager;
    public AssetManager assetManager;

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

    public bool ToggleStationInNewLine(Station station)
    {
        if (station == stationUnderMouse) return false;
        stationUnderMouse = station;

        // 있던 역 제외
        if (station == line_onMouse.stations[^1] && line_onMouse.stations.Count > 1)
        {
            line_onMouse.RemoveStation(line_onMouse.stations.Count - 1);

            if (line_onMouse.isCircular && line_onMouse.stations.Count < 3)
            {
                line_onMouse.isCircular = false;
                line_onMouse.UpdateWaypoints();
                line_onMouse.UpdateHandles();
            }
        }

        // 역 추가
        else
        {
            if (station == line_onMouse.stations[0] && line_onMouse.stations.Count >= 3)
            {
                line_onMouse.isCircular = true; // 순환 노선 설정
                line_onMouse.UpdateWaypoints();
                line_onMouse.UpdateHandles();
                isStartHandle = false;
                return true;
            }

            else if (!line_onMouse.stations.Contains(station))
            {
                line_onMouse.AddStation(station);   // 포함되지 않은 역은 추가
            }
        }

        line_onMouse.UpdateWaypoints();
        line_onMouse.UpdateHandles();
        return false;
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
            trainManager.Stations = line_onMouse.stations;            
            line_onMouse.trains.Add(trainManager.SpawnTrain(lineId, line_onMouse.waypoints));
            assetManager.UpdateTrainUI(); // 열차 자산 UI 업데이트
        }

        foreach (var station in line_onMouse.stations)
        {
            station.lines.Add(line_onMouse); // 역에 노선 참조 추가
        }

        RevealHandles();

        if (line_onMouse.isCircular)
            line_onMouse.handleStart.gameObject.SetActive(false);

        line_onMouse.UpdateHandles();
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

        if (line_onMouse.isCircular)
        {
            line_onMouse.isCircular = false;
            line_onMouse.UpdateWaypoints();
            RevealHandles();
            isStartHandle = false;
        }

        touchingHandle = handleHit.collider.gameObject;
        touchingHandle.SetActive(false);
    }

    public bool ToggleStationInExtendLine(Station station, bool isStart)
    {
        if (station == stationUnderMouse) return false;
        stationUnderMouse = station;

        // 있던 역 제외
        if (isStart && station == line_onMouse.stations[0] && line_onMouse.stations.Count > 1)
            line_onMouse.RemoveStation(0);
        else if (!isStart && station == line_onMouse.stations[^1] && line_onMouse.stations.Count > 1)
            line_onMouse.RemoveStation(line_onMouse.stations.Count - 1);

        // 없던 역 추가
        else if (isStart)   // 시작 핸들
        {
            if (!line_onMouse.isCircular)
            {
                if (station == line_onMouse.stations[line_onMouse.stations.Count -1])
                {
                    var lastStation = line_onMouse.stations[^1];
                    line_onMouse.RemoveStation(line_onMouse.stations.Count -1);
                    line_onMouse.InsertStation(lastStation, 0);

                    line_onMouse.isCircular = true; // 순환 노선 설정
                    line_onMouse.UpdateWaypoints();
                    line_onMouse.UpdateHandles();
                    return true;
                }

                else if (!line_onMouse.stations.Contains(station))
                {
                    line_onMouse.InsertStation(station, 0);   // 포함되지 않은 역은 추가
                }
            }
        }

        else   // 끝 핸들
        {
            if (!line_onMouse.isCircular)
            {
                if (station == line_onMouse.stations[0])
                {
                    line_onMouse.isCircular = true; // 순환 노선 설정
                    line_onMouse.UpdateWaypoints();
                    line_onMouse.UpdateHandles();
                    return true;
                }

                else if (!line_onMouse.stations.Contains(station))
                {
                    line_onMouse.AddStation(station);   // 포함되지 않은 역은 추가
                }
            }
        }

        line_onMouse.UpdateWaypoints();
        line_onMouse.UpdateHandles();
        return false;
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

        // 기존 연결 전부 제거
        foreach (var station in line_onMouse.stations)
            station.lines.Remove(line_onMouse);

        // 현재 stations 기준으로 다시 연결
        foreach (var station in line_onMouse.stations)
            station.lines.Add(line_onMouse);

        foreach (var train in trainManager.activeTrains)
        {
            if (train.lineId == line_onMouse.lineId)
                train.SetPath(line_onMouse.stations, line_onMouse.waypoints);
        }

        if (touchingHandle != null)
        {
            touchingHandle.SetActive(true);
            touchingHandle = null;
        }

        RevealHandles();

        if (line_onMouse.isCircular)
            line_onMouse.handleStart.gameObject.SetActive(false);

        line_onMouse = null;
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

            line_onMouse.RemoveStation(index);

            if (line_onMouse.isCircular && line_onMouse.stations.Count < 3)
            {
                line_onMouse.isCircular = false;
                line_onMouse.UpdateWaypoints();
                line_onMouse.UpdateHandles();
                // 핸들 둘 활성화
                line_onMouse.handleStart.gameObject.SetActive(true);
                line_onMouse.handleEnd.gameObject.SetActive(true);
            }

            if (index <= segmentIndex) segmentIndex--;
            segmentIndex = Mathf.Clamp(segmentIndex, 0, line_onMouse.stations.Count - 2);

            if (isEndStation)
            {
                isStartHandle = index == 0; // LineManager 멤버 변수
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

        // 기존 연결 전부 제거
        foreach (var station in line_onMouse.stations)
            station.lines.Remove(line_onMouse);

        // 현재 stations 기준으로 다시 연결
        foreach (var station in line_onMouse.stations)
            station.lines.Add(line_onMouse);

        foreach (var train in trainManager.activeTrains)
        {
            if (train.lineId == line_onMouse.lineId)
                train.SetPath(line_onMouse.stations, line_onMouse.waypoints);
        } 

        line_onMouse = null;
        stationUnderMouse = null;
        lr = null;
    }

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

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

        foreach (var station in lines[index].stations)
            station.lines.Remove(lines[index]);

        foreach (var train in lines[index].trains)
            trainManager.RemoveTrain(train);
        

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

    public void RevealHandle(bool isStart)
    {
        var handle = isStart ? line_onMouse.handleStart.gameObject : line_onMouse.handleEnd.gameObject;
        handle.SetActive(true);
    }

    public void RevealHandles()
    {
        line_onMouse.handleStart.gameObject.SetActive(true);
        line_onMouse.handleEnd.gameObject.SetActive(true);
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

        Vector3 from, to;
        if (line_onMouse.isCircular && segmentIndex >= line_onMouse.stations.Count -1)
        {
            from = line_onMouse.stations[segmentIndex].transform.position;
            to = line_onMouse.stations[0].transform.position;
        }
        else
        {
            from = line_onMouse.stations[segmentIndex].transform.position;
            to = line_onMouse.stations[segmentIndex + 1].transform.position;
        }

        Vector3 bend1 = line_onMouse.GetBendPoint(from, previewPoint);
        Vector3 bend2 = line_onMouse.GetBendPoint(previewPoint, to);
        bend1.z = 0f;
        bend2.z = 0f;

        var tempWaypoints = new List<Vector3>();
        int stationCount = line_onMouse.stations.Count;

        for (int i = 0; i < stationCount; i++)
        {
            int nextIndex = (i + 1) % stationCount;
            bool isLastSegment = line_onMouse.isCircular && i == stationCount - 1;
            bool isLastNonCircular = !line_onMouse.isCircular && i == stationCount - 1;

            // 승강장 위치 추가
            var pos = line_onMouse.stations[i].transform.position;
            pos.z = 0f;
            tempWaypoints.Add(pos);

            if (isLastNonCircular) break; // 순환 노선이 아니면 마지막 역 추가 후 종료

            var nextPos = line_onMouse.stations[nextIndex].transform.position;

            if (i == segmentIndex) // 당기는 구간
            {
                tempWaypoints.Add(bend1);
                tempWaypoints.Add(previewPoint);
                tempWaypoints.Add(bend2);
            }
            else
            {
                var bendPoint = line_onMouse.GetBendPoint(pos, nextPos);
                bendPoint.z = 0f;
                tempWaypoints.Add(bendPoint);
            }
        }

        if (line_onMouse.isCircular)    // 순환 노선이면 첫번째 역 추가
        {
            var pos = line_onMouse.stations[0].transform.position;
            pos.z = 0f;
            tempWaypoints.Add(pos);
        }

        // 실제 선에 적용
        lr.positionCount = tempWaypoints.Count;
        for (int i = 0; i < tempWaypoints.Count; i++)
            lr.SetPosition(i, tempWaypoints[i]);
    }
}