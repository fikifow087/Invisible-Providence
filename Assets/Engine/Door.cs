using UnityEngine;

// Memastikan objek pintu otomatis memiliki komponen AudioSource di Inspector
[RequireComponent(typeof(AudioSource))]
public class Door : MonoBehaviour
{
    public bool isOpen = false;
    
    [Header("Rotation Settings")]
    [SerializeField] private float openAngle = 90f; 
    [SerializeField] private float smoothSpeed = 2f;

    [Header("Audio SFX Settings")]
    [SerializeField] private AudioClip openSound;  // Slot untuk SFX Pintu Terbuka
    [SerializeField] private AudioClip closeSound; // Slot untuk SFX Pintu Tertutup

    private Quaternion defaultRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;

    void Start()
    {
        // Menyimpan rotasi awal pintu saat game dimulai
        defaultRotation = transform.localRotation;
        // Menghitung rotasi saat pintu terbuka pada sumbu Y
        openRotation = defaultRotation * Quaternion.Euler(0, openAngle, 0);

        // Mengambil komponen AudioSource yang terpasang di objek ini
        audioSource = GetComponent<AudioSource>();
        
        // Setup dasar AudioSource via code agar menjadi suara 3D yang realistis
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 1.0 = Full 3D Sound (suara mengecil/membesar sesuai jarak Player)
    }

    void Update()
    {
        // Menggerakkan pintu secara halus ke posisi target
        if (isOpen)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, openRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, defaultRotation, Time.deltaTime * smoothSpeed);
        }
    }

    // Fungsi utama yang dipanggil oleh script PlayerInteraction
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Mainkan SFX yang sesuai dengan kondisi state pintu terbaru
        if (isOpen)
        {
            PlaySound(openSound);
        }
        else
        {
            PlaySound(closeSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // Hentikan audio yang sedang berjalan (antispam klik cepat dari player)
            audioSource.Stop(); 
            // Mainkan audio satu kali tanpa merusak kalkulasi volume 3D
            audioSource.PlayOneShot(clip);
        }
    }
}