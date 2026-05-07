using UnityEngine;

public class MouseInput : MonoBehaviour
{
    public GameManager gm;
    
    public enum Mode { None, NewLine, ExtendLine, EditLine, NewTrain, MoveTrain, InterchangeStation }
    public Mode mode;

    private Camera cam;
    public CameraController cameraDirector;
    public LineManager lineManager;
    public TrainManager trainManager;
    public AssetManager assetManager;
    private bool isStartHandle;
    private Station interchangeTarget;
    private Line trainTarget;


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
            if (assetManager.isWeekend || gm.isGameOver) return;

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
                isStartHandle = handleHit.collider.GetComponent<Handle>().isStartHandle;

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
                if (gm.isGameOver)
                {
                    StopDragging();
                    return;
                }

                var previewPoint = point;
                previewPoint.z = 0f;

                switch (mode)
                {
                    case Mode.NewLine:
                        lineManager.UpdateEndPreviewPoint(previewPoint);
                        if (stationHit.collider != null)
                        {
                            var station = stationHit.collider.GetComponent<Station>();
                            var isCircular = lineManager.ToggleStationInNewLine(station);
                            if (isCircular)
                            {
                                isStartHandle = false;
                                StopDragging();
                            }
                        }
                        break;

                    case Mode.ExtendLine:
                        if (isStartHandle)  lineManager.UpdateStartPreviewPoint(previewPoint);
                        else                lineManager.UpdateEndPreviewPoint(previewPoint);
                        if (stationHit.collider != null)
                        {
                            var station = stationHit.collider.GetComponent<Station>();
                            var isCircular = lineManager.ToggleStationInExtendLine(station, isStartHandle);
                            if (isCircular)
                            {
                                isStartHandle = lineManager.isStartHandle;
                                StopDragging();
                            }
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
                                lineManager.HideHandle(true); // 핸들 숨기기
                            }
                        }
                        else
                            lineManager.stationUnderMouse = null;
                        break;

                    case Mode.NewTrain:
                        trainTarget = lineHit.collider != null ? lineHit.collider.GetComponent<Line>() : null;
                        break;

                    case Mode.MoveTrain:
                        Debug.Log("Mode.MoveTrain");
                        break;

                    case Mode.InterchangeStation:
                        interchangeTarget = stationHit.collider != null ? stationHit.collider.GetComponent<Station>() : null;
                        break;
                }
            }

            if (Input.GetMouseButtonUp(0))  // 릴리즈
            {
                switch (mode)
                {
                    case Mode.InterchangeStation:
                        interchangeTarget = stationHit.collider != null ? stationHit.collider.GetComponent<Station>() : null;

                        if (interchangeTarget != null)
                        {
                            assetManager.InterchangeUsed();
                        }
                        
                        break;
                    case Mode.NewTrain:
                        
                        trainTarget = lineHit.collider != null ? lineHit.collider.GetComponent<Line>() : null;
                        break;
                }
                
                StopDragging();
            }
        }
    }

    public void ChangeToNewTrainMode()
    {
        mode = Mode.NewTrain;
    }

    public void StopDragging()
    {
        switch (mode)
        {
            case Mode.NewLine:
                if (lineManager.IsValidLine)
                    lineManager.FixNewLine();                
                else 
                    lineManager.CancelNewLine();
                break;

            case Mode.ExtendLine:
                lineManager.FinishExtendLine();
                break;

            case Mode.EditLine:
                lineManager.FinishEditLine();
                break;

            case Mode.NewTrain:
                if (trainTarget != null && trainTarget.trains.Count < Line.MAX_TRAIN_COUNT && assetManager.RemainingTrainCount > 0)
                {
                    trainManager.Stations = trainTarget.stations;
                    trainTarget.trains.Add(trainManager.SpawnTrain(trainTarget.lineId, trainTarget.waypoints, trainTarget));
                    Debug.Log($"[열차 배치] 열차가 배치되었습니다. 라인 ID: {trainTarget.lineId}");
                    assetManager.UpdateTrainUI();
                }

                if (trainTarget != null && trainTarget.trains.Count >= Line.MAX_TRAIN_COUNT)
                {
                    Debug.Log($"[열차 배치] 해당 라인에 이미 최대 열차 수가 배치되어 있습니다. 라인 ID: {trainTarget.lineId}");
                }

                trainTarget = null;
                break;

            case Mode.MoveTrain:
                Debug.Log("Mode.MoveTrain End");
                break;

            case Mode.InterchangeStation:
                interchangeTarget?.SetAsInterchange();
                interchangeTarget = null;
                break;
        }

        mode = Mode.None;
        assetManager.OnInputReleased();
    }

    public void ChangeToInterchangeStationMode()
    {
        mode = Mode.InterchangeStation;
    }
}