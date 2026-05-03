using UnityEngine;

public class CameraController : MonoBehaviour
{
    public StationManager sm;
    
    [SerializeField] private float padding = 3f;
    [SerializeField] private float minOrthSize = 5f;
    [SerializeField] private float positionSmoothTime = 0.6f;
    [SerializeField] private float zoomSoothTime = 1.0f;

    private Camera cam;
    private Vector3 positionVelocity;
    private float zoomVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
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
