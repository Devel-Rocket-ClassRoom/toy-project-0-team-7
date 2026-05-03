using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrainDirection
{
    Forward,
    Backward
}
public class Train : MonoBehaviour
{
    public int targetStationIndex = 0;
    public TrainDirection direction = TrainDirection.Forward;
    public List<Passenger> passengers = new List<Passenger>();
    public int capacity = 6;
    private bool isStopping = false;
    private bool departedFromStop = false; // 정차 후 출발했는지 여부

    public int lineId;

    [Header("Movement Settings")]
    public float maxSpeed = 2.5f;
    public float minSpeed = 0.5f;
    public float accelerationDist = 1.7f; // 가속 구간 거리
    public float decelerationDist = 1.7f; // 감속 구간 거리

    public Vector3 startPos; //출발 위치 기록용

    //열차 테스트 경로(승강장 리스트)
    private List<Station> path;


    //열차 경로 설정 및 열차 생성위치 초기화
    public void SetPath(List<Station> stations)
    {
        path = stations;
        targetStationIndex = 0;
        transform.position = path[0].transform.position;
    }


    public void Move()
    {
        if (isStopping || path == null || path.Count == 0) return;

        Station currentStation = path[targetStationIndex];
        Vector3 targetPos = path[targetStationIndex].transform.position;

        float remainingDistance = Vector3.Distance(transform.position, targetPos); //남은거리
        float traveledDistance = Vector3.Distance(startPos, transform.position); //달린거리

        float currentSpeed = maxSpeed;
        //감속 판정 - 이번 역이 종점 이거나 정차해야 하는 역이면 감속 준비
        bool shouldStopHere = (targetStationIndex == 0 || targetStationIndex == path.Count - 1 ||
            ShouldStopAtStation(currentStation));

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
        transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
        if (remainingDistance < 0.05f)
        {
            transform.position = targetPos;

            if (shouldStopHere)
            {
                //정차 및 승하차 프로세스 시작
                StartCoroutine(CoStationProcessRoutine());
            }
            else
            {
                DetermineNextTarget();
            }

        }
    }
    // 열차가 다음 역으로 이동할 때 타겟 역 인덱스 결정
    public void DetermineNextTarget()
    {
        startPos = transform.position;
        // 방향에 따라 다음 타겟 역 인덱스 결정 
        if (direction == TrainDirection.Forward)
        {
            // Forward일때 다음역이 있으면 index 증가
            if (targetStationIndex < path.Count - 1)
            {
                targetStationIndex++;
            }
            else // 다음역 없으면 방향 전환
            {
                direction = TrainDirection.Backward;
                targetStationIndex--;
            }
        }
        else
        {
            if (targetStationIndex > 0)
            {
                targetStationIndex--;
            }
            else
            {
                direction = TrainDirection.Forward;
                targetStationIndex++;
            }
        }
    }

    public IEnumerator CoStationProcessRoutine()
    {
        isStopping = true;

        Station currentStation = path[targetStationIndex];

        if (currentStation != null)
        {
            bool isTerminal = (targetStationIndex == 0 || targetStationIndex == path.Count - 1);
            if (isTerminal)
            {
                UpdateDirection();
            }

            Debug.Log($"<color=yellow>{currentStation.name} 정차 중...</color>");
            //내릴 승객 처리
            HandleAlighting(currentStation);

            //탈 승객 처리
            HandleBoarding(currentStation);
        }

        yield return new WaitForSeconds(2f);

        DetermineNextTarget();
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
                //[수정 예정] station에서 remove 메서드 생성 후 여기서 호출하는 쪽으로
                station.waitingPassengers.RemoveAt(i);
                if (p.gameObject != null) Destroy(p.gameObject);

                //열차 승객 리스트 추가
                p.State = PassengerState.OnTrain;
                passengers.Add(p);
                Debug.Log($"승객 탑승! 목적지: {p.destination} (열차 잔여석: {capacity - passengers.Count})");
            }
            else
            {
                i++;
            }

        }
    }
    public void HandleAlighting(Station station)
    {
        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            if (passengers[i].destination == station.Shape)
            {
                passengers[i].State = PassengerState.Arrived;
                Score.score++;
                passengers.RemoveAt(i);
                Debug.Log($"<color=green>[하차 완료]</color> 목적지 {station.Shape} 도착! 점수 +1 (열차 잔여석: {capacity - passengers.Count})");
            }
        }
    }
    //노선에 목적지 역이 포함되는지 검사
    public bool CanBoard(Passenger p)
    {
        if (direction == TrainDirection.Forward) //정방향
        {
            //정차한 역의 다음역 부터 검사
            for (int i = targetStationIndex + 1; i < path.Count; i++)
            {
                if (path[i].Shape == p.destination && path[i] != null)
                {
                    return true;
                }
            }
        }
        else //역방향
        {
            for (int i = targetStationIndex - 1; i >= 0; i--)
            {
                if (path[i].Shape == p.destination && path[i] != null)
                {
                    return true;
                }
            }
        }
        return false;
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
    private void UpdateDirection()
    {
        if (targetStationIndex == path.Count - 1)
            direction = TrainDirection.Backward;
        else if (targetStationIndex == 0)
            direction = TrainDirection.Forward;
    }
}
