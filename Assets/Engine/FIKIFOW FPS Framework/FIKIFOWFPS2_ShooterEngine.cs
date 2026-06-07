using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("FIKIFOW FPS/2 - Shooter Engine")]
[RequireComponent(typeof(AudioSource))]
public partial class FIKIFOWFPS2_ShooterEngine : MonoBehaviour
{
    [Header("Weapon Loadout (Inventory)")]
    public FIKIFOW_WeaponDataSO[] weaponSlots; 
    private int currentWeaponIndex = -1;
    
    [Header("Dynamic Data")]
    private int currentAmmo;
    private FIKIFOW_WeaponDataSO currentWeaponData;
    private GameObject currentWeaponInstance;
    
    private ParticleSystem currentMuzzleFlash;
    private Transform rightHandTarget;
    private Transform leftHandTarget;

    // Penampung kordinat asli senjata (Titik 0)
    private Vector3 initialWeaponLocalPos;
    private Quaternion initialWeaponLocalRot;

    // VARIABEL RECOIL PEGAS 
    private Vector3 currentRecoilPosition;
    private Vector3 targetRecoilPosition;
    private Vector3 currentRecoilRotation;
    private Vector3 targetRecoilRotation;

    [Header("References")]
    public Transform playerCamera;
    private Animator animator;
    private AudioSource audioSource; 

    [Header("Input Actions")]
    public InputActionReference fireInput;
    public InputActionReference reloadInput;
    public InputActionReference[] numberInputs; 
    public InputActionReference lookInput; 
    public InputActionReference aimInput; 
    public InputActionReference sprintInput;

    private float nextFireTime = 0f;
    private bool isBursting = false;
    private int currentShootSoundIndex = 0; 
    
    // Status Aiming
    private bool isAiming = false;
    private float currentAimWeight = 0f;
    private Transform currentAimPoint;

    // Status Reload
    private bool isReloading = false;
    private float reloadEndTime = 0f;
    private float currentReloadWeight = 0f; // <--- BARU: Penampung transisi mulus reload

    void Awake()
    {
        if (lookInput == null)
        {
            FIKIFOWFPS1_FirstPersonEngine fpEngine = GetComponent<FIKIFOWFPS1_FirstPersonEngine>();
            if (fpEngine != null) lookInput = fpEngine.lookInput;
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (weaponSlots.Length > 0) EquipWeapon(0); 
    }

    void Update()
    {
        HandleWeaponSwitching();
        HandleAiming(); 
        HandleShooting();
        HandleReloading();
        HandleReloadCompletion(); 
        UpdateWeaponOffsets(); 
        HandleProceduralMotion(); 
    }

    private void UpdateWeaponOffsets()
    {
        if (currentWeaponInstance == null || currentWeaponData == null) return;

        initialWeaponLocalPos = currentWeaponData.spawnPositionOffset;
        initialWeaponLocalRot = Quaternion.Euler(currentWeaponData.spawnRotationOffset);

        if (currentWeaponData.motionData == null || !currentWeaponData.motionData.enableProceduralMotion)
        {
            currentWeaponInstance.transform.localPosition = initialWeaponLocalPos;
            currentWeaponInstance.transform.localRotation = initialWeaponLocalRot;
        }
    }

    private void HandleAiming()
    {
        if (aimInput == null || aimInput.action == null)
        {
            isAiming = false;
        }
        else if (currentWeaponData == null || currentWeaponData.aimData == null)
        {
            isAiming = false;
        }
        else
        {
            isAiming = aimInput.action.IsPressed();
        }

        float targetWeight = isAiming ? 1f : 0f;
        float speed = (currentWeaponData != null && currentWeaponData.aimData != null) ? currentWeaponData.aimData.aimSpeed : 10f;
        currentAimWeight = Mathf.Lerp(currentAimWeight, targetWeight, Time.deltaTime * speed);
    }

    private Vector2 GetLookInput()
    {
        if (lookInput != null && lookInput.action != null)
        {
            return lookInput.action.ReadValue<Vector2>();
        }
        if (Mouse.current != null)
        {
            return Mouse.current.delta.ReadValue();
        }
        return Vector2.zero;
    }

    // ==============================================================================
    // --- UPGRADE: STRUKTUR BASE STANCE & EXECUTION ORDER YANG BENAR ---
    // ==============================================================================
    private void HandleProceduralMotion()
    {
        if (currentWeaponInstance == null || currentWeaponData == null) return;
        
        FIKIFOW_WeaponMotionDataSO motionData = currentWeaponData.motionData;
        if (motionData == null || !motionData.enableProceduralMotion) return;

        // HAPUS BLOK "RELOAD PRIORITY" YANG LAMA DI SINI 
        // (Yang berisi if(isReloading) { ... return; })

        HandleSprinting(); // Update bobot lari

        // 1. TENTUKAN MULTIPLIER DEFAULT (Mode Hip-Fire)
        float currentSwayMult = motionData.swayMultiplier;
        float currentMaxSway = motionData.maxSwayAmount;
        float currentBobSpeed = motionData.bobSpeed;
        float currentBobAmount = motionData.bobAmount;
        float currentSwaySmoothness = motionData.swaySmoothness;
        float currentSnapiness = motionData.snapiness;
        float currentReturnSpeed = motionData.returnSpeed;

        // 2. TENTUKAN BASE STANCE DEFAULT (Mode Hip-Fire)
        Vector3 basePosition = initialWeaponLocalPos;
        Quaternion baseRotation = initialWeaponLocalRot;

        // 3. PRIORITAS KEDUA: SPRINTING
        if (currentWeaponData.sprintData != null && currentSprintWeight > 0f)
        {
            ApplySprintProceduralMotion(
                ref currentSwayMult,
                ref currentMaxSway,
                ref currentBobSpeed,
                ref currentBobAmount,
                ref currentSwaySmoothness,
                ref basePosition,
                ref baseRotation);
        }

        // 4. PRIORITAS UTAMA: AIMING
        if (currentWeaponData.aimData != null && currentAimWeight > 0f)
        {
            currentSwayMult = Mathf.Lerp(currentSwayMult, currentWeaponData.aimData.swayMultiplier, currentAimWeight);
            currentMaxSway = Mathf.Lerp(currentMaxSway, currentWeaponData.aimData.maxSwayAmount, currentAimWeight);
            currentBobSpeed = Mathf.Lerp(currentBobSpeed, currentWeaponData.aimData.bobSpeed, currentAimWeight);
            currentBobAmount = Mathf.Lerp(currentBobAmount, currentWeaponData.aimData.bobAmount, currentAimWeight);
            currentSwaySmoothness = Mathf.Lerp(currentSwaySmoothness, currentWeaponData.aimData.swaySmoothness, currentAimWeight);
            currentSnapiness = Mathf.Lerp(currentSnapiness, currentWeaponData.aimData.snapiness, currentAimWeight);
            currentReturnSpeed = Mathf.Lerp(currentReturnSpeed, currentWeaponData.aimData.returnSpeed, currentAimWeight);

            Vector3 targetAimPos = initialWeaponLocalPos;
            Quaternion targetAimRot = initialWeaponLocalRot;

            if (currentWeaponData.aimData.useAimPoint && currentAimPoint != null)
            {
                targetAimRot = Quaternion.Inverse(currentAimPoint.localRotation);
                Vector3 requiredOffset = -(targetAimRot * currentAimPoint.localPosition);

                targetAimPos = new Vector3(
                    currentWeaponData.aimData.alignX ? requiredOffset.x : initialWeaponLocalPos.x,
                    currentWeaponData.aimData.alignY ? requiredOffset.y : initialWeaponLocalPos.y,
                    currentWeaponData.aimData.alignZ ? requiredOffset.z : initialWeaponLocalPos.z
                );
            }

            targetAimPos += currentWeaponData.aimData.aimPositionOffset;
            targetAimRot *= Quaternion.Euler(currentWeaponData.aimData.aimRotationOffset);

            basePosition = Vector3.Lerp(basePosition, targetAimPos, currentAimWeight);
            baseRotation = Quaternion.Slerp(baseRotation, targetAimRot, currentAimWeight);
        }

        // --- BARU: PRIORITAS MUTLAK (RELOAD) ---
        // Jika sedang reload, offset posisi/rotasinya akan menimpa AIM dan SPRINT.
        // Kita juga meng-override multiplier dengan nilai sway/bob khusus reload.
        if (isReloading)
        {
            Vector3 targetReloadPos = initialWeaponLocalPos + motionData.reloadPositionOffset;
            Quaternion targetReloadRot = initialWeaponLocalRot * Quaternion.Euler(motionData.reloadRotationOffset);

            // Transisi mulus ke pose reload
            basePosition = Vector3.Lerp(basePosition, targetReloadPos, Time.deltaTime * motionData.reloadMovementSpeed);
            baseRotation = Quaternion.Slerp(baseRotation, targetReloadRot, Time.deltaTime * motionData.reloadMovementSpeed);

            // Gunakan nilai sway & bob khusus reload agar tidak berlebihan
            currentSwayMult = motionData.reloadSwayMultiplier;
            currentMaxSway = motionData.reloadMaxSwayAmount;
            currentBobSpeed = motionData.reloadBobSpeed;
            currentBobAmount = motionData.reloadBobAmount;
            
            // Opsional: mempercepat recovery saat reload agar gerakan mouse lebih responsif
            currentSwaySmoothness = 15f; 
        }

        // --- BARU: PRIORITAS MUTLAK (RELOAD) ---
        // Kita menggunakan currentReloadWeight yang sudah dihaluskan di luar fungsi ini
        if (currentReloadWeight > 0f)
        {
            Vector3 targetReloadPos = initialWeaponLocalPos + motionData.reloadPositionOffset;
            Quaternion targetReloadRot = initialWeaponLocalRot * Quaternion.Euler(motionData.reloadRotationOffset);

            // Blending mulus ke pose reload
            basePosition = Vector3.Lerp(basePosition, targetReloadPos, currentReloadWeight);
            baseRotation = Quaternion.Slerp(baseRotation, targetReloadRot, currentReloadWeight);

            // Blending parameter sway & bob (dari mode aktif sebelumnya menuju mode khusus reload)
            currentSwayMult = Mathf.Lerp(currentSwayMult, motionData.reloadSwayMultiplier, currentReloadWeight);
            currentMaxSway = Mathf.Lerp(currentMaxSway, motionData.reloadMaxSwayAmount, currentReloadWeight);
            currentBobSpeed = Mathf.Lerp(currentBobSpeed, motionData.reloadBobSpeed, currentReloadWeight);
            currentBobAmount = Mathf.Lerp(currentBobAmount, motionData.reloadBobAmount, currentReloadWeight);
            
            // Percepat recovery senjata agar stabil di posisi reload
            currentSwaySmoothness = Mathf.Lerp(currentSwaySmoothness, 15f, currentReloadWeight); 
        }

        // 5. KALKULASI RECOIL PEGAS
        targetRecoilPosition = Vector3.Lerp(targetRecoilPosition, Vector3.zero, currentReturnSpeed * Time.deltaTime);
        currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, targetRecoilPosition, currentSnapiness * Time.deltaTime);

        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, currentReturnSpeed * Time.deltaTime);
        currentRecoilRotation = Vector3.Lerp(currentRecoilRotation, targetRecoilRotation, currentSnapiness * Time.deltaTime);

        // 6. KALKULASI PROCEDURAL SWAY 
        Vector2 look = GetLookInput();
        float dt = Time.deltaTime;
        if (dt <= 0f) dt = 0.01666f;
        Vector2 normalizedLook = look * (0.01666f / dt);

        float swayX = Mathf.Clamp(-normalizedLook.x * currentSwayMult, -currentMaxSway, currentMaxSway);
        float swayY = Mathf.Clamp(normalizedLook.y * currentSwayMult, -currentMaxSway, currentMaxSway);
        Quaternion swayRotation = Quaternion.Euler(swayY, swayX, 0f);

        float posSwayLimit = currentMaxSway * 0.005f; 
        float swayPosX = Mathf.Clamp(-normalizedLook.x * currentSwayMult * 0.0005f, -posSwayLimit, posSwayLimit);
        float swayPosY = Mathf.Clamp(-normalizedLook.y * currentSwayMult * 0.0005f, -posSwayLimit, posSwayLimit);

        // 7. KALKULASI PROCEDURAL BOBBING 
        float bobY = Mathf.Sin(Time.time * currentBobSpeed) * currentBobAmount;
        float bobX = Mathf.Cos(Time.time * currentBobSpeed / 2f) * (currentBobAmount / 2f); 

        // 8. ADDITIVE COMBINATION 
        Vector3 finalPosition = basePosition + new Vector3(bobX + swayPosX, bobY + swayPosY, 0f) + currentRecoilPosition;
        Quaternion finalRotation = baseRotation * swayRotation * Quaternion.Euler(currentRecoilRotation);

        // 9. APLIKASIKAN KE SENJATA 
        currentWeaponInstance.transform.localPosition = Vector3.Lerp(currentWeaponInstance.transform.localPosition, finalPosition, Time.deltaTime * currentSwaySmoothness);
        currentWeaponInstance.transform.localRotation = Quaternion.Slerp(currentWeaponInstance.transform.localRotation, finalRotation, Time.deltaTime * currentSwaySmoothness);
    }
    // ==============================================================================

    void LateUpdate()
    {
        if (animator == null || currentWeaponData == null) return;

        // 0. PROSES CUSTOM ARM POSE (ADDITIVE)
        FIKIFOW_ArmRotation currentRightArmPose = null;
        FIKIFOW_ArmRotation currentLeftArmPose = null;
        List<FIKIFOW_BoneConstraint> activeConstraints = new List<FIKIFOW_BoneConstraint>();
        bool useArm = false;

        if (currentWeaponData.useCustomArmPose)
        {
            useArm = true;
            currentRightArmPose = new FIKIFOW_ArmRotation { upperArm = currentWeaponData.rightArmRotation.upperArm, lowerArm = currentWeaponData.rightArmRotation.lowerArm };
            currentLeftArmPose = new FIKIFOW_ArmRotation { upperArm = currentWeaponData.leftArmRotation.upperArm, lowerArm = currentWeaponData.leftArmRotation.lowerArm };
            
            if (currentWeaponData.boneConstraints != null)
            {
                foreach (var constraint in currentWeaponData.boneConstraints)
                {
                    if (constraint != null) activeConstraints.Add(constraint);
                }
            }
        }

        if (currentWeaponData.aimData != null && currentWeaponData.aimData.useCustomArmPose && currentAimWeight > 0f)
        {
            useArm = true;
            if (currentRightArmPose == null) currentRightArmPose = new FIKIFOW_ArmRotation();
            if (currentLeftArmPose == null) currentLeftArmPose = new FIKIFOW_ArmRotation();

            FIKIFOW_ArmRotation aimRight = currentWeaponData.aimData.rightArmRotation;
            FIKIFOW_ArmRotation aimLeft = currentWeaponData.aimData.leftArmRotation;

            currentRightArmPose.upperArm = Vector3.Lerp(currentRightArmPose.upperArm, aimRight.upperArm, currentAimWeight);
            currentRightArmPose.lowerArm = Vector3.Lerp(currentRightArmPose.lowerArm, aimRight.lowerArm, currentAimWeight);

            currentLeftArmPose.upperArm = Vector3.Lerp(currentLeftArmPose.upperArm, aimLeft.upperArm, currentAimWeight);
            currentLeftArmPose.lowerArm = Vector3.Lerp(currentLeftArmPose.lowerArm, aimLeft.lowerArm, currentAimWeight);

            if (currentWeaponData.aimData.boneConstraints != null && currentWeaponData.aimData.boneConstraints.Length > 0)
            {
                activeConstraints.Clear();
                foreach (var aimConstraint in currentWeaponData.aimData.boneConstraints)
                {
                    if (aimConstraint != null) activeConstraints.Add(aimConstraint);
                }
            }
        }

        if (currentWeaponData.sprintData != null && currentWeaponData.sprintData.useCustomArmPose && currentSprintWeight > 0f)
        {
            useArm = true;
            ApplySprintArmPoses(ref currentRightArmPose, ref currentLeftArmPose, activeConstraints);
        }

        if (useArm)
        {
            ApplyAdditiveArmRotation(currentRightArmPose, true);
            ApplyAdditiveArmRotation(currentLeftArmPose, false);
            ApplyBoneConstraints(activeConstraints);
        }

        // 1. PROSES IK STRETCH TANGAN KANAN 
        if (currentWeaponData.allowRightHandStretch && rightHandTarget != null)
        {
            Transform rHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (rHand != null)
            {
                if (currentWeaponData.rightStretchMode == FIKIFOW_StretchMode.PergelanganTangan_Hand)
                {
                    rHand.position = rightHandTarget.position;
                    rHand.rotation = rightHandTarget.rotation;
                }
                else if (currentWeaponData.rightStretchMode == FIKIFOW_StretchMode.Siku_LowerArm)
                {
                    Transform rLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    if (rLowerArm != null)
                    {
                        Vector3 offset = rightHandTarget.position - rHand.position;
                        rLowerArm.position += offset;
                        rHand.rotation = rightHandTarget.rotation;
                    }
                }
                else if (currentWeaponData.rightStretchMode == FIKIFOW_StretchMode.PangkalLengan_UpperArm)
                {
                    Transform rUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                    if (rUpperArm != null)
                    {
                        Vector3 offset = rightHandTarget.position - rHand.position;
                        rUpperArm.position += offset;
                        rHand.rotation = rightHandTarget.rotation; 
                    }
                }
            }
        }

        // 2. PROSES IK STRETCH TANGAN KIRI 
        if (currentWeaponData.allowLeftHandStretch && leftHandTarget != null)
        {
            Transform lHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            if (lHand != null)
            {
                if (currentWeaponData.leftStretchMode == FIKIFOW_StretchMode.PergelanganTangan_Hand)
                {
                    lHand.position = leftHandTarget.position;
                    lHand.rotation = leftHandTarget.rotation;
                }
                else if (currentWeaponData.leftStretchMode == FIKIFOW_StretchMode.Siku_LowerArm)
                {
                    Transform lLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    if (lLowerArm != null)
                    {
                        Vector3 offset = leftHandTarget.position - lHand.position;
                        lLowerArm.position += offset;
                        lHand.rotation = leftHandTarget.rotation;
                    }
                }
                else if (currentWeaponData.leftStretchMode == FIKIFOW_StretchMode.PangkalLengan_UpperArm)
                {
                    Transform lUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                    if (lUpperArm != null)
                    {
                        Vector3 offset = leftHandTarget.position - lHand.position;
                        lUpperArm.position += offset;
                        lHand.rotation = leftHandTarget.rotation; 
                    }
                }
            }
        }

        // 3. PROSES CUSTOM FINGER POSE (ADDITIVE) 
        if (currentWeaponData.useCustomFingerPose)
        {
            ApplyAdditiveFingerRotation(currentWeaponData.rightHandFingers, true);
            ApplyAdditiveFingerRotation(currentWeaponData.leftHandFingers, false);
        }
    }

    private void ApplyAdditiveFingerRotation(FIKIFOW_HandGripPose poseData, bool isRightHand)
    {
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightThumbProximal : HumanBodyBones.LeftThumbProximal, poseData.thumb.proximal);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightThumbIntermediate : HumanBodyBones.LeftThumbIntermediate, poseData.thumb.intermediate);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightThumbDistal : HumanBodyBones.LeftThumbDistal, poseData.thumb.distal);

        ApplySingleJoint(isRightHand ? HumanBodyBones.RightIndexProximal : HumanBodyBones.LeftIndexProximal, poseData.index.proximal);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightIndexIntermediate : HumanBodyBones.LeftIndexIntermediate, poseData.index.intermediate);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightIndexDistal : HumanBodyBones.LeftIndexDistal, poseData.index.distal);

        ApplySingleJoint(isRightHand ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.LeftMiddleProximal, poseData.middle.proximal);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightMiddleIntermediate : HumanBodyBones.LeftMiddleIntermediate, poseData.middle.intermediate);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightMiddleDistal : HumanBodyBones.LeftMiddleDistal, poseData.middle.distal);

        ApplySingleJoint(isRightHand ? HumanBodyBones.RightRingProximal : HumanBodyBones.LeftRingProximal, poseData.ring.proximal);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightRingIntermediate : HumanBodyBones.LeftRingIntermediate, poseData.ring.intermediate);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightRingDistal : HumanBodyBones.LeftRingDistal, poseData.ring.distal);

        ApplySingleJoint(isRightHand ? HumanBodyBones.RightLittleProximal : HumanBodyBones.LeftLittleProximal, poseData.little.proximal);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightLittleIntermediate : HumanBodyBones.LeftLittleIntermediate, poseData.little.intermediate);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightLittleDistal : HumanBodyBones.LeftLittleDistal, poseData.little.distal);
    }

    private void ApplyAdditiveArmRotation(FIKIFOW_ArmRotation armPose, bool isRightHand)
    {
        if (armPose == null) return;

        ApplySingleJoint(isRightHand ? HumanBodyBones.RightUpperArm : HumanBodyBones.LeftUpperArm, armPose.upperArm);
        ApplySingleJoint(isRightHand ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm, armPose.lowerArm);
    }

    private void ApplyBoneConstraints(List<FIKIFOW_BoneConstraint> constraints)
    {
        if (constraints == null || constraints.Count == 0) return;

        foreach (var constraint in constraints)
        {
            if (constraint == null) continue;

            HumanBodyBones targetBone = GetBoneFromEnum(constraint.armSide, constraint.boneType);
            Transform joint = animator.GetBoneTransform(targetBone);
            
            if (joint == null) continue;

            Vector3 euler = joint.localRotation.eulerAngles;
            euler.x = NormalizeAngle(euler.x);
            euler.y = NormalizeAngle(euler.y);
            euler.z = NormalizeAngle(euler.z);

            if (constraint.lockX) euler.x = 0f;
            else euler.x = Mathf.Clamp(euler.x, constraint.minRotationX, constraint.maxRotationX);

            if (constraint.lockY) euler.y = 0f;
            else euler.y = Mathf.Clamp(euler.y, constraint.minRotationY, constraint.maxRotationY);

            if (constraint.lockZ) euler.z = 0f;
            else euler.z = Mathf.Clamp(euler.z, constraint.minRotationZ, constraint.maxRotationZ);

            joint.localRotation = Quaternion.Euler(euler);
        }
    }

    private HumanBodyBones GetBoneFromEnum(FIKIFOW_ArmSide armSide, FIKIFOW_BoneType boneType)
    {
        if (armSide == FIKIFOW_ArmSide.Right)
        {
            return boneType switch
            {
                FIKIFOW_BoneType.Hand => HumanBodyBones.RightHand,
                FIKIFOW_BoneType.LowerArm => HumanBodyBones.RightLowerArm,
                FIKIFOW_BoneType.UpperArm => HumanBodyBones.RightUpperArm,
                _ => HumanBodyBones.RightHand
            };
        }
        else
        {
            return boneType switch
            {
                FIKIFOW_BoneType.Hand => HumanBodyBones.LeftHand,
                FIKIFOW_BoneType.LowerArm => HumanBodyBones.LeftLowerArm,
                FIKIFOW_BoneType.UpperArm => HumanBodyBones.LeftUpperArm,
                _ => HumanBodyBones.LeftHand
            };
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private void ApplySingleJoint(HumanBodyBones bone, Vector3 additiveRotation)
    {
        if (additiveRotation == Vector3.zero) return; 

        Transform joint = animator.GetBoneTransform(bone);
        if (joint != null)
        {
            joint.localRotation *= Quaternion.Euler(additiveRotation);
        }
    }

    private void HandleWeaponSwitching()
    {
        for (int i = 0; i < numberInputs.Length; i++)
        {
            if (i < weaponSlots.Length && numberInputs[i].action.WasPressedThisFrame())
            {
                if (currentWeaponIndex == i) UnequipWeapon();
                else EquipWeapon(i);
            }
        }
    }

    private void EquipWeapon(int index)
    {
        if (weaponSlots[index] == null) return;
        
        UnequipWeapon(); 

        currentWeaponIndex = index;
        currentWeaponData = weaponSlots[index];
        currentAmmo = currentWeaponData.ammoCapacity;
        currentShootSoundIndex = 0; 

        currentWeaponInstance = Instantiate(currentWeaponData.weaponPrefab, playerCamera);
        
        initialWeaponLocalPos = currentWeaponData.spawnPositionOffset;
        initialWeaponLocalRot = Quaternion.Euler(currentWeaponData.spawnRotationOffset);

        currentWeaponInstance.transform.localPosition = initialWeaponLocalPos;
        currentWeaponInstance.transform.localRotation = initialWeaponLocalRot;

        currentRecoilPosition = Vector3.zero;
        targetRecoilPosition = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        targetRecoilRotation = Vector3.zero;

        rightHandTarget = FindChildRecursive(currentWeaponInstance.transform, currentWeaponData.rightHandGripName);
        leftHandTarget = FindChildRecursive(currentWeaponInstance.transform, currentWeaponData.leftHandGripName);
        
        if (currentWeaponData.aimData != null && currentWeaponData.aimData.useAimPoint)
            currentAimPoint = FindChildRecursive(currentWeaponInstance.transform, currentWeaponData.aimData.aimPointName);
        else
            currentAimPoint = null;

        Transform muzzleObj = FindChildRecursive(currentWeaponInstance.transform, currentWeaponData.muzzleFlashName);
        if (muzzleObj != null) currentMuzzleFlash = muzzleObj.GetComponent<ParticleSystem>();
        
        if (animator) animator.SetBool("IsEquipped", true);
    }

    private void UnequipWeapon()
    {
        if (currentWeaponInstance != null) Destroy(currentWeaponInstance);
        
        currentWeaponData = null;
        currentWeaponIndex = -1;
        rightHandTarget = null;
        leftHandTarget = null;
        currentMuzzleFlash = null;
        isReloading = false;
        reloadEndTime = 0f;
        
        if (animator) animator.SetBool("IsEquipped", false); 
    }

    private void HandleShooting()
    {
        if (currentWeaponData == null || currentWeaponData.fireMode == FIKIFOW_FireMode.SAFE || isBursting || isReloading) return;

        bool isFiring = currentWeaponData.fireMode == FIKIFOW_FireMode.FULL ? fireInput.action.IsPressed() : fireInput.action.WasPressedThisFrame();

        if (isFiring && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                if (currentWeaponData.fireMode == FIKIFOW_FireMode.BURST) StartCoroutine(HandleBurstFire());
                else FireBullet();
            }
            else
            {
                nextFireTime = Time.time + GetCurrentFireRate();
                PlayEmptySound(); 
            }
        }
    }

    private void FireBullet()
    {
        currentAmmo--;
        nextFireTime = Time.time + GetCurrentFireRate();
        
        if (currentMuzzleFlash != null) currentMuzzleFlash.Play();

        PlayShootSound(); 

        if (currentWeaponData.motionData != null && currentWeaponData.motionData.enableProceduralMotion)
        {
            FIKIFOW_WeaponMotionDataSO mData = currentWeaponData.motionData;
            
            targetRecoilPosition += new Vector3(0f, 0f, mData.recoilKickbackZ);
            float randY = Random.Range(-mData.recoilRandomY, mData.recoilRandomY);
            float randZ = Random.Range(-mData.recoilRandomZ, mData.recoilRandomZ);
            targetRecoilRotation += new Vector3(mData.recoilRotationX, randY, randZ);
        }
    }

    private IEnumerator HandleBurstFire()
    {
        isBursting = true;
        for (int i = 0; i < currentWeaponData.burstBulletCount; i++)
        {
            if (currentAmmo <= 0) break;
            FireBullet();
            yield return new WaitForSeconds(GetCurrentFireRate());
        }
        isBursting = false;
    }

    private float GetCurrentFireRate()
    {
        if (currentWeaponData == null) return 0.1f;

        return currentWeaponData.fireMode switch
        {
            FIKIFOW_FireMode.SEMI => currentWeaponData.semiFireRate,
            FIKIFOW_FireMode.BURST => currentWeaponData.burstFireRate,
            FIKIFOW_FireMode.FULL => currentWeaponData.fullFireRate,
            _ => 0.1f,
        };
    }

    private void HandleReloading()
    {
        // 1. Hitung transisi bobot reload (Lerp nilai 0 ke 1)
        float targetWeight = isReloading ? 1f : 0f;
        float speed = (currentWeaponData != null && currentWeaponData.motionData != null) ? currentWeaponData.motionData.reloadMovementSpeed : 8f;
        currentReloadWeight = Mathf.Lerp(currentReloadWeight, targetWeight, Time.deltaTime * speed);

        if (isReloading) return; 

        // 2. Deteksi input untuk mulai reload
        if (reloadInput.action.WasPressedThisFrame() && currentWeaponData != null && currentAmmo < currentWeaponData.ammoCapacity)
        {
            isReloading = true;
            reloadEndTime = Time.time + currentWeaponData.reloadTime;
            PlayReloadSound();
        }
    }

    private void HandleReloadCompletion()
    {
        if (!isReloading) return;

        if (Time.time >= reloadEndTime)
        {
            currentAmmo = currentWeaponData.ammoCapacity;
            isReloading = false;
            PlayReloadCompleteSound();
        }
    }

    private void PlayShootSound()
    {
        if (currentWeaponData.shootClips == null || currentWeaponData.shootClips.Length == 0 || audioSource == null) return;
        AudioClip clipToPlay = null;

        if (currentWeaponData.shootClips.Length == 1)
        {
            clipToPlay = currentWeaponData.shootClips[0];
        }
        else
        {
            if (currentWeaponData.shootClipRandomizeMode == FIKIFOW_SoundPlayMode.Berbaris)
            {
                clipToPlay = currentWeaponData.shootClips[currentShootSoundIndex];
                currentShootSoundIndex++;
                if (currentShootSoundIndex >= currentWeaponData.shootClips.Length) currentShootSoundIndex = 0;
            }
            else if (currentWeaponData.shootClipRandomizeMode == FIKIFOW_SoundPlayMode.Randomize)
            {
                int randomIndex = Random.Range(0, currentWeaponData.shootClips.Length);
                clipToPlay = currentWeaponData.shootClips[randomIndex];
            }
        }

        if (clipToPlay != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); 
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    private void PlayReloadSound()
    {
        if (currentWeaponData.reloadClip != null && audioSource != null)
        {
            audioSource.pitch = 1f; 
            audioSource.PlayOneShot(currentWeaponData.reloadClip);
        }
    }

    private void PlayReloadCompleteSound()
    {
        if (currentWeaponData.reloadCompleteClip != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(currentWeaponData.reloadCompleteClip);
        }
    }

    private void PlayEmptySound()
    {
        if (currentWeaponData.emptyAmmoClip != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(currentWeaponData.emptyAmmoClip);
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || currentWeaponData == null) return;

        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
    }

    void OnEnable()
    {
        if (fireInput != null && fireInput.action != null) fireInput.action.Enable();
        if (reloadInput != null && reloadInput.action != null) reloadInput.action.Enable();
        if (aimInput != null && aimInput.action != null) aimInput.action.Enable();
        if (lookInput != null && lookInput.action != null) lookInput.action.Enable();
        if (sprintInput != null && sprintInput.action != null) sprintInput.action.Enable();
        if (numberInputs != null)
        {
            foreach (var input in numberInputs)
            {
                if (input != null && input.action != null) input.action.Enable();
            }
        }
    }
    
    void OnDisable()
    {
        if (fireInput != null && fireInput.action != null) fireInput.action.Disable();
        if (reloadInput != null && reloadInput.action != null) reloadInput.action.Disable();
        if (lookInput != null && lookInput.action != null) lookInput.action.Disable(); 
        if (sprintInput != null && sprintInput.action != null) sprintInput.action.Disable();
        if (numberInputs != null)
        {
            foreach (var input in numberInputs)
            {
                if (input != null && input.action != null) input.action.Disable();
            }
        }
    }
}