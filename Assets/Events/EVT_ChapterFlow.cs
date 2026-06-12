using UnityEngine;
using System.Collections;
public class EVT_ChapterFlow : MonoBehaviour
{
    public GameObject TransisiHitam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(atTheBeginning());
    }

    IEnumerator atTheBeginning()
    {
        yield return StartCoroutine(FIKIFOW_GameOBJ_Transition.FadeOut(TransisiHitam, 0f, 120));
        KIRISA_DialogueSystem.Instance.StartDialogCallback(
            () => 
            {          
                Debug.Log("Dialog Selesai");
            },
            new KirisaDialogLine(
                speaker: "ARIAN", 
                dialog: "......", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                speaker: "ADRIAN", 
                dialog: "Akhirnya sampai juga di rumah. Kuy lah waktunya gadang ", 
                portrait: "", 
                voice: ""
            )
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
