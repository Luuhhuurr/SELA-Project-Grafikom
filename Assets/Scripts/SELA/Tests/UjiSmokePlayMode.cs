using System.Collections;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// UjiSmokePlayMode.cs
// Uji asap (smoke test) untuk game SELA.
//
// Uji ini memuat scene demo SELA_Play dalam mode Play, lalu memastikan:
// 1. Semua sistem (GameState, Session, Activity, CV, Interview, Inventory) ada.
// 2. Semua panel dan teks UI ditemukan (tidak ada panel yang gagal dicari).
// 3. Font TextMeshPro tersedia, kalau tidak teks tidak akan tampil.
// 4. Membuka panel Catatan mengunci gerak pemain, menutupnya melepas kunci.
// 5. Mengambil kartu bukti menambah ID bukti dan isi tas.
// 6. Menjalankan aktivitas memotong tenaga dan menambah tekanan.
// 7. Tidak ada error/exception yang muncul selama semua langkah di atas.
//
// Cara menjalankan (dari terminal):
// Unity.exe -batchmode -runTests -testPlatform PlayMode \
// -projectPath <path project> -testResults hasil.xml
//
// ATAU dari Editor: Window > General > Test Runner > PlayMode > Run All.
public class UjiSmokePlayMode
{
    [UnityTest]
    public IEnumerator SceneDemo_BisaDimuatDanSistemBekerja()
    {
        // 1. Arahkan file save ke folder sementara. Jadi file save asli
        //    milik pemain di persistentDataPath tidak ikut tersentuh.
        //    Folder juga dibersihkan dulu supaya uji selalu mulai dari
        //    keadaan awal (hari 1, tenaga penuh).
        string folderUji = Path.Combine(Path.GetTempPath(), "sela_uji_asap");
        if (Directory.Exists(folderUji)) Directory.Delete(folderUji, true);
        SaveSystem.FolderSimpanKustom = folderUji;

        // 2. Muat scene demo.
        SceneManager.LoadScene("SELA_Play");
        yield return null;
        yield return null;
        yield return null;

        // 3. GameState dan seluruh sistemnya harus terisi.
        GameState gs = GameState.Instance;
        Assert.IsNotNull(gs, "GameState.Instance tidak ditemukan.");
        Assert.IsNotNull(gs.sesiManager, "SessionManager tidak terpasang.");
        Assert.IsNotNull(gs.activitySystem, "ActivitySystem tidak terpasang.");
        Assert.IsNotNull(gs.cvSystem, "CVSystem tidak terpasang.");
        Assert.IsNotNull(gs.interviewManager, "InterviewManager tidak terpasang.");
        Assert.IsNotNull(gs.inventory, "SimpleInventory tidak terpasang.");

        // 4. UI harus menemukan semua panel dan teks.
        UIManager ui = UIManager.Instance;
        Assert.IsNotNull(ui, "UIManager tidak ditemukan.");

        Assert.IsNotNull(ui.panelHUD, "PanelHUD tidak ditemukan.");
        Assert.IsNotNull(ui.panelPrompt, "PanelPrompt tidak ditemukan.");
        Assert.IsNotNull(ui.panelNotifikasi, "PanelNotifikasi tidak ditemukan.");
        Assert.IsNotNull(ui.panelDialog, "PanelDialog tidak ditemukan.");
        Assert.IsNotNull(ui.panelCatatan, "PanelCatatan tidak ditemukan.");
        Assert.IsNotNull(ui.panelLaptop, "PanelLaptop tidak ditemukan.");
        Assert.IsNotNull(ui.panelKonfirmasi, "PanelKonfirmasiAktivitas tidak ditemukan.");
        Assert.IsNotNull(ui.panelPause, "PanelPause tidak ditemukan.");
        Assert.IsNotNull(ui.panelJeda, "PanelJeda tidak ditemukan.");
        Assert.IsNotNull(ui.panelPilihanFinal, "PanelPilihanFinal tidak ditemukan.");
        Assert.IsNotNull(ui.panelPeringatanC, "PanelPeringatanC tidak ditemukan.");
        Assert.IsNotNull(ui.panelEnding, "PanelEnding tidak ditemukan.");

        Assert.IsNotNull(ui.teksHUD, "Teks HUD tidak ditemukan.");
        Assert.IsNotNull(ui.teksPrompt, "Teks prompt tidak ditemukan.");
        Assert.IsNotNull(ui.teksNotifikasi, "Teks notifikasi tidak ditemukan.");
        Assert.IsNotNull(ui.teksDialogPembicara, "Teks pembicara dialog tidak ditemukan.");
        Assert.IsNotNull(ui.teksDialogTeks, "Teks dialog tidak ditemukan.");
        Assert.IsNotNull(ui.teksCatatan, "Teks catatan tidak ditemukan.");
        Assert.IsNotNull(ui.teksLaptop, "Teks laptop tidak ditemukan.");
        Assert.IsNotNull(ui.teksKonfirmasi, "Teks konfirmasi tidak ditemukan.");
        Assert.IsNotNull(ui.teksPause, "Teks pause tidak ditemukan.");
        Assert.IsNotNull(ui.teksJeda, "Teks jeda tidak ditemukan.");
        Assert.IsNotNull(ui.teksPilihanFinal, "Teks pilihan final tidak ditemukan.");
        Assert.IsNotNull(ui.teksPeringatanC, "Teks peringatan C tidak ditemukan.");
        Assert.IsNotNull(ui.teksEnding, "Teks ending tidak ditemukan.");

        // Font default TextMeshPro wajib ada, kalau tidak teks tidak tampil.
        Assert.IsNotNull(TMP_Settings.defaultFontAsset,
            "Font TextMeshPro default belum di-import (TMP Essential Resources).");

        // 5. Pemain harus punya CharacterController dan kamera.
        FirstPersonController fpc = ui.firstPersonController;
        Assert.IsNotNull(fpc, "FirstPersonController tidak terhubung ke UIManager.");
        Assert.IsNotNull(fpc.GetComponent<CharacterController>(),
            "Pemain tidak punya CharacterController.");
        Camera kamera = Camera.main;
        Assert.IsNotNull(kamera, "Kamera utama tidak ditemukan.");

        // 6. Daftar aktivitas harus terisi dari asset ActivityData.
        Assert.GreaterOrEqual(gs.activitySystem.daftarAktivitas.Count, 10,
            "Daftar aktivitas kosong. Asset .asset belum dibuat?");

        // 7. Menu awal harus muncul saat game dibuka dan mengunci pemain.
        Assert.IsNotNull(ui.panelMenuAwal, "PanelMenuAwal tidak ditemukan.");
        Assert.IsTrue(ui.panelMenuAwal.activeSelf, "Menu awal tidak tampil.");
        Assert.IsTrue(fpc.uiTerbuka, "Pemain masih bisa jalan saat menu awal terbuka.");

        // Tekan "Mulai Game": menu tertutup, pemain boleh jalan lagi.
        ui.MulaiGame();
        yield return null;
        Assert.IsFalse(ui.panelMenuAwal.activeSelf, "Menu awal tidak tertutup.");
        Assert.IsFalse(fpc.uiTerbuka, "Pemain masih terkunci setelah menu ditutup.");

        // 7b. Pemain harus benar-benar berdiri di lantai.
        // Kalau gravitasi manualnya nggak jalan, pemain melayang.
        CharacterController karakterPemain = fpc.GetComponent<CharacterController>();
        yield return new WaitForSeconds(0.5f);

        Assert.Less(karakterPemain.transform.position.y, 0.25f,
            "Pemain masih melayang di atas lantai. y = " + karakterPemain.transform.position.y);
        Assert.Greater(karakterPemain.transform.position.y, -0.5f,
            "Pemain jatuh menembus lantai. y = " + karakterPemain.transform.position.y);

        // 7c. Angkat pemain, dia harus turun sendiri karena gravitasi.
        Vector3 posisiAwal = karakterPemain.transform.position;
        karakterPemain.transform.position = new Vector3(posisiAwal.x, posisiAwal.y + 1.5f, posisiAwal.z);
        yield return new WaitForSeconds(0.8f);

        Assert.Less(karakterPemain.transform.position.y, 0.35f,
            "Pemain melayang, gravitasi manual tidak bekerja. y = " + karakterPemain.transform.position.y);
        Assert.Greater(karakterPemain.transform.position.y, -0.5f,
            "Pemain jatuh menembus lantai. y = " + karakterPemain.transform.position.y);

        // 7d. Tidak boleh ada perabot yang menelan pemain di posisi awal.
        Collider[] nabok = Physics.OverlapSphere(
            karakterPemain.transform.position + new Vector3(0f, 0.9f, 0f), 0.2f);
        for (int i = 0; i < nabok.Length; i++)
        {
            if (nabok[i] is CharacterController) continue;
            Assert.Fail("Pemain berada di dalam '" + nabok[i].gameObject.name + "' saat mulai game.");
        }

        // 8. Membuka panel Catatan harus mengunci gerak; menutupnya melepas.
        ui.BukaPanelCatatan();
        yield return null;
        Assert.IsTrue(fpc.uiTerbuka, "uiTerbuka tidak aktif saat panel terbuka.");
        Assert.IsTrue(ui.panelCatatan.activeSelf, "PanelCatatan tidak terbuka.");
        StringAssert.Contains("Kartu Proyek (E0)", ui.teksCatatan.text,
            "Catatan tidak menampilkan kartu proyek E0.");

        // 8a. Catatan harus berisi tujuan harian dan daftar pekerjaan,
        //     supaya pemain tahu harus ngapain.
        StringAssert.Contains("TUJUAN HARI INI", ui.teksCatatan.text,
            "Catatan tidak menampilkan bagian tujuan harian.");
        StringAssert.Contains("DAFTAR PEKERJAAN", ui.teksCatatan.text,
            "Catatan tidak menampilkan daftar pekerjaan yang belum beres.");
        StringAssert.Contains("[ ]", ui.teksCatatan.text,
            "Checklist di Catatan tidak menandai pekerjaan yang belum selesai.");

        // 8b. HUD harus selalu menampilkan tujuan harian.
        Assert.IsNotNull(ui.teksTujuan, "Teks tujuan di HUD tidak ditemukan.");
        StringAssert.Contains("TUJUAN", ui.teksTujuan.text,
            "HUD tidak menampilkan tujuan harian.");
        StringAssert.DoesNotContain("TUJUAN: ...", ui.teksTujuan.text,
            "Tujuan di HUD belum dihitung.");

        ui.TutupSemuaPanel();
        yield return null;
        Assert.IsFalse(fpc.uiTerbuka, "uiTerbuka masih aktif setelah panel ditutup.");

        // 8b. Laptop harus punya 3 tab dan isinya diambil dari data naskah.
        ui.BukaPanelLaptop();
        yield return null;
        Assert.IsTrue(ui.panelLaptop.activeSelf, "PanelLaptop tidak terbuka.");

        ui.GantiTabLaptop(0);
        yield return null;
        StringAssert.Contains("Studio Lajur", ui.teksLaptop.text,
            "Tab Lamaran tidak menampilkan draf lowongan dari data naskah.");

        ui.GantiTabLaptop(1);
        yield return null;
        StringAssert.Contains("Ibu", ui.teksLaptop.text,
            "Tab Pesan tidak menampilkan pesan pembuka dari Ibu.");
        StringAssert.Contains("tiket bus", ui.teksLaptop.text,
            "Isi pesan Ibu tidak termuat dari data naskah.");

        ui.GantiTabLaptop(2);
        yield return null;
        StringAssert.Contains("Kartu Proyek (E0)", ui.teksLaptop.text,
            "Tab Catatan di laptop tidak menampilkan kartu proyek E0.");

        ui.TutupSemuaPanel();
        yield return null;

        // 9. Mengambil kartu bukti menambah ID bukti dan mengisi tas.
        EvidenceCard kartu = Object.FindAnyObjectByType<EvidenceCard>();
        Assert.IsNotNull(kartu, "Kartu bukti E0 tidak ada di scene.");
        kartu.Interact();
        yield return null;
        Assert.IsTrue(gs.PunyaBukti("E0"), "Bukti E0 tidak masuk daftar bukti.");
        Assert.AreEqual("KartuE0", gs.inventory.bendaSaatIni,
            "Kartu E0 tidak masuk tas Raka.");

        // 9. Menjalankan aktivitas harus memotong tenaga dan menambah tekanan.
        int tenagaAwal = gs.tenaga;
        int tekananAwal = gs.tekanan;
        GameState.Sesi sesiAwal = gs.sesi;

        // Cari aktivitas biasa yang syaratnya bisa dipenuhi sekarang.
        ActivityData aktivitas = null;
        string alasan = "";
        for (int i = 0; i < gs.activitySystem.daftarAktivitas.Count; i++)
        {
            ActivityData calon = gs.activitySystem.daftarAktivitas[i];
            if (calon.tipe != ActivityData.Tipe.Biasa) continue;
            if (calon.biayaTenaga <= 0) continue;
            if (!gs.activitySystem.CekSyarat(calon, out alasan)) continue;
            aktivitas = calon;
            break;
        }
        Assert.IsNotNull(aktivitas,
            "Tidak ada aktivitas biasa yang bisa dijalankan. " + alasan);

        // Panel konfirmasi harus menampilkan nama aktivitas, perubahan
        // stat tenaga/tekanan, dan sesi berikutnya.
        gs.activitySystem.MintaJalankan(aktivitas);
        yield return null;
        Assert.IsTrue(ui.panelKonfirmasi.activeSelf, "Panel konfirmasi tidak terbuka.");
        StringAssert.Contains("Aktivitas:", ui.teksKonfirmasi.text,
            "Panel konfirmasi tidak menampilkan nama aktivitas.");
        StringAssert.Contains(aktivitas.nama, ui.teksKonfirmasi.text,
            "Nama aktivitas tidak muncul di panel konfirmasi.");
        StringAssert.Contains("Tenaga:", ui.teksKonfirmasi.text,
            "Panel konfirmasi tidak menampilkan perubahan tenaga.");
        StringAssert.Contains("Tekanan:", ui.teksKonfirmasi.text,
            "Panel konfirmasi tidak menampilkan perubahan tekanan.");
        StringAssert.Contains("Sesi berikutnya:", ui.teksKonfirmasi.text,
            "Panel konfirmasi tidak menampilkan sesi berikutnya.");

        ui.TutupSemuaPanel();
        yield return null;

        gs.activitySystem.JalankanAktivitas(aktivitas);
        yield return null;

        Assert.AreEqual(tenagaAwal - aktivitas.biayaTenaga, gs.tenaga,
            "Tenaga tidak berkurang sesuai biaya aktivitas.");
        Assert.AreEqual(tekananAwal + aktivitas.biayaTekanan, gs.tekanan,
            "Tekanan tidak bertambah sesuai biaya aktivitas.");

        // 10. Sesi harus berpindah setelah satu aktivitas selesai.
        Assert.AreNotEqual(sesiAwal, gs.sesi, "Sesi tidak berpindah setelah aktivitas.");

        // 11. File save harus muncul di folder simpan.
        Assert.IsTrue(SaveSystem.AdaSave(), "File save tidak dibuat.");

        // 12. Tidak boleh ada error log selama seluruh langkah di atas.
        LogAssert.NoUnexpectedReceived();

        // 13. Kembalikan folder simpan ke settingan semula.
        SaveSystem.FolderSimpanKustom = "";
        if (Directory.Exists(folderUji)) Directory.Delete(folderUji, true);
    }
}
