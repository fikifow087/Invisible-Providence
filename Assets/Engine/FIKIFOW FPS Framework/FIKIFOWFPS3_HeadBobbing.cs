using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("FIKIFOW FPS/ - Head Bobbing")]
public class FIKIFOWFPS3_HeadBobbing : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform yang akan diberi efek head bobbing. Jangan isi dengan Camera jika senjata attach ke Camera.")]
    public Transform bobTarget;
    [Tooltip("Optional: gunakan CharacterController untuk mendeteksi gerakan berjalan secara otomatis.")]
    public CharacterController attachedController;
    [Tooltip("Optional: referensi ke FIKIFOWFPS1_FirstPersonEngine jika ingin mengambil status sprint dan input yang sama.")]
    public FIKIFOWFPS1_FirstPersonEngine fpsEngine;

    [Header("Input")]
    public InputActionReference moveInput;
    public InputActionReference sprintInput;

    [Header("Bobbing Settings")]
    public bool enableBobbing = true;
    public float walkBobSpeed = 10f;
    public float walkBobAmount = 0.02f;
    public float sprintBobSpeed = 14f;
    public float sprintBobAmount = 0.04f;
    public float walkFootstepDepth = 0.03f;
    public float sprintFootstepDepth = 0.06f;
    public float bobReturnSpeed = 8f;
    public float bobRotationAmount = 1.5f;

    [Header("Stabilization")]
    [Tooltip("Jika aktif, transform akan kembali ke posisi awal secara halus saat berhenti.")]
    public bool stabilizeWhenIdle = true;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float bobTimer;
    private bool previousEnabledState;

    void Awake()
    {
        if (bobTarget == null)
        {
            bobTarget = transform;
        }

        initialLocalPosition = bobTarget.localPosition;
        initialLocalRotation = bobTarget.localRotation;

        if (attachedController == null)
        {
            attachedController = GetComponent<CharacterController>();
        }
    }

    void OnEnable()
    {
        if (moveInput != null && moveInput.action != null)
        {
            moveInput.action.Enable();
        }

        if (sprintInput != null && sprintInput.action != null)
        {
            sprintInput.action.Enable();
        }

        previousEnabledState = enableBobbing;
    }

    void OnDisable()
    {
        if (moveInput != null && moveInput.action != null)
        {
            moveInput.action.Disable();
        }

        if (sprintInput != null && sprintInput.action != null)
        {
            sprintInput.action.Disable();
        }

        ResetTargetTransform();
    }

    void LateUpdate()
    {
        if (bobTarget == null)
        {
            return;
        }

        if (!enableBobbing)
        {
            if (stabilizeWhenIdle)
            {
                bobTarget.localPosition = Vector3.Lerp(bobTarget.localPosition, initialLocalPosition, Time.deltaTime * bobReturnSpeed);
                bobTarget.localRotation = Quaternion.Slerp(bobTarget.localRotation, initialLocalRotation, Time.deltaTime * bobReturnSpeed);
            }
            return;
        }

        bool isMoving;
        bool isSprinting;
        float moveMagnitude = 0f;

        if (fpsEngine != null)
        {
            moveMagnitude = fpsEngine.moveInput != null && fpsEngine.moveInput.action != null ? fpsEngine.moveInput.action.ReadValue<Vector2>().magnitude : 0f;
            isSprinting = fpsEngine.sprintInput != null && fpsEngine.sprintInput.action != null && fpsEngine.sprintInput.action.IsPressed();
            isMoving = moveMagnitude > 0.1f;
        }
        else if (moveInput != null && moveInput.action != null)
        {
            moveMagnitude = moveInput.action.ReadValue<Vector2>().magnitude;
            isSprinting = sprintInput != null && sprintInput.action != null && sprintInput.action.IsPressed();
            isMoving = moveMagnitude > 0.1f;
        }
        else if (attachedController != null)
        {
            moveMagnitude = attachedController.velocity.magnitude;
            isSprinting = false;
            isMoving = attachedController.isGrounded && moveMagnitude > 0.1f;
        }
        else
        {
            isMoving = false;
            isSprinting = false;
        }

        if (isMoving)
        {
            float speed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            bobTimer += Time.deltaTime * speed;
        }
        else
        {
            bobTimer = 0f;
        }

        float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;
        float footDepth = isSprinting ? sprintFootstepDepth : walkFootstepDepth;

        Vector3 bobOffset = Vector3.zero;
        Quaternion bobRotation = Quaternion.identity;

        if (isMoving)
        {
            float bobY = Mathf.Sin(bobTimer * 2f) * bobAmount;
            float bobX = Mathf.Cos(bobTimer) * bobAmount * 0.6f;
            float bobZ = Mathf.Cos(bobTimer * 2f) * footDepth * 0.2f;
            float roll = Mathf.Sin(bobTimer) * bobRotationAmount * bobAmount;

            bobOffset = new Vector3(bobX, bobY, bobZ);
            bobRotation = Quaternion.Euler(0f, 0f, roll);
        }

        Vector3 targetPos = initialLocalPosition + bobOffset;
        Quaternion targetRot = initialLocalRotation * bobRotation;

        bobTarget.localPosition = Vector3.Lerp(bobTarget.localPosition, targetPos, Time.deltaTime * bobReturnSpeed);
        bobTarget.localRotation = Quaternion.Slerp(bobTarget.localRotation, targetRot, Time.deltaTime * bobReturnSpeed);
    }

    private void ResetTargetTransform()
    {
        if (bobTarget == null) return;

        bobTarget.localPosition = initialLocalPosition;
        bobTarget.localRotation = initialLocalRotation;
    }
}
