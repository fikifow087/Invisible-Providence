using UnityEngine;
using System.Collections;
public class EVT_S1_ChapterFlow : MonoBehaviour
{
    public GameObject TransisiHitam;
    public ObjectiveTrigger CONNECT_ObjectiveTrigger;

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
                CONNECT_ObjectiveTrigger.misiBerikutnya = "Cari makanan di dapur";    
                if (FIKIFOW_ObjectiveManager.Instance != null)
                {
                    FIKIFOW_ObjectiveManager.Instance.GantiMisi(CONNECT_ObjectiveTrigger.misiBerikutnya);
                }   
                Debug.Log("Dialog Selesai");
                
            },
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ARIAN", 
                dialog: "......", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Akhirnya sampai juga di rumah. Kuy lah waktunya gadang", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Tinggal lanjut main game kemarin", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Nanggung banget kalo ditinggal, udah mau tamat nih", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "(Suara perut keroncongan)", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Gahh lapar banget...", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Mending makan sambil nonton tv dah.", 
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
