using System.Collections;
using UnityEngine;

public class FIKIFOW_SitToWatchTV : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Target posisi duduk (Objek kosong yang dibuat di Langkah 1)")]
    [SerializeField] private Transform sitPoint;
    
    [Tooltip("Target TV yang ingin dilihat (Bisa drag objek TV-nya ke sini)")]
    [SerializeField] private Transform tvTarget;
    
    [Tooltip("Script ReadMode yang ada di scene kamu")]
    [SerializeField] private ReadMode readModeScript;

    [Header("Settings")]
    [Tooltip("Kecepatan transisi bergerak dan berputar ke kursi")]
    [SerializeField] private float transitionSpeed = 2f;

    private bool isSitting = false;

    // Fungsi utama yang akan dipanggil oleh FIKIFOW_Interactable (OnInteractSuccess)
    public void SitDown()
    {
        if (isSitting) return;

        // Mulai proses duduk secara mulus menggunakan Coroutine
        StartCoroutine(SitDownSequence());
    }

    private IEnumerator SitDownSequence()
    {
        var fpsEngine = FIKIFOWFPS1_FirstPersonEngine.Instance;
        if (fpsEngine == null) yield break;

        // 1. Ambil CharacterController untuk dimatikan sementara agar tidak bentrok dengan Lerp posisi
        CharacterController cc = fpsEngine.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 2. Aktifkan Read Mode (Memblokir input pergerakan & memunculkan kursor mouse)
        if (readModeScript != null)
        {
            readModeScript.ReadMode_ON();
        }
        else
        {
            fpsEngine.BlockInput();
        }

        // 3. Catat posisi dan rotasi awal player/kamera sebelum bergerak
        Vector3 startPlayerPos = fpsEngine.transform.position;
        Quaternion startPlayerRot = fpsEngine.transform.rotation;
        Quaternion startCameraRot = fpsEngine.cameraHolder.rotation;

        // 4. Hitung arah rotasi target ke TV (Hanya horizontal Y untuk badan Player)
        Vector3 directionToTV = tvTarget.position - sitPoint.position;
        directionToTV.y = 0; // Biar badan player gak mendongak/nunduk, tetap tegak
        Quaternion targetPlayerRot = Quaternion.LookRotation(directionToTV);

        // 5. Hitung arah rotasi target untuk kamera (Melihat langsung ke TV secara presisi)
        Vector3 camDirectionToTV = tvTarget.position - fpsEngine.cameraHolder.position;
        Quaternion targetCameraRot = Quaternion.LookRotation(camDirectionToTV);

        // 6. Proses Transisi Mulus (Lerp & Slerp)
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;

            // Maju/geser posisi player ke kursi secara mulus
            fpsEngine.transform.position = Vector3.Lerp(startPlayerPos, sitPoint.position, elapsed);

            // Putar badan player menghadap ke TV secara horizontal
            fpsEngine.transform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, elapsed);

            // Putar kamera holder agar pas membidik ke arah TV
            fpsEngine.cameraHolder.rotation = Quaternion.Slerp(startCameraRot, targetCameraRot, elapsed);

            yield return null;
        }

        // 7. Kunci posisi akhir agar presisi sempurna di target
        fpsEngine.transform.position = sitPoint.position;
        fpsEngine.transform.rotation = targetPlayerRot;
        fpsEngine.cameraHolder.rotation = targetCameraRot;

        isSitting = true;
    }

    // BONUS: Fungsi untuk berdiri kembali dari kursi (Bisa dipanggil via tombol UI Keluar/Back)
    public void StandUp()
    {
        if (!isSitting) return;

        var fpsEngine = FIKIFOWFPS1_FirstPersonEngine.Instance;
        if (fpsEngine == null) return;

        // Nyalakan kembali fisik CharacterController
        CharacterController cc = fpsEngine.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        // Kembalikan kontrol game ke mode FPS biasa
        if (readModeScript != null)
        {
            readModeScript.ReadMode_OFF();
        }
        else
        {
            fpsEngine.UnblockInput();
        }

        isSitting = false;
    }
}