using System.Collections.Generic;
using UnityEngine;

//테스트 스크립트로 나중에 삭제예정
public class TestStation : MonoBehaviour
{
    public StationType shapeType;
    public List<Passenger> waitingPassengers = new List<Passenger>(); // 대기 중인 승객들

    //삭제예정
    public List<GameObject> passengerObjects = new List<GameObject>();

    // 승객이 추가될 때 호출될 함수
    public void AddPassenger(Passenger p)
    {
        waitingPassengers.Add(p);

        //삭제예정
        CreateTempPassengerObject(p);
        //테스트로그
        Debug.Log($"<color=cyan>[승객 입성] {gameObject.name}({shapeType}) -> 목적지: {p.destination} (현재: {waitingPassengers.Count}명)");
        
    }

    //여기서부터 다 삭제예정
    void CreateTempPassengerObject(Passenger p)
    {
        GameObject passengerObj;

        switch (p.destination)
        {
            case StationType.Circle:
                passengerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case StationType.Square:
                passengerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case StationType.Triangle:
                // 유니티 기본형에 삼각형이 없으므로 캡슐이나 실린더로 대체
                passengerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            default:
                passengerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
        }

        // 2. 위치 및 크기 설정 (역 위쪽에 나열)
        float offset = (waitingPassengers.Count - 1) * 0.4f;
        passengerObj.transform.position = transform.position + new Vector3(offset, 1.2f, 0);
        passengerObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        // 3. 모양에 맞춰 색깔도 입혀주기 (가독성 up)
        Renderer renderer = passengerObj.GetComponent<Renderer>();
        renderer.material.color = GetColorByShape(p.destination);

        // 4. 관리 리스트에 추가 및 부모 설정
        passengerObj.transform.SetParent(this.transform);
        passengerObjects.Add(passengerObj);
    }

    // 색상 구분용 함수
    Color GetColorByShape(StationType type)
    {
        switch (type)
        {
            case StationType.Circle: return Color.red;    // 빨간 원
            case StationType.Square: return Color.blue;   // 파란 사각형
            case StationType.Triangle: return Color.green; // 초록 삼각형(캡슐)
            default: return Color.white;
        }
    }

}
