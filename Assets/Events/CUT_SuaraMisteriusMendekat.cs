using UnityEngine;

public class CUT_SuaraMisteriusMendekat : MonoBehaviour
{
    [Header("Referensi Objek Misterius")]
    // Tarik Game Object musuh yang memiliki komponen XCOM ke kolom ini di Inspector
    [SerializeField] private XCOM_ObjectStepAndMove enemyXcomComponent;

    [Header("Trigger Event (Contoh)")]
    [SerializeField] private bool triggerMulaiLewatInspector = false;

    void Start()
    {
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
}