using UnityEngine;
using UnityEngine.AI;

[AddComponentMenu("FIKIFOW FPS - Enemy Footsteps")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class FIKIFOW_EnemyFootsteps : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Otomatis mengambil NavMeshAgent di objek ini")]
    private NavMeshAgent agent;
    private AudioSource audioSource;

    [Header("Footstep Intervals")]
    [Tooltip("Jeda suara saat musuh patroli (jalan santai)")]
    [SerializeField] private float walkStepInterval = 0.6f;
    [Tooltip("Jeda suara saat musuh mengejar (lari)")]
    [SerializeField] private float chaseStepInterval = 0.35f;

    [Header("Threshold Settings")]
    [Tooltip("Batas kecepatan untuk menentukan apakah musuh sedang jalan atau lari mengejar. Sesuaikan dengan walkSpeed di FIKIFOW_EnemyAI.")]
    [SerializeField] private float runSpeedThreshold = 3.0f;

    [Header("Surface Audio Settings")]
    [SerializeField] private SurfaceFootstep[] surfaceFootsteps;
    
    [Tooltip("Audio cadangan jika mendeteksi lantai yang tidak ada Tag-nya")]
    [SerializeField] private AudioClip[] defaultWalkSounds;
    [SerializeField] private AudioClip[] defaultSprintSounds;

    [System.Serializable]
    public struct SurfaceFootstep
    {
        public string surfaceTag;
        public AudioClip[] walkSounds;
        public AudioClip[] sprintSounds;
    }

    private float footstepTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        // Setup otomatis untuk Audio 3D Horor
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 1.0 = Full 3D Audio (Penting agar pemain tahu arah musuh)
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    void Update()
    {
        if (agent == null) return;

        // Cek kecepatan musuh secara mendatar
        Vector3 horizontalVelocity = new Vector3(agent.velocity.x, 0f, agent.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        // Jika musuh diam, reset timer
        if (currentSpeed < 0.1f)
        {
            footstepTimer = 0f; 
            return;
        }

        // Tentukan apakah musuh sedang mode lari (Chasing) atau jalan (Patrol)
        bool isChasing = currentSpeed > runSpeedThreshold;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            PlayFootstepSound(isChasing);
            footstepTimer = isChasing ? chaseStepInterval : walkStepInterval;
        }
    }

    private void PlayFootstepSound(bool isChasing)
    {
        string currentSurfaceTag = "Default";
        RaycastHit hit;

        // Tembakkan Raycast sedikit dari atas pusat musuh ke bawah untuk mendeteksi lantai
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; 

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2.0f))
        {
            currentSurfaceTag = hit.collider.tag;
        }

        AudioClip[] clipsToPlay = null;

        // Cari suara yang cocok dengan Tag lantai
        foreach (var surface in surfaceFootsteps)
        {
            if (surface.surfaceTag == currentSurfaceTag)
            {
                clipsToPlay = isChasing ? surface.sprintSounds : surface.walkSounds;
                break;
            }
        }

        // Jika tidak ada Tag yang cocok, gunakan default
        if (clipsToPlay == null || clipsToPlay.Length == 0)
        {
            clipsToPlay = isChasing ? defaultSprintSounds : defaultWalkSounds;
        }

        // Mainkan suara secara acak
        if (clipsToPlay != null && clipsToPlay.Length > 0)
        {
            int randomIndex = Random.Range(0, clipsToPlay.Length);
            
            // Variasi pitch agar langkah kaki terdengar natural dan organik
            audioSource.pitch = Random.Range(0.85f, 1.1f);
            
            // Suara lari dibuat sedikit lebih keras daripada jalan
            float dynamicVolume = isChasing ? Random.Range(0.9f, 1.0f) : Random.Range(0.6f, 0.8f);

            audioSource.PlayOneShot(clipsToPlay[randomIndex], dynamicVolume);
        }
    }
}