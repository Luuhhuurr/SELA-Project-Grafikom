using System;
using System.Collections.Generic;
using UnityEngine;

// ActivitySystem.cs
// Mengatur semua aktivitas (kegiatan) yang bisa dipilih Raka.
//
// Alur:
//
// UIManager -> ActivitySystem.MintaJalankan(aktivitas)
// -> UI menampilkan panel konfirmasi (biayaTenaga / biayaTekanan)
// -> UIManager -> ActivitySystem.JalankanAktivitas(aktivitas)
// -> biaya dipotong, satu sesi terpakai, hasil masuk ke GameState
//
// Semua hasil bersifat deterministic: tidak ada angka acak sama sekali.
public class ActivitySystem : MonoBehaviour
{
    [Header("Daftar Aktivitas")]
    [Tooltip("Isi dengan asset ActivityData. Urutannya = urutan di panel aktivitas.")]
    public List<ActivityData> daftarAktivitas = new List<ActivityData>();

    // Dipanggil saat pemain mau menjalankan aktivitas (UI menampilkan konfirmasi).
    public event Action<ActivityData> OnMintaKonfirmasi;

    // Dipanggil setelah aktivitas selesai dijalankan.
    public event Action<ActivityData> OnAktivitasSelesai;

    // Dipanggil saat aktivitas Interview perlu membuka dialog. Parameter = kode perusahaan.
    public event Action<string> OnMintaInterview;

    // Aktivitas yang sudah pernah selesai (nama aktivitas -> true).
    private Dictionary<string, bool> riwayatSelesai = new Dictionary<string, bool>();

    // Aktivitas yang sudah dijalankan hari ini (untuk aktivitas sekali per hari).
    private List<string> aktivitasHariIni = new List<string>();
    private int hariTercatat = -1;

    // ---------------------------------------------------------------
    // Cek syarat
    // ---------------------------------------------------------------

    // Cek apakah aktivitas boleh dijalankan sekarang.
    // Mengembalikan pesan alasan kalau tidak bisa (dipakai UI).
    public bool CekSyarat(ActivityData aktivitas, out string alasan)
    {
        alasan = "";
        GameState gs = GameState.Instance;
        if (gs == null) { alasan = "GameState belum siap."; return false; }
        if (aktivitas == null) { alasan = "Aktivitas tidak valid."; return false; }

        // Sudah pernah dijalankan?
        if (aktivitas.hanyaSekali && SudahSelesai(aktivitas.nama))
        {
            alasan = "Aktivitas '" + aktivitas.nama + "' sudah pernah dilakukan.";
            return false;
        }

        // Sudah dijalankan hari ini?
        // Daftar "hari ini" cuma valid kalau masih hari yang sama, kalau
        // sudah ganti hari daftarnya dianggap habis.
        if (aktivitas.sekaliPerHari && hariTercatat == gs.hari
            && aktivitasHariIni.Contains(aktivitas.nama))
        {
            alasan = "Aktivitas '" + aktivitas.nama + "' sudah dilakukan hari ini.";
            return false;
        }

        // Sudah hari yang cukup?
        if (gs.hari < aktivitas.hariMinimal)
        {
            alasan = "Aktivitas '" + aktivitas.nama + "' baru bisa dilakukan mulai hari "
                     + aktivitas.hariMinimal + ".";
            return false;
        }

        // Batas akhirnya sudah lewat?
        if (aktivitas.hariMaks > 0 && gs.hari > aktivitas.hariMaks)
        {
            alasan = "Batas '" + aktivitas.nama + "' sudah lewat. Lamaran harus dikirim "
                     + "sebelum hari " + aktivitas.hariMaks + ".";
            return false;
        }

        // Malam hari ketujuh dipakai buat keputusan akhir, jadi tidak ada
        // kegiatan yang boleh jalan lagi.
        if (gs.hari == 7 && gs.sesi == GameState.Sesi.Malam)
        {
            alasan = "Malam terakhir. Pilih dulu tawaran, atau pulang.";
            return false;
        }

        // Bukti syarat sudah lengkap?
        for (int i = 0; i < aktivitas.syaratBukti.Count; i++)
        {
            string id = aktivitas.syaratBukti[i];
            if (string.IsNullOrEmpty(id)) continue;

            if (!gs.PunyaBukti(id))
            {
                alasan = "Aktivitas '" + aktivitas.nama + "' butuh bukti " + id + ".";
                return false;
            }
        }

        // Lamaran butuh CV yang sudah dirakit: dua baris klaim dan
        // minimal satu bukti. Lamaran cuma boleh dikirim sekali, jadi CV
        // yang masih kosong akan membuat permainan buntu.
        if (aktivitas.tipe == ActivityData.Tipe.KirimLamaran)
        {
            if (gs.cvSystem == null)
            {
                alasan = "Sistem CV belum siap.";
                return false;
            }

            if (string.IsNullOrEmpty(gs.klaimDraf[0]) || string.IsNullOrEmpty(gs.klaimDraf[1]))
            {
                alasan = "Rakit CV dulu: dua baris klaim belum dipilih.";
                return false;
            }

            if (gs.buktiTerpasang.Count == 0)
            {
                alasan = "Rakit CV dulu: belum ada bukti yang dipasang.";
                return false;
            }
        }

        // Interview hanya boleh kalau lamarannya sudah terjadwal.
        if (aktivitas.tipe == ActivityData.Tipe.Interview)
        {
            if (gs.AmbilStatusLamaran(aktivitas.perusahaan) != GameState.StatusLamaran.Terjadwal)
            {
                alasan = "Lamaran ke " + aktivitas.perusahaan + " belum terkirim.";
                return false;
            }

            int jadwal = gs.AmbilJadwalInterview(aktivitas.perusahaan);
            if (jadwal <= 0)
            {
                alasan = "Belum ada jadwal interview untuk " + aktivitas.perusahaan + ".";
                return false;
            }
            if (gs.hari > jadwal)
            {
                alasan = "Jadwal interview " + aktivitas.perusahaan + " sudah terlewat.";
                return false;
            }
            if (gs.hari == jadwal && gs.sesi != GameState.Sesi.Siang)
            {
                alasan = "Interview " + aktivitas.perusahaan + " berlangsung di sesi siang hari " + jadwal + ".";
                return false;
            }
        }

        return true;
    }

    // Versi ringkas CekSyarat (kalau alasan tidak dipakai).
    public bool CekSyarat(ActivityData aktivitas)
    {
        string alasan;
        return CekSyarat(aktivitas, out alasan);
    }

    // ---------------------------------------------------------------
    // Menjalankan aktivitas
    // ---------------------------------------------------------------

    // Dipanggil UIManager saat pemain mengklik aktivitas.
    // Yardanya: cek syarat dulu, lalu minta konfirmasi ke UI.
    public void MintaJalankan(ActivityData aktivitas)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        string alasan;
        if (!CekSyarat(aktivitas, out alasan))
        {
            gs.KirimNotifikasi(alasan);
            return;
        }

        if (OnMintaKonfirmasi != null) OnMintaKonfirmasi(aktivitas);
    }

    // Benar-benar menjalankan aktivitas. Dipanggil UIManager setelah
    // pemain menyetujui konfirmasi.
    public void JalankanAktivitas(ActivityData aktivitas)
    {
        if (aktivitas == null) return;

        GameState gs = GameState.Instance;
        if (gs == null) return;

        SessionManager sm = gs.sesiManager;
        if (sm == null) return;

        string alasan;
        if (!CekSyarat(aktivitas, out alasan))
        {
            gs.KirimNotifikasi(alasan);
            return;
        }

        // Cek tenaga dan tekanan sebelum ada efek apa pun.
        if (!sm.EnergiCukup(aktivitas.biayaTenaga, aktivitas.biayaTekanan))
        {
            gs.KirimNotifikasi("Tenaga atau tekanan Raka tidak cukup untuk '" + aktivitas.nama + "'.");
            return;
        }

        PerbaruiHariIni(gs.hari);

        // Catat bahwa aktivitas ini sudah dilakukan.
        CatatSelesai(aktivitas.nama);
        aktivitasHariIni.Add(aktivitas.nama);

        // Efek khusus (bukti, verifikasi, kirim lamaran, buka interview).
        TerapkanEfek(gs, aktivitas);

        // Potong biaya, geser satu sesi, lalu simpan game.
        // SaveSystem.Simpan() dipanggil di dalam SessionManager.
        sm.JalankanAktivitas(aktivitas.nama, aktivitas.biayaTenaga, aktivitas.biayaTekanan);

        if (!string.IsNullOrEmpty(aktivitas.catatanHasil))
        {
            gs.KirimNotifikasi(aktivitas.catatanHasil);
        }

        if (OnAktivitasSelesai != null) OnAktivitasSelesai(aktivitas);
    }

    // ---------------------------------------------------------------
    // Efek khusus per tipe aktivitas
    // ---------------------------------------------------------------

    private void TerapkanEfek(GameState gs, ActivityData aktivitas)
    {
        // Bukti hasil (E1 / E2).
        if (!string.IsNullOrEmpty(aktivitas.hasil))
        {
            gs.TambahBukti(aktivitas.hasil);
        }

        if (aktivitas.tipe == ActivityData.Tipe.Verifikasi)
        {
            if (aktivitas.indeksPerusahaan >= 0 && aktivitas.indeksPerusahaan < gs.verifikasiSudah.Length)
            {
                gs.verifikasiSudah[aktivitas.indeksPerusahaan] = true;
            }
        }
        else if (aktivitas.tipe == ActivityData.Tipe.KirimLamaran)
        {
            // Buat salinan CV yang tidak bisa diubah lagi.
            if (gs.cvSystem != null) gs.cvSystem.KirimLamaran(aktivitas.perusahaan);
            gs.SetStatusLamaran(aktivitas.perusahaan, GameState.StatusLamaran.Terjadwal);

            // Catat jadwal interview (hari ke berapa, sesi siang).
            // Kalau Raka mengirim telat, jadwalnya ikut geser supaya
            // interview-nya tidak langsung dianggap terlewat.
            if (aktivitas.hariInterview > 0)
            {
                int jadwal = Mathf.Max(aktivitas.hariInterview, gs.hari);
                gs.SetJadwalInterview(aktivitas.perusahaan, jadwal);
            }
        }
        else if (aktivitas.tipe == ActivityData.Tipe.Interview)
        {
            if (OnMintaInterview != null) OnMintaInterview(aktivitas.perusahaan);
        }
    }

    // ---------------------------------------------------------------
    // Riwayat aktivitas
    // ---------------------------------------------------------------

    private void PerbaruiHariIni(int hari)
    {
        if (hari == hariTercatat) return;
        hariTercatat = hari;
        aktivitasHariIni.Clear();
    }

    private void CatatSelesai(string nama)
    {
        if (string.IsNullOrEmpty(nama)) return;
        riwayatSelesai[nama] = true;
    }

    // Cek apakah aktivitas dengan nama ini sudah pernah selesai.
    public bool SudahSelesai(string nama)
    {
        if (string.IsNullOrEmpty(nama)) return false;
        return riwayatSelesai.ContainsKey(nama) && riwayatSelesai[nama];
    }

    // Daftar nama aktivitas yang sudah selesai (dipakai SaveSystem).
    public List<string> AmbilNamaSelesai()
    {
        List<string> hasil = new List<string>();
        foreach (KeyValuePair<string, bool> pasangan in riwayatSelesai)
        {
            if (pasangan.Value) hasil.Add(pasangan.Key);
        }
        return hasil;
    }

    // Hapus riwayat aktivitas (dipakai waktu mulai permainan baru).
    // Kalau tidak dihapus, aktivitas yang "sekali saja" akan tetap
    // terkunci walaupun permainan sudah diulang dari awal.
    public void ResetRiwayat()
    {
        riwayatSelesai.Clear();
        aktivitasHariIni.Clear();
        hariTercatat = -1;
    }

    // Isi riwayat dari data save (dipakai SaveSystem).
    public void TerapkanRiwayat(List<string> daftar)
    {
        riwayatSelesai.Clear();
        if (daftar == null) return;

        for (int i = 0; i < daftar.Count; i++)
        {
            if (!string.IsNullOrEmpty(daftar[i])) riwayatSelesai[daftar[i]] = true;
        }
    }

    // Nama aktivitas yang sudah dijalankan hari ini (dipakai SaveSystem).
    public List<string> AmbilNamaHariIni()
    {
        return new List<string>(aktivitasHariIni);
    }

    // Isi daftar aktivitas hari ini dari data save (dipakai SaveSystem).
    public void TerapkanHariIni(List<string> daftar)
    {
        aktivitasHariIni.Clear();
        if (daftar == null) return;

        for (int i = 0; i < daftar.Count; i++)
        {
            if (!string.IsNullOrEmpty(daftar[i])) aktivitasHariIni.Add(daftar[i]);
        }
    }
}

/* ============================================================
   [CARA PASANG] ActivitySystem.cs
   1. Taruh script ini di GameObject "Sistem" yang sama dengan GameState.
   2. Isi kolom "Daftar Aktivitas" dengan asset ActivityData:
      klik kotak daftar, tekan tombol + di kanan bawah, lalu drag asset
      dari Project ke slotnya.
      Urutan yang disarankan (sesuai naskah):
        1. RapikanAntarmuka
        2. LengkapiCatatanUji
        3. PeriksaPerusahaanA
        4. PeriksaPerusahaanB
        5. PeriksaPerusahaanC
        6. KirimLamaranA
        7. KirimLamaranB
        8. IkutiInterviewA
        9. IkutiInterviewB
       10. Istirahat
       11. HubungiDita
       12. DudukTanpaLayar
       13. RapikanMeja
   3. UIManager yang subscribe ke event:
        OnMintaKonfirmasi -> tampilkan panel konfirmasi
        OnMintaInterview   -> tampilkan panel dialog interview
        OnAktivitasSelesai -> tutup panel konfirmasi
   4. Biaya negatif berarti pemulihan. Contoh "Istirahat":
        Biaya Tenaga = -3, BiayaTekanan = -2
   5. Kalau UIManager punya bug dan memanggil JalankanAktivitas tanpa
      persetujuan, aktivitas tetap jalan. Jadi jalur yang benar adalah:
      MintaJalankan -> konfirmasi -> JalankanAktivitas.
   ============================================================ */
