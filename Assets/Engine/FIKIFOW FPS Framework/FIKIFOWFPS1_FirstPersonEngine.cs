using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("FIKIFOW FPS/1 - First Person Engine")]
[RequireComponent(typeof(CharacterController))]
public class FIKIFOWFPS1_FirstPersonEngine : MonoBehaviour
{
    [Header("References")]
    public Transform cameraHolder; 
    public Transform headBone; 
    
    [Header("Procedural Animation")]
    [Tooltip("Masukkan tulang dada (misal: spine_03.x) agar badan bawah ikut menekuk")]
    public Transform spineBone; 

    [Header("Movement Settings")]
    public float walkSpeed = 5f; 
    [Tooltip("Kecepatan saat menekan tombol Sprint")]
    public float sprintSpeed = 8f; 
    public float gravity = -9.81f; 
    
    [Header("Look Settings")]
    public float mouseSensitivity = 15f; 
    [Tooltip("Batas minimum rotasi X kamera (melihat ke atas/bawah).")]
    [Range(-90f, 0f)]
    public float minLookAngle = -80f; 
    [Tooltip("Batas maksimum rotasi X kamera (melihat ke atas/bawah).")]
    [Range(0f, 90f)]
    public float maxLookAngle = 80f;

    [Header("Animator Blending (8-Way)")]
    [Tooltip("Seberapa halus transisi perpindahan antar arah animasi (semakin tinggi semakin instan)")]
    public float animationSmoothTime = 6f; 

    [Header("Camera Bobbing Settings")]
    public bool enableBobbing = true; 
    [Tooltip("Kecepatan dynamic ayunan saat berjalan biasa")]
    public float walkBobSpeed = 12f; 
    [Tooltip("Intensitas/jarak ayunan saat berjalan biasa")]
    public float walkBobAmount = 0.04f; 
    [Tooltip("Kecepatan dynamic ayunan saat lari (sprint)")]
    public float sprintBobSpeed = 16f; 
    [Tooltip("Intensitas/jarak ayunan saat lari (sprint)")]
    public float sprintBobAmount = 0.08f; 
    [Tooltip("Seberapa cepat kamera kembali ke posisi tengah saat berhenti")]
    public float bobReturnSpeed = 10f; 

    [Header("Input Actions")]
    public InputActionReference moveInput; 
    public InputActionReference lookInput; 
    [Tooltip("Input Action untuk mendeteksi tombol Sprint (misal: Left Shift)")]
    public InputActionReference sprintInput; 

    [HideInInspector] public bool isSprinting = false; 

    private CharacterController controller; 
    private Animator animator; 
    private Vector3 velocity; 
    private float xRotation = 0f; 

    private Vector2 currentInputVector; 
    private float bobTimer = 0f; 
    private Vector3 currentBobOffset; 

    void Start()
    {
        controller = GetComponent<CharacterController>(); 
        animator = GetComponent<Animator>(); 
        Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        HandleLook(); 
        HandleMovement(); 
        HandleCameraBobbing(); 
    }

    void LateUpdate()
    {
        if (spineBone != null) 
        {
            spineBone.localRotation = Quaternion.Euler(xRotation, 0f, 0f); 
        }

        if (cameraHolder != null && headBone != null) 
        {
            cameraHolder.position = headBone.position + currentBobOffset; 
        }
    }

    private void HandleLook()
    {
        Vector2 look = lookInput.action.ReadValue<Vector2>(); 
        float mouseX = look.x * mouseSensitivity * Time.deltaTime; 
        float mouseY = look.y * mouseSensitivity * Time.deltaTime; 

        xRotation -= mouseY; 
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle); 

        if (cameraHolder != null) 
        {
            cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f); 
        }
        
        transform.Rotate(Vector3.up * mouseX); 
    }

    private void HandleMovement()
    {
        Vector2 rawInput = moveInput.action.ReadValue<Vector2>(); 

        if (rawInput.magnitude > 1f) 
        {
            rawInput.Normalize(); 
        }

        currentInputVector = Vector2.MoveTowards(currentInputVector, rawInput, animationSmoothTime * Time.deltaTime); 

        Vector3 moveDirection = transform.right * currentInputVector.x + transform.forward * currentInputVector.y; 
        
        float currentSpeed = walkSpeed; 

        if (sprintInput != null && sprintInput.action != null && sprintInput.action.IsPressed() && rawInput.y > 0.1f) 
        {
            currentSpeed = sprintSpeed; 
            isSprinting = true; 
        }
        else
        {
            isSprinting = false; 
        }

        // --- PERBAIKAN: Satukan Vektor Gerakan Mendatar & Gravitasi Vertikal ---
        Vector3 finalMotion = moveDirection * currentSpeed; 

        if (controller.isGrounded && velocity.y < 0) 
        {
            velocity.y = -2f; 
        }
        velocity.y += gravity * Time.deltaTime; 
        
        // Satukan posisi Y ke dalam vektor finalMotion
        finalMotion.y = velocity.y; 

        // Cukup panggil Move SATU KALI agar internal velocity Unity terbaca dengan benar
        controller.Move(finalMotion * Time.deltaTime); 

        if (animator != null) 
        {
            animator.SetFloat("MoveX", currentInputVector.x); 
            animator.SetFloat("MoveY", currentInputVector.y); 
        }
    }

    private void HandleCameraBobbing()
    {
        if (!enableBobbing) 
        {
            currentBobOffset = Vector3.Lerp(currentBobOffset, Vector3.zero, Time.deltaTime * bobReturnSpeed); 
            return; 
        }

        // Menggunakan logika horizontal velocity agar lebih akurat
        Vector3 horizontalVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        bool isMoving = controller.isGrounded && horizontalVel.magnitude > 0.1f; 

        Vector3 targetBobOffset = Vector3.zero; 

        if (isMoving) 
        {
            float speed = isSprinting ? sprintBobSpeed : walkBobSpeed; 
            float amount = isSprinting ? sprintBobAmount : walkBobAmount; 

            bobTimer += Time.deltaTime * speed; 

            float bobY = Mathf.Sin(bobTimer) * amount; 
            float bobX = Mathf.Cos(bobTimer * 0.5f) * (amount * 0.6f); 

            targetBobOffset = new Vector3(bobX, bobY, 0f); 
        }
        else
        {
            bobTimer = 0f; 
            targetBobOffset = Vector3.zero; 
        }

        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * bobReturnSpeed); 
    }

    void OnEnable() 
    { 
        if (moveInput != null) moveInput.action.Enable();  
        if (lookInput != null) lookInput.action.Enable();  
        if (sprintInput != null && sprintInput.action != null) sprintInput.action.Enable();  
    } 

    void OnDisable() 
    { 
        if (moveInput != null) moveInput.action.Disable(); 
        if (lookInput != null) lookInput.action.Disable(); 
        if (sprintInput != null && sprintInput.action != null) sprintInput.action.Disable(); 
    }
}