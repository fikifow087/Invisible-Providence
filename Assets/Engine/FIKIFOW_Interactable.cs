using UnityEngine;
using UnityEngine.Events;

public class FIKIFOW_Interactable : MonoBehaviour
{
    [Header("UI Prompt Settings")]
    [Tooltip("Teks default jika objek tidak butuh item apa pun (Misal: Tombol Saklar)")]
    public string promptBiasa = "Tekan E untuk Berinteraksi";

    [Header("Requirement Settings")]
    public bool butuhItemSpesifik = false;

    [Tooltip("Tinggal DRAG Prefab atau Game Object Item yang memiliki script FIKIFOW_ImportantItem ke sini!")]
    public FIKIFOW_ImportantItem itemDibutuhkan;

    // Menampilkan nama yang terdeteksi di Inspector secara otomatis (Read-Only)
    [SerializeField] private string namaItemTerdeteksi = "";

    // Properti ini menjaga agar script PlayerInteraction atau Inventory lama kamu tidak error 
    // karena dia tetap mengembalikan string nama item asli dari script ImportantItem
    public string namaItemDibutuhkan 
    {
        get 
        {
            if (itemDibutuhkan != null) return itemDibutuhkan.namaItem;
            return namaItemTerdeteksi;
        }
    }

    [TextArea(1, 2)] public string promptJikaBawaItem = "<color=#2ecc71>Tekan E untuk Gunakan Item</color>";
    [TextArea(1, 2)] public string promptJikaTidakBawaItem = "<color=red>Butuh Item Spesifik</color>";

    [Header("Item Action Settings")]
    [Tooltip("Apakah item di tangan player akan dilepas dan ditaruh secara fisik di dunia game saat sukses?")]
    public bool lepasItemDariTangan = false;
    [Tooltip("Tempat koordinat penempatan objek item (Opsional, jika kosong akan mengambil posisi objek ini)")]
    public Transform titikPenempatanItem;

    [Header("Interaction Event (SANGAT REUSABLE)")]
    [Tooltip("Aksi kustom apa saja yang terjadi ketika sukses? Hubungkan fungsi dari skrip lain lewat Unity Inspector di sini!")]
    public UnityEvent OnInteractSuccess;

    // Fungsi otomatis Unity: Berjalan di dalam Editor saat kamu mengubah nilai atau me-drag objek di Inspector
    private void OnValidate()
    {
        if (itemDibutuhkan != null)
        {
            // Ambil string namaItem langsung dari skrip FIKIFOW_ImportantItem tanpa ketik manual
            namaItemTerdeteksi = itemDibutuhkan.namaItem;
        }
        else
        {
            namaItemTerdeteksi = "Tidak Ada Item (Akan Error Jika Butuh Item)";
        }
    }

    // Fungsi utama yang dieksekusi oleh Raycast Player
    public void Interact()
    {
        if (butuhItemSpesifik)
        {
            if (FIKIFOW_InventoryManager.Instance != null)
            {
                Vector3 targetPos = titikPenempatanItem != null ? titikPenempatanItem.position : transform.position;
                Quaternion targetRot = titikPenempatanItem != null ? titikPenempatanItem.rotation : Quaternion.identity;

                if (lepasItemDariTangan)
                {
                    // Menggunakan namaItemDibutuhkan yang sudah otomatis sinkron
                    bool suksesLepas = FIKIFOW_InventoryManager.Instance.TaruhItemDariTangan(namaItemDibutuhkan, targetPos, targetRot, titikPenempatanItem);
                    if (!suksesLepas) return; // Gagalkan interaksi jika item tidak cocok/tidak dipegang
                }
            }
        }

        // Pemicu utama event yang kamu susun di Unity Inspector
        if (OnInteractSuccess != null)
        {
            OnInteractSuccess.Invoke();
        }
    }
}