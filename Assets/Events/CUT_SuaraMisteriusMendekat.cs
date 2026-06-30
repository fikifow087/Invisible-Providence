using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class CUT_SuaraMisteriusMendekat : MonoBehaviour
{
    [Header("Referensi Objek Misterius")]
    // Tarik Game Object musuh yang memiliki komponen XCOM ke kolom ini di Inspector
    [SerializeField] private XCOM_ObjectStepAndMove enemyXcomComponent;
    public FIKIFOW_CameraRotationFocus cameraRotFocus; 

    [Header("Trigger Event (Contoh)")]
    //public GameObject GO_SuaraMisteriusMendekat; // Contoh trigger area untuk memulai event
    public int LanjutanDialog = 0;
    [SerializeField] private bool triggerMulaiLewatInspector = false;
    public Transform titikSuaraKaki;
    [SerializeField] private AudioSource AUS_LangkahPertama;
    [SerializeField] private AudioClip SFX_LangkahPertama;


    void Start()
    {
        //GO_SuaraMisteriusMendekat.SetActive(false); // Nonaktifkan trigger area agar tidak mengganggu gameplay
        // Saat scene mulai, pastikan komponen XCOM dinonaktifkan terlebih dahulu
        if (enemyXcomComponent != null)
        {
            enemyXcomComponent.StopMoving();
            enemyXcomComponent.enabled = false; // Mematikan skrip sementara
            Debug.Log("XCOM Component dinonaktifkan sementara menunggu event.");
        }
    }

    void Update()
    {
        // Hanya contoh trigger sederhana lewat centang di Inspector saat Playmode
        if (triggerMulaiLewatInspector)
        {
            triggerMulaiLewatInspector = false; // Reset trigger agar tidak ke-call terus-menerus
            TriggerCutsceneSuara();
        }

        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            Load_SuaraMisteriusMendekat();
        }
    }

    // Panggil fungsi ini kapan pun event cutscene kamu dimulai (misal dari sistem dialog atau trigger area)
    public void TriggerCutsceneSuara()
    {
        if (enemyXcomComponent != null)
        {
            Debug.Log("Event Mulai: Menyalakan suara langkah dan pergerakan musuh!");
            
            // Aktifkan kembali skripnya, lalu perintahkan untuk mulai bergerak
            enemyXcomComponent.enabled = true; 
            enemyXcomComponent.StartMoving();
        }
    }

    public void Load_SuaraMisteriusMendekat()
    {
        StartCoroutine(MulaiCutscene_SuaraMisteriusMendekat());
    }

    IEnumerator MulaiCutscene_SuaraMisteriusMendekat()
    {
        

        if (LanjutanDialog == 0)
        {
            KIRISA_DialogueSystem.Instance.StartDialogCallback(
                () => 
                {   
                    if (FIKIFOWFPS1_FirstPersonEngine.Instance != null)
                    {
                        FIKIFOWFPS1_FirstPersonEngine.Instance.BlockInput();
                    }
                    LanjutanDialog = 1;
                    AUS_LangkahPertama.PlayOneShot(SFX_LangkahPertama);
                    cameraRotFocus.TriggerFocus(titikSuaraKaki, 2f, FIKIFOW_CameraRotationFocus.InterpolationMode.EaseOut, true, 0f);
                    Load_SuaraMisteriusMendekat();
                },
                new KirisaDialogLine(
                    mode: 1, 
                    duration: 0f,
                    speaker: "ADRIAN", 
                    dialog: "...", 
                    portrait: "", 
                    voice: ""
                ),
                new KirisaDialogLine(
                    mode: 1, 
                    duration: 0f,
                    speaker: "ADRIAN", 
                    dialog: "Aku tak suka ini...", 
                    portrait: "", 
                    voice: ""
                ),
                new KirisaDialogLine(
                    mode: 1, 
                    duration: 0f,
                    speaker: "ADRIAN", 
                    dialog: "Apa yang menyebabkan suara tadi!?", 
                    portrait: "", 
                    voice: ""
                )
            );
        }
        else if (LanjutanDialog == 1)
        {
            yield return new WaitForSeconds(1f);
            KIRISA_DialogueSystem.Instance.StartDialogCallback(
                () => 
                {   
                    //FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
                    triggerMulaiLewatInspector = true;
                    Debug.Log("AAAAAAAAAAAAAAAAAAAAAAA.");
                    
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
                    dialog: "HAH!", 
                    portrait: "", 
                    voice: ""
                ),
                new KirisaDialogLine(
                    mode: 1, 
                    duration: 0f,
                    speaker: "ADRIAN", 
                    dialog: "Ada sesuatu yang mendekat!", 
                    portrait: "", 
                    voice: ""
                )
            );
        }
        


        
       
    }
}