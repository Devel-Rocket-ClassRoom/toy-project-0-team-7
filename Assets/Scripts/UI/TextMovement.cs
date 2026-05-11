using UnityEngine;

public class TextMovement : MonoBehaviour
{
    public float speed = 50f; // 초당 올라가는 픽셀(유닛)
    public float resetY = -500f; // 이 Y 위치 아래로 내려가면 리셋 (루프용, 필요 없으면 제거)

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        transform.localPosition += Vector3.up * speed * Time.deltaTime;
    }
}
