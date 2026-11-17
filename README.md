# SELA

Game naratif first-person 3D tentang Raka, 23 tahun, yang mencari kerja
dalam tujuh hari terakhir sewa kosnya.

Dibuat untuk tugas mata kuliah **Grafika Komputer**, Teknik Informatika
semester 5, **Kelompok VI**. Bagian naratif dan UI dikerjakan oleh
Luhur Pangaribowo Handaruji.

> SELA ini fiksi. Perusahaan, kontrak, dan nama yang muncul di dalam game
> tidak merujuk orang sungguhan. "Tekanan" di game ini adalah abstraksi
> permainan, bukan ukuran kondisi mental.

## Isi permainan

Raka tinggal di kos kamar 07. Ibunya sudah membeli tiket bus tanggal 8 dan
menanyakan sampai kapan sewanya ditanggung. Jawabannya: tujuh hari.

Selama tujuh hari itu Raka harus menyusun portofolio, melamar, verifikasi
lowongan, dan merapikan CV-nya. Tekanan datang dari dua arah sekaligus —
dari mencari kerja, dan dari tidak punya tempat tinggal yang pasti. Di
malam terakhir Raka harus memilih satu tawaran, atau pulang.

**Tokoh**

| Nama | Peran |
|---|---|
| Raka | Protagonis, 23 tahun, baru wisuda, kamar 07 |
| Ibu | Membeli tiket bus, mengirim pesan di hari 1, 6, dan 7 |
| Bapak | Disebut di ending "pulang" |
| Dita | Teman kuliah, membantu cek CV, mengirim pesan tiap hari |
| Nara | Recruiter Studio Lajur, interview hari 5 |
| Bagas | Recruiter Koperasi Pijar, interview hari 6 |

Kalimat penutup cerita: *"Pekerjaan bukan garis finish. Ini mulai, dengan
batas yang bisa diucapkan."*

## Cara menjalankan

1. Buka project ini dengan **Unity 6000.3.27f1**.
2. Tunggu import selesai. TextMesh Pro Essential Resources sudah ikut di repo.
3. Buka scene `Assets/Scenes/SELA_Play.unity`.
4. Tekan **Play**.

Saat game dibuka, **menu awal** muncul lebih dulu dan gerakan pemain
dikunci. Tombol yang ada di sana:

| Tombol | Fungsi |
|---|---|
| **Lanjutkan** | Lanjut dari save. Hanya muncul kalau ada file save. |
| **Mulai Baru** | Hapus save lama, mulai ulang dari hari 1. |
| **Info Konten** | Disclaimer, catatan soal isi game, dan daftar kontrol. |
| **Pengaturan** | Ukuran teks, sensitivitas mouse, invert Y, kurangi gerak. |
| **Credits dan Sumber** | Pembagian tugas dan daftar yang dipakai. |
| **Keluar Game** | Keluar. |

**Enter atau Spasi** juga bisa memulai game langsung dari menu. Karena
save dimuat otomatis saat game dibuka, Enter melanjutkan save terakhir.
Kalau mau benar-benar mengulang dari hari 1, klik **Mulai Baru**.

Kalau scene-nya belum ada (mis. clone baru), scene bisa dibangun ulang dari
menu **Tools > SELA > Buat Scene Demo**. Perintah yang sama dari terminal:

```
Unity.exe -batchmode -quit -projectPath <path project> \
  -executeMethod SELA.Editor.BuatSceneDemo.BuildFromCommandLine
```

## Kontrol

| Tombol | Fungsi |
|---|---|
| WASD | Jalan |
| Mouse | Melihat sekeliling |
| **E** | Interaksi dengan benda di depan (ambil kartu, pasang benda) |
| Klik kiri | Interaksi juga |
| **Tab** | Buka / tutup panel Catatan |
| **L** | Buka / tutup panel Laptop |
| **Esc** | Buka menu jeda / tutup panel yang sedang terbuka |
| Enter / Spasi | Lanjut dialog |
| **1 / 2** | Pilih jawaban dialog (saat ada pilihan) |

Mouse terkunci otomatis setelah game dimulai. Kalau cursor keluar dari
jendela, klik kiri untuk mengunci ulang.

## Sistem permainan

### Waktu: hari dan sesi

Tujuh hari, tiap hari tiga sesi: **Pagi → Siang → Malam**. Satu aktivitas
menggunakan satu sesi penuh, jadi pemain harus memilih urutan kegiatannya.

Malam hari ketujuh bukan waktu untuk tidur, tapi waktu untuk memutuskan.
 Hari berikutnya berganti otomatis setelah sesi Malam habis.

**Tidur** hanya bisa dilakukan di sesi Malam — lewat tombol **Tidur** di
panel Catatan, atau dekati kasur lalu tekan E. Tidur memulihkan tenaga dan
menurunkan tekanan, lalu hari berikutnya dimulai.

### Kondisi Raka

| Status | Nilai awal | Rentang | Arti |
|---|---|---|---|
| **Tenaga** | 3 | 0–4 | Sisa energi. Kalau habis, aktivitas yang butuh tenaga tidak bisa jalan. |
| **Tekanan** | 1 | 0–6 | Beban yang menumpuk. Kalau mencapai 6, sistem jeda terbuka. |

Tidak ada angka uang, lapar, atau reputasi. Semua yang lain adalah penanda
keadaan, bukan angka yang bisa naik turun.

**Sistem jeda.** Begitu tekanan menyentuh 6, panel jeda terbuka — maksimal
sekali per hari. Tekanan diturunkan kembali dan tenaga dijaga tetap minimal
1. **Jeda tidak memakai sesi**, jadi ini jeda darurat, bukan aktivitas.

### Tas Raka

Tas hanya muat **satu benda**. Kalau sudah ada bendanya, ambil barang baru
akan ditolak. Ini yang membuat tiga retakan ruang harus dikerjakan satu per
satu.

### Aktivitas

Ada 13 aktivitas. Biaya tenaga positif berarti tenaga terpakai, negatif
berarti tenaga pulih. Biaya tekanan positif berarti tekanan naik.

| Aktivitas | Biaya tenaga | Biaya tekanan | Bisa dijalankan | Catatan |
|---|---|---|---|---|
| Rapikan Contoh Antarmuka | 1 | 1 | Hari 1, sekali | Hasilnya bukti **E1**. |
| Lengkapi Catatan Pengujian | 1 | 1 | Hari ≥ 3, sekali | Butuh **E1**. Hasilnya bukti **E2**. |
| Periksa Studio Lajur | 1 | 0 | Hari 1, sekali | Buka panel verifikasi lowongan. |
| Periksa Koperasi Pijar | 1 | 0 | Hari 1, sekali | Buka panel verifikasi lowongan. |
| Periksa Akses Prima | 1 | 0 | Hari ≥ 4, sekali | Lowongannya baru muncul hari 4. |
| Kirim Lamaran ke Studio Lajur | 1 | 1 | Hari 1–3, sekali | Butuh **E1** dan CV yang sudah dirakit. |
| Kirim Lamaran ke Koperasi Pijar | 1 | 1 | Hari 3–5, sekali | Butuh **E1 + E2** dan CV yang sudah dirakit. |
| Ikuti Interview Studio Lajur | 1 | 2 | Hari 5, sekali | Wajib di sesi siang. |
| Ikuti Interview Koperasi Pijar | 1 | 2 | Hari 6, sekali | Wajib di sesi siang. |
| Rapikan Meja | 1 | −1 | Setiap hari | Mengakai tenaga, menurunkan tekanan. |
| Duduk Tanpa Layar | −2 | −1 | Setiap hari | Pulihkan tenaga, turunkan tekanan. |
| Istirahat | −3 | −2 | Setiap hari | Recovery paling besar. |
| Hubungi Dita | −2 | −2 | Setiap hari | Dita bantu cek CV dan kasih saran. |

Aktivitas yang tidak bisa dijalankan akan menunjukkan **alasannya** di panel
konfirmasi — misalnya "butuh E1" atau "batas sudah lewat".

### Bukti

| ID | Nama | Cara didapat | Dipakai buat |
|---|---|---|---|
| **E0** | Kartu Proyek | Ambil di **Ruang Terpanjang**, di atas rak panjang. | Benda untuk retakan ruang 1. |
| **E1** | Portofolio Antarmuka | Aktivitas *Rapikan Contoh Antarmuka*. | Wajib untuk kedua lamaran. |
| **E2** | Catatan Uji | Aktivitas *Lengkapi Catatan Pengujian* (butuh E1). | Wajib untuk lamaran Koperasi Pijar. |

Bukti yang sudah dipakai di CV **dikunci**. CV yang sudah dikirim juga
tidak bisa diubah lagi.

### Lamaran dan lowongan

Ada tiga perusahaan yang dipantau Raka. Dua di antaranya bisa dilamar.

| Kode | Perusahaan | Posisi | Kompensasi | Lamaran | Batas | Syarat |
|---|---|---|---|---|---|---|
| **A** | Studio Lajur | Junior UI Implementer | Rp4.200.000/bulan, 40 jam/minggu, 2 hari istirahat | Bisa | **Malam hari 3** | **E1** |
| **B** | Koperasi Pijar | Web Support | Rp3.800.000/bulan, kontrak 3 bulan, 35 jam/minggu, 1 hari libur tambahan | Bisa | **Malam hari 5** | **E1 + E2** |
| **C** | Akses Prima | Junior Developer | "Sampai Rp12.000.000", jam kerja "fleksibel" | **Tidak bisa dilamar** | — | — |

Studio Lajur dan Koperasi Pijar menerima interview **sesi siang** di hari
lima dan enam — Studio Lajur di hari 5, Koperasi Pijar di hari 6.

Kalau batas lamaran terlewat, aktivitasnya terkunci dan perusahaan itu
tidak akan pernah memberi tawaran — pilihannya tidak muncul di malam
terakhir. Tapi kalau **terlewat interview**, lamaran langsung ditandai
ditolak dan tekanan naik satu.

### Verifikasi lowongan

Sebelum melamar, Raka boleh cek lowongannya. Ada empat aspek yang
diperiksa: **Tugas, Kompensasi, Jam kerja, Dokumen pribadi**. Tiap aspek
diberi tanda:

- **Jelas** — apa adanya, tidak perlu dicurigai.
- **Perlu ditanyakan** — ada yang belum jelas.
- **Berisiko** — ada yang mencurigakan.

Studio Lajur dan Koperasi Pijar keempat aspeknya "Jelas". Akses Prima
tidak: tugasnya "Perlu ditanyakan", sedangkan kompensasi, jam kerja, dan
dokumen pribadi ditandai "Berisiko". Catatan pemeriksaannya: alamat kantor
tidak ada di peta, nomor WhatsApp cuma aktif satu jam sehari, dan domain
emailnya nggak nyambung sama alamatnya.

Akses Prima sengaja tidak punya alur lamaran. Memeriksa lowongannya
justru untuk menunjukkan seperti apa lowongan yang mencurigakan.

### Rakit CV

CV punya dua baris klaim, dan tiap baris punya dua versi:

| Baris | Sesuai | Berlebihan |
|---|---|---|
| Klaim 1 | "membantu pengembangan antarmuka proyek kuliah" | "memimpin sistem produksi" |
| Klaim 2 | "menguji formulir secara manual" | "menjamin sistem bebas seluruh kesalahan" |

Klaim yang berlebihan **tidak otomatis menggagalkan** lamaran, tapi harus
dikoreksi saat interview. Kalau dibiarkan, tawaran tidak akan diberikan.

Bukti yang dipasang ke CV juga menentukan: bukti yang belum ikut saat
lamaran dikirim berarti syarat bukti interview tidak terpenuhi.

### Interview

Tiap interview punya satu baris pembuka, **dua pertanyaan**, dan satu baris
penutup. Setiap pertanyaan punya dua pilihan jawaban.

Tiap jawaban punya nilai 0, 1, atau 2 — dan **nilai itu tidak pernah
ditampilkan ke pemain**. Tidak ada tombol "lihat nilai", tidak ada umpan
balik setelah menjawab.

Tawaran diberikan hanya kalau keempat syarat ini terpenuhi:

1. Semua bukti wajib sudah ikut **di lamaran yang terkirim** (bukan cuma di
   draf).
2. Tidak ada klaim berlebihan yang belum dikoreksi.
3. Setiap jawaban bernilai minimal 1.
4. Total nilai minimal **3 dari 4**.

Kalau gagal, lamaran ditolak, tekanan naik satu, dan game menampilkan
alasan penolakannya. Tidak ada game-over — penolakan dari satu perusahaan
tidak menutup pintu di perusahaan lain.

### Retakan Ruang

Tiga retakan muncul di ruang kos, satu setiap dua hari. Mekaniknya
sederhana: **ambil bendanya dulu, baru pasang** di tempatnya.

| Retakan | Muncul | Ruang | Benda yang diambil | Pemasangan |
|---|---|---|---|---|
| 1 | Hari 2 | Ruang Terpanjang | Kartu Proyek (E0), di rak panjang | Penjepit foto di ujung meja panjang |
| 2 | Hari 4 | Lorong Kos | Gantungan Kunci 07, di bangku dekat pintu | Pintu kamar 07 |
| 3 | Hari 6 | Kamar 07 | Kartu Pesan Ibu, di atas meja | Dudukan kursi jendela |

Tidak ada timer dan tidak ada hukuman. Benda yang salah akan diberi
petunjuk, dan setelah tiga kali percobaan salah muncul label tujuan
sebagai petunjuk tambahan. Kalau bendanya benar, retakan tertutup, ruang
kembali normal, dan bendanya habis dipakai.

### Ending

Malam hari ketujuh, panel **MALAM TERAKHIR** terbuka dan Raka harus
memilih:

| Pilihan | Muncul kalau | Ending |
|---|---|---|
| **A. Terima tawaran Studio Lajur** | Lamaran A sudah ditawari | *Mulai dengan Batas* |
| **B. Terima tawaran Koperasi Pijar** | Lamaran B sudah ditawari | *Mulai dengan Batas* |
| **C. Terima tawaran yang cepat** | Selalu | *Kontrak yang Mengunci* |
| **Tidak ada. Pulang dengan portofolio yang sudah rapi.** | Selalu | *Pulang, Bukan Selesai* |

Pilihan C kalau dipilih tidak langsung selesai — ada peringatan khusus
bahwa tawaran itu meminta penyerahan dokumen asli dan membatasi pengunduran
diri, dan perlu **dua kali konfirmasi** sebelum diteruskan.

Ending **hanya mengikuti pilihan kontrak**, bukan angka tenaga atau
tekanan. Berapa pun tenaga Raka punya, kalau tidak ada tawarannya dia
tetap pulang.

Layar akhir berisi cerita, tiga pertanyaan refleksi, dan disclaimer.

### Hemat waktu: tujuan harian dan checklist

HUD selalu menampilkan satu baris **TUJUAN** — hal yang harus dilakukan
sekarang, dihitung ulang sesuai keadaan game. Panel **Catatan** (tombol
Tab) berisi:

- **TUJUAN HARI INI** — satu kalimat arah.
- **DAFTAR PEKERJAAN** — checklist `[ ]` untuk yang belum beres, `[x]` untuk
  yang sudah.
- Status bukti, status lamaran, dan progres retakan ruang.

Jadi tidak ada langkah yang disembunyikan. Kalau sebuah aktivitas terkunci,
catatannya menyebutkan alasannya.

## Panduan tujuh hari

| Hari | Judul | Yang terjadi | Yang harus dikerjakan |
|---|---|---|---|
| 1 | Bangun & Pesan Ibu | Ibu sudah beli tiket bus tanggal 8 dan menanyakan sampai kapan sewa kos ditanggung. Lowongan Studio Lajur terbuka. | Dapat **E1**, rakit CV, kirim lamaran ke Studio Lajur sebelum malam hari 3. |
| 2 | Dita Mengecek Lowongan Studio Lajur | Dita mengirim ulang lowongan Studio Lajur dan memperingatkan batasnya. | Ambil **Kartu Proyek** di Ruang Terpanjang, pasang di penjepit foto (retakan 1). |
| 3 | Batas Lamaran dan Koperasi Pijar | Batas lamaran Studio Lajur malam ini. Koperasi Pijar membuka posisi Web Support dengan gaji lebih kecil tapi risiko kecil. | Kirim lamaran A **malam ini**. Dapat **E2** (butuh E1), lalu lamaran B sebelum malam hari 5. |
| 4 | Akses Prima Menghubungi Raka | Akses Prima menawarkan gaji besar tanpa interview, minta KTP dan ijazah asli. Lorong kos terasa lebih panjang. | Periksa lowongan Akses Prima. Ambil **Kunci 07** dari bangku, pakai di pintu 07 (retakan 2). |
| 5 | Interview Studio Lajur | Studio Lajur menjadwalkan interview pukul 13.00. | Interview di **sesi siang**. Bawa E1, luruskan klaim berlebihan kalau ada. |
| 6 | Interview Koperasi Pijar | Interview Koperasi Pijar pukul 13.00. Sore Ibu bilang tiket busnya sudah dibayar semua. | Interview di **sesi siang**. Ambil **Kartu Pesan Ibu**, pasang di dudukan kursi jendela (retakan 3). |
| 7 | Malam Terakhir | Malam ini Raka memilih: diterima, ditolak, atau pulang. | Buka panel Catatan, bandingkan empat hal, lalu putuskan di MALAM TERAKHIR. |

Catatan penting: interview **wajib** di sesi siang. Kalau Raka tidur tanpa
menghadiri interview, lamaran langsung ditolak.

## Data cerita

Naskah tiap hari disimpan sebagai ScriptableObject `NaskahData` — satu asset
per hari di `Assets/GameData/Naskah/Resources/` (`NaskahHari1..7.asset`),
dengan salinan JSON (`naskah_hari1..7.json`) sebagai cadangan.

Runtime memuat lewat `NaskahData.Ambil(hari)`: coba asset ScriptableObject
dulu, kalau gagal fallback ke JSON, kalau masih gagal dikembalikan null
dan UI harus tahan null.

Tiga tab laptop membaca dari data itu:

- **Lamaran** — draf lowongan hari ini, batas lamaran, syaratnya, plus draf
  CV Raka dan daftar aktivitas.
- **Pesan** — pesan pembuka hari ini dari Ibu/Dita/perusahaan, pesan
  tambahan, dan email otomatis (jadwal interview, tawaran, penolakan).
- **Catatan** — sama dengan panel Catatan yang dibuka tombol Tab.

Data perusahaan dan script interview tidak berupa asset — keduanya statis di
`Assets/Scripts/SELA/Karir/`.

## Teknologi

- **Unity 6000.3.27f1** + **URP 17.3.0**
- C# — seluruh logika inti, tanpa plugin pihak ketiga
- **uGUI 2.0.0** + TextMeshPro. Di Unity 6, TextMeshPro sudah menyatu ke
  paket uGUI, jadi tidak ada paket `com.unity.textmeshpro` terpisah.
- **Input System 1.20.1**. Legacy Input Manager **dimatikan**
  (`activeInputHandler: 1`), dan input dibaca langsung lewat
  `Keyboard.current` / `Mouse.current` — bukan lewat `.inputactions` asset.
- `ScriptableObject` untuk data naskah dan aktivitas, `JsonUtility` untuk
  save/load
- Assembly definition: `SELA.Runtime` (semua platform), `SELA.Editor`
  (Editor-only), `SELA.Tests` (gated `UNITY_INCLUDE_TESTS`)

### Seluruh dunia dibangun dari kode

Tidak ada file model 3D maupun gambar eksternal di project ini.

- **Geometri** dibangun dari primitif bawaan Unity (`GameObject.CreatePrimitive`)
  yang lalu di-*scale* dan diberi material.
- **Material** (25 buah) dibuat lewat `AssetDatabase` dan `Shader.Find("Universal Render Pipeline/Lit")`.
- **Tekstur** (5 PNG 64×64: keramik, plester, kayu, kain, kardus) digambar
  piksel per piksel di `BuatDetailRuang.BuatTekstur()`. Noise-nya memakai
  hash deterministik, jadi hasilnya identik setiap kali scene dibangun ulang.
- **Pencahayaan** memakai lampu titik di dalam kamar supaya suasananya
  hangat, bukan terang bener dari luar.

### Arsitektur

Semua sistem runtime bertukar keadaan lewat `GameState.Instance`
(singleton, `DontDestroyOnLoad`), dengan pembaruan berbasis event
(`OnStateChanged`, `OnNotifikasi`, `OnJedaTerbuka`) supaya UI berlangganan
alih-alih mem-poll. `FirstPersonController` dan class data statis sengaja
dipisahkan dari `GameState` supaya tidak terikat ke alur permainan.

Tidak ada `FindObjectOfType` di dalam `Update()` — pencarian objek hanya
dipakai di dalam test.

### Menyimpan progres

File save disimpan di `Application.persistentDataPath` (Windows:
`C:/Users/<user>/AppData/LocalLow/DefaultCompany/SELA/`), format JSON:

- `sela_save.json` — file utama
- `sela_save_tmp.json` — file sementara saat menulis
- `sela_save_backup.json` — checkpoint file sebelumnya

Penulisan dibuat crash-safe: tulis ke file sementara, salin file lama ke
checkpoint, baru timpa file utama. Kalau file utama rusak saat dimuat,
game menawarkan checkpoint sebelumnya. Progres disimpan otomatis setelah
setiap aktivitas, setiap kali tidur, dan setiap kali retakan ditutup.
Save manual juga ada di menu Esc.

## Struktur folder

```
Assets/Scripts/SELA/
├── Core/        GameState, SessionManager, SaveSystem, TujuanGame
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
`Assets/GameData/Naskah/Resources/`.

## Pengujian

Dua uji PlayMode ada di `Assets/Scripts/SELA/Tests/`:

- **`UjiSmokePlayMode`** — scene bisa dimuat, semua sistem dan 12 panel UI
  ditemukan, font TextMeshPro tersedia, pemain benar-benar berdiri di
  lantai (gravitasi manual jalan, dan tidak melayang setelah diangkat),
  tidak ada perabot yang menelan pemain di posisi awal, panel Catatan
  menampilkan tujuan dan checklist, laptop punya 3 tab yang terisi,
  mengambil kartu bukti menambah bukti ke tas, dan menjalankan aktivitas
  memotong tenaga, menambah tekanan, serta menggeser sesi.
- **`UjiTujuhHariPlayMode`** — main penuh tujuh hari: CV dirakit, lamaran A
  dan B terkirim pada waktunya, interview A dan B dilewati dengan nilai
  yang cukup, lowongan Akses Prima terverifikasi, dua tawaran masuk, dan
  panel MALAM TERAKHIR terbuka sampai layar ending.

Jalankan dari **Window > General > Test Runner > PlayMode > Run All**, atau
dari terminal:

```
Unity.exe -batchmode -runTests -testPlatform PlayMode \
  -projectPath <path project> -testResults hasil-uji.xml
```

Uji ini mengarahkan file save ke folder sementara, jadi save asli pemain
tidak ikut tersentuh.

## Credits

**Luhur Pangaribowo Handaruji** — naratif dan UI.

Pembagian tugas lengkap ada di panel **Credits dan Sumber** di dalam game.
Teks panel itu disimpan di `Assets/Scripts/SELA/UI/UIManager.cs` pada field
`teksCreditsPanel`, jadi tinggal diisi nama anggota dan bagian masing-masing.

**Yang dipakai**

- Unity 6 LTS + URP
- uGUI + TextMeshPro
- Input System
- ScriptableObject untuk data naskah dan aktivitas
- JsonUtility untuk save/load

**Sumber**

- Game Design Document oleh kelompok
- Naskah Game oleh kelompok

## Catatan

- Semua isi game — perusahaan, kontrak, pesan, dan nama tokoh — bersifat
  fiktif.
- Game ini bukan terapi, diagnosis, rehabilitasi, atau pelatihan hukum
  ketenagakerjaan. Ada layar **Info Konten** di menu awal yang menjelaskan
  hal ini.
- File percobaan pemanggilan API (`ClaudeConnector.cs` dan `TesClaude.cs`)
  sudah dihapus dari project ini dan tidak ada di dalam repo.
- Scene `Assets/Scenes/SELA_Play.unity` dibangun dari kode, tapi tetap
  ikut di-commit supaya repo bisa langsung dibuka tanpa perlu menjalankan
  ulang generator.
