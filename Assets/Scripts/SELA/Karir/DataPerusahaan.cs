using System.Collections.Generic;

// DataPerusahaan.cs
//
// Isi lowongan ketiga perusahaan yang dipantau Raka.
//
// Kalau mau verifikasi, Raka cuma perlu lihat empat hal:
// tugas, kompensasi, jam kerja, dan dokumen pribadi.
//
// Datanya statis (nggak berubah saat game berjalan), jadi ditulis
// langsung di sini, bukan di asset terpisah.

public class DataPerusahaan
{
    // Satu aspek lowongan. Catatan boleh kosong kalau memang nggak ada
    // yang perlu dicatat.
    public class Aspek
    {
        public string judul = "";
        public string isi = "";
        public string catatan = "";

        // Tanda hasil pemeriksaan: "Jelas", "Perlu ditanyakan", atau
        // "Berisiko". Yang jadi pedoman buat Raka.
        public string tanda = "Jelas";
    }

    // Kode perusahaan. A = Studio Lajur, B = Koperasi Pijar, C = Akses Prima.
    public static string[] Kode = { "A", "B", "C" };

    public static string[] Nama = { "Studio Lajur", "Koperasi Pijar", "Akses Prima" };

    public static string[] Posisi = { "Junior UI Implementer", "Web Support", "Junior Developer" };

    // Ringkasan untuk dibaca cepat di panel tawaran.
    public static string[] Ringkasan =
    {
        "Rp4.200.000 per bulan, 40 jam per minggu, 2 hari istirahat. Dokumen asli dipegang pekerja.",
        "Rp3.800.000 per bulan, kontrak 3 bulan, 35 jam per minggu. Dokumen asli tidak diminta.",
        "Sampai Rp12.000.000, proses cepat. Minta KTP dan ijazah asli, pengunduran diri minimal 1 tahun."
    };

    // Catatan hasil pemeriksaan lowongan.
    public static string[] CatatanPemeriksaan =
    {
        "Semua tertulis jelas. Tidak ada yang perlu ditanyakan lagi.",
        "Risikonya kecil. Kontraknya pendek, jadi jangan terlalu jauh merencanakan.",
        "Alamat kantor tidak ada di peta. Nomor WhatsApp cuma aktif satu jam sehari. "
        + "Domain emailnya juga nggak nyambung sama alamatnya."
    };

    // Empat aspek tiap perusahaan, urut dari tugas sampai dokumen pribadi.
    public static List<Aspek> AspekMilik(string kode)
    {
        List<Aspek> daftar = new List<Aspek>();

        if (kode == "A")
        {
            daftar.Add(Buat("Tugas", "Bikin antarmuka formulir internal, hasilnya direview senior.",
                            "Ada pendamping tim, jadi nggak sendirian.", "Jelas"));
            daftar.Add(Buat("Kompensasi", "Rp4.200.000 per bulan, jaminan sosial tertulis.",
                            "Gajinya lebih besar dari Koperasi Pijar.", "Jelas"));
            daftar.Add(Buat("Jam kerja", "40 jam per minggu, 2 hari istirahat, lembur perlu persetujuan.",
                            "Lemburnya dicatat dan dikompensasi.", "Jelas"));
            daftar.Add(Buat("Dokumen pribadi", "Dokumen asli dipegang pekerja, salinan tetap milik Raka.",
                            "Ini tertulis di lowongan, jadi bisa dicek ke HR.", "Jelas"));
        }
        else if (kode == "B")
        {
            daftar.Add(Buat("Tugas", "Dukung situs CMS koperasi dan jawab obrolan anggota.",
                            "Tugas awal dibatasi, tidak semua fitur.", "Jelas"));
            daftar.Add(Buat("Kompensasi", "Rp3.800.000 per bulan, kontrak 3 bulan.",
                            "Lebih kecil dari Studio Lajur, tapi kontrak dan evaluasinya jelas.", "Jelas"));
            daftar.Add(Buat("Jam kerja", "35 jam per minggu, 1 hari libur tambahan.",
                            "Jam paling sepi, bisa dipakai belajar.", "Jelas"));
            daftar.Add(Buat("Dokumen pribadi", "Dokumen asli tidak diminta, fotokopi cukup.",
                            "Tidak ada dokumen yang nempel ke orang.", "Jelas"));
        }
        else if (kode == "C")
        {
            daftar.Add(Buat("Tugas", "Aplikasi formulir untuk klien. Katanya butuh orang yang bisa mulai minggu ini.",
                            "Tidak ada penjelasan bentuk kerjasamanya.", "Perlu ditanyakan"));
            daftar.Add(Buat("Kompensasi", "Sampai Rp12.000.000, tapi angkanya 'tergantung klien'.",
                            "Angkanya besar, tapi tidak dijamin.", "Berisiko"));
            daftar.Add(Buat("Jam kerja", "Tidak tertulis. Cuma ada tulisan 'fleksibel'.",
                            "Jam fleksibel bisa berarti jamnya juga nggak ada batasnya.", "Berisiko"));
            daftar.Add(Buat("Dokumen pribadi", "Minta KTP dan ijazah asli. Pengunduran diri minimal satu tahun.",
                            "Bagian ini perlu ekstra hati-hati.", "Berisiko"));
        }

        return daftar;
    }

    // Nama perusahaan dari kodenya. ("A" jadi "Studio Lajur")
    public static string NamaPerusahaan(string kode)
    {
        for (int i = 0; i < Kode.Length; i++)
        {
            if (Kode[i] == kode) return Nama[i];
        }
        return kode;
    }

    // Catatan hasil pemeriksaan lowongan.
    public static string Catatan(string kode)
    {
        for (int i = 0; i < Kode.Length; i++)
        {
            if (Kode[i] == kode) return CatatanPemeriksaan[i];
        }
        return "";
    }

    // Posisi lowongan di perusahaan itu.
    public static string PosisiPerusahaan(string kode)
    {
        for (int i = 0; i < Kode.Length; i++)
        {
            if (Kode[i] == kode) return Posisi[i];
        }
        return "";
    }

    private static Aspek Buat(string judul, string isi, string catatan, string tanda)
    {
        Aspek a = new Aspek();
        a.judul = judul;
        a.isi = isi;
        a.catatan = catatan;
        a.tanda = tanda;
        return a;
    }
}

/* ============================================================
   [CARA PASANG] DataPerusahaan.cs
   1. Script ini TIDAK ditaruh di GameObject apa pun. Cuma class biasa
      yang isinya dibaca lewat DataPerusahaan.AspekMilik("A").
   2. Kalau mau nambah perusahaan:
        - tambah kodenya di array Kode, Nama, Posisi, Ringkasan,
          dan CatatanPemeriksaan (urutannya harus sama)
        - tambah cabang else if di AspekMilik()
   3. Aspek yang dipakai panel verifikasi:
        "Tugas", "Kompensasi", "Jam kerja", "Dokumen pribadi"
      Kalau mau ganti, ubah string judul di AspekMilik.
   4. Catatan di bawah isi aspek muncul sebagai teks kecil di panel
      verifikasi. Kalau nggak ada yang mencurigakan, kosongkan saja.
   ============================================================ */
