using UnityEngine;
using System.Collections.Generic;

// --- Station 데이터 클래스 ---
public class Station : MonoBehaviour
{
    private StationType shape;
    public StationType Shape => shape;

    [SerializeField] private int capacity = 8; // 조절하면서 게임 실행하다가 나중에 고정하든가 수정
    [SerializeField] private float overflowTimer = 180f; // 3분으로 설정, 조절하면서 게임 실행하다가 나중에 고정하든가 수정

    public List<Passenger> watiningPassengers = new();
    public List<LineManager> lines = new List<LineManager>(); // 승강장에 연결된 노선을 저장할 리스트
}
