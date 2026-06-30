using UnityEngine;

public class XCOM_ObjectStepAndMove : MonoBehaviour
{
    [Header("Pergerakan")]
    [SerializeField] private Transform targetPoint; // Game object kosong sebagai target
    [SerializeField] private float moveSpeed = 2f;    // Kecepatan jalan yang bisa diatur
    [SerializeField] private float reachDistance = 0.1f; // Jarak toleransi dianggap "sampai"

    [Header("Audio SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips; // Array biar langkah kakinya bervariasi (tidak monoton)
    [SerializeField] private float footstepInterval = 0.5f; // Jeda waktu antar langkah (makin kecil makin cepat)

    private float footstepTimer;
    private bool isMoving = false;

    void Start()
    {
        // Auto-ambil AudioSource jika lupa di-assign di Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Mulai pergerakan jika target sudah ditentukan
        if (targetPoint != null)
        {
            isMoving = true;
        }
    }

    void Update()
    {
        if (isMoving && targetPoint != null)
        {
            MoveTowardsTarget();
            HandleFootsteps();
        }
    }

    void MoveTowardsTarget()
    {
        // Menggerakkan object menuju posisi targetPoint secara halus
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        // Cek apakah jarak ke target sudah sangat dekat
        if (Vector3.Distance(transform.position, targetPoint.position) <= reachDistance)
        {
            isMoving = false;
            OnReachedTarget();
        }
    }

    void HandleFootsteps()
    {
        // Hitung mundur timer langkah kaki
        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            PlayFootstepSound();
            footstepTimer = footstepInterval; // Reset timer ke jeda yang ditentukan
        }
    }

    void PlayFootstepSound()
    {
        if (audioSource != null && footstepClips.Length > 0)
        {
            // Ambil clip random dari array supaya suara langkahnya lebih natural
            int randomIndex = Random.Range(0, footstepClips.Length);
            audioSource.PlayOneShot(footstepClips[randomIndex]);
        }
    }

    // Fungsi trigger ketika musuh sampai di titik tujuan
    void OnReachedTarget()
    {
        Debug.Log("Musuh misterius telah sampai di target point!");
        // Kamu bisa memicu event cutscene selanjutnya di sini (misal: dialog baru, ganti scene, dsb.)
    }
}