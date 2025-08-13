using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KutuphaneOtomasyon
{
    public partial class YoneticiPanel : TemaUygulama
    {
        private Kullanicilar aktifYonetici;
        KutuphaneEntities db = new KutuphaneEntities();
        private int secilenID = -1;
        public YoneticiPanel(Kullanicilar kullanici)
        {
            InitializeComponent();
            aktifYonetici = kullanici;
            GizlePanel();
            pnlYoneticiAcilis.Visible = true;
            BilgileriYukle();
            DashboardVerileriniYukle();
            KutuphanecileriListele();
            UyeleriListele();
            KitapListesiniDoldur();
            HareketleriGetir();
            TalepleriGetir();
            UyeleriYukle();
            RaporlariListele();
            ListeleGecikenler();
            Emanetler();
        }
        private void GizlePanel()
        {
            pnlYoneticiAcilis.Visible = false;
            pnlProfil.Visible = false;
            pnlKutuphaneciIslem.Visible = false;
            pnlUyeBilgileri.Visible = false;
            pnlRaporlar.Visible = false;
            pnlKitapTakip.Visible = false;
            pnlKitapTalep.Visible = false;
            pnlGecikenveCeza.Visible = false;
            pnlEmanetler.Visible = false;
        }
        private void YoneticiPanel_Load(object sender, EventArgs e)
        {
            if (aktifYonetici == null)
            {
                MessageBox.Show("Aktif kullanıcı bilgisi bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            MessageBox.Show($"Hoş geldiniz {aktifYonetici.K_Ad}!", "Giriş Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtEskiSifre.UseSystemPasswordChar = true;
            txtYeniSifre.UseSystemPasswordChar = true;

            cmbStatu.Items.Clear();
            cmbStatu.Items.Add("Tümü");
            cmbStatu.Items.Add("Normal");
            cmbStatu.Items.Add("VIP");
            cmbStatu.Items.Add("KaraListe");
            cmbStatu.SelectedIndex = 0;

            cmbRaporTuru.Items.Add("GÖRÜNTÜLEMEK İSTEDİĞİNİZ RAPORU SEÇİNİZ.");
            cmbRaporTuru.Items.Add("KATEGORİ BAZINDA KİTAP DAĞILIMI");
            cmbRaporTuru.Items.Add("YAYINEVİNE GÖRE KİTAP SAYISI");
            cmbRaporTuru.Items.Add("AYLARA GÖRE ÖDÜNÇ VERİLENLER");
            cmbRaporTuru.Items.Add("EN ÇOK OKUNAN KİTAPLAR");
            cmbRaporTuru.Items.Add("EN ÇOK KİTAP ALAN ÜYELER");
            cmbRaporTuru.Items.Add("GECİKENLER, ZAMANINDA TESLİM ALINANLAR, İADE EDİLMEYENLER");
            cmbRaporTuru.Items.Add("KATEGORİYE GÖRE GECİKMELER");
            cmbRaporTuru.Items.Add("YILLARA GÖRE ÜYE SAYISI");
            cmbRaporTuru.Items.Add("GÜNLÜK VERİLEN KİTAPLAR");
            cmbRaporTuru.Items.Add("CEZA GELİRLERİ");
            cmbRaporTuru.SelectedIndex = 0;

            cmbIslemTuru.Items.Add("Tümü");
            cmbIslemTuru.Items.Add("Kitap İşlemleri");
            cmbIslemTuru.Items.Add("Üye İşlemleri");
            cmbIslemTuru.Items.Add("Talep İşlemleri");
            cmbIslemTuru.Items.Add("İade İşlemleri");
            cmbIslemTuru.SelectedIndex = 0;

            cmbTalepDurum.Items.Add("Tümü");
            cmbTalepDurum.Items.Add("Talep Edildi");
            cmbTalepDurum.Items.Add("Talep Onaylandı");
            cmbTalepDurum.Items.Add("Talep Reddedildi");
            cmbTalepDurum.Items.Add("Talep İptal Edildi");
            cmbTalepDurum.SelectedIndex = 0;

            cmbEmanetFiltrele.Items.Add("Tümü");
            cmbEmanetFiltrele.Items.Add("Aktif");
            cmbEmanetFiltrele.Items.Add("Teslim Edildi");
            cmbEmanetFiltrele.SelectedIndex = 0;

            cmbKitapDurum.Items.Add("Hepsi");
            cmbKitapDurum.Items.Add("Kütüphanede");
            cmbKitapDurum.Items.Add("Stokta Yok");
            cmbKitapDurum.Items.Add("Üyede"); 
            cmbKitapDurum.SelectedIndex = 0;

        }

        private void btn_Cikis_Click(object sender, EventArgs e)
        {
            Giris login = new Giris();
            login.Show();
            this.Hide();
        }

        //AÇILIŞ PANEL
        private void DashboardVerileriniYukle()
        {
            lblToplamKitap.Text = db.Kitaplar.Count().ToString();
            lblToplamUye.Text = db.Kullanicilar.Count(k => k.Roller.Any(r => r.RolAdi == "Üye")).ToString();
            lblToplamKutuphaneci.Text = db.Kullanicilar.Count(k => k.Roller.Any(r => r.RolAdi == "Kütüphaneci")).ToString();
            lblEmanette.Text = db.OduncIslemleri.Count(o => o.IadeTarihi == null).ToString();
            lblGeciken.Text = db.OduncIslemleri
                                .Count(o => o.IadeTarihi == null &&
                                            DbFunctions.DiffDays(o.VerilisTarihi, DateTime.Now) > o.GecikmeGun)
                                .ToString();
            lblCezaGeliri.Text = db.OduncIslemleri
                                .Sum(o => (decimal?)o.CezaTutari)
                                ?.ToString("C2") ?? "₺0,00";
        }
        private void btnAnaSayfa_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlYoneticiAcilis.Visible = true;
        }
        //PROFİL PANEL
        private void btnProfil_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlProfil.Visible = true;
        }
        private void BilgileriYukle()
        {
            lblAdSoyad.Text = $"{aktifYonetici.K_Ad} {aktifYonetici.Soyad}";
            txtEmail.Text = aktifYonetici.Email;
            lblRol.Text = aktifYonetici.Roller?.FirstOrDefault()?.RolAdi ?? "Bilinmiyor";
        }
        private void btnGuncelBilgi_Click(object sender, EventArgs e)
        {
            string yeniEmail = txtEmail.Text.Trim();
            string eskiSifre = txtEskiSifre.Text.Trim();
            string yeniSifre = txtYeniSifre.Text.Trim();

            var kullaniciDb = db.Kullanicilar.Find(aktifYonetici.KullaniciID);
            if (kullaniciDb == null)
            {
                MessageBox.Show("Kullanıcı bulunamadı.");
                return;
            }

            bool guncellemeVar = false;

            // Email güncelleme
            if (!string.IsNullOrEmpty(yeniEmail) && yeniEmail != kullaniciDb.Email)
            {
                if (db.Kullanicilar.Any(k => k.Email == yeniEmail && k.KullaniciID != aktifYonetici.KullaniciID))
                {
                    MessageBox.Show("Bu e-posta başka bir kullanıcı tarafından kullanılıyor.");
                    return;
                }
                kullaniciDb.Email = yeniEmail;
                aktifYonetici.Email = yeniEmail;
                guncellemeVar = true;
            }

            // Şifre güncelleme
            if (!string.IsNullOrEmpty(eskiSifre) && !string.IsNullOrEmpty(yeniSifre))
            {
                if (kullaniciDb.Sifre != eskiSifre)
                {
                    MessageBox.Show("Eski şifre yanlış!");
                    return;
                }
                if (eskiSifre == yeniSifre)
                {
                    MessageBox.Show("Yeni şifre eski şifreyle aynı olamaz.");
                    return;
                }
                // Şifre kuralları
                if (yeniSifre.Length < 8 ||
                    !yeniSifre.Any(char.IsUpper) ||
                    !yeniSifre.Any(char.IsLower) ||
                    !yeniSifre.Any(char.IsDigit) ||
                    !yeniSifre.Any(c => !char.IsLetterOrDigit(c)))
                {
                    MessageBox.Show("Yeni şifre en az 8 karakter uzunluğunda, büyük harf, küçük harf, rakam ve özel karakter içermelidir.");
                    return;
                }
                kullaniciDb.Sifre = yeniSifre;
                aktifYonetici.Sifre = yeniSifre;
                guncellemeVar = true;
            }

            if (guncellemeVar)
            {
                db.SaveChanges();
                MessageBox.Show("Bilgiler başarıyla güncellendi.");
                txtEskiSifre.Clear();
                txtYeniSifre.Clear();
            }
            else
            {
                MessageBox.Show("Güncellenecek bir bilgi yok.");
            }
        }
        private void chkSifreGoster_CheckedChanged(object sender, EventArgs e)
        {
            bool sifreyiGoster = chkSifreGoster.Checked;
            txtEskiSifre.UseSystemPasswordChar = !sifreyiGoster;
            txtYeniSifre.UseSystemPasswordChar = !sifreyiGoster;
        }

        //KÜTÜPHANECİ İŞLEMLERİ PANEL
        private void btnKutuphaneciIslemleri_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlKutuphaneciIslem.Visible = true;
        }
        private void btnKutuphaneciDurumGuncelle_Click(object sender, EventArgs e)
        {

            if (dgvKutuphaneciler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir üye seçin.");
                return;
            }

            int secilenID = Convert.ToInt32(dgvKutuphaneciler.SelectedRows[0].Cells["KullaniciID"].Value);
            var kutuphaneci = db.Kullanicilar.Find(secilenID);

            if (kutuphaneci == null)
            {
                MessageBox.Show("Üye bulunamadı.");
                return;
            }

            bool yeniDurum = chkKutuphaneciAktif.Checked;
            kutuphaneci.Aktif = yeniDurum;

            string adSoyad = $"{kutuphaneci.K_Ad} {kutuphaneci.Soyad}";
            string durumMetni = yeniDurum ? "Aktif" : "Pasif";

            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifYonetici.KullaniciID,
                IslemTipi = "Durum Güncelleme",
                IslemAciklamasi = $"Kütüphanecinin durumu güncellendi: {adSoyad} (ID: {secilenID}) → Yeni Durum: {durumMetni}",
                IslemTarihi = DateTime.Now
            });

            db.SaveChanges();
            MessageBox.Show("Durum güncellendi!");
            KutuphanecileriListele();
        }
        private void KutuphanecileriListele()
        {
            var kutuphaneciler = db.Kullanicilar
                .Where(k => k.Roller.Any(r => r.RolID == 1))
                .Select(k => new
                {
                    k.KullaniciID,
                    k.K_Ad,
                    k.Soyad,
                    k.Email,
                    k.Sifre,
                    k.Telefon,
                    AktifMi = k.Aktif ? "Aktif" : "Pasif",
                    IlkGiris = db.KutuphaneGorevliDetay
               .Where(d => d.KullaniciID == k.KullaniciID)
               .Select(d => d.IlkGiris)
               .FirstOrDefault() == false
        })
                .ToList();

            dgvKutuphaneciler.DataSource = kutuphaneciler;
            dgvKutuphaneciler.Columns["KullaniciID"].Visible = false;
            dgvKutuphaneciler.Columns["IlkGiris"].Visible = false;
            dgvKutuphaneciler.Columns["Sifre"].Visible = false;
            dgvKutuphaneciler.Columns["K_Ad"].HeaderText = "Ad";
            dgvKutuphaneciler.Columns["Soyad"].HeaderText = "Soyad";
            dgvKutuphaneciler.Columns["Email"].HeaderText = "Email";
            dgvKutuphaneciler.Columns["Telefon"].HeaderText = "Telefon";
            dgvKutuphaneciler.Columns["AktifMi"].HeaderText = "Durum";

        }
        private void btnKutuphaneciEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKutuphaneciAdi.Text) ||
                string.IsNullOrWhiteSpace(txtKutuphaneciSoyadi.Text) ||
                string.IsNullOrWhiteSpace(txtKutuphaneciEmail.Text) ||
                string.IsNullOrWhiteSpace(txtKutuphaneciTelefon.Text) ||
                string.IsNullOrWhiteSpace(txtKutuphaneciSifre.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            var kutuphaneciRol = db.Roller.FirstOrDefault(r => r.RolID == 1);
            if (kutuphaneciRol == null)
            {
                MessageBox.Show("Kütüphaneci rolü bulunamadı.");
                return;
            }

            var yeniKullanici = new Kullanicilar
            {
                K_Ad = txtKutuphaneciAdi.Text.Trim(),
                Soyad = txtKutuphaneciSoyadi.Text.Trim(),
                Email = txtKutuphaneciEmail.Text.Trim(),
                Telefon = txtKutuphaneciTelefon.Text.Trim(),
                Sifre = txtKutuphaneciSifre.Text.Trim(),

            };

            yeniKullanici.Roller.Add(kutuphaneciRol);
            db.Kullanicilar.Add(yeniKullanici);

            var yeniGorevliDetay = new KutuphaneGorevliDetay
            {
                KullaniciID = yeniKullanici.KullaniciID,
                IlkGiris = false
            };
            db.KutuphaneGorevliDetay.Add(yeniGorevliDetay);
            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifYonetici.KullaniciID,
                IslemTipi = "Ekleme",
                IslemAciklamasi = $"Kütüphaneci eklendi: {yeniKullanici.K_Ad} {yeniKullanici.Soyad}",
                IslemTarihi = DateTime.Now
            });

            db.SaveChanges();
            MessageBox.Show("Kütüphaneci başarıyla eklendi.");
            KutuphanecileriListele();
            Temizle();

        }
        private void btnKutuphaneciGuncelle_Click(object sender, EventArgs e)
        {
            if (secilenID == -1)
            {
                MessageBox.Show("Lütfen güncellenecek kişiyi seçin.");
                return;
            }

            var kullanici = db.Kullanicilar.Find(secilenID);

            kullanici.K_Ad = txtKutuphaneciAdi.Text.Trim();
            kullanici.Soyad = txtKutuphaneciSoyadi.Text.Trim();
            kullanici.Email = txtKutuphaneciEmail.Text.Trim();
            kullanici.Telefon = txtKutuphaneciTelefon.Text.Trim();
            kullanici.Sifre = txtKutuphaneciSifre.Text.Trim();

            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifYonetici.KullaniciID,
                IslemTipi = "Güncelleme",
                IslemAciklamasi = $"Kütüphaneci güncellendi: {kullanici.K_Ad} {kullanici.Soyad}",
                IslemTarihi = DateTime.Now
            });

            db.SaveChanges();
            MessageBox.Show("Kütüphaneci güncellendi.");
            KutuphanecileriListele();
            Temizle();
        }
        private void btnKutuphaneciSil_Click(object sender, EventArgs e)
        {
            if (secilenID == 0)
            {
                MessageBox.Show("Lütfen silinecek kişiyi seçin.");
                return;
            }
            var kullanici = db.Kullanicilar.Find(secilenID);
            var oduncIslemleri = db.OduncIslemleri.Where(o => o.IslemYapanID == secilenID).ToList();
            if (oduncIslemleri.Any())
            {
                foreach (var odunc in oduncIslemleri)
                {
                    odunc.IslemYapanID = null;
                }
            }
            var oduncTalepleri = db.OduncTalepleri.Where(t => t.OnaylayanID == secilenID).ToList();
            if (oduncTalepleri.Any())
            {
                foreach (var talep in oduncTalepleri)
                {
                    talep.OnaylayanID = null;
                }
            }
            string adSoyad = $"{kullanici.K_Ad} {kullanici.Soyad}";
            db.Kullanicilar.Remove(kullanici);
            var gorevliDetay = db.KutuphaneGorevliDetay.FirstOrDefault(g => g.KullaniciID == secilenID);
            if (gorevliDetay != null)
            db.KutuphaneGorevliDetay.Remove(gorevliDetay);


            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifYonetici.KullaniciID,
                IslemTipi = "Silme",
                IslemAciklamasi = $"Kütüphaneci silindi: {adSoyad}",
                IslemTarihi = DateTime.Now
            });

            db.SaveChanges();

            MessageBox.Show("Kütüphaneci silindi.");
            KutuphanecileriListele();
            Temizle();
        }
        private void Temizle()
        {
            txtKutuphaneciAdi.Clear();
            txtKutuphaneciSoyadi.Clear();
            txtKutuphaneciEmail.Clear();
            txtKutuphaneciTelefon.Clear();
            txtKutuphaneciSifre.Clear();
            secilenID = -1;
        }
        private void dgvKutuphaneciler_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow secilen = dgvKutuphaneciler.Rows[e.RowIndex];

            txtKutuphaneciAdi.Text = secilen.Cells["K_Ad"].Value?.ToString();
            txtKutuphaneciSoyadi.Text = secilen.Cells["Soyad"].Value?.ToString();
            txtKutuphaneciEmail.Text = secilen.Cells["Email"].Value?.ToString();
            txtKutuphaneciTelefon.Text = secilen.Cells["Telefon"].Value?.ToString();
            secilenID = Convert.ToInt32(secilen.Cells["KullaniciID"].Value);
        }
        private void dgvKutuphaneciler_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvKutuphaneciler.ClearSelection();
        }
        //ÜYE BİLGİLERİ PANEL
        private void btnUyeBilgileri_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlUyeBilgileri.Visible = true;
        }
        private void UyeleriListele()
        {
            var uyeler = db.Kullanicilar
                .Where(k => k.Roller.Any(r => r.RolAdi == "Üye"))
                .Select(k => new
                {
                    k.KullaniciID,
                    AdSoyad = k.K_Ad + " " + k.Soyad,
                    k.Email,
                    k.Telefon,
                    AktifMi = k.Aktif ? "Aktif" : "Pasif",
                    Statu = db.UyeDetay
                             .Where(ud => ud.KullaniciID == k.KullaniciID)
                             .Select(ud => ud.Statu)
                             .FirstOrDefault()
                });

            dgvUyeler.DataSource = uyeler.ToList();

            dgvUyeler.Columns["KullaniciID"].Visible = false;
            dgvUyeler.Columns["AdSoyad"].HeaderText = "Ad Soyad";
            dgvUyeler.Columns["Email"].HeaderText = "E‑Mail";
            dgvUyeler.Columns["Telefon"].HeaderText = "Telefon";
            dgvUyeler.Columns["AktifMi"].HeaderText = "Durum";
            dgvUyeler.Columns["Statu"].HeaderText = "Statü";

            grpUyeDetay.Visible = false;
        }

        private void btnUyeSil_Click(object sender, EventArgs e)
        {
            if (dgvUyeler.SelectedRows.Count == 0) return;

            int uyeID = Convert.ToInt32(dgvUyeler.SelectedRows[0].Cells["KullaniciID"].Value);

            bool aktifEmanetVar = db.OduncIslemleri.Any(o => o.KullaniciID == uyeID && o.IadeTarihi == null);
            bool cezaVar = db.OduncIslemleri.Any(o => o.KullaniciID == uyeID && o.CezaTutari > 0);

            if (aktifEmanetVar || cezaVar)
            {
                MessageBox.Show("Bu üye silinemez. Aktif emanetleri veya cezası var.");
                return;
            }

            var uye = db.Kullanicilar.Find(uyeID);
            if (uye == null)
            {
                MessageBox.Show("Üye bulunamadı.");
                return;
            }

            string adSoyad = $"{uye.K_Ad} {uye.Soyad}";

            if (MessageBox.Show("Üyeyi silmek istediğinize emin misiniz?",
                                "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (uye.Roller != null && uye.Roller.Count > 0)
                {
                    uye.Roller.Clear(); 
                }
                var detay = db.UyeDetay.FirstOrDefault(d => d.KullaniciID == uyeID);
                if (detay != null)
                {
                    db.UyeDetay.Remove(detay);
                }
                db.Kullanicilar.Remove(uye);
                db.Raporlar.Add(new Raporlar
                {
                    KullaniciID = aktifYonetici.KullaniciID,
                    IslemTipi = "Üye Silme",
                    IslemAciklamasi = $"Üye silindi: {adSoyad} (ID: {uyeID})",
                    IslemTarihi = DateTime.Now
                });

                db.SaveChanges();
                MessageBox.Show("Üye silindi.");
                UyeleriListele();
            }
        }
        private void dgvUyeler_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int secilenKullaniciID = Convert.ToInt32(dgvUyeler.Rows[e.RowIndex].Cells["KullaniciID"].Value);

            var kullanici = db.Kullanicilar
                .Where(k => k.KullaniciID == secilenKullaniciID)
                .Select(k => new
                {
                    k.KullaniciID,
                    k.K_Ad,
                    k.Soyad,
                    k.Email,
                    k.Telefon,
                    Detay = db.UyeDetay.FirstOrDefault(ud => ud.KullaniciID == k.KullaniciID)
                })
                .FirstOrDefault();

            if (kullanici == null) return;

            txtAdSoyad.Text = kullanici.K_Ad + " " + kullanici.Soyad;
            txtEposta.Text = kullanici.Email;
            txtTelefon.Text = kullanici.Telefon;

            if (kullanici.Detay != null)
            {
                txtAdres.Text = kullanici.Detay.Adres ?? "";
                txtBakiye.Text = (kullanici.Detay.Bakiye ?? 0m).ToString("0.00") + " ₺";
            }
            else
            {
                txtAdres.Text = "";
                txtBakiye.Text = "0.00 ₺";
            }

            decimal toplamCeza = db.OduncIslemleri
                .Where(o => o.KullaniciID == secilenKullaniciID && o.CezaTutari > 0)
                .Sum(o => (decimal?)o.CezaTutari) ?? 0;

            int aktifEmanet = db.OduncIslemleri
                .Count(o => o.KullaniciID == secilenKullaniciID && o.IadeTarihi == null);

            int gecikenEmanet = db.OduncIslemleri
                .Count(o => o.KullaniciID == secilenKullaniciID &&
                            o.IadeTarihi == null &&
                            o.TeslimTarihi < DateTime.Now);

            txtToplamCeza.Text = toplamCeza.ToString("0.00") + " ₺";
            txtAktifEmanet.Text = aktifEmanet.ToString();
            txtGecikenEmanet.Text = gecikenEmanet.ToString();

            grpUyeDetay.Visible = true;
        }
        private void btnStatuGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvUyeler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir üye seçin.");
                return;
            }

            int secilenID = Convert.ToInt32(dgvUyeler.SelectedRows[0].Cells["KullaniciID"].Value);
            string yeniStatu = cmbStatu.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(yeniStatu))
            {
                MessageBox.Show("Lütfen bir statü seçin.");
                return;
            }

            var detay = db.UyeDetay.FirstOrDefault(ud => ud.KullaniciID == secilenID);
            var uye = db.Kullanicilar.Find(secilenID);

            if (detay == null || uye == null)
            {
                MessageBox.Show("Üyenin detay bilgisi bulunamadı.");
                return;
            }

            detay.Statu = yeniStatu;
            string adSoyad = $"{uye.K_Ad} {uye.Soyad}";

            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifYonetici.KullaniciID,
                IslemTipi = "Statü Güncelleme",
                IslemAciklamasi = $"Üyenin statüsü güncellendi: {adSoyad} (ID: {secilenID}) → Yeni Statü: {yeniStatu}",
                IslemTarihi = DateTime.Now
            });

            db.SaveChanges();
            MessageBox.Show("Üyenin statüsü güncellendi!");
            UyeleriListele();
        }
        private void btnDurumGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvUyeler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir üye seçin.");
                return;
            }

            int secilenID = Convert.ToInt32(dgvUyeler.SelectedRows[0].Cells["KullaniciID"].Value);
            var uye = db.Kullanicilar.Find(secilenID);

            if (uye == null)
            {
                MessageBox.Show("Üye bulunamadı.");
                return;
            }

            bool yeniDurum = chkAktif.Checked;
            uye.Aktif = yeniDurum;

            string adSoyad = $"{uye.K_Ad} {uye.Soyad}";
            string durumMetni = yeniDurum ? "Aktif" : "Pasif";

            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifYonetici.KullaniciID,
                IslemTipi = "Durum Güncelleme",
                IslemAciklamasi = $"Üyenin durumu güncellendi: {adSoyad} (ID: {secilenID}) → Yeni Durum: {durumMetni}",
                IslemTarihi = DateTime.Now
            });

            db.SaveChanges();
            MessageBox.Show("Durum güncellendi!");
            UyeleriListele();
        }
        private void dgvUyeler_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvUyeler.ClearSelection();
        }
      
        //KİTAP TAKİP PANEL
        private void btnKitapTakip_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlKitapTakip.Visible = true;
        }
        private void KitapListesiniDoldur()
        {
            
            var kitapListesi = db.Kitaplar
                .Select(k => new { k.KitapID, k.KitapAdi })
                .ToList();

            kitapListesi.Insert(0, new { KitapID = 0, KitapAdi = "Tümü" });

            cmbKitapSec.DisplayMember = "KitapAdi"; //Kitap Takip
            cmbKitapSec.ValueMember = "KitapID";
            cmbKitapSec.DataSource = kitapListesi;

            cmbKitapTalep.DisplayMember = "KitapAdi"; //Kitap Talep
            cmbKitapTalep.ValueMember = "KitapID";
            cmbKitapTalep.DataSource = kitapListesi;
        }
        private void HareketleriGetir(int? kitapId = null, string durumFiltre = null)
        {
            var liste = db.Kitaplar
        .Where(k => !kitapId.HasValue || k.KitapID == kitapId.Value)
        .GroupJoin(
            db.OduncIslemleri.Where(o => o.Kullanicilar.Roller.Any(r => r.RolAdi == "Üye")),
            kitap => kitap.KitapID,
            odunc => odunc.KitapID,
            (kitap, oduncList) => new { kitap, oduncList }
        )
        .SelectMany(
            x => x.oduncList.DefaultIfEmpty(),
            (x, odunc) => new
            {
                x.kitap.KitapID,
                x.kitap.KitapAdi,
                UyeAdi = (odunc != null && odunc.Kullanicilar != null)
                    ? (odunc.Kullanicilar.K_Ad + " " + odunc.Kullanicilar.Soyad)
                    : "",
                VerilisTarihi = odunc != null ? odunc.VerilisTarihi : (DateTime?)null,
                IadeTarihi = odunc != null ? odunc.IadeTarihi : (DateTime?)null,
                Durum = (x.kitap.Stok <= 0)
                    ? "Stokta Yok"
                    : (
                        (odunc != null && odunc.IadeTarihi == null)
                        ? ("Üyede: " + odunc.Kullanicilar.K_Ad + " " + odunc.Kullanicilar.Soyad)
                        : "Kütüphanede"
                      )
            }
        )
        .ToList();
            if (!string.IsNullOrEmpty(durumFiltre) && durumFiltre != "Hepsi")
            {
               
                if (durumFiltre == "Üyede")
                {
                    liste = liste.Where(x => x.Durum.StartsWith("Üyede:")).ToList();
                }
                else
                {
                    liste = liste.Where(x => x.Durum == durumFiltre).ToList();
                }
            }

            dgvKitapTakip.DataSource = liste;

            if (dgvKitapTakip.Columns.Count > 0)
            {
                dgvKitapTakip.Columns["KitapID"].Visible = false; 
                dgvKitapTakip.Columns["KitapAdi"].HeaderText = "Kitap Adı";
                dgvKitapTakip.Columns["UyeAdi"].HeaderText = "Son Alan Üye";
                dgvKitapTakip.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
                dgvKitapTakip.Columns["IadeTarihi"].HeaderText = "İade Tarihi";
                dgvKitapTakip.Columns["Durum"].HeaderText = "Durum";
            }
        }
        private void cmbKitapSec_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbKitapSec.SelectedValue != null)
            {
                int secilenKitapId = Convert.ToInt32(cmbKitapSec.SelectedValue);

                if (secilenKitapId == 0)
                    HareketleriGetir();
                else
                    HareketleriGetir(secilenKitapId);
            }
        }
        private void cmbKitapDurum_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string secilenDurum = cmbKitapDurum.SelectedItem.ToString();
            HareketleriGetir(null, secilenDurum);
        }
        private void dgvKitapTakip_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvKitapTakip.ClearSelection();
        }
        //KİTAP TALEPLERİ PANEL
        private void btnKitapTalep_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlKitapTalep.Visible = true;
        }
        private void TalepleriGetir(int? kitapId = null)
        {
            var talepler = db.OduncTalepleri
                .Where(t => !kitapId.HasValue || t.KitapID == kitapId.Value)
                .Select(t => new
                {
                    t.TalepID,
                    t.Kitaplar.KitapAdi,
                    UyeAdi = t.Uye.K_Ad + " " + t.Uye.Soyad,
                    t.TalepTarihi,
                    t.TalepDurumu,
                    OnaylayanAdi = t.Onaylayan != null
                                   ? (t.Onaylayan.K_Ad + " " + t.Onaylayan.Soyad)
                                   : "",
                    t.OnayTarihi,
                    t.Aciklama
                })
                .OrderByDescending(t => t.TalepTarihi)
                .ToList();

            dgvKitapTalep.DataSource = talepler;

            if (dgvKitapTalep.Columns.Count > 0)
            {
                dgvKitapTalep.Columns["TalepID"].Visible = false;
                dgvKitapTalep.Columns["KitapAdi"].HeaderText = "Kitap Adı";
                dgvKitapTalep.Columns["UyeAdi"].HeaderText = "Talep Eden Üye";
                dgvKitapTalep.Columns["TalepTarihi"].HeaderText = "Talep Tarihi";
                dgvKitapTalep.Columns["TalepDurumu"].HeaderText = "Durum";
                dgvKitapTalep.Columns["OnaylayanAdi"].HeaderText = "Onaylayan";
                dgvKitapTalep.Columns["OnayTarihi"].HeaderText = "Onay Tarihi";
                dgvKitapTalep.Columns["Aciklama"].HeaderText = "Açıklama";
            }
        }
        private void cmbKitapTalep_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbKitapTalep.SelectedValue != null)
            {
                int secilen = Convert.ToInt32(cmbKitapTalep.SelectedValue);
                if (secilen == 0)
                    TalepleriGetir();        
                else
                    TalepleriGetir(secilen); 
            }
        }
        private void UyeleriYukle()
        {
            var uyeler = db.Kullanicilar
                           .Where(k => k.Roller.Any(r => r.RolAdi == "Üye"))
                           .Select(k => new { k.KullaniciID, AdSoyad = k.K_Ad + " " + k.Soyad })
                           .ToList();

            uyeler.Insert(0, new { KullaniciID = 0, AdSoyad = "Tümü" });

            cmbTalepUye.DisplayMember = "AdSoyad";  //Kitap Talep Panel
            cmbTalepUye.ValueMember = "KullaniciID";
            cmbTalepUye.DataSource = uyeler;

            cmbUyeFiltrele.DisplayMember = "AdSoyad"; //Gecikme ve Ceza Panel
            cmbUyeFiltrele.ValueMember = "KullaniciID";
            cmbUyeFiltrele.DataSource = uyeler;
        }
        private void RaporlariListele()
        {
            int secilenUyeID = (int)cmbTalepUye.SelectedValue;
            string secilenDurum = cmbTalepDurum.SelectedItem?.ToString() ?? "Tümü";

            var talepIslemTipleri = new[]
            {
                "Talep Edildi",
                "Talep Onaylandı",
                "Talep Reddedildi",
                "Talep İptal Edildi"
            };

            var liste = (from r in db.Raporlar
                         where talepIslemTipleri.Contains(r.IslemTipi)
                               && (secilenDurum == "Tümü" || r.IslemTipi == secilenDurum)
                               &&
                               (secilenUyeID == 0 || r.KullaniciID == secilenUyeID || r.IlgiliUyeID == secilenUyeID)
                         join u in db.Kullanicilar.Where(k => k.Roller.Any(rr => rr.RolAdi == "Üye"))
                            on (r.IlgiliUyeID ?? r.KullaniciID) equals u.KullaniciID into uyeJoin
                         from uj in uyeJoin.DefaultIfEmpty()

                         join k in db.Kitaplar
                            on r.IlgiliKitapID equals k.KitapID into kitapJoin
                         from kj in kitapJoin.DefaultIfEmpty()

                         orderby r.IslemTarihi descending
                         select new
                         {
                             r.RaporID,
                             UyeAdi = uj != null ? (uj.K_Ad + " " + uj.Soyad) : "Bilinmiyor",
                             KitapAdi = kj != null ? kj.KitapAdi : "Bilinmiyor",
                             r.IslemTipi,
                             r.IslemAciklamasi,
                             r.IslemTarihi
                         }).ToList();

            dgvRaporlar.DataSource = liste;

            if (dgvRaporlar.Columns.Count > 0)
            {
                dgvRaporlar.Columns["RaporID"].Visible = false;
                dgvRaporlar.Columns["UyeAdi"].HeaderText = "Üye";
                dgvRaporlar.Columns["KitapAdi"].HeaderText = "Kitap";
                dgvRaporlar.Columns["IslemTipi"].HeaderText = "İşlem Tipi";
                dgvRaporlar.Columns["IslemAciklamasi"].HeaderText = "Açıklama";
                dgvRaporlar.Columns["IslemTarihi"].HeaderText = "Tarih";
            }
        }
        private void dgvRaporlar_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvRaporlar.ClearSelection();
        }
        private void cmbTalepUye_SelectedIndexChanged(object sender, EventArgs e)
        {
            RaporlariListele();
        }
        private void cmbTalepDurum_SelectedIndexChanged(object sender, EventArgs e)
        {
            RaporlariListele();
        }
        private void dgvKitapTalep_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvKitapTalep.ClearSelection();
        }
        //EMANETLER PANEL
        private void btnEmanetler_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlEmanetler.Visible = true;
        }
        private void Emanetler(string emanetFiltre = "Tümü")
        {
            var sorgu = db.OduncIslemleri.AsQueryable();


            if (emanetFiltre == "Aktif")
            {
                sorgu = sorgu.Where(x => x.IadeTarihi == null);
            }
            else if (emanetFiltre == "Teslim Edildi")
            {
                sorgu = sorgu.Where(x => x.IadeTarihi != null);
            }

            var liste = sorgu
                .Select(x => new
                {
                    x.IslemID,
                    x.Kitaplar.KitapAdi,
                    UyeAdi = x.Kullanicilar.K_Ad + " " + x.Kullanicilar.Soyad,
                    Verilis = x.VerilisTarihi,
                    TeslimEdilmesiGereken = x.TeslimTarihi,
                    x.IadeTarihi,
                    Durum = (x.IadeTarihi == null ? "Aktif" : "Teslim Edildi")
                })
                .ToList();

            dgvEmanetler.DataSource = liste;

            if (dgvEmanetler.Columns.Count > 0)
            {
                dgvEmanetler.Columns["IslemID"].Visible = false;
                dgvEmanetler.Columns["KitapAdi"].HeaderText = "Kitap Adı";
                dgvEmanetler.Columns["UyeAdi"].HeaderText = "Üye Adı";
                dgvEmanetler.Columns["Verilis"].HeaderText = "Veriliş Tarihi";
                dgvEmanetler.Columns["TeslimEdilmesiGereken"].HeaderText = "Teslim Tarihi";
                dgvEmanetler.Columns["IadeTarihi"].HeaderText = "İade Tarihi";
                dgvEmanetler.Columns["Durum"].HeaderText = "Durum";
            }
        }
        private void cmbEmanetFiltrele_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string secilenDurum = cmbEmanetFiltrele.SelectedItem.ToString();
            Emanetler(secilenDurum);
        }
        private void dgvEmanetler_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvEmanetler.ClearSelection();
        }
        //GECİKEN KİTAPLAR VE CEZA DURUMU PANEL
        private void btnGecikenveCeza_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlGecikenveCeza.Visible = true;
        }
        private void ListeleGecikenler(int? kullaniciId = null)
        {
            var query = db.OduncIslemleri
                .Where(o => o.IadeTarihi != null && o.CezaTutari > 0 && o.Kullanicilar.Roller.Any(r => r.RolAdi == "Üye"));

            if (kullaniciId.HasValue && kullaniciId.Value != 0)
                query = query.Where(o => o.KullaniciID == kullaniciId.Value);

            var liste = query.Select(o => new
            {
                o.KullaniciID,
                AdSoyad = o.Kullanicilar.K_Ad + " " + o.Kullanicilar.Soyad,
                o.Kitaplar.KitapAdi,
                o.VerilisTarihi,
                o.TeslimTarihi,
                CezaOdemeTarihi=o.IadeTarihi,
                o.GecikmeGun,
                o.CezaTutari,
                ToplamGecikmeSayisi = db.OduncIslemleri.Count(x => x.KullaniciID == o.KullaniciID && x.CezaTutari > 0),
                Durum = o.CezaOdendi ? "Ödendi (Bakiyeden)" : "Ödenmedi"
            }).ToList();

           
            dgvGecikenveCeza.DataSource = liste;
            
            if (dgvGecikenveCeza.Columns.Count > 0)
            {
                dgvGecikenveCeza.Columns["KullaniciID"].Visible = false;
                dgvGecikenveCeza.Columns["AdSoyad"].HeaderText = "Üye Adı";
                dgvGecikenveCeza.Columns["KitapAdi"].HeaderText = "Kitap Adı";
                dgvGecikenveCeza.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
                dgvGecikenveCeza.Columns["TeslimTarihi"].HeaderText = "Teslim Tarihi";
                dgvGecikenveCeza.Columns["CezaOdemeTarihi"].HeaderText = "Ceza Ödeme Tarihi";
                dgvGecikenveCeza.Columns["GecikmeGun"].HeaderText = "Gecikme (Gün)";
                dgvGecikenveCeza.Columns["CezaTutari"].HeaderText = "Ceza Tutarı";
                dgvGecikenveCeza.Columns["ToplamGecikmeSayisi"].HeaderText = "Toplam Gecikme Sayısı";
                dgvGecikenveCeza.Columns["Durum"].HeaderText = "Ceza Durumu";

                decimal toplamCeza = liste.Sum(x => x.CezaTutari ?? 0);
                lblToplam.Text = "Toplam Ceza: " + toplamCeza.ToString("C2");
            }
        }
        private void cmbUyeFiltrele_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUyeFiltrele.SelectedValue == null)
                return;

            int secilenKullaniciId;

            if (!int.TryParse(cmbUyeFiltrele.SelectedValue.ToString(), out secilenKullaniciId))
                secilenKullaniciId = 0;

            if (secilenKullaniciId == 0)
                ListeleGecikenler();  
            else
                ListeleGecikenler(secilenKullaniciId);
        }
        private void dgvGecikenveCeza_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvGecikenveCeza.ClearSelection();
        }
       
        //RAPORLAR PANEL
        private void btnRaporlar_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlRaporlar.Visible = true;
        }
        private void cmbRaporTuru_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRaporTuru.SelectedItem == null) return;

            string secim = cmbRaporTuru.SelectedItem.ToString();

            chartRapor.Series.Clear();
            chartRapor.Titles.Clear();
            chartRapor.Titles.Add(secim);

            switch (secim)
            {
                case "KATEGORİ BAZINDA KİTAP DAĞILIMI":
                    Grafik_KategoriDagilim();
                    break;
                case "YAYINEVİNE GÖRE KİTAP SAYISI":
                    Grafik_YayineviDagilim();
                    break;
                case "AYLARA GÖRE ÖDÜNÇ VERİLENLER":
                    Grafik_AylaraGoreOdunc();
                    break;
                case "EN ÇOK OKUNAN KİTAPLAR":
                    Grafik_EnCokOkunan();
                    break;
                case "EN ÇOK KİTAP ALAN ÜYELER":
                    Grafik_EnCokKitapAlanUyeler();
                    break;
                case "GECİKENLER, ZAMANINDA TESLİM ALINANLAR, İADE EDİLMEYENLER":
                    Grafik_GecikmeOrani();
                    break;
                case "KATEGORİYE GÖRE GECİKMELER":
                    Grafik_KategoriyeGoreGecikmeler();
                    break;
                case "YILLARA GÖRE ÜYE SAYISI":
                    Grafik_YillaraGoreUye();
                    break;
                case "GÜNLÜK VERİLEN KİTAPLAR":
                    Grafik_GunlukVerilenKitaplar();
                    break;
                case "CEZA GELİRLERİ":
                    Grafik_CezaGelirleri();
                    break;
            }
        }
        private void Grafik_KategoriDagilim()
        {
            try
            {
                var data = db.Kitaplar
                     .SelectMany(k => k.Kategoriler)
                     .GroupBy(cat => cat.KategoriAdi)
                     .Select(g => new { Kategori = g.Key, Adet = g.Count() })
                     .ToList();

                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("KATEGORİ BAZINDA KİTAP DAĞILIMI");

                var s = chartRapor.Series.Add("KATEGORİLER");
                s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                s.IsValueShownAsLabel = true;

                foreach (var item in data)
                {
                    s.Points.AddXY(item.Kategori, item.Adet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_YayineviDagilim()
        {
            try
            {
                var data = db.Kitaplar
                             .GroupBy(k => k.Yayinevleri.Yayinevi_Ad)
                             .Select(g => new { Yayinevi = g.Key, Adet = g.Count() })
                             .ToList();

                var s = chartRapor.Series.Add("KİTAPLAR");
                s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                s.IsValueShownAsLabel = true;

                foreach (var item in data)
                    s.Points.AddXY(item.Yayinevi, item.Adet);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_AylaraGoreOdunc()
        {
            try
            {
                var data = db.OduncIslemleri
                     .Where(h => h.VerilisTarihi.HasValue)
                     .GroupBy(h => new { Yil = h.VerilisTarihi.Value.Year, Ay = h.VerilisTarihi.Value.Month })
                     .Select(g => new { Yil = g.Key.Yil, Ay = g.Key.Ay, Adet = g.Count() })
                     .ToList();
                var dataa = data
               .Select(x => new { Ay = $"{x.Yil}-{x.Ay:00}", x.Adet })
               .OrderBy(x => x.Ay)
               .ToList();

                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("AYLARA GÖRE ÖDÜNÇ VERİLENLER");

                var s = chartRapor.Series.Add("ÖDÜNÇ");
                s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                s.BorderWidth = 2;
                s.IsValueShownAsLabel = true;

                foreach (var item in data)
                {
                    s.Points.AddXY(item.Ay, item.Adet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_EnCokOkunan()
        {
            try
            {
                var data = db.OduncIslemleri
                             .GroupBy(h => h.Kitaplar.KitapAdi)
                             .Select(g => new { Kitap = g.Key, Okunma = g.Count() })
                             .OrderByDescending(x => x.Okunma)
                             .Take(10)
                             .ToList();

                var s = chartRapor.Series.Add("OKUNMA SAYISI");
                s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
                s.IsValueShownAsLabel = true;


                foreach (var item in data)
                    s.Points.AddXY(item.Kitap, item.Okunma);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_EnCokKitapAlanUyeler()
        {
            try
            {
                var data = db.OduncIslemleri
        .Where(o => o.Kullanicilar != null &&
                    o.Kullanicilar.Roller.Any(r => r.RolAdi == "Üye"))
        .GroupBy(o => o.Kullanicilar.K_Ad + " " + o.Kullanicilar.Soyad)
        .Select(g => new { Uye = g.Key, Adet = g.Count() })
        .OrderByDescending(x => x.Adet)
        .Take(10)
        .ToList();

                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("EN ÇOK KİTAP ALAN ÜYELER");

                var s = chartRapor.Series.Add("ALDIĞI KİTAP SAYISI");
                s.ChartType = SeriesChartType.Bar;
                s.IsValueShownAsLabel = true;

                foreach (var item in data)
                {
                    s.Points.AddXY(item.Uye, item.Adet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_GecikmeOrani()
        {
            try
            {
                var data = db.OduncIslemleri
                    .Select(o => new
                    {
                        Durum = !o.IadeTarihi.HasValue
                            ? "İade Edilmedi"
                            : (o.IadeTarihi > o.TeslimTarihi ? "Geciken" : "Zamanında")
                    })
                    .GroupBy(x => x.Durum)
                    .Select(g => new { Durum = g.Key, Adet = g.Count() })
                    .ToList();

                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("TESLİM DURUMLARI");

                var s = chartRapor.Series.Add("TESLİMLER");
                s.ChartType = SeriesChartType.Pie;
                s.IsValueShownAsLabel = true;


                foreach (var item in data)
                {
                    s.Points.AddXY(item.Durum, item.Adet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_KategoriyeGoreGecikmeler()
        {
            try
            {
                var gecikenKitapIdler = db.OduncIslemleri
                    .Where(o => o.IadeTarihi.HasValue && o.IadeTarihi > o.TeslimTarihi)
                    .Select(o => o.KitapID)
                    .ToList();

                var data = db.Kitaplar
                    .Where(k => gecikenKitapIdler.Contains(k.KitapID))
                    .SelectMany(k => k.Kategoriler)
                    .GroupBy(cat => cat.KategoriAdi)
                    .Select(g => new { Kategori = g.Key, Adet = g.Count() })
                    .OrderByDescending(x => x.Adet)
                    .ToList();

                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("KATEGORİYE GÖRE GECİKMELER");

                var s = chartRapor.Series.Add("GECİKMELER");
                s.ChartType = SeriesChartType.Column;
                s.IsValueShownAsLabel = true;


                foreach (var item in data)
                {
                    s.Points.AddXY(item.Kategori, item.Adet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_YillaraGoreUye()
        {
            try
            {
                var data = db.UyeDetay
                    .Where(u => u.KayitTarihi.HasValue)
                    .GroupBy(u => u.KayitTarihi.Value.Year)
                    .Select(g => new { Yil = g.Key, Adet = g.Count() })
                    .OrderBy(x => x.Yil)
                    .ToList();

                if (data.Count == 0)
                {
                    MessageBox.Show("Görüntülenecek veri bulunamadı!");
                    return;
                }
                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("YILLARA GÖRE ÜYE SAYISI");

                var s = chartRapor.Series.Add("ÜYELER");
                s.ChartType = SeriesChartType.Area;
                s.IsValueShownAsLabel = true;


                foreach (var item in data)
                {
                    s.Points.AddXY(item.Yil, item.Adet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_GunlukVerilenKitaplar()
        {
            try
            {
                var data = db.OduncIslemleri
                .Where(o => o.VerilisTarihi.HasValue)
                .GroupBy(o => new
                {
                    Yıl = o.VerilisTarihi.Value.Year,
                    Ay = o.VerilisTarihi.Value.Month,
                    Gun = o.VerilisTarihi.Value.Day
                })
                .Select(g => new
                {
                    Year = g.Key.Yıl,
                    Month = g.Key.Ay,
                    Day = g.Key.Gun,
                    Adet = g.Count()
                })
               .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
               .ToList();

                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("GÜNLÜK VERİLEN KİTAPLAR");

                var s = chartRapor.Series.Add("KİTAPLAR");
                s.ChartType = SeriesChartType.Line;
                s.IsValueShownAsLabel = true;

                foreach (var item in data)
                {
                    var tarih = new DateTime(item.Year, item.Month, item.Day);
                    s.Points.AddXY(tarih.ToString("yyyy-MM-dd"), item.Adet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void Grafik_CezaGelirleri()
        {
            try
            {
                var rawData = db.OduncIslemleri
            .Where(o => o.IadeTarihi.HasValue && o.CezaTutari > 0)
            .Select(o => new { IadeTarihi = o.IadeTarihi.Value, o.CezaTutari })
            .ToList();

                var data = rawData
                    .GroupBy(o => o.IadeTarihi.Date)
                    .Select(g => new { Tarih = g.Key, Toplam = g.Sum(x => x.CezaTutari) })
                    .OrderBy(x => x.Tarih)
                    .ToList();

                chartRapor.Series.Clear();
                chartRapor.Titles.Clear();
                chartRapor.Titles.Add("CEZA GELİRLERİ");
                var s = chartRapor.Series.Add("CEZA");
                s.ChartType = SeriesChartType.Column;
                s.IsValueShownAsLabel = true;

                foreach (var item in data)
                {
                    s.Points.AddXY(item.Tarih.ToShortDateString(), item.Toplam);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }

        }
        private void KutuphaneciIslemleriniGetir()
        {
            try
            {
                string secim = cmbIslemTuru.SelectedItem?.ToString();
                var islem = db.Raporlar.AsQueryable();

                switch (secim)
                {
                    case "Kitap İşlemleri":
                        islem = islem.Where(r =>
                            r.IslemTipi == "Kitap Ekle" ||
                            r.IslemTipi == "Kitap Silme" ||
                            r.IslemTipi == "Kitap Güncelleme");
                        break;

                    case "Üye İşlemleri":
                        islem = islem.Where(r =>
                            r.IslemTipi == "Üye Pasif" ||
                            r.IslemTipi == "Üye Aktif");
                        break;
                    case "Talep İşlemleri":
                        islem = islem.Where(r =>
                            r.IslemTipi == "Talep Onaylandı" ||
                            r.IslemTipi == "Talep Reddedildi");
                        break;
                    case "İade İşlemleri":
                        islem = islem.Where(r =>
                            r.IslemTipi == "İade İşlemleri");
                        break;
                    case "Tüm İşlemler":
                    default:
                        break;
                }
                var islemler = islem
                    .Where(r => r.Kullanicilar.Roller.Any(role => role.RolAdi == "Kütüphaneci"))
                    .OrderByDescending(r => r.IslemTarihi)
                    .Select(r => new
                    {
                        r.IslemTipi,
                        r.IslemAciklamasi,
                        UyeAdi = r.KullaniciID != null
                        ? db.Kullanicilar
                        .Where(u => u.KullaniciID == r.IlgiliUyeID && u.Roller.Any(role => role.RolAdi == "Üye"))
                        .Select(u => u.K_Ad + " " + u.Soyad)
                        .FirstOrDefault()
                         : null,
                        IslemiYapan = r.Kullanicilar.K_Ad + " " + r.Kullanicilar.Soyad,
                        r.IslemTarihi
                    })
                    .ToList();

                dgvKutuphaneciIslemleri.DataSource = islemler;

                if (dgvKutuphaneciIslemleri.Columns.Count > 0)
                {
                    dgvKutuphaneciIslemleri.Columns["IslemTipi"].HeaderText = "İşlem";
                    dgvKutuphaneciIslemleri.Columns["IslemAciklamasi"].HeaderText = "Açıklama";
                    dgvKutuphaneciIslemleri.Columns["UyeAdi"].HeaderText = "Üye";
                    dgvKutuphaneciIslemleri.Columns["IslemiYapan"].HeaderText = "Kütüphaneci";
                    dgvKutuphaneciIslemleri.Columns["IslemTarihi"].HeaderText = "Tarih";

                    if (secim == "Kitap İşlemleri")
                        dgvKutuphaneciIslemleri.Columns["UyeAdi"].Visible = false;
                    else
                        dgvKutuphaneciIslemleri.Columns["UyeAdi"].Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşlemler getirilirken bir hata oluştu: {ex.Message}",
                               "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dgvKutuphaneciIslemleri_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvKutuphaneciIslemleri.ClearSelection();
        }
        private void cmbIslemTuru_SelectedIndexChanged(object sender, EventArgs e)
        {
            KutuphaneciIslemleriniGetir();
        }

        //TEMA CLASS
        private void pictureTema_Click(object sender, EventArgs e)
        {
            TemaDegistir(pictureTema);
        }

       
    }
}