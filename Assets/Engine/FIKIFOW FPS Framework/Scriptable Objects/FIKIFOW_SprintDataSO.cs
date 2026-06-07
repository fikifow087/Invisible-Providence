using UnityEngine;

[CreateAssetMenu(fileName = "New Sprint Data", menuName = "FIKIFOW FPS/Sprint Data")]
public class FIKIFOW_SprintDataSO : ScriptableObject
{
    [Header("MANUAL TRANSFORM OFFSET")]
    [Tooltip("Offset manual posisi saat Sprint (contoh: ditarik ke bawah badan)")]
    public Vector3 sprintPositionOffset = new Vector3(0.1f, -0.2f, -0.1f);
    [Tooltip("Offset manual rotasi saat Sprint (contoh: miring ke samping/bawah)")]
    public Vector3 sprintRotationOffset = new Vector3(-20f, 30f, -10f);

    [Header("CUSTOM ARM POSES (SPRINTING)")]
    [Tooltip("Gunakan rotasi lengan kustom saat lari?")]
    public bool useCustomArmPose = true;
    [Tooltip("Rotasi Lengan Kanan saat Sprint (Upper & Lower Arm)")]
    public FIKIFOW_ArmRotation rightArmRotation;
    [Tooltip("Rotasi Lengan Kiri saat Sprint (Upper & Lower Arm)")]
    public FIKIFOW_ArmRotation leftArmRotation;

    [Header("BONE CONSTRAINTS (SPRINTING)")]
    [Tooltip("Daftar constraint untuk tulang tangan saat sprint")]
    public FIKIFOW_BoneConstraint[] boneConstraints = new FIKIFOW_BoneConstraint[0];

    [Header("PROCEDURAL MOTION OVERRIDE (SAAT SPRINT)")]
    [Tooltip("Multiplier sway saat sprint (Biasanya lebih besar karena lari)")]
    public float swayMultiplier = 1.5f;
    [Tooltip("Batas maksimal sway saat sprint")]
    public float maxSwayAmount = 8f;
    [Tooltip("Kecepatan manggut-manggut (bobbing) saat sprint")]
    public float bobSpeed = 6f;
    [Tooltip("Seberapa tinggi senjata bergerak (bobbing) saat sprint")]
    public float bobAmount = 0.05f;
    [Tooltip("Kecepatan smoothing sway saat sprint")]
    public float swaySmoothness = 12f;

    [Header("SETTINGS")]
    [Tooltip("Kecepatan transisi masuk/keluar dari pose Sprint")]
    public float sprintTransitionSpeed = 8f;

    [Header("DEBUG / POSE PREVIEW")]
    [Tooltip("Paksa masuk ke pose sprint meskipun tombol sprint tidak ditekan. Berguna untuk mengatur posisi sprint di editor dan preview pose.")]
    public bool forceSprintPose = false;
}