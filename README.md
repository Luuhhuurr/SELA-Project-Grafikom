# SELA

Game naratif first-person 3D bertema tekanan kerja dan ekspektasi, dibuat untuk
tugas Grafika Komputer (Kelompok VI).

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

## Cara menjalankan

1. Buka folder ini dengan Unity 6000.3.27f1.
2. Tunggu import selesai (TextMesh Pro Essential Resources sudah ikut di repo).
3. Buka scene `Assets/Scenes/SELA_Play.unity`.
4. Tekan Play. Menu awal muncul, tekan **Enter** untuk mulai.

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

## Struktur folder

```
Assets/Scripts/SELA/
├── Core/        GameState, SessionManager, SaveSystem
├── Player/      FirstPersonController, SimpleInventory
├── Interaksi/   ObjekInteraktif, EvidenceCard, PuzzleAnchor
├── Aktivitas/   ActivityData (ScriptableObject), ActivitySystem
├── Karir/       CVSystem, InterviewManager
├── Akhir/       EndingManager
├── UI/          UIManager
├── Naskah/      NaskahData (ScriptableObject)
├── Editor/      Alat bantu membuat scene demo (Tools > SELA)
└── Tests/       Uji asap PlayMode (Test Runner > PlayMode)
```

Data cerita & aktivitas berada di `Assets/GameData/`, teks naskah hari ini di
`Assets/GameData/Naskah/Resources/` (asset `NaskahData` dengan cadangan JSON).

## Uji

Uji asap tersedia di `Assets/Scripts/SELA/Tests/UjiSmokePlayMode.cs`. Jalankan dari
**Window > General > Test Runner > PlayMode > Run All**. Uji ini tidak menyentuh
file save pemain (save diarahkan ke folder sementara).

## Catatan

`Assets/ClaudeConnector.cs` dan `Assets/TesClaude.cs` sengaja tidak ikut dalam repo
(cobaan pemanggilan API, tidak terkait dengan game).
