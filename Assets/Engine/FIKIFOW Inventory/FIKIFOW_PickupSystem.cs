using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("FIKIFOW FPS/5 - Pickup System Fixed")]
public class FIKIFOW_PickupSystem : MonoBehaviour
{
    [SerializeField] private float pickupDistance = 2.5f;
    [SerializeField] private InputActionReference interactInput;

    void Update()
    {
        if (FIKIFOWFPS1_FirstPersonEngine.Instance != null && FIKIFOWFPS1_FirstPersonEngine.Instance.isInputBlocked)
            return;

        if (interactInput != null && interactInput.action.WasPressedThisFrame())
        {
            TryPickupItem();
        }
    }

    private void TryPickupItem()
    {
        Transform cameraTransform = FIKIFOWFPS1_FirstPersonEngine.Instance != null ? FIKIFOWFPS1_FirstPersonEngine.Instance.cameraHolder : null;
        if (cameraTransform == null) return;

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupDistance))
        {
            FIKIFOW_ImportantItem importantItem = hit.collider.GetComponent<FIKIFOW_ImportantItem>();
            if (importantItem == null) return;

            // Masukkan data barang ke manajemen inventory
            if (FIKIFOW_InventoryManager.Instance != null)
            {
                bool success = FIKIFOW_InventoryManager.Instance.AddToInventory(importantItem);
                if (success)
                {
                    // Hilangkan aspek fisik dunia nyata
                    Collider col = importantItem.GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    Rigidbody rb = importantItem.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;
                }
            }
        }
    }
}