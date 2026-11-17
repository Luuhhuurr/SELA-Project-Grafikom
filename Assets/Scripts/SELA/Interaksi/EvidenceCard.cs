using UnityEngine;

/// <summary>
/// EvidenceCard.cs
/// Kartu bukti yang bisa diambil Raka (BAB 3.2).
///
/// Daftar kartu bukti:
///   E0 = Kartu Proyek      (rak di Ruang Terpanjang)
///   E1 = Antarmuka Bersih  (hasil aktivitas "Rapikan Antarmuka")
///   E2 = Catatan Uji Lengkap (hasil aktivitas "Lengkapi Catatan Uji")
///
/// Saat diambil, kartu masuk ke daftar bukti milik GameState, mesh-nya
/// hilang, dan pemain mendapat notifikasi.
/// </summary>
public class EvidenceCard : ObjekInteraktif
{
    [Header("Kartu Bukti")]
    [Tooltip("ID bukti: E0, E1, atau E2. Harus sesuai dengan naskah.")]
    public string idBukti = "E0";

    [Tooltip("Teks notifikasi yang muncul saat kartu diambil.")]
    public string pesanAmbil = "Bukti baru masuk ke tas Raka.";

    public override void Interact()
    {
        if (sudahDigunakan) return;

        GameState gs = GameState.Instance;
        if (gs == null) return;

        // Kalau kartu ini sekaligus benda puzzle Retakan Ruang (kartu E0),
        // tas harus kosong lebih dulu. Dicek sebelum bukti masuk.
        if (ambilSebagaiBenda && gs.inventory != null)
        {
            if (!gs.inventory.AmbilBenda(namaBenda))
            {
                gs.KirimNotifikasi("Tas Raka sudah berisi satu benda.");
                return;
            }
        }

        bool buktiBaru = !gs.PunyaBukti(idBukti);
        if (buktiBaru)
        {
            gs.TambahBukti(idBukti);
            gs.KirimNotifikasi(pesanAmbil + "  [" + idBukti + "]");
        }
        else
        {
            gs.KirimNotifikasi(namaTampilan + " sudah ada di tas Raka.");
        }

        sudahDigunakan = true;

        // Hilangkan mesh dan collider supaya tidak bisa diambil lagi.
        if (objekVisual != null) objekVisual.SetActive(false);
        MatikanCollider();

        if (onInteract != null) onInteract.Invoke();

        SaveSystem.Simpan();
    }

    public override string TeksPrompt()
    {
        if (sudahDigunakan) return "";
        return "[E] Ambil " + namaTampilan;
    }
}

/* ============================================================
   [CARA PASANG] EvidenceCard.cs
   1. Buat GameObject "KartuBukti_E0" (atau nama bebas).
   2. Tambahkan Collider (bisa Is Trigger) di GameObject ini atau
      di child meshnya.
   3. Drag script EvidenceCard.cs ke GameObject itu.
   4. Isi kolom:
        Nama Tampilan  : contoh "Kartu Proyek"
        Objek Visual   : mesh kartunya (agar hilang saat diambil)
        ID Bukti       : E0 / E1 / E2  (WAJIB konsisten dengan naskah)
        Pesan Ambil    : bebas
        Ambil Sebagai Benda : JANGAN centang (kartu bukti bukan benda puzzle)
        Sembunyi Setelah Diambil : centang
   5. E0 diletakkan di rak buku di "Ruang Terpanjang".
      E1 dan E2 TIDAK dibuat sebagai objek di scene, karena keduanya
      didapat lewat aktivitas (Rapikan Antarmuka & Lengkapi Catatan Uji).
   6. PENTING untuk kartu E0: karena E0 juga dipakai sebagai benda
      puzzle Retakan Ruang 1, isi kolom:
        Ambil Sebagai Benda : CENTANG
        Nama Benda          : "KartuE0"
      Dan jangkar Retakan 1 harus punya "Nama Benda Yang Dibutuhkan"
      = "KartuE0" (persis sama, huruf besar/kecil harus sama).
   7. Kalau mau menambah kartu baru, cukup duplikat GameObject ini
      lalu ganti ID Bukti-nya.
   ============================================================ */
