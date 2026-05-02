using UnityEngine;

public class Handle : MonoBehaviour
{
    public Line line;
    public bool isStartHandle;

    public GameObject sero;
    public GameObject garo;

    private LineRenderer lrSero;
    private LineRenderer lrGaro;

    public void Awake()
    {
        lrSero = sero.GetComponent<LineRenderer>();
        lrGaro = garo.GetComponent<LineRenderer>();
    }

    public void SetHandleDirection(Vector3 dir)
    {
        var perp = new Vector3(-dir.y, dir.x, 0); // 수직 방향

        // 세로 막대
        lrSero.SetPosition(0, transform.position);
        lrSero.SetPosition(1, transform.position + dir * 0.5f);

        // 가로 막대
        lrGaro.SetPosition(0, transform.position + dir * 0.5f - perp * 0.3f);
        lrGaro.SetPosition(1, transform.position + dir * 0.5f + perp * 0.3f);
    }

    public void SetColor(Color color)
    {
        if (lrSero == null) lrSero = sero.GetComponent<LineRenderer>(); // 방어 코드
        if (lrGaro == null) lrGaro = garo.GetComponent<LineRenderer>();

        lrSero.startColor = color;
        lrSero.endColor = color;
        lrGaro.startColor = color;
        lrGaro.endColor = color;
    }
}