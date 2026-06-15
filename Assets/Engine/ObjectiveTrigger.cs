using UnityEngine;
using UnityEngine.Events; // Wajib ditambah untuk mendeteksi UnityEvent

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Pengaturan Objektif")]
    [SerializeField] private string misiBerikutnya = "Periksa Dapur yang Terkunci";

    [Header("Event Tambahan (Opsional)")]
    // Ini akan memunculkan kotak event interaktif di Inspector Unity
    [SerializeField] private UnityEvent eventTambahanSaatDisentuh;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Jalankan fungsi ganti misi seperti biasa
            if (FIKIFOW_ObjectiveManager.Instance != null)
            {
                FIKIFOW_ObjectiveManager.Instance.GantiMisi(misiBerikutnya);
            }

            // 2. Jalankan event tambahan apa pun yang kamu pasang di Inspector
            if (eventTambahanSaatDisentuh != null)
            {
                eventTambahanSaatDisentuh.Invoke();
            }

            // 3. Hancurkan trigger
            Destroy(gameObject);
        }
    }
}