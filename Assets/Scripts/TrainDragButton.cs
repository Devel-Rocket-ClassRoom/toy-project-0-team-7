using UnityEngine;
using UnityEngine.EventSystems;

public class TrainDragButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public MouseInput mouseInput;
    public GameObject ghostImage;

    private RectTransform ghostRect;
    private Canvas canvas;  
    private Camera cam; 

    private void Awake()
    {
        ghostRect = ghostImage.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        cam = Camera.main;
        ghostImage.SetActive(false);    
    }    

    private void OnDisable()
    {
        ghostImage.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mouseInput.ChangeToNewTrainMode();
        ghostImage.SetActive(true); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateGhostPos(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ghostImage.SetActive(false);
        
    }

    private void UpdateGhostPos(Vector2 screenPos)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        var hits = Physics2D.RaycastAll(worldPos, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Line"))
            {
                Vector2 lineScreenPos = cam.WorldToScreenPoint(hit.collider.ClosestPoint(worldPos));
                MoveGhost(lineScreenPos);
                return;
            }
        }

        MoveGhost(screenPos);
    }

    private void MoveGhost(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        ghostRect.localPosition = localPoint;
    }
}
