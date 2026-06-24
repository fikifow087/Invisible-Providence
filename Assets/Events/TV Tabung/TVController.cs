using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

public class TVController : MonoBehaviour
{
    [Header("Komponen Utama")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource; // 1. Tambahkan variabel AudioSource

    [Header("Daftar Channel (Array/List)")]
    [Tooltip("Masukkan semua video clip untuk masing-masing channel di sini")]
    public List<VideoClip> daftarChannel = new List<VideoClip>();

    public int channelAktif = 0;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // 2. Otomatis ambil AudioSource jika belum di-drag di Inspector
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 3. Jalankan konfigurasi Audio Mode lewat code
        SetupTVAudio();

        if (daftarChannel.Count > 0)
        {
            PindahChannel(0);
        }
    }

    void SetupTVAudio()
    {
        if (videoPlayer != null && audioSource != null)
        {
            // Mengubah Audio Output Mode dari 'Direct' menjadi 'Audio Source'
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            // Mengaktifkan track audio pertama (index 0) pada video
            videoPlayer.EnableAudioTrack(0, true);

            // Memasangkan AudioSource target ke track video tersebut
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }
        else
        {
            Debug.LogWarning("VideoPlayer atau AudioSource belum terpasang di Inspector!");
        }
    }

    public void PindahChannel(int indexChannel)
    {
        if (indexChannel >= 0 && indexChannel < daftarChannel.Count)
        {
            if (daftarChannel[indexChannel] != null)
            {
                channelAktif = indexChannel;
                
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