# SELA

Game naratif first-person 3D bertema tekanan kerja dan ekspektasi, dibuat untuk
tugas mata kuliah Grafika Komputer (Kelompok VI).

**Premis.** Raka (23 tahun, lulusan Informatika) tinggal di kos kamar 07. Ia punya
tujuh hari sebelum sewa kosnya habis, sambil mencari pekerjaan. Tiap hari ia
menjalankan kegiatan, mengelola tenaga dan tekanan mental, lalu memilih tawaran
kerja yang ditawarkan di malam terakhir.

## Teknologi

- Unity 6 LTS (6000.3.27f1) + URP
- C# — seluruh logika inti, tanpa plugin pihak ketiga
- uGUI + TextMeshPro untuk UI
- Input System (`Keyboard.current` / `Mouse.current`), legacy `Input` dimatikan
- `JsonUtility` untuk save/load, `ScriptableObject` untuk data naskah & aktivitas
- Assembly definition: `SELA.Runtime`, `SELA.Editor`, `SELA.Tests`

## Cara menjalankan

1. Buka folder ini dengan Unity 6000.3.27f1.
2. Tunggu import selesai (TextMesh Pro Essential Resources sudah ikut di repo).
3. Buka scene `Assets/Scenes/SELA_Play.unity`.
4. Tekan Play. Menu awal muncul, tekan **Enter** untuk mulai.

Kalau scene-nya belum ada (mis. clone baru), scene bisa dibuat ulang dari menu
**Tools > SELA > Buat Scene Demo**. Perintah yang sama dipakai dari terminal:

```
Unity.exe -batchmode -quit -projectPath <path project> \
  -executeMethod SELA.Editor.BuatSceneDemo.BuildFromCommandLine
```

## Kontrol

| Tombol | Fungsi |
|---|---|
| WASD | Jalan |
| Mouse | Melihat sekeliling |
| E | Interaksi (ambil kartu bukti, pasang benda) |
| Tab | Buka / tutup Catatan |
| L | Buka / tutup Laptop |
| Esc | Buka Pause menu / tutup panel |
| Enter / Spasi | Lanjut dialog |
| 1 / 2 | Pilih jawaban dialog |

## Isi permainan

Tujuh hari, tiga sesi tiap hari (pagi, siang, malam). Satu kegiatan memakai satu
sesi, jadi pemain harus memilih urutan kegiatan.

- **Bukti.** E0 (kartu proyek), E1 (antarmuka formulir), E2 (catatan pengujian).
  Bukti harus ikut di CV dan di-checklist perusahaan.
- **Lamaran.** Lamaran A (Studio Lajur) dibuka hari 1 dan ditutup hari 3 malam.
  Lamaran B (Koperasi Pijar) butuh E1 + E2, dibuka hari 3 dan ditutup hari 5 malam.
  CV yang sudah dikirim dikunci, tidak bisa diubah.
- **Verifikasi.** Sebelum melamar, Raka boleh cek lowongan: Akses Prima baru muncul
  hari 4 pagi, dan tawaran itu mencurigakan (minta dokumen asli, kontak tidak
  konsisten).
- **Retakan Ruang.** Tiga retakan, muncul hari 2, 4, dan 6. Bongkar dulu, baru pasang
  bendanya.
- **Interview.** Sesi siang, dua pertanyaan. Nilai tiap jawaban tidak pernah
  ditampilkan, tapi menentukan tawaran atau penolakan.
- **Ending.** Malam hari ke-7 Raka memilih tawaran A, B, C, atau pulang. Ending
  hanya mengikuti pilihan itu, bukan angka tenaga atau tekanan.

Menu **Pengaturan** (dari menu awal) berisi ukuran teks 100% / 125% / 150% dan
saat matikan animasi notifikasi.

## Struktur folder

```
Assets/Scripts/SELA/
├── Core/        GameState, SessionManager, SaveSystem
├── Player/      FirstPersonController, SimpleInventory
├── Interaksi/   ObjekInteraktif, EvidenceCard, PuzzleAnchor
├── Aktivitas/   ActivityData (ScriptableObject), ActivitySystem
├── Karir/       CVSystem, InterviewManager, DataPerusahaan
├── Akhir/       EndingManager
├── UI/          UIManager, PengaturanGame
├── Naskah/      NaskahData (ScriptableObject)
├── Editor/      Alat bantu membuat scene demo (Tools > SELA)
└── Tests/       Uji PlayMode (Test Runner > PlayMode)
```

Data cerita & aktivitas berada di `Assets/GameData/`, teks naskah hari ini di
`Assets/GameData/Naskah/Resources/` (asset `NaskahData` dengan cadangan JSON).

Semua antar script berkomunikasi lewat `GameState.Instance`, tidak ada
`FindObjectOfType` di dalam `Update()`.

## Uji

Dua uji PlayMode ada di `Assets/Scripts/SELA/Tests/`:

- `UjiSmokePlayMode` — scene bisa dimuat, semua sistem ke-reference dengan benar.
- `UjiTujuhHariPlayMode` — main penuh tujuh hari: CV, kirim lamaran A dan B,
  interview, dua tawaran, sampai layar ending.

Jalankan dari **Window > General > Test Runner > PlayMode > Run All**, atau dari
terminal:

```
Unity.exe -batchmode -runTests -testPlatform PlayMode \
  -projectPath <path project> -testResults hasil-uji.xml
```

Uji ini tidak menyentuh file save pemain, karena save diarahkan ke folder
sementara.

## Credits

Luhur Pangaribowo Handaruji — naratif dan UI. Pembagian tugas lengkap ada di
panel **Credits dan Sumber** pada menu awal.

## Catatan

File percobaan pemanggilan API (`ClaudeConnector.cs` dan `TesClaude.cs`) sudah dihapus
dari project ini dan tidak ada di dalam repo.
