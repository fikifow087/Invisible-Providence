using UnityEngine;

[AddComponentMenu("FIKIFOW FPS/5b - Important Item Marker")]
public class FIKIFOW_ImportantItem : MonoBehaviour
{
    [Header("Item Info")]
    public string namaItem = "Kunci Gudang";
    
    [Tooltip("Jika dicentang, objek langsung menempel instan ke kamera saat diambil")]
    public bool ambilInstan = true;
}