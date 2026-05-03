using UnityEngine;
using UnityEngine.EventSystems;

public class TrainDragButton : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public MouseInput mouseInput;

    public void OnBeginDrag(PointerEventData eventData)
    {
        mouseInput.ChangeToNewTrainMode();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 실제 동작 없음. 근데 얘 없으면 작동 안됨;;;
    }
}
