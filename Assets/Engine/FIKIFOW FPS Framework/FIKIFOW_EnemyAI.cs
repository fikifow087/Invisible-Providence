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

    [Header("Attack Settings")]
    [Tooltip("Jarak minimum musuh untuk bisa menangkap Player")]
    public float catchDistance = 1.5f;
    private bool isPlayerDead = false;

    [Header("Death Jumpscare Sequence")]
    [Tooltip("Kecepatan kamera Player dipaksa berputar melihat ke arah wajah musuh")]
    public float cameraLookSpeed = 15f;
    [Tooltip("Berapa detik kamera mengunci musuh sebelum game di-restart")]
    public float timeBeforeRestart = 2.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        FindPlayerTarget();
    }

    void FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("FIKIFOW AI: Objek dengan Tag 'Player' tidak ditemukan!");
        }
    }

    void Update()
    {
        if (isPlayerDead || playerTarget == null) return;

        agent.SetDestination(playerTarget.position);

        // Hitung jarak horizontal murni antara musuh dan player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer <= catchDistance)
        {
            TriggerPlayerDeath();
        }
    }

    void TriggerPlayerDeath()
    {
        isPlayerDead = true;
        agent.isStopped = true; 
        agent.velocity = Vector3.zero;

        Debug.Log("FIKIFOW AI: Player tertangkap! Mematikan seluruh kontrol player...");

        if (playerTarget != null)
        {
            // PERBAIKAN UTAMA: Mencari skrip FPS Engine di seluruh hierarki Player (Root, Child, ataupun Parent)
            FIKIFOWFPS1_FirstPersonEngine fpsEngine = playerTarget.GetComponent<FIKIFOWFPS1_FirstPersonEngine>();
            if (fpsEngine == null) fpsEngine = playerTarget.GetComponentInChildren<FIKIFOWFPS1_FirstPersonEngine>();
            if (fpsEngine == null) fpsEngine = playerTarget.GetComponentInParent<FIKIFOWFPS1_FirstPersonEngine>();

            if (fpsEngine != null)
            {
                fpsEngine.enabled = false; // Matikan skrip utama (Mouse Look & Pergerakan lumpuh total)
                Debug.Log("FIKIFOW AI: FIKIFOWFPS1_FirstPersonEngine berhasil dimatikan.");
            }
            else
            {
                Debug.LogError("FIKIFOW AI: Gagal menemukan skrip FIKIFOWFPS1_FirstPersonEngine pada hirarki Player!");
            }

            // BONUS PROTEKSI EKSTRA: Matikan skrip pembantu lainnya (Bobbing, Footsteps, Sway) agar tidak merusak rotasi kamera
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

        // Mulai sekuens manipulasi kamera jumpscare
        StartCoroutine(CameraLookAtEnemySequence());
    }

    IEnumerator CameraLookAtEnemySequence()
    {
        Transform mainCamera = Camera.main.transform;
        float timer = 0f;

        // Pastikan kursor tetap terkunci saat mati
        Cursor.lockState = CursorLockMode.Locked;

        while (timer < timeBeforeRestart)
        {
            timer += Time.deltaTime;

            if (mainCamera != null)
            {
                // Target pandangan diarahkan sedikit ke atas (area wajah/mata model musuh)
                Vector3 targetLookPosition = new Vector3(transform.position.x, transform.position.y + 1.3f, transform.position.z);
                Vector3 targetDirection = targetLookPosition - mainCamera.position;
                
                if (targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    
                    // Slerp rotasi kamera secara agresif menuju musuh
                    mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, targetRotation, Time.deltaTime * cameraLookSpeed);
                }
            }

            yield return null;
        }

        Debug.Log("FIKIFOW AI: Sekuens selesai. Merestart level...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}