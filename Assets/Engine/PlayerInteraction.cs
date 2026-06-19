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

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Tembakkan raycast sesuai layer mask jika diatur
        bool hasHit = interactableLayer == 0 
            ? Physics.Raycast(ray, out hit, interactionDistance) 
            : Physics.Raycast(ray, out hit, interactionDistance, interactableLayer);

        if (hasHit)
        {
            // =====================================================================
            // A. UTAMAKAN SISTEM BARU: Cek komponen universal FIKIFOW_Interactable
            // =====================================================================
            FIKIFOW_Interactable interactable = hit.collider.GetComponent<FIKIFOW_Interactable>();

            if (interactable != null)
            {
                if (interactionText != null) interactionText.gameObject.SetActive(true);
                bool bisaEksekusi = true;

                // Jika objek butuh item spesifik dari slot tangan player
                if (interactable.butuhItemSpesifik)
                {
                    if (FIKIFOW_InventoryManager.Instance != null && 
                        FIKIFOW_InventoryManager.Instance.ApakahItemSedangDipegang(interactable.namaItemDibutuhkan))
                    {
                        if (interactionText != null) interactionText.text = interactable.promptJikaBawaItem;
                    }
                    else
                    {
                        if (interactionText != null) interactionText.text = interactable.promptJikaTidakBawaItem;
                        bisaEksekusi = false; // Kunci interaksi karena syarat item belum terpenuhi
                    }
                }
                else
                {
                    // Objek interaktif biasa (tanpa syarat item, misal: saklar tombol atau pintu biasa)
                    if (interactionText != null) interactionText.text = interactable.promptBiasa;
                }

                // Eksekusi ketika tombol interaksi dipicu oleh pemain
                if (bisaEksekusi && interactInput != null && interactInput.action != null && interactInput.action.triggered)
                {
                    interactable.Interact();
                }
                return; // Keluar dari fungsi agar teks tidak disembunyikan di bawah
            }

            // =====================================================================
            // B. BACKUP CADANGAN: Tetap dukung sistem tag "Door" lama milikmu
            // =====================================================================
            if (hit.collider.CompareTag("Door"))
            {
                Door doorScript = hit.collider.GetComponent<Door>();

                if (doorScript != null)
                {
                    if (interactionText != null)
                    {
                        interactionText.gameObject.SetActive(true);
                        interactionText.text = doorScript.isOpen ? "Tekan E untuk Menutup" : "Tekan E untuk Membuka";
                    }

                    if (interactInput != null && interactInput.action != null && interactInput.action.triggered)
                    {
                        doorScript.ToggleDoor();
                    }
                    return; 
                }
            }
        }

        // Jika raycast tidak mengenai apapun atau bukan objek interaktif, sembunyikan UI teks
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}