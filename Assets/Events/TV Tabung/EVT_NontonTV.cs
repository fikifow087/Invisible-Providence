using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic; // 1. TAMBAHKAN INI untuk menggunakan List
using TMPro;

public class EVT_NontonTV : MonoBehaviour
{
    [Header("Pengaturan Input")]
    public InputActionReference tombolAksi;

    [Header("Koneksi")]
    public EVT_S2_kulkas CONNECT_EVT_kulkas;
    public EVT_S3_PizzaToOven CONNECT_EVT_PizzaToOven;
    public TVController CONNECT_TVController;
    public GameObject UI_GantiChannelTV;
    [SerializeField] private TextMeshProUGUI HarusNontonSeginiDuluBaruLanjut;
    public GameObject CANCEL_NontonTV;
    
    [Header("Variabel Dan Event")]
    public bool sedangNontonTV = false;
    [SerializeField] private int current_MisiNontonTV = 0;
    [SerializeField] private int max_MisiNontonTV = 3;
    public UnityEvent Event_List_Setelah_NontonTV;

    // 2. TAMBAHKAN INI untuk mencatat channel yang sudah ditonton/dipencet
    [SerializeField] private List<int> channelSudahDitonton = new List<int>();
    
    // ==========================================
    // TAMBAHKAN INI AGAR INPUT ACTION-NYA AKTIF
    // ==========================================
    private void OnEnable()
    {
        if (tombolAksi != null)
        {
            tombolAksi.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (tombolAksi != null)
        {
            tombolAksi.action.Disable();
        }
    }
    // ==========================================


    void Start()
    {
        UI_GantiChannelTV.SetActive(false);
        CANCEL_NontonTV.SetActive(false);
        sedangNontonTV = false;
    }

    public void OnClick_START_NontonTV()
    {
        UI_GantiChannelTV.SetActive(true);
        sedangNontonTV = true;
    }

    void OnClick_STOP_NontonTV()
    {
        Debug.Log("Berhenti Nonton TV");
        
        if (CONNECT_TVController != null)
        {
            CONNECT_TVController.StopDanClearTV();
        }

        UI_GantiChannelTV.SetActive(false);
        sedangNontonTV = false;
        if (Event_List_Setelah_NontonTV != null)
        {
            Event_List_Setelah_NontonTV.Invoke();
        }
    }

    public void OnClick_MisiChannelTV() 
    {
        if (CONNECT_TVController != null)
        {
            StartCoroutine(channelAktif_AND_SeginiDuluBaruLanjut());
        }
    }

    IEnumerator channelAktif_AND_SeginiDuluBaruLanjut()
    {
        // Ambil index channel yang sedang aktif saat ini
        int channelSkala = CONNECT_TVController.channelAktif;

        // 3. CEK APAKAH CHANNEL INI SUDAH PERNAH DITONTON
        if (channelSudahDitonton.Contains(channelSkala))
        {
            Debug.Log("Channel " + channelSkala + " sudah pernah ditonton. Misi tidak bertambah.");
            yield break; // Keluar dari Coroutine (membatalkan proses di bawahnya)
        }

        // Jika belum pernah ditonton, langsung masukkan ke list agar tidak bisa di-spam klik selama jeda 2 detik
        channelSudahDitonton.Add(channelSkala);

        if (channelSkala == 0)
        {
            Debug.Log("Channel 0 Proses Menambah INT Misi Nonton TV");
            yield return new WaitForSeconds(2f);
            current_MisiNontonTV += 1;
            UpdateTextMisiNontonTV();
        } 
        else if (channelSkala == 1)
        {
            Debug.Log("Channel 1 Proses Menambah INT Misi Nonton TV");
            yield return new WaitForSeconds(2f);
            current_MisiNontonTV += 1;
            UpdateTextMisiNontonTV();
        } 
        else if (channelSkala == 2)
        {
            Debug.Log("Channel 2 Proses Menambah INT Misi Nonton TV");
            yield return new WaitForSeconds(2f);
            current_MisiNontonTV += 1;
            UpdateTextMisiNontonTV();
        }        
    }

    void UpdateTextMisiNontonTV()
    {
        Debug.Log("Text Misi Nonton TV Updated");
        // 4. BATASI DAN CEK APAKAH MISI SUDAH MAKSIMAL
        if (current_MisiNontonTV >= max_MisiNontonTV)
        {
            if (CANCEL_NontonTV != null)
            {
                CANCEL_NontonTV.SetActive(true);
            }
            
            current_MisiNontonTV = max_MisiNontonTV;
            Debug.Log("Misi Nonton TV Berhasil Sepenuhnya"); // Log keberhasilan sesuai request
        }

        // Update teks UI setelah nilainya dipastikan aman
        HarusNontonSeginiDuluBaruLanjut.text = current_MisiNontonTV + " / " + max_MisiNontonTV;
        
    }

    void Update()
    {
        if (sedangNontonTV && current_MisiNontonTV >= max_MisiNontonTV)
        {
            if (tombolAksi != null && tombolAksi.action.WasPressedThisFrame())
            {
                OnClick_STOP_NontonTV();
            }
        }
    }
}