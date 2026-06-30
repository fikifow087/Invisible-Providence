using UnityEngine;

public class XCOM_ObjectStepAndMove : MonoBehaviour
{
    [Header("Pergerakan")]
    [SerializeField] private Transform targetPoint; 
    [SerializeField] private float moveSpeed = 2f;    
    [SerializeField] private float reachDistance = 0.1f; 

    [Header("Audio SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips; 
    [SerializeField] private float footstepInterval = 0.5f; 

    private float footstepTimer;
    private bool isMoving = false;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // HAPUS otomatis isMoving = true di sini agar dikontrol oleh skrip CUT_Suara
    }

    void Update()
    {
        if (isMoving && targetPoint != null)
        {
            MoveTowardsTarget();
            HandleFootsteps();
        }
    }

    // FUNGSI BARU: Untuk dipanggil oleh skrip CUT_Suara saat event dimulai
    public void StartMoving()
    {
        if (targetPoint != null)
        {
            isMoving = true;
            this.enabled = true; // Memastikan komponen dalam keadaan aktif
        }
        else
        {
            Debug.LogWarning("Gagal jalan: Target Point belum di-assign di " + gameObject.name);
        }
    }

    // FUNGSI BARU: Untuk menghentikan paksa jika diperlukan di tengah jalan
    public void StopMoving()
    {
        isMoving = false;
    }

    void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) <= reachDistance)
        {
            isMoving = false;
            OnReachedTarget();
        }
    }

    void HandleFootsteps()
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            PlayFootstepSound();
            footstepTimer = footstepInterval; 
        }
    }

    void PlayFootstepSound()
    {
        if (audioSource != null && footstepClips.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepClips.Length);
            audioSource.PlayOneShot(footstepClips[randomIndex]);
        }
    }

    void OnReachedTarget()
    {
        Debug.Log("Musuh misterius telah sampai di target point!");
        // Kamu juga bisa menonaktifkan komponen ini kembali setelah sampai tujuan
        this.enabled = false; 
    }
}