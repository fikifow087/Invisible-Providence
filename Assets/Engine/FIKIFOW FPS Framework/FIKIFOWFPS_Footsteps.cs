using UnityEngine;

[AddComponentMenu("FIKIFOW FPS/4 - Footsteps Fixed")]
[RequireComponent(typeof(AudioSource))]
public class FIKIFOWFPS_Footsteps : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referensi ke CharacterController milik Player")]
    [SerializeField] private CharacterController controller;
    
    [Tooltip("Referensi ke skrip utama First Person Engine untuk cek status Sprint")]
    [SerializeField] private FIKIFOWFPS1_FirstPersonEngine fpsEngine;

    [Header("Footstep Intervals")]
    [SerializeField] private float walkStepInterval = 0.6f;
    [SerializeField] private float sprintStepInterval = 0.35f;

    [Header("Surface Audio Settings")]
    [SerializeField] private SurfaceFootstep[] surfaceFootsteps;
    
    [Tooltip("Audio cadangan jika mendeteksi lantai yang tidak ada Tag-nya")]
    [SerializeField] private AudioClip[] defaultWalkSounds;
    [SerializeField] private AudioClip[] defaultSprintSounds;

    [System.Serializable]
    public struct SurfaceFootstep
    {
        [Tooltip("Tag Unity yang ada pada objek lantai (Contoh: Wood, Concrete, Dirt)")]
        public string surfaceTag;
        public AudioClip[] walkSounds;
        public AudioClip[] sprintSounds;
    }

    private AudioSource audioSource;
    private float footstepTimer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.0f; // Set ke 2D agar terdengar jelas di telinga player

        if (controller == null) controller = GetComponentInParent<CharacterController>();
        if (fpsEngine == null) fpsEngine = GetComponentInParent<FIKIFOWFPS1_FirstPersonEngine>();
    }

    void Update()
    {
        if (controller == null) return;

        // --- PERBAIKAN BUG: HENTIKAN SUARA JIKA DIALOG MODE 1 AKTIF ---
        if (KIRISA_DialogueSystem.IsPlayerInputBlocked)
        {
            footstepTimer = 0f; // Reset timer
            return; // Berhenti mengeksekusi sisa kode di bawah
        }

        // Hitung hanya kecepatan horizontal (X dan Z), abaikan getaran Y agar stabil
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        bool isMoving = controller.isGrounded && horizontalVelocity.magnitude > 0.1f;

        if (!isMoving)
        {
            footstepTimer = 0f; 
            return;
        }

        bool sprinting = (fpsEngine != null) ? fpsEngine.isSprinting : false;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            PlayFootstepSound(sprinting);
            footstepTimer = sprinting ? sprintStepInterval : walkStepInterval;
        }
    }

    private void PlayFootstepSound(bool sprinting)
    {
        string currentSurfaceTag = "Default";
        RaycastHit hit;

        // Tembakkan Raycast dari posisi bawah pusat CharacterController
        Vector3 rayOrigin = controller.transform.position; 

        if (Physics.Raycast(rayOrigin + Vector3.up * 0.2f, Vector3.down, out hit, 1.5f))
        {
            currentSurfaceTag = hit.collider.tag;
        }

        AudioClip[] clipsToPlay = null;

        foreach (var surface in surfaceFootsteps)
        {
            if (surface.surfaceTag == currentSurfaceTag)
            {
                clipsToPlay = sprinting ? surface.sprintSounds : surface.walkSounds;
                break;
            }
        }

        if (clipsToPlay == null || clipsToPlay.Length == 0)
        {
            clipsToPlay = sprinting ? defaultSprintSounds : defaultWalkSounds;
        }

        if (clipsToPlay != null && clipsToPlay.Length > 0)
        {
            int randomIndex = Random.Range(0, clipsToPlay.Length);
            
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            float dynamicVolume = Random.Range(0.85f, 1.0f);

            audioSource.PlayOneShot(clipsToPlay[randomIndex], dynamicVolume);
        }
    }
}