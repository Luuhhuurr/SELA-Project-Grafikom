using System;
using System.Collections.Generic;
using UnityEngine;

// CVSystem.cs
// Mengatur draf CV Raka dan snapshot lamaran yang dikirim.
//
// Naskah BAB 3.5:
// - Klaim 1: "membantu pengembangan antarmuka proyek kuliah"  (sesuai bukti)
// "memimpin sistem produksi"                        (berlebihan)
// - Klaim 2: "menguji formulir secara manual"                 (sesui bukti)
// "menjamin sistem bebas seluruh kesalahan"         (berlebihan)
//
// Saat lamaran dikirim, CV DIBEKU (snapshot). Bukti yang dibuat
// belakangan tidak ikut masuk ke lamaran lama.
public class CVSystem : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Teks klaim (biar mudah dibaca dan konsisten antar script)
    // ---------------------------------------------------------------
    public const string Klaim1Sesuai = "membantu pengembangan antarmuka proyek kuliah";
    public const string Klaim1Berlebihan = "memimpin sistem produksi";
    public const string Klaim2Sesuai = "menguji formulir secara manual";
    public const string Klaim2Berlebihan = "menjamin sistem bebas seluruh kesalahan";

    // Salinan CV saat dikirim. Tidak berubah lagi setelah itu.
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

    // Semua lamaran yang sudah dikirim (satu per perusahaan).
    public List<SnapshotCV> daftarLamaran = new List<SnapshotCV>();

    // Dipanggil setiap ada lamaran baru yang dikirim.
    public event Action<string> OnLamaranTerkirim;

    // ---------------------------------------------------------------
    // Draf CV
    // ---------------------------------------------------------------

    // Isi kolom klaim draf. Nomor 1 atau 2.
    public void AturKlaim(int nomor, string teks)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        if (nomor == 1) gs.klaimDraf[0] = teks;
        else if (nomor == 2) gs.klaimDraf[1] = teks;

        gs.PanggilPerubahanState();
    }

    // Pasang bukti ke draf CV (E0 / E1 / E2).
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

    // Lepas bukti dari draf CV.
    public void LepasBukti(string idBukti)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        gs.buktiTerpasang.Remove(idBukti);
        gs.PanggilPerubahanState();
    }

    // Kosongkan draf (dipakai saat mulai permainan baru).
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

    // True kalau teks klaim itu termasuk klaim berlebihan.
    public static bool AdalahKlaimBerlebihan(string klaim)
    {
        if (string.IsNullOrEmpty(klaim)) return false;
        return klaim == Klaim1Berlebihan || klaim == Klaim2Berlebihan;
    }

    // True kalau draf CV sekarang punya klaim berlebihan.
    public bool DraftPunyaKlaimBerlebihan()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return false;

        return AdalahKlaimBerlebihan(gs.klaimDraf[0]) || AdalahKlaimBerlebihan(gs.klaimDraf[1]);
    }

    // True kalau snapshot lamaran punya klaim berlebihan yang belum dikoreksi.
    // Ini salah satu syarat agar tawaran diberikan.
    public bool PunyaKlaimBerlebihan(SnapshotCV snapshot)
    {
        if (snapshot == null) return false;

        return AdalahKlaimBerlebihan(snapshot.klaim1) || AdalahKlaimBerlebihan(snapshot.klaim2);
    }

    // ---------------------------------------------------------------
    // Mengirim lamaran
    // ---------------------------------------------------------------

    // Kirim lamaran ke perusahaan "A" atau "B".
    // Membuat salinan (snapshot) dari draf. Salinan ini tidak ikut berubah
    // walau nanti Raka mengedit drafnya lagi.
    // Return false kalau lamaran untuk perusahaan itu sudah pernah dikirim.
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

    // True kalau lamaran ke perusahaan itu sudah pernah dikirim.
    public bool SudahKirim(string perusahaan)
    {
        SnapshotCV snapshot = AmbilSnapshot(perusahaan);
        return snapshot != null && snapshot.terkirim;
    }

    // Ambil snapshot lamaran suatu perusahaan. null kalau belum dikirim.
    public SnapshotCV AmbilSnapshot(string perusahaan)
    {
        for (int i = 0; i < daftarLamaran.Count; i++)
        {
            if (daftarLamaran[i].perusahaan == perusahaan) return daftarLamaran[i];
        }
        return null;
    }

    // Semua snapshot lamaran (dipakai SaveSystem).
    public List<SnapshotCV> AmbilDaftarLamaran()
    {
        return daftarLamaran;
    }

    // Isi daftar lamaran dari data save (dipakai SaveSystem).
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
