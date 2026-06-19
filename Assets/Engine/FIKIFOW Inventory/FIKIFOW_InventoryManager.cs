using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 
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
    
    [Header("Drop Settings")]
    [Tooltip("Titik referensi jatuhnya barang (Misal: Kosongkan GameObject di depan kamera pemain)")]
    public Transform dropPoint;

    [Header("Inventory Settings")]
    public int maxCapacity = 6;
    private List<FIKIFOW_ImportantItem> itemsInInventory = new List<FIKIFOW_ImportantItem>();

    [Header("UI Canvas Panels")]
    [SerializeField] private GameObject inventoryPanel; 
    [SerializeField] private Transform itemContainer;    
    [SerializeField] private GameObject textButtonPrefab; 
    
    [Header("UI Popup Action Settings")]
    [SerializeField] private GameObject actionPopupPanel; 
    [SerializeField] private Button unequipButton;        

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
    private int currentFilterIndex = 0; 
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
            
            if (actionPopupPanel != null) actionPopupPanel.SetActive(false); 
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
        if (actionPopupPanel != null) actionPopupPanel.SetActive(false); 
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        if (itemContainer == null || textButtonPrefab == null) return;

        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemContainer.GetChild(i).gameObject);
        }

        string[] filterNames = { "SEMUA ITEM", "UNKNOWN", "KEY", "DOCUMENT" };
        filterClueText.text = currentFilterIndex == 0 ? "FILTER: " + filterNames[currentFilterIndex] : "<color=yellow>[FILTER AKTIF: " + filterNames[currentFilterIndex] + "]</color>";
        capacityText.text = "KAPASITAS: " + itemsInInventory.Count + " / " + maxCapacity;

        foreach (var item in itemsInInventory)
        {
            if (currentFilterIndex > 0 && (int)item.kategori != (currentFilterIndex - 1)) 
                continue; 

            GameObject btn = Instantiate(textButtonPrefab, itemContainer);
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            
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

    private void OpenActionPopup(FIKIFOW_ImportantItem item)
    {
        selectedItem = item;
        descriptionText.text = "<b>" + item.namaItem + "</b>\nKategori: " + item.kategori.ToString() + "\n\n" + item.deskripsiItem;

        if (actionPopupPanel != null)
        {
            actionPopupPanel.SetActive(true);

            if (unequipButton != null)
            {
                bool isCurrentlyEquipped = (item == equippedLeft || item == equippedRight);
                unequipButton.gameObject.SetActive(isCurrentlyEquipped); 
            }
        }
    }

    public void CloseActionPopup()
    {
        if (actionPopupPanel != null) actionPopupPanel.SetActive(false);
    }

    public void ClickActionEquipLeft()
    {
        if (!hasLeftArm || selectedItem == null) return;
        EquipItem(selectedItem, false);
        CloseActionPopup();
    }

    public void ClickActionEquipRight()
    {
        if (!hasRightArm || selectedItem == null) return;
        EquipItem(selectedItem, true);
        CloseActionPopup();
    }

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

    // --- FITUR BARU: DROP BARANG (DENGAN FIX COLLIDER) ---
    public void ClickActionDrop()
    {
        if (selectedItem == null) return;

        // 1. Lepas dari tangan jika sedang dipegang
        if (equippedLeft == selectedItem) equippedLeft = null;
        if (equippedRight == selectedItem) equippedRight = null;

        // 2. Hapus dari list inventory sistem
        itemsInInventory.Remove(selectedItem);

        // 3. Keluarkan ke dunia nyata (Unparent)
        selectedItem.transform.SetParent(null); 
        
        // Posisikan barang ke titik jatuh
        if (dropPoint != null) 
            selectedItem.transform.position = dropPoint.position;
        else if (itemHoldPointR != null) 
            selectedItem.transform.position = itemHoldPointR.position;

        selectedItem.gameObject.SetActive(true); 

        // --- PERBAIKAN BUG: NYALAKAN KEMBALI COLLIDER ---
        Collider col = selectedItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true; // Agar objek bisa berbenturan dengan lantai/tanah
        }

        // 4. Pastikan Rigidbody berjalan (agar terkena gravitasi dan jatuh ke lantai)
        Rigidbody rb = selectedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; 
            
            // Berikan sedikit dorongan ke depan saat di-drop agar tidak mengenai kaki player
            if (Camera.main != null)
            {
                rb.AddForce(Camera.main.transform.forward * 2f, ForceMode.Impulse);
            }
        }

        selectedItem = null;
        CloseActionPopup();
        UpdateHandSlotsUI();
        RefreshInventoryUI();
    }

    private void EquipItem(FIKIFOW_ImportantItem item, bool isRightHand)
    {
        if (isRightHand)
        {
            if (equippedLeft == item) equippedLeft = null;

            if (equippedRight != null && equippedRight != item)
            {
                equippedRight.gameObject.SetActive(false);
            }
            
            equippedRight = item;
            AttachToHolder(item, itemHoldPointR);
        }
        else
        {
            if (equippedRight == item) equippedRight = null;

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
            
            // Saat dipegang, pastikan Rigidbody dimatikan agar tidak jatuh dari tangan
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
    }

    // ==========================================
    // SEBAGAI SYARAT INTEGRASI SISTEM INTERAKSI UNIVERSAL
    // ==========================================
    
    public bool ApakahItemSedangDipegang(string namaItem)
    {
        bool diKiri = (equippedLeft != null && equippedLeft.namaItem == namaItem);
        bool diKanan = (equippedRight != null && equippedRight.namaItem == namaItem);
        return diKiri || diKanan;
    }

    public bool TaruhItemDariTangan(string namaItemYangDicari, Vector3 posisiTarget, Quaternion rotasiTarget, Transform parentBaru = null)
    {
        FIKIFOW_ImportantItem itemDitemukan = null;

        if (equippedLeft != null && equippedLeft.namaItem == namaItemYangDicari)
        {
            itemDitemukan = equippedLeft;
            equippedLeft = null;
        }
        else if (equippedRight != null && equippedRight.namaItem == namaItemYangDicari)
        {
            itemDitemukan = equippedRight;
            equippedRight = null;
        }

        if (itemDitemukan != null)
        {
            if (itemsInInventory.Contains(itemDitemukan))
            {
                itemsInInventory.Remove(itemDitemukan);
            }

            itemDitemukan.transform.SetParent(parentBaru);
            itemDitemukan.transform.position = posisiTarget;
            itemDitemukan.transform.rotation = rotasiTarget;
            itemDitemukan.gameObject.SetActive(true);

            // Nyalakan kembali fisik dunia nyata agar objek diam rapi di target lokasi
            Rigidbody rb = itemDitemukan.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = itemDitemukan.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            UpdateHandSlotsUI();
            RefreshInventoryUI();
            return true;
        }
        return false;
    }
}