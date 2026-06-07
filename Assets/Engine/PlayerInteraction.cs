using UnityEngine;
using UnityEngine.UI; 
using TMPro;        
using UnityEngine.InputSystem; // WAJIB: Untuk menggunakan New Input System

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionDistance = 3f; // Jarak maksimal bisa interaksi
    [SerializeField] private LayerMask interactableLayer;    // Untuk menyaring objek interaktif saja (opsional)

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI interactionText; // Drag text UI "Tekan E" ke sini

    [Header("Input Settings")]
    [Tooltip("Masukkan InputActionReference untuk tombol interaksi (contoh: tombol E atau Klik Kiri)")]
    [SerializeField] private InputActionReference interactInput; // Slot custom input di Inspector

    private Camera cam;

    void Start()
    {
        cam = Camera.main; 
        if (interactionText != null) interactionText.gameObject.SetActive(false);
    }

    // WAJIB: Daftarkan input action saat objek aktif agar bisa membaca pencetan tombol
    void OnEnable()
    {
        if (interactInput != null && interactInput.action != null)
        {
            interactInput.action.Enable();
        }
    }

    // WAJIB: Matikan input saat objek tidak aktif untuk mencegah memory leak / eror
    void OnDisable()
    {
        if (interactInput != null && interactInput.action != null)
        {
            interactInput.action.Disable();
        }
    }

    void Update()
    {
        if (cam == null) return;

        // Membuat Ray tepat dari titik tengah layar (Crosshair)
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Cek apakah kita menggunakan layer mask tertentu atau menembak semua objek
        bool hasHit = interactableLayer == 0 
            ? Physics.Raycast(ray, out hit, interactionDistance) 
            : Physics.Raycast(ray, out hit, interactionDistance, interactableLayer);

        // Tembakkan raycast
        if (hasHit)
        {
            // Cek apakah objek yang ditabrak memiliki Tag "Door"
            if (hit.collider.CompareTag("Door"))
            {
                Door doorScript = hit.collider.GetComponent<Door>();

                if (doorScript != null)
                {
                    // Tampilkan UI teks petunjuk
                    if (interactionText != null)
                    {
                        interactionText.gameObject.SetActive(true);
                        interactionText.text = doorScript.isOpen ? "Tekan E untuk Menutup" : "Tekan E untuk Membuka";
                    }

                    // PENGGANTI INPUT LAMA: .triggered mendeteksi ketika tombol baru saja ditekan di frame ini
                    if (interactInput != null && interactInput.action != null && interactInput.action.triggered)
                    {
                        doorScript.ToggleDoor();
                    }
                    
                    return; // Keluar dari fungsi agar teks tidak langsung disembunyikan
                }
            }
        }

        // Jika raycast tidak menabrak apa-apa atau bukan pintu, sembunyikan UI teks
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}