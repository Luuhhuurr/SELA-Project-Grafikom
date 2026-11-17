using System;
using UnityEngine;

/// <summary>
/// SessionManager.cs
/// Mengatur siklus sesi dan hari dalam game SELA.
///
/// Satu aktivitas = satu sesi.
/// Urutan sesi: Pagi -> Siang -> Malam -> (tidur) -> Pagi hari berikutnya.
///
/// Biaya pada aktivitas boleh bernilai NEGATIF. Angka negatif berarti
/// pemulihan, bukan biaya. Contoh: Istirahat memakai biayaTenaga = -3
/// (tenaga naik 3) dan biayaTekanan = -2 (tekanan turun 2).
/// </summary>
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    /// <summary>Dipanggil setiap sesi berganti (Pagi/Siang/Malam).</summary>
    public event Action OnSesiBerpindah;

    /// <summary>Dipanggil setiap masuk hari baru.</summary>
    public event Action OnHariBaru;

    /// <summary>Dipanggil satu kali pada Malam hari ke-7 (keputusan final).</summary>
    public event Action OnFinalNight;

    private bool finalNightTerbuka = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------------------------------------------------------------
    // Cek energi
    // ---------------------------------------------------------------

    /// <summary>
    /// Cek apakah Raka cukup tenaga dan tekanan untuk menjalankan aktivitas.
    /// Yang dicek hanya biaya POSITIF. Biaya negatif (pemulihan) selalu boleh.
    /// </summary>
    public bool EnergiCukup(int biayaTenaga, int biayaTekanan)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return false;

        if (biayaTenaga > 0 && gs.tenaga < biayaTenaga) return false;
        if (biayaTekanan > 0 && gs.tekanan + biayaTekanan > 6) return false;

        return true;
    }

    // ---------------------------------------------------------------
    // Menjalankan aktivitas
    // ---------------------------------------------------------------

    /// <summary>
    /// Jalankan satu aktivitas: potong biaya, geser satu sesi, cek malam
    /// terakhir, lalu simpan game.
    /// Return true kalau aktivitasnya benar-benar jalan.
    /// </summary>
    public bool JalankanAktivitas(string nama, int biayaTenaga, int biayaTekanan)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return false;

        if (!EnergiCukup(biayaTenaga, biayaTekanan))
        {
            gs.KirimNotifikasi("Tenaga atau tekanan Raka tidak cukup untuk '" + nama + "'.");
            return false;
        }

        int hariSebelum = gs.hari;
        GameState.Sesi sesiSebelum = gs.sesi;

        // BiayaTenaga POSITIF = tenaga terpakai (jadi dikurangi),
        // biayaTenaga NEGATIF = pemulihan (tenaga naik).
        // BiayaTekanan sudah berlawanan: positif = tekanan naik.
        if (biayaTenaga != 0) gs.UbahTenaga(-biayaTenaga);
        if (biayaTekanan != 0) gs.UbahTekanan(biayaTekanan);

        // Satu aktivitas = satu sesi.
        gs.GantiSesi();

        if (gs.hari != hariSebelum)
        {
            if (OnHariBaru != null) OnHariBaru();
        }
        if (gs.sesi != sesiSebelum)
        {
            if (OnSesiBerpindah != null) OnSesiBerpindah();
        }

        CekFinalNight();
        CekInterviewTerlewat();

        // Auto-save setelah setiap aktivitas selesai.
        SaveSystem.Simpan();
        return true;
    }

    // ---------------------------------------------------------------
    // Tidur
    // ---------------------------------------------------------------

    /// <summary>Tidur: memulihkan tenaga, menutup hari, simpan game.</summary>
    public void Tidur()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        int hariSebelum = gs.hari;

        gs.Tidur();

        if (gs.hari != hariSebelum)
        {
            if (OnHariBaru != null) OnHariBaru();
        }
        if (OnSesiBerpindah != null) OnSesiBerpindah();

        CekFinalNight();
        CekInterviewTerlewat();

        // Auto-save setelah tidur.
        SaveSystem.Simpan();
    }

    // ---------------------------------------------------------------
    // Malam terakhir
    // ---------------------------------------------------------------

    /// <summary>
    /// Malam hari ke-7 adalah waktu keputusan akhir (BAB VI).
    /// Also tetap dipicu kalau pemain somehow melewati hari 7 tanpa
    /// pernah menyentuh sesi Malam.
    /// </summary>
    private void CekFinalNight()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;
        if (finalNightTerbuka) return;

        bool sudahMalamHariTujuh = (gs.hari == 7 && gs.sesi == GameState.Sesi.Malam);
        bool sudahLewatHariTujuh = (gs.hari >= 8);

        if (sudahMalamHariTujuh || sudahLewatHariTujuh)
        {
            finalNightTerbuka = true;
            if (OnFinalNight != null) OnFinalNight();
        }
    }

    // ---------------------------------------------------------------
    // Interview yang terlewat
    // ---------------------------------------------------------------

    /// <summary>
    /// Interview selalu berlangsung di sesi siang pada hari yang dijadwalkan.
    /// Kalau pemain tidur tanpa melakukannya, jadwal terlewat: lamaran
    /// ditandai ditolak dan Tekanan naik satu kali (naskah BAB IV Hari 5).
    /// </summary>
    private void CekInterviewTerlewat()
    {
        CekSatuPerusahaan("A");
        CekSatuPerusahaan("B");
    }

    private void CekSatuPerusahaan(string perusahaan)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        int jadwal = gs.AmbilJadwalInterview(perusahaan);
        if (jadwal <= 0) return;
        if (gs.hari <= jadwal) return;
        if (gs.AmbilStatusLamaran(perusahaan) != GameState.StatusLamaran.Terjadwal) return;

        gs.SetStatusLamaran(perusahaan, GameState.StatusLamaran.Ditolak);
        gs.UbahTekanan(1);
        gs.SetJadwalInterview(perusahaan, 0);
        gs.KirimNotifikasi("Interview untuk perusahaan " + perusahaan
                         + " terlewat. Lamaran ditandai ditolak.");
    }

    // ---------------------------------------------------------------
    // Bantuan tampilan
    // ---------------------------------------------------------------

    /// <summary>Ubah enum sesi jadi teks yang enak dibaca di HUD.</summary>
    public static string TeksSesi(GameState.Sesi sesi)
    {
        if (sesi == GameState.Sesi.Pagi) return "Pagi";
        if (sesi == GameState.Sesi.Siang) return "Siang";
        return "Malam";
    }
}

/* ============================================================
   [CARA PASANG] SessionManager.cs
   1. Taruh script ini di GameObject "Sistem" yang sama dengan GameState.
   2. Tidak perlu ada kolom yang diisi manual.
   3. Yang memanggil script ini:
      - ActivitySystem.JalankanAktivitas()  -> isi nama + biayaTenaga + biayaTekanan
      - UIManager tombol "Tidur"             -> SessionManager.Instance.Tidur()
   4. Yang mendengarkan event-nya:
      - UIManager  : OnSesiBerpindah, OnHariBaru  ( buat update HUD)
      - EndingManager : OnFinalNight  (buka layar keputusan akhir)
   5. Biaya aktivitas boleh negatif = pemulihan.
      Contoh isi ActivityData "Istirahat":
        Biaya Tenaga  = -3   (tenaga +3)
        BiayaTekanan = -2   (tekanan -2)
   ============================================================ */
