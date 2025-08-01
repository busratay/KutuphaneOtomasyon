using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KutuphaneOtomasyon
{
    public partial class UyePanel : TemaUygulama
    {
        private Kullanicilar aktifUye;
        KutuphaneEntities db = new KutuphaneEntities();

        
        public UyePanel(Kullanicilar kullanici)
        {
            InitializeComponent();
            aktifUye = kullanici;
            
            BilgileriYukle();
            GizlePanel();
            pnlAcilis.Visible = true;
            KitaplariFiltrele();
            UyeTalepGecmisiniYukle();
            KategorileriYukle();
            YazarlarAutoCompleteHazirla();
            ListeleAktifEmanetler();
            GecmisEmanetleriListele();
            BakiyeDurumunuGuncelle();
        }

        private void UyePanel_Load(object sender, EventArgs e)
        {

            if (aktifUye == null)
            {
                MessageBox.Show("Aktif kullanıcı bilgisi bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            MessageBox.Show($"Hoş geldiniz {aktifUye.K_Ad}!", "Giriş Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
           
            txtEskiSifre.UseSystemPasswordChar = true;
            txtYeniSifre.UseSystemPasswordChar = true;

            timer1.Interval = 60000;
            timer1.Start();
            
            timerYenile.Interval = 60000;
            timerYenile.Start();


        }
        private void GizlePanel()
        {
            pnlAcilis.Visible = false;
            pnlProfil.Visible = false;
            pnlOduncTalep.Visible = false;
            pnlAktifEmanetlerim.Visible = false;
            pnlEmanetGecmisi.Visible = false;
            pnlBakiyem.Visible = false;
        }
        private void btn_Cikis_Click(object sender, EventArgs e)
        {
            Giris login = new Giris();
            login.Show();
            this.Hide();
        }
        
        //PROFİLİM PANEL
        private void btnProfil_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlProfil.Visible = true;
        }
        private void btnGuncelBilgi_Click(object sender, EventArgs e)
        {
            string yeniEmail = txtEmail.Text.Trim();
            string eskiSifre = txtEskiSifre.Text.Trim();
            string yeniSifre = txtYeniSifre.Text.Trim();
            string yeniAdres = txtAdres.Text.Trim();
            string yeniTelefon = txtTelefon.Text.Trim();

            var kullaniciDb = db.Kullanicilar.Find(aktifUye.KullaniciID);
            var uyeDetay = db.UyeDetay.FirstOrDefault(u => u.KullaniciID == aktifUye.KullaniciID);

            if (uyeDetay == null)
            {
                uyeDetay = new UyeDetay { KullaniciID = aktifUye.KullaniciID };
                db.UyeDetay.Add(uyeDetay);
            }

            bool guncellemeVar = false;

            // Email güncelleme
            if (!string.IsNullOrEmpty(yeniEmail) && yeniEmail != kullaniciDb.Email)
            {
                if (db.Kullanicilar.Any(k => k.Email == yeniEmail && k.KullaniciID != aktifUye.KullaniciID))
                {
                    MessageBox.Show("Bu e-posta başka bir kullanıcı tarafından kullanılıyor.");
                    return;
                }
                kullaniciDb.Email = yeniEmail;
                aktifUye.Email = yeniEmail;
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
                aktifUye.Sifre = yeniSifre;
                guncellemeVar = true;
            }

            // Adres güncelleme
            if (!string.IsNullOrEmpty(yeniAdres) && yeniAdres != uyeDetay.Adres)
            {
                if (yeniAdres.Length < 5 || yeniAdres.Length > 250)
                {
                    MessageBox.Show("Adres 5 ile 250 karakter arasında olmalıdır.");
                    return;
                }
                uyeDetay.Adres = yeniAdres;
                guncellemeVar = true;
            }

            // Telefon güncelleme
            if (!string.IsNullOrEmpty(yeniTelefon) && yeniTelefon != kullaniciDb.Telefon)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(yeniTelefon, @"^\d{10,15}$"))
                {
                    MessageBox.Show("Telefon numarası yalnızca rakamlardan oluşmalı ve 10 ile 15 hane arasında olmalıdır.");
                    return;
                }
                kullaniciDb.Telefon = yeniTelefon;
                aktifUye.Telefon = yeniTelefon;
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
        private void BilgileriYukle()
        {
            lblAdSoyad.Text = $"{aktifUye.K_Ad} {aktifUye.Soyad}";
            txtEmail.Text = aktifUye.Email;
            txtTelefon.Text = aktifUye.Telefon;

            var uyeDetay = db.UyeDetay.FirstOrDefault(u => u.KullaniciID == aktifUye.KullaniciID);
            txtAdres.Text = uyeDetay?.Adres ?? string.Empty;
            if (uyeDetay.KayitTarihi.HasValue)
            {
                lblKayitTarihi.Text = uyeDetay.KayitTarihi.Value.ToString("dd.MM.yyyy");
            }
            else
            {
                lblKayitTarihi.Text = "Kayıt tarihi yok";
            }
        }
        private void chkSifreGoster_CheckedChanged(object sender, EventArgs e)
        {
            bool sifreyiGoster = chkSifreGoster.Checked;
            txtEskiSifre.UseSystemPasswordChar = !sifreyiGoster;
            txtYeniSifre.UseSystemPasswordChar = !sifreyiGoster;
        }
        //BAKİYEM PANEL
        private void btnBakiyem_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlBakiyem.Visible = true;
        }
        private void BakiyeDurumunuGuncelle()
        {
            var uyeDetay = db.UyeDetay.FirstOrDefault(u => u.KullaniciID == aktifUye.KullaniciID);
            if (uyeDetay == null)
            {
                MessageBox.Show("Üye detayları bulunamadı.");
                return;
            }

            txtMevcutBakiye.Text = (uyeDetay.Bakiye.HasValue ? uyeDetay.Bakiye.Value.ToString("0.00") : "0.00") + " ₺";
            lblBakiyeDegeri.Text = (uyeDetay.Bakiye.HasValue ? uyeDetay.Bakiye.Value.ToString("0.00") : "0.00") + " ₺";

            if (uyeDetay.Bakiye < 20)
            {
                lblBakiyeUyarisi.Visible = true;
                btnBakiye.Visible = true;
            }
            else
            {
                lblBakiyeUyarisi.Visible = false;
                btnBakiye.Visible = false;
            }
        }
        private void btnBakiyeYukle_Click(object sender, EventArgs e)
        {
            const decimal minimumYukleme = 10.0m;

            if (!decimal.TryParse(txtYuklenecek.Text, out decimal yuklenecekTutar))
            {
                MessageBox.Show("Lütfen geçerli bir sayı giriniz.");
                return;
            }

            if (yuklenecekTutar < minimumYukleme)
            {
                MessageBox.Show($"Minimum yükleme tutarı {minimumYukleme:0.00} ₺ olmalıdır.");
                return;
            }

            var uyeDetay = db.UyeDetay.FirstOrDefault(u => u.KullaniciID == aktifUye.KullaniciID);
            if (uyeDetay == null)
            {
                MessageBox.Show("Üye detayları bulunamadı.");
                return;
            }

            uyeDetay.Bakiye += yuklenecekTutar;
            db.SaveChanges();

            MessageBox.Show($"Bakiye başarıyla yüklendi. Yeni bakiyeniz: {uyeDetay.Bakiye:0.00} ₺");
            txtYuklenecek.Clear();
            BakiyeDurumunuGuncelle();
        }
        private void btnBakiye_Click(object sender, EventArgs e)
        {
            pnlProfil.Visible = false;
            pnlBakiyem.Visible = true;
        }
        //KİTAP TALEP ET PANEL
        private void btnKitapAl_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlOduncTalep.Visible = true;
        }
        private void KitaplariFiltrele(int? kategoriId = null, string kitapAdi = "", string yazarAdi = "")
        {
            var kitaplar = db.Kitaplar
       .Include("Yayinevleri")
       .Include("Yazarlar")
       .Include("Kategoriler")
       .AsQueryable();


            if (kategoriId.HasValue && kategoriId.Value != 0)
            {
                kitaplar = kitaplar.Where(k => k.Kategoriler.Any(kat => kat.KategoriID == kategoriId.Value));
            }
            if (!string.IsNullOrWhiteSpace(kitapAdi))
            {
                kitaplar = kitaplar.Where(k => k.KitapAdi.Contains(kitapAdi));
            }
            if (!string.IsNullOrWhiteSpace(yazarAdi))
            {
                kitaplar = kitaplar.Where(k => k.Yazarlar.Any(y => (y.Yazar_Ad + " " + y.Soyad).Contains(yazarAdi)));
            }

            var sonuc = kitaplar
                .ToList()
                .Select(k => new
                {
                    k.KitapID,
                    k.KitapAdi,
                    k.ISBN,
                    k.BasimYili,
                    k.Stok,
                    StokDurumu = k.Stok > 0 ? "Mevcut" : "Stokta Yok",
                    k.SayfaSayisi,
                    Yayinevi = k.Yayinevleri != null ? k.Yayinevleri.Yayinevi_Ad : "",
                    Yazarlar = string.Join(", ", k.Yazarlar.Select(y => y.Yazar_Ad + " " + y.Soyad)),
                    Kategoriler = string.Join(", ", k.Kategoriler.Select(c => c.KategoriAdi))
                })
                .ToList();

            dgvTalepEt.DataSource = sonuc;
            dgvTalepEt.Columns["KitapID"].Visible = false;

            dgvTalepEt.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvTalepEt.Columns["ISBN"].HeaderText = "ISBN";
            dgvTalepEt.Columns["BasimYili"].HeaderText = "Basım Yılı";
            dgvTalepEt.Columns["Stok"].HeaderText = "Stok";
            dgvTalepEt.Columns["StokDurumu"].HeaderText = "Stok Durumu";
            dgvTalepEt.Columns["SayfaSayisi"].HeaderText = "Sayfa Sayısı";
            dgvTalepEt.Columns["Yayinevi"].HeaderText = "Yayınevi";
            dgvTalepEt.Columns["Yazarlar"].HeaderText = "Yazar";
            dgvTalepEt.Columns["Kategoriler"].HeaderText = "Kategori";


        }
        private void UyeTalepGecmisiniYukle()
        {
            var talepler = db.OduncTalepleri
        .Where(t => t.UyeID == aktifUye.KullaniciID);

            
            if (chkReddedildi.Checked)
            {
                talepler = talepler.Where(t => t.TalepDurumu == "Reddedildi");
            }

            var sonuc = talepler
                .OrderByDescending(t => t.TalepTarihi)
                .Select(t => new
                {
                    t.TalepID,
                    t.Kitaplar.KitapAdi,
                    t.TalepTarihi,
                    t.TalepDurumu,
                    t.OnayTarihi,
                    t.Aciklama
                })
                .ToList();

            dgvTalepGecmisi.DataSource = sonuc;
            dgvTalepGecmisi.Columns["TalepID"].Visible = false;
            dgvTalepGecmisi.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvTalepGecmisi.Columns["TalepTarihi"].HeaderText = "Talep Tarihi";
            dgvTalepGecmisi.Columns["TalepDurumu"].HeaderText = "Talep Durumu";
            dgvTalepGecmisi.Columns["OnayTarihi"].HeaderText = "Onay Tarihi";
            dgvTalepGecmisi.Columns["Aciklama"].HeaderText = "Açıklama";
        }
        private void chkReddedildi_CheckedChanged(object sender, EventArgs e)
        {
            UyeTalepGecmisiniYukle();
        }
        private void KategorileriYukle()
        {
            var kategoriler = db.Kategoriler
                .Select(k => new
                {
                    k.KategoriID,
                    k.KategoriAdi
                })
                .ToList();


            kategoriler.Insert(0, new { KategoriID = 0, KategoriAdi = "Tüm Kategoriler" });

            cmbKategori.DataSource = kategoriler;
            cmbKategori.DisplayMember = "KategoriAdi";
            cmbKategori.ValueMember = "KategoriID";
        }
        private void YazarlarAutoCompleteHazirla()
        {
            var yazarlarListesi = db.Yazarlar
                .Select(y => y.Yazar_Ad + " " + y.Soyad)
                .ToList();

            var autoComplete = new AutoCompleteStringCollection();
            autoComplete.AddRange(yazarlarListesi.ToArray());
            txtYazarAra.AutoCompleteCustomSource = autoComplete;
        }
        private void cmbKategori_SelectedIndexChanged(object sender, EventArgs e)
        {
            int kategoriId = (int)cmbKategori.SelectedValue;
            string kitapAdiAra = txtKitapAra.Text.Trim();
            string yazarAdiAra = txtYazarAra.Text.Trim();
            KitaplariFiltrele(kategoriId, kitapAdiAra, yazarAdiAra);
        }
        private void txtKitapAra_TextChanged(object sender, EventArgs e)
        {
            int kategoriId = (int)cmbKategori.SelectedValue;
            string kitapAdiAra = txtKitapAra.Text.Trim();
            string yazarAdiAra = txtYazarAra.Text.Trim();
            KitaplariFiltrele(kategoriId, kitapAdiAra, yazarAdiAra);
        }
        private void txtYazarAra_TextChanged(object sender, EventArgs e)
        {
            int kategoriId = (int)cmbKategori.SelectedValue;
            string kitapAdiAra = txtKitapAra.Text.Trim();
            string yazarAdiAra = txtYazarAra.Text.Trim();
            KitaplariFiltrele(kategoriId, kitapAdiAra, yazarAdiAra);
        }
        private void btnTalepEt_Click(object sender, EventArgs e)
        {
            var secilenKitaplar = dgvTalepEt.SelectedRows;

            if (secilenKitaplar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir kitap seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int aktifEmanetSayisi = db.OduncIslemleri
                .Count(x => x.KullaniciID == aktifUye.KullaniciID && x.IadeTarihi == null);

            int bekleyenTalepSayisi = db.OduncTalepleri
                .Count(t => t.UyeID == aktifUye.KullaniciID && t.TalepDurumu == "Beklemede");

            int yeniTalepSayisi = secilenKitaplar.Count;
            int toplamKitapSayisi = aktifEmanetSayisi + bekleyenTalepSayisi + yeniTalepSayisi;

            if (toplamKitapSayisi > 3)
            {
                MessageBox.Show($"En fazla 3 kitap talep edebilirsiniz. Şu anda {aktifEmanetSayisi} emanet, {bekleyenTalepSayisi} bekleyen talebiniz var.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (db.OduncIslemleri.Any(x => x.KullaniciID == aktifUye.KullaniciID && x.IadeTarihi == null))
            {
                MessageBox.Show("Üzerinizde iade edilmemiş kitap(lar) var. Bu durum taleplerinizin reddedilmesine neden olabilir.",
                    "Bilgilendirme", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            bool cezaVarMi = db.OduncIslemleri.Any(o =>
                o.KullaniciID == aktifUye.KullaniciID &&
                o.CezaTutari > 0 &&
                o.CezaOdendi == false);

            if (cezaVarMi)
            {
                MessageBox.Show("Ödenmemiş cezanız olduğu için yeni talep oluşturamazsınız.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (DataGridViewRow row in secilenKitaplar)
            {
                int kitapId = (int)row.Cells["KitapID"].Value;
                var kitap = db.Kitaplar.FirstOrDefault(k => k.KitapID == kitapId);

                if (kitap == null || kitap.Stok <= 0)
                {
                    MessageBox.Show($"'{kitap?.KitapAdi ?? "Bilinmeyen Kitap"}' stokta olmadığı için talep oluşturulamaz.",
                        "Stok Yetersiz", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; 
                }
            }
           
            List<string> mevcutTaleplerdenKitaplar = new List<string>();
            List<string> basariliTalepler = new List<string>();

            foreach (DataGridViewRow row in secilenKitaplar)
            {
                int kitapId = (int)row.Cells["KitapID"].Value;

                bool talepVarMi = db.OduncTalepleri.Any(t =>
                    t.UyeID == aktifUye.KullaniciID &&
                    t.KitapID == kitapId &&
                    t.TalepDurumu == "Beklemede");

                if (talepVarMi)
                {
                    var kitapAdi = db.Kitaplar.First(k => k.KitapID == kitapId).KitapAdi;
                    mevcutTaleplerdenKitaplar.Add(kitapAdi);
                    continue;
                }

                bool emanetteVarMi = db.OduncIslemleri.Any(o =>
                    o.KullaniciID == aktifUye.KullaniciID &&
                    o.KitapID == kitapId &&
                    o.IadeTarihi == null);

                if (emanetteVarMi)
                {
                    var kitapAdi = db.Kitaplar.First(k => k.KitapID == kitapId).KitapAdi;
                    mevcutTaleplerdenKitaplar.Add(kitapAdi + " (Zaten emanette)");
                    continue;
                }

                var yeniTalep = new OduncTalepleri
                {
                    UyeID = aktifUye.KullaniciID,
                    KitapID = kitapId,
                    TalepTarihi = DateTime.Now,
                    TalepDurumu = "Beklemede",
                    Aciklama = "Talep edildi"
                };

                db.OduncTalepleri.Add(yeniTalep);

                var rapor = new Raporlar
                {
                    KullaniciID = aktifUye.KullaniciID,
                    IlgiliKitapID = kitapId,
                    IslemTipi = "Talep Edildi",
                    IslemAciklamasi = $"Üye {aktifUye.K_Ad} {aktifUye.Soyad} kitap talep etti.",
                    IslemTarihi = DateTime.Now
                };
                db.Raporlar.Add(rapor);
                basariliTalepler.Add(db.Kitaplar.First(k => k.KitapID == kitapId).KitapAdi);

                try
                {
                    db.SaveChanges();
                    StringBuilder mesaj = new StringBuilder();

                    if (basariliTalepler.Any())
                    {
                        mesaj.AppendLine($"Başarılı talepler ({basariliTalepler.Count}):");
                        mesaj.AppendLine(string.Join("\n", basariliTalepler));
                    }

                    if (mevcutTaleplerdenKitaplar.Any())
                    {
                        mesaj.AppendLine($"\nZaten talep edilmiş/emanette olan kitaplar ({mevcutTaleplerdenKitaplar.Count}):");
                        mesaj.AppendLine(string.Join("\n", mevcutTaleplerdenKitaplar));
                    }

                    if (basariliTalepler.Any())
                    {
                        MessageBox.Show(mesaj.ToString(), "Talep Sonuçları", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(mesaj.ToString(), "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Talep oluşturulurken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                UyeTalepGecmisiniYukle();
                ListeleAktifEmanetler();
            }
        }
        private void btnTalepIptalEt_Click(object sender, EventArgs e)
        {
            if (dgvTalepGecmisi.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen iptal etmek istediğiniz talebi seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (DataGridViewRow row in dgvTalepGecmisi.SelectedRows)
            {
                int talepId = (int)row.Cells["TalepID"].Value;

                var talep = db.OduncTalepleri.FirstOrDefault(t => t.TalepID == talepId && t.TalepDurumu == "Beklemede");

                if (talep != null)
                {
                    db.OduncTalepleri.Remove(talep);
                    var rapor = new Raporlar
                    {
                        KullaniciID = aktifUye.KullaniciID,
                        IlgiliKitapID = talep.KitapID,
                        IslemTipi = "Talep İptal Edildi",
                        IslemAciklamasi = $"Üye {aktifUye.K_Ad} {aktifUye.Soyad} talebini iptal etti.",
                        IslemTarihi = DateTime.Now
                    };
                    db.Raporlar.Add(rapor);
                }
            }
            db.SaveChanges();

            MessageBox.Show("Seçilen talepler iptal edildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

            UyeTalepGecmisiniYukle();
        }
        private void dgvTalepEt_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvTalepEt.ClearSelection();
        }
        private void dgvTalepGecmisi_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            var durumCell = grid.Rows[e.RowIndex].Cells["TalepDurumu"];
            if (durumCell?.Value == null) return;

            string durum = durumCell.Value.ToString();

            switch (durum)
            {
                case "Beklemede":
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 228, 205);
                    break;
                case "Onaylandı":
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(238, 190, 150);
                    break;
                case "Reddedildi":
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(222, 150, 130);
                    break;
                default:
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(242, 232, 223);
                    break;
            }
            grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
        }
        private void dgvTalepGecmisi_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvTalepGecmisi.ClearSelection();
        }
        //AKTİF KİTAPLARIM PANEL
        private void btnAktifKitaplar_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlAktifEmanetlerim.Visible = true;
        }
        private void ListeleAktifEmanetler()
        {
            var oduncVerileri = (from odunc in db.OduncIslemleri
                                 join kitap in db.Kitaplar on odunc.KitapID equals kitap.KitapID
                                 where odunc.KullaniciID == aktifUye.KullaniciID && odunc.IadeTarihi == null
                                 select new
                                 {
                                     odunc,
                                     kitap,
                                     yazarlar = kitap.Yazarlar
                                 }).ToList();

            var emanetler = oduncVerileri.Select(x =>
            {
               
                int kalanGun = 0;
                if (x.odunc.TeslimTarihi.HasValue)
                {
                    kalanGun = (x.odunc.TeslimTarihi.Value.Date - DateTime.Now.Date).Days;
                }

                
                decimal cezaTutari = 0;
                if (kalanGun < 0)
                {
                    int gecikmeGun = -kalanGun;
                    decimal gunlukCeza = 2.5m;
                    cezaTutari = gecikmeGun * gunlukCeza;
                }

          
                return new
                {
                    x.odunc.IslemID,
                    x.kitap.KitapAdi,
                    Yazarlar = string.Join(", ", x.yazarlar.Select(y => y.Yazar_Ad + " " + y.Soyad)),
                    x.odunc.VerilisTarihi,
                    x.odunc.TeslimTarihi,
                    KalanGun = kalanGun,
                    Ceza = cezaTutari,
                    Uyari = kalanGun < 0 ? "Teslim Süresi Aşıldı!" :
                            kalanGun == 0 ? "Teslim Günü Bugün" :
                            kalanGun <= 2 ? "Teslim Süresi Yaklaşıyor" : "",
                    x.odunc.UzatmaTalepEdildi,
                    x.odunc.UzatmaReddedildi,
                    x.odunc.UzatmaReddetmeNedeni,
                };
            }).OrderBy(e => e.TeslimTarihi).ToList();

            dgvAktifEmanetler.DataSource = emanetler;

            dgvAktifEmanetler.Columns["IslemID"].Visible = false;
            dgvAktifEmanetler.Columns["UzatmaTalepEdildi"].Visible = false;
            dgvAktifEmanetler.Columns["UzatmaReddedildi"].Visible = false;
            dgvAktifEmanetler.Columns["UzatmaReddetmeNedeni"].Visible = false;
            
            dgvAktifEmanetler.Columns["Ceza"].Visible = false;
            bool cezaVarMi = emanetler.Any(e => e.Ceza > 0);

            if (cezaVarMi)
            {
                dgvAktifEmanetler.Columns["Ceza"].Visible = true;
            }

            dgvAktifEmanetler.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvAktifEmanetler.Columns["Yazarlar"].HeaderText = "Yazarlar";
            dgvAktifEmanetler.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
            dgvAktifEmanetler.Columns["TeslimTarihi"].HeaderText = "Teslim Tarihi";
            dgvAktifEmanetler.Columns["KalanGun"].HeaderText = "Kalan Gün";
            dgvAktifEmanetler.Columns["Ceza"].HeaderText = "Ceza (TL)";
            dgvAktifEmanetler.Columns["Uyari"].HeaderText = "Uyarı";


        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            ListeleAktifEmanetler();
        }
        private void dgvAktifEmanetler_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvAktifEmanetler.ClearSelection();
        }
        private void dgvAktifEmanetler_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            var uyariCell = grid.Rows[e.RowIndex].Cells["Uyari"];
            if (uyariCell?.Value == null) return;

            string durum = uyariCell.Value.ToString();

            switch (durum)
            {
                case "Teslim Süresi Aşıldı!":
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                    break;
                case "Teslim Günü Bugün":
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
                    break;
                case "Teslim Süresi Yaklaşıyor":
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Moccasin;
                    break;
                default:
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    break;
            }
        }

        private void btnUzat_Click(object sender, EventArgs e)
        {
            if (dgvAktifEmanetler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen uzatmak istediğiniz kitabı seçiniz.");
                return;
            }

            int secilenIslemID = Convert.ToInt32(dgvAktifEmanetler.SelectedRows[0].Cells["IslemID"].Value);
            var oduncIslem = db.OduncIslemleri.Find(secilenIslemID);

            if (oduncIslem == null)
            {
                MessageBox.Show("Seçilen işlem bulunamadı.");
                return;
            }

            if (oduncIslem.UzatmaTalepEdildi)
            {
                MessageBox.Show("Bu kitap için zaten bir uzatma talebi yapılmıştır.");
                return;
            }
            string redNedeni = null;
            string mesaj = null;

            if (oduncIslem.CezaTutari > 0 && !oduncIslem.CezaOdendi)
            {
                redNedeni = "Ödenmemiş ceza bulunduğundan uzatma talebi reddedildi.";
                mesaj = "Ödenmemiş ceza olduğundan uzatma talebi reddedildi.";
            }
            else if (DateTime.Now.Date > oduncIslem.TeslimTarihi.Value.Date)
            {
                redNedeni = "Teslim süresi geçtiği için uzatma yapılamaz.";
                mesaj = "Teslim süresi aşıldığından uzatma talebi yapılamaz.";
            }
            else if (db.OduncIslemleri.Any(o => o.KullaniciID == aktifUye.KullaniciID && o.GecikmeGun > 0))
            {
                redNedeni = "Geçmişte gecikmeli iadeler bulunduğundan uzatma talebi reddedildi.";
                mesaj = "Geçmişte gecikmeli iadeleriniz bulunduğundan uzatma talebi yapılamaz.";
            }

            oduncIslem.UzatmaTalepEdildi = true;
            oduncIslem.UzatmaTarihi = DateTime.Now;

            if (redNedeni != null)
            {
                oduncIslem.UzatmaReddedildi = true;
                oduncIslem.UzatmaReddetmeNedeni = redNedeni;
                MessageBox.Show(mesaj);
            }
            else
            {
                const int uzatmaGunSayisi = 15;
                oduncIslem.TeslimTarihi = oduncIslem.TeslimTarihi.Value.AddDays(uzatmaGunSayisi);
                oduncIslem.UzatmaReddedildi = false;
                oduncIslem.UzatmaReddetmeNedeni = null;
                MessageBox.Show($"Uzatma talebiniz başarıyla onaylandı. Kitap teslim tarihi {uzatmaGunSayisi} gün uzatıldı.");
            }

            db.SaveChanges();
            ListeleAktifEmanetler();
        }
        private void chkDetayGoster_CheckedChanged(object sender, EventArgs e)
        {
            bool detayGoster = chkDetayGoster.Checked;

            dgvAktifEmanetler.Columns["UzatmaTalepEdildi"].Visible = detayGoster;
            dgvAktifEmanetler.Columns["UzatmaReddetmeNedeni"].Visible = detayGoster;

        }
        //EMANET GEÇMİŞİM PANEL
        private void btnEmanetGecmisi_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlEmanetGecmisi.Visible = true;
        }
        private void GecmisEmanetleriListele()
        {
          var sorgu = db.OduncIslemleri.Where(o => o.KullaniciID == aktifUye.KullaniciID && o.IadeTarihi != null);

            if (chkSadeceCeza.Checked)
                sorgu = sorgu.Where(o => o.IadeTarihi > o.TeslimTarihi);

            var liste = sorgu.Select(o => new
            {
                o.IslemID,
                o.Kitaplar.KitapAdi,
                o.VerilisTarihi,
                o.TeslimTarihi,
                o.IadeTarihi,
                GecikmeGun = o.IadeTarihi > o.TeslimTarihi ?
                    System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", o.TeslimTarihi, o.IadeTarihi) : 0,
                Ceza = o.IadeTarihi > o.TeslimTarihi ?
                    System.Data.Entity.SqlServer.SqlFunctions.DateDiff("day", o.TeslimTarihi, o.IadeTarihi) * 2.5m : 0,
                o.CezaOdendi
            });

            string kitapAra = txtUyeKitapAra.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(kitapAra))
                liste = liste.Where(e => e.KitapAdi.ToLower().Contains(kitapAra));

            var emanetler = liste.OrderByDescending(e => e.IadeTarihi).ToList();
            dgvEmanetGecmisim.DataSource = emanetler;

         
            dgvEmanetGecmisim.Columns["IslemID"].Visible = false;
            dgvEmanetGecmisim.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvEmanetGecmisim.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
            dgvEmanetGecmisim.Columns["TeslimTarihi"].HeaderText = "Teslim Tarihi";
            dgvEmanetGecmisim.Columns["IadeTarihi"].HeaderText = "İade Tarihi";
            dgvEmanetGecmisim.Columns["GecikmeGun"].HeaderText = "Gecikme (Gün)";
            dgvEmanetGecmisim.Columns["Ceza"].HeaderText = "Ceza (TL)";
            dgvEmanetGecmisim.Columns["CezaOdendi"].HeaderText = "Ceza Ödendi";

            lblToplamCeza.Text = $"TOPLAM ÖDENEN CEZA: {emanetler.Sum(x => x.Ceza ?? 0):0.00} ₺";
        }
        
        private void chkSadeceCeza_CheckedChanged(object sender, EventArgs e)
        {
            GecmisEmanetleriListele();
        }

        private void txtUyeKitapAra_TextChanged(object sender, EventArgs e)
        {
            GecmisEmanetleriListele();
        }

        private void timerYenile_Tick(object sender, EventArgs e)
        {
            GecmisEmanetleriListele();
        }
        private void dgvEmanetGecmisim_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            int gecikmeGun = Convert.ToInt32(dgvEmanetGecmisim.Rows[e.RowIndex].Cells["GecikmeGun"].Value);

            if (gecikmeGun >= 10)
            {
                dgvEmanetGecmisim.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(192, 110, 90);
            }
        }
        private void dgvEmanetGecmisim_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvEmanetGecmisim.ClearSelection();
        }

        //TEMA CLASS
        private void pictureTema_Click(object sender, EventArgs e)
        {
            TemaDegistir(pictureTema);
        }
       
    }
}
  
  
   
