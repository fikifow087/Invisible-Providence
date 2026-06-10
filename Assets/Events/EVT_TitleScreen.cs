using UnityEngine;
using UnityEngine.SceneManagement;

public class EVT_MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick_NewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnClick_LoadScene()
    {
        Debug.Log("Load Menu Opened");
    }
    public void OnClick_Endings()
    {
       Debug.Log("Endings Menu Opened");
    }
    public void OnClick_Exit()
    {
        Application.Quit();
    }
}
