using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TrainDirection
{
    Forward,
    Backward
}
public class Train : MonoBehaviour
{
    private GameManager gm;
    
    public int lineId;
    public int capacity = 6;

    public int targetStationIndex = 0;
    public float rotationSpeed = 180f;
    public Vector3 startPos; //출발 위치 기록용
    private int waypointTargetIndex = 0;
    private bool isStopping = false;
    private bool departedFromStop = false; // 정차 후 출발했는지 여부
    public List<Passenger> passengers = new List<Passenger>();
    public Transform[] passengerSlots;
    public GameObject passengerIconPrefab;
    public Sprite[] passengerIconSprites;
    private List<GameObject> passengerIcons = new List<GameObject>();

    private List<Station> path;
    private List<Vector3> routeWaypoints = new List<Vector3>(); //라인에서 받아올 경로
    public TrainDirection direction = TrainDirection.Forward;
    private Vector3 lastDirection = Vector3.right;

    [Header("Movement Settings")]
    public float maxSpeed = 2.5f;
    public float minSpeed = 0.5f;
    public float accelerationDist = 1.7f; // 가속 구간 거리
    public float decelerationDist = 1.7f; // 감속 구간 거리

    private void Awake()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    //열차 승객 시각화. 초기 6개 슬릇 생성후 Active로 관리
    public void Init()
    {
        for (int i = 0; i < capacity; i++)
        {
            GameObject icon = Instantiate(passengerIconPrefab, passengerSlots[i]);
            Color baseColor = Colors.colors[lineId];
            Color passengerColor = Color.Lerp(baseColor, Color.white, 0.8f);
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
                passengerIcons[i].GetComponent<SpriteRenderer>().sprite =
                    passengerIconSprites[(int)passengers[i].destination];
                passengerIcons[i].SetActive(true);
            }
            else
                passengerIcons[i].SetActive(false);
        }
    }
    //열차 경로 설정 및 열차 생성위치 초기화
    public void SetPath(List<Station> stations, List<Vector3> waypoints, bool isInit = false)
    {
        var prevWaypoints = routeWaypoints;
        path = stations;
        routeWaypoints = new List<Vector3>(waypoints);

        if (isInit)
        {
            transform.position = routeWaypoints[0];
            waypointTargetIndex = 1;
            targetStationIndex = 0;

            if (routeWaypoints.Count > 1)
            {
                lastDirection = (routeWaypoints[1] - routeWaypoints[0]).normalized;
                float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        else
        {
            var currentTargetWayPoint = prevWaypoints[waypointTargetIndex];

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
                    p.transferStation = path[targetStationIndex];
                }
                else
                {
                    p.transferStation = FindTransferStation(p);
                }
            }
        }
    }

    public void Move()
    {
        if (isStopping || routeWaypoints.Count == 0) return;

        Vector3 targetPos = routeWaypoints[waypointTargetIndex];
        //Station currentStation = path[targetStationIndex];

        float remainingDistance = Vector3.Distance(transform.position, targetPos); //남은거리
        float traveledDistance = Vector3.Distance(startPos, transform.position); //달린거리

        bool isNextAStation = isStationWaypoint(waypointTargetIndex);
        Station nextStation = isNextAStation ? GetStationAtWaypoint(waypointTargetIndex) : null;
        //감속 판정 - 이번 역이 종점 이거나 정차해야 하는 역이면 감속 준비
        bool shouldStopHere = isNextAStation && nextStation != null &&
            (IsTerminalStation(nextStation) || ShouldStopAtStation(nextStation));

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
            if (traveledDistance >= accelerationDist) departedFromStop = false;
        }
        //실제 이동
        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir != Vector3.zero)
            lastDirection = dir;

        float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
        if (remainingDistance < 0.05f)
        {
            transform.position = targetPos;

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
    }
    // waypoint 인덱스 진행 (방향 포함)
    public void AdvanceWaypoint()
    {
        startPos = transform.position;
        // 방향에 따라 다음 타겟 waypoint 결정 
        if (direction == TrainDirection.Forward)
        {
            if (waypointTargetIndex < routeWaypoints.Count - 1)
            {
                waypointTargetIndex++;
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
                float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            Debug.Log($"<color=yellow>{currentStation.name} 정차 중...</color>");
            //내릴 승객 처리
            HandleAlighting(currentStation);

            yield return null;

            //탈 승객 처리
            HandleBoarding(currentStation);
        }

        yield return new WaitForSeconds(1.2f);

        AdvanceWaypoint();
        departedFromStop = true;
        isStopping = false;
    }

    public void HandleBoarding(Station station)
    {
        int i = 0;
        while (i < station.waitingPassengers.Count)
        {
            if (passengers.Count >= capacity) break;

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

                p.gameObject.SetActive(false);  // 대기 중인 승객 비활성화
                Debug.Log($"승객 탑승! 목적지: {p.destination} (열차 잔여석: {capacity - passengers.Count})");
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
            Debug.Log($"p.transferStation: {p.transferStation?.name ?? "없음"}, station: {station.name}");

            if (p.destination == station.Shape)
            {
                p.State = PassengerState.Arrived;

                Score.score++;
                gm.UpdateUIText();

                Destroy(p.gameObject);
                passengers.RemoveAt(i);

                Debug.Log($"<color=green>[하차 완료]</color> 목적지 {station.Shape} 도착! 점수 +1 (열차 잔여석: {capacity - passengers.Count})");
            }
            //환승하는경우
            else if (p.transferStation == station)
            {
                p.transferStation = null;

                p.State = PassengerState.Waiting;
                Passenger newP = station.AddPasssenger(p.destination);
                if (newP != null) newP.blockedLineId = lineId;

                Destroy(p.gameObject);
                passengers.RemoveAt(i);
            }
        }
        RefreshPassengerIcons();
    }

    public int BFSDistance(StationType dest, int currentIndex, int dir)
    {
        if (currentIndex < 0 || currentIndex >= path.Count) return int.MaxValue;

        HashSet<Station> visitedStations = new();
        Queue<(Station station, int dist)> queue = new(); // 역 + 거리 쌍

        // 현재 노선에서 방향 기준으로 갈 수 있는 역만 추가
        int distance = 0;
        for (int i = currentIndex; i >= 0 && i < path.Count; i += dir)
        {
            queue.Enqueue((path[i], distance));
            distance++;
        }

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();
            if (visitedStations.Contains(current)) continue;
            visitedStations.Add(current);

            if (current.Shape == dest) return dist; // 목적지 찾으면 거리 반환

            foreach (var line in current.lines)
            {
                if (line.lineId == lineId) continue; // 현재 노선 스킵
                foreach (var station in line.stations)
                {
                    if (!visitedStations.Contains(station))
                        queue.Enqueue((station, dist + 1));
                }
            }
        }

        return int.MaxValue; // 못 찾으면 최대값
    }

    public bool CanBoard(Passenger p)
    {
        if (p.blockedLineId == lineId) return false;

        var dir = direction == TrainDirection.Forward ? 1 : -1;
        int myDist = BFSDistance(p.destination, targetStationIndex +dir, dir);
        int oppDist = BFSDistance(p.destination, targetStationIndex -dir, -dir);

        return myDist != int.MaxValue && myDist <= oppDist;
    }

    public bool NeedsTransfer(Passenger p, Station station)
    {
        bool notOnCurrentLine = !path.Any(s => s.Shape == p.destination);   // 현재 노선에 목적지 없음
        bool canReachViaTransfer = BFS(p.destination, path.IndexOf(station), 1)
                                || BFS(p.destination, path.IndexOf(station), -1);   // 이 역에 내리면 목적지에 갈 수 있음

        return notOnCurrentLine && canReachViaTransfer;
    }

    public bool BFS(StationType dest, int currentIndex, int dir)
    {
        HashSet<Station> visitedStations = new();
        Queue<Station> queue = new();

        for (int i = currentIndex; i >= 0 && i < path.Count; i += dir)
        {
            queue.Enqueue(path[i]);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visitedStations.Contains(current)) continue;
            visitedStations.Add(current);

            if (current.Shape == dest) return true;

            foreach (var line in current.lines) // 이곳을 지나는 모든 노선
            {
                if (line.lineId == lineId) continue;
                foreach (var station in line.stations)  // 노선에 속하는 모든 역
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
            if (p.destination == station.Shape) return true;
        }
        foreach (var p in station.waitingPassengers)
        {
            if (CanBoard(p)) return true;
        }
        return false;
    }

    public Station FindTransferStation(Passenger p)
    {
        int dir = direction == TrainDirection.Forward ? 1 : -1;
        int startIndex = targetStationIndex + dir;

        if (startIndex < 0 || startIndex >= path.Count) return null;

        // 현재 노선에서 방향 기준으로 순회
        for (int i = startIndex; i >= 0 && i < path.Count; i += dir)
        {
            var station = path[i];

            // 직통으로 목적지 있으면 환승 필요 없음
            if (station.Shape == p.destination) return null;

            // 이 역에서 환승하면 목적지 갈 수 있으면 환승역으로 지정
            foreach (var line in station.lines)
            {
                if (line.lineId == lineId) continue;
                if (line.stations.Any(s => s.Shape == p.destination))
                    return station;
            }
        }

        return null;
    }

    private void UpdateDirection()
    {
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
        if (stationIndex < path.Count) return path[stationIndex];
        return null;
    }
    private int GetStationIndex(Station station)
    {
        return path.IndexOf(station);
    }
    private bool IsTerminalStation(Station station)
    {
        int idx = path.IndexOf(station);
        return idx == 0 || idx == path.Count - 1;
    }
}
