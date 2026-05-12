using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class Train : MonoBehaviour
{
    public enum TrainDirection
    {
        Forward,
        Backward,
    }

    public TrainDirection direction = TrainDirection.Forward;

    private GameManager gm;

    public Line myLine;
    public int lineId;

    // --- carriage 연결 관련 ---
    public int capacity = 6;
    public int carriageCapacity = 6;
    private List<GameObject> attachedCarriages = new List<GameObject>();
    private List<Vector3> positionHistory = new List<Vector3>(); // 위치 기록용
    public const int MAX_CARRIAGE_COUNT = 2;
    public int CarriageCount => attachedCarriages.Count;

    // --- 자산 삭제 관련 필드 ---
    public bool isHighSpeedTrain = false;
    public IReadOnlyList<GameObject> AttachedCarriages => attachedCarriages;

    public float rotationSpeed = 180f;

    public Vector3 startPos; //출발 위치 기록용
    public int targetStationIndex = 0;
    private int waypointTargetIndex = 0;
    private List<Station> path;
    private List<Vector3> routeWaypoints = new List<Vector3>(); // 라인에서 받아올 경로

    private bool isShorteningPending = false; // 노선 단축 예약 플래그
    private List<Station> pendingStations;
    private List<Vector3> pendingWaypoints;
    private bool isStopping = false;
    private bool departedFromStop = false; // 정차 후 출발했는지 여부

    public List<Passenger> passengers = new List<Passenger>();
    public Transform[] passengerSlots;
    public GameObject passengerIconPrefab;
    public Sprite[] passengerIconSprites;
    private List<GameObject> passengerIcons = new List<GameObject>();

    private Vector3 lastDirection = Vector3.right;

    [Header("Movement Settings")]
    public float maxSpeed = 2.5f;
    public float minSpeed = 0.5f;
    public float accelerationDist = 1.7f; // 가속 구간 거리
    public float decelerationDist = 1.7f; // 감속 구간 거리

    private LineRenderer lr;
    Color color;
    Color showColor;

    private void Awake()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        lr = GetComponent<LineRenderer>();
    }

    //열차 승객 시각화. 초기 6개 슬릇 생성후 Active로 관리
    public void Init()
    {
        for (int i = 0; i < capacity; i++)
        {
            GameObject icon = Instantiate(passengerIconPrefab, passengerSlots[i]);

            color = Colors.colors[lineId];
            showColor = color;
            Color.RGBToHSV(showColor, out float h, out float s, out float v);
            showColor = Color.HSVToRGB(h, s * 0.3f, v); // 채도 30%

            var passengerColor = Color.Lerp(color, Color.white, 0.8f);
            icon.GetComponent<SpriteRenderer>().color = passengerColor;
            icon.SetActive(false);
            passengerIcons.Add(icon);
        }
    }

    private void RefreshPassengerIcons()
    {
        for (int i = 0; i < capacity; i++)
        {
            if (i < passengers.Count)
            {
                passengerIcons[i].GetComponent<SpriteRenderer>().sprite = passengerIconSprites[
                    (int)passengers[i].destination
                ];
                passengerIcons[i].SetActive(true);
            }
            else
                passengerIcons[i].SetActive(false);
        }
    }

    // 열차 경로 설정 및 열차 생성 위치 초기화
    public void SetPath(List<Station> stations, List<Vector3> waypoints, bool isInit = false)
    {
        if (isInit)
        {
            path = new List<Station>(stations);
            routeWaypoints = new List<Vector3>(waypoints);
            transform.position = routeWaypoints[0];
            waypointTargetIndex = 1;
            targetStationIndex = 0;

            if (routeWaypoints.Count > 1)
            {
                lastDirection = (routeWaypoints[1] - routeWaypoints[0]).normalized;
                float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            return;
        }

        Vector3 currentTargetWayPoint = routeWaypoints[waypointTargetIndex];
        bool foundMatch = false;

        for (int i = 0; i < waypoints.Count; ++i)
        {
            if (currentTargetWayPoint == waypoints[i])
            {
                foundMatch = true;
                break;
            }
        }

        if (!foundMatch) // 노선 삭제 했을때
        {
            isShorteningPending = true; //새로운 경로 대기
            pendingStations = new List<Station>(stations);
            pendingWaypoints = new List<Vector3>(waypoints);

            foreach (var p in passengers)
            {
                int dir = direction == TrainDirection.Forward ? 1 : -1;
                int myDist = BFSDistance(p.destination, targetStationIndex + dir, dir);

                if (myDist == int.MaxValue) // 현재 방향으로 갈 수 없으면
                {
                    // 다음 정차역에서 내림
                    int safeIndex = Mathf.Clamp(targetStationIndex, 0, path.Count - 1);
                    p.transferStation = path[safeIndex];
                }
                else
                {
                    p.transferStation = FindTransferStation(p);
                }
            }
            return;
        }
        path = new List<Station>(stations);
        routeWaypoints = new List<Vector3>(waypoints);

        for (int i = 0; i < routeWaypoints.Count; ++i)
        {
            if (currentTargetWayPoint == routeWaypoints[i])
            {
                waypointTargetIndex = i;
                break;
            }
        }
        foreach (var p in passengers)
        {
            int dir = direction == TrainDirection.Forward ? 1 : -1;
            int myDist = BFSDistance(p.destination, targetStationIndex + dir, dir);

            if (myDist == int.MaxValue) // 현재 방향으로 갈 수 없으면
            {
                // 다음 정차역에서 내림
                int safeIndex = Mathf.Clamp(targetStationIndex, 0, path.Count - 1);
                p.transferStation = path[safeIndex];
            }
            else
            {
                p.transferStation = FindTransferStation(p);
            }
        }
    }

    public void Move()
    {
        // 라인 그리기
        lr.positionCount = routeWaypoints.Count;

        for (int i = 0; i < routeWaypoints.Count; i++)
        {
            lr.SetPosition(i, routeWaypoints[i]);
        }
        lr.startColor = showColor;
        lr.endColor = showColor;
        // 라인 그리기

        if (isStopping || routeWaypoints.Count == 0)
            return;

        Vector3 targetPos = routeWaypoints[waypointTargetIndex];
        //Station currentStation = path[targetStationIndex];

        float remainingDistance = Vector3.Distance(transform.position, targetPos); //남은거리
        float traveledDistance = Vector3.Distance(startPos, transform.position); //달린거리

        bool isNextAStation = isStationWaypoint(waypointTargetIndex);
        Station nextStation = isNextAStation ? GetStationAtWaypoint(waypointTargetIndex) : null;
        //감속 판정 - 이번 역이 종점 이거나 정차해야 하는 역이면 감속 준비
        bool shouldStopHere =
            isNextAStation
            && nextStation != null
            && (IsTerminalStation(nextStation) || ShouldStopAtStation(nextStation));

        float currentSpeed = maxSpeed;
        //속도 조절 로직. 느리게 출발해서 중간부분은 최고속도 유지하고 도착할때쯤에는 다시 느리게 이동
        if (shouldStopHere && remainingDistance < decelerationDist)
        {
            //도착할 역에 멈출 예정일 때만 감속 로직 실행
            float t = remainingDistance / decelerationDist;
            currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
        }
        //이전 역에서 정차 후 출발이면 가속 실행
        else if (departedFromStop && traveledDistance < accelerationDist)
        {
            float t = traveledDistance / accelerationDist;
            currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
            if (traveledDistance >= accelerationDist)
                departedFromStop = false;
        }
        //실제 이동
        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir != Vector3.zero)
            lastDirection = dir;

        float newAngle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, newAngle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            currentSpeed * Time.deltaTime
        );
        if (remainingDistance < 0.05f)
        {
            transform.position = targetPos;
            if (isShorteningPending) //노선 일부 삭제했을때
            {
                int mergeIdx = -1;
                for (int i = 0; i < pendingWaypoints.Count; i++)
                {
                    if (Vector3.Distance(transform.position, pendingWaypoints[i]) < 0.05f)
                    {
                        mergeIdx = i;
                        break;
                    }
                }

                if (mergeIdx >= 0)
                {
                    isShorteningPending = false;
                    path = pendingStations;
                    routeWaypoints = pendingWaypoints;

                    waypointTargetIndex = mergeIdx;

                    startPos = transform.position;
                    targetStationIndex = waypointTargetIndex / 2;

                    //lr.positionCount = 0; // 잔상 제거
                    return;
                }

                // 겹치는 waypoint 없음 + 승객 전원 하차 완료 - 새 경로의 가장 가까운 waypoint로 이동
                if (passengers.Count == 0)
                {
                    isShorteningPending = false;
                    path = pendingStations;
                    routeWaypoints = pendingWaypoints;

                    int nearestIdx = 0;
                    float nearestDist = float.MaxValue;
                    for (int i = 0; i < routeWaypoints.Count; i++)
                    {
                        float d = Vector3.Distance(transform.position, routeWaypoints[i]);
                        if (d < nearestDist)
                        {
                            nearestDist = d;
                            nearestIdx = i;
                        }
                    }

                    transform.position = routeWaypoints[nearestIdx];

                    bool isNewCircular = myLine != null && myLine.isCircular;
                    if (!isNewCircular && nearestIdx >= routeWaypoints.Count - 1)
                    {
                        direction = TrainDirection.Backward;
                        waypointTargetIndex = nearestIdx - 1;
                    }
                    else if (!isNewCircular && nearestIdx == 0)
                    {
                        direction = TrainDirection.Forward;
                        waypointTargetIndex = 1;
                    }
                    else
                    {
                        direction = TrainDirection.Forward;
                        waypointTargetIndex = (nearestIdx + 1) % routeWaypoints.Count;
                    }

                    targetStationIndex = waypointTargetIndex / 2;
                    startPos = transform.position;
                    positionHistory.Clear();

                    if (routeWaypoints.Count > 1)
                    {
                        Vector3 nextPos = routeWaypoints[waypointTargetIndex];
                        lastDirection = (nextPos - transform.position).normalized;
                        float lastAngle =
                            Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(0f, 0f, lastAngle);
                    }
                    return;
                }
            }
            if (shouldStopHere)
            {
                //정차 및 승하차 프로세스 시작
                targetStationIndex = GetStationIndex(nextStation);
                StartCoroutine(CoStationProcessRoutine());
            }
            else
            {
                AdvanceWaypoint();
            }
        }

        if (attachedCarriages.Count > 0)
        {
            RecordPosition();
            UpdateCarriagePositions();
        }
    }

    // waypoint 인덱스 진행 (방향 포함)
    public void AdvanceWaypoint()
    {
        startPos = transform.position;
        bool isCircularLine = (myLine != null) && myLine.isCircular;
        // 방향에 따라 다음 타겟 waypoint 결정
        if (direction == TrainDirection.Forward)
        {
            if (waypointTargetIndex < routeWaypoints.Count - 1)
            {
                waypointTargetIndex++;
            }
            else if (isCircularLine)
            {
                waypointTargetIndex = 0;
            }
            else // 다음역 없으면 방향 전환
            {
                direction = TrainDirection.Backward;
                waypointTargetIndex--;
            }
        }
        else
        {
            if (waypointTargetIndex > 0)
            {
                waypointTargetIndex--;
            }
            else if (isCircularLine)
            {
                waypointTargetIndex = routeWaypoints.Count - 1;
            }
            else
            {
                direction = TrainDirection.Forward;
                waypointTargetIndex++;
            }
        }
    }

    public IEnumerator CoStationProcessRoutine()
    {
        isStopping = true;

        Station currentStation = path[targetStationIndex];

        if (currentStation != null)
        {
            bool isTerminal = IsTerminalStation(currentStation);
            if (isTerminal)
            {
                UpdateDirection();

                //정차 중에 미리 반대 방향으로 lastDirection 설정
                lastDirection = -lastDirection;
                float newAngle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
            }

            Debug.Log(
                $"<color=yellow>{currentStation.name} 정차 중...</color>승강장 개수: {path.Count}"
            );
            //내릴 승객 처리
            HandleAlighting(currentStation);

            yield return new WaitForSeconds(0.5f);

            //탈 승객 처리
            HandleBoarding(currentStation);
        }

        float stopTime = currentStation.IsInterchange ? 0.6f : 1.2f;
        Debug.Log(
            $"[열차 정차 시간] 교차역: {currentStation.IsInterchange} / 정차 시간: {stopTime}"
        );
        yield return new WaitForSeconds(stopTime);

        AdvanceWaypoint();
        departedFromStop = true;
        isStopping = false;
    }

    public void HandleBoarding(Station station)
    {
        int i = 0;
        while (i < station.waitingPassengers.Count)
        {
            if (passengers.Count >= capacity)
                break;

            Passenger p = station.waitingPassengers[i];

            //노선에 목적이 역이 포함 되면 탑승
            if (CanBoard(p))
            {
                p.blockedLineId = -1; // 초기화
                p.transferStation = FindTransferStation(p);

                station.RemovePassenger(p);

                //열차 승객 리스트 추가
                p.State = PassengerState.OnTrain;
                passengers.Add(p);

                p.gameObject.SetActive(false); // 대기 중인 승객 비활성화
                Debug.Log(
                    $"승객 탑승! 목적지: {p.destination} (열차 잔여석: {capacity - passengers.Count})"
                );
            }
            else
            {
                i++;
            }
        }
        RefreshPassengerIcons();
    }

    public void HandleAlighting(Station station)
    {
        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            var p = passengers[i];
            if (p.destination == station.Shape)
            {
                p.State = PassengerState.Arrived;

                Score.score++;
                gm.UpdateUIText();

                Destroy(p.gameObject);
                passengers.RemoveAt(i);

                Debug.Log(
                    $"<color=green>[하차 완료]</color> 목적지 {station.Shape} 도착! 점수 +1 (열차 잔여석: {capacity - passengers.Count})"
                );
            }
            //환승하는경우
            else if (p.transferStation == station)
            {
                p.transferStation = null;

                p.State = PassengerState.Waiting;
                Passenger newP = station.AddPasssenger(p.destination);
                if (newP != null)
                    newP.blockedLineId = lineId;

                Destroy(p.gameObject);
                passengers.RemoveAt(i);
            }
        }
        RefreshPassengerIcons();
    }

    public int BFSDistance(StationType dest, int currentIndex, int dir)
    {
        if (currentIndex < 0 || currentIndex >= path.Count)
            return int.MaxValue;

        HashSet<Station> visitedStations = new();
        // (역, 거리, 노선ID, 노선에서의 인덱스) 저장
        Queue<(Station station, int dist, Line line, int lineIndex)> queue = new();

        // 시작역 추가
        queue.Enqueue((path[currentIndex], 0, myLine, currentIndex));

        while (queue.Count > 0)
        {
            var (current, dist, currentLine, currentLineIndex) = queue.Dequeue();
            if (visitedStations.Contains(current))
                continue;
            visitedStations.Add(current);

            if (current.Shape == dest)
                return dist;

            // 현재 노선에서 다음 역 추가 (한 역씩만)
            if (currentLine.isCircular)
            {
                int nextIdx =
                    (currentLineIndex + dir + currentLine.stations.Count)
                    % currentLine.stations.Count;
                var nextStation = currentLine.stations[nextIdx];
                if (!visitedStations.Contains(nextStation))
                    queue.Enqueue((nextStation, dist + 1, currentLine, nextIdx));
            }
            else
            {
                int nextIdx = currentLineIndex + dir;
                if (nextIdx >= 0 && nextIdx < currentLine.stations.Count)
                {
                    var nextStation = currentLine.stations[nextIdx];
                    if (!visitedStations.Contains(nextStation))
                        queue.Enqueue((nextStation, dist + 1, currentLine, nextIdx));
                }
                // 반대 방향도 탐색 (환승 후엔 양방향)
                int prevIdx = currentLineIndex - dir;
                if (
                    currentLine.lineId != lineId
                    && prevIdx >= 0
                    && prevIdx < currentLine.stations.Count
                )
                {
                    var prevStation = currentLine.stations[prevIdx];
                    if (!visitedStations.Contains(prevStation))
                        queue.Enqueue((prevStation, dist + 1, currentLine, prevIdx));
                }
            }

            // 환승 노선 진입 (현재 역에서 다른 노선으로)
            foreach (var line in current.lines)
            {
                if (line.lineId == currentLine.lineId)
                    continue;
                int transferIdx = line.stations.IndexOf(current);
                if (transferIdx < 0)
                    continue;
                // 환승역 자체는 이미 visited 처리됐으니 양방향 다음 역만 추가
                foreach (int transferDir in new[] { 1, -1 })
                {
                    if (line.isCircular)
                    {
                        int nextIdx =
                            (transferIdx + transferDir + line.stations.Count) % line.stations.Count;
                        var nextStation = line.stations[nextIdx];
                        if (!visitedStations.Contains(nextStation))
                            queue.Enqueue((nextStation, dist + 1, line, nextIdx));
                    }
                    else
                    {
                        int nextIdx = transferIdx + transferDir;
                        if (nextIdx >= 0 && nextIdx < line.stations.Count)
                        {
                            var nextStation = line.stations[nextIdx];
                            if (!visitedStations.Contains(nextStation))
                                queue.Enqueue((nextStation, dist + 1, line, nextIdx));
                        }
                    }
                }
            }
        }

        return int.MaxValue;
    }

    public bool CanBoard(Passenger p)
    {
        if (p.blockedLineId == lineId)
            return false;

        if (myLine != null && myLine.isCircular)
        {
            return BFSDistance(p.destination, targetStationIndex, 1) != int.MaxValue;
        }

        var dir = direction == TrainDirection.Forward ? 1 : -1;
        int nextIndex = Mathf.Clamp(targetStationIndex + dir, 0, path.Count - 1);
        int myDist = BFSDistance(p.destination, nextIndex, dir);

        return myDist != int.MaxValue;
    }

    public bool NeedsTransfer(Passenger p, Station station)
    {
        bool notOnCurrentLine = !path.Any(s => s.Shape == p.destination); // 현재 노선에 목적지 없음
        bool canReachViaTransfer =
            BFS(p.destination, path.IndexOf(station), 1)
            || BFS(p.destination, path.IndexOf(station), -1); // 이 역에 내리면 목적지에 갈 수 있음

        return notOnCurrentLine && canReachViaTransfer;
    }

    public bool BFS(StationType dest, int currentIndex, int dir)
    {
        HashSet<Station> visitedStations = new();
        Queue<Station> queue = new();

        if (myLine.isCircular)
        {
            int i = currentIndex;
            do
            {
                queue.Enqueue(path[i]);
                i = (i + dir + path.Count) % path.Count;
            } while (i != currentIndex);
        }
        else
        {
            for (int i = currentIndex; i >= 0 && i < path.Count; i += dir)
            {
                queue.Enqueue(path[i]);
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visitedStations.Contains(current))
                continue;
            visitedStations.Add(current);

            if (current.Shape == dest)
                return true;

            foreach (var line in current.lines) // 이곳을 지나는 모든 노선
            {
                if (line.lineId == lineId)
                    continue;
                foreach (var station in line.stations) // 노선에 속하는 모든 역
                {
                    if (!visitedStations.Contains(station))
                        queue.Enqueue(station); // 환승 가능한 역 추가
                }
            }
        }

        return false; // 경로 못 찾음
    }

    // 현재 역에 정차 해야하는지 검사
    private bool ShouldStopAtStation(Station station)
    {
        foreach (var p in passengers)
        {
            if (p.destination == station.Shape)
                return true;
        }
        foreach (var p in station.waitingPassengers)
        {
            if (CanBoard(p))
                return true;
        }
        return false;
    }

    public Station FindTransferStation(Passenger p)
    {
        int dir = direction == TrainDirection.Forward ? 1 : -1;
        //int startIndex = targetStationIndex + dir;
        int startIndex = Mathf.Clamp(targetStationIndex + dir, 0, path.Count - 1);

        //if (startIndex < 0 || startIndex >= path.Count) return null;

        // 현재 노선에서 방향 기준으로 순회
        for (int i = startIndex; i >= 0 && i < path.Count; i += dir)
        {
            var station = path[i];

            // 직통으로 목적지 있으면 환승 필요 없음
            if (station.Shape == p.destination)
                return null;

            // 이 역에서 환승하면 목적지 갈 수 있으면 환승역으로 지정
            foreach (var line in station.lines)
            {
                if (line.lineId == lineId)
                    continue;
                if (line.stations.Any(s => s.Shape == p.destination))
                    return station;
            }
        }

        // 현재 방향으로 못 찾으면 반대 방향도 탐색
        for (int i = startIndex - dir; i >= 0 && i < path.Count; i -= dir)
        {
            var station = path[i];

            if (station.Shape == p.destination)
                return null;

            foreach (var line in station.lines)
            {
                if (line.lineId == lineId)
                    continue;
                if (line.stations.Any(s => s.Shape == p.destination))
                    return station;
            }
        }

        return null;
    }

    private void UpdateDirection()
    {
        if (myLine != null && myLine.isCircular)
            return;

        if (targetStationIndex == path.Count - 1)
            direction = TrainDirection.Backward;
        else if (targetStationIndex == 0)
            direction = TrainDirection.Forward;
    }

    //역 판별 헬퍼 함수들
    private bool isStationWaypoint(int index)
    {
        return index % 2 == 0;
    }

    private Station GetStationAtWaypoint(int wpindex)
    {
        int stationIndex = wpindex / 2;
        if (stationIndex < path.Count)
            return path[stationIndex];
        return null;
    }

    private int GetStationIndex(Station station)
    {
        return path.IndexOf(station);
    }

    private bool IsTerminalStation(Station station)
    {
        if (myLine != null && myLine.isCircular)
            return false;

        int idx = path.IndexOf(station);
        return idx == 0 || idx == path.Count - 1;
    }

    // --- carriage 관련 메서드 ---

    public void AttachCarriage(GameObject carriage, Transform[] carriageSlots)
    {
        attachedCarriages.Add(carriage);

        if (carriageSlots != null)
        {
            foreach (var slot in carriageSlots)
            {
                GameObject icon = Instantiate(passengerIconPrefab, slot);
                Color baseColor = Colors.colors[lineId];
                Color passengerColor = Color.Lerp(baseColor, Color.white, 0.8f);
                icon.GetComponent<SpriteRenderer>().color = passengerColor;
                icon.SetActive(false);
                passengerIcons.Add(icon);
            }
            capacity += carriageCapacity;
        }
    }

    private void RecordPosition()
    {
        if (
            positionHistory.Count == 0
            || Vector3.Distance(transform.position, positionHistory[positionHistory.Count - 1])
                > 0.02f
        )
        {
            positionHistory.Add(transform.position);
        }
    }

    private void UpdateCarriagePositions()
    {
        for (int i = 0; i < attachedCarriages.Count; i++)
        {
            float targetDist = 0.7f * (i + 1);
            float dist = 0f;

            for (int j = positionHistory.Count - 1; j > 0; j--)
            {
                float segLen = Vector3.Distance(positionHistory[j], positionHistory[j - 1]);
                if (dist + segLen >= targetDist)
                {
                    float t = (targetDist - dist) / segLen;
                    Vector3 targetPos = Vector3.Lerp(positionHistory[j], positionHistory[j - 1], t);
                    Vector3 dir = (positionHistory[j - 1] - positionHistory[j]).normalized;
                    attachedCarriages[i].transform.position = targetPos;
                    if (dir != Vector3.zero)
                    {
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        attachedCarriages[i].transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    }
                    break;
                }
                dist += segLen;
            }
        }
    }

    // --- 자산 삭제 관련 메서드 ---
}
