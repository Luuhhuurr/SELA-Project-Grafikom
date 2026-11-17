using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CVSystem.cs
/// Mengatur draf CV Raka dan snapshot lamaran yang dikirim.
///
/// Naskah BAB 3.5:
///   - Klaim 1: "membantu pengembangan antarmuka proyek kuliah"  (sesuai bukti)
///              "memimpin sistem produksi"                        (berlebihan)
///   - Klaim 2: "menguji formulir secara manual"                 (sesui bukti)
///              "menjamin sistem bebas seluruh kesalahan"         (berlebihan)
///
/// Saat lamaran dikirim, CV DIBEKU (snapshot). Bukti yang dibuat
/// belakangan tidak ikut masuk ke lamaran lama.
/// </summary>
public class CVSystem : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Teks klaim (biar mudah dibaca dan konsisten antar script)
    // ---------------------------------------------------------------
    public const string Klaim1Sesuai = "membantu pengembangan antarmuka proyek kuliah";
    public const string Klaim1Berlebihan = "memimpin sistem produksi";
    public const string Klaim2Sesuai = "menguji formulir secara manual";
    public const string Klaim2Berlebihan = "menjamin sistem bebas seluruh kesalahan";

    /// <summary>Salinan CV saat dikirim. Tidak berubah lagi setelah itu.</summary>
    [System.Serializable]
    public class SnapshotCV
    {
        public string perusahaan = "";
        public string klaim1 = "";
        public string klaim2 = "";
        public List<string> buktiTerpasang = new List<string>();
        public int hariKirim = 1;
        public bool terkirim = false;
    }

    /// <summary>Semua lamaran yang sudah dikirim (satu per perusahaan).</summary>
    public List<SnapshotCV> daftarLamaran = new List<SnapshotCV>();

    /// <summary>Dipanggil setiap ada lamaran baru yang dikirim.</summary>
    public event Action<string> OnLamaranTerkirim;

    // ---------------------------------------------------------------
    // Draf CV
    // ---------------------------------------------------------------

    /// <summary>Isi kolom klaim draf. Nomor 1 atau 2.</summary>
    public void AturKlaim(int nomor, string teks)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        if (nomor == 1) gs.klaimDraf[0] = teks;
        else if (nomor == 2) gs.klaimDraf[1] = teks;

        gs.PanggilPerubahanState();
    }

    /// <summary>Pasang bukti ke draf CV (E0 / E1 / E2).</summary>
    public void LampirkanBukti(string idBukti)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;
        if (string.IsNullOrEmpty(idBukti)) return;

        if (!gs.buktiTerpasang.Contains(idBukti))
        {
            gs.buktiTerpasang.Add(idBukti);
        }
        gs.PanggilPerubahanState();
    }

    /// <summary>Lepas bukti dari draf CV.</summary>
    public void LepasBukti(string idBukti)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        gs.buktiTerpasang.Remove(idBukti);
        gs.PanggilPerubahanState();
    }

    /// <summary>Kosongkan draf (dipakai saat mulai permainan baru).</summary>
    public void ResetDraf()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        gs.klaimDraf[0] = "";
        gs.klaimDraf[1] = "";
        gs.buktiTerpasang.Clear();
        daftarLamaran.Clear();
    }

    // ---------------------------------------------------------------
    // Klaim berlebihan
    // ---------------------------------------------------------------

    /// <summary>True kalau teks klaim itu termasuk klaim berlebihan.</summary>
    public static bool AdalahKlaimBerlebihan(string klaim)
    {
        if (string.IsNullOrEmpty(klaim)) return false;
        return klaim == Klaim1Berlebihan || klaim == Klaim2Berlebihan;
    }

    /// <summary>True kalau draf CV sekarang punya klaim berlebihan.</summary>
    public bool DraftPunyaKlaimBerlebihan()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return false;

        return AdalahKlaimBerlebihan(gs.klaimDraf[0]) || AdalahKlaimBerlebihan(gs.klaimDraf[1]);
    }

    /// <summary>
    /// True kalau snapshot lamaran punya klaim berlebihan yang belum dikoreksi.
    /// Ini salah satu syarat agar tawaran diberikan.
    /// </summary>
    public bool PunyaKlaimBerlebihan(SnapshotCV snapshot)
    {
        if (snapshot == null) return false;

        return AdalahKlaimBerlebihan(snapshot.klaim1) || AdalahKlaimBerlebihan(snapshot.klaim2);
    }

    // ---------------------------------------------------------------
    // Mengirim lamaran
    // ---------------------------------------------------------------

    /// <summary>
    /// Kirim lamaran ke perusahaan "A" atau "B".
    /// Membuat salinan (snapshot) dari draf. Salinan ini tidak ikut berubah
    /// walau nanti Raka mengedit drafnya lagi.
    /// Return false kalau lamaran untuk perusahaan itu sudah pernah dikirim.
    /// </summary>
    public bool KirimLamaran(string perusahaan)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return false;
        if (string.IsNullOrEmpty(perusahaan)) return false;

        // Satu lamaran per perusahaan.
        if (SudahKirim(perusahaan)) return false;

        SnapshotCV snapshot = new SnapshotCV();
        snapshot.perusahaan = perusahaan;
        snapshot.klaim1 = gs.klaimDraf[0];
        snapshot.klaim2 = gs.klaimDraf[1];

        // Salin daftar bukti (bukan pakai reference yang sama).
        snapshot.buktiTerpasang = new List<string>(gs.buktiTerpasang);

        snapshot.hariKirim = gs.hari;
        snapshot.terkirim = true;

        daftarLamaran.Add(snapshot);

        if (OnLamaranTerkirim != null) OnLamaranTerkirim(perusahaan);

        return true;
    }

    /// <summary>True kalau lamaran ke perusahaan itu sudah pernah dikirim.</summary>
    public bool SudahKirim(string perusahaan)
    {
        SnapshotCV snapshot = AmbilSnapshot(perusahaan);
        return snapshot != null && snapshot.terkirim;
    }

    /// <summary>Ambil snapshot lamaran suatu perusahaan. null kalau belum dikirim.</summary>
    public SnapshotCV AmbilSnapshot(string perusahaan)
    {
        for (int i = 0; i < daftarLamaran.Count; i++)
        {
            if (daftarLamaran[i].perusahaan == perusahaan) return daftarLamaran[i];
        }
        return null;
    }

    /// <summary>Semua snapshot lamaran (dipakai SaveSystem).</summary>
    public List<SnapshotCV> AmbilDaftarLamaran()
    {
        return daftarLamaran;
    }

    /// <summary>Isi daftar lamaran dari data save (dipakai SaveSystem).</summary>
    public void TerapkanDaftarLamaran(List<SnapshotCV> daftar)
    {
        daftarLamaran.Clear();
        if (daftar == null) return;

        for (int i = 0; i < daftar.Count; i++)
        {
            if (daftar[i] != null) daftarLamaran.Add(daftar[i]);
        }
    }
}

/* ============================================================
   [CARA PASANG] CVSystem.cs
   1. Taruh script ini di GameObject "Sistem" yang sama dengan GameState.
   2. Tidak ada kolom yang perlu diisi manual.
   3. Cara pakainya dari script lain:
        // ubah klaim draf
        cv.AturKlaim(1, CVSystem.Klaim1Sesuai);
        cv.AturKlaim(2, CVSystem.Klaim2Sesuai);

        // pasang bukti ke draf
        cv.LampirkanBukti("E1");

        // kirim lamaran (membuat snapshot yang beku)
        cv.KirimLamaran("A");

        // cek isi snapshot
        SnapshotCV snap = cv.AmbilSnapshot("A");
        bool adaE1 = snap.buktiTerpasang.Contains("E1");
   4. Draf CV disimpan di GameState (klaimDraf + buktiTerpasang),
      sedangkan snapshot disimpan di CVSystem.daftarLamaran.
   5. Urutan yang benar: DRAF dulu, baru KIRIM. Setelah dikirim,
      bukti yang dibuat belakangan TIDAK ikut masuk lamaran lama.
   ============================================================ */
