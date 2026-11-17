using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// UjiTujuhHariPlayMode.cs
// Uji main penuh: dari menu awal sampai layar ending, selama tujuh hari.
//
// Yang dicek:
// 1. Lamaran tidak bisa dikirim kalau CV-nya belum dirakit.
// 2. Bukti E1 dan E2 bisa didapat lewat aktivitas yang tepat.
// 3. Lamaran A dan B terkirim, interview jalan, dan dua tawaran didapat.
// 4. Retakan Ruang baru muncul di hari yang ditentukan naskah.
// 5. Malam hari ke-7 membuka panel keputusan akhir.
// 6. Memilih tawaran A menampilkan layar ending yang benar.
//
// Cara menjalankan (dari terminal):
// Unity.exe -batchmode -runTests -testPlatform PlayMode \
// -projectPath &lt;path project&gt; -testResults hasil.xml
public class UjiTujuhHariPlayMode
{
    [UnityTest]
    public IEnumerator TujuhHari_SampaiLayarEnding()
    {
        // 1. Save diarahkan ke folder sementara supaya file save asli
        //    milik pemain tidak ikut tersentuh.
        string folderUji = Path.Combine(Path.GetTempPath(), "sela_uji_tujuh_hari");
        if (Directory.Exists(folderUji)) Directory.Delete(folderUji, true);
        SaveSystem.FolderSimpanKustom = folderUji;

        SceneManager.LoadScene("SELA_Play");
        yield return null;
        yield return null;
        yield return null;

        GameState gs = GameState.Instance;
        UIManager ui = UIManager.Instance;
        Assert.IsNotNull(gs, "GameState tidak ditemukan.");
        Assert.IsNotNull(ui, "UIManager tidak ditemukan.");

        ui.MulaiBaru();
        yield return null;

        Assert.AreEqual(1, gs.hari, "Permainan baru harus mulai dari hari 1.");
        Assert.AreEqual(GameState.Sesi.Pagi, gs.sesi, "Hari 1 harus dimulai sesi pagi.");

        // ================================================================
        // Hari 1
        // ================================================================

        // Rapikan antarmuka -> dapat E1.
        ActivityData rapikan = CariAktivitas("Rapikan Contoh Antarmuka");
        Jalankan(gs, rapikan);
        Assert.IsTrue(gs.PunyaBukti("E1"), "Bukti E1 tidak didapat dari rapikan antarmuka.");
        Assert.AreEqual(GameState.Sesi.Siang, gs.sesi, "Sesi harus bergeser setelah aktivitas.");

        // Lamaran belum bisa dikirim: CV-nya belum dirakit.
        ActivityData kirimA = CariAktivitas("Kirim Lamaran ke Studio Lajur");
        string alasan = "";
        Assert.IsFalse(gs.activitySystem.CekSyarat(kirimA, out alasan),
            "Lamaran harus ditolak kalau CV belum dirakit.");
        StringAssert.Contains("Rakit CV", alasan, "Alasan penolakan harus menyebut rakit CV.");

        // Rakit CV: dua klaim yang wajar dan bukti E1.
        gs.cvSystem.AturKlaim(1, CVSystem.Klaim1Sesuai);
        gs.cvSystem.AturKlaim(2, CVSystem.Klaim2Sesuai);
        gs.cvSystem.LampirkanBukti("E1");

        Assert.IsTrue(gs.activitySystem.CekSyarat(kirimA, out alasan),
            "Lamaran harus boleh dikirim setelah CV dirakit. " + alasan);

        // Kirim lamaran A (jendela lamaran A: hari 1 sampai hari 3).
        Jalankan(gs, kirimA);
        Assert.AreEqual(GameState.StatusLamaran.Terjadwal, gs.statusLamaranA,
            "Lamaran A harus berstatus terjadwal.");
        Assert.AreEqual(5, gs.jadwalInterviewA, "Interview A dijadwalkan hari 5.");
        Assert.IsNotNull(gs.cvSystem.AmbilSnapshot("A"), "Snapshot lamaran A tidak tersimpan.");
        Assert.AreEqual(1, gs.cvSystem.AmbilSnapshot("A").buktiTerpasang.Count,
            "Snapshot harus menyimpan bukti saat dikirim.");

        // Retakan 1 belum muncul di hari 1.
        PuzzleAnchor rift1 = CariRetakan(0);
        Assert.AreEqual("", rift1.TeksPrompt(),
            "Retakan 1 tidak boleh muncul sebelum hari 2.");

        // Tidur ke hari 2.
        Tidur(gs);
        Assert.AreEqual(2, gs.hari, "Setelah tidur harus masuk hari 2.");

        // ================================================================
        // Hari 2
        // ================================================================

        // Ambil kartu proyek E0, lalu pasang di penjepit foto.
        EvidenceCard kartu = Object.FindAnyObjectByType<EvidenceCard>();
        Assert.IsNotNull(kartu, "Kartu bukti E0 tidak ada di scene.");
        kartu.Interact();
        yield return null;
        Assert.AreEqual("KartuE0", gs.inventory.bendaSaatIni, "Kartu E0 tidak masuk tas.");

        rift1 = CariRetakan(0);
        Assert.AreNotEqual("", rift1.TeksPrompt(), "Retakan 1 harus muncul di hari 2.");

        rift1.Interact();
        yield return null;
        Assert.IsTrue(gs.retakanSelesai[0], "Retakan 1 tidak tertutup.");
        Assert.AreEqual("", gs.inventory.bendaSaatIni, "Kartu E0 harus habis dipakai.");

        // Istirahat supaya tenaga cukup untuk hari-hari berikutnya.
        Jalankan(gs, CariAktivitas("Istirahat"));
        Tidur(gs);
        Assert.AreEqual(3, gs.hari, "Setelah tidur harus masuk hari 3.");

        // ================================================================
        // Hari 3
        // ================================================================

        // Lengkapi catatan uji -> dapat E2.
        Jalankan(gs, CariAktivitas("Lengkapi Catatan Pengujian"));
        Assert.IsTrue(gs.PunyaBukti("E2"), "Bukti E2 tidak didapat dari catatan pengujian.");

        // Lamaran B butuh E1 dan E2 ikut dikirim.
        gs.cvSystem.LampirkanBukti("E2");

        ActivityData kirimB = CariAktivitas("Kirim Lamaran ke Koperasi Pijar");
        alasan = "";
        Assert.IsTrue(gs.activitySystem.CekSyarat(kirimB, out alasan),
            "Lamaran B harus boleh dikirim di hari 3. " + alasan);

        Jalankan(gs, kirimB);
        Assert.AreEqual(GameState.StatusLamaran.Terjadwal, gs.statusLamaranB,
            "Lamaran B harus berstatus terjadwal.");
        Assert.AreEqual(6, gs.jadwalInterviewB, "Interview B dijadwalkan hari 6.");

        Tidur(gs);
        Assert.AreEqual(4, gs.hari, "Setelah tidur harus masuk hari 4.");

        // ================================================================
        // Hari 4
        // ================================================================

        // Cek lowongan Akses Prima lewat panel verifikasi.
        ActivityData periksaC = CariAktivitas("Periksa Akses Prima");
        ui.MintaJalankanAktivitas(periksaC);
        yield return null;
        Assert.IsTrue(ui.panelVerifikasi.activeSelf, "Panel verifikasi tidak terbuka.");
        StringAssert.Contains("Akses Prima", ui.teksVerifikasi.text,
            "Panel verifikasi tidak menampilkan nama perusahaan.");
        StringAssert.Contains("Berisiko", ui.teksVerifikasi.text,
            "Panel verifikasi tidak menampilkan tanda aspek.");

        ui.TutupSemuaPanel();
        yield return null;

        // Verifikasi spending satu sesi. Jalankan lewat sistem.
        Jalankan(gs, periksaC);
        Assert.IsTrue(gs.verifikasiSudah[2], "Verifikasi Akses Prima tidak tercatat.");

        Jalankan(gs, CariAktivitas("Istirahat"));
        Tidur(gs);
        Assert.AreEqual(5, gs.hari, "Setelah tidur harus masuk hari 5.");

        // ================================================================
        // Hari 5
        // ================================================================

        // Interview A cuma boleh di sesi siang, jadi pakai satu sesi pagi dulu.
        Jalankan(gs, CariAktivitas("Duduk Tanpa Layar"));
        Assert.AreEqual(GameState.Sesi.Siang, gs.sesi,
            "Interview harus dijalankan di sesi siang.");

        ActivityData interviewA = CariAktivitas("Ikuti Interview Studio Lajur");
        alasan = "";
        Assert.IsTrue(gs.activitySystem.CekSyarat(interviewA, out alasan),
            "Interview A harus boleh dijalankan. " + alasan);

        Jalankan(gs, interviewA);
        yield return null;

        // Jawab dialog: koreksi klaim, lalu dua jawaban terbaik.
        InterviewManager im = gs.interviewManager;
        Assert.IsNotNull(im, "InterviewManager tidak ada.");
        im.PilihOpsi(0);
        im.PilihOpsi(0);
        im.PilihOpsi(0);
        im.Lanjut();
        yield return null;

        Assert.AreEqual(GameState.StatusLamaran.Ditawari, gs.statusLamaranA,
            "Interview A harus menghasilkan tawaran.");

        Tidur(gs);
        Assert.AreEqual(6, gs.hari, "Setelah tidur harus masuk hari 6.");

        // ================================================================
        // Hari 6
        // ================================================================

        Jalankan(gs, CariAktivitas("Duduk Tanpa Layar"));
        Assert.AreEqual(GameState.Sesi.Siang, gs.sesi, "Interview B harus di sesi siang.");

        Jalankan(gs, CariAktivitas("Ikuti Interview Koperasi Pijar"));
        yield return null;

        im.PilihOpsi(0);
        im.PilihOpsi(0);
        im.PilihOpsi(0);
        im.Lanjut();
        yield return null;

        Assert.AreEqual(GameState.StatusLamaran.Ditawari, gs.statusLamaranB,
            "Interview B harus menghasilkan tawaran.");

        Tidur(gs);
        Assert.AreEqual(7, gs.hari, "Setelah tidur harus masuk hari 7.");

        // ================================================================
        // Hari 7
        // ================================================================

        // Dua kegiatan supaya malam hari 7 tercapai. Duduk tanpa layar
        // cuma boleh sekali sehari, jadi sesi siang pakai Rapikan Meja.
        Jalankan(gs, CariAktivitas("Duduk Tanpa Layar"));
        Jalankan(gs, CariAktivitas("Rapikan Meja"));
        Assert.AreEqual(GameState.Sesi.Malam, gs.sesi, "Harus sampai sesi malam hari 7.");
        yield return null;

        Assert.IsTrue(ui.panelPilihanFinal.activeSelf,
            "Panel keputusan akhir tidak terbuka di malam hari ke-7.");

        // A dan B sama-sama ditawarkan, jadi keduanya boleh dipilih.
        string isiFinal = ui.teksPilihanFinal.text;
        StringAssert.Contains("A. Terima tawaran Studio Lajur", isiFinal,
            "Tawaran A belum didapat, jadi tidak boleh dipilih.");
        StringAssert.Contains("B. Terima tawaran Koperasi Pijar", isiFinal,
            "Tawaran B belum didapat, jadi tidak boleh dipilih.");

        // Pilih tawaran A.
        EndingManager em = EndingManager.Instance;
        Assert.IsNotNull(em, "EndingManager tidak ditemukan.");
        em.MintaPilihFinal(GameState.PilihanTawaran.A);
        yield return null;

        Assert.AreEqual(GameState.PilihanTawaran.A, gs.pilihanTawaran,
            "Pilihan kontrak tidak tersimpan.");
        Assert.IsTrue(ui.panelEnding.activeSelf, "Layar ending tidak muncul.");
        StringAssert.Contains("Batas", ui.teksEnding.text,
            "Layar ending untuk tawaran A memakai judul yang salah.");

        // Tawaran yang belum didapat tidak boleh dipilih.
        gs.statusLamaranB = GameState.StatusLamaran.BelumDikirim;
        em.MintaPilihFinal(GameState.PilihanTawaran.B);
        yield return null;
        Assert.AreEqual(GameState.PilihanTawaran.A, gs.pilihanTawaran,
            "Tawaran B tidak boleh dipilih tanpa interview.");

        LogAssert.NoUnexpectedReceived();

        SaveSystem.FolderSimpanKustom = "";
        if (Directory.Exists(folderUji)) Directory.Delete(folderUji, true);
    }

    // ---------------------------------------------------------------
    // Helper
    // ---------------------------------------------------------------

    private static ActivityData CariAktivitas(string nama)
    {
        GameState gs = GameState.Instance;
        for (int i = 0; i < gs.activitySystem.daftarAktivitas.Count; i++)
        {
            if (gs.activitySystem.daftarAktivitas[i].nama == nama)
            {
                return gs.activitySystem.daftarAktivitas[i];
            }
        }

        Assert.Fail("Aktivitas '" + nama + "' tidak ada di daftar aktivitas.");
        return null;
    }

    private static PuzzleAnchor CariRetakan(int indeks)
    {
        PuzzleAnchor[] semua = Object.FindObjectsByType<PuzzleAnchor>(FindObjectsSortMode.None);
        for (int i = 0; i < semua.Length; i++)
        {
            if (semua[i].indeksRetakan == indeks) return semua[i];
        }

        Assert.Fail("Jangkar retakan " + indeks + " tidak ada di scene.");
        return null;
    }

    private static void Jalankan(GameState gs, ActivityData aktivitas)
    {
        gs.activitySystem.JalankanAktivitas(aktivitas);
    }

    private static void Tidur(GameState gs)
    {
        gs.sesiManager.Tidur();
    }
}
