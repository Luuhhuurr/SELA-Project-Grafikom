using System;
using UnityEngine;

// EndingManager.cs
// Mengatur tiga ending akhir game SELA (naskah BAB VI).
//
// PENTING: ending HANYA ditentukan oleh pilihan kontrak pemain
// (GameState.pilihanTawaran). Tekanan dan Tenaga TIDAK boleh dibaca di
// methods ini. Status kesehatan memengaruhi Alma nanti, bukan_epilog.
//
// A atau B  -> "Mulai dengan Batas"
// Tidak ada -> "Pulang, Bukan Selesai"
// C         -> "Kontrak yang Mengunci" (peringatan khusus + konfirmasi 2x)
public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    // ---------------------------------------------------------------
    // Event
    // ---------------------------------------------------------------

    // Dipanggil saat panel keputusan final harus muncul (malam hari 7).
    public event Action OnPilihanFinalMuncul;

    // Dipanggil saat layar ending + refleksi harus muncul.
    public event Action OnEndingMuncul;

    // Dipanggil saat peringatan khusus tawaran C muncul.
    public event Action<int> OnPeringatanCMuncul;

    // Dipanggil saat pemain batal dari peringatan C.
    public event Action OnPeringatanCBatal;

    // ---------------------------------------------------------------
    // Status EndingManager
    // ---------------------------------------------------------------

    // Judul ending yang sedang tampil.
    public string EndingSaatIni { get; private set; }

    // Isi cerita ending yang sedang tampil.
    public string IsiEndingSaatIni { get; private set; }

    // Berapa kali pemain menekan "Ya, saya yakin" untuk tawaran C.
    public int jumlahKonfirmasiC = 0;

    // Catatan batas / langkah berikutnya yang ditulis pemain (opsional).
    public string catatanBatas = "";

    private bool pilihanFinalTerbuka = false;

    // ---------------------------------------------------------------
    // Teks (dari naskah BAB VI)
    // ---------------------------------------------------------------

    [Header("Teks Ending")]
    public string judulDenganBatas = "Mulai dengan Batas";
    public string judulPulang = "Pulang, Bukan Selesai";
    public string judulTerkunci = "Kontrak yang Mengunci";

    [TextArea(6, 20)]
    public string isiDenganBatas =
        "Malam hari 7. Raka menekan kirim pada email balasan tawaran. Surat penerimaan "
        + "dicetak dengan catatan batas yang ditulis Raka sendiri.\n\n"
        + "Raka: \"Bu, saya dapat kerja. Mulai minggu depan. Tapi saya catat batasnya: tugasnya "
        + "jelas, lembur perlu persetujuan, dokumen asli saya pegang sendiri.\"\n\n"
        + "Ibu: \"Alhamdulillah. Tiketnya ibu jual balik saja. Kalau ada apa-apa, cerita ke ibu. "
        + "Jangan dipendam.\"\n\n"
        + "Epilog: Raka memulai kerja. Meja di kamar yang lebih rapi, dan satu bingkai kecil "
        + "berisi foto proyek kuliah.\n\n"
        + "Narator: \"Pekerjaan bukan garis finish. Ini mulai, dengan batas yang bisa diucapkan.\"";

    [TextArea(6, 20)]
    public string isiPulang =
        "Pagi hari 8. Raka membawa tas ke pintu. Kamar kos dikosongkan. Satu amplop berisi "
        + "bukti yang benar-benar selesai (E0, E1, E2) dimasukkan ke tas. Bus menuju terminal.\n\n"
        + "Ibu: \"Ka, busnya sudah berangkat? Ibu sudah goreng tempe.\"\n\n"
        + "Raka: \"Sudah, Bu. Saya bawa portofolio yang sudah lengkap. Nanti di rumah saya lanjut "
        + "cari sambil bantu bapak.\"\n\n"
        + "Ibu: \"Iya, tidak apa-apa. Pulang bukan berarti selesai.\"\n\n"
        + "Epilog: Raka menyusun rencana lanjutan bersama keluarga. Tiket terpakai, bukan sebagai "
        + "tanda kekalahan.\n\n"
        + "Narator: \"Tujuh hari tidak menentukan delapan bulan berikutnya.\"";

    [TextArea(6, 20)]
    public string isiTerkunci =
        "Raka menyerahkan dokumen asli.\n\n"
        + "Epilog: komunikasi dengan keluarga dibatasi (hanya pesan singkat), Raka sulit menarik "
        + "diri karena dokumen tertahan. Tidak ada kekerasan grafis dan Raka tidak disalahkan. "
        + "Kamar kos ditinggalkan dengan kunci masih di meja.\n\n"
        + "Narator: \"Kemudahan ada harganya. Beberapa harga baru terlihat setelah dibayar.\"\n\n"
        + "Jika Anda atau orang terdekat menghadapi situasi serupa, hubungi pihak berwenang. "
        + "Game ini fiksi; kontrak dan perusahaan bersifat fiktif.";

    [Header("Peringatan khusus Tawaran C")]
    [TextArea(3, 8)]
    public string peringatanC =
        "Tawaran ini meminta penyerahan dokumen asli dan membatasi pengunduran diri. "
        + "Informasi kontak tidak konsisten.\n\nApakah Anda yakin ingin Raka menandatangani?";

    public string teksTombolYakin = "Ya, saya yakin";

    [Header("Layar Refleksi & Disclaimer (BAB 6.4)")]
    public string[] pertanyaanRefleksi = new string[]
    {
        "Syarat apa yang membuat tawaran terlihat aman atau berisiko?",
        "Apakah Raka masih punya pilihan ketika lamaran ditolak?",
        "Bagian tekanan mana yang berasal dari keputusan pribadi, mana dari kondisi rekrutmen?"
    };

    [TextArea(4, 10)]
    public string disclaimer =
        "Game ini bukan terapi, diagnosis, rehabilitasi, atau pelatihan hukum ketenagakerjaan. "
        + "Kontrak dan perusahaan bersifat fiktif. \"Tekanan\" adalah abstraksi permainan, "
        + "bukan ukuran kondisi mental.";

    // ---------------------------------------------------------------
    // Setup
    // ---------------------------------------------------------------

    void Awake()
    {
        // Jangan rebut Instance dari EndingManager yang lama (kalau scene
        // dimuat ulang, yang lama masih hidup karena GameState persistent).
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        gs.OnStateChanged += PeriksaWaktuFinal;

        SessionManager sm = gs.sesiManager;
        if (sm != null) sm.OnFinalNight += TampilkanPilihanFinal;
    }

    void OnDestroy()
    {
        GameState gs = GameState.Instance;
        if (gs != null) gs.OnStateChanged -= PeriksaWaktuFinal;

        SessionManager sm = gs.sesiManager;
        if (sm != null) sm.OnFinalNight -= TampilkanPilihanFinal;

        if (Instance == this) Instance = null;
    }

    // ---------------------------------------------------------------
    // Keputusan final
    // ---------------------------------------------------------------

    // Dipanggil setiap state berubah. Malam hari ke-7 adalah waktu
    // keputusan final (naskah BAB IV Hari 7).
    private void PeriksaWaktuFinal()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        if (gs.hari == 7 && gs.sesi == GameState.Sesi.Malam)
        {
            TampilkanPilihanFinal();
        }
    }

    // Buka panel keputusan final (A / B / C / Tidak ada).
    public void TampilkanPilihanFinal()
    {
        if (pilihanFinalTerbuka) return;
        pilihanFinalTerbuka = true;

        if (OnPilihanFinalMuncul != null) OnPilihanFinalMuncul();
    }

    // Dipanggil UIManager saat pemain menutup panel keputusan final.
    public void TutupPilihanFinal()
    {
        pilihanFinalTerbuka = false;
    }

    // Dipanggil UIManager saat pemain memilih salah satu tawaran.
    // Kalau pilih C, yang terjadi PERINGATAN SPESIAL dulu (bukan langsung
    // selesai), supaya ada konfirmasi tambahan dua kali.
    public void MintaPilihFinal(GameState.PilihanTawaran pilihan)
    {
        if (pilihan == GameState.PilihanTawaran.C)
        {
            // Tawaran C tidak langsung diproses. Peringatan dulu.
            jumlahKonfirmasiC = 0;
            if (OnPeringatanCMuncul != null) OnPeringatanCMuncul(jumlahKonfirmasiC);
            return;
        }

        SelesaikanPilihan(pilihan);
    }

    // Dipanggil UIManager saat pemain menekan "Ya, saya yakin" pada
    // peringatan tawaran C. Dua kali klik baru ending-nya jalan.
    public void KonfirmasiPeringatanC()
    {
        jumlahKonfirmasiC++;

        if (jumlahKonfirmasiC < 2)
        {
            // Konfirmasi kedua masih dibutuhkan.
            if (OnPeringatanCMuncul != null) OnPeringatanCMuncul(jumlahKonfirmasiC);
            return;
        }

        SelesaikanPilihan(GameState.PilihanTawaran.C);
    }

    // Dipanggil UIManager saat pemain membatalkan peringatan C.
    public void BatalPilihFinal()
    {
        jumlahKonfirmasiC = 0;
        if (OnPeringatanCBatal != null) OnPeringatanCBatal();
    }

    private void SelesaikanPilihan(GameState.PilihanTawaran pilihan)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        // Tawaran A dan B cuma bisa dipilih kalau Raka memang sudah
        // mendapat tawarantya. Kalau belum, pilihan ini nggak ada.
        if (pilihan == GameState.PilihanTawaran.A
            && gs.statusLamaranA != GameState.StatusLamaran.Ditawari)
        {
            gs.KirimNotifikasi("Raka belum dapat tawaran Studio Lajur.");
            return;
        }

        if (pilihan == GameState.PilihanTawaran.B
            && gs.statusLamaranB != GameState.StatusLamaran.Ditawari)
        {
            gs.KirimNotifikasi("Raka belum dapat tawaran Koperasi Pijar.");
            return;
        }

        gs.PilihTawaran(pilihan);
        TampilkanEnding();
    }

    // ---------------------------------------------------------------
    // Menampilkan ending
    // ---------------------------------------------------------------

    // Tampilkan ending sesuai pilihan kontrak pemain.
    //
    // CATATAN PENTING: di method ini TIDAK ADA satu pun pembacaan
    // GameState.tenaga atau GameState.tekanan. Ending murni mengikuti
    // pilihan kontrak.
    public void TampilkanEnding()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        if (gs.pilihanTawaran == GameState.PilihanTawaran.A
            || gs.pilihanTawaran == GameState.PilihanTawaran.B)
        {
            EndingSaatIni = judulDenganBatas;
            IsiEndingSaatIni = isiDenganBatas;
        }
        else if (gs.pilihanTawaran == GameState.PilihanTawaran.C)
        {
            EndingSaatIni = judulTerkunci;
            IsiEndingSaatIni = isiTerkunci;
        }
        else
        {
            EndingSaatIni = judulPulang;
            IsiEndingSaatIni = isiPulang;
        }

        if (OnEndingMuncul != null) OnEndingMuncul();

        SaveSystem.Simpan();
    }

    // Teks lengkap layar akhir: cerita + refleksi + disclaimer.
    public string TeksLayarAkhir()
    {
        string hasil = EndingSaatIni + "\n\n" + IsiEndingSaatIni + "\n\n";

        if (!string.IsNullOrEmpty(catatanBatas))
        {
            hasil += "Catatan Raka: \"" + catatanBatas + "\"\n\n";
        }

        hasil += "---\nPertanyaan refleksi:\n";
        for (int i = 0; i < pertanyaanRefleksi.Length; i++)
        {
            hasil += (i + 1) + ". " + pertanyaanRefleksi[i] + "\n";
        }

        hasil += "\n" + disclaimer;
        return hasil;
    }
}

/* ============================================================
   [CARA PASANG] EndingManager.cs
   1. Taruh script ini di GameObject "Sistem" yang sama dengan GameState.
   2. Semua teks ending sudah ditulis di dalam script, jadi tidak perlu
      diisi manual. Kalau mau mengubah kalimatnya, ubah di Inspector.
   3. UIManager yang berlangganan (subscribe) ke event:
        OnPilihanFinalMuncul -> buka panel keputusan final
        OnPeringatanCMuncul  -> buka layar peringatan khusus tawaran C
        OnPeringatanCBatal   -> kembali ke panel keputusan final
        OnEndingMuncul       -> buka panel ending + refleksi
   4. Tombol "Ya, saya yakin" pada peringatan C harus memanggil
      EndingManager.Instance.KonfirmasiPeringatanC().
      Tombol lain harus memanggil BatalPilihFinal().
   5. Jangan pernah membaca GameState.tenaga atau GameState.tekanan
      di EndingManager. Ending ditentukan oleh pilihan kontrak saja.
   6. Kalau UIManager punya InputField untuk "catatan batas", isi nilainya
      ke EndingManager.Instance.catatanBatas sebelum TampilkanEnding().
   ============================================================ */
