using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// PENTING: Gunakan kata kunci partial
public partial class FIKIFOWFPS2_ShooterEngine : MonoBehaviour
{
    // ==============================================================================
    // BAB 2: SPRINT SYSTEM PROCEDURAL MOTION
    // ==============================================================================
    
    // Status Sprint
    private bool isSprinting = false;
    private float currentSprintWeight = 0f;

    /// <summary>
    /// Panggil fungsi ini di dalam Update() pada file utama.
    /// </summary>
    private void HandleSprinting()
    {
        // 1. Validasi: Jangan sprint jika sedang menembak, reload, atau membidik
        if (sprintInput == null || sprintInput.action == null)
        {
            isSprinting = false;
        }
        else if (currentWeaponData == null || currentWeaponData.sprintData == null)
        {
            isSprinting = false;
        }
        else if (isReloading || isAiming || isBursting || (fireInput != null && fireInput.action.IsPressed()))
        {
            isSprinting = false;
        }
        else
        {
            // --- BARU: Validasi Pergerakan ---
            // Cek apakah tombol sprint ditekan
            bool sprintHeld = sprintInput.action.IsPressed();
            
            // Cek apakah pemain benar-benar menekan tombol maju (W atau Joystick maju)
            bool isMovingForward = false;
            
            // Kita coba ambil referensi dari FirstPersonEngine jika ada
            FIKIFOWFPS1_FirstPersonEngine fpEngine = GetComponent<FIKIFOWFPS1_FirstPersonEngine>();
            if (fpEngine != null && fpEngine.moveInput != null && fpEngine.moveInput.action != null)
            {
                Vector2 moveRaw = fpEngine.moveInput.action.ReadValue<Vector2>();
                if (moveRaw.y > 0.1f) isMovingForward = true;
            }

            // Karakter baru bisa sprint procedural JIKA tombol ditekan dan sedang bergerak maju
            isSprinting = sprintHeld && isMovingForward;

            // Jika sprint data memiliki switch force (untuk debugging), paksa pose sprint aktif
            if (currentWeaponData != null && currentWeaponData.sprintData != null && currentWeaponData.sprintData.forceSprintPose)
            {
                isSprinting = true;
            }
        }

        // 2. Kalkulasi Bobot Transisi (Lerp)
        float targetWeight = isSprinting ? 1f : 0f;
        float speed = (currentWeaponData != null && currentWeaponData.sprintData != null) ? currentWeaponData.sprintData.sprintTransitionSpeed : 8f;
        
        currentSprintWeight = Mathf.Lerp(currentSprintWeight, targetWeight, Time.deltaTime * speed);
    }
    
    /// <summary>
    /// Fungsi ini akan menimpa (override) perhitungan sway, bob, dan rotasi lengan 
    /// jika tombol sprint sedang ditekan.
    /// </summary>
    private void ApplySprintProceduralMotion(
        ref float currentSwayMult, 
        ref float currentMaxSway, 
        ref float currentBobSpeed, 
        ref float currentBobAmount,
        ref float currentSwaySmoothness,
        ref Vector3 targetWeaponPos,
        ref Quaternion targetWeaponRot)
    {
        if (currentWeaponData.sprintData == null || currentSprintWeight <= 0f) return;

        FIKIFOW_SprintDataSO sprintSO = currentWeaponData.sprintData;

        // 1. Override Parameter Motion (Blend dari Hip-fire/Aim ke Sprint)
        currentSwayMult = Mathf.Lerp(currentSwayMult, sprintSO.swayMultiplier, currentSprintWeight);
        currentMaxSway = Mathf.Lerp(currentMaxSway, sprintSO.maxSwayAmount, currentSprintWeight);
        currentBobSpeed = Mathf.Lerp(currentBobSpeed, sprintSO.bobSpeed, currentSprintWeight);
        currentBobAmount = Mathf.Lerp(currentBobAmount, sprintSO.bobAmount, currentSprintWeight);
        currentSwaySmoothness = Mathf.Lerp(currentSwaySmoothness, sprintSO.swaySmoothness, currentSprintWeight);

        // 2. Kalkulasi Offset Posisi dan Rotasi Senjata (Turun/Miring ke bawah)
        Vector3 sprintPos = initialWeaponLocalPos + sprintSO.sprintPositionOffset;
        Quaternion sprintRot = initialWeaponLocalRot * Quaternion.Euler(sprintSO.sprintRotationOffset);

        targetWeaponPos = Vector3.Lerp(targetWeaponPos, sprintPos, currentSprintWeight);
        targetWeaponRot = Quaternion.Slerp(targetWeaponRot, sprintRot, currentSprintWeight);
    }

    /// <summary>
    /// Menerapkan rotasi lengan kustom khusus saat berlari.
    /// </summary>
    private void ApplySprintArmPoses(
        ref FIKIFOW_ArmRotation currentRightArmPose, 
        ref FIKIFOW_ArmRotation currentLeftArmPose, 
        List<FIKIFOW_BoneConstraint> activeConstraints)
    {
        if (currentWeaponData.sprintData == null || !currentWeaponData.sprintData.useCustomArmPose || currentSprintWeight <= 0f) return;

        FIKIFOW_SprintDataSO sprintSO = currentWeaponData.sprintData;

        if (currentRightArmPose == null) currentRightArmPose = new FIKIFOW_ArmRotation();
        if (currentLeftArmPose == null) currentLeftArmPose = new FIKIFOW_ArmRotation();

        // Lerp rotasi lengan atas dan bawah
        currentRightArmPose.upperArm = Vector3.Lerp(currentRightArmPose.upperArm, sprintSO.rightArmRotation.upperArm, currentSprintWeight);
        currentRightArmPose.lowerArm = Vector3.Lerp(currentRightArmPose.lowerArm, sprintSO.rightArmRotation.lowerArm, currentSprintWeight);

        currentLeftArmPose.upperArm = Vector3.Lerp(currentLeftArmPose.upperArm, sprintSO.leftArmRotation.upperArm, currentSprintWeight);
        currentLeftArmPose.lowerArm = Vector3.Lerp(currentLeftArmPose.lowerArm, sprintSO.leftArmRotation.lowerArm, currentSprintWeight);

        // Blend sprint constraints (mengganti constraint aim/hip-fire)
        if (sprintSO.boneConstraints != null && sprintSO.boneConstraints.Length > 0)
        {
            activeConstraints.Clear();
            foreach (var sprintConstraint in sprintSO.boneConstraints)
            {
                if (sprintConstraint != null)
                {
                    activeConstraints.Add(sprintConstraint);
                }
            }
        }
    }
}