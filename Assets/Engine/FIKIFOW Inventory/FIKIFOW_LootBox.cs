using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(FIKIFOW_Interactable))] // Otomatis menambahkan script interaksi generik jika belum ada
public class FIKIFOW_LootBox : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Masukkan PREFAB item yang akan didapat dari kotak/kulkas ini")]
    public FIKIFOW_ImportantItem prefabItemLoot;
    public bool sudahDiLoot = false;

    [Header("Feedback Settings")]
    [Tooltip("Teks interaksi yang muncul setelah box diambil isinya (Misal: 'Kosong')")]
    public string teksSetelahKosong = "Kosong";
    [Tooltip("Centang jika ingin objek ini tidak bisa disorot/di-klik sama sekali setelah kosong")]
    public bool matikanInteraksiSetelahLoot = false;

    [Header("Peringatan Tas Penuh")]
    [Tooltip("Teks yang muncul sejenak saat player mencoba ambil tapi tas penuh")]
    public string teksTasPenuh = "Inventory Penuh!";
    [Tooltip("Berapa lama teks 'Inventory Penuh!' muncul sebelum kembali normal?")]
    public float durasiTeksPenuh = 2f;

    [Header("Visual & Audio Events")]
    [Tooltip("Centang ini jika ingin event (seperti animasi brankas buka) jalan LEBIH DULU, baru itemnya menyusul ditaruh ke tangan player.")]
    public bool panggilEventSebelumAmbil = false;

    [Tooltip("Event tambahan saat box berhasil dibuka (Misal: Putar animasi buka pintu kulkas, putar suara engsel peti)")]
    public UnityEvent OnLootSuccess;

    [Tooltip("Event tambahan jika GAGAL ambil karena tas penuh (Misal: Putar suara error 'BZZT')")]
    public UnityEvent OnInventoryFull;

    private FIKIFOW_Interactable interactable;
    private string teksBiasaAsli; // Menyimpan teks default dari inspector (Misal: "Buka Kulkas")
    private Coroutine peringatanCoroutine;

    void Start()
    {
        interactable = GetComponent<FIKIFOW_Interactable>();
        
        // Simpan teks asli saat game dimulai agar bisa dikembalikan setelah peringatan penuh selesai
        if (interactable != null)
        {
            teksBiasaAsli = interactable.promptBiasa;
        }
    }

    // Fungsi utama yang dihubungkan ke UnityEvent milik FIKIFOW_Interactable
    public void AmbilLoot()
    {
        if (sudahDiLoot || prefabItemLoot == null) return;

        if (FIKIFOW_InventoryManager.Instance != null)
        {
            // --- Trigger Event Lebih Dulu (Jika dicentang) ---
            if (panggilEventSebelumAmbil)
            {
                OnLootSuccess?.Invoke();
            }

            // 1. Cetak objek item baru dari master Prefab
            FIKIFOW_ImportantItem itemBaru = Instantiate(prefabItemLoot);
            itemBaru.gameObject.name = prefabItemLoot.gameObject.name; 

            // 2. Kirim ke Inventory Manager untuk di-Add dan Force Equip
            bool suksesAmbil = FIKIFOW_InventoryManager.Instance.AddAndForceEquip(itemBaru);

            if (suksesAmbil)
            {
                sudahDiLoot = true;

                // 3. Update status Interactable agar pemain tahu kotak sudah kosong
                if (interactable != null)
                {
                    // Pastikan Coroutine peringatan dihentikan jika sebelumnya sedang berjalan
                    if (peringatanCoroutine != null) StopCoroutine(peringatanCoroutine);
                    
                    interactable.promptBiasa = teksSetelahKosong;
                    if (matikanInteraksiSetelahLoot)
                    {
                        interactable.enabled = false; 
                    }
                }

                // 4. Jalankan event JIKA TIDAK dicentang panggil di awal
                if (!panggilEventSebelumAmbil)
                {
                    OnLootSuccess?.Invoke();
                }
            }
            else
            {
                // Jika tas penuh, hancurkan item clone-nya
                Destroy(itemBaru.gameObject);
                
                // --- FITUR BARU: MUNCULKAN INDIKATOR PENUH ---
                OnInventoryFull?.Invoke();
                
                if (interactable != null)
                {
                    // Reset coroutine jika player nge-spam klik berkali-kali
                    if (peringatanCoroutine != null) StopCoroutine(peringatanCoroutine);
                    peringatanCoroutine = StartCoroutine(TampilkanPeringatanPenuh());
                }
            }
        }
    }

    // Coroutine untuk memunculkan teks "Inventory Penuh!" lalu mengembalikannya seperti semula
    private IEnumerator TampilkanPeringatanPenuh()
    {
        interactable.promptBiasa = teksTasPenuh;
        
        yield return new WaitForSeconds(durasiTeksPenuh);
        
        // Kembalikan ke teks semula HANYA jika status belum di-loot
        if (!sudahDiLoot)
        {
            interactable.promptBiasa = teksBiasaAsli;
        }
    }
}