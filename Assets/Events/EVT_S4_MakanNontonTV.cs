using UnityEngine;

public class EVT_S4_MakanNontonTV : MonoBehaviour
{
    private FIKIFOW_Interactable scriptInteraksi;
    public EVT_S2_kulkas CONNECT_EVT_kulkas;
    public EVT_S3_PizzaToOven CONNECT_EVT_PizzaToOven;

    void Start()
    {
        scriptInteraksi = GetComponent<FIKIFOW_Interactable>();

        // Matikan di awal jika pizza belum diambil
        if (scriptInteraksi != null && !CONNECT_EVT_kulkas.sudahAmbilPizza)
        {
            scriptInteraksi.enabled = false;
        }
    }

    // Ubah menjadi PUBLIC agar bisa dipanggil dari script Kulkas
    public void CekKondisiPizza()
    {
        if (CONNECT_EVT_kulkas != null && CONNECT_EVT_kulkas.sudahAmbilPizza && CONNECT_EVT_PizzaToOven.PizzaSudahHangat)
        {
            if (scriptInteraksi != null)
            {
                scriptInteraksi.enabled = true;
                Debug.Log("FIKIFOW_Interactable Berhasil Diaktifkan Permanen!");
            }
        }
        else
        {
            Debug.Log("Belum dapat pizza nya di dapur");
        }
    }
    
    // FUNGSI UPDATE DIHAPUS agar tidak menimpa state komponen setiap frame
}