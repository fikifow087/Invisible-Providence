using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("FIKIFOW FPS/5 - Pickup System Fixed")]
public class FIKIFOW_PickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupDistance = 2.5f;
    [SerializeField] private Transform itemHoldPoint;
    [SerializeField] private float smoothSpeed = 12f;

    [Header("Input Action")]
    [SerializeField] private InputActionReference interactInput;

    private GameObject currentHeldItem = null;
    private Collider itemCollider = null;
    private Rigidbody itemRigidbody = null;
    private bool shouldLerp = false;

    void Update()
    {
        // Cegah pickup jika input player sedang di-block (baca dokumen/dialog)
        if (FIKIFOWFPS1_FirstPersonEngine.Instance != null && FIKIFOWFPS1_FirstPersonEngine.Instance.isInputBlocked)
            return;

        if (interactInput != null && interactInput.action.WasPressedThisFrame())
        {
            if (currentHeldItem == null)
                TryPickupItem();
            else
                DropItem();
        }

        // Efek visual pergerakan objek
        if (currentHeldItem != null && itemHoldPoint != null)
        {
            if (shouldLerp)
            {
                // Meluncur halus
                currentHeldItem.transform.localPosition = Vector3.Lerp(currentHeldItem.transform.localPosition, Vector3.zero, Time.deltaTime * smoothSpeed);
                currentHeldItem.transform.localRotation = Quaternion.Slerp(currentHeldItem.transform.localRotation, Quaternion.identity, Time.deltaTime * smoothSpeed);
            }
        }
    }

    private void TryPickupItem()
    {
        Transform cameraTransform = FIKIFOWFPS1_FirstPersonEngine.Instance != null ? FIKIFOWFPS1_FirstPersonEngine.Instance.cameraHolder : null;
        if (cameraTransform == null) return;

        RaycastHit hit;
        // Tembakkan raycast (mendeteksi semua objek terlebih dahulu)
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupDistance))
        {
            // --- FILTER UTAMA: Cek apakah objek memiliki KTP/Tanda 'Important Item' ---
            FIKIFOW_ImportantItem importantItem = hit.collider.GetComponent<FIKIFOW_ImportantItem>();

            // Jika objek TIDAK memiliki script penanda, abaikan!
            if (importantItem == null) return;

            // Jika lolos seleksi, ambil objeknya
            currentHeldItem = hit.collider.gameObject;

            itemCollider = currentHeldItem.GetComponent<Collider>();
            if (itemCollider != null) itemCollider.enabled = false;

            itemRigidbody = currentHeldItem.GetComponent<Rigidbody>();
            if (itemRigidbody != null) itemRigidbody.isKinematic = true;

            // Ikat ke Hold Point Kamera
            currentHeldItem.transform.SetParent(itemHoldPoint);

            // --- PENGATURAN INSTAN ATAU HALUS ---
            if (importantItem.ambilInstan)
            {
                // Langsung teleportasi instan ke posisi kamera detik itu juga!
                currentHeldItem.transform.localPosition = Vector3.zero;
                currentHeldItem.transform.localRotation = Quaternion.identity;
                shouldLerp = false;
            }
            else
            {
                shouldLerp = true; // Biarkan meluncur halus jika opsi instan dimatikan
            }
        }
    }

    public void DropItem()
    {
        if (currentHeldItem == null) return;

        currentHeldItem.transform.SetParent(null);

        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
            Transform cameraTransform = FIKIFOWFPS1_FirstPersonEngine.Instance.cameraHolder;
            if (cameraTransform != null)
            {
                itemRigidbody.AddForce(cameraTransform.forward * 2f, ForceMode.Impulse);
            }
        }

        if (itemCollider != null) itemCollider.enabled = true;

        currentHeldItem = null;
        itemCollider = null;
        itemRigidbody = null;
    }
}