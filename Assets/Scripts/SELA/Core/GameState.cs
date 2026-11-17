using System;
using System.Collections.Generic;
using UnityEngine;

// GameState.cs
// Menyimpan SEMUA data permainan SELA: hari, sesi, tenaga, tekanan,
// bukti, status lamaran, pilihan tawaran, dan progres Retakan Ruang.
//
// GameState adalah Singleton. Semua script lain membaca dan menulis data
// lewat GameState.Instance, jadi tidak perlu FindObjectOfType sama sekali.
public class GameState : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Enum. Ditaruh di dalam GameState supaya semua script memakai
    // satu sumber yang sama, tanpa perlu file enum terpisah.
    // ---------------------------------------------------------------
    public enum Sesi { Pagi, Siang, Malam }
    public enum StatusLamaran { BelumDikirim, Terjadwal, Ditolak, Ditawari }
    public enum PilihanTawaran { TidakAda, A, B, C }

    public static GameState Instance { get; private set; }

    [Header("Waktu")]
    [Range(1, 7)] public int hari = 1;
    public Sesi sesi = Sesi.Pagi;

    [Header("Kondisi Raka")]
    [Range(0, 4)] public int tenaga = 3;
    [Range(0, 6)] public int tekanan = 1;

    [Header("Bukti yang dimiliki (E0 / E1 / E2)")]
    public List<string> bukti = new List<string>();

    [Header("Status lamaran resmi")]
    public StatusLamaran statusLamaranA = StatusLamaran.BelumDikirim;
    public StatusLamaran statusLamaranB = StatusLamaran.BelumDikirim;

    [Header("Tawaran yang dipilih pemain")]
    public PilihanTawaran pilihanTawaran = PilihanTawaran.TidakAda;

    [Header("Verifikasi perusahaan (index 0 = A, 1 = B, 2 = C)")]
    public bool[] verifikasiSudah = new bool[3];

    [Header("Retakan Ruang (index 0 = retakan 1, sampai 2 = retakan 3)")]
    public bool[] retakanSelesai = new bool[3];

    [Header("Draf CV (2 baris klaim + bukti yang terpasang)")]
    public string[] klaimDraf = new string[2];
    public List<string> buktiTerpasang = new List<string>();

    [Header("Jadwal interview per perusahaan (0 = belum dikirim)")]
    public int jadwalInterviewA = 0;
    public int jadwalInterviewB = 0;

    [Header("Sistem jeda (BAB 3.4)")]
    public bool jedaTerpakaiHariIni = false;

    [Header("Referensi sistem (biasanya terisi otomatis)")]
    public SessionManager sesiManager;
    public ActivitySystem activitySystem;
    public CVSystem cvSystem;
    public InterviewManager interviewManager;
    public SimpleInventory inventory;

    // ---------------------------------------------------------------
    // Event. UI cukup berlangganan (subscribe) lalu otomatis berubah
    // tampilan setiap state berubah.
    // ---------------------------------------------------------------
    public event Action OnStateChanged;
    public event Action<string> OnNotifikasi;
    public event Action OnJedaTerbuka;

    // Penanda internal: sesi Malam sudah dipakai sehingga hari sudah
    // berganti sendiri. Dipakai supaya Tidur() tidak menambah hari 2x.
    public bool rolloverMalamTerjadi = false;

    void Awake()
    {
        // Kalau somehow ada dua GameState, buang yang kedua.
        if (Instance != null && Instance != this)
        {
            // Dimatikan dulu, supaya Start() (yang memuat save) tidak jalan
            // di GameState yang lagi dibuang.
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsiReferensiSistem();
    }

    void Start()
    {
        // Coba muat save otomatis. Kalau file utama rusak, SaveSystem akan
        // membaca file cadangan (checkpoint sebelumnya).
        SaveSystem.Muat();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ---------------------------------------------------------------
    // Pengisian referensi sistem
    // ---------------------------------------------------------------

    // Semua sistem (SessionManager, ActivitySystem, dan friends) diletakkan
    // pada GameObject yang sama dengan GameState, jadi bisa diambil
    // otomatis lewat GetComponent. Kalau mau manual, isi lewat Inspector.
    void IsiReferensiSistem()
    {
        if (sesiManager == null) sesiManager = GetComponent<SessionManager>();
        if (activitySystem == null) activitySystem = GetComponent<ActivitySystem>();
        if (cvSystem == null) cvSystem = GetComponent<CVSystem>();
        if (interviewManager == null) interviewManager = GetComponent<InterviewManager>();
        if (inventory == null) inventory = GetComponent<SimpleInventory>();
    }

    // ---------------------------------------------------------------
    // Method mengubah state
    // ---------------------------------------------------------------

    // Tambah atau kurangi tenaga. Nilai selalu dijaga di rentang 0-4.
    public void UbahTenaga(int nilai)
    {
        tenaga = Mathf.Clamp(tenaga + nilai, 0, 4);
        PanggilPerubahanState();
    }

    // Tambah atau kurangi tekanan. Nilai selalu dijaga di rentang 0-6.
    // Kalau tekanan menyentuh 6, sistem jeda akan terbuka (sekali per hari).
    public void UbahTekanan(int nilai)
    {
        tekanan = Mathf.Clamp(tekanan + nilai, 0, 6);
        PanggilPerubahanState();

        if (tekanan >= 6 && !jedaTerpakaiHariIni)
        {
            jedaTerpakaiHariIni = true;
            if (OnJedaTerbuka != null) OnJedaTerbuka();
        }
    }

    // Tambah kartu bukti (E0 / E1 / E2). Kalau sudah ada, tidak diulang.
    public void TambahBukti(string idBukti)
    {
        if (string.IsNullOrEmpty(idBukti)) return;
        if (bukti.Contains(idBukti)) return;

        bukti.Add(idBukti);
        PanggilPerubahanState();
    }

    // Cek apakah Raka sudah memegang bukti tertentu.
    public bool PunyaBukti(string idBukti)
    {
        return bukti.Contains(idBukti);
    }

    // Geser sesi: Pagi -> Siang -> Malam -> (hari berikutnya) Pagi.
    // Satu aktivitas = satu sesi, jadi method ini dipanggil setiap selesai
    // satu aktivitas.
    public void GantiSesi()
    {
        if (sesi == Sesi.Pagi)
        {
            sesi = Sesi.Siang;
        }
        else if (sesi == Sesi.Siang)
        {
            sesi = Sesi.Malam;
        }
        else
        {
            // Dari Malam langsung ke hari berikutnya.
            sesi = Sesi.Pagi;
            hari++;
            rolloverMalamTerjadi = true;
            jedaTerpakaiHariIni = false;
        }

        PanggilPerubahanState();
    }

    // Tidur: memulihkan tenaga (+2) dan menurunkan tekanan (-1), lalu
    // menutup hari dan pindah ke sesi Pagi hari berikutnya.
    public void Tidur()
    {
        UbahTenaga(2);
        UbahTekanan(-1);

        // Kalau sesi Malam sudah dipakai, hari sudah berganti sendiri,
        // jadi jangan tambah hari sekali lagi.
        if (!rolloverMalamTerjadi)
        {
            hari++;
        }

        rolloverMalamTerjadi = false;
        sesi = Sesi.Pagi;
        jedaTerpakaiHariIni = false;

        PanggilPerubahanState();
    }

    // Pemain memilih tawaran A / B / C / Tidak ada (BAB VI).
    public void PilihTawaran(PilihanTawaran pilihan)
    {
        pilihanTawaran = pilihan;
        PanggilPerubahanState();
    }

    // Dipakai UIManager saat pemain menekan tombol "Lanjut" pada jeda.
    public void TutupJeda()
    {
        if (tekanan > 4) tekanan = 4;
        if (tenaga < 1) tenaga = 1;
        PanggilPerubahanState();
    }

    // Kembalikan semua data ke kondisi awal (mulai permainan baru).
    public void ResetSeluruhState()
    {
        hari = 1;
        sesi = Sesi.Pagi;
        tenaga = 3;
        tekanan = 1;
        bukti.Clear();
        statusLamaranA = StatusLamaran.BelumDikirim;
        statusLamaranB = StatusLamaran.BelumDikirim;
        pilihanTawaran = PilihanTawaran.TidakAda;
        for (int i = 0; i < verifikasiSudah.Length; i++) verifikasiSudah[i] = false;
        for (int i = 0; i < retakanSelesai.Length; i++) retakanSelesai[i] = false;
        klaimDraf[0] = "";
        klaimDraf[1] = "";
        buktiTerpasang.Clear();
        jadwalInterviewA = 0;
        jadwalInterviewB = 0;
        jedaTerpakaiHariIni = false;
        rolloverMalamTerjadi = false;

        if (cvSystem != null) cvSystem.ResetDraf();
        if (inventory != null) inventory.Kosongkan();
        if (activitySystem != null) activitySystem.ResetRiwayat();

        PanggilPerubahanState();
    }

    // Status lamaran untuk perusahaan "A" atau "B".
    public StatusLamaran AmbilStatusLamaran(string perusahaan)
    {
        if (perusahaan == "A") return statusLamaranA;
        if (perusahaan == "B") return statusLamaranB;
        return StatusLamaran.BelumDikirim;
    }

    // Catat hari interview untuk perusahaan "A" atau "B".
    public void SetJadwalInterview(string perusahaan, int hariInterview)
    {
        if (perusahaan == "A") jadwalInterviewA = hariInterview;
        else if (perusahaan == "B") jadwalInterviewB = hariInterview;
        PanggilPerubahanState();
    }

    // Hari interview untuk perusahaan "A" atau "B". 0 = belum dijadwalkan.
    public int AmbilJadwalInterview(string perusahaan)
    {
        if (perusahaan == "A") return jadwalInterviewA;
        if (perusahaan == "B") return jadwalInterviewB;
        return 0;
    }

    // Ubah status lamaran untuk perusahaan "A" atau "B".
    public void SetStatusLamaran(string perusahaan, StatusLamaran status)
    {
        if (perusahaan == "A") statusLamaranA = status;
        else if (perusahaan == "B") statusLamaranB = status;
        PanggilPerubahanState();
    }

    // Beri tahu semua pendengar bahwa state berubah (dipakai UI).
    public void PanggilPerubahanState()
    {
        if (OnStateChanged != null) OnStateChanged();
    }

    // Tampilkan pesan singkat ke pemain.
    public void KirimNotifikasi(string pesan)
    {
        if (OnNotifikasi != null) OnNotifikasi(pesan);
    }
}

/* ============================================================
   [CARA PASANG] GameState.cs
   1. Buat GameObject baru di Hierarchy, beri nama "Sistem".
   2. Drag script GameState.cs ke GameObject "Sistem".
   3. GameState otomatis jadi Singleton dan DontDestroyOnLoad,
      jadi cukup ADA SATU GameState di seluruh project.
   4. Di bagian "Referensi Sistem" pada Inspector:
      - biarkan kosong. Awake() akan mengisinya otomatis dengan
        GetComponent, karena semua sistem dipasang di GameObject yang sama.
      - Kalau nanti sistemnya dipisah ke GameObject lain, drag
        manual ke kolom yang tersedia.
   5. Semua GameObject sistem (SessionManager, ActivitySystem,
      CVSystem, InterviewManager, SimpleInventory, EndingManager)
      HARUS diletakkan di GameObject "Sistem" yang sama.
   6. Jangan menaruh GameState di Player atau Canvas.
   ============================================================ */
