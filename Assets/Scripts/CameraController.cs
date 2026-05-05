using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameManager gm;
    public StationManager sm;
    
    // --- 역 스폰에 따라 카메라 조정하기 위한 필드들 ---
    [SerializeField] private float padding = 3f;
    [SerializeField] private float minOrthSize = 5f;
    [SerializeField] private float positionSmoothTime = 0.6f;
    [SerializeField] private float zoomSoothTime = 1.0f;

    // --- 게임오버될 때 게임 오버된 역 줌인할 때 사용할 필드들 ---
    [SerializeField] private float gameOverSize = 3f;
    [SerializeField] private float gameOverSmoothTime = 0.6f;
    [SerializeField] public float transitionDelay = 1.5f;

    private Camera cam;
    private Vector3 positionVelocity;
    private float zoomVelocity;
    public Vector3 targetStation;

    private bool IsGameOver => gm != null && gm.isGameOver;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (IsGameOver)
        {
            Vector3 gameOverPos = new Vector3(targetStation.x, targetStation.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, gameOverPos, ref positionVelocity, gameOverSmoothTime);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, gameOverSize, ref zoomVelocity, gameOverSmoothTime);

            return;
        }

        var stations = sm.ExisitingStations;
        if (stations == null || stations.Count == 0) return;

        Bounds bounds = new Bounds(stations[0].transform.position, Vector3.zero);
        foreach (var station in stations)
        {
            bounds.Encapsulate(station.transform.position);
        }

        float newH = bounds.extents.y + padding;
        float newW = (bounds.extents.x + padding) / cam.aspect;
        float targetSize = Mathf.Max(newH, newW, minOrthSize);

        Vector3 targetPos = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);

        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPos, 
            ref positionVelocity, 
            positionSmoothTime);

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize, 
            targetSize, 
            ref zoomVelocity, 
            zoomSoothTime);
    }
}
