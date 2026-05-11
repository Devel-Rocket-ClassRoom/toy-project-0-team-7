using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HighTrainDragButton : TrainDragButton
{
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!GetComponent<Button>().interactable)
            return;
        mouseInput.ChangeToHighTrainMode();
        ghostImage.SetActive(true);
    }
}
