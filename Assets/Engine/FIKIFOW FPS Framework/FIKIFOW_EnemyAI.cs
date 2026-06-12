using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[AddComponentMenu("FIKIFOW FPS/6 - Enemy AI Core Fixed")]
[RequireComponent(typeof(NavMeshAgent))]
public class FIKIFOW_EnemyAI : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Tag yang digunakan oleh objek Player")]
    public string playerTag = "Player";
    private Transform playerTarget;
    private NavMeshAgent agent;

    [Header("Movement Settings")]
    [Tooltip("Kecepatan AI saat patroli mencari pemain")]
    public float walkSpeed = 2f;
    [Tooltip("Kecepatan AI saat melihat dan mengejar pemain")]
    public float chaseSpeed = 4.5f;

    [Header("Vision System")]
    [Tooltip("Seberapa jauh musuh bisa melihat (dalam meter)")]
    public float visionRange = 15f;
    [Tooltip("Sudut pandang mata musuh (180 = pandangan setengah lingkaran)")]
    [Range(0f, 360f)]
    public float visionAngle = 120f;
    [Tooltip("Layer tembok/rintangan yang bisa menghalangi pandangan musuh")]
    public LayerMask obstacleLayer;

    [Header("Free Roam Settings")]
    [Tooltip("Seberapa jauh radius musuh mencari titik acak saat patroli")]
    public float roamRadius = 10f;
    [Tooltip("Berapa lama musuh diam sebelum jalan ke titik patroli baru")]
    public float roamWaitTime = 2f;
    
    private float roamTimer;
    private bool isChasing = false;

    [Header("Attack Settings")]
    public float catchDistance = 1.5f;
    private bool isPlayerDead = false;

    [Header("Death Jumpscare Sequence")]
    public float cameraLookSpeed = 15f;
    public float timeBeforeRestart = 2.5f;

    // --- VARIABEL UNTUK BLOCKING DIALOG ---
    private bool isAIBlocked = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        FindPlayerTarget();
        roamTimer = 0f; // Agar musuh langsung jalan saat game mulai
    }

    // --- DAFTARKAN EVENT LISTENER DI SINI ---
    void OnEnable()
    {
        KIRISA_DialogueSystem.OnBlockPlayerInput += BlockAI;
        KIRISA_DialogueSystem.OnUnblockPlayerInput += UnblockAI;
    }

    void OnDisable()
    {
        KIRISA_DialogueSystem.OnBlockPlayerInput -= BlockAI;
        KIRISA_DialogueSystem.OnUnblockPlayerInput -= UnblockAI;
    }

    private void BlockAI()
    {
        isAIBlocked = true;
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;       // Rem paksa NavMeshAgent
            agent.velocity = Vector3.zero; // Hilangkan momentum
        }
    }

    private void UnblockAI()
    {
        isAIBlocked = false;
        if (agent != null && agent.isActiveAndEnabled && !isPlayerDead)
        {
            agent.isStopped = false; // Lanjut bergerak
        }
    }

    void FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
    }

    void Update()
    {
        // JIKA DIALOG AKTIF ATAU PLAYER MATI, JANGAN LAKUKAN APA-APA
        if (isPlayerDead || playerTarget == null || isAIBlocked) return;

        bool wasChasing = isChasing;
        
        // 1. Cek apakah Player terlihat oleh musuh
        CheckVision();

        // 2. Logic State: Chasing vs Free Roam
        if (isChasing)
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(playerTarget.position);
        }
        else
        {
            if (wasChasing)
            {
                agent.ResetPath();
                roamTimer = roamWaitTime; 
            }
            FreeRoam();
        }

        // 3. Jumpscare Catch Mechanic
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer <= catchDistance)
        {
            TriggerPlayerDeath();
        }
    }

    void CheckVision()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= visionRange)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
            Vector3 playerPos = playerTarget.position + Vector3.up * 1.5f;
            Vector3 directionToPlayer = (playerPos - rayOrigin).normalized;

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer <= visionAngle / 2f)
            {
                if (!Physics.Raycast(rayOrigin, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    isChasing = true;
                    return; 
                }
            }
        }
        
        isChasing = false;
    }

    void FreeRoam()
    {
        agent.speed = walkSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            roamTimer -= Time.deltaTime;

            if (roamTimer <= 0f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
                randomDirection += transform.position;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, 1))
                {
                    agent.SetDestination(hit.position);
                }

                roamTimer = roamWaitTime; 
            }
        }
    }

    void TriggerPlayerDeath()
    {
        isPlayerDead = true;
        agent.isStopped = true; 
        agent.velocity = Vector3.zero;

        if (playerTarget != null)
        {
            FIKIFOWFPS1_FirstPersonEngine fpsEngine = playerTarget.GetComponent<FIKIFOWFPS1_FirstPersonEngine>();
            if (fpsEngine == null) fpsEngine = playerTarget.GetComponentInChildren<FIKIFOWFPS1_FirstPersonEngine>();
            if (fpsEngine == null) fpsEngine = playerTarget.GetComponentInParent<FIKIFOWFPS1_FirstPersonEngine>();

            if (fpsEngine != null) fpsEngine.enabled = false;

            MonoBehaviour[] allScripts = playerTarget.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in allScripts)
            {
                string scriptName = script.GetType().Name;
                if (scriptName.Contains("HeadBobbing") || scriptName.Contains("Footsteps") || scriptName.Contains("FlashlightSway"))
                {
                    script.enabled = false;
                }
            }
        }

        StartCoroutine(CameraLookAtEnemySequence());
    }

    IEnumerator CameraLookAtEnemySequence()
    {
        Transform mainCamera = Camera.main.transform;
        float timer = 0f;
        Cursor.lockState = CursorLockMode.Locked;

        while (timer < timeBeforeRestart)
        {
            timer += Time.deltaTime;

            if (mainCamera != null)
            {
                Vector3 targetLookPosition = new Vector3(transform.position.x, transform.position.y + 1.3f, transform.position.z);
                Vector3 targetDirection = targetLookPosition - mainCamera.position;
                
                if (targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, targetRotation, Time.deltaTime * cameraLookSpeed);
                }
            }
            yield return null;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * forward;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(eyePos, eyePos + leftBoundary * visionRange);
        Gizmos.DrawLine(eyePos, eyePos + rightBoundary * visionRange);
    }
}