using System;
using UnityEngine;

// SimpleInventory.cs
// Tas Raka. Maksimal satu benda saja (biar sederhana dan sesuai naskah:
// satu puzzle butuh satu benda yang dibawa).
//
// Benda diambil dari objek yang punya "Ambil Sebagai Benda" centang,
// lalu dipasang ke jangkar Retakan Ruang lewat tombol E.
public class SimpleInventory : MonoBehaviour
{
    // Nama benda yang sedang dibawa. String kosong = tas kosong.
    public string bendaSaatIni = "";

    // Dipanggil setiap isi tas berubah (benda diambil atau dipasang).
    public event Action<string> OnBendaBerubah;

    // Ambil sebuah benda ke tas.
    // Return false kalau tas sudah berisi benda lain (maksimal 1).
    public bool AmbilBenda(string nama)
    {
        if (string.IsNullOrEmpty(nama)) return false;
        if (!string.IsNullOrEmpty(bendaSaatIni)) return false;

        bendaSaatIni = nama;
        BeriTahu();
        return true;
    }

    // Pakai / pasang benda yang sedang dibawa (dipakai PuzzleAnchor).
    // Setelah dipakai, tas jadi kosong.
    public void PasangBenda()
    {
        bendaSaatIni = "";
        BeriTahu();
    }

    // Kosongkan tas (dipakai saat reset permainan).
    public void Kosongkan()
    {
        bendaSaatIni = "";
        BeriTahu();
    }

    // Cek apakah Raka sedang membawa benda dengan nama tertentu.
    public bool PunyaBenda(string nama)
    {
        return !string.IsNullOrEmpty(nama) && bendaSaatIni == nama;
    }

    // Isi tas langsung (dipakai SaveSystem saat memuat data).
    public void SetBenda(string nama)
    {
        bendaSaatIni = string.IsNullOrEmpty(nama) ? "" : nama;
        BeriTahu();
    }

    // True kalau tas sedang kosong.
    public bool Kosong()
    {
        return string.IsNullOrEmpty(bendaSaatIni);
    }

    private void BeriTahu()
    {
        if (OnBendaBerubah != null) OnBendaBerubah(bendaSaatIni);
    }
}

/* ============================================================
   [CARA PASANG] SimpleInventory.cs
   1. Taruh script ini di GameObject "Sistem" yang sama dengan GameState.
      (GameState akan otomatis mengambilnya lewat GetComponent)
   2. Tidak ada kolom yang perlu diisi manual.
   3. Cara pakainya dari script lain:
        GameState.Instance.inventory.AmbilBenda("KartuProyek");
        GameState.Instance.inventory.PunyaBenda("KartuProyek");
        GameState.Instance.inventory.PasangBenda();
   4. Kalau ingin tampil di HUD, UIManager cukup berlangganan (subscribe)
      ke event SimpleInventory.OnBendaBerubah.
   ============================================================ */
