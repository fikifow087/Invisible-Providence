using UnityEngine;

public class EVT_kulkas : MonoBehaviour
{
    public EVT_ChapterFlow CONNECT_EVT_ChapterFlow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void BukaKulkas()
    {
       KIRISA_DialogueSystem.Instance.StartDialogCallback(
            () => 
            {   
                
                Debug.Log("Dialog Selesai");
                
            },
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "......", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Aha...Ada pizza dingin di kulkas. Pas banget nih buat ganjel perut dulu", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Tinggal angetin bentar, nonton, terus lanjut main game, easy..", 
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
