using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NaskahData.cs
/// Struktur data untuk isi naskah game (isi cerita / isi lowongan).
///
/// TUJUAN SCRIPT INI:
///   Memisahkan TEKS NASKAH dari logika permainan. Semua isi naskah
///   disimpan di sini sebagai data, bukan ditulis di dalam kode UI.
///   Jadi kalau teksnya berubah, cukup ubah asset-nya, tidak perlu
///   menyentuh kode sama sekali.
///
/// CONTOH ISI HARI 1:
///   - Pesan pembuka dari Ibu (masuk ke tab "Pesan" di laptop).
///   - Draf lowongan Studio Lajur (masuk ke tab "Lamaran" di laptop).
///
/// CARA MEMBUAT ASSET BARU:
///   Assets > Create > SELA > Naskah Hari
///   Lalu isi kolom Inspector dan simpan asset-nya.
///   Supaya file otomatis ikut ke folder Resources, buat lewat
///   menu Create itu sudah otomatis menaruhnya di Assets/Naskah/.
///
/// CATATAN PENTING:
///   Script ini TIDAK punya logika sesi, logika hari, atau logikaEnding.
///   Dia hanya menyimpan teks. Semua aturan main tetap di GameState,
///   SessionManager, dan UIManager.
/// </summary>
[CreateAssetMenu(fileName = "NaskahHari1", menuName = "SELA/Naskah Hari", order = 2)]
public class NaskahData : ScriptableObject
{
    [Header("Penanda hari")]
    [Tooltip("Hari ke berapa naskah ini dipakai. Contoh: 1 untuk Hari 1.")]
    public int hari = 1;

    [Tooltip("Judul naskah hari ini, contoh: 'Bangun & Pesan Ibu'.")]
    public string judul = "Hari 1";

    [TextArea(2, 4)]
    [Tooltip("Catatan singkat untuk layar utama, contoh: 'Raka bangun di kamar 07'.")]
    public string ringkasan = "";

    [Header("Pesan pembuka (masuk tab Pesan)")]
    [Tooltip("Siapa pengirimnya. Contoh: Ibu, Dita, Studio Lajur.")]
    public string pengirim = "Ibu";

    [TextArea(3, 8)]
    [Tooltip("Isi pesan. Untuk Hari 1: pesan pembuka dari Ibu.")]
    public string pesanPembuka = "";

    [Header("Draf lowongan (masuk tab Lamaran)")]
    [Tooltip("Nama perusahaan. Contoh: Studio Lajur.")]
    public string perusahaanLowongan = "";

    [Tooltip("Nama posisi. Contoh: Junior UI Implementer.")]
    public string judulLowongan = "";

    [TextArea(4, 10)]
    [Tooltip("Isi draf lowongan: tanggung jawab, kompensasi, jam kerja, syarat.")]
    public string draftLowongan = "";

    [Tooltip("Batas waktu melamar. Contoh: 'Hari 3 malam'.")]
    public string batasLamaran = "";

    [Tooltip("Syarat yang harus dikirim. Contoh: 'Portofolio antarmuka (E1)'.")]
    public string syaratLamaran = "";

    [Header("Catatan tambahan (opsional)")]
    [TextArea(2, 6)]
    [Tooltip("Catatan kecil untuk pemain, ditampilkan di tab Catatan.")]
    public string catatanTambahan = "";

    /// <summary>
    /// Ambil naskah untuk hari tertentu.
    /// Urutan pencarian:
    ///   1. Cari asset ScriptableObject di folder Resources.
    ///   2. Kalau tidak ada, cari file JSON-nya.
    ///   3. Kalau tetap tidak ada, kembalikan null (UI harus aman kalau null).
    /// </summary>
    public static NaskahData Ambil(int hari)
    {
        // 1. Asset ScriptableObject (bikin lewat menu Create).
        NaskahData data = Resources.Load<NaskahData>("NaskahHari" + hari);
        if (data != null) return data;

        // 2. File JSON sebagai cadangan.
        TextAsset json = Resources.Load<TextAsset>("naskah_hari" + hari);
        if (json == null || string.IsNullOrEmpty(json.text)) return null;

        NaskahData dariJson = CreateInstance<NaskahData>();
        JsonUtility.FromJsonOverwrite(json.text, dariJson);
        return dariJson;
    }
}

/* ============================================================
   [CARA PASANG] NaskahData.cs
   1. Script ini TIDAK ditaruh di GameObject apa pun.
      Foldernya: Assets/Scripts/SELA/Naskah/
   2. Buat asset naskah:
        Assets > Create > SELA > Naskah Hari
      Beri nama "NaskahHari1" lalu simpan DI DALAM FOLDER Resources
      (Assets/GameData/Naskah/Resources/) supaya bisa-found
      lewat Resources.Load.
   3. Isi kolom Inspector:
        Hari                : 1
        Judul               : "Bangun & Pesan Ibu"
        Ringkasan           : "Raka bangun di kamar 07. Ponsel bergetar."
        Pengirim            : "Ibu"
        Pesan Pembuka       : isi pesan dari Ibu
        Perusahaan Lowongan : "Studio Lajur"
        Judul Lowongan      : "Junior UI Implementer"
        Draft Lowongan      : isi draf lowongan (tugas, gaji, jam kerja)
        Batas Lamaran       : "Hari 3 malam"
        Syarat Lamaran      : "Portofolio antarmuka (E1)"
   4. Isi naskah Hari 1 (nilai di atas sudah dipakai game):
        Pesan Pembuka:
          "Ka, ibu sudah beliin tiket bus tanggal 8. Kalau mau lebih
           cepat bilang saja. Kamar kosnya sampai kapan?"
        Draft Lowongan:
          "Tugas: kerjakan antarmuka formulir. Gaji Rp4.200.000/bulan,
           40 jam/minggu, 2 hari istirahat. Dokumen asli dipegang
           pekerja. Jaminan sosial tertulis. Ada pendamping tim."
        Batas Lamaran: "Hari 3 malam"
        Syarat Lamaran: "Portofolio antarmuka (E1)"
   5. Script ini tidak pegang logika sesi. Kalau mau ganti isi cerita,
      cukup ubah asset-nya.
   ============================================================ */
