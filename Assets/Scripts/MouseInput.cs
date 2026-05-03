using UnityEngine;

public class MouseInput : MonoBehaviour
{
    public enum Mode { None, NewLine, ExtendLine, EditLine, AddTrain, MoveTrain }
    public Mode mode;

    private Camera cam;
    public LineManager lineManager;

    private bool isStartHandle;

    public AssetManager assetManager;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        var point = cam.ScreenToWorldPoint(Input.mousePosition);
        var hits = Physics2D.RaycastAll(point, Vector2.zero);

        RaycastHit2D stationHit = default;
        RaycastHit2D handleHit = default;
        RaycastHit2D lineHit = default;

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Station")) stationHit = h;
            else if (h.collider.CompareTag("Handle")) handleHit = h;
            else if (h.collider.CompareTag("Line")) lineHit = h;
        }

        if (Input.GetMouseButtonDown(0))    // 클릭
        {
            if (assetManager.isWeekend) return;

            if (stationHit.collider != null && !lineManager.IsLinesFull)    // 새 선 만들기
            {
                mode = Mode.NewLine;
                var pos = stationHit.collider.gameObject.transform.position;
                pos.z = 0f;

                lineManager.StartNewLine(stationHit, pos);
                return;                    
            }

            else if (handleHit.collider != null) // 기존 선 연장하기
            {
                mode = Mode.ExtendLine;
                var pos = handleHit.collider.gameObject.transform.position;
                pos.z = 0f;

                lineManager.StartExtendLine(handleHit, pos);

                if (handleHit.collider.GetComponent<Handle>().isStartHandle)
                    isStartHandle = true;

                else
                    isStartHandle = false;

                return;
            }

            else if (lineHit.collider != null)  // 기존 선 편집하기 (중간 선택)
            {
                mode = Mode.EditLine;
                var pos = point;
                pos.z = 0f;

                lineManager.StartEditLine(lineHit, pos);
                return;
            }
        }

        if (mode != Mode.None)
        {
            if (Input.GetMouseButton(0))    // 드래그
            {
                var previewPoint = point;
                previewPoint.z = 0f;

                switch (mode)
                {
                    case Mode.NewLine:
                        lineManager.UpdateEndPreviewPoint(previewPoint);
                        if (stationHit.collider != null)
                        {
                            var station = stationHit.collider.GetComponent<Station>();
                            lineManager.AddStationInMakingLine(station);
                        }
                        break;

                    case Mode.ExtendLine:
                        if (isStartHandle)  lineManager.UpdateStartPreviewPoint(previewPoint);
                        else                lineManager.UpdateEndPreviewPoint(previewPoint);
                        if (stationHit.collider != null)
                        {
                            var station = stationHit.collider.GetComponent<Station>();
                            lineManager.ToggleStationInExtendLine(station, isStartHandle);
                        }
                        else
                        {
                            lineManager.stationUnderMouse = null; // 역에서 벗어나면 초기화
                        }
                        break;

                    case Mode.EditLine:
                        lineManager.UpdateEditPreviewPoint(previewPoint); 
                        if (stationHit.collider != null)
                        {
                            bool goExtend = lineManager.ToggleStationInEditLine(stationHit.collider.GetComponent<Station>());
                            if (goExtend)
                            {
                                mode = Mode.ExtendLine;
                                isStartHandle = lineManager.isStartHandle;
                                lineManager.HideHandle(isStartHandle); // 핸들 숨기기
                            }
                        }
                        else
                            lineManager.stationUnderMouse = null;
                        break;

                    case Mode.MoveTrain:
                        break;
                }
            }

            if (Input.GetMouseButtonUp(0))  // 릴리즈
            {
                switch (mode)
                {
                    case Mode.NewLine:
                        if (lineManager.IsValidLine)    lineManager.FixNewLine();
                        else                            lineManager.CancelNewLine();
                        break;

                    case Mode.ExtendLine:
                        lineManager.FinishExtendLine();
                        break;

                    case Mode.EditLine:
                        lineManager.FinishEditLine();
                        break;

                    case Mode.MoveTrain:

                        break;
                }

                mode = Mode.None;
                assetManager.OnInputReleased();
            }
        }
    }
}
