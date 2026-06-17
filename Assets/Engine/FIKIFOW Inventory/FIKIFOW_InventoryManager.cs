using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Murni menggunakan New Input System
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
    [SerializeField] private Transform itemContainer;    
    [SerializeField] private GameObject textButtonPrefab; 

    [Header("UI Text Displays (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI capacityText;
    [SerializeField] private TextMeshProUGUI filterClueText;
    [SerializeField] private TextMeshProUGUI leftSlotText;
    [SerializeField] private TextMeshProUGUI rightSlotText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Input Actions Unity 6 (Modern)")]
    [Tooltip("Input Action untuk membuka/tutup inventory (Misal: TAB)")]
    [SerializeField] private InputActionReference toggleInventoryInput;
    
    [Tooltip("Input Action untuk mengganti filter kategori saat inventory terbuka (Misal: Q)")]
    [SerializeField] private InputActionReference switchFilterInput; // PERBAIKAN: Input Action Baru

    // Track status barang di tangan
    private FIKIFOW_ImportantItem equippedLeft = null;
    private FIKIFOW_ImportantItem equippedRight = null;
    
    // Filter status
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
        UpdateHandSlotsUI();
    }

    void Update()
    {
        // 1. Cek Input Buka/Tutup Tas (TAB)
        if (toggleInventoryInput != null && toggleInventoryInput.action.WasPressedThisFrame())
        {
            ToggleInventory();
        }

        // 2. PERBAIKAN: Cek Input Ganti Filter menggunakan New Input System (Q)
        if (inventoryPanel.activeSelf && switchFilterInput != null && switchFilterInput.action.WasPressedThisFrame())
        {
            SwitchFilter();
        }
    }

    // Aktifkan seluruh aksi input saat objek aktif
    void OnEnable()
    {
        if (toggleInventoryInput != null && toggleInventoryInput.action != null)
        {
            toggleInventoryInput.action.Enable();
        }

        if (switchFilterInput != null && switchFilterInput.action != null)
        {
            switchFilterInput.action.Enable();
        }
    }

    // Matikan seluruh aksi input saat objek tidak aktif (Mencegah Memory Leak)
    void OnDisable()
    {
        if (toggleInventoryInput != null && toggleInventoryInput.action != null)
        {
            toggleInventoryInput.action.Disable();
        }

        if (switchFilterInput != null && switchFilterInput.action != null)
        {
            switchFilterInput.action.Disable();
        }
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
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        foreach (Transform child in itemContainer) Destroy(child.gameObject);

        string[] filterNames = { "SEMUA ITEM", "UNKNOWN", "KEY", "DOCUMENT" };
        if (currentFilterIndex == 0)
            filterClueText.text = "FILTER: " + filterNames[currentFilterIndex];
        else
            filterClueText.text = "<color=yellow>FILTER: " + filterNames[currentFilterIndex] + "</color>";

        capacityText.text = "KAPASITAS: " + itemsInInventory.Count + " / " + maxCapacity;

        foreach (var item in itemsInInventory)
        {
            if (currentFilterIndex > 0 && (int)item.kategori != (currentFilterIndex - 1)) 
                continue; 

            GameObject btn = Instantiate(textButtonPrefab, itemContainer);
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            
            if (item == equippedLeft) btnText.text = "[Kiri] " + item.namaItem;
            else if (item == equippedRight) btnText.text = "[Kanan] " + item.namaItem;
            else btnText.text = item.namaItem;

            btn.GetComponent<Button>().onClick.AddListener(() => SelectItem(item));
        }
    }

    private void UpdateHandSlotsUI()
    {
        if (!hasLeftArm)
        {
            leftSlotText.text = "<color=red>[SLOT KIRI: CACAT/AMPUTASI]</color>";
        }
        else
        {
            leftSlotText.text = "TANGAN KIRI: " + (equippedLeft != null ? equippedLeft.namaItem : "(Kosong)");
        }

        if (!hasRightArm)
        {
            rightSlotText.text = "<color=red>[SLOT KANAN: CACAT/AMPUTASI]</color>";
        }
        else
        {
            rightSlotText.text = "TANGAN KANAN: " + (equippedRight != null ? equippedRight.namaItem : "(Kosong)");
        }
    }

    private void SelectItem(FIKIFOW_ImportantItem item)
    {
        selectedItem = item;
        descriptionText.text = "<b>" + item.namaItem + "</b>\nKategori: " + item.kategori.ToString() + "\n\n" + item.deskripsiItem;
    }

    public void ClickEquipLeft()
    {
        if (!hasLeftArm || selectedItem == null) return;
        EquipItem(selectedItem, false);
    }

    public void ClickEquipRight()
    {
        if (!hasRightArm || selectedItem == null) return;
        EquipItem(selectedItem, true);
    }

    private void EquipItem(FIKIFOW_ImportantItem item, bool isRightHand)
    {
        if (isRightHand)
        {
            if (equippedLeft == item) equippedLeft = null; 
            if (equippedRight != null) equippedRight.gameObject.SetActive(false);
            
            equippedRight = item;
            AttachToHolder(item, itemHoldPointR);
        }
        else
        {
            if (equippedRight == item) equippedRight = null; 
            if (equippedLeft != null) equippedLeft.gameObject.SetActive(false);

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