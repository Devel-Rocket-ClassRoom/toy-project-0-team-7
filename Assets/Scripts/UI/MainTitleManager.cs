using UnityEngine;
using UnityEngine.SceneManagement;
public class MainTitleManager : MonoBehaviour
{
    public RectTransform rt;
    [SerializeField] private float speed = 400f;
    [SerializeField] private float limitXPos = 2000f;

    private bool isMoved = false;
    private Vector2 originPos;

    private void Start()
    {
        originPos = rt.anchoredPosition;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("MenuScene"); 
        }

        if (isMoved) return;

        rt.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        if (rt.anchoredPosition.x > limitXPos)
        {
            isMoved = true;
            rt.anchoredPosition = originPos;
        }
    }
}
