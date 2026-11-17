using System.Collections.Generic;
using UnityEngine;

// TujuanGame.cs
// Menentukan "Raka harus ngapain sekarang".
//
// Semua isinya dihitung dari GameState, jadi tidak ada angka acak dan
// tidak ada urutan yang disembunyikan. Kalau players bingung, tinggal
// nampilin teks dari fungsi-fungsi di bawah.
//
// Aturan:
//   - Satu baris tujuan utama buat HUD (dipakai UIManager).
//   - Daftar checklist buat panel Catatan, jadi pemain bisa lihat
//     apa yang sudah beres dan apa yang belum.

public static class TujuanGame
{
    // Nama perusahaan buat dibaca manusia.
    private static string NamaPerusahaan(string kode)
    {
        if (kode == "A") return "Studio Lajur";
        if (kode == "B") return "Koperasi Pijar";
        return kode;
    }

    // ---------------------------------------------------------------
    // Tujuan utama
    // ---------------------------------------------------------------

    // Satu baris: apa yang harus dilakukan sekarang.
    public static string Tujuan(GameState gs)
    {
        if (gs == null) return "";

        // Malam terakhir: tinggal pilih.
        if (gs.hari == 7 && gs.sesi == GameState.Sesi.Malam)
        {
            return "Malam terakhir. Pilih tawarannya sekarang.";
        }

        // Interview hari ini (dijadwalkan untuk hari ini).
        string interviewHariIni = InterviewHariIni(gs);
        if (!string.IsNullOrEmpty(interviewHariIni)) return interviewHariIni;

        // Lamaran A ditutup hari 3 malam. Kalau lewat, kasih tahu.
        if (gs.statusLamaranA == GameState.StatusLamaran.BelumDikirim && gs.hari > 3)
        {
            return "Lamaran Studio Lajur sudah lewat batas. Fokus ke Koperasi Pijar.";
        }

        // Bukti E1 dulu, karena lamaran butuh bukti di CV.
        if (!gs.PunyaBukti("E1"))
        {
            return "Rapikan Contoh Antarmuka di laptop (tombol L) buat dapat bukti E1.";
        }

        // Lamaran A.
        if (gs.statusLamaranA == GameState.StatusLamaran.BelumDikirim)
        {
            return "Rakit CV, lalu kirim lamaran ke Studio Lajur. Batasnya malam hari 3.";
        }

        // Bukti E2 buat lamaran B.
        if (!gs.PunyaBukti("E2") && gs.hari >= 3)
        {
            return "Lengkapi Catatan Pengujian di laptop buat dapat bukti E2.";
        }

        // Lamaran B ditutup hari 5 malam.
        if (gs.statusLamaranB == GameState.StatusLamaran.BelumDikirim)
        {
            if (gs.hari > 5) return "Lamaran Koperasi Pijar sudah lewat batas.";
            return "Kirim lamaran ke Koperasi Pijar. Butuh E1 dan E2. Batasnya malam hari 5.";
        }

        // Retakan yang harus dibuka hari ini.
        string retakanHariIni = RetakanHariIni(gs);
        if (!string.IsNullOrEmpty(retakanHariIni)) return retakanHariIni;

        // Hari 4: Akses Prima baru muncul, dan dalamnya itu penting.
        if (gs.hari >= 4 && !gs.verifikasiSudah[2])
        {
            return "Lowongan Akses Prima baru muncul. Cek dulu di laptop (Periksa Akses Prima).";
        }

        // Tawaran sudah dapat, tinggal nunggu hari terakhir.
        if (gs.statusLamaranA == GameState.StatusLamaran.Ditawari
            || gs.statusLamaranB == GameState.StatusLamaran.Ditawari)
        {
            return "Tawaran udah masuk. Sisa hari buat istirahat, malam terakhir pilih yang mana.";
        }

        // Sisa hari: istirahat dulu, baru tidur.
        if (gs.sesi == GameState.Sesi.Malam)
        {
            return "Sesi malam. Tidur dulu (tombol Tidur di Catatan, atau dekati kasur lalu tekan E).";
        }

        return "Sisa sesi buat istirahat. Kalau sesinya sudah habis, tidur supaya besok pagi.";
    }

    // Interview yang dijadwalkan untuk hari ini, kalau ada.
    private static string InterviewHariIni(GameState gs)
    {
        if (gs.statusLamaranA == GameState.StatusLamaran.Terjadwal && gs.jadwalInterviewA == gs.hari)
        {
            return "Interview " + NamaPerusahaan("A") + " hari ini, sesi siang.";
        }

        if (gs.statusLamaranB == GameState.StatusLamaran.Terjadwal && gs.jadwalInterviewB == gs.hari)
        {
            return "Interview " + NamaPerusahaan("B") + " hari ini, sesi siang.";
        }

        return "";
    }

    // Retakan yang mestinya dibuka hari ini.
    private static string RetakanHariIni(GameState gs)
    {
        if (gs.hari == 2 && !gs.retakanSelesai[0])
        {
            return "Ada retakan di Ruang Terpanjang. Ambil Kartu Proyek, pasang di penjepit foto.";
        }

        if (gs.hari == 4 && !gs.retakanSelesai[1])
        {
            return "Ada retakan di Lorong Kos. Ambil kunci 07 dari bangku, pakai di pintu 07.";
        }

        if (gs.hari == 6 && !gs.retakanSelesai[2])
        {
            return "Ada retakan di kamar. Ambil Kartu Pesan Ibu, pasang di dudukan kursi jendela.";
        }

        return "";
    }

    // ---------------------------------------------------------------
    // Checklist
    // ---------------------------------------------------------------

    // Daftar pekerjaan yang masih ada, dipisah antara yang sudah beres
    // dan yang belum.
    public static List<string> Checklist(GameState gs)
    {
        List<string> daftar = new List<string>();
        if (gs == null) return daftar;

        daftar.Add(Baris(gs.PunyaBukti("E0"), "Bukti E0 - Kartu Proyek (di rak ruang terpanjang)"));
        daftar.Add(Baris(gs.PunyaBukti("E1"), "Bukti E1 - antarmuka formulir (rapikan contoh antarmuka)"));
        daftar.Add(Baris(gs.PunyaBukti("E2"), "Bukti E2 - catatan pengujian (perlu E1)"));

        // Lamaran A.
        if (gs.statusLamaranA == GameState.StatusLamaran.BelumDikirim)
        {
            string alasan = (gs.hari > 3) ? " (lewat batas hari 3)" : " (batas malam hari 3)";
            daftar.Add(Baris(false, "Kirim lamaran Studio Lajur" + alasan));
        }
        else
        {
            daftar.Add(Baris(true, "Lamaran Studio Lajur terkirim"));
        }

        // Lamaran B.
        if (gs.statusLamaranB == GameState.StatusLamaran.BelumDikirim)
        {
            string alasan = (gs.hari > 5) ? " (lewat batas hari 5)" : " (batas malam hari 5, butuh E1 + E2)";
            daftar.Add(Baris(false, "Kirim lamaran Koperasi Pijar" + alasan));
        }
        else
        {
            daftar.Add(Baris(true, "Lamaran Koperasi Pijar terkirim"));
        }

        // Interview.
        daftar.Add(Baris(gs.statusLamaranA == GameState.StatusLamaran.Ditawari
                         || gs.statusLamaranA == GameState.StatusLamaran.Ditolak,
                         "Interview Studio Lajur (hari 5 siang)"));
        daftar.Add(Baris(gs.statusLamaranB == GameState.StatusLamaran.Ditawari
                         || gs.statusLamaranB == GameState.StatusLamaran.Ditolak,
                         "Interview Koperasi Pijar (hari 6 siang)"));

        // Retakan.
        daftar.Add(Baris(gs.retakanSelesai[0], "Retakan 1 - penjepit foto (buka hari 2)"));
        daftar.Add(Baris(gs.retakanSelesai[1], "Retakan 2 - pintu 07 (buka hari 4)"));
        daftar.Add(Baris(gs.retakanSelesai[2], "Retakan 3 - dudukan kursi jendela (buka hari 6)"));

        daftar.Add(Baris(gs.hari >= 4 && gs.verifikasiSudah[2], "Cek lowongan Akses Prima (muncul hari 4)"));
        daftar.Add(Baris(gs.pilihanTawaran != GameState.PilihanTawaran.TidakAda,
                         "Malam hari 7: pilih tawaran yang diterima"));

        return daftar;
    }

    // Satu baris checklist. Yang sudah beres pakai tanda centang.
    private static string Baris(bool selesai, string teks)
    {
        return (selesai ? "[x] " : "[ ] ") + teks;
    }

    // Jumlah yang belum beres, buat ditampilin sebagai badge.
    public static int Sisa(GameState gs)
    {
        int sisa = 0;
        List<string> daftar = Checklist(gs);
        for (int i = 0; i < daftar.Count; i++)
        {
            if (daftar[i].StartsWith("[ ]")) sisa++;
        }
        return sisa;
    }
}
