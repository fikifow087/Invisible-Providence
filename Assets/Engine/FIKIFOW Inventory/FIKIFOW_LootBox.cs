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
    
    [Tooltip("Beri jeda waktu (detik) setelah event dipanggil agar animasi membuka selesai dulu, baru item masuk ke tangan.")]
    public float jedaSebelumAmbilItem = 1.2f;

    [Tooltip("Event tambahan saat box berhasil dibuka (Misal: Putar animasi buka pintu kulkas, putar suara engsel peti)")]
    public UnityEvent OnLootSuccess;

    [Tooltip("Event tambahan jika GAGAL ambil karena tas penuh (Misal: Putar suara error 'BZZT')")]
    public UnityEvent OnInventoryFull;

    private FIKIFOW_Interactable interactable;
    private string teksBiasaAsli; // Menyimpan teks default dari inspector (Misal: "Buka Kulkas")
    private Coroutine peringatanCoroutine;
    private bool sedangProsesLoot = false; // Mencegah spam klik selama jeda animasi

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
        if (sudahDiLoot || sedangProsesLoot || prefabItemLoot == null) return;

        if (FIKIFOW_InventoryManager.Instance != null)
        {
            if (panggilEventSebelumAmbil)
            {
                // Jika dicentang, jalankan proses loot menggunakan jeda waktu coroutine
                StartCoroutine(ProsesLootDenganJeda());
            }
            else
            {
                // Jika tidak dicentang, langsung ambil instan seperti biasa
                EksekusiAmbilItem();
            }
        }
    }

    // --- COROUTINE BARU: Memberikan waktu agar animasi bermain dulu ---
    private IEnumerator ProsesLootDenganJeda()
    {
        sedangProsesLoot = true;

        // 1. Jalankan event visual/audio dulu (misal: animasi kulkas/peti terbuka)
        OnLootSuccess?.Invoke();

        // 2. Tunggu selama beberapa detik di sini agar animasinya selesai
        yield return new WaitForSeconds(jedaSebelumAmbilItem);

        // 3. Setelah selesai menunggu, baru eksekusi ambil itemnya
        EksekusiAmbilItem();

        sedangProsesLoot = false;
    }

    // Memisahkan logika pengambilan item agar bisa dipanggil instan atau via jeda
    private void EksekusiAmbilItem()
    {
        // Pastikan belum di-loot saat coroutine selesai
        if (sudahDiLoot) return;

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
            
            // MUNCULKAN INDIKATOR PENUH
            OnInventoryFull?.Invoke();
            
            if (interactable != null)
            {
                if (peringatanCoroutine != null) StopCoroutine(peringatanCoroutine);
                peringatanCoroutine = StartCoroutine(TampilkanPeringatanPenuh());
            }
        }
    }

    // Coroutine untuk memunculkan teks "Inventory Penuh!" lalu mengembalikannya seperti semula
    private IEnumerator TampilkanPeringatanPenuh()
    {
        interactable.promptBiasa = teksTasPenuh;
        
        yield return new WaitForSeconds(durasiTeksPenuh);
        
        if (!sudahDiLoot)
        {
            interactable.promptBiasa = teksBiasaAsli;
        }
    }
}