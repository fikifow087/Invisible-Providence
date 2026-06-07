using UnityEngine;

public enum FIKIFOW_FireMode { SAFE, SEMI, BURST, FULL }
public enum FIKIFOW_StretchMode 
{ 
    PergelanganTangan_Hand, 
    Siku_LowerArm,         
    PangkalLengan_UpperArm 
} 
public enum FIKIFOW_SoundPlayMode { Berbaris, Randomize }
public enum FIKIFOW_ArmSide { Right, Left }
public enum FIKIFOW_BoneType { Hand, LowerArm, UpperArm }

// --- STRUKTUR DATA JARI ---
[System.Serializable]
public class FIKIFOW_FingerRotation
{
    public Vector3 proximal; // Ruas pangkal (bawah)
    public Vector3 intermediate; // Ruas tengah
    public Vector3 distal; // Ruas pucuk (ujung)
}

[System.Serializable]
public class FIKIFOW_HandGripPose
{
    public FIKIFOW_FingerRotation thumb;  // Jempol
    public FIKIFOW_FingerRotation index;  // Telunjuk
    public FIKIFOW_FingerRotation middle; // Tengah
    public FIKIFOW_FingerRotation ring;   // Manis
    public FIKIFOW_FingerRotation little; // Kelingking
}

[System.Serializable]
public class FIKIFOW_BoneConstraint
{
    [Header("BONE SELECTION")]
    [Tooltip("Pilih tangan mana: Kanan atau Kiri?")]
    public FIKIFOW_ArmSide armSide = FIKIFOW_ArmSide.Right;
    
    [Tooltip("Pilih tulang mana yang di-constraint?")]
    public FIKIFOW_BoneType boneType = FIKIFOW_BoneType.LowerArm;

    [Header("AXIS LOCK")]
    [Tooltip("Kunci rotasi di sumbu X (pitch/naik-turun)?")]
    public bool lockX = false;
    [Tooltip("Kunci rotasi di sumbu Y (yaw/kiri-kanan)?")]
    public bool lockY = false;
    [Tooltip("Kunci rotasi di sumbu Z (roll/miring)?")]
    public bool lockZ = false;

    [Header("ROTATION RANGE LIMITS")]
    [Tooltip("Batasan minimum rotasi X")]
    [Range(-180f, 180f)]
    public float minRotationX = -90f;
    [Tooltip("Batasan maksimum rotasi X")]
    [Range(-180f, 180f)]
    public float maxRotationX = 90f;

    [Tooltip("Batasan minimum rotasi Y")]
    [Range(-180f, 180f)]
    public float minRotationY = -180f;
    [Tooltip("Batasan maksimum rotasi Y")]
    [Range(-180f, 180f)]
    public float maxRotationY = 180f;

    [Tooltip("Batasan minimum rotasi Z")]
    [Range(-180f, 180f)]
    public float minRotationZ = -90f;
    [Tooltip("Batasan maksimum rotasi Z")]
    [Range(-180f, 180f)]
    public float maxRotationZ = 90f;
}

[System.Serializable]
public class FIKIFOW_LowerArmConstraint
{
    [Header("AXIS LOCK")]
    [Tooltip("Kunci rotasi di sumbu X (pitch/naik-turun)?")]
    public bool lockX = false;
    [Tooltip("Kunci rotasi di sumbu Y (yaw/kiri-kanan)?")]
    public bool lockY = false;
    [Tooltip("Kunci rotasi di sumbu Z (roll/miring)?")]
    public bool lockZ = false;

    [Header("ROTATION RANGE LIMITS")]
    [Tooltip("Batasan minimum rotasi X")]
    [Range(-180f, 180f)]
    public float minRotationX = -90f;
    [Tooltip("Batasan maksimum rotasi X")]
    [Range(-180f, 180f)]
    public float maxRotationX = 90f;

    [Tooltip("Batasan minimum rotasi Y")]
    [Range(-180f, 180f)]
    public float minRotationY = -180f;
    [Tooltip("Batasan maksimum rotasi Y")]
    [Range(-180f, 180f)]
    public float maxRotationY = 180f;

    [Tooltip("Batasan minimum rotasi Z")]
    [Range(-180f, 180f)]
    public float minRotationZ = -90f;
    [Tooltip("Batasan maksimum rotasi Z")]
    [Range(-180f, 180f)]
    public float maxRotationZ = 90f;
}

[System.Serializable]
public class FIKIFOW_ArmRotation
{
    [Tooltip("Rotasi tambahan sumbu X, Y, Z untuk Lengan Atas (Upper Arm / Bahu)")]
    public Vector3 upperArm;
    [Tooltip("Rotasi tambahan sumbu X, Y, Z untuk Lengan Bawah (Lower Arm / Siku)")]
    public Vector3 lowerArm;
}

[CreateAssetMenu(fileName = "New FIKIFOW Weapon", menuName = "FIKIFOW FPS/Weapon Data")]
public class FIKIFOW_WeaponDataSO : ScriptableObject
{
    [Header("General Info")]
    public string weaponName = "New Weapon";
    public GameObject weaponPrefab;

    [Header("Current Fire Mode For This Weapon")]
    public FIKIFOW_FireMode fireMode = FIKIFOW_FireMode.SEMI;

    [Header("Firing Specifications")]
    [Tooltip("Damage yang diterima musuh per peluru")]
    public float damage = 10f;
    public int ammoCapacity = 30;

    [Header("SEMI Settings")]
    [Tooltip("Fire rate untuk mode SEMI, dalam detik per peluru")]
    public float semiFireRate = 1f;

    [Header("BURST Settings")]
    [Tooltip("Jumlah peluru per burst")]
    public int burstBulletCount = 3;
    [Tooltip("Delay antar peluru di mode BURST")]
    public float burstFireRate = 1f;

    [Header("FULL Settings")]
    [Tooltip("Fire rate untuk mode FULL, dalam detik per peluru")]
    public float fullFireRate = 0.1f;

    [Header("Reload Settings")]
    public float reloadTime = 2.5f;

    [Header("Weapon Transform (Relative to Camera)")]
    public Vector3 spawnPositionOffset;
    public Vector3 spawnRotationOffset;

    [Header("IK & FX Targets")]
    public string rightHandGripName = "IK_RightHand";
    public string leftHandGripName = "IK_LeftHand";
    public string muzzleFlashName = "MuzzleFlash_FX"; 

    [Header("IK Stretch Settings")]
    public bool allowRightHandStretch = false;
    public FIKIFOW_StretchMode rightStretchMode = FIKIFOW_StretchMode.PangkalLengan_UpperArm;

    public bool allowLeftHandStretch = true;
    public FIKIFOW_StretchMode leftStretchMode = FIKIFOW_StretchMode.PangkalLengan_UpperArm;

    [Header("Weapon Sounds")]
    public AudioClip[] shootClips;
    public FIKIFOW_SoundPlayMode shootClipRandomizeMode = FIKIFOW_SoundPlayMode.Randomize;
    public AudioClip reloadClip;
    public AudioClip reloadCompleteClip;
    public AudioClip emptyAmmoClip;

    [Header("CUSTOM FINGER POSES (ADDITIVE ROTATION)")]
    [Tooltip("Centang untuk mengaktifkan kustomisasi jari secara manual")]
    public bool useCustomFingerPose = false;
    
    [Tooltip("Rotasi tambahan untuk Jari Tangan Kanan")]
    public FIKIFOW_HandGripPose rightHandFingers;
    
    [Tooltip("Rotasi tambahan untuk Jari Tangan Kiri")]
    public FIKIFOW_HandGripPose leftHandFingers;

    [Header("CUSTOM ARM POSES (ADDITIVE ROTATION)")]
    [Tooltip("Centang untuk mengaktifkan kustomisasi rotasi lengan secara manual")]
    public bool useCustomArmPose = false;

    [Tooltip("Rotasi tambahan untuk Lengan Kanan (Upper & Lower Arm)")]
    public FIKIFOW_ArmRotation rightArmRotation;

    [Tooltip("Rotasi tambahan untuk Lengan Kiri (Upper & Lower Arm)")]
    public FIKIFOW_ArmRotation leftArmRotation;

    [Header("BONE CONSTRAINTS (HIP-FIRE)")]
    [Tooltip("Daftar constraint untuk tulang tangan (Hand, Lower Arm, Upper Arm) - bisa Right atau Left")]
    public FIKIFOW_BoneConstraint[] boneConstraints = new FIKIFOW_BoneConstraint[0];

// --- BARU: REFERENSI KE MOTION DATA SO ---
    [Header("PROCEDURAL MOTION (SWAY, BOB, RECOIL)")]
    [Tooltip("Masukkan SO Motion Data di sini. Biarkan kosong jika tidak ingin ada pergerakan prosedural.")]
    public FIKIFOW_WeaponMotionDataSO motionData;
    
    [Header("AIMING DATA")]
    [Tooltip("Masukkan SO Aim Data di sini. Biarkan kosong jika senjata tidak bisa membidik.")]
    public FIKIFOW_AimDataSO aimData;

    // --- BARU: TAMBAHAN SPRINT DATA ---
    [Header("SPRINTING DATA")]
    [Tooltip("Masukkan SO Sprint Data di sini. Biarkan kosong jika senjata tidak memiliki animasi lari prosedural.")]
    public FIKIFOW_SprintDataSO sprintData;
}