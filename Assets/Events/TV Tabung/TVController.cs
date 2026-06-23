using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

public class TVController : MonoBehaviour
{
    [Header("Komponen Utama")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Daftar Channel (Array/List)")]
    [Tooltip("Masukkan semua video clip untuk masing-masing channel di sini")]
    public List<VideoClip> daftarChannel = new List<VideoClip>();

    private int channelAktif = 0;

    void Start()
    {
        // Ambil komponen Video Player jika belum di-drag di Inspector
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Putar channel pertama secara otomatis saat game dimulai (opsional)
        if (daftarChannel.Count > 0)
        {
            PindahChannel(0);
        }
    }

    // Fungsi Utama yang akan dipanggil oleh Tombol UI
    public void PindahChannel(int indexChannel)
    {
        // Validasi apakah index channel yang ditekan ada di dalam List
        if (indexChannel >= 0 && indexChannel < daftarChannel.Count)
        {
            if (daftarChannel[indexChannel] != null)
            {
                channelAktif = indexChannel;
                
                // Ganti video clip dan putar
                videoPlayer.clip = daftarChannel[indexChannel];
                videoPlayer.Play();
                
                Debug.Log("Menonton Channel " + (indexChannel + 1));
            }
            else
            {
                Debug.LogWarning("Slot video pada Channel " + (indexChannel + 1) + " masih kosong!");
            }
        }
        else
        {
            Debug.LogError("Channel tidak ditemukan!");
        }
    }
}