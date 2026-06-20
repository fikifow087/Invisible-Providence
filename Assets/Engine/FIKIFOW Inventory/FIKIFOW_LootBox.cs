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

    [Header("Visual & Audio Events")]
    [Tooltip("Event tambahan saat box berhasil dibuka (Misal: Putar animasi buka pintu kulkas, putar suara engsel peti)")]
    public UnityEvent OnLootSuccess;

    private FIKIFOW_Interactable interactable;

    void Start()
    {
        interactable = GetComponent<FIKIFOW_Interactable>();
    }

    // Fungsi utama yang dihubungkan ke UnityEvent milik FIKIFOW_Interactable
    public void AmbilLoot()
    {
        if (sudahDiLoot || prefabItemLoot == null) return;

        if (FIKIFOW_InventoryManager.Instance != null)
        {
            // 1. Cetak objek item baru dari master Prefab
            FIKIFOW_ImportantItem itemBaru = Instantiate(prefabItemLoot);
            // Rapikan nama di Hierarchy agar tidak ada tulisan "(Clone)"
            itemBaru.gameObject.name = prefabItemLoot.gameObject.name; 

            // 2. Kirim ke Inventory Manager untuk di-Add dan Force Equip
            bool suksesAmbil = FIKIFOW_InventoryManager.Instance.AddAndForceEquip(itemBaru);

            if (suksesAmbil)
            {
                sudahDiLoot = true;

                // 3. Update status Interactable agar pemain tahu kotak sudah kosong
                if (interactable != null)
                {
                    interactable.promptBiasa = teksSetelahKosong;
                    if (matikanInteraksiSetelahLoot)
                    {
                        interactable.enabled = false; 
                    }
                }

                // 4. Jalankan event visual/audio dari Inspector
                OnLootSuccess?.Invoke();
            }
            else
            {
                // Jika tas ternyata penuh dan batal ambil, hancurkan kembali item yang terlanjur dicetak
                Destroy(itemBaru.gameObject);
            }
        }
    }
}