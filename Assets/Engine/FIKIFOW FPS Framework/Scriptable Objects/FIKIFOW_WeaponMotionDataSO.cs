using UnityEngine;

[CreateAssetMenu(fileName = "New Motion Data", menuName = "FIKIFOW FPS/Procedural Motion Data")]
public class FIKIFOW_WeaponMotionDataSO : ScriptableObject
{
    [Header("TOGGLE SYSTEM")]
    [Tooltip("Matikan jika ingin senjata statis 100%")]
    public bool enableProceduralMotion = true;

    [Header("PROCEDURAL SWAY (MOUSE MOVEMENT)")]
    [Tooltip("Seberapa jauh senjata merespon gerakan mouse (ayunan)")]
    public float swayMultiplier = 0.5f;
    [Tooltip("Batas maksimal kemiringan senjata saat mouse digerakkan cepat")]
    public float maxSwayAmount = 4f;
    [Tooltip("Kecepatan senjata kembali lurus ke tengah")]
    public float swaySmoothness = 10f;

    [Header("PROCEDURAL BOBBING (IDLE NAPAS)")]
    [Tooltip("Kecepatan manggut-manggut efek bernapas")]
    public float bobSpeed = 2f;
    [Tooltip("Seberapa tinggi/jauh senjata bergerak saat bernapas")]
    public float bobAmount = 0.015f;

    [Header("PROCEDURAL RECOIL (VISUAL HENTAKAN)")]
    [Tooltip("Dorongan senjata ke arah badan/kamera (mundur di sumbu Z)")]
    public float recoilKickbackZ = -0.1f;
    
    [Tooltip("Hentakan laras senjata ke atas (rotasi di sumbu X)")]
    public float recoilRotationX = -5f;
    
    [Tooltip("Hentakan acak laras ke kiri/kanan (rotasi di sumbu Y) - Bikin recoil tidak monoton")]
    public float recoilRandomY = 2f;
    
    [Tooltip("Hentakan acak miring (rotasi di sumbu Z) - Menambah efek organik")]
    public float recoilRandomZ = 1f;

    [Header("RECOIL RECOVERY SETTINGS")]
    [Tooltip("Kecepatan senjata menghentak secara instan saat peluru keluar (semakin tinggi semakin kasar)")]
    public float snapiness = 25f;
    
    [Tooltip("Kecepatan senjata kembali lurus/maju ke posisi awal setelah menembak (Recovery)")]
    public float returnSpeed = 10f;

    [Header("RELOAD MOVEMENT SETTINGS")]
    [Tooltip("Posisi senjata saat reload (offset dari posisi standar)")]
    public Vector3 reloadPositionOffset;
    
    [Tooltip("Rotasi senjata saat reload")]
    public Vector3 reloadRotationOffset;
    
    [Tooltip("Kecepatan transisi ke posisi reload")]
    public float reloadMovementSpeed = 8f;


    // --- BARU: PENGATURAN SWAY & BOB SAAT RELOAD ---
    [Header("RELOAD SWAY & BOB OVERRIDES")]
    [Tooltip("Multiplier sway saat reload (biasanya lebih kecil agar tidak pusing)")]
    public float reloadSwayMultiplier = 0.2f;
    
    [Tooltip("Batas maksimal sway saat reload")]
    public float reloadMaxSwayAmount = 2f;
    
    [Tooltip("Kecepatan manggut-manggut (bobbing) saat reload")]
    public float reloadBobSpeed = 2f;
    
    [Tooltip("Seberapa tinggi senjata bergerak (bobbing) saat reload")]
    public float reloadBobAmount = 0.01f;
}