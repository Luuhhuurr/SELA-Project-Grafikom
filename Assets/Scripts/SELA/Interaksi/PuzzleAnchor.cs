using System;
using UnityEngine;

// PuzzleAnchor.cs
// Jangkar Retakan Ruang (BAB 3.3).
//
// Cara mainnya sederhana dan ramah (naskah: tanpa timer, tanpa hukuman):
// - Retakan butuh satu benda tertentu (mis. "KartuProyek").
// - Raka ambil benda itu dulu, lalu tekan E di dekat jangkar.
// - Kalau bendanya salah: muncul petunjuk. Setelah 3 kali salah,
// muncul label tujuan sebagai petunjuk tambahan.
// - Kalau bendanya benar: retakan selesai, ruang kembali normal.
//
// Progres disimpan di GameState.retakanSelesai (bool[3]).
public class PuzzleAnchor : ObjekInteraktif
{
    [Header("Retakan")]
    [Tooltip("Nomor retakan: 0, 1, atau 2. Maksimal 3 retakan dalam game.")]
    [Range(0, 2)] public int indeksRetakan = 0;

    [Tooltip("Hari paling awal retakan ini muncul. Retakan 1 hari 2, "
           + "retakan 2 hari 4, retakan 3 hari 6.")]
    public int hariMinimal = 1;

    [Tooltip("Nama benda yang harus dibawa Raka. Harus PERSIS sama dengan "
           + "nama benda di objek pengambilan (mis. 'KartuProyek').")]
    public string namaBendaYangDibutuhkan = "";

    [Header("Tampilan")]
    [Tooltip("Mesh retakan yang akan hilang kalau puzzle selesai. Boleh kosong.")]
    public GameObject objekEfek;

    [Tooltip("Label tujuan yang muncul setelah 3 kali percobaan salah. Boleh kosong.")]
    public GameObject labelTujuan;

    [Header("Pesan")]
    [Tooltip("Pesan yang muncul saat Raka memakai benda yang salah.")]
    public string pesanBendaSalah = "Benda ini tidak cocok dengan retakan.";

    [Tooltip("Pesan yang muncul saat retakan berhasil diselesaikan.")]
    public string pesanBerhasil = "Retakan ruang tertutup. Ruang kembali normal.";

    // Berapa kali Raka salah memakai benda di jangkar ini.
    public int jumlahPercobaanSalah = 0;

    // Dipanggil saat retakan solved. Parameter = indeks retakan.
    public event Action<int> OnRetakanSelesai;

    public override void Interact()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return;

        // Retakan baru muncul di hari yang ditentukan naskah.
        if (gs.hari < hariMinimal)
        {
            gs.KirimNotifikasi("Belum ada apa-apa yang aneh di ruang ini hari ini.");
            return;
        }

        // Sudah selesai? kasih tahu saja.
        if (indeksRetakan >= 0 && indeksRetakan < gs.retakanSelesai.Length
            && gs.retakanSelesai[indeksRetakan])
        {
            gs.KirimNotifikasi("Retakan ini sudah tertutup.");
            return;
        }

        SimpleInventory tas = gs.inventory;
        if (tas == null) return;

        // Benda cocok?
        if (tas.PunyaBenda(namaBendaYangDibutuhkan))
        {
            Selesaikan(gs, tas);
        }
        else
        {
            Gagal(gs);
        }
    }

    private void Selesaikan(GameState gs, SimpleInventory tas)
    {
        if (indeksRetakan >= 0 && indeksRetakan < gs.retakanSelesai.Length)
        {
            gs.retakanSelesai[indeksRetakan] = true;
        }

        // Benda habis dipakai.
        tas.PasangBenda();

        // Efek retakan hilang.
        if (objekEfek != null) objekEfek.SetActive(false);
        if (labelTujuan != null) labelTujuan.SetActive(false);

        sudahDigunakan = true;
        MatikanCollider();

        gs.KirimNotifikasi(pesanBerhasil);

        if (OnRetakanSelesai != null) OnRetakanSelesai(indeksRetakan);

        SaveSystem.Simpan();
    }

    private void Gagal(GameState gs)
    {
        jumlahPercobaanSalah++;

        string pesan = pesanBendaSalah;
        if (jumlahPercobaanSalah >= 3)
        {
            // Setelah tiga kali salah, tampilkan petunjuk tujuan.
            if (labelTujuan != null) labelTujuan.SetActive(true);
            pesan = pesan + " (Cari petunjuk tujuan di dekat sini.)";
        }

        gs.KirimNotifikasi(pesan);
    }

    public override string TeksPrompt()
    {
        if (sudahDigunakan) return "";

        GameState gs = GameState.Instance;
        if (gs != null && gs.hari < hariMinimal) return "";

        return "[E] " + namaTampilan;
    }
}

/* ============================================================
   [CARA PASANG] PuzzleAnchor.cs
   1. Buat GameObject jangkar, misalnya "Retakan1_Anchor".
   2. Tambahkan Collider (Is Trigger boleh) di GameObject ini.
   3. Drag script PuzzleAnchor.cs ke GameObject itu.
   4. Isi kolom:
        Nama Tampilan          : contoh "Pasang di retakan"
        Objek Visual           : mesh jangkar (boleh kosong)
        Indeks Retakan         : 0 untuk retakan 1, 1 untuk retakan 2,
                                 2 untuk retakan 3
        Nama Benda Yang Dibutuhkan : "KartuProyek" / "Kunci07" / "KartuIbu"
        Objek Efek             : mesh retakan (yang hilang kalau selesai)
        Label Tujuan           : GameObject teks/label petunjuk
        Pesan Benda Salah      : teks petunjuk saat salah
        Pesan Berhasil         : teks saat retakan tertutup
        Ambil Sebagai Benda    : JANGAN centang
   5. PENTING: "Nama Benda Yang Dibutuhkan" harus sama persis dengan
      kolom "Nama Benda" pada objek pengambilan bendanya.
      Contoh pasangan yang harus cocok:
        Objek pengambilan  ->  Nama Benda = "KartuProyek"
        Jangkar retakan 1  ->  Nama Benda Yang Dibutuhkan = "KartuProyek"
   6. Kalau mau tau retakan yang mana yang selesai, subscribe ke
      event PuzzleAnchor.OnRetakanSelesai.
   7. Tidak ada timer dan tidak ada hukuman, jadi pemain tidak bisa gagal
      secara permanen.
   ============================================================ */
