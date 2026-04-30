using UnityEngine;
using UnityEngine.EventSystems;

public class MouseInput : MonoBehaviour
{
    private Camera cam;

    public GameObject holdingObj;
    public bool isHolding;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        var point = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(point, Vector2.zero);
            if (hit.collider != null)   // 해당 위치에 충돌한 오브젝트가 존재하는 경우
            {
                if (hit.collider.CompareTag("Station"))
                {
                    Debug.Log("Station");
                    holdingObj = hit.collider.gameObject;
                }
            }
            else
            {
                Debug.Log("충돌 collider 없음");
            }
        }

        if (isHolding)
        {
            if (Input.GetMouseButton(0))
            {
                Debug.Log("눌려 있음");
                holdingObj.transform.position = point;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("올라감");
                holdingObj = null;
            }
        }
    }
}
