using UnityEngine;

[CreateAssetMenu(fileName = "New Aim Data", menuName = "FIKIFOW FPS/Aim Data")]
public class FIKIFOW_AimDataSO : ScriptableObject
{
    [Header("AIM POINT ALIGNMENT")]
    [Tooltip("Gunakan objek 'AimPoint' di prefab untuk menentukan posisi bidikan?")]
    public bool useAimPoint = true;
    [Tooltip("Nama GameObject titik aim di dalam prefab senjata")]
    public string aimPointName = "AimPoint";
    
    [Tooltip("Sesuaikan posisi X kamera ke AimPoint?")]
    public bool alignX = true;
    [Tooltip("Sesuaikan posisi Y kamera ke AimPoint?")]
    public bool alignY = true;
    [Tooltip("Sesuaikan posisi Z (maju/mundur) kamera ke AimPoint?")]
    public bool alignZ = true;

    [Header("MANUAL TRANSFORM OFFSET")]
    [Tooltip("Offset manual posisi saat Aiming (tambahan / override)")]
    public Vector3 aimPositionOffset;
    [Tooltip("Offset manual rotasi saat Aiming")]
    public Vector3 aimRotationOffset;

    [Header("CUSTOM ARM POSES (AIMING)")]
    [Tooltip("Gunakan rotasi lengan kustom saat membidik?")]
    public bool useCustomArmPose = true;
    [Tooltip("Rotasi Lengan Kanan saat Aiming (Upper & Lower Arm)")]
    public FIKIFOW_ArmRotation rightArmRotation;
    [Tooltip("Rotasi Lengan Kiri saat Aiming (Upper & Lower Arm)")]
    public FIKIFOW_ArmRotation leftArmRotation;

    [Header("BONE CONSTRAINTS (AIMING)")]
    [Tooltip("Daftar constraint untuk tulang tangan saat aiming (Hand, Lower Arm, Upper Arm) - bisa Right atau Left")]
    public FIKIFOW_BoneConstraint[] boneConstraints = new FIKIFOW_BoneConstraint[0];

    [Header("PROCEDURAL MOTION OVERRIDE (SAAT AIMING)")]
    [Tooltip("Multiplier sway saat aiming (Biasanya lebih kecil dari pinggul agar stabil)")]
    public float swayMultiplier = 0.1f;
    [Tooltip("Batas maksimal sway saat aiming")]
    public float maxSwayAmount = 1f;
    [Tooltip("Kecepatan manggut-manggut (bobbing) saat aiming")]
    public float bobSpeed = 1f;
    [Tooltip("Seberapa tinggi senjata bergerak (bobbing) saat aiming")]
    public float bobAmount = 0.005f;
    [Tooltip("Kecepatan smoothing sway/recoil saat aiming")]
    public float swaySmoothness = 10f;
    [Tooltip("Kecepatan senjata kembali ke posisi awal saat aiming (recoil recovery)")]
    public float returnSpeed = 10f;
    [Tooltip("Besaran recoil recovery snapiness saat aiming")]
    public float snapiness = 25f;

    [Header("SETTINGS")]
    [Tooltip("Kecepatan transisi masuk/keluar dari mode Aiming")]
    public float aimSpeed = 10f;
}
