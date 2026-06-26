using UnityEngine;
using UnityEngine.Events;

public class FIKIFOW_TriggerZone : MonoBehaviour
{
    [Header("Pengaturan Filter")]
    [Tooltip("Tag objek yang bisa memicu area ini")]
    [SerializeField] private string targetTag = "Player";

    [Tooltip("Jika dicentang, trigger ini hanya akan aktif SATU KALI saja (cocok untuk Jumpscare/Event cerita)")]
    [SerializeField] private bool sekaliPakai = false;
    private bool sudahAktif = false;

    [Header("Event List (Jangka Panjang)")]
    // Event yang dipanggil saat player MASUK area
    public UnityEvent OnPlayerEnter;
    
    // Event yang dipanggil saat player KELUAR area (Opsional, kosongi jika tidak butuh)
    public UnityEvent OnPlayerExit;

    private void OnTriggerEnter(Collider other)
    {
        // Jika disetting sekali pakai dan sudah pernah aktif, batalkan.
        if (sekaliPakai && sudahAktif) return;

        if (other.CompareTag(targetTag))
        {
            sudahAktif = true;
            
            // Jalankan semua fungsi yang kamu daftarkan di Inspector
            if (OnPlayerEnter != null)
            {
                OnPlayerEnter.Invoke();
            }
            
            Debug.Log($"[TriggerZone] {gameObject.name} BERHASIL dipicu oleh Player.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (OnPlayerExit != null)
            {
                OnPlayerExit.Invoke();
            }
        }
    }

    // Fungsi tambahan untuk mereset trigger jika sewaktu-waktu dibutuhkan lewat script lain
    public void ResetTrigger()
    {
        sudahAktif = false;
    }
}