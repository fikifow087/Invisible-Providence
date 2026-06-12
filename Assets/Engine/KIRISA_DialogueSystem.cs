using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class KirisaDialogLine
{
    public int DialogMode; // 1 = Butuh Input (Player diam), 2 = Auto Durasi (Player bebas)
    public float Duration; // Durasi untuk mode 2
    
    public string SpeakerName;
    public string DialogText;
    public string PortraitID;
    public string VoiceID;
    public string PembeliImageID;
    public string LinkSPK;

    // Constructor Baru sesuai permintaan (Mode & Durasi di awal)
    public KirisaDialogLine(int mode, float duration, string speaker, string dialog, string portrait = "", string pembeliImage = "", string voice = "", string linkSPK = "")
    {
        DialogMode = mode;
        Duration = duration;
        SpeakerName = speaker;
        DialogText = dialog;
        PortraitID = portrait;
        VoiceID = voice;
        PembeliImageID = pembeliImage;
        LinkSPK = linkSPK;
    }

    // Constructor Lama (Untuk kompatibilitas jika ada script lama yang masih pakai ini, otomatis jadi Mode 1)
    public KirisaDialogLine(string speaker, string dialog, string portrait = "", string pembeliImage = "", string voice = "", string linkSPK = "")
    {
        DialogMode = 1;
        Duration = 0f;
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

    [Header("Input System Baru")]
    [Tooltip("Input untuk lanjut dialog (Otomatis di-bind ke Space dan Left Click)")]
    public InputAction continueDialogAction;

    public static bool ShowSkipButton = false;
    public static bool ShowSkipButtonV2 = false;

    // --- FITUR BLOCK INPUT PLAYER ---
    public static bool IsPlayerInputBlocked { get; private set; }
    public static Action OnBlockPlayerInput;
    public static Action OnUnblockPlayerInput;

    private Queue<KirisaDialogLine> dialogQueue = new Queue<KirisaDialogLine>();
    private bool isDialogActive = false;
    private KirisaDialogLine currentLine;
    private Action onDialogFinished;
    private Coroutine autoAdvanceCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Setup otomatis Input Action tanpa perlu setting di Inspector
        if (continueDialogAction == null || continueDialogAction.bindings.Count == 0)
        {
            continueDialogAction = new InputAction("ContinueDialog", binding: "<Keyboard>/space");
            continueDialogAction.AddBinding("<Mouse>/leftButton");
        }
    }

    void OnEnable()
    {
        continueDialogAction.Enable();
    }

    void OnDisable()
    {
        continueDialogAction.Disable();
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

    public void StartDialogCallback(Action onFinished, params KirisaDialogLine[] lines)
    {
        onDialogFinished = onFinished; 
        StartDialog(lines); 
    }

    public void NextLine()
    {
        // Stop coroutine jika sebelumnya sedang mode auto
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        if (dialogQueue.Count == 0) 
        { 
            EndDialog(); 
            return; 
        }

        voiceAudioSource.Stop();
        currentLine = dialogQueue.Dequeue();

        speakerNameText.text = currentLine.SpeakerName;
        dialogBodyText.text = currentLine.DialogText;

        HandlePembeliImage(currentLine);
        HandlePortrait(currentLine);
        HandleVoice(currentLine);

        // --- PENANGANAN MODE 1 & 2 ---
        if (currentLine.DialogMode == 1)
        {
            BlockPlayer(); // Player diam, tunggu input
        }
        else if (currentLine.DialogMode == 2)
        {
            UnblockPlayer(); // Player bebas bergerak
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDialog(currentLine.Duration));
        }
    }

    private IEnumerator AutoAdvanceDialog(float duration)
    {
        yield return new WaitForSeconds(duration);
        NextLine(); // Lanjut otomatis setelah durasi habis
    }

    void Update()
    {
        if (!isDialogActive) return;

        // Hanya deteksi input jika saat ini berada di Mode 1
        if (currentLine != null && currentLine.DialogMode == 1)
        {
            if (continueDialogAction.WasPressedThisFrame())
            {
                NextLine();
            }
        }
    }

    // --- FUNGSI UNTUK MENGONTROL BLOCKING ---
    private void BlockPlayer()
    {
        if (!IsPlayerInputBlocked)
        {
            IsPlayerInputBlocked = true;
            OnBlockPlayerInput?.Invoke(); // Trigger event untuk script player
        }
    }

    private void UnblockPlayer()
    {
        if (IsPlayerInputBlocked)
        {
            IsPlayerInputBlocked = false;
            OnUnblockPlayerInput?.Invoke(); // Trigger event untuk script player
        }
    }

    void EndDialog()
    {
        isDialogActive = false;
        dialogPanel.SetActive(false);

        if (skipButtonContainer != null)
            skipButtonContainer.SetActive(false);

        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
        }

        UnblockPlayer(); // Pastikan player bisa bergerak lagi setelah dialog selesai

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

        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);

        voiceAudioSource.Stop();
        currentLine = finalLine;
        speakerNameText.text = finalLine.SpeakerName;
        dialogBodyText.text = finalLine.DialogText;
        HandlePembeliImage(finalLine);
        HandlePortrait(finalLine);
        HandleVoice(finalLine);

        if (finalLine.DialogMode == 1) BlockPlayer();
        else if (finalLine.DialogMode == 2)
        {
            UnblockPlayer();
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDialog(finalLine.Duration));
        }
    }

    private void OnSkipButtonClicked()
    {
        if (ShowSkipButton && ShowSkipButtonV2)
        {
            Debug.LogWarning("[KIRISA] Skip button has conflicting modes. Defaulting to standard skip.");
            SkipDialog();
            return;
        }

        if (ShowSkipButtonV2) { SkipDialogV2(); return; }

        SkipDialog();
    }

    // --- FUNGSI HANDLE RESOURCE (TIDAK ADA PERUBAHAN) ---
    void HandleVoice(KirisaDialogLine line)
    {
        if (string.IsNullOrEmpty(line.VoiceID) || line.VoiceID.ToLower() == "none") return;
        string speakerFolder = string.IsNullOrEmpty(line.LinkSPK) || line.LinkSPK.ToLower() == "none" ? line.SpeakerName : line.LinkSPK;
        string pathKarakter = "Voices/VA_" + speakerFolder.ToUpper() + "/" + line.VoiceID;
        AudioClip clip = Resources.Load<AudioClip>(pathKarakter) ?? Resources.Load<AudioClip>("Voices/" + line.VoiceID);
        if (clip != null) voiceAudioSource.PlayOneShot(clip);
        else Debug.LogWarning($"[KIRISA AUDIO ERROR] File tidak ketemu di: {pathKarakter} atau Voices/{line.VoiceID}");
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
                string speakerFolder = string.IsNullOrEmpty(line.LinkSPK) || line.LinkSPK.ToLower() == "none" ? line.SpeakerName : line.LinkSPK;
                string path = "Pembeli/" + speakerFolder.ToUpper() + "/" + line.PembeliImageID;
                Sprite s = Resources.Load<Sprite>(path) ?? Resources.Load<Sprite>("Pembeli/" + line.PembeliImageID);
                if (s != null) { buyerStoryImage.sprite = s; buyerStoryImage.color = Color.white; }
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
}