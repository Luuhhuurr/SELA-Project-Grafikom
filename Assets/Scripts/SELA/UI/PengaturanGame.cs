using UnityEngine;

// PengaturanGame.cs
//
// Pengaturan yang bisa diubah pemain dari menu Pengaturan:
//   - ukuran teks (100%, 125%, 150%)
//   - sensitivitas mouse
//   - invert Y (mouse terbalik)
//   - kurangi gerak (ayunan kepala dimatikan)
//
// Nilainya ditaruh di sini (class static) karena dipakai beberapa script
// sekaligus: UIManager untuk ukuran teks, FirstPersonController untuk mouse
// dan ayunan kepala.
//
// Sifatnya sementara: kalau game ditutup, kembali ke settingan awal.
// Kalau mau ikut disimpan, baru ditambahin ke SaveSystem.

public static class PengaturanGame
{
    // Ukuran teks. 1 = 100%, 1.25 = 125%, 1.5 = 150%
    public static float skalaTeks = 1f;

    // Sensitivitas mouse. Makin besar, makin cepat kepala menoleh.
    public static float sensitivitasMouse = 2f;

    // True = mouse atas/bawah dibalik.
    public static bool invertY = false;

    // True = ayunan kepala dimatikan (bantu pemain yang sensitif terhadap gerak).
    public static bool kurangiGerak = false;

    // Pilihan ukuran teks yang muncul di menu.
    public static float[] PilihanUkuranTeks = { 1f, 1.25f, 1.5f };

    public static string[] LabelUkuranTeks = { "100%", "125%", "150%" };

    // Naikkan ukuran teks. Kalau sudah paling besar, kembali ke yang terkecil.
    public static void GantiUkuranTeks(int arah)
    {
        int indeks = 0;
        for (int i = 0; i < PilihanUkuranTeks.Length; i++)
        {
            if (Mathf.Approximately(PilihanUkuranTeks[i], skalaTeks)) indeks = i;
        }

        indeks = (indeks + arah + PilihanUkuranTeks.Length) % PilihanUkuranTeks.Length;
        skalaTeks = PilihanUkuranTeks[indeks];
    }

    // Label ukuran teks yang sedang dipakai (buat ditampilkan di menu).
    public static string LabelUkuranTeksSekarang()
    {
        for (int i = 0; i < PilihanUkuranTeks.Length; i++)
        {
            if (Mathf.Approximately(PilihanUkuranTeks[i], skalaTeks)) return LabelUkuranTeks[i];
        }
        return "100%";
    }
}

/* ============================================================
   [CARA PASANG] PengaturanGame.cs
   1. Script ini TIDAK ditaruh di GameObject apa pun. Cuma class static.
   2. Yang membaca:
        - UIManager        : skalaTeks (mengubah fontSize semua TMP)
        - FirstPersonController : sensitivitasMouse, invertY, kurangiGerak
   3. Kalau mau setting-nya ikut ke save, panggil PengaturanGame dari
      SaveSystem saat menyimpan dan memuat.
   4. Ukuran teks: 100% (1), 125% (1.25), 150% (1.5).
      Jangan menambah nilai lain tanpa menambah label-nya juga.
   ============================================================ */
