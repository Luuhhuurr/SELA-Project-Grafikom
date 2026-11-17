using System;
using UnityEngine;

/// <summary>
/// SimpleInventory.cs
/// Tas Raka. Maksimal satu benda saja (biar sederhana dan sesuai naskah:
/// satu puzzle butuh satu benda yang dibawa).
///
/// Benda diambil dari objek yang punya "Ambil Sebagai Benda" centang,
/// lalu dipasang ke jangkar Retakan Ruang lewat tombol E.
/// </summary>
public class SimpleInventory : MonoBehaviour
{
    /// <summary>Nama benda yang sedang dibawa. String kosong = tas kosong.</summary>
    public string bendaSaatIni = "";

    /// <summary>Dipanggil setiap isi tas berubah (benda diambil atau dipasang).</summary>
    public event Action<string> OnBendaBerubah;

    /// <summary>
    /// Ambil sebuah benda ke tas.
    /// Return false kalau tas sudah berisi benda lain (maksimal 1).
    /// </summary>
    public bool AmbilBenda(string nama)
    {
        if (string.IsNullOrEmpty(nama)) return false;
        if (!string.IsNullOrEmpty(bendaSaatIni)) return false;

        bendaSaatIni = nama;
        BeriTahu();
        return true;
    }

    /// <summary>
    /// Pakai / pasang benda yang sedang dibawa (dipakai PuzzleAnchor).
    /// Setelah dipakai, tas jadi kosong.
    /// </summary>
    public void PasangBenda()
    {
        bendaSaatIni = "";
        BeriTahu();
    }

    /// <summary>Kosongkan tas (dipakai saat reset permainan).</summary>
    public void Kosongkan()
    {
        bendaSaatIni = "";
        BeriTahu();
    }

    /// <summary>Cek apakah Raka sedang membawa benda dengan nama tertentu.</summary>
    public bool PunyaBenda(string nama)
    {
        return !string.IsNullOrEmpty(nama) && bendaSaatIni == nama;
    }

    /// <summary>Isi tas langsung (dipakai SaveSystem saat memuat data).</summary>
    public void SetBenda(string nama)
    {
        bendaSaatIni = string.IsNullOrEmpty(nama) ? "" : nama;
        BeriTahu();
    }

    /// <summary>True kalau tas sedang kosong.</summary>
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
