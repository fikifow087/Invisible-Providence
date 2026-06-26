using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("FIKIFOW FPS/Camera Rotation Focus")]
public class FIKIFOW_CameraRotationFocus : MonoBehaviour
{
    public enum InterpolationMode { Constant, Linear, EaseIn, EaseOut }

    [Header("Focus Target Settings")]
    [Tooltip("Target Game Object Empty tempat kamera harus menghadap")]
    public Transform focusTarget;
    [Tooltip("Durasi pergerakan kamera menuju target (detik)")]
    public float transitionDuration = 1.0f;
    [Tooltip("Tipe pergerakan kurva animasi rotasi")]
    public InterpolationMode interpolationType = InterpolationMode.Linear;

    [Header("Input Blocking Settings")]
    [Tooltip("Jika dicentang, akan memblokir pergerakan & pandangan player FPS")]
    public bool blockInputFPS = true;
    
    [Tooltip("0 = Infinite (sampai di-unblock manual lewat script dialog).\n1 = Terblokir selama 1 detik saja lalu otomatis lepas.")]
    public float holdDuration = 0f;

    private Coroutine focusCoroutine;
    private Coroutine unblockCoroutine;

    /// <summary>
    /// Menjalankan animasi fokus menggunakan pengaturan bawaan yang ada di Inspector.
    /// Cocok dipanggil dari Event Unity atau fungsi dialog sederhana.
    /// </summary>
    public void StartFocus()
    {
        if (focusTarget == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Focus Target belum dimasukkan di Inspector!");
            return;
        }
        TriggerFocus(focusTarget, transitionDuration, interpolationType, blockInputFPS, holdDuration);
    }

    /// <summary>
    /// Fungsi Super Scalable: Bisa dipanggil dinamis lewat script flow dialog dengan parameter kustom.
    /// Contoh: cameraFocus.TriggerFocus(NPC_Astra, 1.5f, InterpolationMode.EaseOut, true, 0f);
    /// </summary>
    public void TriggerFocus(Transform target, float duration, InterpolationMode mode, bool blockInput, float blockTime)
    {
        if (FIKIFOWFPS1_FirstPersonEngine.Instance == null) return;

        // Hentikan proses fokus atau unblock sebelumnya jika sedang berjalan agar tidak tabrakan
        if (focusCoroutine != null) StopCoroutine(focusCoroutine);
        if (unblockCoroutine != null) StopCoroutine(unblockCoroutine);

        focusCoroutine = StartCoroutine(FocusRoutine(target, duration, mode, blockInput, blockTime));
    }

    private IEnumerator FocusRoutine(Transform target, float duration, InterpolationMode mode, bool blockInput, float blockTime)
    {
        FIKIFOWFPS1_FirstPersonEngine fpsEngine = FIKIFOWFPS1_FirstPersonEngine.Instance;
        Transform playerTransform = fpsEngine.transform;
        Transform cameraHolder = fpsEngine.cameraHolder;

        // 1. Jalankan Sistem Blokir Input FPS
        if (blockInput)
        {
            fpsEngine.BlockInput();
            
            // Jika holdDuration > 0, jadwalkan pelepasan input otomatis otomatis
            if (blockTime > 0f)
            {
                unblockCoroutine = StartCoroutine(AutoUnblockTimeout(blockTime));
            }
        }

        // 2. Ambil Posisi Awal Rotasi
        Quaternion startPlayerRot = playerTransform.rotation;
        Quaternion startCamRot = cameraHolder.localRotation;

        // 3. Hitung Kalkulasi Rotasi Target Akhir
        Vector3 directionToTarget = (target.position - cameraHolder.position).normalized;
        
        // Rotasi Badan (Hanya sumbu Y / Yaw)
        Vector3 lookDirYaw = new Vector3(directionToTarget.x, 0f, directionToTarget.z).normalized;
        Quaternion targetPlayerRot = Quaternion.LookRotation(lookDirYaw);

        // Rotasi Kamera (Sumbu X / Pitch signed angle agar sinkron dengan batasan min/maxLookAngle)
        Vector3 localTargetDir = Quaternion.Inverse(targetPlayerRot) * directionToTarget;
        float targetPitch = Mathf.Atan2(-localTargetDir.y, localTargetDir.z) * Mathf.Rad2Deg;
        targetPitch = Mathf.Clamp(targetPitch, fpsEngine.minLookAngle, fpsEngine.maxLookAngle);
        Quaternion targetCamRot = Quaternion.Euler(targetPitch, 0f, 0f);

        // 4. Jalankan Proses Interpolasi Animasi
        if (mode == InterpolationMode.Constant || duration <= 0f)
        {
            playerTransform.rotation = targetPlayerRot;
            cameraHolder.localRotation = targetCamRot;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Evaluasi Kurva Interpolasi Grafis
                float evaluatedT = t;
                switch (mode)
                {
                    case InterpolationMode.Linear:
                        evaluatedT = t;
                        break;
                    case InterpolationMode.EaseIn:
                        evaluatedT = t * t * t; // Cubic Ease In
                        break;
                    case InterpolationMode.EaseOut:
                        evaluatedT = 1f - Mathf.Pow(1f - t, 3f); // Cubic Ease Out
                        break;
                }

                playerTransform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, evaluatedT);
                cameraHolder.localRotation = Quaternion.Slerp(startCamRot, targetCamRot, evaluatedT);
                yield return null;
            }

            playerTransform.rotation = targetPlayerRot;
            cameraHolder.localRotation = targetCamRot;
        }

        // 5. PENGEREMAN SNAP: Paksa sinkronisasi internal variabel xRotation milik FPS Engine!
        fpsEngine.ResetCameraPitch(targetPitch);
    }

    private IEnumerator AutoUnblockTimeout(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (FIKIFOWFPS1_FirstPersonEngine.Instance != null)
        {
            FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
            Debug.Log("[Focus System] Input FPS otomatis dilepas setelah " + delay + " detik.");
        }
    }
}

// ====================================================================
// CUSTOM INSPECTOR: Membuat UI Tampilan Editor rapi (Hold Duration tersembunyi jika bool uncheck)
// ====================================================================
#if UNITY_EDITOR
[CustomEditor(typeof(FIKIFOW_CameraRotationFocus))]
public class FIKIFOW_CameraRotationFocusEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        FIKIFOW_CameraRotationFocus script = (FIKIFOW_CameraRotationFocus)target;

        EditorGUILayout.LabelField("Focus Target Settings", EditorStyles.boldLabel);
        script.focusTarget = (Transform)EditorGUILayout.ObjectField("Focus Target", script.focusTarget, typeof(Transform), true);
        script.transitionDuration = EditorGUILayout.FloatField("Transition Duration", script.transitionDuration);
        script.interpolationType = (FIKIFOW_CameraRotationFocus.InterpolationMode)EditorGUILayout.EnumPopup("Interpolation Type", script.interpolationType);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input Blocking Settings", EditorStyles.boldLabel);
        script.blockInputFPS = EditorGUILayout.Toggle("Block Input Action FPS Engine", script.blockInputFPS);

        // Opsi Hold Duration HANYA muncul jika "Block Input Action FPS Engine" dicentang true!
        if (script.blockInputFPS)
        {
            EditorGUI.indentLevel++;
            script.holdDuration = EditorGUILayout.FloatField("Hold Duration", script.holdDuration);
            if (script.holdDuration == 0)
            {
                EditorGUILayout.HelpBox("Hold Duration bernilai 0: Kamera terkunci selamanya sampai script dialog memanggil fungsi Unblock manual.", MessageType.Info);
            }
            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif