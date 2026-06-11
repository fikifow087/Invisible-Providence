using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections;


public class EVT_MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject NewGame_to_TitleSequence;
    [SerializeField] private PlayableDirector TitleSequence;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NewGame_to_TitleSequence.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick_NewGame()
    {
        StartCoroutine(TitleSequence_Process());
    }

    IEnumerator TitleSequence_Process()
    {
        NewGame_to_TitleSequence.SetActive(true);
        TitleSequence.Play();
        Debug.Log("Title Sequence Started");
        yield return new WaitForSeconds(58.5f);
        Debug.Log("Title Sequence Finished");
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
