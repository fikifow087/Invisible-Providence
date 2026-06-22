using UnityEngine;
using System.Collections;

public class EVT_S3_PizzaToOven : MonoBehaviour
{
    public AudioClip OvenSound;
    public AudioClip OvenTing;

    public AudioSource audioSource;

    private FIKIFOW_LootBox scriptLootBox;
    public EVT_S2_kulkas CONNECT_EVT_kulkas;

    public bool PizzaSudahHangat = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scriptLootBox = GetComponent<FIKIFOW_LootBox>();

        // Matikan di awal jika pizza belum diambil
        if (scriptLootBox != null && !CONNECT_EVT_kulkas.sudahAmbilPizza)
        {
            scriptLootBox.enabled = false;
        }
    }

    IEnumerator PindahPizzaKeOven()
    {
        if (CONNECT_EVT_kulkas.sudahAmbilPizza)
        {
            audioSource.PlayOneShot(OvenSound, 1.0f);
            yield return new WaitForSeconds(3f);
            audioSource.PlayOneShot(OvenTing, 1.0f);
            scriptLootBox.enabled = true; // Aktifkan LootBox setelah pizza dipindahkan ke oven
            PizzaSudahHangat = true; // Tandai bahwa pizza sudah hangat
            // Tambahkan logika untuk memindahkan pizza ke oven di sini
            yield return null;
        }
        else
        {
            Debug.Log("Belum dapat pizza nya di dapur");
            yield break; // Keluar dari coroutine jika pizza belum diambil
        }   

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
