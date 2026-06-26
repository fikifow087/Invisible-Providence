using UnityEngine;
using System.Collections;

public class EVT_AdaSuaraDiruangan : MonoBehaviour
{
    public EVT_NontonTV CONNECT_EVT_NontonTV;
    [SerializeField] private AudioSource SFXS_suaraDiruangan1;
    [SerializeField] private AudioSource SFXS_suaraDiruangan2;
    
    // Hubungkan komponen script focus yang terpasang di scene
    public FIKIFOW_CameraRotationFocus cameraRotFocus; 
    
    // Titik kosong (Empty GameObject) letak mangkok bakso misterius atau wajah NPC
    public Transform titikSuaraMisterius1;
    public Transform titikSuaraMisterius2;

    public GameObject COL_suaraDiruangan1;
    public GameObject COL_suaraDiruangan2;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //CONNECT_EVT_NontonTV.REQ_EventTV_completion = false;
        COL_suaraDiruangan2.SetActive(false);
    }

    public void OnClick_SuaraDiruangan1()
    {
        StartCoroutine(COLLISION_SuaraDiruangan1());
    }

    public void OnClick_SuaraDiruangan2()
    {
        StartCoroutine(COLLISION_SuaraDiruangan2());
    }

    IEnumerator COLLISION_SuaraDiruangan1()
    {
        COL_suaraDiruangan2.SetActive(true);
        SFXS_suaraDiruangan2.Play();
        cameraRotFocus.TriggerFocus(titikSuaraMisterius2, 0.3f, FIKIFOW_CameraRotationFocus.InterpolationMode.EaseOut, true, 0f);
        yield return new WaitForSeconds(1f);
        KIRISA_DialogueSystem.Instance.StartDialogCallback(
            () => 
            {   
                if (FIKIFOWFPS1_FirstPersonEngine.Instance != null)
                {
                    FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
                }
                Debug.Log("Dialog Selesai");
                
            },
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "!!!", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "NJIR APA SIH!", 
                portrait: "", 
                voice: ""
            ),
            new KirisaDialogLine(
                mode: 1, 
                duration: 0f,
                speaker: "ADRIAN", 
                dialog: "Pasti tikus ini.", 
                portrait: "", 
                voice: ""
            )
        );
    }

    IEnumerator COLLISION_SuaraDiruangan2()
    {
        SFXS_suaraDiruangan1.Play();
        cameraRotFocus.TriggerFocus(titikSuaraMisterius1, 0.3f, FIKIFOW_CameraRotationFocus.InterpolationMode.EaseOut, true, 2f);
        yield return new WaitForSeconds(1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
