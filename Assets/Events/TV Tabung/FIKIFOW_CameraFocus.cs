using UnityEngine;
using System.Collections;

public class FIKIFOW_CameraFocus : MonoBehaviour
{
    [Header("Target Configuration")]
    [Tooltip("Game Object kosong yang ditaruh di depan TV sebagai target kamera")]
    [SerializeField] private Transform tvTargetPoint;
    
    [Tooltip("Kecepatan transisi pergerakan kamera menuju target")]
    [SerializeField] private float transitionSpeed = 5f;

    private bool isFocusing = false;
    private Transform playerCameraHolder;

    // Untuk menyimpan posisi & rotasi awal kamera sebelum digeser ke TV
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    void Start()
    {
        // Ambil referensi cameraHolder dari Singleton FPS Engine milikmu
        if (FIKIFOWFPS1_FirstPersonEngine.Instance != null)
        {
            playerCameraHolder = FIKIFOWFPS1_FirstPersonEngine.Instance.cameraHolder;
        }
    }

    void Update()
    {
        // Jika tidak sedang fokus, atau target belum di-assign, abaikan
        if (!isFocusing || playerCameraHolder == null || tvTargetPoint == null) return;

        // REALTIME: Kamera akan selalu mengikuti posisi dan rotasi target secara halus.
        // Jika kamu menggeser Game Object kosong di Editor, kamera akan langsung ikut bergeser secara realtime!
        playerCameraHolder.position = Vector3.Lerp(playerCameraHolder.position, tvTargetPoint.position, Time.deltaTime * transitionSpeed);
        playerCameraHolder.rotation = Quaternion.Slerp(playerCameraHolder.rotation, tvTargetPoint.rotation, Time.deltaTime * transitionSpeed);
    }

    /// <summary>
    /// Panggil fungsi ini melalui Trigger / Interactable saat pemain mulai berinteraksi dengan TV
    /// </summary>
    public void StartFocus()
    {
        if (FIKIFOWFPS1_FirstPersonEngine.Instance == null || tvTargetPoint == null) return;

        playerCameraHolder = FIKIFOWFPS1_FirstPersonEngine.Instance.cameraHolder;

        // 1. Simpan posisi & rotasi asli kamera (relatif terhadap tubuh player)
        originalLocalPosition = playerCameraHolder.localPosition;
        originalLocalRotation = playerCameraHolder.localRotation;

        // 2. Kunci total pergerakan dan arah pandang mouse player
        FIKIFOWFPS1_FirstPersonEngine.Instance.BlockInput();

        // 3. Aktifkan mode tracking realtime di Update
        isFocusing = true;
    }

    /// <summary>
    /// Panggil fungsi ini saat pemain selesai menonton (misal menekan tombol ESC / klik kanan)
    /// </summary>
    public void StopFocus()
    {
        if (!isFocusing) return;
        
        // Kembalikan kamera ke posisi semula di tubuh player secara halus
        StartCoroutine(ReturnCameraToPlayer());
    }

    private IEnumerator ReturnCameraToPlayer()
    {
        isFocusing = false; // Matikan realtime tracking TV target

        float elapsed = 0f;
        float duration = 0.5f; // Durasi animasi kembali ke tubuh pemain (0.5 detik)

        Vector3 startPos = playerCameraHolder.localPosition;
        Quaternion startRot = playerCameraHolder.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Kembalikan posisi lokal kamera ke koordinat default player
            playerCameraHolder.localPosition = Vector3.Lerp(startPos, originalLocalPosition, t);
            playerCameraHolder.localRotation = Quaternion.Slerp(startRot, originalLocalRotation, t);
            yield return null;
        }

        // Pastikan posisinya benar-benar pas di akhir animasi
        playerCameraHolder.localPosition = originalLocalPosition;
        playerCameraHolder.localRotation = originalLocalRotation;

        // Buka kembali kontrol player
        if (FIKIFOWFPS1_FirstPersonEngine.Instance != null)
        {
            FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
        }
    }
}