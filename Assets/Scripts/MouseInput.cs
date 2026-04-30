using UnityEngine;

public class MouseInput : MonoBehaviour
{
    public enum Movable { None, Line, Train }

    private Camera cam;
    public LineManager lineManager;

    private Movable holding;

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
                if (hit.collider.CompareTag("Station") && !lineManager.IsLinesFull)
                {
                    var pos = hit.collider.gameObject.transform.position;
                    pos.z = 0f;

                    lineManager.StartLine(hit, pos);
                    holding = Movable.Line;
                    return;
                }
            }
        }

        if (holding != Movable.None)
        {
            if (Input.GetMouseButton(0))    // 드래그
            {
                var previewPoint = point;
                previewPoint.z = 0f;

                switch (holding)
                {
                    case Movable.Line:
                        lineManager.UpdatePreviewPoint(previewPoint);
                        break;
                    case Movable.Train:
                        break;
                }

                if (hit.collider != null)   // 해당 위치에 충돌한 오브젝트가 존재하는 경우
                {
                    if (hit.collider.CompareTag("Station"))
                    {
                        var station = hit.collider.GetComponent<Station>();
                        lineManager.AddStationInMakingLine(station);
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))  // 릴리즈
            {
                if (lineManager.IsValidLine)    // 유효한 선이면 확정
                {
                    lineManager.FixLine();
                }

                else
                {
                    lineManager.CancelLine();
                }

                holding = Movable.None;                
            }
        }
    }
}
