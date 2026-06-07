using System; // Tambahkan ini untuk System.Action
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class KirisaDialogLine
{
    public string SpeakerName;
    public string DialogText;
    public string PortraitID;
    public string VoiceID;
    public string PembeliImageID;
    public string LinkSPK;

    public KirisaDialogLine(string speaker, string dialog, string portrait = "", string pembeliImage = "", string voice = "", string linkSPK = "")
    {
        SpeakerName = speaker;
        DialogText = dialog;
        PortraitID = portrait;
        VoiceID = voice;
        PembeliImageID = pembeliImage;
        LinkSPK = linkSPK;
    }
}

public class KIRISA_DialogueSystem : MonoBehaviour
{
    public static KIRISA_DialogueSystem Instance;

    [Header("UI Components")]
    public GameObject dialogPanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogBodyText;
    public Image portraitImage;
    
    [Header("UI Pembeli (Gambar Besar)")]
    public Image buyerStoryImage;

    [Header("Audio")]
    public AudioSource voiceAudioSource;

    [Header("Skip UI")]
    public GameObject skipButtonContainer;
    public Button skipButton;
    public TextMeshProUGUI skipButtonText;
    public string skipButtonLabel = "Skip";

    public static bool ShowSkipButton = false;
    public static bool ShowSkipButtonV2 = false;

    private Queue<KirisaDialogLine> dialogQueue = new Queue<KirisaDialogLine>();
    private bool isDialogActive = false;

    // Tambahan: Menyimpan fungsi yang akan dipanggil setelah dialog selesai
    private Action onDialogFinished;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (skipButtonContainer != null) skipButtonContainer.SetActive(false);

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipButtonClicked);
        }

        if (skipButtonText != null)
        {
            skipButtonText.text = skipButtonLabel;
        }
    }

    // Fungsi lama tetap dipertahankan agar script lain yang pakai ini tidak error
    public void StartDialog(params KirisaDialogLine[] lines)
    {
        if (ShowSkipButton && ShowSkipButtonV2)
        {
            Debug.LogWarning("[KIRISA] ShowSkipButton and ShowSkipButtonV2 are both active. Please enable only one skip mode.");
        }

        dialogQueue.Clear();
        foreach (KirisaDialogLine line in lines) dialogQueue.Enqueue(line);
        isDialogActive = true;
        dialogPanel.SetActive(true);

        if (skipButtonContainer != null)
            skipButtonContainer.SetActive(ShowSkipButton || ShowSkipButtonV2);

        NextLine();
    }

    // FUNGSI BARU: StartDialog dengan Callback
    public void StartDialogCallback(Action onFinished, params KirisaDialogLine[] lines)
    {
        onDialogFinished = onFinished; // Simpan perintah selanjutnya
        StartDialog(lines); // Jalankan dialog seperti biasa
    }

    public void NextLine()
    {
        if (dialogQueue.Count == 0) { EndDialog(); return; }

        voiceAudioSource.Stop();
        KirisaDialogLine currentLine = dialogQueue.Dequeue();

        speakerNameText.text = currentLine.SpeakerName;
        dialogBodyText.text = currentLine.DialogText;

        HandlePembeliImage(currentLine);
        HandlePortrait(currentLine);
        HandleVoice(currentLine);
    }

    void HandleVoice(KirisaDialogLine line)
    {
        if (string.IsNullOrEmpty(line.VoiceID) || line.VoiceID.ToLower() == "none") return;

        string speakerFolder = line.SpeakerName;
        if (!string.IsNullOrEmpty(line.LinkSPK) && line.LinkSPK.ToLower() != "none")
        {
            speakerFolder = line.LinkSPK;
        }

        string pathKarakter = "Voices/VA_" + speakerFolder.ToUpper() + "/" + line.VoiceID;
        AudioClip clip = Resources.Load<AudioClip>(pathKarakter);

        if (clip == null)
        {
            clip = Resources.Load<AudioClip>("Voices/" + line.VoiceID);
        }

        if (clip != null)
        {
            voiceAudioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[KIRISA AUDIO ERROR] File tidak ketemu di: {pathKarakter} atau Voices/{line.VoiceID}");
        }
    }

    void HandlePembeliImage(KirisaDialogLine line)
    {
        if (buyerStoryImage == null) return;

        if (!string.IsNullOrEmpty(line.PembeliImageID))
        {
            if (line.PembeliImageID.ToLower() == "null")
            {
                buyerStoryImage.sprite = null;
                buyerStoryImage.color = new Color(1, 1, 1, 0);
            }
            else
            {
                string speakerFolder = line.SpeakerName;
                if (!string.IsNullOrEmpty(line.LinkSPK) && line.LinkSPK.ToLower() != "none")
                {
                    speakerFolder = line.LinkSPK;
                }

                string path = "Pembeli/" + speakerFolder.ToUpper() + "/" + line.PembeliImageID;
                Sprite s = Resources.Load<Sprite>(path);
                if (s == null) s = Resources.Load<Sprite>("Pembeli/" + line.PembeliImageID);

                if (s != null)
                {
                    buyerStoryImage.sprite = s;
                    buyerStoryImage.color = Color.white;
                }
            }
        }
    }

    void HandlePortrait(KirisaDialogLine line)
    {
        if (portraitImage == null) return;

        if (!string.IsNullOrEmpty(line.PortraitID) && line.PortraitID.ToLower() != "none")
        {
            string path = "Portraits/" + line.SpeakerName.ToUpper() + "/" + line.PortraitID;
            Sprite s = Resources.Load<Sprite>(path);
            if (s != null) { portraitImage.sprite = s; portraitImage.enabled = true; }
        }
        else { portraitImage.enabled = false; }
    }

    void EndDialog()
    {
        isDialogActive = false;
        dialogPanel.SetActive(false);

        if (skipButtonContainer != null)
            skipButtonContainer.SetActive(false);

        // PERUBAHAN: Panggil fungsi yang dititipkan (jika ada), lalu hapus dari memori
        onDialogFinished?.Invoke();
        onDialogFinished = null;
    }

    public void SkipDialog()
    {
        if (!isDialogActive) return;
        dialogQueue.Clear();
        voiceAudioSource?.Stop();
        EndDialog();
    }

    public void SkipDialogV2()
    {
        if (!isDialogActive) return;
        if (dialogQueue.Count == 0) { EndDialog(); return; }

        KirisaDialogLine finalLine = null;
        while (dialogQueue.Count > 0)
        {
            finalLine = dialogQueue.Dequeue();
        }

        if (finalLine == null) { EndDialog(); return; }

        voiceAudioSource.Stop();
        speakerNameText.text = finalLine.SpeakerName;
        dialogBodyText.text = finalLine.DialogText;
        HandlePembeliImage(finalLine);
        HandlePortrait(finalLine);
        HandleVoice(finalLine);
    }

    private void OnSkipButtonClicked()
    {
        if (ShowSkipButton && ShowSkipButtonV2)
        {
            Debug.LogWarning("[KIRISA] Skip button has conflicting modes: both ShowSkipButton and ShowSkipButtonV2 are enabled. Defaulting to standard skip.");
            SkipDialog();
            return;
        }

        if (ShowSkipButtonV2)
        {
            SkipDialogV2();
            return;
        }

        SkipDialog();
    }

    void Update()
    {
        if (!isDialogActive) return;
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
            NextLine();
    }
}