using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro

public class FIKIFOW_ObjectiveManager : MonoBehaviour
{
    public static FIKIFOW_ObjectiveManager Instance;

    [SerializeField] private TextMeshProUGUI objectiveText; 

    // Variabel string untuk menyimpan huruf dan spasi
    private string misiSekarang = "---";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Jalankan di awal game agar teks pertama langsung muncul
        UpdateTeksUI();
    }

    // Fungsi untuk mengubah isi variabel string dari script lain (misal dari Trigger)
    public void GantiMisi(string misiBaru)
    {
        misiSekarang = misiBaru; // Menyimpan teks baru + spasi ke variabel
        UpdateTeksUI();          // Perbarui tampilan TMPro
    }

    // Fungsi internal untuk memperbarui visual TMPro
    private void UpdateTeksUI()
    {
        if (objectiveText != null)
        {
            // Menggunakan String Interpolation ($) untuk mengambil isi dari variabel misiSekarang
            // \n berfungsi untuk membuat baris baru (Enter)
            objectiveText.text = $"Objective List :\n- {misiSekarang}";
        }
    }
}