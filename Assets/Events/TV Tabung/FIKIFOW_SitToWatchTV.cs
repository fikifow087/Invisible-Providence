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
    [Tooltip("Kecepatan transisi bergerak dan berputar ke kursi")]
    [SerializeField] private float transitionSpeed = 2f;

    private bool isSitting = false;

    public void SitDown()
    {
        if (isSitting) return;
        StartCoroutine(SitDownSequence());
    }

    private IEnumerator SitDownSequence()
    {
        var fpsEngine = FIKIFOWFPS1_FirstPersonEngine.Instance;
        if (fpsEngine == null) yield break;

        // 1. Matikan CharacterController agar tidak bentrok dengan Lerp
        CharacterController cc = fpsEngine.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 2. Blokir input (Ini akan menghentikan kamera mengikuti kepala di FPS Engine)
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

            // C. FIX BUG: Tarik posisi kamera secara manual agar tetap menempel di kepala player
            if (fpsEngine.cameraHolder != null && fpsEngine.headBone != null)
            {
                fpsEngine.cameraHolder.position = fpsEngine.headBone.position;
            }

            // D. FOKUS ROTASI: Hitung arah kamera ke TV secara real-time dari posisinya saat ini
            if (fpsEngine.cameraHolder != null)
            {
                Vector3 currentCamDir = tvTarget.position - fpsEngine.cameraHolder.position;
                Quaternion currentTargetCamRot = Quaternion.LookRotation(currentCamDir);
                
                // Putar kepala (kamera) ke TV secara mulus
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

        var fpsEngine = FIKIFOWFPS1_FirstPersonEngine.Instance;
        if (fpsEngine == null) return;

        CharacterController cc = fpsEngine.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

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