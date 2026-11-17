using UnityEngine;
using UnityEngine.Events;

// ObjekInteraktif.cs
// Script dasar untuk SEMUA benda yang bisa disentuh pemain dengan tombol E.
//
// (Ini script pembantu. EvidenceCard.cs dan PuzzleAnchor.cs mewarisi
// script ini, dan benda biasa seperti laptop/kasur/ponsel juga memakainya
// lewat tombol UnityEvent OnInteract.)
public class ObjekInteraktif : MonoBehaviour
{
    [Header("Tampilan")]
    [Tooltip("Nama yang muncul di prompt, contoh: 'Ambil Kartu Bukti'")]
    public string namaTampilan = "Benda";

    [Tooltip("Mesh yang akan disembunyikan setelah benda ini diambil. Boleh kosong.")]
    public GameObject objekVisual;

    [Header("Ambil sebagai benda (dipakai untuk puzzle Retakan Ruang)")]
    [Tooltip("Centang kalau benda ini bisa diambil dan dibawa ke jangkar puzzle.")]
    public bool ambilSebagaiBenda = false;

    [Tooltip("Nama benda yang tersimpan di tas. Contoh: 'KartuProyek'")]
    public string namaBenda = "";

    [Tooltip("Centang kalau mesh harus hilang begitu benda diambil.")]
    public bool sembunyiSetelahDiambil = true;

    [Header("Aksi tambahan (opsional)")]
    [Tooltip("Event yang dipanggil setelah interaksi berhasil. Isi lewat Inspector "
           + "(misal: buka panel laptop, geser pintu, dan sebagainya).")]
    public UnityEvent onInteract = new UnityEvent();

    // True kalau benda ini sudah diambil / sudah dipakai.
    public bool sudahDigunakan = false;

    // Dipanggil FirstPersonController saat pemain menekan E di dekat benda ini.
    public virtual void Interact()
    {
        // Kalau benda ini harus diambil lebih dulu, ambil sekarang.
        if (ambilSebagaiBenda)
        {
            GameState gs = GameState.Instance;
            if (gs != null && gs.inventory != null)
            {
                if (!gs.inventory.AmbilBenda(namaBenda))
                {
                    gs.KirimNotifikasi("Tas Raka sudah berisi satu benda.");
                    return;
                }
            }
        }

        if (sembunyiSetelahDiambil && objekVisual != null)
        {
            objekVisual.SetActive(false);
        }

        if (onInteract != null) onInteract.Invoke();
    }

    // Teks prompt yang tampil di layar. Kosong kalau benda tak boleh disentuh.
    public virtual string TeksPrompt()
    {
        if (sudahDigunakan) return "";
        return "[E] " + namaTampilan;
    }

    // True kalau pemain boleh berinteraksi sekarang.
    public virtual bool BisaDiInteraksi()
    {
        return !sudahDigunakan;
    }

    // Matikan semua Collider di GameObject ini dan anak-anaknya.
    // Dipakai setelah benda diambil, supaya raycast tidak terus
    // menemukan benda yang sudah hilang.
    protected void MatikanCollider()
    {
        Collider[] semua = GetComponentsInChildren<Collider>();
        for (int i = 0; i < semua.Length; i++)
        {
            semua[i].enabled = false;
        }
    }
}

/* ============================================================
   [CARA PASANG] ObjekInteraktif.cs
   1. Drag script ini ke GameObject benda yang bisa disentuh.
   2. Isi kolom:
      - Nama Tampilan      : teks yang muncul di prompt, mis. "Buka Laptop"
      - Objek Visual       : mesh benda ini (biar bisa disembunyikan)
      - Ambil Sebagai Benda: centang HANYA untuk benda puzzle
      - Nama Benda         : mis. "Kunci07" (harus sama persis dengan
                              nama yang diminta PuzzleAnchor)
      - Sembunyi Setelah Diambil : centang untuk kartu bukti
      - On Interact        : drag objek UI / method lain ke sini kalau
                              perlu menjalankan sesuatu (mis. buka panel)
   3. WAJIB ada Collider di GameObject ini (atau di child-nya) dan
      centang "Is Trigger" kalau benda ini cuma pemicu.
   4. Jangan lupa GameObject-nya punya Rigidbody kalau pakai trigger.
   ============================================================ */
