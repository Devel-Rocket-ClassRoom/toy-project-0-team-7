using UnityEngine;

public class Line : MonoBehaviour
{
    public int lineId;
    public Color color;
    //public List<Station> stations = new();  // 순서 중요
    //public List<Train> trains = new();
    //public bool isCircular = false;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Init()
    {

    }
}