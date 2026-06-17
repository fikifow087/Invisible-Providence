using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Menggunakan New Input System Unity 6
using TMPro;

public class FIKIFOW_InventoryManager : MonoBehaviour
{
    public static FIKIFOW_InventoryManager Instance;

    [Header("Lore & Hand Configuration")]
    public bool hasLeftArm = true;
    public bool hasRightArm = true;

    [Header("Hand Holders (Transforms Near Camera)")]
    public Transform itemHoldPointL;
    public Transform itemHoldPointR;

    [Header("Inventory Settings")]
    public int maxCapacity = 6;
    private List<FIKIFOW_ImportantItem> itemsInInventory = new List<FIKIFOW_ImportantItem>();

    [Header("UI Canvas Panels")]
    [SerializeField] private GameObject inventoryPanel; 
    [SerializeField] private Transform itemContainer;    // Tempat scroll view list item
    [SerializeField] private GameObject textButtonPrefab; // Prefab tombol list item
    
    [Header("UI Popup Action Settings")]
    [SerializeField] private GameObject actionPopupPanel; // GameObject Panel Popup Kecil
    [SerializeField] private Button unequipButton;        // Referensi tombol Lepas di dalam Popup

    [Header("UI Text Displays (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI capacityText;
    [SerializeField] private TextMeshProUGUI filterClueText;
    [SerializeField] private TextMeshProUGUI leftSlotText;
    [SerializeField] private TextMeshProUGUI rightSlotText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Input Actions Unity 6")]
    [SerializeField] private InputActionReference toggleInventoryInput;
    [SerializeField] private InputActionReference switchFilterInput; 

    // Status tracking item di tangan
    private FIKIFOW_ImportantItem equippedLeft = null;
    private FIKIFOW_ImportantItem equippedRight = null;
    
    // Filter & Selection
    private int currentFilterIndex = 0; // 0: ALL, 1: UNKNOWN, 2: KEY, 3: DOCUMENT
    private FIKIFOW_ImportantItem selectedItem = null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        inventoryPanel.SetActive(false);
        if (actionPopupPanel != null) actionPopupPanel.SetActive(false);
        UpdateHandSlotsUI();
    }

    void Update()
    {
        if (toggleInventoryInput != null && toggleInventoryInput.action.WasPressedThisFrame())
        {
            ToggleInventory();
        }

        if (inventoryPanel.activeSelf && switchFilterInput != null && switchFilterInput.action.WasPressedThisFrame())
        {
            SwitchFilter();
        }
    }

    void OnEnable()
    {
        if (toggleInventoryInput != null && toggleInventoryInput.action != null) toggleInventoryInput.action.Enable();
        if (switchFilterInput != null && switchFilterInput.action != null) switchFilterInput.action.Enable();
    }

    void OnDisable()
    {
        if (toggleInventoryInput != null && toggleInventoryInput.action != null) toggleInventoryInput.action.Disable();
        if (switchFilterInput != null && switchFilterInput.action != null) switchFilterInput.action.Disable();
    }

    public bool AddToInventory(FIKIFOW_ImportantItem item)
    {
        if (itemsInInventory.Count >= maxCapacity)
        {
            Debug.LogWarning("Inventory Penuh!");
            return false;
        }

        itemsInInventory.Add(item);
        item.gameObject.SetActive(false); 

        // Auto-equip bawaan saat pertama kali memungut jika tangan kosong
        if (hasRightArm && equippedRight == null) EquipItem(item, true);
        else if (hasLeftArm && equippedLeft == null) EquipItem(item, false);

        return true;
    }

    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            FIKIFOWFPS1_FirstPersonEngine.Instance.BlockInput();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (actionPopupPanel != null) actionPopupPanel.SetActive(false); // Reset popup saat buka
            RefreshInventoryUI();
        }
        else
        {
            FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SwitchFilter()
    {
        currentFilterIndex = (currentFilterIndex + 1) % 4;
        if (actionPopupPanel != null) actionPopupPanel.SetActive(false); // Tutup popup jika ganti filter
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        if (itemContainer == null || textButtonPrefab == null) return;

        // Bersihkan UI lama
        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemContainer.GetChild(i).gameObject);
        }

        string[] filterNames = { "SEMUA ITEM", "UNKNOWN", "KEY", "DOCUMENT" };
        filterClueText.text = currentFilterIndex == 0 ? "FILTER: " + filterNames[currentFilterIndex] : "<color=yellow>[FILTER AKTIF: " + filterNames[currentFilterIndex] + "]</color>";
        capacityText.text = "KAPASITAS: " + itemsInInventory.Count + " / " + maxCapacity;

        // Buat daftar list item sesuai filter
        foreach (var item in itemsInInventory)
        {
            if (currentFilterIndex > 0 && (int)item.kategori != (currentFilterIndex - 1)) 
                continue; 

            GameObject btn = Instantiate(textButtonPrefab, itemContainer);
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            
            // Berikan status teks visual di daftar tas
            if (item == equippedLeft) btnText.text = "<color=#3498db>[Kiri]</color> " + item.namaItem;
            else if (item == equippedRight) btnText.text = "<color=#2ecc71>[Kanan]</color> " + item.namaItem;
            else btnText.text = item.namaItem;

            btn.GetComponent<Button>().onClick.AddListener(() => OpenActionPopup(item));
        }
    }

    private void UpdateHandSlotsUI()
    {
        if (!hasLeftArm) leftSlotText.text = "<color=red>[SLOT KIRI: CACAT]</color>";
        else leftSlotText.text = "TANGAN KIRI:\n" + (equippedLeft != null ? "<color=#3498db>" + equippedLeft.namaItem + "</color>" : "(Kosong)");

        if (!hasRightArm) rightSlotText.text = "<color=red>[SLOT KANAN: CACAT]</color>";
        else rightSlotText.text = "TANGAN KANAN:\n" + (equippedRight != null ? "<color=#2ecc71>" + equippedRight.namaItem + "</color>" : "(Kosong)");
    }

    // --- SISTEM POPUP BARU ---
    private void OpenActionPopup(FIKIFOW_ImportantItem item)
    {
        selectedItem = item;
        descriptionText.text = "<b>" + item.namaItem + "</b>\nKategori: " + item.kategori.ToString() + "\n\n" + item.deskripsiItem;

        if (actionPopupPanel != null)
        {
            actionPopupPanel.SetActive(true);

            // Cek apakah item ini sedang di-equip atau tidak untuk menentukan aktifnya tombol unequip
            if (unequipButton != null)
            {
                bool isCurrentlyEquipped = (item == equippedLeft || item == equippedRight);
                unequipButton.gameObject.SetActive(isCurrentlyEquipped); // Hanya muncul jika barang sedang dipegang
            }
        }
    }

    public void CloseActionPopup()
    {
        if (actionPopupPanel != null) actionPopupPanel.SetActive(false);
    }

    // --- METODE AKSI UNTUK TOMBOL DI DALAM POPUP ---

    // Hubungkan ini ke OnClick() Tombol "Equip Kiri" di dalam Panel Popup
    public void ClickActionEquipLeft()
    {
        if (!hasLeftArm || selectedItem == null) return;
        EquipItem(selectedItem, false);
        CloseActionPopup();
    }

    // Hubungkan ini ke OnClick() Tombol "Equip Kanan" di dalam Panel Popup
    public void ClickActionEquipRight()
    {
        if (!hasRightArm || selectedItem == null) return;
        EquipItem(selectedItem, true);
        CloseActionPopup();
    }

    // Hubungkan ini ke OnClick() Tombol "Unequip / Lepas" di dalam Panel Popup
    public void ClickActionUnequip()
    {
        if (selectedItem == null) return;

        if (equippedLeft == selectedItem)
        {
            selectedItem.gameObject.SetActive(false);
            equippedLeft = null;
        }
        else if (equippedRight == selectedItem)
        {
            selectedItem.gameObject.SetActive(false);
            equippedRight = null;
        }

        CloseActionPopup();
        UpdateHandSlotsUI();
        RefreshInventoryUI();
    }

    // --- INTI LOGIKA EQUIP & REPLACEMENT ---
    private void EquipItem(FIKIFOW_ImportantItem item, bool isRightHand)
    {
        if (isRightHand)
        {
            // Jika item ini ternyata sedang dipegang di tangan kiri, kosongkan dulu tangan kiri
            if (equippedLeft == item) equippedLeft = null;

            // --- LOGIKA REPLACE KANAN ---
            // Jika tangan kanan memegang barang LAIN, sembunyikan barang lama tersebut ke tas kembali
            if (equippedRight != null && equippedRight != item)
            {
                equippedRight.gameObject.SetActive(false);
            }
            
            equippedRight = item;
            AttachToHolder(item, itemHoldPointR);
        }
        else
        {
            // Jika item ini ternyata sedang dipegang di tangan kanan, kosongkan dulu tangan kanan
            if (equippedRight == item) equippedRight = null;

            // --- LOGIKA REPLACE KIRI ---
            // Jika tangan kiri memegang barang LAIN, sembunyikan barang lama tersebut ke tas kembali
            if (equippedLeft != null && equippedLeft != item)
            {
                equippedLeft.gameObject.SetActive(false);
            }

            equippedLeft = item;
            AttachToHolder(item, itemHoldPointL);
        }

        UpdateHandSlotsUI();
        RefreshInventoryUI();
    }

    private void AttachToHolder(FIKIFOW_ImportantItem item, Transform holder)
    {
        if (holder == null) return;
        item.gameObject.SetActive(true);
        item.transform.SetParent(holder);
        
        if (item.ambilInstan)
        {
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }
    }
}