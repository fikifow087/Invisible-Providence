using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class EVT_NontonTV : MonoBehaviour
{
    [Header("Pengaturan Input")]
    // Variabel ini akan muncul di Inspector untuk kamu pilih Action-nya
    public InputActionReference tombolAksi;

    [Header("Koneksi")]
    public EVT_S2_kulkas CONNECT_EVT_kulkas;
    public EVT_S3_PizzaToOven CONNECT_EVT_PizzaToOven;
    public TVController CONNECT_TVController;
    public GameObject UI_GantiChannelTV;
    [SerializeField] private TextMeshProUGUI HarusNontonSeginiDuluBaruLanjut;
    
    [Header("Variabel Dan Event")]
    public bool sedangNontonTV = false;
    [SerializeField] private int current_MisiNontonTV = 0;
    [SerializeField] private int max_MisiNontonTV = 3;
    public UnityEvent Event_List_Setelah_NontonTV;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UI_GantiChannelTV.SetActive(false);
        sedangNontonTV = false;
    }

    public void OnClick_START_NontonTV()
    {
        //if (CONNECT_EVT_kulkas != null && CONNECT_EVT_kulkas.sudahAmbilPizza && CONNECT_EVT_PizzaToOven.PizzaSudahHangat)
        //{
            
        //}

        UI_GantiChannelTV.SetActive(true);
        sedangNontonTV = true;
        //HarusNontonSeginiDuluBaruLanjut.text = current_MisiNontonTV + " / " + max_MisiNontonTV;

        

    }

    void OnClick_STOP_NontonTV()
    {
        //if (CONNECT_EVT_kulkas != null && CONNECT_EVT_kulkas.sudahAmbilPizza && CONNECT_EVT_PizzaToOven.PizzaSudahHangat)
        //{
            
        //}

        UI_GantiChannelTV.SetActive(false);
        sedangNontonTV = false;
        if (Event_List_Setelah_NontonTV != null)
        {
            Event_List_Setelah_NontonTV.Invoke();
        }

    }

    public void OnClick_MisiChannelTV() // Untuk Di Pasang DI Setiap Tombol CH(nomerchannel)
    {
        if (CONNECT_TVController != null)
        {
            StartCoroutine(channelAktif_AND_SeginiDuluBaruLanjut());
        }
    }

    IEnumerator channelAktif_AND_SeginiDuluBaruLanjut()
    {
        if (CONNECT_TVController.channelAktif == 0 )
        {
            Debug.Log("Channel 0 Proses Menambah INT Misi Nonton TV");
            yield return new WaitForSeconds(2f);
            current_MisiNontonTV += 1;
            UpdateTextMisiNontonTV();
        } 
        else if (CONNECT_TVController.channelAktif == 1)
        {
            Debug.Log("Channel 1 Proses Menambah INT Misi Nonton TV");
            yield return new WaitForSeconds(2f);
            current_MisiNontonTV += 1;
            UpdateTextMisiNontonTV();
        } 
        else if (CONNECT_TVController.channelAktif == 2)
        {
            Debug.Log("Channel 2 Proses Menambah INT Misi Nonton TV");
            yield return new WaitForSeconds(2f);
            current_MisiNontonTV += 1;
            UpdateTextMisiNontonTV();
        }        
    }

    void UpdateTextMisiNontonTV()
    {
        HarusNontonSeginiDuluBaruLanjut.text = current_MisiNontonTV + " / " + max_MisiNontonTV;
        // Jika sudah mencapai max, jangan biarkan melebihi max
        if (current_MisiNontonTV >= max_MisiNontonTV)
        {
            current_MisiNontonTV = max_MisiNontonTV;
        }
        
        Debug.Log("Text Misi Nonton TV Updated");
    }

    // Update is called once per frame
    void Update()
    {
        if (sedangNontonTV)
        {
            if (tombolAksi != null && tombolAksi.action.WasPressedThisFrame())
            {
                OnClick_STOP_NontonTV();
            }
        }
        
    }
}
