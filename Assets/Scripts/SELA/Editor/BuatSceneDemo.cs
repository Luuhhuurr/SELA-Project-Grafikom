#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SELA.Editor
{
    // BuatSceneDemo.cs
    // Script EDITOR yang membuat scene demo SELA beserta asset ActivityData.
    //
    // Cara pakai (dari menu Unity):
    // Tools > SELA > Buat Scene Demo
    //
    // Cara pakai (batch mode / terminal):
    // Unity.exe -batchmode -projectPath &lt;project&gt; \
    // -executeMethod SELA.Editor.BuatSceneDemo.BuildFromCommandLine
    //
    // Yang dibuat:
    // - Assets/GameData/Aktivitas/*.asset  (13 ActivityData)
    // - Assets/GameData/Material/*.mat     (material URP sederhana)
    // - Assets/GameData/VolumeProfile.asset
    // - Assets/Scenes/SELA_Play.unity       (scene demo siap main)
    public static class BuatSceneDemo
    {
        private const string FolderGameData = "Assets/GameData";
        private const string FolderAktivitas = "Assets/GameData/Aktivitas";
        private const string FolderMaterial = "Assets/GameData/Material";
        private const string PathVolume = "Assets/GameData/VolumeProfile.asset";
        private const string PathScene = "Assets/Scenes/SELA_Play.unity";

        // ---------------------------------------------------------------
        // Entry point
        // ---------------------------------------------------------------

        [MenuItem("Tools/SELA/Buat Scene Demo", false, 1)]
        public static void BuildFromMenu()
        {
            try
            {
                Build();
                EditorUtility.DisplayDialog("SELA", "Scene demo + asset berhasil dibuat.\n\n"
                    + "Buka scene: Assets/Scenes/SELA_Play.unity\nTekan Play untuk main.", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError("[BuatSceneDemo] Gagal: " + e);
                EditorUtility.DisplayDialog("SELA", "Gagal membuat scene:\n" + e.Message, "OK");
            }
        }

        public static void BuildFromCommandLine()
        {
            try
            {
                Build();
                Debug.Log("[BuatSceneDemo] Selesai.");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[BuatSceneDemo] Gagal: " + e);
                EditorApplication.Exit(1);
            }
        }

        // ---------------------------------------------------------------
        // Langkah-langkah build
        // ---------------------------------------------------------------

        public static void Build()
        {
            SiapkanFolder();
            ImportTmpEssentials();

            Material lantai = BuatMaterial("M_Lantai", new Color(0.32f, 0.30f, 0.28f));
            Material dinding = BuatMaterial("M_Dinding", new Color(0.72f, 0.70f, 0.66f));
            Material mebel = BuatMaterial("M_Mebel", new Color(0.55f, 0.40f, 0.28f));
            Material kertas = BuatMaterial("M_Kertas", new Color(0.93f, 0.92f, 0.86f));
            Material pintu = BuatMaterial("M_Pintu", new Color(0.30f, 0.36f, 0.50f));
            Material retakan = BuatMaterial("M_Retakan", new Color(0.62f, 0.18f, 0.18f));
            Material barang = BuatMaterial("M_Barang", new Color(0.85f, 0.75f, 0.35f));

            List<ActivityData> daftarAktivitas = BuatSemuaAktivitas();

            BangunScene(daftarAktivitas, lantai, dinding, mebel, kertas, pintu, retakan, barang);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SiapkanFolder()
        {
            BuatFolderIfBelumAda("Assets/Scenes");
            BuatFolderIfBelumAda(FolderGameData);
            BuatFolderIfBelumAda(FolderAktivitas);
            BuatFolderIfBelumAda(FolderMaterial);
        }

        private static void BuatFolderIfBelumAda(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string nama = System.IO.Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                BuatFolderIfBelumAda(parent);
            }

            AssetDatabase.CreateFolder(parent, nama);
        }

        // Pastikan TMP Essential Resources ada di project.
        //
        // Kenapa tidak pakai AssetDatabase.ImportPackage? Karena import
        // package itu ASYNC, dan di batch mode Unity bisa keluar sebelum
        // import selesai. Jadi file .unitypackage-nya diekstrak manual di
        // sini (deterministik).
        private static void ImportTmpEssentials()
        {
            if (TmpSudahAda()) return;

            if (!SalinTmpResources())
            {
                Debug.LogWarning("[BuatSceneDemo] TMP Essential Resources gagal di-import. "
                    + "Teks di layar mungkin tidak tampil. Import manual lewat "
                    + "Window > TextMeshPro > Import TMP Essential Resources.");
                return;
            }

            AssetDatabase.Refresh();
        }

        private static bool TmpSudahAda()
        {
            string[] guid = AssetDatabase.FindAssets("t:TMP_Settings");
            return (guid != null && guid.Length > 0);
        }

        // Ekstrak "TMP Essential Resources.unitypackage" ke Assets/TextMesh Pro.
        // Format .unitypackage = tar yang dikompres gzip, berisi folder per
        // GUID. Tiap folder punya file "pathname" (tujuan asset di project)
        // dan "asset" (isi file) + "asset.meta".
        private static bool SalinTmpResources()
        {
            string folderPackage = CariFolderPackageTmp();
            if (string.IsNullOrEmpty(folderPackage)) return false;

            string filePackage = Path.Combine(folderPackage, "Package Resources", "TMP Essential Resources.unitypackage");
            if (!File.Exists(filePackage)) return false;

            string folderTemp = Path.Combine(Path.GetTempPath(), "sela_tmp_extract");
            string fileTar = Path.Combine(folderTemp, "isi.tar");

            try
            {
                if (Directory.Exists(folderTemp)) Directory.Delete(folderTemp, true);
                Directory.CreateDirectory(folderTemp);

                // 1. buka gzip -> tar
                using (FileStream fs = File.OpenRead(filePackage))
                using (FileStream fsTar = File.Create(fileTar))
                {
                    byte[] kepala = new byte[2];
                    fs.Read(kepala, 0, 2);
                    fs.Position = 0;

                    if (kepala[0] == 0x1f && kepala[1] == 0x8b)
                    {
                        using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
                        {
                            gz.CopyTo(fsTar);
                        }
                    }
                    else
                    {
                        fs.CopyTo(fsTar);
                    }
                }

                // 2. ekstrak tar
                using (FileStream fs = File.OpenRead(fileTar))
                {
                    EkstrakTar(fs, folderTemp);
                }

                // 3. salin asset ke project (pathname sudah mengandung
                //    "Assets/TextMesh Pro/..." jadi cwd-nya project root)
                string rootProject = Directory.GetParent(Application.dataPath).FullName;
                int jumlah = 0;

                string[] folderGuid = Directory.GetDirectories(folderTemp);
                for (int i = 0; i < folderGuid.Length; i++)
                {
                    string filePathname = Path.Combine(folderGuid[i], "pathname");
                    if (!File.Exists(filePathname)) continue;

                    string relatif = File.ReadAllText(filePathname).Trim().Replace('/', Path.DirectorySeparatorChar);
                    if (string.IsNullOrEmpty(relatif)) continue;

                    string fileAsli = Path.Combine(folderGuid[i], "asset");
                    if (!File.Exists(fileAsli)) continue;

                    string tujuanAsset = Path.Combine(rootProject, relatif);
                    Directory.CreateDirectory(Path.GetDirectoryName(tujuanAsset));
                    File.Copy(fileAsli, tujuanAsset, true);

                    string asalMeta = fileAsli + ".meta";
                    if (File.Exists(asalMeta))
                    {
                        File.Copy(asalMeta, tujuanAsset + ".meta", true);
                    }

                    jumlah++;
                }

                Debug.Log("[BuatSceneDemo] TMP Essential Resources disalin (" + jumlah + " asset).");
                return jumlah > 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BuatSceneDemo] Gagal mengekstrak TMP resources: " + e.Message);
                return false;
            }
        }

        private static string CariFolderPackageTmp()
        {
            // Cari lewat PackageInfo (paling aman).
            try
            {
                UnityEditor.PackageManager.PackageInfo info =
                    UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Settings).Assembly);
                if (info != null && !string.IsNullOrEmpty(info.resolvedPath)) return info.resolvedPath;
            }
            catch (Exception) { }

            // Cadangan: cari manual di PackageCache.
            string folderCache = "Library/PackageCache";
            if (Directory.Exists(folderCache))
            {
                string[] daftar = Directory.GetDirectories(folderCache);
                for (int i = 0; i < daftar.Length; i++)
                {
                    if (daftar[i].Contains("com.unity.ugui"))
                    {
                        string cek = Path.Combine(daftar[i], "Package Resources", "TMP Essential Resources.unitypackage");
                        if (File.Exists(cek)) return daftar[i];
                    }
                }
            }

            return string.Empty;
        }

        // Baca arsip tar dan tulis semua isinya ke folder tujuan.
        private static void EkstrakTar(Stream stream, string folderTujuan)
        {
            byte[] header = new byte[512];

            while (true)
            {
                if (!BacaPenuh(stream, header, 512)) break;
                if (header[0] == 0) break; // akhir arsip

                string nama = Encoding.UTF8.GetString(header, 0, 100).TrimEnd('\0');
                string prefix = Encoding.UTF8.GetString(header, 345, 155).TrimEnd('\0');
                if (!string.IsNullOrEmpty(prefix)) nama = prefix + "/" + nama;

                long ukuran = ParseOktal(header, 124, 12);
                byte tipe = header[156];

                long bulat = ((ukuran + 511) / 512) * 512;

                if (tipe == '5')
                {
                    Directory.CreateDirectory(Path.Combine(folderTujuan, nama));
                    stream.Seek(bulat, SeekOrigin.Current);
                }
                else
                {
                    string tujuan = Path.Combine(folderTujuan, nama);
                    Directory.CreateDirectory(Path.GetDirectoryName(tujuan));

                    using (FileStream fs = File.Create(tujuan))
                    {
                        SalinN(stream, fs, ukuran);
                    }

                    long sisa = bulat - ukuran;
                    if (sisa > 0) stream.Seek(sisa, SeekOrigin.Current);
                }
            }
        }

        private static bool BacaPenuh(Stream stream, byte[] buffer, int jumlah)
        {
            int terbaca = 0;
            while (terbaca < jumlah)
            {
                int n = stream.Read(buffer, terbaca, jumlah - terbaca);
                if (n <= 0) return false;
                terbaca += n;
            }
            return true;
        }

        private static void SalinN(Stream sumber, Stream tujuan, long jumlah)
        {
            byte[] buffer = new byte[8192];
            long sisa = jumlah;

            while (sisa > 0)
            {
                int n = sumber.Read(buffer, 0, (int)Math.Min(buffer.Length, sisa));
                if (n <= 0) break;
                tujuan.Write(buffer, 0, n);
                sisa -= n;
            }
        }

        private static long ParseOktal(byte[] data, int offset, int panjang)
        {
            long hasil = 0;
            for (int i = 0; i < panjang; i++)
            {
                byte b = data[offset + i];
                if (b == 0 || b == ' ') continue;
                if (b < '0' || b > '7') break;
                hasil = hasil * 8 + (b - '0');
            }
            return hasil;
        }

        // ---------------------------------------------------------------
        // Material
        // ---------------------------------------------------------------

        private static Material BuatMaterial(string nama, Color warna)
        {
            string path = FolderMaterial + "/" + nama + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (m == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Sprites/Default");

                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }

            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
            if (m.HasProperty("_Color")) m.SetColor("_Color", warna);

            EditorUtility.SetDirty(m);
            return m;
        }

        private static VolumeProfile BuatVolumeProfile()
        {
            VolumeProfile profil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PathVolume);
            if (profil == null)
            {
                profil = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profil, PathVolume);
            }

            ColorAdjustments warna = profil.Add<ColorAdjustments>(true);
            warna.postExposure.Override(0f);
            warna.contrast.Override(12f);
            warna.saturation.Override(-8f);

            Vignette vignette = profil.Add<Vignette>(true);
            vignette.intensity.Override(0.28f);
            vignette.smoothness.Override(0.4f);

            EditorUtility.SetDirty(profil);
            return profil;
        }

        // ---------------------------------------------------------------
        // Asset ActivityData
        // ---------------------------------------------------------------

        private static ActivityData BuatAktivitas(string namaAsset, string nama, string deskripsi,
                                                   int biayaTenaga, int biayaTekanan,
                                                   string[] syaratBukti, int hariMinimal,
                                                   bool hanyaSekali, bool sekaliPerHari,
                                                   string hasil, string catatanHasil,
                                                   ActivityData.Tipe tipe, string perusahaan,
                                                   int indeksPerusahaan, int hariInterview,
                                                   int hariMaks = 0)
        {
            string path = FolderAktivitas + "/" + namaAsset + ".asset";
            ActivityData data = AssetDatabase.LoadAssetAtPath<ActivityData>(path);

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ActivityData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.nama = nama;
            data.deskripsi = deskripsi;
            data.biayaTenaga = biayaTenaga;
            data.biayaTekanan = biayaTekanan;

            data.syaratBukti.Clear();
            if (syaratBukti != null)
            {
                for (int i = 0; i < syaratBukti.Length; i++)
                {
                    if (!string.IsNullOrEmpty(syaratBukti[i])) data.syaratBukti.Add(syaratBukti[i]);
                }
            }

            data.hariMinimal = hariMinimal;
            data.hariMaks = hariMaks;
            data.hanyaSekali = hanyaSekali;
            data.sekaliPerHari = sekaliPerHari;
            data.hasil = hasil;
            data.catatanHasil = catatanHasil;
            data.tipe = tipe;
            data.perusahaan = perusahaan;
            data.indeksPerusahaan = indeksPerusahaan;
            data.hariInterview = hariInterview;

            EditorUtility.SetDirty(data);
            return data;
        }

        private static List<ActivityData> BuatSemuaAktivitas()
        {
            List<ActivityData> daftar = new List<ActivityData>();

            // 1. Rapikan contoh antarmuka -> E1
            daftar.Add(BuatAktivitas(
                "Aktivitas_RapikanAntarmuka",
                "Rapikan Contoh Antarmuka",
                "Buka lagi formulir pendaftaran tugas besar. Rapikan label input dan pesan kesalahannya, lalu potret sebelum/sesudahnya.",
                1, 1, null, 1, true, false,
                "E1",
                "Antarmuka formulir jadi lebih rapi. Bukti E1 (gambar sebelum/sesudah) masuk ke tas.",
                ActivityData.Tipe.Biasa, "", -1, 1));

            // 2. Lengkapi catatan pengujian -> E2 (perlu E1, mulai hari 3)
            daftar.Add(BuatAktivitas(
                "Aktivitas_LengkapiCatatanUji",
                "Lengkapi Catatan Pengujian",
                "Tulis tiga kasus uji: kolom kosong, format salah, dan data valid. Tulis juga batas pengujiannya.",
                1, 1, new string[] { "E1" }, 3, true, false,
                "E2",
                "Catatan pengujian lengkap. Bukti E2 masuk ke tas.",
                ActivityData.Tipe.Biasa, "", -1, 1));

            // 3. Verifikasi A
            daftar.Add(BuatAktivitas(
                "Aktivitas_PeriksaStudioLajur",
                "Periksa Studio Lajur",
                "Baca profil dan kontak Studio Lajur. Catat empat hal: tugas, kompensasi, jam kerja, dan dokumen pribadi.",
                1, 0, null, 1, true, false,
                "",
                "Studio Lajur: profil dan kontak sesuai. Ada pendamping, lembur perlu persetujuan, dokumen asli dipegang pekerja.",
                ActivityData.Tipe.Verifikasi, "A", 0, 1));

            // 4. Verifikasi B
            daftar.Add(BuatAktivitas(
                "Aktivitas_PeriksaKoperasiPijar",
                "Periksa Koperasi Pijar",
                "Baca profil dan kontak Koperasi Pijar. Kontraknya 3 bulan, jadi cek dulu apa yang dijanjikan.",
                1, 0, null, 1, true, false,
                "",
                "Koperasi Pijar: kontak terkonfirmasi, tugas awal dibatasi, evaluasi akhir kontrak tidak menjamin perpanjangan.",
                ActivityData.Tipe.Verifikasi, "B", 1, 1));

            // 5. Verifikasi C
            daftar.Add(BuatAktivitas(
                "Aktivitas_PeriksaAksesPrima",
                "Periksa Akses Prima",
                "Cek alamat dan kontak Akses Prima. Pastikan juga apa yang mereka minta soal dokumen.",
                1, 0, null, 4, true, false,
                "",
                "Akses Prima: alamat website berbeda dengan alamat di kontrak, nomor telepon tidak aktif, rincian tugas ditolak saat diminta klarifikasi.",
                ActivityData.Tipe.Verifikasi, "C", 2, 1));

            // 6. Kirim lamaran A (butuh E1, interview hari 5)
            daftar.Add(BuatAktivitas(
                "Aktivitas_KirimLamaranA",
                "Kirim Lamaran ke Studio Lajur",
                "Kirim CV berisi klaim dan bukti yang kamu punya ke Studio Lajur. Lamaran dibekukan saat dikirim.",
                1, 1, new string[] { "E1" }, 1, true, false,
                "",
                "Lamaran ke Studio Lajur terkirim. Interview dijadwalkan sesi siang.",
                ActivityData.Tipe.KirimLamaran, "A", 0, 5, 3));

            // 7. Kirim lamaran B (butuh E1+E2, interview hari 6)
            daftar.Add(BuatAktivitas(
                "Aktivitas_KirimLamaranB",
                "Kirim Lamaran ke Koperasi Pijar",
                "Kirim CV berisi klaim dan bukti yang kamu punya ke Koperasi Pijar.",
                1, 1, new string[] { "E1", "E2" }, 3, true, false,
                "",
                "Lamaran ke Koperasi Pijar terkirim. Interview dijadwalkan sesi siang.",
                ActivityData.Tipe.KirimLamaran, "B", 1, 6, 5));

            // 8. Interview A
            daftar.Add(BuatAktivitas(
                "Aktivitas_IkutiInterviewA",
                "Ikuti Interview Studio Lajur",
                "Interview bersama Nara dari tim rekrutmen Studio Lajur. Sesi siang, dua pertanyaan.",
                1, 2, null, 5, true, false,
                "",
                "Interview Studio Lajur selesai. Cek tab Pesan di laptop.",
                ActivityData.Tipe.Interview, "A", 0, 5));

            // 9. Interview B
            daftar.Add(BuatAktivitas(
                "Aktivitas_IkutiInterviewB",
                "Ikuti Interview Koperasi Pijar",
                "Interview bersama Bagas dari Koperasi Pijar. Sesi siang, dua pertanyaan.",
                1, 2, null, 6, true, false,
                "",
                "Interview Koperasi Pijar selesai. Cek tab Pesan di laptop.",
                ActivityData.Tipe.Interview, "B", 1, 6));

            // 10. Istirahat (pemulihan)
            daftar.Add(BuatAktivitas(
                "Aktivitas_Istirahat",
                "Istirahat",
                "Rebahan sebentar di kasur. Tenaga naik, tekanan turun.",
                -3, -2, null, 1, false, true,
                "",
                "Raka istirahat. Tubuh terasa lebih ringan.",
                ActivityData.Tipe.Istirahat, "", -1, 1));

            // 11. Hubungi Dita
            daftar.Add(BuatAktivitas(
                "Aktivitas_HubungiDita",
                "Hubungi Dita",
                "Telepon Dita. Dia teman kuliah yang biasa bantu cek CV.",
                -2, -2, null, 1, false, true,
                "",
                "Dita: \"Bentar lagi selesai. Lo sudah punya bukti yang jelas. Apapun yang lo pilih, itu keputusan lo, bukan kegagalan.\"",
                ActivityData.Tipe.Dita, "", -1, 1));

            // 12. Duduk tanpa layar
            daftar.Add(BuatAktivitas(
                "Aktivitas_DudukTanpaLayar",
                "Duduk Tanpa Layar",
                "Duduk di kursi dekat jendela tanpa membuka laptop. Cumaidah.",
                -2, -1, null, 1, false, true,
                "",
                "Raka duduk sebentar. Tidak ada yang harus dijawab.",
                ActivityData.Tipe.Biasa, "", -1, 1));

            // 13. Rapikan meja
            daftar.Add(BuatAktivitas(
                "Aktivitas_RapikanMeja",
                "Rapikan Meja",
                "Bersihkan meja: tumpukan revisi CV disatukan, lakban dilepas, kabel digulung.",
                1, -1, null, 1, false, true,
                "",
                "Meja jadi rapi. Semuanya terlihat lebih jelas.",
                ActivityData.Tipe.Biasa, "", -1, 1));

            return daftar;
        }

        // ---------------------------------------------------------------
        // Membangun scene
        // ---------------------------------------------------------------

        private static void BangunScene(List<ActivityData> daftarAktivitas,
                                        Material lantai, Material dinding, Material mebel,
                                        Material kertas, Material pintu, Material retakan,
                                        Material barang)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Objek yang perlu dihubungkan ke UIManager setelah UI dibuat.
            ObjekInteraktif objLaptop = null;
            ObjekInteraktif objKasur = null;

            // ---------- Pencahayaan ----------
            GameObject goLight = new GameObject("Directional Light");
            goLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = goLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.90f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;

            GameObject goVolume = new GameObject("Global Volume");
            Volume volume = goVolume.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = BuatVolumeProfile();

            // ---------- Lantai ----------
            Kubik("Lantai", new Vector3(4f, -0.1f, -1f), new Vector3(26f, 0.2f, 14f), lantai);

            // ============================================================
            // Kamar 07
            // ============================================================
            GameObject kamar = new GameObject("Kamar 07");

            Dinding("Dinding_Kamar_Belakang", new Vector3(0f, 1.5f, 2.5f), new Vector3(6.2f, 3f, 0.2f), dinding, kamar);
            Dinding("Dinding_Kamar_Depan", new Vector3(0f, 1.5f, -2.5f), new Vector3(6.2f, 3f, 0.2f), dinding, kamar);
            Dinding("Dinding_Kamar_Kiri", new Vector3(-3f, 1.5f, 0f), new Vector3(0.2f, 3f, 5.2f), dinding, kamar);
            // Dinding kanan dibuat dua bagian, ada pintu di tengah (z = 0).
            Dinding("Dinding_Kamar_Kanan_1", new Vector3(3f, 1.5f, 1.65f), new Vector3(0.2f, 3f, 1.9f), dinding, kamar);
            Dinding("Dinding_Kamar_Kanan_2", new Vector3(3f, 1.5f, -1.65f), new Vector3(0.2f, 3f, 1.9f), dinding, kamar);

            // Meja kerja
            GameObject meja = Kubik("Meja", new Vector3(-1.5f, 0.4f, -1.2f), new Vector3(1.8f, 0.8f, 0.9f), mebel, kamar);

            // Laptop di meja
            GameObject laptop = Kubik("Laptop", new Vector3(-1.5f, 0.84f, -1.2f), new Vector3(0.55f, 0.05f, 0.38f), barang, meja);
            objLaptop = laptop.AddComponent<ObjekInteraktif>();
            objLaptop.namaTampilan = "Buka Laptop";

            // Kartu pesan ibu (benda puzzle Retakan 3)
            GameObject kartuIbu = Kubik("KartuIbu", new Vector3(-1.1f, 0.87f, -0.95f), new Vector3(0.22f, 0.02f, 0.16f), kertas, meja);
            ObjekInteraktif objKartuIbu = kartuIbu.AddComponent<ObjekInteraktif>();
            objKartuIbu.namaTampilan = "Kartu Pesan Ibu";
            objKartuIbu.ambilSebagaiBenda = true;
            objKartuIbu.namaBenda = "KartuIbu";

            // Kasur
            GameObject kasur = Kubik("Kasur", new Vector3(1.7f, 0.3f, 1.4f), new Vector3(1.2f, 0.6f, 2f), mebel, kamar);
            objKasur = kasur.AddComponent<ObjekInteraktif>();
            objKasur.namaTampilan = "Tidur";

            // Rak buku
            Kubik("Rak Buku", new Vector3(-2.5f, 0.6f, 1.2f), new Vector3(0.5f, 1.2f, 1.6f), mebel, kamar);

            // Jendela + kursi (jangkar Retakan 3)
            Kubik("Jendela", new Vector3(1.7f, 1.6f, 2.4f), new Vector3(1.2f, 1.2f, 0.1f), barang, kamar);
            GameObject kursi = Kubik("Kursi Jendela", new Vector3(1.7f, 0.45f, 1.9f), new Vector3(0.5f, 0.9f, 0.5f), mebel, kamar);

            GameObject anchor3Go = new GameObject("Retakan3_Anchor");
            anchor3Go.transform.SetParent(kursi.transform, false);
            anchor3Go.transform.localPosition = new Vector3(0f, 0.52f, -0.2f);
            anchor3Go.AddComponent<BoxCollider>();
            PuzzleAnchor anchor3 = anchor3Go.AddComponent<PuzzleAnchor>();
            anchor3.namaTampilan = "Pasang di dudukan kursi";
            anchor3.indeksRetakan = 2;
            anchor3.hariMinimal = 6;
            anchor3.namaBendaYangDibutuhkan = "KartuIbu";
            anchor3.objekEfek = EfekRetakan(anchor3Go.transform, retakan, new Vector3(0f, 0.1f, 0f));
            anchor3.labelTujuan = BuatLabel3D("LabelTujuan_Retakan3", "Dudukan kosong menghadap jendela.",
                                             new Vector3(1.7f, 1.45f, 1.75f), Quaternion.Euler(0f, 180f, 0f));
            anchor3.labelTujuan.SetActive(false);
            anchor3.pesanBendaSalah = "Kartu ini bukan untuk dudukan kursi itu.";

            // ============================================================
            // Lorong Kos
            // ============================================================
            GameObject lorong = new GameObject("Lorong Kos");

            Dinding("Dinding_Lorong_Atas", new Vector3(6f, 1.5f, 1.5f), new Vector3(6.2f, 3f, 0.2f), dinding, lorong);
            Dinding("Dinding_Lorong_Ujung", new Vector3(9f, 1.5f, 0f), new Vector3(0.2f, 3f, 3.2f), dinding, lorong);
            // Dinding bawah lorong ada celah di x = 5.5..8 untuk masuk ke Ruang Terpanjang.
            Dinding("Dinding_Lorong_Bawah_1", new Vector3(4.25f, 1.5f, -1.5f), new Vector3(2.5f, 3f, 0.2f), dinding, lorong);
            Dinding("Dinding_Lorong_Bawah_2", new Vector3(8.5f, 1.5f, -1.5f), new Vector3(1f, 3f, 0.2f), dinding, lorong);

            // Bangku + kunci 07
            GameObject bangku = Kubik("Bangku", new Vector3(5f, 0.25f, 0.8f), new Vector3(0.5f, 0.5f, 1.6f), mebel, lorong);
            GameObject kunci = Kubik("Kunci07", new Vector3(5f, 0.62f, 0.8f), new Vector3(0.12f, 0.24f, 0.05f), barang, bangku);
            ObjekInteraktif objKunci = kunci.AddComponent<ObjekInteraktif>();
            objKunci.namaTampilan = "Gantungan Kunci 07";
            objKunci.ambilSebagaiBenda = true;
            objKunci.namaBenda = "Kunci07";

            // Tiga pintu di ujung lorong
            Kubik("Pintu_DITERIMA", new Vector3(8.8f, 1f, 1.1f), new Vector3(0.12f, 2f, 0.9f), pintu, lorong);
            Kubik("Pintu_TERLAMBAT", new Vector3(8.8f, 1f, -1.1f), new Vector3(0.12f, 2f, 0.9f), pintu, lorong);

            GameObject pintu07 = Kubik("Pintu_07", new Vector3(8.8f, 1f, 0f), new Vector3(0.12f, 2f, 0.9f), pintu, lorong);
            PuzzleAnchor anchor2 = pintu07.AddComponent<PuzzleAnchor>();
            anchor2.namaTampilan = "Pakai kunci pada pintu 07";
            anchor2.indeksRetakan = 1;
            anchor2.hariMinimal = 4;
            anchor2.namaBendaYangDibutuhkan = "Kunci07";
            anchor2.objekEfek = EfekRetakan(pintu07.transform, retakan, new Vector3(-0.12f, 0.35f, 0f));
            anchor2.labelTujuan = BuatLabel3D("LabelTujuan_Retakan2", "Pintu dengan label 07.",
                                             new Vector3(8.55f, 1.75f, 0f), Quaternion.Euler(0f, -90f, 0f));
            anchor2.labelTujuan.SetActive(false);
            anchor2.pesanBendaSalah = "Kunci ini bukan untuk pintu 07.";

            // ============================================================
            // Ruang Terpanjang
            // ============================================================
            GameObject ruang = new GameObject("Ruang Terpanjang");

            Dinding("Dinding_Ruang_Bawah", new Vector3(8.25f, 1.5f, -6f), new Vector3(5.5f, 3f, 0.2f), dinding, ruang);
            Dinding("Dinding_Ruang_Kanan", new Vector3(11f, 1.5f, -3.75f), new Vector3(0.2f, 3f, 4.7f), dinding, ruang);
            Dinding("Dinding_Ruang_Kiri", new Vector3(5.5f, 1.5f, -3.75f), new Vector3(0.2f, 3f, 4.7f), dinding, ruang);

            // Meja panjang + penjepit foto (jangkar Retakan 1)
            GameObject mejaPanjang = Kubik("Meja Panjang", new Vector3(6.4f, 0.4f, -3.2f), new Vector3(2.4f, 0.8f, 0.9f), mebel, ruang);

            GameObject penjepit = Kubik("Penjepit Foto", new Vector3(7.1f, 0.88f, -3.2f), new Vector3(0.12f, 0.16f, 0.12f), barang, mejaPanjang);
            PuzzleAnchor anchor1 = penjepit.AddComponent<PuzzleAnchor>();
            anchor1.namaTampilan = "Pasang kartu di penjepit foto";
            anchor1.indeksRetakan = 0;
            anchor1.hariMinimal = 2;
            anchor1.namaBendaYangDibutuhkan = "KartuE0";
            anchor1.objekEfek = EfekRetakan(penjepit.transform, retakan, new Vector3(0f, 0.18f, 0f));
            anchor1.labelTujuan = BuatLabel3D("LabelTujuan_Retakan1", "Penjepit foto di ujung meja.",
                                             new Vector3(7.1f, 1.45f, -3.05f), Quaternion.Euler(0f, 0f, 0f));
            anchor1.labelTujuan.SetActive(false);
            anchor1.pesanBendaSalah = "Kartu ini tidak cocok dengan penjepit foto.";

            // Rak + kartu E0
            GameObject rakPanjang = Kubik("Rak Panjang", new Vector3(9.5f, 0.6f, -4.5f), new Vector3(0.5f, 1.2f, 1.6f), mebel, ruang);
            GameObject kartuE0 = Kubik("KartuBukti_E0", new Vector3(9.5f, 0.88f, -4.5f), new Vector3(0.24f, 0.02f, 0.17f), kertas, rakPanjang);
            EvidenceCard kartu = kartuE0.AddComponent<EvidenceCard>();
            kartu.namaTampilan = "Kartu Proyek";
            kartu.idBukti = "E0";
            kartu.objekVisual = kartuE0;
            kartu.ambilSebagaiBenda = true;
            kartu.namaBenda = "KartuE0";
            kartu.sembunyiSetelahDiambil = true;

            // ---------- Pemain ----------
            GameObject pemain = new GameObject("Pemain");
            pemain.transform.position = new Vector3(0.5f, 0.1f, 1.2f);

            CharacterController karakter = pemain.AddComponent<CharacterController>();
            karakter.height = 1.8f;
            karakter.center = new Vector3(0f, 0.9f, 0f);
            karakter.radius = 0.28f;
            karakter.slopeLimit = 45f;
            karakter.stepOffset = 0.3f;

            FirstPersonController fp = pemain.AddComponent<FirstPersonController>();

            GameObject kamera = new GameObject("Camera");
            kamera.transform.SetParent(pemain.transform, false);
            kamera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            Camera cam = kamera.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 120f;
            kamera.AddComponent<AudioListener>();
            try { kamera.AddComponent<UniversalAdditionalCameraData>(); } catch (Exception) { }
            kamera.tag = "MainCamera";

            // ---------- Sistem ----------
            GameObject sistem = new GameObject("Sistem");

            GameState gameState = sistem.AddComponent<GameState>();
            SessionManager sesiManager = sistem.AddComponent<SessionManager>();
            ActivitySystem activitySystem = sistem.AddComponent<ActivitySystem>();
            CVSystem cvSystem = sistem.AddComponent<CVSystem>();
            InterviewManager interviewManager = sistem.AddComponent<InterviewManager>();
            SimpleInventory inventory = sistem.AddComponent<SimpleInventory>();
            EndingManager endingManager = sistem.AddComponent<EndingManager>();

            activitySystem.daftarAktivitas.Clear();
            for (int i = 0; i < daftarAktivitas.Count; i++)
            {
                activitySystem.daftarAktivitas.Add(daftarAktivitas[i]);
            }

            gameState.sesiManager = sesiManager;
            gameState.activitySystem = activitySystem;
            gameState.cvSystem = cvSystem;
            gameState.interviewManager = interviewManager;
            gameState.inventory = inventory;

            // ---------- Canvas + UI ----------
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            UIManager ui = canvasGo.AddComponent<UIManager>();
            ui.kanvas = canvasGo;
            ui.firstPersonController = fp;

            BangunSeluruhPanel(canvasGo.transform, ui);

            // Hubungkan objek 3D dengan UIManager.
            if (objLaptop != null && objLaptop.onInteract != null) objLaptop.onInteract.AddListener(delegate { ui.BukaPanelLaptop(); });
            if (objKasur != null && objKasur.onInteract != null) objKasur.onInteract.AddListener(delegate { ui.Tidur(); });

            // ---------- EventSystem ----------
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            // ---------- Simpan ----------
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, PathScene);

            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(PathScene, true)
            };

            Debug.Log("[BuatSceneDemo] Scene demo tersimpan di " + PathScene);
        }

        // ---------------------------------------------------------------
        // Helper pembuatan objek 3D
        // ---------------------------------------------------------------

        private static GameObject Kubik(string nama, Vector3 posisi, Vector3 ukuran,
                                        Material material, GameObject induk = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nama;
            go.transform.position = posisi;
            go.transform.localScale = ukuran;

            if (material != null && go.GetComponent<Renderer>() != null)
            {
                go.GetComponent<Renderer>().sharedMaterial = material;
            }

            if (induk != null) go.transform.SetParent(induk.transform, true);

            return go;
        }

        private static GameObject Dinding(string nama, Vector3 posisi, Vector3 ukuran,
                                          Material material, GameObject induk)
        {
            return Kubik(nama, posisi, ukuran, material, induk);
        }

        // Mesh retakan merah yang menempel di jangkar puzzle.
        // Collider-nya dimatikan supaya tidak menghalangi raycast tombol E.
        private static GameObject EfekRetakan(Transform induk, Material material, Vector3 offsetLokal)
        {
            GameObject go = Kubik("EfekRetakan", Vector3.zero, new Vector3(0.3f, 0.3f, 0.06f), material, null);
            go.transform.SetParent(induk, false);
            go.transform.localPosition = offsetLokal;
            go.transform.localRotation = Quaternion.identity;

            BoxCollider col = go.GetComponent<BoxCollider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            return go;
        }

        // Membuat label 3D (Canvas world space) berisi teks.
        // Dipakai sebagai "label tujuan" pada puzzle Retakan Ruang.
        private static GameObject BuatLabel3D(string nama, string teks, Vector3 posisi, Quaternion rotasi)
        {
            GameObject go = new GameObject(nama, typeof(RectTransform));
            go.transform.position = posisi;
            go.transform.rotation = rotasi;
            go.transform.localScale = Vector3.one;

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2.2f, 0.55f);

            GameObject latar = new GameObject("Latar", typeof(RectTransform));
            latar.transform.SetParent(go.transform, false);
            RectTransform rtLatar = latar.GetComponent<RectTransform>();
            rtLatar.anchorMin = Vector2.zero;
            rtLatar.anchorMax = Vector2.one;
            rtLatar.offsetMin = Vector2.zero;
            rtLatar.offsetMax = Vector2.zero;
            Image img = latar.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.55f);
            img.raycastTarget = false;

            GameObject goTeks = new GameObject("Teks", typeof(RectTransform));
            goTeks.transform.SetParent(go.transform, false);
            RectTransform rtTeks = goTeks.GetComponent<RectTransform>();
            rtTeks.anchorMin = Vector2.zero;
            rtTeks.anchorMax = Vector2.one;
            rtTeks.offsetMin = new Vector2(8f, 4f);
            rtTeks.offsetMax = new Vector2(-8f, -4f);

            TextMeshProUGUI tmp = goTeks.AddComponent<TextMeshProUGUI>();
            tmp.text = teks;
            tmp.fontSize = 0.13f;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return go;
        }

        // ---------------------------------------------------------------
        // Helper pembuatan UI
        // ---------------------------------------------------------------

        private static GameObject BuatPanel(Transform induk, string nama, Color warnaLatar, bool bisaDiklik)
        {
            GameObject go = new GameObject(nama, typeof(RectTransform));
            go.transform.SetParent(induk, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color = warnaLatar;
            img.raycastTarget = bisaDiklik;

            return go;
        }

        private static TextMeshProUGUI BuatTeks(Transform induk, string nama, string teks,
                                                Vector2 anchorMin, Vector2 anchorMax,
                                                float ukuran, TextAlignmentOptions perataan)
        {
            GameObject go = new GameObject(nama, typeof(RectTransform));
            go.transform.SetParent(induk, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(24f, 16f);
            rt.offsetMax = new Vector2(-24f, -16f);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = teks;
            tmp.fontSize = ukuran;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.alignment = perataan;
            tmp.raycastTarget = false;

            return tmp;
        }

        private static RectTransform BuatWadah(Transform induk, string nama,
                                               Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(nama, typeof(RectTransform));
            go.transform.SetParent(induk, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(24f, 16f);
            rt.offsetMax = new Vector2(-24f, -16f);

            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rt;
        }

        private static Button BuatTombolUI(Transform induk, string nama, string label,
                                           Vector2 anchorMin, Vector2 anchorMax,
                                           Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(nama, typeof(RectTransform));
            go.transform.SetParent(induk, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.19f, 0.23f, 0.95f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject goTeks = new GameObject("Teks", typeof(RectTransform));
            goTeks.transform.SetParent(go.transform, false);
            RectTransform rtTeks = goTeks.GetComponent<RectTransform>();
            rtTeks.anchorMin = Vector2.zero;
            rtTeks.anchorMax = Vector2.one;
            rtTeks.offsetMin = new Vector2(12f, 6f);
            rtTeks.offsetMax = new Vector2(-12f, -6f);

            TextMeshProUGUI tmp = goTeks.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            return btn;
        }

        private static void BangunSeluruhPanel(Transform canvas, UIManager ui)
        {
            Color latar = new Color(0.04f, 0.05f, 0.07f, 0.90f);

            // ---------- Panel HUD ----------
            GameObject pHud = BuatPanel(canvas, "PanelHUD", new Color(0f, 0f, 0f, 0.45f), false);
            RectTransform rtHud = pHud.GetComponent<RectTransform>();
            rtHud.anchorMin = new Vector2(0f, 1f);
            rtHud.anchorMax = new Vector2(0f, 1f);
            rtHud.pivot = new Vector2(0f, 1f);
            rtHud.anchoredPosition = Vector2.zero;
            rtHud.sizeDelta = new Vector2(540f, 230f);
            ui.teksHUD = BuatTeks(pHud.transform, "Teks", "Hari 1 / 7", Vector2.zero, Vector2.one, 20f, TextAlignmentOptions.TopLeft);

            // ---------- Panel Prompt ----------
            GameObject pPrompt = BuatPanel(canvas, "PanelPrompt", new Color(0f, 0f, 0f, 0f), false);
            RectTransform rtPrompt = pPrompt.GetComponent<RectTransform>();
            rtPrompt.anchorMin = new Vector2(0.5f, 0f);
            rtPrompt.anchorMax = new Vector2(0.5f, 0f);
            rtPrompt.pivot = new Vector2(0.5f, 0f);
            rtPrompt.anchoredPosition = new Vector2(0f, 120f);
            rtPrompt.sizeDelta = new Vector2(760f, 60f);
            ui.teksPrompt = BuatTeks(pPrompt.transform, "Teks", "[E] Ambil Kartu", Vector2.zero, Vector2.one, 24f, TextAlignmentOptions.Center);

            // ---------- Panel Notifikasi ----------
            GameObject pNotif = BuatPanel(canvas, "PanelNotifikasi", new Color(0f, 0f, 0f, 0f), false);
            RectTransform rtNotif = pNotif.GetComponent<RectTransform>();
            rtNotif.anchorMin = new Vector2(0.5f, 1f);
            rtNotif.anchorMax = new Vector2(0.5f, 1f);
            rtNotif.pivot = new Vector2(0.5f, 1f);
            rtNotif.anchoredPosition = new Vector2(0f, -40f);
            rtNotif.sizeDelta = new Vector2(1200f, 90f);
            ui.teksNotifikasi = BuatTeks(pNotif.transform, "Teks", "", Vector2.zero, Vector2.one, 22f, TextAlignmentOptions.Center);

            // ---------- Panel Dialog ----------
            GameObject pDialog = BuatPanel(canvas, "PanelDialog", latar, true);
            ui.teksDialogPembicara = BuatTeks(pDialog.transform, "Pembicara", "Nara", new Vector2(0f, 1f), new Vector2(1f, 1f), 30f, TextAlignmentOptions.TopLeft);
            ui.teksDialogTeks = BuatTeks(pDialog.transform, "Teks", "", new Vector2(0f, 0.32f), new Vector2(1f, 0.78f), 24f, TextAlignmentOptions.TopLeft);
            ui.wadahOpsiDialog = BuatWadah(pDialog.transform, "DaftarOpsi", new Vector2(0f, 0.05f), new Vector2(1f, 0.30f));
            ui.tombolLanjutDialog = BuatTombolUI(pDialog.transform, "TombolLanjut", "Lanjut (Enter)", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-280f, 24f), new Vector2(-40f, 84f));

            // ---------- Panel Catatan ----------
            GameObject pCatatan = BuatPanel(canvas, "PanelCatatan", latar, true);
            ui.teksCatatan = BuatTeks(pCatatan.transform, "Teks", "CATATAN RAKA", Vector2.zero, new Vector2(1f, 1f), 22f, TextAlignmentOptions.TopLeft);
            Button tombolTidur = BuatTombolUI(pCatatan.transform, "TombolTidur", "Tidur (malam)", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(260f, 86f));
            tombolTidur.onClick.AddListener(delegate { ui.Tidur(); });

            // ---------- Panel Laptop ----------
            GameObject pLaptop = BuatPanel(canvas, "PanelLaptop", latar, true);
            ui.teksLaptop = BuatTeks(pLaptop.transform, "Teks", "LAMARAN", new Vector2(0.5f, 0f), new Vector2(1f, 1f), 20f, TextAlignmentOptions.TopLeft);
            ui.wadahAktivitas = BuatWadah(pLaptop.transform, "DaftarAktivitas", new Vector2(0f, 0f), new Vector2(0.48f, 1f));

            Button tab1 = BuatTombolUI(pLaptop.transform, "TombolTab1", "Lamaran", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -84f), new Vector2(200f, -24f));
            Button tab2 = BuatTombolUI(pLaptop.transform, "TombolTab2", "Pesan", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(210f, -84f), new Vector2(380f, -24f));
            Button tab3 = BuatTombolUI(pLaptop.transform, "TombolTab3", "Catatan", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, -84f), new Vector2(560f, -24f));
            tab1.onClick.AddListener(delegate { ui.GantiTabLaptop(0); });
            tab2.onClick.AddListener(delegate { ui.GantiTabLaptop(1); });
            tab3.onClick.AddListener(delegate { ui.GantiTabLaptop(2); });
            ui.tombolRakitCV = BuatTombolUI(pLaptop.transform, "TombolRakitCV", "Rakit CV", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-240f, -84f), new Vector2(-40f, -24f));

            // ---------- Panel Konfirmasi Aktivitas ----------
            GameObject pKonf = BuatPanel(canvas, "PanelKonfirmasiAktivitas", latar, true);
            ui.teksKonfirmasi = BuatTeks(pKonf.transform, "Teks", "Aktivitas", Vector2.zero, Vector2.one, 24f, TextAlignmentOptions.TopLeft);
            ui.tombolKonfirmasiYa = BuatTombolUI(pKonf.transform, "TombolYa", "Ya, jalankan", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-280f, 40f), new Vector2(-20f, 110f));
            ui.tombolKonfirmasiBatal = BuatTombolUI(pKonf.transform, "TombolBatal", "Batal", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(20f, 40f), new Vector2(260f, 110f));

            // ---------- Panel Pause ----------
            GameObject pPause = BuatPanel(canvas, "PanelPause", latar, true);
            ui.teksPause = BuatTeks(pPause.transform, "Teks", "SELA", Vector2.zero, new Vector2(1f, 1f), 24f, TextAlignmentOptions.TopLeft);
            ui.tombolPauseLanjut = BuatTombolUI(pPause.transform, "TombolLanjut", "Lanjutkan", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-200f, 200f), new Vector2(200f, 270f));
            ui.tombolPauseSimpan = BuatTombolUI(pPause.transform, "TombolSimpan", "Simpan", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-200f, 120f), new Vector2(200f, 190f));
            ui.tombolPauseMulaiUlang = BuatTombolUI(pPause.transform, "TombolMulaiUlang", "Mulai ulang", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-200f, 40f), new Vector2(200f, 110f));
            ui.tombolPauseKeluar = BuatTombolUI(pPause.transform, "TombolKeluar", "Keluar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-200f, -40f), new Vector2(200f, 30f));

            // ---------- Panel Jeda ----------
            GameObject pJeda = BuatPanel(canvas, "PanelJeda", latar, true);
            ui.teksJeda = BuatTeks(pJeda.transform, "Teks", "Raka berhenti sebentar.", Vector2.zero, new Vector2(1f, 1f), 24f, TextAlignmentOptions.TopLeft);
            ui.tombolJedaLanjut = BuatTombolUI(pJeda.transform, "TombolLanjut", "Lanjut (Enter)", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-160f, 60f), new Vector2(160f, 130f));

            // ---------- Panel Pilihan Final ----------
            GameObject pFinal = BuatPanel(canvas, "PanelPilihanFinal", latar, true);
            ui.teksPilihanFinal = BuatTeks(pFinal.transform, "Teks", "MALAM TERAKHIR", new Vector2(0f, 0.28f), new Vector2(1f, 1f), 24f, TextAlignmentOptions.TopLeft);
            ui.wadahPilihanFinal = BuatWadah(pFinal.transform, "DaftarPilihan", new Vector2(0f, 0f), new Vector2(1f, 0.26f));

            // ---------- Panel Peringatan C ----------
            GameObject pPering = BuatPanel(canvas, "PanelPeringatanC", new Color(0.25f, 0.05f, 0.05f, 0.95f), true);
            ui.teksPeringatanC = BuatTeks(pPering.transform, "Teks", "PERINGATAN", new Vector2(0f, 0.24f), new Vector2(1f, 1f), 26f, TextAlignmentOptions.TopLeft);
            ui.tombolPeringatanYakin = BuatTombolUI(pPering.transform, "TombolYakin", "Ya, saya yakin", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-280f, 40f), new Vector2(0f, 110f));
            ui.tombolPeringatanBatal = BuatTombolUI(pPering.transform, "TombolBatal", "Batal", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(280f, 110f));

            // ---------- Panel Ending ----------
            GameObject pEnding = BuatPanel(canvas, "PanelEnding", new Color(0.02f, 0.02f, 0.03f, 0.97f), true);
            ui.teksEnding = BuatTeks(pEnding.transform, "Teks", "", Vector2.zero, Vector2.one, 22f, TextAlignmentOptions.TopLeft);
            ui.tombolKembaliMenu = BuatTombolUI(pEnding.transform, "TombolKembaliMenu", "Kembali ke menu utama", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-320f, 24f), new Vector2(-40f, 84f));

            // ---------- Panel Rakit CV ----------
            GameObject pRakit = BuatPanel(canvas, "PanelRakitCV", latar, true);
            ui.teksRakitCV = BuatTeks(pRakit.transform, "Teks", "RAKIT CV", new Vector2(0f, 0.20f), new Vector2(1f, 1f), 22f, TextAlignmentOptions.TopLeft);
            ui.wadahPilihanCV = BuatWadah(pRakit.transform, "DaftarPilihanCV", new Vector2(0f, 0f), new Vector2(1f, 0.20f));
            ui.tombolTutupRakitCV = BuatTombolUI(pRakit.transform, "TombolTutup", "Tutup (Esc)", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-240f, 24f), new Vector2(-40f, 84f));

            // ---------- Panel Verifikasi ----------
            GameObject pVerif = BuatPanel(canvas, "PanelVerifikasi", latar, true);
            ui.teksVerifikasi = BuatTeks(pVerif.transform, "Teks", "CEK LOWONGAN", new Vector2(0f, 0.16f), new Vector2(1f, 1f), 20f, TextAlignmentOptions.TopLeft);
            ui.tombolVerifikasiSudah = BuatTombolUI(pVerif.transform, "TombolSudahDicek", "Sudah dicek", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-280f, 40f), new Vector2(-20f, 110f));
            ui.tombolVerifikasiBatal = BuatTombolUI(pVerif.transform, "TombolBatal", "Batal", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(20f, 40f), new Vector2(260f, 110f));

            // ---------- Panel Pengaturan ----------
            GameObject pAtur = BuatPanel(canvas, "PanelPengaturan", latar, true);
            ui.teksPengaturan = BuatTeks(pAtur.transform, "Teks", "PENGATURAN", new Vector2(0f, 0.20f), new Vector2(1f, 1f), 22f, TextAlignmentOptions.TopLeft);
            ui.wadahPengaturan = BuatWadah(pAtur.transform, "DaftarPengaturan", new Vector2(0f, 0f), new Vector2(1f, 0.20f));
            ui.tombolTutupPengaturan = BuatTombolUI(pAtur.transform, "TombolTutup", "Tutup (Esc)", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-240f, 24f), new Vector2(-40f, 84f));

            // ---------- Panel Credits ----------
            GameObject pCredits = BuatPanel(canvas, "PanelCredits", latar, true);
            ui.teksCredits = BuatTeks(pCredits.transform, "Teks", "", Vector2.zero, new Vector2(1f, 1f), 19f, TextAlignmentOptions.TopLeft);
            ui.tombolTutupCredits = BuatTombolUI(pCredits.transform, "TombolTutup", "Tutup (Esc)", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-240f, 24f), new Vector2(-40f, 84f));

            // ---------- Panel Menu Awal ----------
            // Dibuat paling akhir supaya menimpa panel lain (di atas).
            // Tombol menunya dibuat UIManager lewat kode.
            GameObject pMenu = BuatPanel(canvas, "PanelMenuAwal", new Color(0.05f, 0.06f, 0.08f, 1f), true);
            ui.teksMenuAwal = BuatTeks(pMenu.transform, "Teks", "SELA", Vector2.zero, new Vector2(1f, 1f), 30f, TextAlignmentOptions.Top);

            // ---------- Panel Info Konten ----------
            GameObject pInfo = BuatPanel(canvas, "PanelInfoKonten", new Color(0.05f, 0.06f, 0.08f, 1f), true);
            ui.teksInfoKonten = BuatTeks(pInfo.transform, "Teks", "INFO KONTEN", Vector2.zero, new Vector2(1f, 1f), 19f, TextAlignmentOptions.TopLeft);
        }
    }
}
#endif

/* ============================================================
   [CARA PASANG] BuatSceneDemo.cs
   1. File ini HARUS berada di folder yang namanya "Editor"
      (Assets/Scripts/SELA/Editor/). Kalau tidak, Unity tidak
      mengenali sebagai script editor dan bisa error saat build.
   2. Script ini tidak perlu ditaruh di GameObject apa pun.
   3. Cara menjalankan:
      - Dari Unity: menu Tools > SELA > Buat Scene Demo
      - Dari terminal (batch mode):
        "C:/Program Files/Unity/Hub/Editor/6000.3.27f1/Editor/Unity.exe" ^
          -batchmode -projectPath "b:/Unity Project/Universal 3D/SELA" ^
          -executeMethod SELA.Editor.BuatSceneDemo.BuildFromCommandLine
   4. Hasilnya:
      - Assets/GameData/Aktivitas/  : 13 asset ActivityData
      - Assets/GameData/Material/     : material URP sederhana
      - Assets/GameData/VolumeProfile.asset
      - Assets/Scenes/SELA_Play.unity : scene demo
   5. Scene SELA_Play otomatis jadi scene pertama di Build Settings.
   6. Kalau script ini error, cek dulu:
      - TMP Essential Resources sudah di-import (script ini mengimpornya otomatis)
      - Tidak ada GameObject bernama "Pemain" ganda di scene
   7. Script ini boleh dihapus setelah scene selesai dibuat. Tidak ada
      kode runtime yang memakainya.
   ============================================================ */
