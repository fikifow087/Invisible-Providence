using UnityEngine;

public class EVT_AdaSuaraDiruangan : MonoBehaviour
{
    public EVT_NontonTV CONNECT_EVT_NontonTV;
    [SerializeField] private AudioSource SFXS_suaraDiruangan1;
    [SerializeField] private AudioSource SFXS_suaraDiruangan2;
    
    // Hubungkan komponen script focus yang terpasang di scene
    public FIKIFOW_CameraRotationFocus cameraRotFocus; 
    
    // Titik kosong (Empty GameObject) letak mangkok bakso misterius atau wajah NPC
    public Transform titikSuaraMisterius1;
    public Transform titikSuaraMisterius2;

    public GameObject COL_suaraDiruangan1;
    public GameObject COL_suaraDiruangan2;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //CONNECT_EVT_NontonTV.REQ_EventTV_completion = false;
    }

    public void COLLISION_SuaraDiruangan1()
    {
        cameraRotFocus.TriggerFocus(titikSuaraMisterius2, 0.5f, FIKIFOW_CameraRotationFocus.InterpolationMode.EaseOut, true, 2f);
        SFXS_suaraDiruangan2.Play();
    }

    public void COLLISION_SuaraDiruangan2()
    {
        cameraRotFocus.TriggerFocus(titikSuaraMisterius1, 0.5f, FIKIFOW_CameraRotationFocus.InterpolationMode.EaseOut, true, 2f);
        SFXS_suaraDiruangan1.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
