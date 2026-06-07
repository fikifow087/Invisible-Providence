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
    [Tooltip("Kecepatan ayunan saat berjalan biasa")]
    public float walkBobSpeed = 12f;
    [Tooltip("Intensitas/jarak ayunan saat berjalan biasa")]
    public float walkBobAmount = 0.04f;
    [Tooltip("Kecepatan ayunan saat lari (sprint)")]
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

    private CharacterController controller; 
    private Animator animator; 
    private Vector3 velocity; 
    private float xRotation = 0f; 

    // Variabel penampung untuk menghaluskan perpindahan animasi 8 arah
    private Vector2 currentInputVector; 

    // VARIABEL INTERNAL BOBBING
    private bool isSprinting = false;
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
        // Jalankan rotasi Spine
        if (spineBone != null) 
        {
            spineBone.localRotation = Quaternion.Euler(xRotation, 0f, 0f); 
        }

        // Ikat koordinat posisi kamera ke posisi headBone + tambahan offset efek bobbing
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
        // 1. Baca input mentah dari keyboard / WASD
        Vector2 rawInput = moveInput.action.ReadValue<Vector2>(); 

        // 2. Normalisasi Input Diagonal
        if (rawInput.magnitude > 1f) 
        {
            rawInput.Normalize(); 
        }

        // 3. Input Smoothing / Dampening
        currentInputVector = Vector2.MoveTowards(currentInputVector, rawInput, animationSmoothTime * Time.deltaTime); 

        // 4. Hitung arah gerak berdasarkan rotasi horizontal player
        Vector3 moveDirection = transform.right * currentInputVector.x + transform.forward * currentInputVector.y; 
        
        // --- UPGRADE SPRINT: Penentuan Kecepatan Dinamis ---
        float currentSpeed = walkSpeed;

        // Cek apakah tombol sprint ditekan DAN player sedang bergerak maju (W / input Y positif)
        if (sprintInput != null && sprintInput.action != null && sprintInput.action.IsPressed() && rawInput.y > 0.1f)
        {
            currentSpeed = sprintSpeed;
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
        // ---------------------------------------------------

        // 5. Jalankan pergerakan fisik karakter
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // 6. Logika Gravitasi Sederhana
        if (controller.isGrounded && velocity.y < 0) 
        {
            velocity.y = -2f; 
        }
        velocity.y += gravity * Time.deltaTime; 
        controller.Move(velocity * Time.deltaTime); 

        // 7. Kirim Nilai yang Sudah Halus ke Blend Tree 8 Arah
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

        // Cek apakah player benar-benar bergerak di atas tanah
        bool isMoving = controller.isGrounded && controller.velocity.magnitude > 0.1f;

        Vector3 targetBobOffset = Vector3.zero;

        if (isMoving)
        {
            // Tentukan speed dan bobot berdasarkan status lari/jalan
            float speed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float amount = isSprinting ? sprintBobAmount : walkBobAmount;

            // Timer terus berjalan mengikuti kecepatan langkah kaki
            bobTimer += Time.deltaTime * speed;

            // Rumus matematika pola Figure-8 (Sinus untuk Vertikal, Cosinus setengah frekuensi untuk Horizontal)
            float bobY = Mathf.Sin(bobTimer) * amount;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * (amount * 0.6f); // 0.6f membuat ayunan menyamping tidak terlalu ekstrim

            targetBobOffset = new Vector3(bobX, bobY, 0f);
        }
        else
        {
            // Jika diam atau melompat, reset timer dan target kembali ke nol
            bobTimer = 0f;
            targetBobOffset = Vector3.zero;
        }

        // Haluskan transisi perpindahan posisi bobbing agar tidak patah-patah
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
        if (lookInput != null) moveInput.action.Disable(); 
        if (sprintInput != null && sprintInput.action != null) sprintInput.action.Disable(); 
    }
}