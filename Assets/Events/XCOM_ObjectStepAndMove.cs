using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways] // Memungkinkan script dijalankan sebagian di luar Play Mode[cite: 5]
public class XCOM_ObjectStepAndMove : MonoBehaviour
{
    [Header("Pergerakan")]
    [SerializeField] private Transform targetPoint; // Game object kosong sebagai target[cite: 5]
    [SerializeField] private float moveSpeed = 2f;    // Kecepatan jalan yang bisa diatur[cite: 5]
    [SerializeField] private float reachDistance = 0.1f; // Jarak toleransi dianggap "sampai"[cite: 5]

    [Header("Audio SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips; // Array biar langkah kakinya bervariasi (tidak monoton)[cite: 5]
    
    [Tooltip("Jarak per 1 langkah kaki. Semakin lambat moveSpeed, otomatis jeda suara melambat.")]
    [SerializeField] private float strideLength = 1f; //[cite: 5]

    private float footstepTimer;
    private bool isMoving = false; //[cite: 5]

    // --- VARIABEL RECORD POSISI AWAL ---
    private Vector3 startPosPlayMode; // Menyimpan posisi awal khusus saat Play Mode (Playtest)
    private Vector3 startPosEditor;   // Menyimpan posisi awal khusus saat Edit Mode[cite: 5]

    // --- VARIABEL UNTUK PREVIEW EDITOR (TANPA PLAY MODE) ---
    private bool isTesting = false; //[cite: 5]
    private float lastEditorTime; //[cite: 5]

    // Properti untuk mendeteksi status aktif baik di Play Mode maupun Edit Mode
    public bool IsCurrentlyRunning => Application.isPlaying ? isMoving : isTesting;

    void Start()
    {
        if (Application.isPlaying) //[cite: 5]
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>(); //[cite: 5]
            
            // Catat posisi awal objek tepat saat tombol Play Unity ditekan
            startPosPlayMode = transform.position;
        }
    }

    void Update()
    {
        // Update normal saat Main Game (Play Mode)[cite: 5]
        if (Application.isPlaying && isMoving && targetPoint != null) //[cite: 5]
        {
            ProcessMovementAndSound(Time.deltaTime); //[cite: 5]
        }
    }

    // FUNGSI UTAMA PERGERAKAN (Dipakai di Play Mode maupun Edit Mode)[cite: 5]
    private void ProcessMovementAndSound(float deltaTime)
    {
        // 1. Gerakkan Posisi[cite: 5]
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * deltaTime); //[cite: 5]

        // 2. Kalkulasi Jeda Suara Dinamis Berdasarkan Kecepatan[cite: 5]
        if (moveSpeed > 0f) //[cite: 5]
        {
            float dynamicInterval = strideLength / moveSpeed; //[cite: 5]
            
            footstepTimer -= deltaTime; //[cite: 5]
            if (footstepTimer <= 0f) //[cite: 5]
            {
                PlayFootstepSound(); //[cite: 5]
                footstepTimer = dynamicInterval; //[cite: 5]
            }
        }

        // 3. Cek Jarak Sampai Tujuan[cite: 5]
        if (Vector3.Distance(transform.position, targetPoint.position) <= reachDistance) //[cite: 5]
        {
            if (Application.isPlaying) //[cite: 5]
            {
                isMoving = false; //[cite: 5]
                OnReachedTarget(); //[cite: 5]
            }
            else
            {
                TogglePreview(); //[cite: 5]
                Debug.Log("Preview Editor: Musuh sampai di tujuan!"); //[cite: 5]
            }
        }
    }

    // FUNGSI TOMBOL UTAMA
    public void HandleInspectorButtonClick()
    {
        if (Application.isPlaying)
        {
            if (isMoving)
            {
                StopMoving();
            }
            else
            {
                StartMoving();
            }
        }
        else
        {
            TogglePreview();
        }
    }

    // FUNGSI RESET UNTUK PLAY MODE (Mengembalikan objek ke titik semula dan membuatnya diam)
    public void ResetToInitialPosition()
    {
        if (Application.isPlaying)
        {
            isMoving = false;                   // Stop pergerakan jalan
            transform.position = startPosPlayMode; // Kembalikan ke posisi awal saat Start
            footstepTimer = 0f;                 // Reset interval suara langkah kaki
            this.enabled = true;                // Pastikan skrip siap menerima trigger lagi
            Debug.Log("Play Mode: Posisi musuh berhasil di-reset ke awal!");
        }
    }

    public void StartMoving()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        if (targetPoint != null) //[cite: 5]
        {
            isMoving = true; //[cite: 5]
            this.enabled = true; //[cite: 5]
        }
    }

    public void StopMoving()
    {
        isMoving = false; //[cite: 5]
    }

    void PlayFootstepSound()
    {
        if (audioSource != null && footstepClips != null && footstepClips.Length > 0) //[cite: 5]
        {
            int randomIndex = Random.Range(0, footstepClips.Length); //[cite: 5]
            audioSource.PlayOneShot(footstepClips[randomIndex]); //[cite: 5]
        }
    }

    void OnReachedTarget()
    {
        Debug.Log("Musuh misterius telah sampai di target point!"); //[cite: 5]
        this.enabled = false; //[cite: 5]
    }

#if UNITY_EDITOR
    public void TogglePreview()
    {
        if (!isTesting) //[cite: 5]
        {
            if (targetPoint == null) //[cite: 5]
            {
                Debug.LogWarning("Isi dulu Target Point di Inspector sebelum mulai preview!"); //[cite: 5]
                return; //[cite: 5]
            }
            
            if (audioSource == null) audioSource = GetComponent<AudioSource>(); //[cite: 5]

            startPosEditor = transform.position; //[cite: 5]
            isTesting = true; //[cite: 5]
            lastEditorTime = (float)EditorApplication.timeSinceStartup; //[cite: 5]
            
            EditorApplication.update += EditorUpdate; //[cite: 5]
        }
        else
        {
            isTesting = false; //[cite: 5]
            EditorApplication.update -= EditorUpdate; //[cite: 5]
            transform.position = startPosEditor; //[cite: 5]
            footstepTimer = 0f; //[cite: 5]
        }
    }

    private void EditorUpdate()
    {
        if (isTesting && !Application.isPlaying) //[cite: 5]
        {
            float currentEditorTime = (float)EditorApplication.timeSinceStartup; //[cite: 5]
            float deltaTime = currentEditorTime - lastEditorTime; //[cite: 5]
            lastEditorTime = currentEditorTime; //[cite: 5]

            ProcessMovementAndSound(deltaTime); //[cite: 5]
            SceneView.RepaintAll(); //[cite: 5]
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying && isTesting) //[cite: 5]
        {
            TogglePreview(); //[cite: 5]
        }
    }
#endif
}

// ========================================================
// CUSTOM INSPECTOR BUTTON DENGAN TOMBOL RESET tambahan
// ========================================================
#if UNITY_EDITOR
[CustomEditor(typeof(XCOM_ObjectStepAndMove))]
public class XCOM_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //[cite: 5]

        XCOM_ObjectStepAndMove script = (XCOM_ObjectStepAndMove)target; //[cite: 5]

        GUILayout.Space(15); //[cite: 5]
        
        bool isRunning = script.IsCurrentlyRunning;
        
        string modeLabel = Application.isPlaying ? " (Play Mode)" : " (Tanpa Playtest)";
        string buttonText = isRunning ? $"🛑 Stop Movement{modeLabel}" : $"▶️ Play Movement{modeLabel}";
        
        GUI.color = isRunning ? Color.red : Color.green; //[cite: 5]

        // Tombol Play / Stop utama
        if (GUILayout.Button(buttonText, GUILayout.Height(35))) //[cite: 5]
        {
            script.HandleInspectorButtonClick();
        }
        
        // TOMBOL BARU: Muncul khusus saat Play Mode (Playtest) aktif untuk reset posisi
        if (Application.isPlaying)
        {
            GUILayout.Space(5);
            GUI.color = new Color(1f, 0.6f, 0f); // Warna Oranye/Kuning untuk tombol reset
            
            if (GUILayout.Button("🔄 Reset Position & Stop", GUILayout.Height(30)))
            {
                script.ResetToInitialPosition();
            }
        }
        
        GUI.color = Color.white; //[cite: 5]
    }
}
#endif