using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("FIKIFOW FPS/Animation Preview")]
[RequireComponent(typeof(Animator))]
[ExecuteInEditMode]
public class FIKIFOWFPS_AnimationPreview : MonoBehaviour
{
    [HideInInspector] public AnimationClip animationToPreview;
    [HideInInspector] public AnimationClip defaultPose;
    [HideInInspector] public bool loopPreview;

    // Variabel internal untuk sistem Playable Graph di Editor
    [HideInInspector] public bool isPlaying;
    private PlayableGraph graph;
    private double lastTime;
    private float currentTime;

#if UNITY_EDITOR
    void OnEnable()
    {
        // Mendaftarkan fungsi update ke dalam sistem Unity Editor
        EditorApplication.update += EditorUpdate;
    }

    void OnDisable()
    {
        // Membersihkan sistem saat komponen dimatikan
        EditorApplication.update -= EditorUpdate;
        StopPreview();
    }

    void EditorUpdate()
    {
        if (!isPlaying || animationToPreview == null || Application.isPlaying) return;

        double timeSinceStartup = EditorApplication.timeSinceStartup;
        float deltaTime = (float)(timeSinceStartup - lastTime);
        lastTime = timeSinceStartup;

        currentTime += deltaTime;

        // Logika Looping
        if (currentTime > animationToPreview.length)
        {
            if (loopPreview) 
            {
                currentTime %= animationToPreview.length;
            }
            else
            {
                StopPreview();
                return;
            }
        }

        // Mengevaluasi grafik animasi secara manual tanpa Play Mode
        if (graph.IsValid())
        {
            graph.Evaluate(deltaTime);
            SceneView.RepaintAll(); // Memaksa Scene View untuk update pergerakan
        }
    }

    public void StartPreview()
    {
        if (animationToPreview == null)
        {
            Debug.LogWarning("[FIKIFOW FPS] Masukkan Animation Clip terlebih dahulu!");
            return;
        }

        StopPreview(); // Bersihkan grafik sebelumnya jika ada

        Animator anim = GetComponent<Animator>();
        
        // Membuat Playable Graph modern bawaan Unity untuk Humanoid
        graph = PlayableGraph.Create("FIKIFOW_PreviewGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.Manual); // Wajib manual untuk Editor

        var output = AnimationPlayableOutput.Create(graph, "Preview Output", anim);
        var clipPlayable = AnimationClipPlayable.Create(graph, animationToPreview);

        output.SetSourcePlayable(clipPlayable);
        
        // Setup awal waktu jalannya animasi
        isPlaying = true;
        currentTime = 0f;
        lastTime = EditorApplication.timeSinceStartup;
        graph.Evaluate(0);
    }

    public void StopPreview()
    {
        isPlaying = false;
        
        if (graph.IsValid())
        {
            graph.Destroy();
        }

        // Mengembalikan karakter ke pose default (T-Pose / Idle statis) jika disediakan
        if (defaultPose != null)
        {
            defaultPose.SampleAnimation(gameObject, 0f);
        }
    }
#endif
}

// ====================================================================
// BAGIAN INI UNTUK MEMBUAT TAMPILAN UI CUSTOM SEPERTI DI FOTO
// ====================================================================
#if UNITY_EDITOR
[CustomEditor(typeof(FIKIFOWFPS_AnimationPreview))]
public class FIKIFOWFPS_AnimationPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FIKIFOWFPS_AnimationPreview script = (FIKIFOWFPS_AnimationPreview)target;

        // Judul Tebal
        EditorGUILayout.LabelField("Editor Animation Preview", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Input Field (dibuat mirip dengan referensi gambar)
        script.animationToPreview = (AnimationClip)EditorGUILayout.ObjectField("Animation To Preview", script.animationToPreview, typeof(AnimationClip), false);
        script.defaultPose = (AnimationClip)EditorGUILayout.ObjectField("Default Pose", script.defaultPose, typeof(AnimationClip), false);
        script.loopPreview = EditorGUILayout.Toggle("Loop Preview", script.loopPreview);

        GUILayout.Space(10);

        // Mengatur posisi tombol berdampingan (Horizontal)
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Start Preview", GUILayout.Height(25)))
        {
            script.StartPreview();
        }

        if (GUILayout.Button("Stop Preview", GUILayout.Height(25)))
        {
            script.StopPreview();
        }

        GUILayout.EndHorizontal();

        // Menyimpan perubahan UI jika ada yang diganti
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif