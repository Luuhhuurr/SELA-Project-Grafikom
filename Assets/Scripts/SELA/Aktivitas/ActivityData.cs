using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ActivityData.cs
/// Deskripsi satu aktivitas (kegiatan) yang bisa dipilih Raka.
///
/// Ini adalah ScriptableObject, jadi datanya disimpan sebagai asset (.asset)
/// di dalam project, bukan di dalam GameObject. Kalau datanya diubah,
/// asset ikut berubah dan tidak perlu compile ulang.
///
/// Cara membuat asset baru:
///   Assets > Create > SELA > Aktivitas
/// </summary>
[CreateAssetMenu(fileName = "AktivitasBaru", menuName = "SELA/Aktivitas", order = 1)]
public class ActivityData : ScriptableObject
{
    /// <summary>Jenis aktivitas. menentukan efek tambahan saat dijalankan.</summary>
    public enum Tipe
    {
        Biasa,        // tidak ada efek khusus
        Verifikasi,   // menandai perusahaan sudah dicek
        KirimLamaran, // membuat snapshot CV lalu status lamaran jadi Terjadwal
        Interview,    // membuka panel dialog interview
        Istirahat,    // pemulihan (tenaga naik, tekanan turun)
        Dita          // menghubungi Dita (dialog singkat)
    }

    [Header("Informasi")]
    public string nama = "Aktivitas Baru";

    [TextArea(2, 5)]
    public string deskripsi = "Penjelasan singkat aktivitas ini.";

    [Header("Biaya")]
    [Tooltip("Tenaga yang terpakai. Boleh NEGATIF untuk pemulihan "
           + "(contoh -3 berarti tenaga naik 3).")]
    public int biayaTenaga = 1;

    [Tooltip("Tekanan yang bertambah. Boleh NEGATIF untuk mengurangi tekanan.")]
    public int biayaTekanan = 1;

    [Header("Syarat")]
    [Tooltip("ID bukti yang harus sudah dimiliki, mis. E1 atau E2. Kosongkan kalau tidak perlu.")]
    public List<string> syaratBukti = new List<string>();

    [Tooltip("Aktivitas ini baru bisa dijalankan mulai hari ke berapa.")]
    public int hariMinimal = 1;

    [Tooltip("Centang kalau aktivitas ini hanya bisa dilakukan satu kali sepanjang game.")]
    public bool hanyaSekali = true;

    [Tooltip("Centang kalau aktivitas ini maksimal satu kali per hari.")]
    public bool sekaliPerHari = false;

    [Header("Hasil")]
    [Tooltip("ID bukti yang didapat dari aktivitas ini: E1 atau E2. Kosongkan kalau tidak dapat bukti.")]
    public string hasil = "";

    [TextArea(2, 4)]
    public string catatanHasil = "Hasil aktivitas ini.";

    [Header("Efek khusus")]
    public Tipe tipe = Tipe.Biasa;

    [Tooltip("Kode perusahaan: A atau B. Untuk tipe Verifikasi isi A / B / C. Kosongkan kalau tidak perlu.")]
    public string perusahaan = "";

    [Tooltip("Index perusahaan untuk tipe Verifikasi: 0 = A, 1 = B, 2 = C. -1 kalau tidak dipakai.")]
    public int indeksPerusahaan = -1;

    [Tooltip("Hari paling awal interview untuk perusahaan ini. Dipakai aktivitas Interview.")]
    public int hariInterview = 1;
}

/* ============================================================
   [CARA PASANG] ActivityData.cs
   1. Script ini TIDAK ditaruh di GameObject apa pun.
   2. Buat asset aktivitas:
        Assets > Create > SELA > Aktivitas
      Beri nama asset, mis. "Aktivitas_RapikanAntarmuka".
   3. Isi kolom Inspector:
        Nama          : "Rapikan Antarmuka"
        Deskripsi     : penjelasan untuk pemain
        Biaya Tenaga   : 1        (1 = pakai 1 tenaga, -3 = pulih 3 tenaga)
        BiayaTekanan  : 1        (1 = tambah 1 tekanan, -2 = turun 2 tekanan)
        Syarat Bukti  : list ID bukti, mis. E1 (boleh dikosongkan)
        Hari Minimal  : 1
        Hanya Sekali  : centang kalau tidak boleh diulang
        Sekali Per Hari : centang kalau maksimal 1x sehari
        Hasil         : ID bukti yang didapat, mis. E1 (kosongkan kalau tidak dapat)
        Catatan Hasil : teks yang muncul setelah aktivitas selesai
        Tipe          : Biasa / Verifikasi / KirimLamaran / Interview /
                        Istirahat / Dita
        Perusahaan    : A atau B (sesuai kebutuhan)
        Indeks Perusahaan : 0 = A, 1 = B, 2 = C, -1 = tidak dipakai
        Hari Interview: hari paling awal interview
   4. Setelah asset dibuat, drag asset itu ke kolom
      "Daftar Aktivitas" pada script ActivitySystem (GameObject "Sistem").
   5. Daftar aktivitas yang dibuat untuk game SELA.
      INGAT tanda: BiayaTenaga POSITIF = tenaga terpakai,
      BiayaTenaga NEGATIF = tenaga pulih. Tekanan-signed sama saja.
        RapikanAntarmuka   (Tenaga +1, Tekanan +1, Hasil E1, sekali)
        LengkapiCatatanUji (Tenaga +1, Tekanan +1, Syarat E1 + hari 3, Hasil E2, sekali)
        PeriksaPerusahaan  (Tenaga +1, Tekanan  0, tipe Verifikasi, sekali)
        KirimLamaranA      (Tenaga +1, Tekanan +1, Syarat E1, tipe KirimLamaran, perusahaan A, interview hari 5, sekali)
        KirimLamaranB      (Tenaga +1, Tekanan +1, Syarat E1+E2, tipe KirimLamaran, perusahaan B, interview hari 6, sekali)
        IkutiInterviewA    (Tenaga +1, Tekanan +2, tipe Interview, perusahaan A, hari 5, sekali)
        IkutiInterviewB    (Tenaga +1, Tekanan +2, tipe Interview, perusahaan B, hari 6, sekali)
        Istirahat          (Tenaga -3, Tekanan -2, tipe Istirahat, sekali per hari)
        HubungiDita        (Tenaga -2, Tekanan -2, tipe Dita, sekali per hari)
        DudukTanpaLayar    (Tenaga -2, Tekanan -1, Biasa, sekali per hari)
        RapikanMeja        (Tenaga +1, Tekanan -1, Biasa, sekali per hari)
   ============================================================ */
