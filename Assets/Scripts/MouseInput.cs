using UnityEngine;

public class MouseInput : MonoBehaviour
{
    private Camera cam;
    public LineManager lineManager;

    public Line linePrefab;
    private Line line_onMaking;
    private LineRenderer lineRenderer;

    private bool isHolding = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        var point = cam.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(point, Vector2.zero);

        if (Input.GetMouseButtonDown(0))    // 클릭
        {
            if (hit.collider != null)   // 해당 위치에 충돌한 오브젝트가 존재하는 경우
            {
                if (hit.collider.CompareTag("Station"))
                {
                    Debug.Log("선 생성");
                    var pos = hit.collider.gameObject.transform.position;
                    pos.z = 0f;

                    line_onMaking = Instantiate(linePrefab, lineManager.transform);
                    line_onMaking.AddStationEnd(hit.collider.gameObject.GetComponent<Station>());

                    isHolding = true;

                    lineRenderer = line_onMaking.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, pos);
                }
            }
        }

        if (isHolding)
        {
            if (Input.GetMouseButton(0))    // 드래그
            {
                var previewPoint = point;
                previewPoint.z = 0f;
                lineRenderer.SetPosition(line_onMaking.stations.Count, previewPoint);

                if (hit.collider != null)   // 해당 위치에 충돌한 오브젝트가 존재하는 경우
                {
                    if (hit.collider.CompareTag("Station"))
                    {
                        var station = hit.collider.GetComponent<Station>();

                        if (!line_onMaking.stations.Contains(station))
                        {
                            line_onMaking.AddStationEnd(station);
                            Debug.Log($"선에 역 추가: {station.name}");
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))  // 릴리즈
            {
                if (line_onMaking.stations.Count < 2)
                {
                    Debug.Log($"미완성 선 삭제");
                    Destroy(line_onMaking.gameObject);
                    line_onMaking = null;
                    isHolding = false;
                    return;
                }

                // 선 확정
                Debug.Log($"선 확정");
                line_onMaking.Init();
                line_onMaking = null;
                isHolding = false;
            }
        }
    }
}
