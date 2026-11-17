using System;
using UnityEngine;
using UnityEngine.InputSystem;

// FirstPersonController.cs
// Menggerakkan pemain orang pertama: jalan WASD, menoleh dengan mouse,
// dan berinteraksi dengan benda di dekatnya memakai tombol E.
//
// CATATAN PENTING:
// Project ini memakai Input System baru (UnityEngine.InputSystem), BUKAN
// Input.GetAxis yang lama. Jadi semua penekanan tombol dibaca lewat
// Keyboard.current dan Mouse.current. Selalu dicek null dulu, karena
// di kondisi tertentu (mis. sedang compile) device-nya bisa null.
public class FirstPersonController : MonoBehaviour
{
    [Header("Gerak")]
    public float kecepatanJalan = 2.2f;   // meter per detik
    public float tinggiKamera = 1.6f;    // tinggi kamera dari lantai (meter)
    public float fov = 70f;              // sudut pandang kamera

    [Header("Interaksi")]
    public float jarakInteraksi = 2f;    // jarak maksimum untuk tombol E

    [Header("Status")]
    [Tooltip("Saat true, pemain tidak bisa jalan dan mouse dilepas. Diatur UIManager.")]
    public bool uiTerbuka = false;

    public static FirstPersonController Instance { get; private set; }

    // Prompt yang berubah tiap kali pemain melirik benda baru.
    public event Action<string> OnPromptBerubah;

    private CharacterController karakter;
    private Transform kepalaCamera;
    private float sudutPuncak = 0f;      // pitch (atas/bawah)
    private string promptTerakhir = "";
    private bool statusUiTerakhir = false;

    void Awake()
    {
        Instance = this;

        karakter = GetComponent<CharacterController>();
        if (karakter == null)
        {
            Debug.LogWarning("[FirstPersonController] GameObject ini belum punya CharacterController!");
        }

        // Cari kamera sebagai child GameObject.
        kepalaCamera = transform.Find("Camera");
        if (kepalaCamera == null)
        {
            Debug.LogWarning("[FirstPersonController] Tidak ada child bernama 'Camera'. "
                           + "Buat GameObject anak bernama Camera dan taruh Camera di dalamnya.");
        }
        else
        {
            kepalaCamera.localPosition = new Vector3(0f, tinggiKamera, 0f);
            Camera kamera = kepalaCamera.GetComponent<Camera>();
            if (kamera != null) kamera.fieldOfView = fov;
        }
    }

    void Start()
    {
        SetCursor(true);
    }

    void Update()
    {
        // Kalau ada panel UI terbuka: cursor dilepas, tombol mouse kiri
        // diklik untuk mengunci cursor lagi.
        if (uiTerbuka != statusUiTerakhir)
        {
            statusUiTerakhir = uiTerbuka;
            SetCursor(!uiTerbuka);
        }

        // Hitung arah jalan (hanya kalau tidak ada UI terbuka).
        Vector3 arah = Vector3.zero;
        if (!uiTerbuka)
        {
            arah = HitungArah();
        }

        // CharacterController.Move HARUS dipanggil setiap frame, termasuk
        // dengan vektor nol, supaya pemain tetap menempel di lantai.
        karakter.Move(arah * kecepatanJalan * Time.deltaTime);

        if (!uiTerbuka)
        {
            Lihat();
            CekInteraksi();
        }
        else
        {
            KosongkanPrompt();
        }
    }

    // Ayunan kepala sengaja tidak dipakai. Kamera diam, biar pemain
    // yang sensitif terhadap gerak nggak migrain.

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------------------------------------------------------------
    // Jalan WASD
    // ---------------------------------------------------------------

    // Baca tombol WASD lalu ubah jadi arahmovement relatif arah pandang.
    // ClampMagnitude dipakai supaya jalan diagonal tidak lebih cepat.
    private Vector3 HitungArah()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector3.zero;

        float x = 0f;
        float z = 0f;

        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed) x -= 1f;

        Vector3 arah = transform.right * x + transform.forward * z;
        return Vector3.ClampMagnitude(arah, 1f);
    }

    // ---------------------------------------------------------------
    // Menoleh dengan mouse
    // ---------------------------------------------------------------

    private void Lihat()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || kepalaCamera == null) return;

        Vector2 delta = mouse.delta.ReadValue();
        if (delta.sqrMagnitude < 0.000001f) return;

        float skala = PengaturanGame.sensitivitasMouse * 0.02f;

        // Putar badan (yaw) ke kiri-kanan.
        transform.Rotate(0f, delta.x * skala, 0f);

        // Miring kepala (pitch) atas-bawah, dibatasi supaya tidak terbalik.
        float vertikal = delta.y * skala;
        if (PengaturanGame.invertY) vertikal = -vertikal;

        sudutPuncak = Mathf.Clamp(sudutPuncak - vertikal, -85f, 85f);
        kepalaCamera.localRotation = Quaternion.Euler(sudutPuncak, 0f, 0f);
    }

    // ---------------------------------------------------------------
    // Interaksi tombol E
    // ---------------------------------------------------------------

    private void CekInteraksi()
    {
        if (kepalaCamera == null) return;

        Ray ray = new Ray(kepalaCamera.position, kepalaCamera.forward);
        RaycastHit hit;
        ObjekInteraktif objek = null;

        if (Physics.Raycast(ray, out hit, jarakInteraksi))
        {
            // GetComponentInParent supaya benda di child tetap terdeteksi.
            objek = hit.collider.GetComponentInParent<ObjekInteraktif>();
        }

        // Perbarui prompt yang tampil di layar.
        string prompt = "";
        if (objek != null && objek.BisaDiInteraksi())
        {
            prompt = objek.TeksPrompt();
        }

        if (prompt != promptTerakhir)
        {
            promptTerakhir = prompt;
            if (OnPromptBerubah != null) OnPromptBerubah(prompt);
        }

        // Tekan E untuk berinteraksi.
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame && objek != null && objek.BisaDiInteraksi())
        {
            objek.Interact();
        }
    }

    private void KosongkanPrompt()
    {
        if (promptTerakhir == "") return;
        promptTerakhir = "";
        if (OnPromptBerubah != null) OnPromptBerubah("");
    }

    // ---------------------------------------------------------------
    // Cursor
    // ---------------------------------------------------------------

    // Kunci atau lepaskan cursor mouse.
    // Dipakai UIManager, jadi UIManager tidak perlu menyentuh Cursor langsung.
    public void SetCursor(bool terkunci)
    {
        if (terkunci)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

/* ============================================================
   [CARA PASANG] FirstPersonController.cs
   1. Buat GameObject "Pemain" di scene. Jadikan posisi awal pemain.
   2. Tambahkan komponen "CharacterController" ke GameObject "Pemain".
      Isi begini:
        Height    = 1.8
        Center    = (0, 0.9, 0)
        Radius    = 0.28
        Slope Limit = 45
        Step Offset = 0.3
   3. Drag script FirstPersonController.cs ke GameObject "Pemain".
   4. Buat GameObject anak (drag ke dalam "Pemain", lalu klik kanan
      "Create Empty > Children > Camera"):
        - GameObject anak ini WAJIB diberi nama persis "Camera"
        - Tambahkan komponen "Camera"
        - Tambahkan "Audio Listener"
      UIManager/FirstPersonController otomatis mengaturnya:
        FOV 70 dan tinggi 1.6 meter dari lantai.
   5. Centang tag "MainCamera" pada kamera anak.
   6. Biarkan setelan "Keyboard/Mouse" pada Mouse Look apa adanya (script ini
      membaca device secara langsung, jadi tidak perlu pengaturan asset input).
   7. Isi kolom Inspector sesuai selera:
        Kecepatan Jalan  = 2.2  (meter/detik)
        Tinggi Kamera    = 1.6
        FOV              = 70
        Jarak Interaksi  = 2 (meter)
      Sensitivitas mouse, invert Y, dan kurangi gerak diatur dari menu
      Pengaturan di dalam game (lihat PengaturanGame.cs).
   8. Semua benda yang bisa disentuh WAJIB punya Collider, dan kalau
      bendanya ada di dalam GameObject lain, pakai
      GetComponentInParent (sudah ditangani script ini).
   9. UIManager akan mengubah kolom "UI Terbuka" menjadi true saat panel
      terbuka. Jangan diubah manual dari script lain.
   ============================================================ */
