using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InterviewManager.cs
/// Menjalankan interview dengan Nara (Studio Lajur) dan Bagas (Koperasi Pijar)
/// sesuai naskah BAB V.
///
/// Aturan main (deterministik, tanpa angka acak):
///   Tawaran diberikan hanya kalau SEMUA syarat ini terpenuhi:
///     1. Semua bukti wajib ada pada snapshot lamaran (bukan pada draf).
///     2. Tidak ada klaim berlebihan yang belum dikoreksi saat interview.
///     3. Setiap jawaban bernilai minimal 1.
///     4. Total nilai dua pertanyaan minimal 3 dari 4.
///
/// Nilai 0/1/2 tiap jawaban TIDAK pernah ditampilkan ke pemain.
/// </summary>
public class InterviewManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Struktur data
    // ---------------------------------------------------------------

    [System.Serializable]
    public class OpsiJawaban
    {
        public string teks = "";
        public int nilai = 0;   // 0 / 1 / 2, tidak ditampilkan ke pemain
    }

    [System.Serializable]
    public class PertanyaanInterview
    {
        public string teks = "";
        public List<OpsiJawaban> opsi = new List<OpsiJawaban>();
    }

    [System.Serializable]
    public class DataInterview
    {
        public string id = "";
        public string perusahaan = "";        // "A" atau "B"
        public string namaPerusahaan = "";
        public string posisi = "";
        public string namaPewawancar = "";
        public string sapaanPembuka = "";
        public string penutup = "";
        public string tawaranIsi = "";
        public List<string> buktiWajib = new List<string>();
        public List<PertanyaanInterview> pertanyaan = new List<PertanyaanInterview>();
    }

    /// <summary>Satu baris dialog yang sedang tampil.</summary>
    private class Baris
    {
        public string pembicara = "";
        public string teks = "";
        public List<string> opsi = new List<string>();
        public bool barisKoreksi = false;   // baris pilihan pembuka
        public int barisPertanyaan = 0;    // 0 = bukan pertanyaan, 1 = pertanyaan 1, 2 = pertanyaan 2
    }

    // ---------------------------------------------------------------
    // Data interview (dari naskah BAB V)
    // ---------------------------------------------------------------

    private List<DataInterview> daftarInterview = new List<DataInterview>();
    private DataInterview interviewAktif;

    private List<Baris> barisDialog = new List<Baris>();
    private int indeksBaris = 0;

    private int skorTotal = 0;
    private bool semuaJawabanMinimal = true;
    private bool klaimSudahDikoreksi = false;

    public static InterviewManager Instance { get; private set; }

    /// <summary>True selama ada dialog interview yang sedang berjalan.</summary>
    public bool SedangBerlangsung { get; private set; }

    /// <summary>Pembicara baris yang sedang tampil.</summary>
    public string PembicaraSaatIni
    {
        get
        {
            if (!SedangBerlangsung) return "";
            return barisDialog[indeksBaris].pembicara;
        }
    }

    /// <summary>Teks baris yang sedang tampil.</summary>
    public string TeksSaatIni
    {
        get
        {
            if (!SedangBerlangsung) return "";
            return barisDialog[indeksBaris].teks;
        }
    }

    /// <summary>Pilihan jawaban untuk baris sekarang. Kosong = tekan Enter untuk lanjut.</summary>
    public List<string> OpsiSaatIni
    {
        get
        {
            List<string> hasil = new List<string>();
            if (!SedangBerlangsung) return hasil;

            Baris baris = barisDialog[indeksBaris];
            for (int i = 0; i < baris.opsi.Count; i++) hasil.Add(baris.opsi[i]);
            return hasil;
        }
    }

    /// <summary>Dipanggil setiap isi dialog berubah (UI tinggal menggambar ulang).</summary>
    public event Action OnDialogBerubah;

    /// <summary>Dipanggil saat interview selesai. Parameter: "A"/"B", lalu ringkasan hasil.</summary>
    public event Action<string, string> OnInterviewSelesai;

    void Awake()
    {
        Instance = this;
        SusunDataInterview();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------------------------------------------------------------
    // Menyusun data interview dari naskah
    // ---------------------------------------------------------------

    private void SusunDataInterview()
    {
        daftarInterview.Clear();

        // ---------------- 5.1 Interview Studio Lajur (Nara) ----------------
        DataInterview a = new DataInterview();
        a.id = "A";
        a.perusahaan = "A";
        a.namaPerusahaan = "Studio Lajur";
        a.posisi = "Junior UI Implementer";
        a.namaPewawancar = "Nara";
        a.sapaanPembuka = "Selamat siang, Raka. Terima kasih sudah melamar. Saya Nara, dari tim rekrutmen.\n\nSebelum mulai, ada yang ingin Anda klarifikasi dari CV yang dikirim?";
        a.penutup = "Terima kasih, Raka. Kami akan memberitahukan hasil melalui email paling lambat tiga hari kerja.";
        a.tawaranIsi = "Studio Lajur — Junior UI Implementer. Rp4.200.000 per bulan, 40 jam per minggu, 2 hari istirahat. Tugas antarmuka. Dokumen asli dipegang pekerja. Jaminan sosial tertulis. Ada pendamping. Lembur perlu persetujuan dan pencatatan kompensasi.";
        a.buktiWajib.Add("E1");

        PertanyaanInterview a1 = new PertanyaanInterview();
        a1.teks = "Baik. Pertanyaan pertama: bisa ceritakan satu contoh antarmuka yang pernah Anda perbaiki, dan apa yang Anda ukur sebagai perbaikan?";
        a1.opsi.Add(new OpsiJawaban { teks = "\"Formulir pendaftaran. Saya perbaiki label input dan pesan kesalahannya supaya jelas, dan saya ukur dari tiga kasus uji yang lolos.\"", nilai = 2 });
        a1.opsi.Add(new OpsiJawaban { teks = "\"Saya pernah memperbaiki beberapa antarmuka, membuatnya lebih bagus.\"", nilai = 0 });
        a.pertanyaan.Add(a1);

        PertanyaanInterview a2 = new PertanyaanInterview();
        a2.teks = "Pertanyaan kedua: posisi ini bekerja dengan tim pengembang. Bagaimana Anda mendokumentasikan perubahan antarmuka agar tim lain dapat mengikuti?";
        a2.opsi.Add(new OpsiJawaban { teks = "\"Saya dokumentasikan lewat catatan pengujian: tiga kasus uji — kolom kosong, format salah, data valid — plus batas pengujiannya, dan dokumentasi sebelum/sesudah.\"", nilai = 2 });
        a2.opsi.Add(new OpsiJawaban { teks = "\"Saya pakai komentar di kode, catat perubahannya, lalu bicara langsung dengan tim.\"", nilai = 1 });
        a.pertanyaan.Add(a2);

        daftarInterview.Add(a);

        // ---------------- 5.2 Interview Koperasi Pijar (Bagas) ----------------
        DataInterview b = new DataInterview();
        b.id = "B";
        b.perusahaan = "B";
        b.namaPerusahaan = "Koperasi Pijar";
        b.posisi = "Web Support";
        b.namaPewawancar = "Bagas";
        b.sapaanPembuka = "Halo Raka, saya Bagas. Terima kasih sudah melamar. Posisi ini kontrak 3 bulan dulu, ya — saya mau jelas dari awal.\n\nAda yang ingin diklarifikasi dari CV?";
        b.penutup = "Baik, terima kasih. Hasil akan kami kirim via email.";
        b.tawaranIsi = "Koperasi Pijar — Web Support. Kontrak 3 bulan, Rp3.800.000 per bulan, 35 jam per minggu, 2 hari istirahat. Pemeliharaan formulir. Ada pendampingan. Jaminan sosial tertulis. Kontak terkonfirmasi, tugas awal dibatasi, evaluasi akhir kontrak tidak menjamin pembaruan, dokumen asli tidak diminta.";
        b.buktiWajib.Add("E1");
        b.buktiWajib.Add("E2");

        PertanyaanInterview b1 = new PertanyaanInterview();
        b1.teks = "Pertanyaan pertama: tugas utama adalah pemeliharaan formulir pendaftaran anggota. Bisa ceritakan pengalaman Anda menangani formulir, termasuk penanganan kesalahan input?";
        b1.opsi.Add(new OpsiJawaban { teks = "\"Formulir pendaftaran anggota. Tiga kasus uji — kolom kosong, format salah, data valid — plus batas pengujian, dan pesan kesalahan yang saya perbaiki.\"", nilai = 2 });
        b1.opsi.Add(new OpsiJawaban { teks = "\"Saya pernah membuat formulir dan menangani input pengguna.\"", nilai = 1 });
        b.pertanyaan.Add(b1);

        PertanyaanInterview b2 = new PertanyaanInterview();
        b2.teks = "Pertanyaan kedua: kontrak 3 bulan, evaluasi di akhir. Kami tidak menjanjikan perpanjangan otomatis. Bagaimana Anda menyikapi itu?";
        b2.opsi.Add(new OpsiJawaban { teks = "\"Saya memahami. Selama tiga bulan saya akan mendokumentasikan pekerjaan dengan baik, supaya evaluasinya berdasar bukti, bukan janji.\"", nilai = 2 });
        b2.opsi.Add(new OpsiJawaban { teks = "\"Saya berharap bisa diperpanjang.\"", nilai = 1 });
        b.pertanyaan.Add(b2);

        daftarInterview.Add(b);
    }

    // ---------------------------------------------------------------
    // Mulai interview
    // ---------------------------------------------------------------

    /// <summary>Mulai interview untuk perusahaan "A" atau "B".</summary>
    public void MulaiInterview(string perusahaan)
    {
        interviewAktif = null;
        for (int i = 0; i < daftarInterview.Count; i++)
        {
            if (daftarInterview[i].perusahaan == perusahaan)
            {
                interviewAktif = daftarInterview[i];
                break;
            }
        }

        if (interviewAktif == null)
        {
            Debug.LogWarning("[InterviewManager] Tidak ada data interview untuk perusahaan " + perusahaan);
            return;
        }

        skorTotal = 0;
        semuaJawabanMinimal = true;
        klaimSudahDikoreksi = false;
        indeksBaris = 0;

        BangunBarisDialog();

        SedangBerlangsung = true;
        if (OnDialogBerubah != null) OnDialogBerubah();
    }

    /// <summary>Susun urutan baris dialog untuk interview yang sedang aktif.</summary>
    private void BangunBarisDialog()
    {
        barisDialog.Clear();

        // Baris 1: sapaan + pertanyaan pembuka.
        Baris pembuka = new Baris();
        pembuka.pembicara = interviewAktif.namaPewawancar;
        pembuka.teks = interviewAktif.sapaanPembuka;
        pembuka.barisKoreksi = true;
        pembuka.opsi.Add(BangunTeksKoreksi());
        pembuka.opsi.Add(BangunTeksTidakKoreksi());
        barisDialog.Add(pembuka);

        // Baris 2 dan 3: dua pertanyaan.
        for (int i = 0; i < interviewAktif.pertanyaan.Count; i++)
        {
            Baris baris = new Baris();
            baris.pembicara = interviewAktif.namaPewawancar;
            baris.teks = interviewAktif.pertanyaan[i].teks;
            baris.barisPertanyaan = i + 1;

            for (int j = 0; j < interviewAktif.pertanyaan[i].opsi.Count; j++)
            {
                baris.opsi.Add(interviewAktif.pertanyaan[i].opsi[j].teks);
            }
            barisDialog.Add(baris);
        }

        // Baris 4: penutup.
        Baris penutup = new Baris();
        penutup.pembicara = interviewAktif.namaPewawancar;
        penutup.teks = interviewAktif.penutup;
        barisDialog.Add(penutup);
    }

    /// <summary>
    /// Teks pilihan "meluruskan klaim".
    /// Kalau lamaran tidak punya klaim berlebihan, teksnya dibuat generik.
    /// </summary>
    private string BangunTeksKoreksi()
    {
        CVSystem.SnapshotCV snapshot = AmbilSnapshot();
        string klaimBerlebihan = "";

        if (snapshot != null)
        {
            if (CVSystem.AdalahKlaimBerlebihan(snapshot.klaim1)) klaimBerlebihan = snapshot.klaim1;
            else if (CVSystem.AdalahKlaimBerlebihan(snapshot.klaim2)) klaimBerlebihan = snapshot.klaim2;
        }

        if (string.IsNullOrEmpty(klaimBerlebihan))
        {
            return "\"Tidak ada yang perlu diklarifikasi.\"";
        }

        return "\"Saya ingin meluruskan: di CV saya menulis '" + klaimBerlebihan
             + "', padahal saya anggota tim yang menangani antarmuka. Server dibantu rekan tim.\"";
    }

    private string BangunTeksTidakKoreksi()
    {
        CVSystem.SnapshotCV snapshot = AmbilSnapshot();
        bool adaKlaimBerlebihan = false;

        if (snapshot != null)
        {
            adaKlaimBerlebihan = CVSystem.AdalahKlaimBerlebihan(snapshot.klaim1)
                              || CVSystem.AdalahKlaimBerlebihan(snapshot.klaim2);
        }

        if (adaKlaimBerlebihan)
        {
            return "\"Tidak ada. CV saya sudah sesuai.\"";
        }
        return "\"Tidak ada. CV saya sudah sesuai, tapi silakan tanya kalau ada yang kurang jelas.\"";
    }

    private CVSystem.SnapshotCV AmbilSnapshot()
    {
        GameState gs = GameState.Instance;
        if (gs == null || gs.cvSystem == null) return null;
        return gs.cvSystem.AmbilSnapshot(interviewAktif.perusahaan);
    }

    // ---------------------------------------------------------------
    // Alur dialog
    // ---------------------------------------------------------------

    /// <summary>Pemain memilih salah satu opsi (nomor mulai dari 0).</summary>
    public void PilihOpsi(int indeks)
    {
        if (!SedangBerlangsung) return;

        Baris baris = barisDialog[indeksBaris];
        if (indeks < 0 || indeks >= baris.opsi.Count) return;

        // Baris pembuka koreksi.
        if (baris.barisKoreksi)
        {
            klaimSudahDikoreksi = (indeks == 0);
        }
        // Baris pertanyaan: ambil nilai jawabannya.
        else if (baris.barisPertanyaan > 0)
        {
            int nomorPertanyaan = baris.barisPertanyaan - 1;
            if (nomorPertanyaan < interviewAktif.pertanyaan.Count)
            {
                OpsiJawaban opsi = interviewAktif.pertanyaan[nomorPertanyaan].opsi[indeks];
                skorTotal += opsi.nilai;
                if (opsi.nilai < 1) semuaJawabanMinimal = false;
            }
        }

        Lanjut();
    }

    /// <summary>Lanjut ke baris berikutnya. Kalau sudah habis, interview selesai.</summary>
    public void Lanjut()
    {
        if (!SedangBerlangsung) return;

        indeksBaris++;
        if (indeksBaris >= barisDialog.Count)
        {
            SelesaikanInterview();
            return;
        }

        if (OnDialogBerubah != null) OnDialogBerubah();
    }

    // ---------------------------------------------------------------
    // Menyelesaikan interview
    // ---------------------------------------------------------------

    private void SelesaikanInterview()
    {
        SedangBerlangsung = false;

        GameState gs = GameState.Instance;
        if (gs == null) return;

        CVSystem.SnapshotCV snapshot = AmbilSnapshot();

        // ---- Syarat 1: bukti wajib ada pada snapshot ----
        bool buktiLengkap = true;
        for (int i = 0; i < interviewAktif.buktiWajib.Count; i++)
        {
            string id = interviewAktif.buktiWajib[i];
            if (snapshot == null || !snapshot.buktiTerpasang.Contains(id))
            {
                buktiLengkap = false;
            }
        }

        // ---- Syarat 2: tidak ada klaim berlebihan yang belum dikoreksi ----
        bool adaKlaimBerlebihan = false;
        if (snapshot != null)
        {
            adaKlaimBerlebihan = CVSystem.AdalahKlaimBerlebihan(snapshot.klaim1)
                              || CVSystem.AdalahKlaimBerlebihan(snapshot.klaim2);
        }
        bool klaimBelumDikoreksi = adaKlaimBerlebihan && !klaimSudahDikoreksi;

        // ---- Syarat 3 & 4: nilai jawaban ----
        bool nilaiCukup = semuaJawabanMinimal && skorTotal >= 3;

        bool dapatTawaran = buktiLengkap && !klaimBelumDikoreksi && nilaiCukup;

        string ringkasan;
        if (dapatTawaran)
        {
            gs.SetStatusLamaran(interviewAktif.perusahaan, GameState.StatusLamaran.Ditawari);
            ringkasan = "TAWARAN DIBERIKAN";
        }
        else
        {
            gs.SetStatusLamaran(interviewAktif.perusahaan, GameState.StatusLamaran.Ditolak);
            gs.UbahTekanan(1);
            ringkasan = "DITOLAK";
        }

        if (OnInterviewSelesai != null)
        {
            OnInterviewSelesai(interviewAktif.perusahaan, ringkasan);
        }
    }

    /// <summary>
    /// Teks hasil interview: email tawaran atau alasan penolakan (BAB 5.3).
    /// Dipakai UIManager untuk menampilkan layar hasil.
    /// </summary>
    public string TeksHasil(bool ditawari)
    {
        if (interviewAktif == null) return "";

        if (ditawari)
        {
            return "Selamat, Anda kami tawari posisi " + interviewAktif.posisi + " di "
                 + interviewAktif.namaPerusahaan + ".\n\n" + interviewAktif.tawaranIsi;
        }

        // Alasan utama ditampilkan berurutan (BAB 5.3).
        List<string> alasan = new List<string>();

        CVSystem.SnapshotCV snapshot = AmbilSnapshot();
        bool buktiLengkap = true;
        for (int i = 0; i < interviewAktif.buktiWajib.Count; i++)
        {
            string id = interviewAktif.buktiWajib[i];
            if (snapshot == null || !snapshot.buktiTerpasang.Contains(id)) buktiLengkap = false;
        }

        if (!buktiLengkap)
        {
            alasan.Add("Bukti yang disyaratkan (E1/E2) tidak lengkap pada saat lamaran dikirim.");
        }

        bool adaKlaimBerlebihan = false;
        if (snapshot != null)
        {
            adaKlaimBerlebihan = CVSystem.AdalahKlaimBerlebihan(snapshot.klaim1)
                              || CVSystem.AdalahKlaimBerlebihan(snapshot.klaim2);
        }
        if (adaKlaimBerlebihan && !klaimSudahDikoreksi)
        {
            alasan.Add("Ada klaim di CV yang belum dapat dipertanggungjawabkan dan tidak dikoreksi saat interview.");
        }

        if (!semuaJawabanMinimal || skorTotal < 3)
        {
            alasan.Add("Jawaban interview kurang spesifik; kami membutuhkan contoh yang dapat diverifikasi.");
        }

        string hasil = "Hasil interview:\n";
        for (int i = 0; i < alasan.Count; i++) hasil += "- " + alasan[i] + "\n";

        hasil += "\nPenolakan ini untuk satu posisi, satu waktu. Kesempatan lain tetap terbuka.\n\n";
        hasil += "Raka: Ini bukan berarti aku gagal total. Ini satu perusahaan, satu jawaban.";

        return hasil;
    }

    /// <summary>True kalau interview ini berakhir dengan tawaran.</summary>
    public bool DiberiTawaran
    {
        get
        {
            GameState gs = GameState.Instance;
            if (gs == null || interviewAktif == null) return false;
            return gs.AmbilStatusLamaran(interviewAktif.perusahaan) == GameState.StatusLamaran.Ditawari;
        }
    }

    /// <summary>Kembalikan ke kondisi awal (dipakai saat reset permainan).</summary>
    public void ResetInterview()
    {
        SedangBerlangsung = false;
        barisDialog.Clear();
        indeksBaris = 0;
        skorTotal = 0;
        semuaJawabanMinimal = true;
        klaimSudahDikoreksi = false;
        interviewAktif = null;
    }
}

/* ============================================================
   [CARA PASANG] InterviewManager.cs
   1. Taruh script ini di GameObject "Sistem" yang sama dengan GameState.
   2. Data interview sudah ditulis langsung di dalam script (dari naskah
      BAB V), jadi tidak perlu asset atau setting di Inspector.
   3. Alur pemakaian:
        - ActivitySystem memanggil OnMintaInterview("A") atau ("B")
          saat aktivitas "Ikuti Interview" dijalankan.
        - UIManager menerima event itu lalu memanggil
          InterviewManager.Instance.MulaiInterview("A").
        - UIManager menggambar baris dialog dari:
            InterviewManager.PembicaraSaatIni
            InterviewManager.TeksSaatIni
            InterviewManager.OpsiSaatIni
        - Pemain menekan 1 atau 2 -> PilihOpsi(0) atau PilihOpsi(1).
        - Baris tanpa pilihan -> Lanjut().
        - InterviewManager.OnInterviewSelesai memberitahu UIManager
          kalau selesai, lalu UIManager menampilkan InterviewManager.TeksHasil().
   4. ATURAN PENTING: nilai 0/1/2 tiap jawaban TIDAK pernah ditampilkan
      ke pemain. Jangan menambahkan kode yang menampilkannya.
   5. Ending (Tawaran / Ditolak) ditentukan oleh pilihan jawaban +
      isi snapshot lamaran, TIDAK oleh angka acak.
   ============================================================ */
