using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// SaveSystem.cs
/// Menyimpan dan memuat seluruh data permainan ke file JSON di
/// Application.persistentDataPath.
///
/// Ini adalah class STATIC (tidak perlu ditaruh di GameObject apa pun).
/// Cara pakainya: SaveSystem.Simpan(); dan SaveSystem.Muat();
///
/// Alur aman saat menyimpan:
///   1. tulis dulu ke file sementara (sela_save_tmp.json)
///   2. salin file utama yang lama jadi checkpoint (sela_save_backup.json)
///   3. timpa file utama dengan isi file sementara
///   4. hapus file sementara
/// Kalau game crash di tengah, file utama yang lama masih utuh
/// dan checkpoint-nya masih ada.
/// </summary>
public static class SaveSystem
{
    /// <summary>Hasil percobaan memuat data.</summary>
    public enum HasilMuat
    {
        Berhasil,          // file utama terbaca
        CheckpointDitemukan, // file utama rusak, tapi checkpoint masih bisa dipakai
        Gagal               // tidak ada data yang bisa dipulihkan
    }

    /// <summary>
    /// Dipanggil setelah percobaan memuat selesai, supaya UIManager bisa
    /// menawarkan checkpoint ke pemain (bukan memaksakan diam-diam).
    /// </summary>
    public static event Action<HasilMuat> OnHasilMuat;

    private const string NamaUtama = "sela_save.json";
    private const string NamaTmp = "sela_save_tmp.json";
    private const string NamaBackup = "sela_save_backup.json";

    /// <summary>
    /// Folder tempat file save ditulis. Kalau dikosongkan (default),
    /// dipakai Application.persistentDataPath seperti biasa.
    /// Diisi hanya saat uji otomatis, supaya file uji tidak menimpa
    /// file save pemain yang asli.
    /// </summary>
    public static string FolderSimpanKustom = "";

    private static string FolderSimpan
    {
        get
        {
            if (!string.IsNullOrEmpty(FolderSimpanKustom)) return FolderSimpanKustom;
            return Application.persistentDataPath;
        }
    }

    /// <summary>
    /// Wadah data yang disimpan ke JSON.
    /// PENTING: JsonUtility hanya bisa menyimpan FIELD (bukan property),
    /// jadi semua isinya harus public field, bukan public property.
    /// </summary>
    [System.Serializable]
    public class DataSimpan
    {
        public int versi = 1;

        public int hari = 1;
        public int sesi = 0;              // 0 = Pagi, 1 = Siang, 2 = Malam
        public int tenaga = 3;
        public int tekanan = 1;

        public List<string> bukti = new List<string>();

        public int statusLamaranA = 0;   // 0..3 sesuai GameState.StatusLamaran
        public int statusLamaranB = 0;
        public int pilihanTawaran = 0;    // 0..3 sesuai GameState.PilihanTawaran

        public bool[] verifikasiSudah = new bool[3];
        public bool[] retakanSelesai = new bool[3];

        public string[] klaimDraf = new string[2];
        public List<string> buktiTerpasang = new List<string>();

        public bool jedaTerpakaiHariIni = false;
        public bool rolloverMalamTerjadi = false;

        public int jadwalInterviewA = 0;
        public int jadwalInterviewB = 0;

        public string bendaSaatIni = "";

        // Aktivitas yang sudah pernah selesai (bukan Dictionary karena
        // JsonUtility tidak bisa menyimpan Dictionary).
        public List<string> aktivitasSelesai = new List<string>();
        public List<string> aktivitasHariIni = new List<string>();

        public List<CVSystem.SnapshotCV> daftarLamaran = new List<CVSystem.SnapshotCV>();
    }

    // ---------------------------------------------------------------
    // Menyimpan
    // ---------------------------------------------------------------

    /// <summary>Simpan seluruh state permainan ke disk. Aman dipanggil sering.</summary>
    public static void Simpan()
    {
        if (GameState.Instance == null) return;

        string folder = FolderSimpan;
        if (string.IsNullOrEmpty(folder)) return;

        string pathUtama = Path.Combine(folder, NamaUtama);
        string pathTmp = Path.Combine(folder, NamaTmp);
        string pathBackup = Path.Combine(folder, NamaBackup);

        string json = JsonUtility.ToJson(BuatData(), true);

        try
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // 1. tulis ke file sementara dulu
            File.WriteAllText(pathTmp, json);

            // 2. file utama yang lama disimpan sebagai checkpoint
            if (File.Exists(pathUtama))
            {
                File.Copy(pathUtama, pathBackup, true);
            }

            // 3. timpa file utama dengan isi file sementara
            File.Copy(pathTmp, pathUtama, true);

            // 4. hapus file sementara
            if (File.Exists(pathTmp))
            {
                File.Delete(pathTmp);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SaveSystem] Gagal menyimpan: " + e.Message);
        }
    }

    // ---------------------------------------------------------------
    // Memuat
    // ---------------------------------------------------------------

    /// <summary>
    /// Muat state permainan dari disk.
    /// Kalau file utama rusak, otomatis mencoba file checkpoint.
    /// Hasilnya dikembalikan agar UIManager bisa menawarkan checkpoint
    /// ke pemain (bukan diam-diam memaksa).
    /// </summary>
    public static HasilMuat Muat()
    {
        HasilMuat hasil = MuatInternal();
        if (OnHasilMuat != null) OnHasilMuat(hasil);
        return hasil;
    }

    private static HasilMuat MuatInternal()
    {
        string folder = FolderSimpan;
        if (string.IsNullOrEmpty(folder)) return HasilMuat.Gagal;

        string pathUtama = Path.Combine(folder, NamaUtama);
        string pathBackup = Path.Combine(folder, NamaBackup);

        // Coba file utama dulu.
        if (File.Exists(pathUtama))
        {
            DataSimpan data = BacaFile(pathUtama);
            if (data != null)
            {
                Terapkan(data);
                return HasilMuat.Berhasil;
            }
        }

        // File utama tidak ada / rusak -> coba checkpoint.
        if (File.Exists(pathBackup))
        {
            DataSimpan data = BacaFile(pathBackup);
            if (data != null)
            {
                Terapkan(data);
                return HasilMuat.CheckpointDitemukan;
            }
        }

        return HasilMuat.Gagal;
    }

    /// <summary>Hapus semua file save. Dipakai kalau pemain memilih "Mulai ulang".</summary>
    public static void HapusSave()
    {
        string folder = FolderSimpan;
        if (string.IsNullOrEmpty(folder)) return;

        string[] daftar = { NamaUtama, NamaTmp, NamaBackup };
        for (int i = 0; i < daftar.Length; i++)
        {
            string path = Path.Combine(folder, daftar[i]);
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Gagal menghapus save: " + e.Message);
            }
        }
    }

    /// <summary>True kalau masih ada file save utama di disk.</summary>
    public static bool AdaSave()
    {
        string folder = FolderSimpan;
        if (string.IsNullOrEmpty(folder)) return false;
        return File.Exists(Path.Combine(folder, NamaUtama));
    }

    // ---------------------------------------------------------------
    // Helper internal
    // ---------------------------------------------------------------

    /// <summary>Kumpulkan state sekarang jadi objek DataSimpan.</summary>
    private static DataSimpan BuatData()
    {
        GameState gs = GameState.Instance;
        DataSimpan data = new DataSimpan();

        data.hari = gs.hari;
        data.sesi = (int)gs.sesi;
        data.tenaga = gs.tenaga;
        data.tekanan = gs.tekanan;

        data.bukti = new List<string>(gs.bukti);

        data.statusLamaranA = (int)gs.statusLamaranA;
        data.statusLamaranB = (int)gs.statusLamaranB;
        data.pilihanTawaran = (int)gs.pilihanTawaran;

        data.verifikasiSudah = new bool[3];
        for (int i = 0; i < 3; i++)
        {
            data.verifikasiSudah[i] = gs.verifikasiSudah[i];
            data.retakanSelesai[i] = gs.retakanSelesai[i];
        }

        data.klaimDraf = new string[2];
        data.klaimDraf[0] = gs.klaimDraf[0];
        data.klaimDraf[1] = gs.klaimDraf[1];
        data.buktiTerpasang = new List<string>(gs.buktiTerpasang);

        data.jedaTerpakaiHariIni = gs.jedaTerpakaiHariIni;
        data.rolloverMalamTerjadi = gs.rolloverMalamTerjadi;
        data.jadwalInterviewA = gs.jadwalInterviewA;
        data.jadwalInterviewB = gs.jadwalInterviewB;

        if (gs.inventory != null)
        {
            data.bendaSaatIni = gs.inventory.bendaSaatIni;
        }

        if (gs.activitySystem != null)
        {
            data.aktivitasSelesai = gs.activitySystem.AmbilNamaSelesai();
            data.aktivitasHariIni = gs.activitySystem.AmbilNamaHariIni();
        }

        if (gs.cvSystem != null)
        {
            data.daftarLamaran = gs.cvSystem.AmbilDaftarLamaran();
        }

        return data;
    }

    /// <summary>Baca file JSON. Kalau rusak, kembalikan null (bukan error).</summary>
    private static DataSimpan BacaFile(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return null;

            DataSimpan data = JsonUtility.FromJson<DataSimpan>(json);
            if (data == null)
            {
                Debug.LogWarning("[SaveSystem] Isi file save tidak bisa dibaca: " + path);
                return null;
            }
            return data;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SaveSystem] File save rusak (" + path + "): " + e.Message);
            return null;
        }
    }

    /// <summary>Tulis data hasil muat ke balik ke semua sistem.</summary>
    private static void Terapkan(DataSimpan data)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        gs.hari = Mathf.Clamp(data.hari, 1, 7);
        gs.sesi = (GameState.Sesi)Mathf.Clamp(data.sesi, 0, 2);
        gs.tenaga = Mathf.Clamp(data.tenaga, 0, 4);
        gs.tekanan = Mathf.Clamp(data.tekanan, 0, 6);

        gs.bukti = new List<string>(data.bukti);
        gs.statusLamaranA = (GameState.StatusLamaran)data.statusLamaranA;
        gs.statusLamaranB = (GameState.StatusLamaran)data.statusLamaranB;
        gs.pilihanTawaran = (GameState.PilihanTawaran)data.pilihanTawaran;

        if (data.verifikasiSudah != null)
        {
            for (int i = 0; i < 3 && i < gs.verifikasiSudah.Length; i++)
            {
                gs.verifikasiSudah[i] = data.verifikasiSudah[i];
            }
        }
        if (data.retakanSelesai != null)
        {
            for (int i = 0; i < 3 && i < gs.retakanSelesai.Length; i++)
            {
                gs.retakanSelesai[i] = data.retakanSelesai[i];
            }
        }

        if (data.klaimDraf != null)
        {
            gs.klaimDraf[0] = data.klaimDraf[0];
            gs.klaimDraf[1] = data.klaimDraf[1];
        }
        gs.buktiTerpasang = new List<string>(data.buktiTerpasang);

        gs.jedaTerpakaiHariIni = data.jedaTerpakaiHariIni;
        gs.rolloverMalamTerjadi = data.rolloverMalamTerjadi;
        gs.jadwalInterviewA = data.jadwalInterviewA;
        gs.jadwalInterviewB = data.jadwalInterviewB;

        if (gs.inventory != null)
        {
            gs.inventory.SetBenda(data.bendaSaatIni);
        }

        if (gs.activitySystem != null)
        {
            gs.activitySystem.TerapkanRiwayat(data.aktivitasSelesai);
            gs.activitySystem.TerapkanHariIni(data.aktivitasHariIni);
        }

        if (gs.cvSystem != null)
        {
            gs.cvSystem.TerapkanDaftarLamaran(data.daftarLamaran);
        }

        gs.PanggilPerubahanState();
    }
}

/* ============================================================
   [CARA PASANG] SaveSystem.cs
   1. TIDAK perlu ditaruh di GameObject apa pun. Ini class static.
   2. Letakkan file ini di folder: Assets/Scripts/SELA/Core/
   3. Unity yang memanggil otomatis:
      - GameState.Start()          -> SaveSystem.Muat()  (muat otomatis)
      - SessionManager             -> SaveSystem.Simpan() (setelah aktivitas & tidur)
   4. Kalau ingin tes manual, buat GameObject kosong "PengujiSave"
      dan taruh script kecil ini di dalamnya:
          public class PengujiSave : MonoBehaviour {
              void Start() { SaveSystem.Simpan(); }
          }
      (opsional, tidak wajib)
   5. Lokasi file save di Windows:
      C:/Users/<NamaUser>/AppData/LocalLow/<NamaPerusahaan>/<NamaProduk>/
      Nama file: sela_save.json, sela_save_backup.json, sela_save_tmp.json
      ("_backup" = checkpoint sebelumnya, dipakai kalau file utama rusak)
   ============================================================ */
