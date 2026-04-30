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
    public float speed = 5f;

    //열차 테스트 경로(승강장 리스트)
    private List<GameObject> path;

    //열차 경로 설정 및 열차 생성위치 초기화
    public void SetPath(List<GameObject> stations)
    {
        path = stations;
        targetStationIndex = 0;
        transform.position = path[0].transform.position;
    }

    
    public void Move()
    {
        if (path == null || path.Count == 0) return; 
        Vector3 targetPos = path[targetStationIndex].transform.position;

        //[수정 예정] 승객 유무에 따라 감속할때 lerp로
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            //[수정 예정] 승객 유무에 따라 대기시간 달라지게
            DetermineNextTarget();
        }
    }
    // 열차가 다음 역으로 이동할 때 타겟 역 인덱스 결정
    void DetermineNextTarget()
    {
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
            // Backward일때 다음역이 있으면 index 감소
            if (targetStationIndex > 0)
            {
                targetStationIndex--;
            }
            else // 다음역 없으면 방향 전환
            {
                direction = TrainDirection.Forward;
                targetStationIndex++;
            }
        }
    }

}
