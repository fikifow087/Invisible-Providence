using System.Collections;
using UnityEngine;

public class FIKIFOW_SitToWatchTV : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Target posisi duduk (Objek kosong di atas kursi)")]
    [SerializeField] private Transform sitPoint;
    
    [Tooltip("Target TV yang ingin dilihat")]
    [SerializeField] private Transform tvTarget;
    
    [Tooltip("Script ReadMode yang ada di scene kamu")]
    [SerializeField] private ReadMode readModeScript;

    [Header("Settings")]
    [Tooltip("Kecepatan transisi bergerak dan berputar")]
    [SerializeField] private float transitionSpeed = 2f;

    private bool isSitting = false;

    // ==========================================
    // VARIABEL BARU: Menyimpan posisi asli player sebelum duduk
    // ==========================================
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;

    public void SitDown()
    {
        if (isSitting) return;
        StartCoroutine(SitDownSequence());
    }

    private IEnumerator SitDownSequence()
    {
        var fpsEngine = FIKIFOWFPS1_FirstPersonEngine.Instance;
        if (fpsEngine == null) yield break;

        // 1. SIMPAN POSISI & ROTASI ASLI PLAYER DI SINI (Supaya tidak kacau pas berdiri)
        originalPlayerPos = fpsEngine.transform.position;
        originalPlayerRot = fpsEngine.transform.rotation;

        // 2. Matikan CharacterController agar tidak bentrok dengan Lerp
        CharacterController cc = fpsEngine.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 3. Blokir input
        if (readModeScript != null)
        {
            readModeScript.ReadMode_ON();
        }
        else
        {
            fpsEngine.BlockInput();
        }

        Vector3 startPlayerPos = fpsEngine.transform.position;
        Quaternion startPlayerRot = fpsEngine.transform.rotation;
        Quaternion startCameraRot = fpsEngine.cameraHolder.rotation;

        // Hitung arah badan (Hanya sumbu Y agar tidak miring)
        Vector3 directionToTV = tvTarget.position - sitPoint.position;
        directionToTV.y = 0; 
        Quaternion targetPlayerRot = Quaternion.LookRotation(directionToTV);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;

            // A. Geser posisi badan player ke kursi
            fpsEngine.transform.position = Vector3.Lerp(startPlayerPos, sitPoint.position, elapsed);

            // B. Putar badan player menghadap ke TV
            fpsEngine.transform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, elapsed);

            // C. Tarik posisi kamera secara manual agar tetap menempel di kepala player
            if (fpsEngine.cameraHolder != null && fpsEngine.headBone != null)
            {
                fpsEngine.cameraHolder.position = fpsEngine.headBone.position;
            }

            // D. Hitung arah kamera ke TV secara real-time
            if (fpsEngine.cameraHolder != null)
            {
                Vector3 currentCamDir = tvTarget.position - fpsEngine.cameraHolder.position;
                Quaternion currentTargetCamRot = Quaternion.LookRotation(currentCamDir);
                
                fpsEngine.cameraHolder.rotation = Quaternion.Slerp(startCameraRot, currentTargetCamRot, elapsed);
            }

            yield return null;
        }

        // Kunci akhir agar presisi sempurna
        fpsEngine.transform.position = sitPoint.position;
        fpsEngine.transform.rotation = targetPlayerRot;

        if (fpsEngine.cameraHolder != null && fpsEngine.headBone != null)
        {
            fpsEngine.cameraHolder.position = fpsEngine.headBone.position;
            Vector3 finalCamDir = tvTarget.position - fpsEngine.cameraHolder.position;
            fpsEngine.cameraHolder.rotation = Quaternion.LookRotation(finalCamDir);
        }

        isSitting = true;
    }

    public void StandUp()
    {
        if (!isSitting) return;
        // GANTI DI SINI: Sekarang memanggil Coroutine transisi otomatis berdiri
        StartCoroutine(StandUpSequence());
    }

    // ==========================================
    // COROUTINE BARU: Transisi otomatis kembali berdiri
    // ==========================================
    private IEnumerator StandUpSequence()
    {
        var fpsEngine = FIKIFOWFPS1_FirstPersonEngine.Instance;
        if (fpsEngine == null) yield break;

        // 1. Matikan CharacterController selama transisi bergerak agar mulus
        CharacterController cc = fpsEngine.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 startSitPos = fpsEngine.transform.position;
        Quaternion startSitRot = fpsEngine.transform.rotation;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;

            // A. Kembalikan posisi badan ke posisi semula (XYZ) sebelum duduk
            fpsEngine.transform.position = Vector3.Lerp(startSitPos, originalPlayerPos, elapsed);

            // B. Kembalikan rotasi badan ke hadapan semula sebelum duduk
            fpsEngine.transform.rotation = Quaternion.Slerp(startSitRot, originalPlayerRot, elapsed);

            // C. Tarik posisi kamera agar tetap sinkron di kepala player
            if (fpsEngine.cameraHolder != null && fpsEngine.headBone != null)
            {
                fpsEngine.cameraHolder.position = fpsEngine.headBone.position;
            }

            // D. Kembalikan rotasi lokal kamera secara mulus agar menghadap lurus ke depan
            if (fpsEngine.cameraHolder != null)
            {
                fpsEngine.cameraHolder.localRotation = Quaternion.Slerp(fpsEngine.cameraHolder.localRotation, Quaternion.identity, elapsed);
            }

            yield return null;
        }

        // Kunci akhir di posisi semula secara presisi mutlak
        fpsEngine.transform.position = originalPlayerPos;
        fpsEngine.transform.rotation = originalPlayerRot;

        if (fpsEngine.cameraHolder != null && fpsEngine.headBone != null)
        {
            fpsEngine.cameraHolder.position = fpsEngine.headBone.position;
            fpsEngine.cameraHolder.localRotation = Quaternion.identity;
        }

        // =======================================================
        // TAMBAHAN: Sinkronkan rotasi internal FPS agar seamless
        // =======================================================
        fpsEngine.ResetCameraPitch(0f);

        // 2. Aktifkan kembali CharacterController setelah sampai di posisi semula
        if (cc != null) cc.enabled = true;

        // 3. Kembalikan kontrol input penuh kepada player
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