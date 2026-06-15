using UnityEngine;

public class ReadMode: MonoBehaviour
{
    public void ReadMode_ON()
    {
        // Matikan total pergerakan dan kamera player
        FIKIFOWFPS1_FirstPersonEngine.Instance.BlockInput();
        
        // Tampilkan UI Dokumen dan munculkan kursor mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    public void ReadMode_OFF()
    {
        // Sembunyikan kursor kembali
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Kembalikan kontrol ke player
        FIKIFOWFPS1_FirstPersonEngine.Instance.UnblockInput();
    }
}

