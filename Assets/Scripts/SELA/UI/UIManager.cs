using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UIManager.cs
// Mengatur semua tampilan layar game SELA (uGUI + TextMeshPro).
//
// Panel yang dikelola:
// PanelHUD              -> hari, sesi, tenaga, tekanan, bukti, retakan
// PanelPrompt           -> tulisan "[E] Ambil Kartu" di tengah bawah
// PanelNotifikasi       -> pesan singkat yang muncul sebentar
// PanelDialog           -> percakapan interview
// PanelCatatan          -> buku catatan Raka (dibuka dengan Tab)
// PanelLaptop           -> laptop (dibuka dengan L), 3 tab
// PanelKonfirmasiAktivitas -> konfirmasi biaya sebelum aktivitas jalan
// PanelPause            -> menu (dibuka dengan Esc)
// PanelJeda             -> jeda saat tekanan mencapai 6
// PanelPilihanFinal     -> pilihan kontrak A / B / C / tidak ada
// PanelPeringatanC      -> peringatan khusus tawaran C
// PanelEnding           -> cerita akhir + pertanyaan refleksi
// PanelMenuAwal         -> main menu saat game baru dibuka
// PanelInfoKonten       -> layar info konten (disclaimer + kontrol)
// PanelRakitCV          -> pilih klaim CV dan pasang bukti
// PanelVerifikasi       -> cek empat aspek lowongan
// PanelPengaturan       -> ukuran teks, sensitivitas, invert Y
// PanelCredits          -> credits dan sumber
//
// Semua panel dicari otomatis lewat NAMA GameObject-nya, jadi tidak perlu
// drag satu per satu di Inspector (tapi tetap bisa diisi manual).
//
// CATATAN INPUT: project ini memakai Input System baru, jadi tombol
// dibaca dari Keyboard.current, bukan Input.GetKeyDown yang lama.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Pemain (boleh kosong, diisi otomatis)")]
    public FirstPersonController firstPersonController;

    [Header("Referensi Canvas (boleh kosong)")]
    public GameObject kanvas;

    // ---------------------------------------------------------------
    // Panel (cari otomatis by nama kalau kosong)
    // ---------------------------------------------------------------

    [Header("Panel")]
    public GameObject panelHUD;
    public GameObject panelPrompt;
    public GameObject panelNotifikasi;
    public GameObject panelDialog;
    public GameObject panelCatatan;
    public GameObject panelLaptop;
    public GameObject panelKonfirmasi;
    public GameObject panelPause;
    public GameObject panelJeda;
    public GameObject panelPilihanFinal;
    public GameObject panelPeringatanC;
    public GameObject panelEnding;
    public GameObject panelMenuAwal;
    public GameObject panelInfoKonten;
    public GameObject panelRakitCV;
    public GameObject panelVerifikasi;
    public GameObject panelPengaturan;
    public GameObject panelCredits;

    // ---------------------------------------------------------------
    // Teks (cari otomatis by nama kalau kosong)
    // ---------------------------------------------------------------

    [Header("Teks")]
    public TextMeshProUGUI teksHUD;
    public TextMeshProUGUI teksPrompt;
    public TextMeshProUGUI teksNotifikasi;
    public TextMeshProUGUI teksDialogPembicara;
    public TextMeshProUGUI teksDialogTeks;
    public TextMeshProUGUI teksCatatan;
    public TextMeshProUGUI teksLaptop;
    public TextMeshProUGUI teksKonfirmasi;
    public TextMeshProUGUI teksPause;
    public TextMeshProUGUI teksJeda;
    public TextMeshProUGUI teksPilihanFinal;
    public TextMeshProUGUI teksPeringatanC;
    public TextMeshProUGUI teksEnding;
    public TextMeshProUGUI teksMenuAwal;
    public TextMeshProUGUI teksInfoKonten;
    public TextMeshProUGUI teksRakitCV;
    public TextMeshProUGUI teksVerifikasi;
    public TextMeshProUGUI teksPengaturan;
    public TextMeshProUGUI teksCredits;

    // ---------------------------------------------------------------
    // Wadah dinamis
    // ---------------------------------------------------------------

    [Header("Wadah tombol (dibuat lewat kode)")]
    public RectTransform wadahOpsiDialog;
    public RectTransform wadahAktivitas;
    public RectTransform wadahPilihanFinal;
    public RectTransform wadahPilihanCV;
    public RectTransform wadahPengaturan;

    // ---------------------------------------------------------------
    // Tombol yang dibuat manual di scene
    // ---------------------------------------------------------------

    [Header("Tombol")]
    public Button tombolLanjutDialog;    public Button tombolKonfirmasiYa;
    public Button tombolKonfirmasiBatal;
    public Button tombolJedaLanjut;
    public Button tombolPeringatanYakin;
    public Button tombolPeringatanBatal;
    public Button tombolPauseLanjut;
    public Button tombolPauseSimpan;
    public Button tombolPauseMulaiUlang;
    public Button tombolPauseKeluar;
    public Button tombolRakitCV;
    public Button tombolTutupRakitCV;
    public Button tombolVerifikasiSudah;
    public Button tombolVerifikasiBatal;
    public Button tombolTutupCredits;
    public Button tombolTutupPengaturan;
    public Button tombolKembaliMenu;

    // ---------------------------------------------------------------
    // Isi layar credits. Bisa diubah langsung di Inspector.
    // ---------------------------------------------------------------

    [Header("Isi Credits")]
    [TextArea(10, 30)]
    public string teksCreditsPanel =
        "SELA\n\n"
        + "Game naratif first-person 3D tentang Raka yang mencari kerja\n"
        + "dalam tujuh hari terakhir sewa kosnya.\n\n"
        + "Tugas mata kuliah Grafika Komputer\n"
        + "Teknik Informatika - Semester 5\n"
        + "Kelompok VI\n\n"
        + "--- pembagian tugas ---\n"
        + "Luhur Pangaribowo Handaruji - naratif dan UI\n"
        + "[nama anggota] - [bagian]\n\n"
        + "--- yang dipakai ---\n"
        + "Unity 6 LTS + URP\n"
        + "uGUI + TextMeshPro\n"
        + "Input System\n"
        + "ScriptableObject untuk data naskah dan aktivitas\n"
        + "JsonUtility untuk save/load\n\n"
        + "--- sumber ---\n"
        + "Game Design Document oleh kelompok\n"
        + "Naskah Game oleh kelompok\n\n"
        + "SELA ini fiksi. Perusahaan, kontrak, dan nama yang muncul di\n"
        + "dalam game tidak merujuk orang sungguhan.";

    // ---------------------------------------------------------------
    // Keadaan internal
    // ---------------------------------------------------------------

    private List<GameObject> daftarPanelModal = new List<GameObject>();

    private ActivityData aktivitasMenungguKonfirmasi = null;
    private int tabLaptop = 0;                 // 0 = Lamaran, 1 = Pesan, 2 = Catatan
    private float waktuNotifikasi = 0f;
    private float durasiNotifikasi = 3f;

    // True kalau pemain sudah menekan "Mulai Game" di menu awal.
    private bool gameSudahMulai = false;

    // Aktivitas yang sedang dicek lowongannya (dipakai panel Verifikasi).
    private ActivityData aktivitasVerifikasi = null;

    // Wadah tombol menu awal (dibuat lewat kode).
    private RectTransform wadahTombolMenu = null;

    // Warna asli teks notifikasi, dipakai buat animasi muncul.
    private Color warnaNotifikasiAwal = new Color(1f, 1f, 1f, 1f);
    private bool warnaNotifikasiTercatat = false;

    // Ukuran teks asli tiap label. Dipakai buat menyetel ukuran teks dari
    // menu Pengaturan. Nilai fontSize di TMP dalam pixel, jadi harus
    // dikalikan, bukan ditambah.
    private Dictionary<TextMeshProUGUI, float> ukuranTeksAwal = new Dictionary<TextMeshProUGUI, float>();

    // ---------------------------------------------------------------
    // Setup
    // ---------------------------------------------------------------

    void Awake()
    {
        Instance = this;

        if (firstPersonController == null) firstPersonController = FirstPersonController.Instance;
        if (kanvas == null) kanvas = gameObject;

        CariPanel();
        CariTeks();

        SiapkanDaftarPanelModal();
        SiapkanTombol();

        // Main menu dan layar info konten dibuat lewat kode kalau belum
        // ada di scene. Jadi UIManager tetap jalan walau scene-nya belum
        // Contain panel-panel itu.
        SiapkanMenuAwal();

        // SaveSystem.Muat() dipanggil GameState.Start(). Semua Awake()
        // selalu selesai sebelum Start(), jadi berlangganan di sini aman.
        SaveSystem.OnHasilMuat += TentangHasilMuat;
    }

    void Start()
    {
        // Semua singleton sudah siap sekarang (semua Awake sudah jalan).
        GameState gs = GameState.Instance;
        if (gs != null)
        {
            gs.OnStateChanged += PerbaruiHUD;
            gs.OnNotifikasi += TampilkanNotifikasi;
            gs.OnJedaTerbuka += BukaJeda;
        }

        SessionManager sm = (gs != null) ? gs.sesiManager : null;
        if (sm != null)
        {
            sm.OnSesiBerpindah += PerbaruiHUD;
            sm.OnHariBaru += OnHariBaru;
        }

        if (gs != null && gs.activitySystem != null)
        {
            gs.activitySystem.OnMintaKonfirmasi += MintaKonfirmasi;
            gs.activitySystem.OnMintaInterview += MintaInterview;
            gs.activitySystem.OnAktivitasSelesai += AktivitasSelesai;
        }

        if (gs != null && gs.cvSystem != null)
        {
            gs.cvSystem.OnLamaranTerkirim += OnLamaranTerkirim;
        }

        InterviewManager im = (gs != null) ? gs.interviewManager : null;
        if (im != null)
        {
            im.OnDialogBerubah += PerbaruiDialog;
            im.OnInterviewSelesai += InterviewSelesai;
        }

        EndingManager em = EndingManager.Instance;
        if (em != null)
        {
            em.OnPilihanFinalMuncul += BukaPilihanFinal;
            em.OnPeringatanCMuncul += BukaPeringatanC;
            em.OnPeringatanCBatal += TutupPilihanFinal;
            em.OnEndingMuncul += TampilkanEnding;
        }

        FirstPersonController fp = firstPersonController;
        if (fp != null)
        {
            fp.OnPromptBerubah += PerbaruiPrompt;
        }

        PerbaruiHUD();
        BangunDaftarAktivitas();
        BangunDaftarPilihanFinal();

        if (panelNotifikasi != null) panelNotifikasi.SetActive(false);
        if (panelPrompt != null) panelPrompt.SetActive(false);
        TutupSemuaPanel();

        // Menu awal tampil lebih dulu. Pemain baru bisa jalan setelah
        // menekan tombol "Mulai Game".
        TampilkanMenuAwal();
    }

    void OnDestroy()
    {
        SaveSystem.OnHasilMuat -= TentangHasilMuat;

        GameState gs = GameState.Instance;
        if (gs != null)
        {
            gs.OnStateChanged -= PerbaruiHUD;
            gs.OnNotifikasi -= TampilkanNotifikasi;
            gs.OnJedaTerbuka -= BukaJeda;
        }

        SessionManager sm = (gs != null) ? gs.sesiManager : null;
        if (sm != null)
        {
            sm.OnSesiBerpindah -= PerbaruiHUD;
            sm.OnHariBaru -= OnHariBaru;
        }

        if (gs != null && gs.activitySystem != null)
        {
            gs.activitySystem.OnMintaKonfirmasi -= MintaKonfirmasi;
            gs.activitySystem.OnMintaInterview -= MintaInterview;
            gs.activitySystem.OnAktivitasSelesai -= AktivitasSelesai;
        }

        if (gs != null && gs.cvSystem != null)
        {
            gs.cvSystem.OnLamaranTerkirim -= OnLamaranTerkirim;
        }

        InterviewManager im = (gs != null) ? gs.interviewManager : null;
        if (im != null)
        {
            im.OnDialogBerubah -= PerbaruiDialog;
            im.OnInterviewSelesai -= InterviewSelesai;
        }

        EndingManager em = EndingManager.Instance;
        if (em != null)
        {
            em.OnPilihanFinalMuncul -= BukaPilihanFinal;
            em.OnPeringatanCMuncul -= BukaPeringatanC;
            em.OnPeringatanCBatal -= TutupPilihanFinal;
            em.OnEndingMuncul -= TampilkanEnding;
        }

        FirstPersonController fp = firstPersonController;
        if (fp != null)
        {
            fp.OnPromptBerubah -= PerbaruiPrompt;
        }

        if (Instance == this) Instance = null;
    }

    // ---------------------------------------------------------------
    // Mencari panel & teks otomatis
    // ---------------------------------------------------------------

    private void CariPanel()
    {
        panelHUD = CariPanel("PanelHUD", panelHUD);
        panelPrompt = CariPanel("PanelPrompt", panelPrompt);
        panelNotifikasi = CariPanel("PanelNotifikasi", panelNotifikasi);
        panelDialog = CariPanel("PanelDialog", panelDialog);
        panelCatatan = CariPanel("PanelCatatan", panelCatatan);
        panelLaptop = CariPanel("PanelLaptop", panelLaptop);
        panelKonfirmasi = CariPanel("PanelKonfirmasiAktivitas", panelKonfirmasi);
        panelPause = CariPanel("PanelPause", panelPause);
        panelJeda = CariPanel("PanelJeda", panelJeda);
        panelPilihanFinal = CariPanel("PanelPilihanFinal", panelPilihanFinal);
        panelPeringatanC = CariPanel("PanelPeringatanC", panelPeringatanC);
        panelEnding = CariPanel("PanelEnding", panelEnding);
        panelMenuAwal = CariPanel("PanelMenuAwal", panelMenuAwal);
        panelInfoKonten = CariPanel("PanelInfoKonten", panelInfoKonten);
        panelRakitCV = CariPanel("PanelRakitCV", panelRakitCV);
        panelVerifikasi = CariPanel("PanelVerifikasi", panelVerifikasi);
        panelPengaturan = CariPanel("PanelPengaturan", panelPengaturan);
        panelCredits = CariPanel("PanelCredits", panelCredits);
    }

    private GameObject CariPanel(string nama, GameObject sekarang)
    {
        if (sekarang != null) return sekarang;

        Transform t = transform.Find(nama);
        if (t == null)
        {
            Debug.LogWarning("[UIManager] Panel '" + nama + "' tidak ditemukan. "
                           + "Buat GameObject anak Canvas dengan nama persis '" + nama + "'.");
            return null;
        }
        return t.gameObject;
    }

    private void CariTeks()
    {
        teksHUD = CariTeks(panelHUD, "Teks", teksHUD);
        teksPrompt = CariTeks(panelPrompt, "Teks", teksPrompt);
        teksNotifikasi = CariTeks(panelNotifikasi, "Teks", teksNotifikasi);
        teksDialogPembicara = CariTeks(panelDialog, "Pembicara", teksDialogPembicara);
        teksDialogTeks = CariTeks(panelDialog, "Teks", teksDialogTeks);
        teksCatatan = CariTeks(panelCatatan, "Teks", teksCatatan);
        teksLaptop = CariTeks(panelLaptop, "Teks", teksLaptop);
        teksKonfirmasi = CariTeks(panelKonfirmasi, "Teks", teksKonfirmasi);
        teksPause = CariTeks(panelPause, "Teks", teksPause);
        teksJeda = CariTeks(panelJeda, "Teks", teksJeda);
        teksPilihanFinal = CariTeks(panelPilihanFinal, "Teks", teksPilihanFinal);
        teksPeringatanC = CariTeks(panelPeringatanC, "Teks", teksPeringatanC);
        teksEnding = CariTeks(panelEnding, "Teks", teksEnding);
        teksMenuAwal = CariTeks(panelMenuAwal, "Teks", teksMenuAwal);
        teksInfoKonten = CariTeks(panelInfoKonten, "Teks", teksInfoKonten);
        teksRakitCV = CariTeks(panelRakitCV, "Teks", teksRakitCV);
        teksVerifikasi = CariTeks(panelVerifikasi, "Teks", teksVerifikasi);
        teksPengaturan = CariTeks(panelPengaturan, "Teks", teksPengaturan);
        teksCredits = CariTeks(panelCredits, "Teks", teksCredits);
    }

    private TextMeshProUGUI CariTeks(GameObject panel, string nama, TextMeshProUGUI sekarang)
    {
        if (sekarang != null) return sekarang;
        if (panel == null) return null;

        Transform t = panel.transform.Find(nama);
        if (t == null)
        {
            Debug.LogWarning("[UIManager] Teks '" + nama + "' tidak ditemukan di panel " + panel.name + ".");
            return null;
        }

        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            Debug.LogWarning("[UIManager] Objek '" + nama + "' di panel " + panel.name
                           + " bukan TextMeshProUGUI.");
        }
        return tmp;
    }

    private void SiapkanDaftarPanelModal()
    {
        daftarPanelModal.Clear();
        daftarPanelModal.Add(panelDialog);
        daftarPanelModal.Add(panelCatatan);
        daftarPanelModal.Add(panelLaptop);
        daftarPanelModal.Add(panelKonfirmasi);
        daftarPanelModal.Add(panelPause);
        daftarPanelModal.Add(panelJeda);
        daftarPanelModal.Add(panelPilihanFinal);
        daftarPanelModal.Add(panelPeringatanC);
        daftarPanelModal.Add(panelEnding);
        daftarPanelModal.Add(panelRakitCV);
        daftarPanelModal.Add(panelVerifikasi);
        daftarPanelModal.Add(panelPengaturan);
        daftarPanelModal.Add(panelCredits);
    }

    private void SiapkanTombol()
    {
        if (tombolLanjutDialog != null) tombolLanjutDialog.onClick.AddListener(LanjutDialog);
        if (tombolKonfirmasiYa != null) tombolKonfirmasiYa.onClick.AddListener(JalankanAktivitasMenunggu);
        if (tombolKonfirmasiBatal != null) tombolKonfirmasiBatal.onClick.AddListener(TutupPanelKonfirmasi);
        if (tombolJedaLanjut != null) tombolJedaLanjut.onClick.AddListener(TutupJeda);
        if (tombolPeringatanYakin != null) tombolPeringatanYakin.onClick.AddListener(KonfirmasiPeringatanC);
        if (tombolPeringatanBatal != null) tombolPeringatanBatal.onClick.AddListener(BatalPeringatanC);
        if (tombolPauseLanjut != null) tombolPauseLanjut.onClick.AddListener(TutupSemuaPanel);
        if (tombolPauseSimpan != null) tombolPauseSimpan.onClick.AddListener(SimpanManual);
        if (tombolPauseMulaiUlang != null) tombolPauseMulaiUlang.onClick.AddListener(MulaiUlang);
        if (tombolPauseKeluar != null) tombolPauseKeluar.onClick.AddListener(KeluarGame);
        if (tombolRakitCV != null) tombolRakitCV.onClick.AddListener(BukaPanelRakitCV);
        if (tombolTutupRakitCV != null) tombolTutupRakitCV.onClick.AddListener(TutupPanelRakitCV);
        if (tombolVerifikasiSudah != null) tombolVerifikasiSudah.onClick.AddListener(SelesaikanVerifikasi);
        if (tombolVerifikasiBatal != null) tombolVerifikasiBatal.onClick.AddListener(TutupPanelVerifikasi);
        if (tombolTutupCredits != null) tombolTutupCredits.onClick.AddListener(TutupPanelCredits);
        if (tombolTutupPengaturan != null) tombolTutupPengaturan.onClick.AddListener(TutupPanelPengaturan);
        if (tombolKembaliMenu != null) tombolKembaliMenu.onClick.AddListener(KembaliKeMenu);
    }

    // ---------------------------------------------------------------
    // Update (pintasan keyboard)
    // ---------------------------------------------------------------

    void Update()
    {
        // Notifikasi menghilang sendiri.
        if (waktuNotifikasi > 0f)
        {
            waktuNotifikasi -= Time.deltaTime;
            if (waktuNotifikasi <= 0f && panelNotifikasi != null)
            {
                panelNotifikasi.SetActive(false);
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Kalau pemain sedang mengetik di kolom teks, pintasan keyboard
        // dinonaktifkan supaya ketikan tidak ketutupan.
        if (AdaKolomTeksAktif()) return;

        // Selama menu awal masih terbuka, pintasan lain diabaikan.
        if (!gameSudahMulai)
        {
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                if (panelInfoKonten != null && panelInfoKonten.activeSelf) TutupInfoKonten();
                else MulaiGame();
            }
            return;
        }

        // Malam terakhir: panel lain jangan dibuka, tinggal pilih tawaran.
        if (panelPilihanFinal != null && panelPilihanFinal.activeSelf) return;
        if (panelEnding != null && panelEnding.activeSelf) return;

        // Tab = buka / tutup buku catatan
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (panelCatatan != null && panelCatatan.activeSelf) TutupPanelCatatan();
            else BukaPanelCatatan();
            return;
        }

        // L = buka / tutup laptop
        if (keyboard.lKey.wasPressedThisFrame)
        {
            if (panelLaptop != null && panelLaptop.activeSelf) TutupPanelLaptop();
            else BukaPanelLaptop();
            return;
        }

        // Esc = tutup panel teratas, atau buka menu jeda
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            EscTekan();
            return;
        }

        // Enter / Spasi = lanjut dialog
        if (panelDialog != null && panelDialog.activeSelf)
        {
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                LanjutDialog();
                return;
            }

            // 1 dan 2 untuk memilih jawaban
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                PilihOpsiDialog(0);
                return;
            }
            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                PilihOpsiDialog(1);
                return;
            }
        }
    }

    private bool AdaKolomTeksAktif()
    {
        if (EventSystem.current == null) return false;

        GameObject terpilih = EventSystem.current.currentSelectedGameObject;
        if (terpilih == null) return false;

        if (terpilih.GetComponent<TMP_InputField>() != null) return true;
        if (terpilih.GetComponentInChildren<TMP_InputField>() != null) return true;

        return false;
    }

    private void EscTekan()
    {
        // Selama menu awal terbuka, Esc hanya menutup layar info konten.
        if (!gameSudahMulai)
        {
            if (panelInfoKonten != null && panelInfoKonten.activeSelf) TutupInfoKonten();
            if (panelCredits != null && panelCredits.activeSelf) TutupPanelCredits();
            if (panelPengaturan != null && panelPengaturan.activeSelf) TutupPanelPengaturan();
            return;
        }

        // Urutan tutup: dari panel yang paling dalam ke luar.
        if (panelPeringatanC != null && panelPeringatanC.activeSelf) { TutupPanelPeringatanC(); return; }
        if (panelDialog != null && panelDialog.activeSelf) { TutupPanelDialog(); return; }
        if (panelKonfirmasi != null && panelKonfirmasi.activeSelf) { TutupPanelKonfirmasi(); return; }
        if (panelVerifikasi != null && panelVerifikasi.activeSelf) { TutupPanelVerifikasi(); return; }
        if (panelRakitCV != null && panelRakitCV.activeSelf) { TutupPanelRakitCV(); return; }
        if (panelPengaturan != null && panelPengaturan.activeSelf) { TutupPanelPengaturan(); return; }
        if (panelCredits != null && panelCredits.activeSelf) { TutupPanelCredits(); return; }
        if (panelLaptop != null && panelLaptop.activeSelf) { TutupPanelLaptop(); return; }
        if (panelCatatan != null && panelCatatan.activeSelf) { TutupPanelCatatan(); return; }

        // Panel malam terakhir dan panel ending nggak bisa ditutup dengan Esc.
        if (panelPilihanFinal != null && panelPilihanFinal.activeSelf) return;
        if (panelEnding != null && panelEnding.activeSelf) return;

        if (panelPause != null && panelPause.activeSelf) TutupSemuaPanel();
        else BukaPanelPause();
    }

    // ---------------------------------------------------------------
    // Membuka & menutup panel
    // ---------------------------------------------------------------

    public void BukaPanel(GameObject panel)
    {
        if (panel == null) return;

        panel.SetActive(true);
        PerbaruiStatusPemain();
    }

    public void TutupPanel(GameObject panel)
    {
        if (panel == null) return;

        panel.SetActive(false);
        PerbaruiStatusPemain();
    }

    // Tutup semua panel modal sekaligus (dipakai menu Pause).
    public void TutupSemuaPanel()
    {
        for (int i = 0; i < daftarPanelModal.Count; i++)
        {
            if (daftarPanelModal[i] != null) daftarPanelModal[i].SetActive(false);
        }
        PerbaruiStatusPemain();
    }

    // Kalau ada panel modal terbuka, pemain tidak boleh jalan dan mouse
    // harus dilepas. Dipanggil setiap kali panel buka/tutup.
    private void PerbaruiStatusPemain()
    {
        bool adaPanelTerbuka = false;

        for (int i = 0; i < daftarPanelModal.Count; i++)
        {
            if (daftarPanelModal[i] != null && daftarPanelModal[i].activeSelf)
            {
                adaPanelTerbuka = true;
                break;
            }
        }

        if (firstPersonController != null)
        {
            // Selama menu awal terbuka, pemain juga tidak boleh jalan.
            firstPersonController.uiTerbuka = adaPanelTerbuka || !gameSudahMulai;
        }
    }

    // ---------------------------------------------------------------
    // Menu awal + layar info konten
    // ---------------------------------------------------------------

    // Siapkan panel menu awal dan layar info konten.
    // Kalau panelnya sudah ada di scene, dipakai apa adanya.
    // Kalau belum ada, dibuat lewat kode di sini.
    private void SiapkanMenuAwal()
    {
        if (panelMenuAwal == null)
        {
            panelMenuAwal = BuatPanelLayar("PanelMenuAwal", new Color(0.05f, 0.06f, 0.08f, 1f));
        }
        if (panelInfoKonten == null)
        {
            panelInfoKonten = BuatPanelLayar("PanelInfoKonten", new Color(0.05f, 0.06f, 0.08f, 1f));
        }

        if (teksMenuAwal == null)
        {
            teksMenuAwal = BuatTeksLayar(panelMenuAwal, "Teks", "", 30f, TextAlignmentOptions.Top);
        }
        if (teksInfoKonten == null)
        {
            teksInfoKonten = BuatTeksLayar(panelInfoKonten, "Teks", "", 19f, TextAlignmentOptions.TopLeft);
        }

        // Wadah tombol menu. Tombolnya sendiri dibuat ulang tiap kali
        // menu ditampilkan, soalnya "Lanjutkan" cuma muncul kalau ada save.
        Transform wadahLama = panelMenuAwal.transform.Find("DaftarTombol");
        if (wadahLama == null)
        {
            wadahTombolMenu = BuatWadahTombol(panelMenuAwal, "DaftarTombol");
        }
        else
        {
            wadahTombolMenu = wadahLama as RectTransform;
        }

        if (panelInfoKonten.transform.Find("DaftarTombol") == null
            || panelInfoKonten.transform.Find("DaftarTombol").childCount == 0)
        {
            RectTransform wadah = BuatWadahTombol(panelInfoKonten, "DaftarTombol");
            BuatTombol(wadah, "Kembali", delegate { TutupInfoKonten(); });
        }
    }

    // Tampilkan menu awal dan kunci pergerakan pemain.
    private void TampilkanMenuAwal()
    {
        gameSudahMulai = false;

        if (teksMenuAwal != null)
        {
            teksMenuAwal.text = "SELA\n\n"
                              + "Raka, 23 tahun. Tujuh hari sebelum sewa kos berakhir,\n"
                              + "sambil mencari kerja.\n\n"
                              + "Grafika Komputer - Kelompok VI";
        }

        BangunTombolMenu();

        if (panelInfoKonten != null) panelInfoKonten.SetActive(false);
        if (panelCredits != null) panelCredits.SetActive(false);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelMenuAwal != null) panelMenuAwal.SetActive(true);

        PerbaruiStatusPemain();
    }

    // Buat ulang daftar tombol menu awal.
    private void BangunTombolMenu()
    {
        if (wadahTombolMenu == null) return;

        for (int i = wadahTombolMenu.childCount - 1; i >= 0; i--)
        {
            GameObject go = wadahTombolMenu.GetChild(i).gameObject;
            if (go == null) continue;
            Destroy(go);
        }

        if (SaveSystem.AdaSave())
        {
            BuatTombol(wadahTombolMenu, "Lanjutkan", delegate { Lanjutkan(); });
        }

        BuatTombol(wadahTombolMenu, "Mulai Baru", delegate { MulaiBaru(); });
        BuatTombol(wadahTombolMenu, "Info Konten", delegate { TampilkanInfoKonten(); });
        BuatTombol(wadahTombolMenu, "Pengaturan", delegate { BukaPanelPengaturan(); });
        BuatTombol(wadahTombolMenu, "Credits dan Sumber", delegate { BukaPanelCredits(); });
        BuatTombol(wadahTombolMenu, "Keluar Game", delegate { KeluarGame(); });

        TerapkanUkuranTeks();
    }

    // Pemain menekan "Mulai Game": menu ditutup, permainan jalan.
    public void MulaiGame()
    {
        gameSudahMulai = true;

        if (panelMenuAwal != null) panelMenuAwal.SetActive(false);
        if (panelInfoKonten != null) panelInfoKonten.SetActive(false);

        PerbaruiStatusPemain();
        PerbaruiHUD();
    }

    // Lanjut dari save yang ada. Save-nya sendiri sudah dimuat otomatis
    // waktu game baru dibuka, jadi di sini cuma muat ulang lalu mulai.
    public void Lanjutkan()
    {
        SaveSystem.Muat();
        MulaiGame();
    }

    // Mulai dari nol: hapus save lama, reset semua state.
    public void MulaiBaru()
    {
        GameState gs = GameState.Instance;
        if (gs != null) gs.ResetSeluruhState();

        SaveSystem.HapusSave();

        MulaiGame();
        TampilkanNotifikasi("Permainan baru. Hari 1, sesi pagi.");
    }

    // Kembali ke menu awal (dipakai dari panel ending).
    public void KembaliKeMenu()
    {
        TutupSemuaPanel();
        TampilkanMenuAwal();
    }

    // Buka layar info konten (disclaimer + kontrol).
    private void TampilkanInfoKonten()
    {
        if (teksInfoKonten != null)
        {
            teksInfoKonten.text = "INFO KONTEN\n\n"
                + "SELA adalah game naratif first-person 3D tentang Raka yang mencari\n"
                + "kerja sebelum sewa kosnya habis.\n\n"
                + "Game ini bukan terapi, diagnosis, rehabilitasi, atau pelatihan hukum\n"
                + "ketenagakerjaan. Perusahaan dan kontrak di dalam game ini fiktif.\n"
                + "\"Tekanan\" adalah abstraksi permainan, bukan ukuran kondisi mental.\n\n"
                + "Kontrol\n"
                + "  WASD : jalan\n"
                + "  Mouse : menoleh, klik kiri untuk interaksi\n"
                + "  E : interaksi dengan benda di depan\n"
                + "  Tab : buka buku catatan\n"
                + "  L : buka laptop\n"
                + "  Esc : buka menu / tutup panel\n\n"
                + "Progres disimpan otomatis. Bisa save manual dari menu Esc.";
        }

        if (panelMenuAwal != null) panelMenuAwal.SetActive(false);
        if (panelInfoKonten != null) panelInfoKonten.SetActive(true);

        PerbaruiStatusPemain();
    }

    // Tutup layar info konten dan kembali ke menu awal.
    private void TutupInfoKonten()
    {
        if (panelInfoKonten != null) panelInfoKonten.SetActive(false);

        if (!gameSudahMulai)
        {
            if (panelMenuAwal != null) panelMenuAwal.SetActive(true);
        }

        PerbaruiStatusPemain();
    }

    // ---------------------------------------------------------------
    // Rakit CV: pilih klaim dan pasang bukti
    // ---------------------------------------------------------------

    public void BukaPanelRakitCV()
    {
        if (teksRakitCV != null) teksRakitCV.text = BuildTeksRakitCV();
        BangunTombolRakitCV();

        if (panelLaptop != null) panelLaptop.SetActive(false);
        BukaPanel(panelRakitCV);
    }

    private void TutupPanelRakitCV()
    {
        TutupPanel(panelRakitCV);

        if (!gameSudahMulai && panelMenuAwal != null) panelMenuAwal.SetActive(true);
    }

    private string BuildTeksRakitCV()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return "";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("RAKIT CV");
        sb.AppendLine();
        sb.AppendLine("Klaim 1 : " + (string.IsNullOrEmpty(gs.klaimDraf[0]) ? "(belum dipilih)" : gs.klaimDraf[0]));
        sb.AppendLine("Klaim 2 : " + (string.IsNullOrEmpty(gs.klaimDraf[1]) ? "(belum dipilih)" : gs.klaimDraf[1]));
        sb.AppendLine("Bukti   : " + BuktiKeTeks(gs.buktiTerpasang));
        sb.AppendLine();
        sb.AppendLine("Pilih klaim di bawah, lalu pasang bukti yang mau dikirim.");
        sb.AppendLine("Begitu lamaran dikirim, CV-nya dikunci dan nggak bisa diubah lagi.");

        return sb.ToString();
    }

    private void BangunTombolRakitCV()
    {
        if (wadahPilihanCV == null) return;

        for (int i = wadahPilihanCV.childCount - 1; i >= 0; i--)
        {
            GameObject go = wadahPilihanCV.GetChild(i).gameObject;
            if (go == null) continue;
            Destroy(go);
        }

        GameState gs = GameState.Instance;
        if (gs == null || gs.cvSystem == null) return;

        BuatTombol(wadahPilihanCV, "Klaim 1: " + CVSystem.Klaim1Sesuai,
                   delegate { PilihKlaim(1, CVSystem.Klaim1Sesuai); });
        BuatTombol(wadahPilihanCV, "Klaim 1: " + CVSystem.Klaim1Berlebihan,
                   delegate { PilihKlaim(1, CVSystem.Klaim1Berlebihan); });
        BuatTombol(wadahPilihanCV, "Klaim 2: " + CVSystem.Klaim2Sesuai,
                   delegate { PilihKlaim(2, CVSystem.Klaim2Sesuai); });
        BuatTombol(wadahPilihanCV, "Klaim 2: " + CVSystem.Klaim2Berlebihan,
                   delegate { PilihKlaim(2, CVSystem.Klaim2Berlebihan); });

        // Bukti yang Raka punya sekarang.
        for (int i = 0; i < gs.bukti.Count; i++)
        {
            string id = gs.bukti[i];
            bool terpasang = gs.buktiTerpasang.Contains(id);

            string label = id + " - " + NamaBukti(id);
            if (terpasang) label += "   [terpasang, klik buat lepas]";

            BuatTombol(wadahPilihanCV, label, delegate { ToggleBukti(id); });
        }

        TerapkanUkuranTeks();
    }

    private void PilihKlaim(int nomor, string teks)
    {
        GameState gs = GameState.Instance;
        if (gs == null || gs.cvSystem == null) return;

        gs.cvSystem.AturKlaim(nomor, teks);

        if (teksRakitCV != null) teksRakitCV.text = BuildTeksRakitCV();
        BangunTombolRakitCV();
    }

    private void ToggleBukti(string id)
    {
        GameState gs = GameState.Instance;
        if (gs == null || gs.cvSystem == null) return;

        if (gs.buktiTerpasang.Contains(id)) gs.cvSystem.LepasBukti(id);
        else gs.cvSystem.LampirkanBukti(id);

        if (teksRakitCV != null) teksRakitCV.text = BuildTeksRakitCV();
        BangunTombolRakitCV();
    }

    // Nama friendly buat tiap kartu bukti.
    private string NamaBukti(string id)
    {
        if (id == "E0") return "Kartu Proyek";
        if (id == "E1") return "Portofolio Antarmuka";
        if (id == "E2") return "Catatan Uji";
        return "Bukti";
    }

    // ---------------------------------------------------------------
    // Verifikasi lowongan (empat aspek)
    // ---------------------------------------------------------------

    private void BukaPanelVerifikasi(ActivityData aktivitas)
    {
        aktivitasVerifikasi = aktivitas;

        if (teksVerifikasi != null) teksVerifikasi.text = BuildTeksVerifikasi(aktivitas);
        if (panelLaptop != null) panelLaptop.SetActive(false);

        BukaPanel(panelVerifikasi);
    }

    private void TutupPanelVerifikasi()
    {
        aktivitasVerifikasi = null;
        TutupPanel(panelVerifikasi);
    }

    private string BuildTeksVerifikasi(ActivityData aktivitas)
    {
        GameState gs = GameState.Instance;
        if (gs == null || aktivitas == null) return "";

        string kode = aktivitas.perusahaan;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CEK LOWONGAN");
        sb.AppendLine(DataPerusahaan.NamaPerusahaan(kode));
        sb.AppendLine();

        List<DataPerusahaan.Aspek> aspek = DataPerusahaan.AspekMilik(kode);
        for (int i = 0; i < aspek.Count; i++)
        {
            sb.AppendLine(aspek[i].judul + "  [" + aspek[i].tanda + "]");
            sb.AppendLine("   " + aspek[i].isi);
            if (!string.IsNullOrEmpty(aspek[i].catatan))
            {
                sb.AppendLine("   " + aspek[i].catatan);
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("Catatan: " + DataPerusahaan.Catatan(kode));
        sb.AppendLine();

        int tenagaHasil = Mathf.Clamp(gs.tenaga - aktivitas.biayaTenaga, 0, 4);
        int tekananHasil = Mathf.Clamp(gs.tekanan + aktivitas.biayaTekanan, 0, 6);
        sb.AppendLine("Tenaga: " + gs.tenaga + " -> " + tenagaHasil
                    + "     Tekanan: " + gs.tekanan + " -> " + tekananHasil);

        return sb.ToString();
    }

    private void SelesaikanVerifikasi()
    {
        GameState gs = GameState.Instance;
        if (gs == null || gs.activitySystem == null) return;
        if (aktivitasVerifikasi == null) return;

        ActivityData aktivitas = aktivitasVerifikasi;
        aktivitasVerifikasi = null;

        TutupPanelVerifikasi();
        gs.activitySystem.JalankanAktivitas(aktivitas);
    }

    // ---------------------------------------------------------------
    // Pengaturan
    // ---------------------------------------------------------------

    public void BukaPanelPengaturan()
    {
        if (teksPengaturan != null) teksPengaturan.text = BuildTeksPengaturan();
        BangunTombolPengaturan();

        if (panelMenuAwal != null) panelMenuAwal.SetActive(false);
        if (panelCredits != null) panelCredits.SetActive(false);

        BukaPanel(panelPengaturan);
    }

    private void TutupPanelPengaturan()
    {
        TutupPanel(panelPengaturan);

        if (!gameSudahMulai && panelMenuAwal != null) panelMenuAwal.SetActive(true);
    }

    private string BuildTeksPengaturan()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("PENGATURAN");
        sb.AppendLine();
        sb.AppendLine("Ukuran teks        : " + PengaturanGame.LabelUkuranTeksSekarang());
        sb.AppendLine("Sensitivitas mouse : " + PengaturanGame.sensitivitasMouse.ToString("0.0"));
        sb.AppendLine("Invert Y           : " + (PengaturanGame.invertY ? "nyala" : "mati"));
        sb.AppendLine("Kurangi gerak      : " + (PengaturanGame.kurangiGerak ? "nyala" : "mati"));
        sb.AppendLine();
        sb.AppendLine("Klik salah satu baris di bawah buat mengubahnya.");

        return sb.ToString();
    }

    private void BangunTombolPengaturan()
    {
        if (wadahPengaturan == null) return;

        for (int i = wadahPengaturan.childCount - 1; i >= 0; i--)
        {
            GameObject go = wadahPengaturan.GetChild(i).gameObject;
            if (go == null) continue;
            Destroy(go);
        }

        BuatTombol(wadahPengaturan, "Ukuran teks: " + PengaturanGame.LabelUkuranTeksSekarang(),
                   delegate { GantiUkuranTeks(); });
        BuatTombol(wadahPengaturan, "Sensitivitas mouse: " + PengaturanGame.sensitivitasMouse.ToString("0.0"),
                   delegate { GantiSensitivitas(); });
        BuatTombol(wadahPengaturan, "Invert Y: " + (PengaturanGame.invertY ? "nyala" : "mati"),
                   delegate { GantiInvertY(); });
        BuatTombol(wadahPengaturan, "Kurangi gerak: " + (PengaturanGame.kurangiGerak ? "nyala" : "mati"),
                   delegate { GantiKurangiGerak(); });

        TerapkanUkuranTeks();
    }

    private void GantiUkuranTeks()
    {
        PengaturanGame.GantiUkuranTeks(1);

        if (teksPengaturan != null) teksPengaturan.text = BuildTeksPengaturan();
        BangunTombolPengaturan();
    }

    private void GantiSensitivitas()
    {
        PengaturanGame.sensitivitasMouse += 0.5f;
        if (PengaturanGame.sensitivitasMouse > 4f) PengaturanGame.sensitivitasMouse = 0.5f;

        if (teksPengaturan != null) teksPengaturan.text = BuildTeksPengaturan();
        BangunTombolPengaturan();
    }

    private void GantiInvertY()
    {
        PengaturanGame.invertY = !PengaturanGame.invertY;

        if (teksPengaturan != null) teksPengaturan.text = BuildTeksPengaturan();
        BangunTombolPengaturan();
    }

    private void GantiKurangiGerak()
    {
        PengaturanGame.kurangiGerak = !PengaturanGame.kurangiGerak;

        if (teksPengaturan != null) teksPengaturan.text = BuildTeksPengaturan();
        BangunTombolPengaturan();
    }

    // ---------------------------------------------------------------
    // Credits dan sumber
    // ---------------------------------------------------------------

    public void BukaPanelCredits()
    {
        if (teksCredits != null) teksCredits.text = teksCreditsPanel;

        if (panelMenuAwal != null) panelMenuAwal.SetActive(false);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);

        BukaPanel(panelCredits);
    }

    private void TutupPanelCredits()
    {
        TutupPanel(panelCredits);

        if (!gameSudahMulai && panelMenuAwal != null) panelMenuAwal.SetActive(true);
    }

    // ---------------------------------------------------------------
    // Ukuran teks
    // ---------------------------------------------------------------

    // Perkecil atau perbesar semua teks di layar sesuai pengaturan.
    // Ukuran asli tiap label disimpan dulu, jadi Setting bisa diubah
    // berkali-kali tanpa bikin teks makin gede terus.
    private void TerapkanUkuranTeks()
    {
        if (kanvas == null) return;

        TextMeshProUGUI[] semuaTeks = kanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < semuaTeks.Length; i++)
        {
            TextMeshProUGUI teks = semuaTeks[i];
            if (teks == null) continue;

            if (!ukuranTeksAwal.ContainsKey(teks))
            {
                ukuranTeksAwal[teks] = teks.fontSize;
            }

            teks.fontSize = Mathf.Round(ukuranTeksAwal[teks] * PengaturanGame.skalaTeks);
        }
    }

    // ---------------------------------------------------------------
    // HUD
    // ---------------------------------------------------------------

    private void PerbaruiHUD()
    {
        GameState gs = GameState.Instance;
        if (gs == null || teksHUD == null) return;

        StringBuilder sb = new StringBuilder();

        sb.Append("Hari ").Append(gs.hari).Append(" / 7");
        sb.Append("  |  ").Append(SessionManager.TeksSesi(gs.sesi));
        sb.AppendLine();
        sb.Append("Tenaga: ").Append(gs.tenaga).Append(" / 4");
        sb.Append("   Tekanan: ").Append(gs.tekanan).Append(" / 6");
        sb.AppendLine();
        sb.Append("Bukti: ");
        sb.Append(BuktiKeTeks(gs.bukti));
        sb.AppendLine();

        int retakanSelesai = 0;
        for (int i = 0; i < gs.retakanSelesai.Length; i++)
        {
            if (gs.retakanSelesai[i]) retakanSelesai++;
        }
        sb.Append("Retakan Ruang: ").Append(retakanSelesai).Append(" / 3");
        sb.AppendLine();
        sb.Append("Lamaran A: ").Append(gs.statusLamaranA.ToString());
        sb.Append("   Lamaran B: ").Append(gs.statusLamaranB.ToString());

        teksHUD.text = sb.ToString();
    }

    private string BuktiKeTeks(List<string> daftar)
    {
        if (daftar == null || daftar.Count == 0) return "(belum ada)";

        string hasil = "";
        for (int i = 0; i < daftar.Count; i++)
        {
            if (i > 0) hasil += ", ";
            hasil += daftar[i];
        }
        return hasil;
    }

    private void PerbaruiPrompt(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            if (panelPrompt != null) panelPrompt.SetActive(false);
            return;
        }

        if (teksPrompt != null) teksPrompt.text = prompt;
        if (panelPrompt != null) panelPrompt.SetActive(true);
    }

    // ---------------------------------------------------------------
    // Notifikasi
    // ---------------------------------------------------------------

    private void TampilkanNotifikasi(string pesan)
    {
        if (string.IsNullOrEmpty(pesan)) return;

        if (teksNotifikasi != null)
        {
            teksNotifikasi.text = pesan;

            // Warna asli dicatat sekali, biar animasi muncul pelan-pelan
            // nggak mengubah warna tulisan.
            if (!warnaNotifikasiTercatat)
            {
                warnaNotifikasiAwal = teksNotifikasi.color;
                warnaNotifikasiTercatat = true;
            }
        }

        if (panelNotifikasi != null) panelNotifikasi.SetActive(true);

        waktuNotifikasi = durasiNotifikasi;

        // Kalau Reduce Motion nyala, notifikasi langsung tampil utuh.
        if (PengaturanGame.kurangiGerak)
        {
            if (teksNotifikasi != null) teksNotifikasi.color = warnaNotifikasiAwal;
        }
        else
        {
            StartCoroutine(MunculkanNotifikasi());
        }
    }

    // Notifikasi muncul pelan-pelan. Kalau Reduce Motion nyala, animasi ini
    // dilewati dan tulisan langsung tampil.
    private IEnumerator MunculkanNotifikasi()
    {
        if (teksNotifikasi == null) yield break;

        for (float t = 0f; t < 0.2f; t += Time.deltaTime)
        {
            Color warna = warnaNotifikasiAwal;
            warna.a = Mathf.Clamp01(t / 0.2f);
            teksNotifikasi.color = warna;
            yield return null;
        }

        if (teksNotifikasi != null) teksNotifikasi.color = warnaNotifikasiAwal;
    }

    // ---------------------------------------------------------------
    // Dialog interview
    // ---------------------------------------------------------------

    private void MintaInterview(string perusahaan)
    {
        InterviewManager im = InterviewManager.Instance;
        if (im == null) return;

        im.MulaiInterview(perusahaan);
        PerbaruiDialog();
        BukaPanel(panelDialog);
    }

    private void PerbaruiDialog()
    {
        InterviewManager im = InterviewManager.Instance;
        if (im == null) return;

        if (teksDialogPembicara != null) teksDialogPembicara.text = im.PembicaraSaatIni;
        if (teksDialogTeks != null) teksDialogTeks.text = im.TeksSaatIni;

        BangunDaftarOpsiDialog(im.OpsiSaatIni);
    }

    private void BangunDaftarOpsiDialog(List<string> opsi)
    {
        if (wadahOpsiDialog == null) return;

        for (int i = wadahOpsiDialog.childCount - 1; i >= 0; i--)
        {
            GameObject go = wadahOpsiDialog.GetChild(i).gameObject;
            if (go == null) continue;
            Destroy(go);
        }

        if (opsi == null) return;

        for (int i = 0; i < opsi.Count; i++)
        {
            int nomor = i;
            string label = (i + 1) + ". " + opsi[i];
            BuatTombol(wadahOpsiDialog, label, delegate { PilihOpsiDialog(nomor); });
        }

        TerapkanUkuranTeks();
    }

    private void PilihOpsiDialog(int indeks)
    {
        InterviewManager im = InterviewManager.Instance;
        if (im == null) return;

        im.PilihOpsi(indeks);
    }

    private void LanjutDialog()
    {
        InterviewManager im = InterviewManager.Instance;
        if (im == null) return;

        if (im.SedangBerlangsung) im.Lanjut();
    }

    private void TutupPanelDialog()
    {
        TutupPanel(panelDialog);
    }

    private void InterviewSelesai(string perusahaan, string ringkasan)
    {
        InterviewManager im = InterviewManager.Instance;

        string hasil = (im != null) ? im.TeksHasil(ringkasan == "TAWARAN DIBERIKAN")
                                     : "Interview selesai.";

        TampilkanNotifikasi("Interview perusahaan " + perusahaan + " selesai. " + ringkasan);

        if (teksDialogTeks != null) teksDialogTeks.text = hasil;
        if (teksDialogPembicara != null) teksDialogPembicara.text = "Raka";

        // Kosongkan daftar opsi supaya tinggal satu tombol "Lanjut".
        if (wadahOpsiDialog != null)
        {
            for (int i = wadahOpsiDialog.childCount - 1; i >= 0; i--)
            {
                GameObject go = wadahOpsiDialog.GetChild(i).gameObject;
                if (go == null) continue;
                Destroy(go);
            }
        }

        if (tombolLanjutDialog != null) tombolLanjutDialog.gameObject.SetActive(true);
    }

    // ---------------------------------------------------------------
    // Konfirmasi aktivitas
    // ---------------------------------------------------------------

    private void MintaKonfirmasi(ActivityData aktivitas)
    {
        aktivitasMenungguKonfirmasi = aktivitas;

        if (teksKonfirmasi != null && aktivitas != null)
        {
            GameState gs = GameState.Instance;

            int tenagaSekarang = (gs != null) ? gs.tenaga : 0;
            int tekananSekarang = (gs != null) ? gs.tekanan : 0;

            // Biaya tenaga boleh negatif (istirahat = tenaga pulih).
            int tenagaHasil = Mathf.Clamp(tenagaSekarang - aktivitas.biayaTenaga, 0, 4);
            int tekananHasil = Mathf.Clamp(tekananSekarang + aktivitas.biayaTekanan, 0, 6);

            teksKonfirmasi.text = "Aktivitas: " + aktivitas.nama + "\n\n"
                                + aktivitas.deskripsi + "\n\n";

            // Ringkasan CV yang dikirim, dipisah biar jelas apa yang terkirim.
            if (aktivitas.tipe == ActivityData.Tipe.KirimLamaran)
            {
                teksKonfirmasi.text += TeksLamaranTerkirim(aktivitas);
            }

            teksKonfirmasi.text += "Tenaga: " + tenagaSekarang + " -> " + tenagaHasil + "\n"
                                + "Tekanan: " + tekananSekarang + " -> " + tekananHasil + "\n"
                                + "Sesi berikutnya: " + TeksSesiBerikutnya() + "\n\n"
                                + "Lanjutkan?";
        }

        BukaPanel(panelKonfirmasi);
    }

    // Ringkasan CV yang bakal dikirim. Dipakai di panel konfirmasi.
    private string TeksLamaranTerkirim(ActivityData aktivitas)
    {
        GameState gs = GameState.Instance;
        if (gs == null) return "";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Perusahaan: " + DataPerusahaan.NamaPerusahaan(aktivitas.perusahaan)
                    + " - " + DataPerusahaan.PosisiPerusahaan(aktivitas.perusahaan));
        sb.AppendLine("Klaim 1: " + (string.IsNullOrEmpty(gs.klaimDraf[0]) ? "-" : gs.klaimDraf[0]));
        sb.AppendLine("Klaim 2: " + (string.IsNullOrEmpty(gs.klaimDraf[1]) ? "-" : gs.klaimDraf[1]));
        sb.AppendLine("Bukti dikirim: " + BuktiKeTeks(gs.buktiTerpasang));

        // Bukti wajib yang belum ikut dikirim.
        List<string> kurang = new List<string>();
        for (int i = 0; i < aktivitas.syaratBukti.Count; i++)
        {
            string id = aktivitas.syaratBukti[i];
            if (string.IsNullOrEmpty(id)) continue;
            if (!gs.buktiTerpasang.Contains(id)) kurang.Add(id);
        }

        if (kurang.Count > 0)
        {
            sb.AppendLine("Bukti wajib belum ikut: " + string.Join(", ", kurang.ToArray()));
            sb.AppendLine("Lamaran tetap terkirim, tapi tawaran belum bisa diberikan.");
        }

        sb.AppendLine("Jadwal interview: hari " + aktivitas.hariInterview + ", sesi siang.");
        sb.AppendLine("Setelah dikirim, CV ini dikunci dan nggak bisa diubah.");
        sb.AppendLine();

        return sb.ToString();
    }

    // Nama sesi yang akan dipakai setelah aktivitas ini selesai.
    private string TeksSesiBerikutnya()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return "-";

        if (gs.sesi == GameState.Sesi.Pagi) return SessionManager.TeksSesi(GameState.Sesi.Siang);
        if (gs.sesi == GameState.Sesi.Siang) return SessionManager.TeksSesi(GameState.Sesi.Malam);

        // Dari malam, aktivitas berikutnya jatuh di pagi hari baru.
        return SessionManager.TeksSesi(GameState.Sesi.Pagi) + " (hari berikutnya)";
    }

    private void JalankanAktivitasMenunggu()
    {
        GameState gs = GameState.Instance;
        if (gs == null || gs.activitySystem == null) return;
        if (aktivitasMenungguKonfirmasi == null) return;

        ActivityData aktivitas = aktivitasMenungguKonfirmasi;
        aktivitasMenungguKonfirmasi = null;

        TutupPanelKonfirmasi();
        gs.activitySystem.JalankanAktivitas(aktivitas);
    }

    private void TutupPanelKonfirmasi()
    {
        aktivitasMenungguKonfirmasi = null;
        TutupPanel(panelKonfirmasi);
    }

    private void AktivitasSelesai(ActivityData aktivitas)
    {
        if (panelKonfirmasi != null) panelKonfirmasi.SetActive(false);
        PerbaruiHUD();
    }

    private void OnLamaranTerkirim(string perusahaan)
    {
        TampilkanNotifikasi("Lamaran ke perusahaan " + perusahaan + " sudah terkirim.");
    }

    private void OnHariBaru()
    {
        PerbaruiHUD();
        TampilkanNotifikasi("Hari baru. Sesi Pagi.");
    }

    // ---------------------------------------------------------------
    // Buku catatan (Tab)
    // ---------------------------------------------------------------

    public void BukaPanelCatatan()
    {
        if (teksCatatan != null) teksCatatan.text = BuildTeksCatatan();
        BukaPanel(panelCatatan);
    }

    private void TutupPanelCatatan()
    {
        TutupPanel(panelCatatan);
    }

    private string BuildTeksCatatan()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return "";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("CATATAN RAKA");
        sb.AppendLine("Hari ").Append(gs.hari).Append(" / 7 — ").Append(SessionManager.TeksSesi(gs.sesi));
        sb.AppendLine("Tenaga ").Append(gs.tenaga).Append("/4   Tekanan ").Append(gs.tekanan).Append("/6");
        sb.AppendLine();

        sb.AppendLine("Bukti yang dimiliki: " + BuktiKeTeks(gs.bukti));
        sb.AppendLine();

        // Kartu proyek E0 (naskah: kartu di rak buku kamar 07).
        sb.AppendLine("Kartu Proyek (E0)");
        if (gs.PunyaBukti("E0"))
        {
            sb.AppendLine("  Status : sudah diperiksa, buktinya ada di tas");
        }
        else
        {
            sb.AppendLine("  Status : belum ditemukan");
            sb.AppendLine("  Cocok   : rak buku di kamar 07");
        }
        sb.AppendLine();

        // Catatan hari ini diambil dari data naskah (asset NaskahData),
        // supaya teks cerita tidak ditulis di dalam kode.
        NaskahData naskah = NaskahData.Ambil(gs.hari);
        if (naskah != null)
        {
            if (!string.IsNullOrEmpty(naskah.judul))
            {
                sb.AppendLine("Hari ini: " + naskah.judul);
            }
            if (!string.IsNullOrEmpty(naskah.catatanTambahan))
            {
                sb.AppendLine(naskah.catatanTambahan);
            }
            sb.AppendLine();
        }

        sb.AppendLine("Lamaran");
        sb.AppendLine("  A (Studio Lajur)   : " + gs.statusLamaranA);
        sb.AppendLine("  B (Koperasi Pijar) : " + gs.statusLamaranB);
        sb.AppendLine();

        sb.AppendLine("Retakan Ruang");
        for (int i = 0; i < gs.retakanSelesai.Length; i++)
        {
            sb.Append("  Retakan ").Append(i + 1).Append(" : ");
            sb.Append(gs.retakanSelesai[i] ? "selesai" : "belum");
            sb.AppendLine();
        }
        sb.AppendLine();

        sb.AppendLine("Tips");
        sb.AppendLine("  Tab  : buka / tutup catatan ini");
        sb.AppendLine("  L    : buka / tutup laptop");
        sb.AppendLine("  Esc  : buka menu");
        sb.AppendLine("  WASD : jalan, E : interaksi");
        sb.AppendLine();

        // Perbandingan tawaran baru dibuka pada malam hari ke-7.
        if (gs.hari == 7 && gs.sesi == GameState.Sesi.Malam)
        {
            sb.AppendLine("--- CATATAN MALAM TERAKHIR ---");
            sb.AppendLine("A. Studio Lajur — 4,2 juta/bulan, 40 jam. Dokumen asli dipegang pekerja.");
            sb.AppendLine("B. Koperasi Pijar — 3,8 juta/bulan, kontrak 3 bulan, 35 jam. Dokumen asli tidak diminta.");
            sb.AppendLine("C. Akses Prima — sampai 12 juta, proses cepat. Minta KTP dan ijazah asli, pengunduran diri minimal 1 tahun. Alamat dan kontak tidak konsisten.");
            sb.AppendLine("Atau: tidak ada yang diambil, pulang dengan portofolio.");
            sb.AppendLine("Pilih lewat panel keputusan akhir.");
        }

        return sb.ToString();
    }

    // ---------------------------------------------------------------
    // Laptop (L) - 3 tab
    // ---------------------------------------------------------------

    public void BukaPanelLaptop()
    {
        tabLaptop = 0;
        PerbaruiLaptop();
        BukaPanel(panelLaptop);
    }

    private void TutupPanelLaptop()
    {
        TutupPanel(panelLaptop);
    }

    public void GantiTabLaptop(int tabBaru)
    {
        tabLaptop = Mathf.Clamp(tabBaru, 0, 2);
        PerbaruiLaptop();
    }

    private void PerbaruiLaptop()
    {
        if (teksLaptop == null) return;

        if (tabLaptop == 0) teksLaptop.text = BuildTeksLamaran();
        else if (tabLaptop == 1) teksLaptop.text = BuildTeksPesan();
        else teksLaptop.text = BuildTeksCatatan();

        // Daftar aktivitas hanya muncul di tab Lamaran.
        if (wadahAktivitas != null)
        {
            wadahAktivitas.gameObject.SetActive(tabLaptop == 0);
        }
    }

    // Tab "Lamaran": draf lowongan dari data naskah + draf CV Raka.
    // Isi lowongan diambil dari asset NaskahData, bukan ditulis di kode.
    private string BuildTeksLamaran()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return "";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("LAMARAN");
        sb.AppendLine();

        NaskahData naskah = NaskahData.Ambil(gs.hari);
        if (naskah != null && !string.IsNullOrEmpty(naskah.draftLowongan))
        {
            sb.AppendLine("Lowongan: " + naskah.perusahaanLowongan + " - " + naskah.judulLowongan);
            sb.AppendLine(naskah.draftLowongan);
            if (!string.IsNullOrEmpty(naskah.batasLamaran))
            {
                sb.AppendLine("Batas lamaran: " + naskah.batasLamaran);
            }
            if (!string.IsNullOrEmpty(naskah.syaratLamaran))
            {
                sb.AppendLine("Syarat: " + naskah.syaratLamaran);
            }
            sb.AppendLine();
        }

        sb.AppendLine("Draf CV Raka");
        sb.AppendLine("  Klaim 1: " + (gs.klaimDraf.Length > 0 ? gs.klaimDraf[0] : "-"));
        sb.AppendLine("  Klaim 2: " + (gs.klaimDraf.Length > 1 ? gs.klaimDraf[1] : "-"));
        sb.AppendLine("  Bukti   : " + BuktiKeTeks(gs.buktiTerpasang));
        sb.AppendLine();
        sb.AppendLine("Buat CV pakai tombol 'Rakit CV' di panel laptop.");
        sb.AppendLine("Pilih kegiatan di bawah. Biaya tenaga dan tekanan");
        sb.AppendLine("ditampilkan dulu sebelum kegiatan jalan.");

        return sb.ToString();
    }

    private string BuildTeksPesan()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return "";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("PESAN");
        sb.AppendLine();

        // Pesan pembuka hari ini (mis. pesan dari Ibu) diambil dari
        // asset NaskahData, supaya teksnya terpisah dari kode.
        NaskahData naskah = NaskahData.Ambil(gs.hari);
        if (naskah != null)
        {
            if (!string.IsNullOrEmpty(naskah.pesanPembuka))
            {
                sb.AppendLine(naskah.pengirim + " - " + naskah.pesanPembuka);
                sb.AppendLine();
            }

            // Pesan lain yang masuk di hari yang sama.
            for (int i = 0; i < naskah.pesanLain.Count; i++)
            {
                if (string.IsNullOrEmpty(naskah.pesanLain[i])) continue;
                sb.AppendLine(naskah.pesanLain[i]);
                sb.AppendLine();
            }
        }

        // Pesan dari Dita (deterministik, depends on hari).
        if (gs.hari == 1)
        {
            sb.AppendLine("Dita (11:04) — Raka, kabarnya ada yang mau magang di tempat kamu? Kabarin aja, aku bantu cek.");
        }
        else if (gs.hari == 2)
        {
            sb.AppendLine("Dita (19:20) — Jangan begadang. Besok masih ada yang perlu dilamar, kan?");
        }
        else
        {
            sb.AppendLine("Dita — Kabarin kalau ada yang membalas. Aku bantu cek syaratnya sekalian.");
        }
        sb.AppendLine();

        // Email dari perusahaan.
        int jadwalA = gs.jadwalInterviewA;
        int jadwalB = gs.jadwalInterviewB;

        if (jadwalA > 0 && gs.statusLamaranA == GameState.StatusLamaran.Terjadwal)
        {
            sb.AppendLine("Studio Lajur — Terima kasih lamaran Anda. Interview dijadwalkan pada hari ke-"
                          + jadwalA + ", sesi siang.");
            sb.AppendLine();
        }
        if (jadwalB > 0 && gs.statusLamaranB == GameState.StatusLamaran.Terjadwal)
        {
            sb.AppendLine("Koperasi Pijar — Terima kasih lamaran Anda. Interview dijadwalkan pada hari ke-"
                          + jadwalB + ", sesi siang.");
            sb.AppendLine();
        }
        if (gs.statusLamaranA == GameState.StatusLamaran.Ditawari)
        {
            sb.AppendLine("Studio Lajur — Kami menawarkan posisi Junior UI Implementer. Rinciannya ada di panel tawaran.");
            sb.AppendLine();
        }
        if (gs.statusLamaranB == GameState.StatusLamaran.Ditawari)
        {
            sb.AppendLine("Koperasi Pijar — Kami menawarkan posisi Web Support. Rinciannya ada di panel tawaran.");
            sb.AppendLine();
        }
        if (gs.statusLamaranA == GameState.StatusLamaran.Ditolak
            || gs.statusLamaranB == GameState.StatusLamaran.Ditolak)
        {
            sb.AppendLine("Lamaran yang ditolak: satu posisi, satu waktu. Kesempatan lain tetap terbuka.");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // ---------------------------------------------------------------
    // Daftar aktivitas (dibuat lewat kode)
    // ---------------------------------------------------------------

    private void BangunDaftarAktivitas()
    {
        if (wadahAktivitas == null) return;

        for (int i = wadahAktivitas.childCount - 1; i >= 0; i--)
        {
            GameObject go = wadahAktivitas.GetChild(i).gameObject;
            if (go == null) continue;
            Destroy(go);
        }

        GameState gs = GameState.Instance;
        if (gs == null || gs.activitySystem == null) return;

        List<ActivityData> daftar = gs.activitySystem.daftarAktivitas;
        for (int i = 0; i < daftar.Count; i++)
        {
            ActivityData aktivitas = daftar[i];
            if (aktivitas == null) continue;

            // Tanda mengikuti naskah: negative = tenaga berkurang (biaya),
            // positive = tenaga bertambah (istirahat).
            string label = aktivitas.nama + "  (tenaga " + (-aktivitas.biayaTenaga)
                         + ", tekanan " + aktivitas.biayaTekanan + ")";

            ActivityData aktivitasDipilih = aktivitas;
            BuatTombol(wadahAktivitas, label, delegate { MintaJalankanAktivitas(aktivitasDipilih); });
        }

        TerapkanUkuranTeks();
    }

    public void MintaJalankanAktivitas(ActivityData aktivitas)
    {
        GameState gs = GameState.Instance;
        if (gs == null || gs.activitySystem == null) return;

        // Cek lowongan punya panel sendiri, bukan konfirmasi biasa.
        if (aktivitas.tipe == ActivityData.Tipe.Verifikasi)
        {
            BukaPanelVerifikasi(aktivitas);
            return;
        }

        // Tutup laptop dulu supaya panel konfirmasi berada di atasnya.
        TutupPanelLaptop();
        gs.activitySystem.MintaJalankan(aktivitas);
    }

    // ---------------------------------------------------------------
    // Jeda (tekanan mencapai 6)
    // ---------------------------------------------------------------

    private void BukaJeda()
    {
        if (teksJeda != null)
        {
            teksJeda.text = "Raka berhenti sebentar.\n\nTekanan sudah tinggi. Tarik napas, minum air, "
                          + "lalu lanjut.\n\n(Tekanan diturunkan, Tenaga dijaga minimal 1. Jeda tidak memakai sesi.)";
        }
        BukaPanel(panelJeda);
    }

    private void TutupJeda()
    {
        GameState gs = GameState.Instance;
        if (gs != null) gs.TutupJeda();

        TutupPanel(panelJeda);
    }

    // ---------------------------------------------------------------
    // Menu Pause (Esc)
    // ---------------------------------------------------------------

    private void BukaPanelPause()
    {
        if (teksPause != null)
        {
            teksPause.text = "SELA\n\nHari / sesi: " + AmbilRingkasanWaktu() + "\n\n"
                           + "Esc atau Lanjut : kembali main\n"
                           + "Simpan         : simpan progres sekarang\n"
                           + "Mulai ulang    : hapus progres dan mulai dari hari 1\n"
                           + "Keluar         : keluar dari game";
        }
        BukaPanel(panelPause);
    }

    private string AmbilRingkasanWaktu()
    {
        GameState gs = GameState.Instance;
        if (gs == null) return "-";
        return "hari " + gs.hari + " / 7, sesi " + SessionManager.TeksSesi(gs.sesi);
    }

    private void SimpanManual()
    {
        SaveSystem.Simpan();
        TampilkanNotifikasi("Progres disimpan.");
    }

    // Tidur. Dipakai tombol di panel Catatan dan juga objek kasur di scene
    // (lewat UnityEvent OnInteract).
    public void Tidur()
    {
        GameState gs = GameState.Instance;
        if (gs == null || gs.sesiManager == null) return;

        // Tidur cuma boleh di sesi malam. Kalau dipanggil waktu lain,
        // activities-nya jadi tidak seimbang.
        if (gs.sesi != GameState.Sesi.Malam)
        {
            TampilkanNotifikasi("Raka belum mau tidur. Tidur cuma boleh di sesi malam.");
            return;
        }

        // Malam hari ketujuh bukan tidur, tapi keputusan akhir.
        if (gs.hari >= 7)
        {
            TampilkanNotifikasi("Malam terakhir. Pilih dulu tawarannya.");
            return;
        }

        gs.sesiManager.Tidur();
        TampilkanNotifikasi("Raka tidur. Besok pagi.");
    }

    private void MulaiUlang()
    {
        GameState gs = GameState.Instance;
        if (gs != null) gs.ResetSeluruhState();

        SaveSystem.HapusSave();

        TutupSemuaPanel();
        PerbaruiHUD();
        TampilkanNotifikasi("Permainan dimulai lagi dari hari 1.");
    }

    private void KeluarGame()
    {
        SaveSystem.Simpan();
        Application.Quit();
    }

    // ---------------------------------------------------------------
    // Keputusan akhir
    // ---------------------------------------------------------------

    private void BukaPilihanFinal()
    {
        GameState gs = GameState.Instance;

        bool adaA = (gs != null && gs.statusLamaranA == GameState.StatusLamaran.Ditawari);
        bool adaB = (gs != null && gs.statusLamaranB == GameState.StatusLamaran.Ditawari);

        if (teksPilihanFinal != null)
        {
            teksPilihanFinal.text = "MALAM TERAKHIR\n\n"
                                  + "Keputusan ini menentukan akhir cerita Raka.\n"
                                  + "Pilih satu, atau pilih untuk pulang.\n\n"
                                  + (adaA ? "A. Terima tawaran Studio Lajur\n"
                                          : "A. Tawaran Studio Lajur belum didapat\n")
                                  + (adaB ? "B. Terima tawaran Koperasi Pijar\n"
                                          : "B. Tawaran Koperasi Pijar belum didapat\n")
                                  + "C. Terima tawaran yang cepat\n"
                                  + "Tidak ada. Pulang dengan portofolio yang sudah rapi.";
        }

        BangunDaftarPilihanFinal();
        BukaPanel(panelPilihanFinal);
    }

    private void TutupPilihanFinal()
    {
        EndingManager em = EndingManager.Instance;
        if (em != null) em.TutupPilihanFinal();

        TutupPanel(panelPilihanFinal);
    }

    private void BangunDaftarPilihanFinal()
    {
        if (wadahPilihanFinal == null) return;

        for (int i = wadahPilihanFinal.childCount - 1; i >= 0; i--)
        {
            GameObject go = wadahPilihanFinal.GetChild(i).gameObject;
            if (go == null) continue;
            Destroy(go);
        }

        GameState gs = GameState.Instance;
        bool adaA = (gs != null && gs.statusLamaranA == GameState.StatusLamaran.Ditawari);
        bool adaB = (gs != null && gs.statusLamaranB == GameState.StatusLamaran.Ditawari);

        if (adaA)
        {
            BuatTombol(wadahPilihanFinal, "A. Terima tawaran Studio Lajur",
                       delegate { PilihTawaranFinal(GameState.PilihanTawaran.A); });
        }

        if (adaB)
        {
            BuatTombol(wadahPilihanFinal, "B. Terima tawaran Koperasi Pijar",
                       delegate { PilihTawaranFinal(GameState.PilihanTawaran.B); });
        }

        BuatTombol(wadahPilihanFinal, "C. Terima tawaran yang cepat",
                   delegate { PilihTawaranFinal(GameState.PilihanTawaran.C); });
        BuatTombol(wadahPilihanFinal, "Tidak ada. Pulang dengan portofolio yang sudah rapi.",
                   delegate { PilihTawaranFinal(GameState.PilihanTawaran.TidakAda); });

        TerapkanUkuranTeks();
    }

    private void PilihTawaranFinal(GameState.PilihanTawaran pilihan)
    {
        EndingManager em = EndingManager.Instance;
        if (em == null) return;

        em.MintaPilihFinal(pilihan);
    }

    // ---------------------------------------------------------------
    // Peringatan khusus tawaran C
    // ---------------------------------------------------------------

    private void BukaPeringatanC(int jumlahKonfirmasi)
    {
        EndingManager em = EndingManager.Instance;
        if (em == null) return;

        if (teksPeringatanC != null)
        {
            string tambahan = (jumlahKonfirmasi <= 0)
                ? ""
                : "\n\n(Konfirmasi " + jumlahKonfirmasi + " dari 2.)";
            teksPeringatanC.text = em.peringatanC + tambahan;
        }

        TutupPanel(panelPilihanFinal);
        BukaPanel(panelPeringatanC);
    }

    private void TutupPanelPeringatanC()
    {
        TutupPanel(panelPeringatanC);
    }

    private void KonfirmasiPeringatanC()
    {
        EndingManager em = EndingManager.Instance;
        if (em == null) return;

        em.KonfirmasiPeringatanC();
    }

    private void BatalPeringatanC()
    {
        EndingManager em = EndingManager.Instance;
        if (em == null) return;

        em.BatalPilihFinal();
        TutupPanel(panelPeringatanC);
    }

    // ---------------------------------------------------------------
    // Ending
    // ---------------------------------------------------------------

    private void TampilkanEnding()
    {
        EndingManager em = EndingManager.Instance;
        if (em == null) return;

        if (teksEnding != null) teksEnding.text = em.TeksLayarAkhir();

        TutupPanel(panelPilihanFinal);
        TutupPanel(panelPeringatanC);
        BukaPanel(panelEnding);
    }

    // ---------------------------------------------------------------
    // Save / load
    // ---------------------------------------------------------------

    private void TentangHasilMuat(SaveSystem.HasilMuat hasil)
    {
        if (hasil == SaveSystem.HasilMuat.CheckpointDitemukan)
        {
            TampilkanNotifikasi("File save utama rusak. Progres dimuat dari checkpoint sebelumnya.");
        }
    }

    // ---------------------------------------------------------------
    // Membuat tombol lewat kode
    // ---------------------------------------------------------------

    // Buat panel layar penuh (dipakai menu awal dan layar info konten).
    private GameObject BuatPanelLayar(string nama, Color warnaLatar)
    {
        GameObject go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image gambar = go.AddComponent<Image>();
        gambar.color = warnaLatar;
        gambar.raycastTarget = true;

        return go;
    }

    // Buat teks besar di dalam panel layar.
    private TextMeshProUGUI BuatTeksLayar(GameObject panel, string nama, string isi,
                                         float ukuran, TextAlignmentOptions perataan)
    {
        GameObject go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(70f, 90f);
        rt.offsetMax = new Vector2(-70f, -220f);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = isi;
        tmp.fontSize = ukuran;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.alignment = perataan;
        tmp.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;

        return tmp;
    }

    // Buat wadah tombol (vertikal) di bawah panel layar.
    private RectTransform BuatWadahTombol(GameObject panel, string nama)
    {
        GameObject go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 50f);
        rt.sizeDelta = new Vector2(320f, 150f);

        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rt;
    }

    // Buat tombol UI lengkap (gambar + teks) di bawah GameObject induk.
    // Dipakai untuk daftar aktivitas, opsi dialog, dan pilihan akhir.
    private Button BuatTombol(RectTransform induk, string label, UnityEngine.Events.UnityAction aksi)
    {
        if (induk == null) return null;

        GameObject go = new GameObject("Tombol", typeof(RectTransform));
        go.transform.SetParent(induk, false);

        Image gambar = go.AddComponent<Image>();
        gambar.color = new Color(0.16f, 0.17f, 0.20f, 0.95f);

        Button tombol = go.AddComponent<Button>();
        tombol.targetGraphic = gambar;

        LayoutElement tata = go.AddComponent<LayoutElement>();
        tata.minHeight = 42f;
        tata.preferredHeight = 42f;

        GameObject goTeks = new GameObject("Teks", typeof(RectTransform));
        goTeks.transform.SetParent(go.transform, false);

        TextMeshProUGUI teks = goTeks.AddComponent<TextMeshProUGUI>();
        teks.text = label;
        teks.fontSize = 17f;
        teks.color = Color.white;
        teks.textWrappingMode = TextWrappingModes.Normal;
        teks.raycastTarget = false;
        teks.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rtTeks = teks.rectTransform;
        rtTeks.anchorMin = new Vector2(0f, 0f);
        rtTeks.anchorMax = new Vector2(1f, 1f);
        rtTeks.offsetMin = new Vector2(10f, 6f);
        rtTeks.offsetMax = new Vector2(-10f, -6f);

        if (TMP_Settings.defaultFontAsset != null)
        {
            teks.font = TMP_Settings.defaultFontAsset;
        }
        else
        {
            Debug.LogWarning("[UIManager] TMP_Settings.defaultFontAsset masih null. "
                           + "Import TMP Essential Resources lewat Window > TMP Importer.");
        }

        if (aksi != null) tombol.onClick.AddListener(aksi);

        return tombol;
    }
}

/* ============================================================
   [CARA PASANG] UIManager.cs
   1. Taruh script ini di GameObject "Canvas" (yang punya Canvas + CanvasScaler).
   2. Semua panel dibuat sebagai GameObject ANAK dari Canvas, dengan NAMA
      PERSIS seperti ini (huruf besar, tanpa spasi):
        PanelHUD
        PanelPrompt
        PanelNotifikasi
        PanelDialog
        PanelCatatan
        PanelLaptop
        PanelKonfirmasiAktivitas
        PanelPause
        PanelJeda
        PanelPilihanFinal
        PanelPeringatanC
        PanelEnding
        PanelMenuAwal
        PanelInfoKonten
      Script ini mencari panel itu otomatis, jadi tidak perlu drag manual.
      Kalau ada yang tidak ketemu, akan muncul warning di Console.
      PanelMenuAwal dan PanelInfoKonten boleh tidak ada di scene:
      UIManager akan membuatnya sendiri lewat kode.
   3. Di dalam tiap panel, buat GameObject anak bernama "Teks" yang punya
      TextMeshProUGUI (atau "Pembicara" untuk panel Dialog).
      Panel Dialog butuh dua teks: "Pembicara" dan "Teks".
   4. Wadah tombol (dibuat lewat kode) — buat GameObject anak dengan
      RectTransform + Vertical Layout Group + Content Size Fitter:
        - di PanelLaptop  : nama "DaftarAktivitas"
        - di PanelDialog   : nama "DaftarOpsi"
        - di PanelPilihanFinal : nama "DaftarPilihan"
   5. Tombol manual yang harus dibuat di scene (nama GameObject):
        TombolLanjutDialog (PanelDialog)
        TombolYa, TombolBatal (PanelKonfirmasiAktivitas)
        TombolLanjut (PanelJeda)
        TombolYakin, TombolBatalC (PanelPeringatanC)
        TombolLanjut, TombolSimpan, TombolMulaiUlang, TombolKeluar (PanelPause)
      Assign ke kolom Inspector yang sesuai.
   6. Kanvas wajib punya "GraphicRaycaster", dan di Hierarchy harus ada
      GameObject "EventSystem" dengan komponen InputSystemUIInputModule
      (BUKAN StandaloneInputModule, karena project memakai Input System baru).
   7. Tab "Lamaran" di laptop: daftar aktivitas dibuat otomatis dari
      ActivitySystem.daftarAktivitas, jadi semua Asset ActivityData
      harus sudah ter-assign di ActivitySystem.
   8. UIManager otomatis mengubah FirstPersonController.uiTerbuka
      saat ada panel terbuka. Jangan diubah manual dari script lain.
   ============================================================ */
