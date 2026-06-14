using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    // Teks ini bebas diisi huruf dan spasi lewat Unity Inspector
    [SerializeField] private string misiBerikutnya = "Periksa Dapur yang Terkunci";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Kirim string baru ke ObjectiveManager
            FIKIFOW_ObjectiveManager.Instance.GantiMisi(misiBerikutnya);

            Destroy(gameObject);
        }
    }
}