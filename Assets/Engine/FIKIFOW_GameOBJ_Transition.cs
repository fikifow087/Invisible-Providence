using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// FIKIFOW Game Object Transition - Fade In/Fade Out Controller
/// Plugin global reusable untuk fade in dan fade out game object dengan durasi frame-based
/// Mendukung: UI (Image, RawImage, Text, TextMeshPro) dan Renderer objects
/// </summary>
public static class FIKIFOW_GameOBJ_Transition
{
    private const float TARGET_FPS = 60.0f;

    /// <summary>
    /// Fade In coroutine - SetActive(true) kemudian opacity 0 → targetOpacity
    /// </summary>
    /// <param name="targetObject">GameObject yang akan di-fade in</param>
    /// <param name="targetOpacity">Target opacity (0-1), default 1</param>
    /// <param name="durationFrames">Durasi dalam frame (60 frame = 1 detik)</param>
    public static IEnumerator FadeIn(GameObject targetObject, float targetOpacity, int durationFrames)
    {
        if (targetObject == null)
        {
            Debug.LogError("[FIKIFOW_GameOBJ_Transition] Target object is null!");
            yield break;
        }

        // Clamp target opacity antara 0 dan 1
        targetOpacity = Mathf.Clamp01(targetOpacity);

        // SetActive true di awal
        targetObject.SetActive(true);

        Graphic uiGraphic = targetObject.GetComponent<Graphic>();
        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();
        Renderer renderer = targetObject.GetComponent<Renderer>();

        // Tentukan tipe object dan set opacity awal ke 0
        if (uiGraphic != null)
        {
            Color color = uiGraphic.color;
            color.a = 0f;
            uiGraphic.color = color;
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        else if (renderer != null)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = 0f;
            mat.color = color;
        }
        else
        {
            Debug.LogWarning("[FIKIFOW_GameOBJ_Transition] Object tidak memiliki Graphic, CanvasGroup, atau Renderer component!", targetObject);
            yield break;
        }

        // Fade in: opacity 0 → targetOpacity
        float elapsedFrames = 0f;
        while (elapsedFrames < durationFrames)
        {
            elapsedFrames += 1f;
            float progress = Mathf.Lerp(0f, targetOpacity, elapsedFrames / durationFrames);

            if (uiGraphic != null)
            {
                Color color = uiGraphic.color;
                color.a = progress;
                uiGraphic.color = color;
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = progress;
            }
            else if (renderer != null)
            {
                Material mat = renderer.material;
                Color color = mat.color;
                color.a = progress;
                mat.color = color;
            }

            yield return null;
        }

        // Pastikan opacity akhir = targetOpacity
        if (uiGraphic != null)
        {
            Color color = uiGraphic.color;
            color.a = targetOpacity;
            uiGraphic.color = color;
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = targetOpacity;
        }
        else if (renderer != null)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = targetOpacity;
            mat.color = color;
        }
    }

    /// <summary>
    /// Fade In coroutine - SetActive(true) kemudian opacity 0 → 1 (full opaque)
    /// Overload untuk backward compatibility
    /// </summary>
    /// <param name="targetObject">GameObject yang akan di-fade in</param>
    /// <param name="durationFrames">Durasi dalam frame (60 frame = 1 detik)</param>
    public static IEnumerator FadeIn(GameObject targetObject, int durationFrames)
    {
        yield return FadeIn(targetObject, 1f, durationFrames);
    }

    /// <summary>
    /// Fade Out coroutine - opacity 100 → targetOpacity
    /// Jika targetOpacity = 0, maka SetActive(false) di akhir
    /// Jika targetOpacity > 0, object tetap SetActive(true) dengan opacity target
    /// </summary>
    /// <param name="targetObject">GameObject yang akan di-fade out</param>
    /// <param name="targetOpacity">Target opacity (0-1), default 0</param>
    /// <param name="durationFrames">Durasi dalam frame (60 frame = 1 detik)</param>
    public static IEnumerator FadeOut(GameObject targetObject, float targetOpacity, int durationFrames)
    {
        if (targetObject == null)
        {
            Debug.LogError("[FIKIFOW_GameOBJ_Transition] Target object is null!");
            yield break;
        }

        // Clamp target opacity antara 0 dan 1
        targetOpacity = Mathf.Clamp01(targetOpacity);

        Graphic uiGraphic = targetObject.GetComponent<Graphic>();
        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();
        Renderer renderer = targetObject.GetComponent<Renderer>();

        if (uiGraphic == null && canvasGroup == null && renderer == null)
        {
            Debug.LogWarning("[FIKIFOW_GameOBJ_Transition] Object tidak memiliki Graphic, CanvasGroup, atau Renderer component!", targetObject);
            yield break;
        }

        // Fade out: opacity 1 → targetOpacity
        float elapsedFrames = 0f;
        while (elapsedFrames < durationFrames)
        {
            elapsedFrames += 1f;
            float progress = Mathf.Lerp(1f, targetOpacity, elapsedFrames / durationFrames);

            if (uiGraphic != null)
            {
                Color color = uiGraphic.color;
                color.a = progress;
                uiGraphic.color = color;
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = progress;
            }
            else if (renderer != null)
            {
                Material mat = renderer.material;
                Color color = mat.color;
                color.a = progress;
                mat.color = color;
            }

            yield return null;
        }

        // Pastikan opacity akhir = targetOpacity
        if (uiGraphic != null)
        {
            Color color = uiGraphic.color;
            color.a = targetOpacity;
            uiGraphic.color = color;
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = targetOpacity;
        }
        else if (renderer != null)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = targetOpacity;
            mat.color = color;
        }

        // SetActive false HANYA jika targetOpacity = 0
        if (targetOpacity <= 0f)
        {
            targetObject.SetActive(false);
        }
    }

    /// <summary>
    /// Fade Out coroutine - opacity 100 → 0 kemudian SetActive(false)
    /// Overload untuk backward compatibility
    /// </summary>
    /// <param name="targetObject">GameObject yang akan di-fade out</param>
    /// <param name="durationFrames">Durasi dalam frame (60 frame = 1 detik)</param>
    public static IEnumerator FadeOut(GameObject targetObject, int durationFrames)
    {
        yield return FadeOut(targetObject, 0f, durationFrames);
    }

    /// <summary>
    /// Fade In dengan durasi dalam detik dan target opacity
    /// </summary>
    public static IEnumerator FadeInSeconds(GameObject targetObject, float targetOpacity, float durationSeconds)
    {
        int durationFrames = Mathf.RoundToInt(durationSeconds * TARGET_FPS);
        yield return FadeIn(targetObject, targetOpacity, durationFrames);
    }

    /// <summary>
    /// Fade In dengan durasi dalam detik (opacity full 100%)
    /// </summary>
    public static IEnumerator FadeInSeconds(GameObject targetObject, float durationSeconds)
    {
        int durationFrames = Mathf.RoundToInt(durationSeconds * TARGET_FPS);
        yield return FadeIn(targetObject, 1f, durationFrames);
    }

    /// <summary>
    /// Fade Out dengan durasi dalam detik dan target opacity
    /// </summary>
    public static IEnumerator FadeOutSeconds(GameObject targetObject, float targetOpacity, float durationSeconds)
    {
        int durationFrames = Mathf.RoundToInt(durationSeconds * TARGET_FPS);
        yield return FadeOut(targetObject, targetOpacity, durationFrames);
    }

    /// <summary>
    /// Fade Out dengan durasi dalam detik (opacity 0%)
    /// </summary>
    public static IEnumerator FadeOutSeconds(GameObject targetObject, float durationSeconds)
    {
        int durationFrames = Mathf.RoundToInt(durationSeconds * TARGET_FPS);
        yield return FadeOut(targetObject, 0f, durationFrames);
    }
}
