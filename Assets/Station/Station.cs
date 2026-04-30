using UnityEngine;

// --- Station 데이터 클래스 ---
public class Station : MonoBehaviour
{
    private StationType shape;
    public StationType Shape => shape;

    // public List<Passenger> watiningPassengers = new();
    [SerializeField] private int capacity = 8; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    [SerializeField] private float overflowTimer = 180f; // 3분으로 설정, 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    // private bool isTransferHub = false; // 추가 구현 요소

    // public List<LineManager> lines = new List<LineManager>(); // 승강장에 연결된 노선을 저장할 리스트
}
