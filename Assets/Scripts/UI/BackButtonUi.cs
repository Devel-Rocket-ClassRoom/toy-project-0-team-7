using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButtonUi : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClickBack);
    }

    private void OnClickBack()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
