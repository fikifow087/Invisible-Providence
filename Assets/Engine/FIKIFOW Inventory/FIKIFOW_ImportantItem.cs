using UnityEngine;

public enum ItemCategory { UNKNOWN, KEY, DOCUMENT }

[AddComponentMenu("FIKIFOW FPS/5b - Important Item Marker Fixed")]
public class FIKIFOW_ImportantItem : MonoBehaviour
{
    [Header("Item Info")]
    public string namaItem = "Kunci Gudang";
    public ItemCategory kategori = ItemCategory.UNKNOWN;
    
    [Tooltip("Jika dicentang, objek langsung menempel instan ke kamera saat diambil")]
    public bool ambilInstan = true;

    [TextArea(2, 5)]
    public string deskripsiItem = "Sebuah objek misterius yang ditemukan di lantai.";
}