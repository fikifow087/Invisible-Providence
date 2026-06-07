using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using Unity.Profiling;
using System;
using System.Diagnostics;
using System.Text;

public class KIRISA_ProfilerBarbones : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Masukkan komponen TextMeshProUGUI untuk menampilkan stats")]
    public TextMeshProUGUI statsText;

    [Header("Inspector Features")]
    [Tooltip("Centang untuk menghitung dalam KB. Hapus centang untuk MB.")]
    public bool useKB = false;
    
    [Tooltip("Seberapa sering UI di-update (detik).")]
    public float updateInterval = 0.5f;

    // State Variables
    private bool isOverlayActive = true;
    private float timer;
    private int frameCount;
    private float dt;
    private float currentFps;

    // Profilers
    private ProfilerRecorder gpuRecorder;
    private Process currentProcess;
    private DateTime lastCpuCheckTime;
    private TimeSpan lastCpuTotalTime;
    private double cpuPercentage;

    // [OPTIMASI 1] Gunakan StringBuilder. Menulis teks di sini tidak memicu GC Alloc
    private StringBuilder sb = new StringBuilder(150);

    void Start()
    {
        // Setup GPU Frame Time Recorder
        gpuRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
        
        // Setup CPU Process Checker
        try 
        {
            currentProcess = Process.GetCurrentProcess();
            lastCpuCheckTime = DateTime.UtcNow;
            lastCpuTotalTime = currentProcess.TotalProcessorTime;
        } 
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("KIRISA_Profiler: Tidak bisa membaca Process OS. " + e.Message);
        }
    }

    void OnDisable()
    {
        if (gpuRecorder.IsRunning) 
            gpuRecorder.Dispose();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            isOverlayActive = !isOverlayActive;
            statsText.enabled = isOverlayActive;
        }

        if (!isOverlayActive) return;

        frameCount++;
        dt += Time.unscaledDeltaTime;
        if (dt >= updateInterval)
        {
            currentFps = frameCount / dt;
            frameCount = 0;
            dt -= updateInterval;
            
            UpdateStatsUI();
        }
    }

    void UpdateStatsUI()
    {
        long ramBytes = Profiler.GetTotalAllocatedMemoryLong();
        long vramBytes = Profiler.GetAllocatedMemoryForGraphicsDriver();
        
        double ram = useKB ? ramBytes / 1024.0 : ramBytes / 1048576.0;
        double vram = useKB ? vramBytes / 1024.0 : vramBytes / 1048576.0;
        string unit = useKB ? "KB" : "MB";

        if (currentProcess != null)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan currentTotalTime = currentProcess.TotalProcessorTime;
            double timePassedMs = (now - lastCpuCheckTime).TotalMilliseconds;
            double cpuMsPassed = (currentTotalTime - lastCpuTotalTime).TotalMilliseconds;
            
            if (timePassedMs > 0) 
            {
                cpuPercentage = (cpuMsPassed / (Environment.ProcessorCount * timePassedMs)) * 100.0;
            }
            
            lastCpuCheckTime = now;
            lastCpuTotalTime = currentTotalTime;
        }

        double gpuMs = gpuRecorder.CurrentValue / 1000000.0;

        // [OPTIMASI 2] Susun teks menggunakan metode Append. 
        // Ini menghindari pembuatan objek string "+" yang boros memori.
        sb.Clear();
        sb.Append("CPU : ").Append(cpuPercentage.ToString("0.0")).Append("%\n");
        sb.Append("GPU: ").Append(gpuMs.ToString("0.00")).Append(" ms\n");
        sb.Append("RAM : ").Append(ram.ToString("0.0")).Append(" ").Append(unit).Append("\n");
        sb.Append("VRAM: ").Append(vram.ToString("0.0")).Append(" ").Append(unit).Append("\n");
        sb.Append("FPS : ").Append(Mathf.RoundToInt(currentFps));

        // [OPTIMASI 3] TextMeshPro punya fitur canggih untuk membaca StringBuilder 
        // secara langsung tanpa dikonversi menjadi .ToString()
        statsText.SetText(sb);
    }
}