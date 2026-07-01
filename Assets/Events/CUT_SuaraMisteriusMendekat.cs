using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class CUT_SuaraMisteriusMendekat : MonoBehaviour
{
    [Header("Referensi Objek Misterius")]
    // Tarik Game Object musuh yang memiliki komponen XCOM ke kolom ini di Inspector
    [SerializeField] private XCOM_ObjectStepAndMove enemyXcomComponent;

    public ObjectiveTrigger CONNECT_ObjectiveTrigger;

    public FIKIFOW_CameraRotationFocus cameraRotFocus; 

    [Header("Trigger Event (Contoh)")]
    //public GameObject GO_SuaraMisteriusMendekat; // Contoh trigger area untuk memulai event
    public int LanjutanDialog = 0;
    [SerializeField] private bool triggerMulaiLewatInspector = false;
    public Transform titikSuaraKaki;
    [SerializeField] private AudioSource AUS_LangkahPertama;
    [SerializeField] private AudioClip SFX_LangkahPertama;

    [Header("JAM 12")]
    [SerializeField] private AudioSource AUS_Jam12;
    [SerializeField] private AudioClip SFX_Jam12;

    public EVT_NontonTV CONNECT_EVT_NontonTV;

    [Header("Referensi Pergerakan Lanjutan")]
    [SerializeField] private Transform targetPointKedua; // Tarik target kedua musuh ke sini di Inspector


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
                    LanjutanDialog = 2;
                    triggerMulaiLewatInspector = true;
      
                    Debug.Log("NAH DISINI SI MUSUH MOVE SPEED NYA JADI DIPERCEPAT");
                    // DISINI
                    if (enemyXcomComponent != null)
                    {
                        // Contoh: Ubah speed dari awal (misal 0.4f) ke 5.0f, dalam durasi 2 detik, pakai EaseOut
                        //enemyXcomComponent.SetMoveSpeed(5f, 2f, XCOM_ObjectStepAndMove.SpeedInterpolationMode.EaseOut);
                    }
                    Load_SuaraMisteriusMendekat();
                    
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
        else if (LanjutanDialog == 2)
        {
            yield return new WaitForSeconds(1f);
            KIRISA_DialogueSystem.Instance.StartDialogCallback(
                () => 
                {   
                    //FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
                    
                    AUS_Jam12.PlayOneShot(SFX_Jam12);
                    CONNECT_ObjectiveTrigger.misiBerikutnya = "LARI KE LANTAI 2 & TUTUP PINTU";    
                    if (FIKIFOW_ObjectiveManager.Instance != null)
                    {
                        FIKIFOW_ObjectiveManager.Instance.GantiMisi(CONNECT_ObjectiveTrigger.misiBerikutnya);
                    }   
                    LanjutanDialog = 3;
                    triggerMulaiLewatInspector = true;
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
                    dialog: "Uhh.....", 
                    portrait: "", 
                    voice: ""
                )
            );
        }
        else if (LanjutanDialog == 3)
        {
            yield return new WaitForSeconds(4f);
            
            FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
            LanjutanDialog = 4;
            CONNECT_EVT_NontonTV.GO_KemunculanSuaraAneh.SetActive(false);
            enemyXcomComponent.SetTargetPoint(targetPointKedua);
            enemyXcomComponent.SetMoveSpeed(3f, 0f, XCOM_ObjectStepAndMove.SpeedInterpolationMode.Constant);

            Debug.Log("CUTSCENE SELESAI");
        }

        
       
    }
}