using UnityEngine;

[AddComponentMenu("FIKIFOW FPS/5 - Flashlight Sway")]
public class FIKIFOWFPS_FlashlightSway : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Masukkan Main Camera milik Player ke sini")]
    public Transform cameraTransform;

    [Header("Sway Core Settings")]
    [Tooltip("Kecepatan senter mengejar rotasi kamera. Semakin kecil nilainya, semakin terasa berat.")]
    public float swaySpeed = 4.5f;

    [Tooltip("Intensitas ayunan horizontal (Kanan/Kiri) saat kamera berputar. Naikkan jika kurang terasa.")]
    public float swayAmountX = 2.0f;

    [Tooltip("Intensitas ayunan vertikal (Atas/Bawah) saat kamera berputar.")]
    public float swayAmountY = 1.5f;

    [Tooltip("Batas maksimum sudut ayunan (dalam derajat) agar senter tidak berputar ekstrem/patah.")]
    public float maxSwayAngle = 12f;

    [Header("Position Offset")]
    [Tooltip("Mengatur posisi senter relatif terhadap kamera. X = Kanan/Kiri, Y = Atas/Bawah, Z = Depan/Belakang.")]
    public Vector3 positionOffset = new Vector3(0.25f, -0.3f, 0.1f);

    // Menyimpan data rotasi kamera pada frame sebelumnya
    private Vector3 lastCameraRotation;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Inisialisasi posisi dan rotasi awal secara instan agar tidak melompat di awal game
        if (cameraTransform != null)
        {
            transform.position = cameraTransform.TransformPoint(positionOffset);
            transform.rotation = cameraTransform.rotation;
            lastCameraRotation = cameraTransform.eulerAngles;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 1. POSISI: Senter harus menempel instan pada posisi kamera ditambah offset
        Vector3 targetPosition = cameraTransform.TransformPoint(positionOffset);
        transform.position = targetPosition;

        // 2. ROTASI DENGAN DELTA SWAY (HORIZONTAL & VERTIKAL)
        Vector3 currentCameraRotation = cameraTransform.eulerAngles;
        
        // Hitung berapa derajat perubahan sudut kamera dari frame lalu ke frame sekarang
        float deltaX = Mathf.DeltaAngle(lastCameraRotation.y, currentCameraRotation.y); // Gerakan horizontal (Yaw)
        float deltaY = Mathf.DeltaAngle(lastCameraRotation.x, currentCameraRotation.x); // Gerakan vertikal (Pitch)

        // Simpan data rotasi frame ini untuk kalkulasi di frame berikutnya
        lastCameraRotation = currentCameraRotation;

        // Hitung nilai rotasi tambahan (gaya inersia/tertinggal). 
        // Nilai delta di-negatifkan (-) agar arah ayunan berlawanan dengan arah gerak kamera (efek lag)
        float swayTiltX = Mathf.Clamp(-deltaY * swayAmountY, -maxSwayAngle, maxSwayAngle);
        float swayTiltY = Mathf.Clamp(-deltaX * swayAmountX, -maxSwayAngle, maxSwayAngle);

        // Gabungkan rotasi dasar kamera dengan offset ayunan prosedural baru
        Quaternion swayRotationOffset = Quaternion.Euler(swayTiltX, swayTiltY, 0f);
        Quaternion targetRotation = cameraTransform.rotation * swayRotationOffset;

        // Eksekusi rotasi secara halus menggunakan Slerp
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * swaySpeed);
    }
}