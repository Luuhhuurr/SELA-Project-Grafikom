#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SELA.Editor
{
    // BuatDetailRuang.cs
    // Nambahin detail kamar kos: kasur berlapis, meja ada lampanya,
    // rak buku berisi buku, jendela ada tirainya, plus lampu kamar,
    // karpet, ember sampah, dan lain-lain.
    //
    // Semua tekstur dibuat dari kode (pola keramik, plester, kayu, kain)
    // lalu disimpan jadi PNG di Assets/GameData/Tekstur. Jadi project
    // nggak perlu file gambar eksternal sama sekali.
    //
    // Dipanggil dari BuatSceneDemo setelah scene kosong dibuat.

    public static class BuatDetailRuang
    {
        private const string FolderTekstur = "Assets/GameData/Tekstur";

        // Kumpulan material yang dipakai di seluruh ruangan.
        public class Paket
        {
            public Material lantai;
            public Material dinding;
            public Material plafon;
            public Material kayu;
            public Material kayuGelap;
            public Material kain;
            public Material kainLembut;
            public Material kainMerah;
            public Material kaca;
            public Material logam;
            public Material lampu;
            public Material layar;
            public Material kardus;
            public Material tanaman;
            public Material keramik;
            public Material plastik;
            public Material papan;
            public Material poster;
            public Material buku1;
            public Material buku2;
            public Material buku3;
            public Material buku4;
            public Material kertas;
        }

        // ---------------------------------------------------------------
        // Entry point
        // ---------------------------------------------------------------

        public static Paket SiapkanMaterial()
        {
            Texture2D texLantai = BuatTekstur("T_Keramik", 64, PixelKeramik);
            Texture2D texDinding = BuatTekstur("T_Plester", 64, PixelPlester);
            Texture2D texKayu = BuatTekstur("T_Kayu", 64, PixelKayu);
            Texture2D texKain = BuatTekstur("T_Kain", 64, PixelKain);
            Texture2D texKardus = BuatTekstur("T_Kardus", 64, PixelKardus);

            Paket p = new Paket();

            p.lantai = MaterialTeks("Lantai", new Color(0.62f, 0.60f, 0.56f), texLantai, 14f, 8f, 0.35f);
            p.dinding = MaterialTeks("Dinding", new Color(0.80f, 0.78f, 0.72f), texDinding, 3f, 1.5f, 0.05f);
            p.plafon = MaterialTeks("Plafon", new Color(0.90f, 0.89f, 0.86f), texDinding, 3f, 3f, 0.03f);
            p.kayu = MaterialTeks("Kayu", new Color(0.52f, 0.36f, 0.22f), texKayu, 1f, 1f, 0.18f);
            p.kayuGelap = MaterialTeks("KayuGelap", new Color(0.33f, 0.23f, 0.14f), texKayu, 1f, 1f, 0.15f);
            p.kain = MaterialTeks("Kain", new Color(0.38f, 0.45f, 0.56f), texKain, 3f, 3f, 0.02f);
            p.kainLembut = MaterialTeks("KainLembut", new Color(0.90f, 0.89f, 0.84f), texKain, 3f, 3f, 0.02f);
            p.kainMerah = MaterialTeks("KainMerah", new Color(0.58f, 0.28f, 0.24f), texKain, 3f, 3f, 0.02f);
            p.kaca = MaterialTeks("Kaca", new Color(0.80f, 0.88f, 0.92f, 0.35f), null, 1f, 1f, 0.92f);
            JadikanTransparan(p.kaca);
            p.logam = MaterialHalus("Logam", new Color(0.62f, 0.63f, 0.66f), 0.75f, 0.85f);
            p.lampu = MaterialLampu("Lampu", new Color(1f, 0.93f, 0.78f), 2.2f);
            p.layar = MaterialLampu("Layar", new Color(0.55f, 0.72f, 0.95f), 1.1f);
            p.kardus = MaterialTeks("Kardus", new Color(0.66f, 0.51f, 0.33f), texKardus, 1f, 1f, 0.05f);
            p.tanaman = MaterialHalus("Tanaman", new Color(0.24f, 0.44f, 0.22f), 0.05f, 0f);
            p.keramik = MaterialHalus("Keramik", new Color(0.86f, 0.84f, 0.79f), 0.45f, 0f);
            p.plastik = MaterialHalus("Plastik", new Color(0.22f, 0.22f, 0.24f), 0.30f, 0f);
            p.papan = MaterialHalus("Papan", new Color(0.90f, 0.90f, 0.87f), 0.10f, 0f);
            p.poster = MaterialHalus("Poster", new Color(0.42f, 0.33f, 0.52f), 0.05f, 0f);
            p.buku1 = MaterialHalus("Buku1", new Color(0.60f, 0.24f, 0.22f), 0.08f, 0f);
            p.buku2 = MaterialHalus("Buku2", new Color(0.22f, 0.34f, 0.55f), 0.08f, 0f);
            p.buku3 = MaterialHalus("Buku3", new Color(0.30f, 0.42f, 0.28f), 0.08f, 0f);
            p.buku4 = MaterialHalus("Buku4", new Color(0.72f, 0.62f, 0.28f), 0.08f, 0f);
            p.kertas = MaterialHalus("Kertas", new Color(0.92f, 0.91f, 0.85f), 0.05f, 0f);

            return p;
        }

        public static void BangunSemua(Paket p, Transform akar)
        {
            IsiKamar(p, akar);
            IsiLorong(p, akar);
            IsiRuang(p, akar);
        }

        // ---------------------------------------------------------------
        // Kamar 07
        // ---------------------------------------------------------------

        private static void IsiKamar(Paket p, Transform akar)
        {
            GameObject kamar = new GameObject("Detail Kamar");
            kamar.transform.SetParent(akar, true);

            // ---- Plafon + lampu gantung ----
            Kubik("Plafon", new Vector3(0f, 3.05f, 0f), new Vector3(6.2f, 0.1f, 5.2f), p.plafon, kamar);
            Kubik("Kabel Lampu", new Vector3(0f, 2.93f, 0f), new Vector3(0.03f, 0.2f, 0.03f), p.logam, kamar);
            Silinder("Kubah Lampu", new Vector3(0f, 2.78f, 0f), 0.24f, 0.16f, p.logam, kamar);
            Silinder("Bolpoin Lampu", new Vector3(0f, 2.67f, 0f), 0.18f, 0.05f, p.lampu, kamar);
            LampuTitik("Lampu Kamar", new Vector3(0f, 2.6f, 0f), new Color(1f, 0.90f, 0.72f), 2.1f, 7f, true);

            // ---- Meja kerja ----
            // Meja utamanya dibuat BuatSceneDemo, di sini cuma bagian bawahnya
            // (kaki dan laci) yang ditambahkan.
            Kubik("Kaki Meja 1", new Vector3(-2.32f, 0.36f, -1.55f), new Vector3(0.08f, 0.72f, 0.08f), p.kayuGelap, kamar);
            Kubik("Kaki Meja 2", new Vector3(-0.68f, 0.36f, -1.55f), new Vector3(0.08f, 0.72f, 0.08f), p.kayuGelap, kamar);
            Kubik("Kaki Meja 3", new Vector3(-2.32f, 0.36f, -0.85f), new Vector3(0.08f, 0.72f, 0.08f), p.kayuGelap, kamar);
            Kubik("Kaki Meja 4", new Vector3(-0.68f, 0.36f, -0.85f), new Vector3(0.08f, 0.72f, 0.08f), p.kayuGelap, kamar);
            Kubik("Balok Meja Belakang", new Vector3(-1.5f, 0.66f, -1.55f), new Vector3(1.7f, 0.1f, 0.06f), p.kayu, kamar);
            Kubik("Laci Meja", new Vector3(-2.0f, 0.6f, -1.2f), new Vector3(0.4f, 0.16f, 0.7f), p.kayu, kamar);
            Kubik("Pegangan Laci", new Vector3(-2.22f, 0.6f, -1.2f), new Vector3(0.03f, 0.04f, 0.16f), p.logam, kamar);

            // Laptop: layar yang opened slightly condong.
            GameObject layar = Kubik("Layar Laptop", new Vector3(-1.5f, 0.98f, -1.33f), new Vector3(0.52f, 0.32f, 0.02f), p.layar, kamar, false);
            layar.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            Kubik("Tuas Laptop", new Vector3(-1.5f, 0.86f, -1.14f), new Vector3(0.5f, 0.02f, 0.12f), p.plastik, kamar, false);

            // Mouse, gelas, tumpukan kertas, stapler, tempat pena.
            Kubik("Mouse", new Vector3(-1.02f, 0.84f, -1.18f), new Vector3(0.08f, 0.035f, 0.12f), p.plastik, kamar, false);
            Silinder("Gelas", new Vector3(-1.92f, 0.85f, -1.02f), 0.045f, 0.12f, p.keramik, kamar, false);
            for (int i = 0; i < 4; i++)
            {
                GameObject kertas = Kubik("Kertas " + (i + 1), new Vector3(-2.2f, 0.805f + i * 0.008f, -1.32f),
                                          new Vector3(0.3f, 0.006f, 0.22f), p.kertas, kamar, false);
                kertas.transform.rotation = Quaternion.Euler(0f, i * 4f - 5f, 0f);
            }
            Kubik("Stapler", new Vector3(-1.05f, 0.83f, -1.45f), new Vector3(0.16f, 0.04f, 0.05f), p.plastik, kamar, false);
            Silinder("Tempat Pena", new Vector3(-2.28f, 0.85f, -1.02f), 0.05f, 0.11f, p.logam, kamar, false);
            Silinder("Pena 1", new Vector3(-2.28f, 0.94f, -1.03f), 0.008f, 0.14f, p.buku4, kamar, false);
            Silinder("Pena 2", new Vector3(-2.26f, 0.93f, -1.0f), 0.008f, 0.13f, p.buku1, kamar, false);

            // Lampu meja, punya cahaya sendiri.
            Silinder("Dasar Lampu Meja", new Vector3(-2.3f, 0.79f, -1.62f), 0.09f, 0.04f, p.logam, kamar, false);
            Kubik("Tiang Lampu Meja", new Vector3(-2.3f, 0.96f, -1.62f), new Vector3(0.03f, 0.34f, 0.03f), p.logam, kamar, false);
            GameObject kepalaLampu = Silinder("Kepala Lampu Meja", new Vector3(-2.16f, 1.12f, -1.62f), 0.09f, 0.14f, p.logam, kamar, false);
            kepalaLampu.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            LampuTitik("Lampu Meja", new Vector3(-2.12f, 1.04f, -1.62f), new Color(1f, 0.86f, 0.66f), 1.1f, 2.6f, false);

            // Kursi kerja.
            Kubik("Dudukan Kursi", new Vector3(-0.55f, 0.45f, -1.95f), new Vector3(0.45f, 0.07f, 0.45f), p.kayu, kamar);
            Kubik("Sandaran Kursi", new Vector3(-0.55f, 0.72f, -2.15f), new Vector3(0.45f, 0.5f, 0.06f), p.kayu, kamar);
            Kubik("Kaki Kursi 1", new Vector3(-0.75f, 0.22f, -2.12f), new Vector3(0.05f, 0.45f, 0.05f), p.kayuGelap, kamar);
            Kubik("Kaki Kursi 2", new Vector3(-0.35f, 0.22f, -2.12f), new Vector3(0.05f, 0.45f, 0.05f), p.kayuGelap, kamar);
            Kubik("Kaki Kursi 3", new Vector3(-0.75f, 0.22f, -1.78f), new Vector3(0.05f, 0.45f, 0.05f), p.kayuGelap, kamar);
            Kubik("Kaki Kursi 4", new Vector3(-0.35f, 0.22f, -1.78f), new Vector3(0.05f, 0.45f, 0.05f), p.kayuGelap, kamar);

            // ---- Kasur ----
            // Badannya sudah dibuat (1.2 x 0.6 x 2 di posisi 0.9, 0.3, 1.4).
            Kubik("Bantal", new Vector3(0.9f, 0.67f, 2.15f), new Vector3(0.55f, 0.13f, 0.32f), p.kainLembut, kamar);
            Kubik("Selimut", new Vector3(0.9f, 0.645f, 1.25f), new Vector3(1.16f, 0.07f, 1.35f), p.kain, kamar);
            Kubik("Lipatan Selimut", new Vector3(0.9f, 0.69f, 0.62f), new Vector3(1.16f, 0.09f, 0.26f), p.kainMerah, kamar);
            Kubik("Sandaran Kasur", new Vector3(0.9f, 0.45f, 2.42f), new Vector3(1.26f, 0.9f, 0.06f), p.kayuGelap, kamar);
            Kubik("Papan Kaki Kasur", new Vector3(0.9f, 0.35f, 0.36f), new Vector3(1.26f, 0.7f, 0.06f), p.kayuGelap, kamar);

            // ---- Lemari ----
            // Diletakkan di dinding kanan, di dekat pintu kamar.
            Kubik("Lemari", new Vector3(2.62f, 0.9f, -1.6f), new Vector3(0.55f, 1.8f, 1f), p.kayu, kamar);
            Kubik("Pintu Lemari 1", new Vector3(2.33f, 0.9f, -1.85f), new Vector3(0.03f, 1.72f, 0.47f), p.kayuGelap, kamar);
            Kubik("Pintu Lemari 2", new Vector3(2.33f, 0.9f, -1.35f), new Vector3(0.03f, 1.72f, 0.47f), p.kayuGelap, kamar);
            Kubik("Pegangan Lemari", new Vector3(2.3f, 0.9f, -1.6f), new Vector3(0.03f, 0.16f, 0.04f), p.logam, kamar);

            // ---- Rak buku + isi rak ----
            // Raknya sudah dibuat (0.5 x 1.2 x 1.6 di -2.5, 0.6, 1.2).
            float[] tinggiBuku = { 0.22f, 0.26f, 0.2f, 0.28f, 0.24f, 0.21f, 0.27f, 0.23f };
            Material[] warnaBuku = { p.buku1, p.buku2, p.buku3, p.buku4 };
            for (int i = 0; i < tinggiBuku.Length; i++)
            {
                Material m = warnaBuku[i % warnaBuku.Length];
                GameObject buku = Kubik("Buku " + (i + 1), new Vector3(-2.5f, 1.2f + tinggiBuku[i] * 0.5f, 0.62f + i * 0.09f),
                                        new Vector3(0.24f, tinggiBuku[i], 0.06f), m, kamar, false);
                buku.transform.rotation = Quaternion.Euler(0f, 0f, (i == 3) ? 9f : 0f);
            }
            Kubik("Kotak Buku", new Vector3(-2.5f, 1.3f, 1.72f), new Vector3(0.3f, 0.2f, 0.28f), p.kardus, kamar, false);
            Silinder("Radio Kecil", new Vector3(-2.5f, 1.28f, 0.55f), 0.07f, 0.16f, p.plastik, kamar, false);

            // ---- Jendela ----
            // Kusennya sudah ada (1.2 x 1.2 di 1.7, 1.6, 2.4).
            Kubik("Kaca Jendela", new Vector3(1.7f, 1.6f, 2.43f), new Vector3(1.08f, 1.08f, 0.02f), p.kaca, kamar, false);
            Kubik("Kusen Atas", new Vector3(1.7f, 2.19f, 2.36f), new Vector3(1.24f, 0.08f, 0.1f), p.kayu, kamar, false);
            Kubik("Kusen Bawah", new Vector3(1.7f, 1.01f, 2.36f), new Vector3(1.24f, 0.08f, 0.1f), p.kayu, kamar, false);
            Kubik("Kusen Kiri", new Vector3(1.11f, 1.6f, 2.36f), new Vector3(0.08f, 1.2f, 0.1f), p.kayu, kamar, false);
            Kubik("Kusen Kanan", new Vector3(2.29f, 1.6f, 2.36f), new Vector3(0.08f, 1.2f, 0.1f), p.kayu, kamar, false);
            Kubik("Tempat Jendela", new Vector3(1.7f, 0.96f, 2.28f), new Vector3(1.4f, 0.06f, 0.26f), p.kayu, kamar);

            GameObject tiraiKiri = Kubik("Tirai Kiri", new Vector3(1.06f, 1.72f, 2.24f), new Vector3(0.32f, 1.5f, 0.04f), p.kain, kamar, false);
            tiraiKiri.transform.rotation = Quaternion.Euler(0f, 7f, 0f);
            GameObject tiraiKanan = Kubik("Tirai Kanan", new Vector3(2.34f, 1.72f, 2.24f), new Vector3(0.32f, 1.5f, 0.04f), p.kain, kamar, false);
            tiraiKanan.transform.rotation = Quaternion.Euler(0f, -7f, 0f);
            Kubik("Gantungan Tirai", new Vector3(1.7f, 2.48f, 2.26f), new Vector3(1.6f, 0.04f, 0.04f), p.logam, kamar, false);

            // Tanaman di tempat jendela.
            Silinder("Pot", new Vector3(2.16f, 1.06f, 2.22f), 0.09f, 0.16f, p.keramik, kamar, false);
            for (int i = 0; i < 5; i++)
            {
                float tinggi = 0.12f + i * 0.04f;
                GameObject daun = Kubik("Daun " + (i + 1), new Vector3(2.16f + Mathf.Sin(i * 1.7f) * 0.05f, 1.12f + tinggi * 0.5f, 2.22f + Mathf.Cos(i * 1.3f) * 0.05f),
                                         new Vector3(0.05f, tinggi, 0.05f), p.tanaman, kamar, false);
                daun.transform.rotation = Quaternion.Euler(i * 9f, i * 40f, i * 5f);
            }

            // ---- Dekorasi dinding ----
            GameObject poster = Kubik("Poster", new Vector3(-0.35f, 1.85f, -2.38f), new Vector3(0.55f, 0.75f, 0.02f), p.poster, kamar, false);
            poster.transform.rotation = Quaternion.Euler(0f, 3f, 0f);
            GameObject poster2 = Kubik("Poster 2", new Vector3(0.35f, 1.8f, -2.38f), new Vector3(0.4f, 0.55f, 0.02f), p.kertas, kamar, false);
            poster2.transform.rotation = Quaternion.Euler(0f, -4f, 0f);

            GameObject kalender = Kubik("Kalender", new Vector3(2.86f, 1.85f, 0.95f), new Vector3(0.03f, 0.42f, 0.3f), p.kertas, kamar, false);
            kalender.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            GameObject jam = Silinder("Jam Dinding", new Vector3(-2.87f, 2.2f, 0.5f), 0.16f, 0.05f, p.papan, kamar, false);
            jam.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Kubik("Jarum Jam 1", new Vector3(-2.83f, 2.26f, 0.5f), new Vector3(0.02f, 0.1f, 0.02f), p.logam, kamar, false);
            Kubik("Jarum Jam 2", new Vector3(-2.83f, 2.2f, 0.56f), new Vector3(0.02f, 0.02f, 0.11f), p.logam, kamar, false);

            // ---- Lantai ----
            Kubik("Karpet", new Vector3(0.2f, 0.012f, 0.3f), new Vector3(1.8f, 0.02f, 1.2f), p.kainMerah, kamar, false);
            Silinder("Ember Sampah", new Vector3(-2.7f, 0.2f, 2.15f), 0.16f, 0.4f, p.plastik, kamar, false);
            Kubik("Sandal 1", new Vector3(2.45f, 0.03f, 0.25f), new Vector3(0.11f, 0.05f, 0.26f), p.kain, kamar, false);
            Kubik("Sandal 2", new Vector3(2.62f, 0.03f, 0.2f), new Vector3(0.11f, 0.05f, 0.26f), p.kain, kamar, false);
            Silinder("Botol Air", new Vector3(-0.95f, 0.13f, 2.12f), 0.045f, 0.26f, p.kaca, kamar, false);
            Kubik("Kabel Colokan", new Vector3(-1.4f, 0.02f, -0.6f), new Vector3(0.03f, 0.02f, 1.6f), p.plastik, kamar, false);

            // Kusen pintu kamar (lubang di dinding kanan, z = 0).
            Kubik("Kusen Pintu Kamar", new Vector3(3f, 2.35f, 0f), new Vector3(0.26f, 0.4f, 1.5f), p.kayu, kamar, false);
        }

        // ---------------------------------------------------------------
        // Lorong Kos
        // ---------------------------------------------------------------

        private static void IsiLorong(Paket p, Transform akar)
        {
            GameObject lorong = new GameObject("Detail Lorong");
            lorong.transform.SetParent(akar, true);

            Kubik("Plafon Lorong", new Vector3(6f, 3.05f, 0f), new Vector3(6.2f, 0.1f, 3.2f), p.plafon, lorong);
            Kubik("Lampu Neon", new Vector3(6f, 2.92f, 0f), new Vector3(1.1f, 0.08f, 0.16f), p.lampu, lorong, false);
            LampuTitik("Lampu Lorong", new Vector3(6f, 2.78f, 0f), new Color(0.92f, 0.95f, 1f), 1.9f, 8f, true);

            // Rak sepatu + sepatu di bawahnya.
            Kubik("Rak Sepatu", new Vector3(4.15f, 0.14f, -1.15f), new Vector3(0.95f, 0.28f, 0.4f), p.kayu, lorong);
            for (int i = 0; i < 3; i++)
            {
                Kubik("Sepatu " + (i + 1), new Vector3(3.85f + i * 0.3f, 0.07f, -1.15f), new Vector3(0.12f, 0.12f, 0.26f), p.plastik, lorong, false);
            }

            // Papan pengumuman di dekat pintu kamar.
            Kubik("Papan Info", new Vector3(3.12f, 1.6f, 0.95f), new Vector3(0.04f, 0.55f, 0.75f), p.kayu, lorong, false);
            for (int i = 0; i < 4; i++)
            {
                GameObject nota = Kubik("Nota " + (i + 1), new Vector3(3.09f, 1.75f - (i % 2) * 0.3f, 0.75f + (i / 2) * 0.35f),
                                         new Vector3(0.01f, 0.2f, 0.16f), p.kertas, lorong, false);
                nota.transform.rotation = Quaternion.Euler(0f, 0f, (i % 2 == 0) ? 3f : -2f);
            }

            // Karpet depan pintu dan ember.
            Kubik("Karpet Lorong", new Vector3(3.6f, 0.012f, 0f), new Vector3(0.8f, 0.02f, 1.1f), p.kain, lorong, false);
            Silinder("Ember Lorong", new Vector3(7.4f, 0.22f, 1.2f), 0.18f, 0.44f, p.plastik, lorong, false);

            // Jam dinding di ujung lorong.
            GameObject jam = Silinder("Jam Lorong", new Vector3(8.82f, 2.25f, 0f), 0.17f, 0.05f, p.papan, lorong, false);
            jam.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            // Gagang pintu + nomor pintu.
            Kubik("Gagang Pintu 1", new Vector3(8.72f, 1.0f, 1.32f), new Vector3(0.1f, 0.05f, 0.05f), p.logam, lorong, false);
            Kubik("Gagang Pintu 2", new Vector3(8.72f, 1.0f, -0.88f), new Vector3(0.1f, 0.05f, 0.05f), p.logam, lorong, false);
            Kubik("Nomor Pintu 1", new Vector3(8.72f, 1.75f, 1.1f), new Vector3(0.03f, 0.12f, 0.18f), p.kertas, lorong, false);
            Kubik("Nomor Pintu 2", new Vector3(8.72f, 1.75f, -1.1f), new Vector3(0.03f, 0.12f, 0.18f), p.kertas, lorong, false);
        }

        // ---------------------------------------------------------------
        // Ruang Terpanjang
        // ---------------------------------------------------------------

        private static void IsiRuang(Paket p, Transform akar)
        {
            GameObject ruang = new GameObject("Detail Ruang");
            ruang.transform.SetParent(akar, true);

            Kubik("Plafon Ruang", new Vector3(8.25f, 3.05f, -3.75f), new Vector3(5.7f, 0.1f, 4.7f), p.plafon, ruang);
            Kubik("Kabel Lampu Ruang", new Vector3(8f, 2.92f, -3.5f), new Vector3(0.03f, 0.2f, 0.03f), p.logam, ruang, false);
            Silinder("Kubah Lampu Ruang", new Vector3(8f, 2.78f, -3.5f), 0.26f, 0.18f, p.logam, ruang, false);
            Silinder("Bolpoin Ruang", new Vector3(8f, 2.66f, -3.5f), 0.2f, 0.05f, p.lampu, ruang, false);
            LampuTitik("Lampu Ruang", new Vector3(8f, 2.6f, -3.5f), new Color(1f, 0.92f, 0.78f), 1.8f, 8f, true);

            // Bangku panjang di sisi meja.
            Kubik("Bangku Panjang", new Vector3(6.4f, 0.45f, -4.0f), new Vector3(2.2f, 0.08f, 0.36f), p.kayu, ruang);
            Kubik("Kaki Bangku 1", new Vector3(5.5f, 0.22f, -4.0f), new Vector3(0.07f, 0.45f, 0.3f), p.kayuGelap, ruang);
            Kubik("Kaki Bangku 2", new Vector3(7.3f, 0.22f, -4.0f), new Vector3(0.07f, 0.45f, 0.3f), p.kayuGelap, ruang);

            // Kardus di sudut.
            Kubik("Kardus 1", new Vector3(9.9f, 0.35f, -2.3f), new Vector3(0.6f, 0.7f, 0.5f), p.kardus, ruang);
            Kubik("Kardus 2", new Vector3(9.85f, 0.95f, -2.25f), new Vector3(0.45f, 0.5f, 0.4f), p.kardus, ruang);
            Kubik("Kardus 3", new Vector3(10.35f, 0.25f, -2.6f), new Vector3(0.5f, 0.5f, 0.5f), p.kardus, ruang);

            // Papan tulis di dinding kanan.
            Kubik("Papan Tulis", new Vector3(10.86f, 1.6f, -3.5f), new Vector3(0.04f, 1.1f, 1.8f), p.papan, ruang, false);
            Kubik("Bingkai Papan", new Vector3(10.83f, 1.6f, -3.5f), new Vector3(0.02f, 1.18f, 1.88f), p.logam, ruang, false);

            // Tumpukan buku di atas rak panjang.
            for (int i = 0; i < 3; i++)
            {
                GameObject tumpukan = Kubik("Buku Tumpukan " + (i + 1), new Vector3(9.5f, 1.24f + i * 0.06f, -4.8f),
                                            new Vector3(0.3f, 0.06f, 0.22f), i % 2 == 0 ? p.buku2 : p.buku3, ruang, false);
                tumpukan.transform.rotation = Quaternion.Euler(0f, i * 7f - 6f, 0f);
            }
        }

        // ---------------------------------------------------------------
        // Bantu object
        // ---------------------------------------------------------------

        private static GameObject Kubik(string nama, Vector3 posisi, Vector3 ukuran,
                                        Material material, GameObject induk, bool adaCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nama;
            go.transform.position = posisi;
            go.transform.localScale = ukuran;

            if (material != null && go.GetComponent<Renderer>() != null)
            {
                go.GetComponent<Renderer>().sharedMaterial = material;
            }

            if (!adaCollider)
            {
                BoxCollider col = go.GetComponent<BoxCollider>();
                if (col != null) UnityEngine.Object.DestroyImmediate(col);
            }

            if (induk != null) go.transform.SetParent(induk.transform, true);
            return go;
        }

        private static GameObject Silinder(string nama, Vector3 posisi, float radius, float tinggi,
                                           Material material, GameObject induk, bool adaCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = nama;
            go.transform.position = posisi;
            // Primitive cylinder punya tinggi 2 unit, jadi diskalakan setengah.
            go.transform.localScale = new Vector3(radius * 2f, tinggi * 0.5f, radius * 2f);

            if (material != null && go.GetComponent<Renderer>() != null)
            {
                go.GetComponent<Renderer>().sharedMaterial = material;
            }

            if (!adaCollider)
            {
                BoxCollider col = go.GetComponent<BoxCollider>();
                if (col != null) UnityEngine.Object.DestroyImmediate(col);
                CapsuleCollider cap = go.GetComponent<CapsuleCollider>();
                if (cap != null) UnityEngine.Object.DestroyImmediate(cap);
            }

            if (induk != null) go.transform.SetParent(induk.transform, true);
            return go;
        }

        private static void LampuTitik(string nama, Vector3 posisi, Color warna,
                                       float intensitas, float jangkauan, bool bayangan)
        {
            GameObject go = new GameObject(nama);
            go.transform.position = posisi;

            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = warna;
            light.intensity = intensitas;
            light.range = jangkauan;
            light.shadows = bayangan ? LightShadows.Soft : LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
        }

        // ---------------------------------------------------------------
        // Material & tekstur
        // ---------------------------------------------------------------

        private static Material MaterialHalus(string nama, Color warna, float halus, float metal)
        {
            Material m = BuatSceneDemo.BuatMaterial("M_" + nama, warna);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", halus);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal);
            return m;
        }

        private static Material MaterialTeks(string nama, Color warna, Texture2D tekstur,
                                              float tilingX, float tilingY, float halus)
        {
            Material m = MaterialHalus(nama, warna, halus, 0f);

            if (tekstur != null && m.HasProperty("_BaseMap"))
            {
                m.SetTexture("_BaseMap", tekstur);
                m.SetTextureScale("_BaseMap", new Vector2(tilingX, tilingY));
                // Kalau ada tekstur, warna dasar dipakai putih supaya
                // teksturnya kelihatan apa adanya.
                m.SetColor("_BaseColor", Color.white);
            }

            return m;
        }

        private static Material MaterialLampu(string nama, Color warna, float strengths)
        {
            Material m = MaterialHalus(nama, new Color(0.05f, 0.05f, 0.05f), 0.2f, 0f);

            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", warna * strengths);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            return m;
        }

        private static void JadikanTransparan(Material m)
        {
            // Cara bikin material URP Lit kelihatan tembus.
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
            if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
            if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 0f);
            if (m.HasProperty("_Surface")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.DisableKeyword("_ALPHATEST_ON");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private static Texture2D BuatTekstur(string nama, int ukuran, Func<int, int, Color> isi)
        {
            BuatSceneDemo.BuatFolderIfBelumAda(FolderTekstur);
            string path = FolderTekstur + "/" + nama + ".png";

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null && tex.width == ukuran) return tex;

            tex = new Texture2D(ukuran, ukuran, TextureFormat.RGBA32, true);
            for (int y = 0; y < ukuran; y++)
            {
                for (int x = 0; x < ukuran; x++)
                {
                    tex.SetPixel(x, y, isi(x, y));
                }
            }
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Angka acak palsu, biar teksturnya tetap sama tiap build.
        private static float Acak(int x, int y, int seed)
        {
            long n = (long)x * 73856093L + (long)y * 19349663L + (long)seed * 83492791L;
            n = (n ^ (n >> 13)) * 1274126177L;
            n = n ^ (n >> 16);
            return ((n & 0x7fffffff) / (float)0x7fffffff);
        }

        // Keramik lantai: pola kotak 4 x 4 dengan nat garis.
        private static Color PixelKeramik(int x, int y)
        {
            int ukuran = 64;
            int sisi = ukuran / 4;

            bool nat = (x % sisi < 2) || (y % sisi < 2);
            float variation = Acak(x / sisi, y / sisi, 7) * 0.06f;

            Color dasar = new Color(0.68f, 0.66f, 0.62f);
            if (nat) dasar = new Color(0.55f, 0.54f, 0.52f);

            dasar.r += variation;
            dasar.g += variation;
            dasar.b += variation;
            return dasar;
        }

        // Plester tembok: bintik halus.
        private static Color PixelPlester(int x, int y)
        {
            float n = Acak(x, y, 3) * 0.08f + Acak(x / 4, y / 4, 11) * 0.05f;
            return new Color(0.82f + n * 0.5f, 0.80f + n * 0.5f, 0.76f + n * 0.5f);
        }

        // Kayu: garis Memanjang.
        private static Color PixelKayu(int x, int y)
        {
            float n = Mathf.Sin(y * 1.7f + Mathf.Sin(x * 0.3f) * 2f) * 0.5f + 0.5f;
            float acak = Acak(x, y, 5) * 0.05f;
            float v = 0.75f + n * 0.22f + acak;
            return new Color(0.86f * v, 0.78f * v, 0.70f * v);
        }

        // Kain: anyaman kotak kecil.
        private static Color PixelKain(int x, int y)
        {
            float anyam = ((x % 4 < 2) == (y % 4 < 2)) ? 0.06f : -0.03f;
            float acak = Acak(x, y, 9) * 0.05f;
            float v = 0.92f + anyam + acak;
            return new Color(v, v, v);
        }

        // Kardus: garis flute.
        private static Color PixelKardus(int x, int y)
        {
            float garis = (x % 8 < 2) ? 0.08f : 0f;
            float acak = Acak(x, y, 13) * 0.06f;
            float v = 0.86f + garis + acak;
            return new Color(v, v * 0.97f, v * 0.94f);
        }
    }
}
#endif
