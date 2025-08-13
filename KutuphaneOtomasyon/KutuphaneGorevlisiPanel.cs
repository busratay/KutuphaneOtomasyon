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
    public partial class KutuphaneGorevlisiPanel : TemaUygulama
    {
        private Kullanicilar aktifKutuphaneci;
        public bool IlkGiris;
        KutuphaneEntities db = new KutuphaneEntities();

        public KutuphaneGorevlisiPanel(Kullanicilar kullanici, bool ilkGiris)
        {
            InitializeComponent();
            aktifKutuphaneci = kullanici;
            IlkGiris = ilkGiris;
            GizlePanel();
            KitaplariListele();
            UyeleriListele();
            BilgileriYukle();
            UyeleriYukle();
            ListeleOduncKitaplar();
            AlanlariTemizle();
            IadeAlinanKitaplariListele();
            pnlAcilis.Visible = true;
            BekleyenTalepleriListele();
            GecmisTalepleriListele();
            FiltreleIadeler();
            VerilenKitaplariYukle();

            var detay = db.KutuphaneGorevliDetay
                    .FirstOrDefault(d => d.KullaniciID == aktifKutuphaneci.KullaniciID);

            if (detay == null)
            {
              
                detay = new KutuphaneGorevliDetay
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IlkGiris = false
                };
                db.KutuphaneGorevliDetay.Add(detay);
                db.SaveChanges();
            }

            
            if (!detay.IlkGiris)
            {
                MessageBox.Show("İlk girişte şifrenizi değiştirmeniz gerekiyor!", "İlk Giriş",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                pnlProfilim.Visible = true;

                
                detay.IlkGiris = true;
                db.SaveChanges();
            }
            else
            {
                MessageBox.Show($"Hoş geldiniz {aktifKutuphaneci.K_Ad}!", "Giriş Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void KutuphaneGorevlisiPanel_Load(object sender, EventArgs e)
        {
            if (aktifKutuphaneci == null)
            {
                MessageBox.Show("Aktif kullanıcı bilgisi bulunamadı!", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            txtEskiSifre.UseSystemPasswordChar = true;
            txtYeniSifre.UseSystemPasswordChar = true;

            cmbDurumFiltre.Items.Clear();
            cmbDurumFiltre.Items.Add("Tümü");
            cmbDurumFiltre.Items.Add("Onaylandı");
            cmbDurumFiltre.Items.Add("Reddedildi");
            cmbDurumFiltre.SelectedIndex = 0;



        }
        private void GizlePanel()
        {
            pnlAcilis.Visible = false;
            pnlProfilim.Visible = false;
            pnlKitapIslemleri.Visible = false;
            pnlUyeBilgileri.Visible = false;
            pnlKitapVer.Visible = false;
            pnlIade.Visible = false;
            pnlGecmisEmanetler.Visible = false;
            pnlTalepIslemleri.Visible = false;

        }
        private void btnCikis_Click(object sender, EventArgs e)
        {
            Giris login = new Giris();
            login.Show();
            this.Hide();
        }
        private void AlanlariTemizle()
        {
            txtIadeKitap.Clear();
            txtIadeUyeAdi.Clear();
            txtGecikmeGun.Clear();
            txtCezaTutari.Clear();
            dtpIadeTarihi.Value = DateTime.Today;
        }

        private void KitaplariTemizle()
        {
            txtKitapID.Clear();
            txtKitapAdi.Clear();
            txtIsbn.Clear();
            txtBasimYili.Clear();
            txtStok.Clear();
            txtSayfaSayisi.Clear();
            txtYayinevi.Clear();
            txtYazarAd.Clear();
            txtYazarSoyad.Clear();
            txtKategori.Clear();
        }
        private void KitaplariListele()
        {
            var kitaplar = db.Kitaplar
                .Include("Yayinevleri")
                .Include("Yazarlar")
                .Include("Kategoriler")
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

            dgvKitap.DataSource = kitaplar;
            dgvKitaplar.DataSource = kitaplar;
            dgvKitaplar.Columns["KitapID"].Visible = false; //KİTAP İŞLEMLERİ PANEL
            dgvKitaplar.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvKitaplar.Columns["ISBN"].HeaderText = "ISBN";
            dgvKitaplar.Columns["BasimYili"].HeaderText = "Basım Yılı";
            dgvKitaplar.Columns["StokDurumu"].HeaderText = "Stok";
            dgvKitaplar.Columns["SayfaSayisi"].HeaderText = "Sayfa Sayısı";
            dgvKitaplar.Columns["Yayinevi"].HeaderText = "Yayınevi";
            dgvKitaplar.Columns["Yazarlar"].HeaderText = "Yazar";
            dgvKitaplar.Columns["Kategoriler"].HeaderText = "Kategori";

            dgvKitap.Columns["KitapID"].Visible = false;//KİTAP ÖDÜNÇ VERME PANEL
            dgvKitap.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvKitap.Columns["ISBN"].HeaderText = "ISBN";
            dgvKitap.Columns["BasimYili"].HeaderText = "Basım Yılı";
            dgvKitap.Columns["StokDurumu"].HeaderText = "Stok";
            dgvKitap.Columns["SayfaSayisi"].HeaderText = "Sayfa Sayısı";
            dgvKitap.Columns["Yayinevi"].HeaderText = "Yayınevi";
            dgvKitap.Columns["Yazarlar"].HeaderText = "Yazar";
            dgvKitap.Columns["Kategoriler"].HeaderText = "Kategori";
        }
        private void UyeleriYukle()
        {
            var uyeler = db.Kullanicilar
                .Where(k => k.Roller.Any(r => r.RolID == 2))
                .Select(k => new
                {
                    k.KullaniciID,
                    AdSoyad = k.K_Ad + " " + k.Soyad
                })
                .ToList();

            uyeler.Insert(0, new { KullaniciID = 0, AdSoyad = "Üye seçiniz" });

            cmbUyeler.DataSource = uyeler;
            cmbUyeler.DisplayMember = "AdSoyad";
            cmbUyeler.ValueMember = "KullaniciID";
            cmbUyeler.SelectedIndex = 0; //KİTAP ÖDÜNÇ VERME PANEL

            cmbUyeSec.DataSource = uyeler;
            cmbUyeSec.DisplayMember = "AdSoyad";
            cmbUyeSec.ValueMember = "KullaniciID";
            cmbUyeSec.SelectedIndex = 0; //GEÇMİŞ EMANETLER PANEL


            cmbFiltreUye.DataSource = uyeler;
            cmbFiltreUye.DisplayMember = "AdSoyad";
            cmbFiltreUye.ValueMember = "KullaniciID";
            cmbFiltreUye.SelectedIndex = 0; //KİTAP TALEP PANEL 
        }
        //PROFİLİM PANEL
        private void btnProfil_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlProfilim.Visible = true;
        }
        private void BilgileriYukle()
        {
            lblAdSoyad.Text = $"{aktifKutuphaneci.K_Ad} {aktifKutuphaneci.Soyad}";
            txtEmail.Text = aktifKutuphaneci.Email;
            lblRol.Text = aktifKutuphaneci.Roller?.FirstOrDefault()?.RolAdi ?? "Bilinmiyor";
            txtKTelefon.Text = aktifKutuphaneci.Telefon;
        }
        private void btnGuncelBilgi_Click(object sender, EventArgs e)
        {
            string yeniEmail = txtEmail.Text.Trim();
            string eskiSifre = txtEskiSifre.Text.Trim();
            string yeniSifre = txtYeniSifre.Text.Trim();
            string yeniTelefon = txtKTelefon.Text.Trim();

            var kullaniciDb = db.Kullanicilar.Find(aktifKutuphaneci.KullaniciID);
            if (kullaniciDb == null)
            {
                MessageBox.Show("Kullanıcı bulunamadı.");
                return;
            }

            bool guncellemeVar = false;

            // Email güncelleme
            if (!string.IsNullOrEmpty(yeniEmail) && yeniEmail != kullaniciDb.Email)
            {
                if (db.Kullanicilar.Any(k => k.Email == yeniEmail && k.KullaniciID != aktifKutuphaneci.KullaniciID))
                {
                    MessageBox.Show("Bu e-posta başka bir kullanıcı tarafından kullanılıyor.");
                    return;
                }
                kullaniciDb.Email = yeniEmail;
                aktifKutuphaneci.Email = yeniEmail;
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
                aktifKutuphaneci.Sifre = yeniSifre;
                guncellemeVar = true;

                var detay = db.KutuphaneGorevliDetay
            .FirstOrDefault(d => d.KullaniciID == aktifKutuphaneci.KullaniciID);

                if (detay != null && !detay.IlkGiris)  
                {
                    detay.IlkGiris = true;
                    db.SaveChanges();
                }
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
                aktifKutuphaneci.Telefon = yeniTelefon;
                guncellemeVar = true;
            }

            // Güncelleme varsa tek bir kez işle
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
        //ÜYE BİLGİLERİ PANEL
        private void btnUyeBilgileri_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlUyeBilgileri.Visible = true;
        }
        private void dgvUyeler_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvUyeler.ClearSelection();
        }
        private void UyeleriListele()
        {
            string aranan = txtUyeAra.Text.Trim().ToLower();
            bool sadeceCezalilar = chkCezalilar.Checked;

            var uyeler = db.Kullanicilar
                .Where(k => k.Roller.Any(r => r.RolAdi == "Üye"))
                .Where(k => string.IsNullOrEmpty(aranan) ||
                           (k.K_Ad + " " + k.Soyad).ToLower().Contains(aranan))
                .Select(k => new
                {
                    k.KullaniciID,
                    AdSoyad = k.K_Ad + " " + k.Soyad,
                    k.Email,
                    k.Telefon,
                    Durum = k.Aktif ? "Aktif" : "Pasif",
                    UyeDetay = db.UyeDetay.FirstOrDefault(ud => ud.KullaniciID == k.KullaniciID),
                    ToplamCeza = db.OduncIslemleri
                        .Where(o => o.KullaniciID == k.KullaniciID && o.CezaTutari > 0)
                        .Sum(o => (decimal?)o.CezaTutari) ?? 0,
                    AktifEmanet = db.OduncIslemleri
                        .Count(o => o.KullaniciID == k.KullaniciID && o.IadeTarihi == null),
                    GecikenEmanet = db.OduncIslemleri
                        .Count(o => o.KullaniciID == k.KullaniciID &&
                                    o.TeslimTarihi < (o.IadeTarihi ?? DateTime.Now)),

                    AktifEmanetler = db.OduncIslemleri
                        .Where(o => o.KullaniciID == k.KullaniciID &&
                                   o.IadeTarihi == null &&
                                   o.TeslimTarihi != null)
                        .Select(o => o.TeslimTarihi.Value)
                        .ToList()
                })
                .AsEnumerable()
                .Select(u => new
                {
                    u.KullaniciID,
                    u.AdSoyad,
                    u.Email,
                    u.Telefon,
                    u.Durum,
                    Adres = u.UyeDetay?.Adres ?? "",
                    Bakiye = u.UyeDetay?.Bakiye ?? 0m,
                    u.ToplamCeza,
                    u.AktifEmanet,
                    u.GecikenEmanet,

                    AktifCeza = u.AktifEmanetler.Sum(teslimTarihi =>
                    {
                        int gecikmeGun = Math.Max(0, (DateTime.Today - teslimTarihi).Days);
                        return gecikmeGun * 2.5m;
                    })
                })
                .Where(u => !sadeceCezalilar || u.ToplamCeza > 0 || u.AktifCeza > 0)
                .ToList();

            dgvUyeler.DataSource = uyeler;

            dgvUyeler.Columns["KullaniciID"].Visible = false;
            dgvUyeler.Columns["Email"].Visible = false;
            dgvUyeler.Columns["Telefon"].Visible = false;
            dgvUyeler.Columns["Adres"].Visible = false;
            dgvUyeler.Columns["AdSoyad"].HeaderText = "Ad Soyad";
            dgvUyeler.Columns["Bakiye"].HeaderText = "Bakiye (₺)";
            dgvUyeler.Columns["ToplamCeza"].HeaderText = "Ödenen Ceza (₺)";
            dgvUyeler.Columns["AktifEmanet"].HeaderText = "Aktif Emanet Sayısı";
            dgvUyeler.Columns["GecikenEmanet"].HeaderText = "Geciken Emanet Sayısı";
            dgvUyeler.Columns["AktifCeza"].HeaderText = "Aktif Gecikme Cezası (₺)";
            dgvUyeler.Columns["Durum"].HeaderText = "Durum";
            dgvUyeler.Columns["AktifCeza"].DefaultCellStyle.Format = "0.00";

        }
        private void txtUyeAra_TextChanged(object sender, EventArgs e)
        {
            UyeleriListele();
        }
        private void chkCezalilar_CheckedChanged(object sender, EventArgs e)
        {
            UyeleriListele();
        }
        private void btnUyeSil_Click(object sender, EventArgs e)
        {
            if (dgvUyeler.SelectedRows.Count == 0) return;

            int uyeID = Convert.ToInt32(dgvUyeler.SelectedRows[0].Cells["KullaniciID"].Value);


            bool aktifEmanetVar = db.OduncIslemleri.Any(o => o.KullaniciID == uyeID && o.IadeTarihi == null);
            bool cezaVar = db.OduncIslemleri.Any(o => o.KullaniciID == uyeID && o.CezaTutari > 0 && o.CezaOdendi == false);

            if (aktifEmanetVar || cezaVar)
            {
                MessageBox.Show("Bu üye silinemez. Aktif emanetleri veya cezası var.");
                return;
            }

            if (MessageBox.Show("Üyeyi pasif yapmak istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var uye = db.Kullanicilar.Find(uyeID);
                if (uye == null)
                {
                    MessageBox.Show("Üye bulunamadı.");
                    return;
                }
                uye.Aktif = false;


                db.Raporlar.Add(new Raporlar
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IslemTipi = "Üye Pasif",
                    IslemAciklamasi = $"Üye pasif yapıldı: {uye.K_Ad} {uye.Soyad}",
                    IslemTarihi = DateTime.Now
                });

                db.SaveChanges();
                UyeleriListele();

                MessageBox.Show("Üye pasif duruma alındı.");
            }
        }
        private void dgvUyeler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUyeler.SelectedRows.Count == 0)
            {
                grpUyeDetay.Visible = false;
                return;
            }

            DataGridViewRow secilen = dgvUyeler.SelectedRows[0];

            txtAdSoyad.Text = secilen.Cells["AdSoyad"].Value?.ToString();
            txtEposta.Text = secilen.Cells["Email"].Value?.ToString();
            txtTelefon.Text = secilen.Cells["Telefon"].Value?.ToString();
            txtAdres.Text = secilen.Cells["Adres"].Value?.ToString();
            txtBakiye.Text = Convert.ToDecimal(secilen.Cells["Bakiye"].Value).ToString("C2");
            txtToplamCeza.Text = Convert.ToDecimal(secilen.Cells["ToplamCeza"].Value).ToString("C2");
            txtAktifEmanet.Text = secilen.Cells["AktifEmanet"].Value.ToString();
            txtGecikenEmanet.Text = secilen.Cells["GecikenEmanet"].Value.ToString();
            grpUyeDetay.Visible = true;
        }
        //KİTAP İŞLEMLERİ PANEL
        private void btnKitapİslemleri_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlKitapIslemleri.Visible = true;
        }
        private void btnEkle_Click(object sender, EventArgs e)
        {

            string yayineviAd = txtYayinevi.Text.Trim();
            var yayinevi = db.Yayinevleri.FirstOrDefault(y => y.Yayinevi_Ad == yayineviAd);
            if (yayinevi == null)
            {
                yayinevi = new Yayinevleri { Yayinevi_Ad = yayineviAd };
                db.Yayinevleri.Add(yayinevi);

            }
            var kitap = new Kitaplar
            {
                KitapAdi = txtKitapAdi.Text.Trim(),
                ISBN = txtIsbn.Text.Trim(),
                BasimYili = int.Parse(txtBasimYili.Text.Trim()),
                Stok = int.Parse(txtStok.Text.Trim()),
                SayfaSayisi = int.Parse(txtSayfaSayisi.Text.Trim()),
                YayineviID = yayinevi.YayineviID
            };
            db.Kitaplar.Add(kitap);

            string yazarAd = txtYazarAd.Text.Trim();
            string yazarSoyad = txtYazarSoyad.Text.Trim();
            var yazar = db.Yazarlar.FirstOrDefault(y => y.Yazar_Ad == yazarAd && y.Soyad == yazarSoyad);
            if (yazar == null)
            {
                yazar = new Yazarlar { Yazar_Ad = yazarAd, Soyad = yazarSoyad };
                db.Yazarlar.Add(yazar);
            }

            string kategoriAd = txtKategori.Text.Trim();
            var kategori = db.Kategoriler.FirstOrDefault(k => k.KategoriAdi == kategoriAd);
            if (kategori == null)
            {
                kategori = new Kategoriler { KategoriAdi = kategoriAd };
                db.Kategoriler.Add(kategori);
            }

            kitap.Yazarlar.Add(yazar);
            kitap.Kategoriler.Add(kategori);

            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifKutuphaneci.KullaniciID,
                IlgiliKitapID = kitap.KitapID,
                IslemTipi = "Kitap Ekle",
                IslemAciklamasi = $"Kitap eklendi: {kitap.KitapAdi}",
                IslemTarihi = DateTime.Now
            });
            db.SaveChanges();
            MessageBox.Show("Kitap başarıyla eklendi.");
            KitaplariListele();
            KitaplariTemizle();
        }
        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            int kitapId = int.Parse(txtKitapID.Text.Trim());
            var kitap = db.Kitaplar
                .Include("Yazarlar")
                .Include("Kategoriler")
                .Include("Yayinevleri")
                .FirstOrDefault(k => k.KitapID == kitapId);

            if (kitap == null)
            {
                MessageBox.Show("Kitap bulunamadı.");
                return;
            }
            kitap.KitapAdi = txtKitapAdi.Text.Trim();
            kitap.ISBN = txtIsbn.Text.Trim();
            kitap.BasimYili = int.Parse(txtBasimYili.Text.Trim());
            kitap.Stok = int.Parse(txtStok.Text.Trim());
            kitap.SayfaSayisi = int.Parse(txtSayfaSayisi.Text.Trim());


            string yayineviAd = txtYayinevi.Text.Trim();
            var yayinevi = db.Yayinevleri.FirstOrDefault(y => y.Yayinevi_Ad == yayineviAd);
            if (yayinevi == null)
            {
                yayinevi = new Yayinevleri { Yayinevi_Ad = yayineviAd };
                db.Yayinevleri.Add(yayinevi);
            }
            kitap.YayineviID = yayinevi.YayineviID;


            kitap.Yazarlar.Clear();

            string yazarAd = txtYazarAd.Text.Trim();
            string yazarSoyad = txtYazarSoyad.Text.Trim();

            var yazar = db.Yazarlar.FirstOrDefault(y => y.Yazar_Ad == yazarAd && y.Soyad == yazarSoyad);
            if (yazar == null)
            {
                yazar = new Yazarlar { Yazar_Ad = yazarAd, Soyad = yazarSoyad };
                db.Yazarlar.Add(yazar);
            }
            kitap.Yazarlar.Add(yazar);


            kitap.Kategoriler.Clear();

            string kategoriAd = txtKategori.Text.Trim();
            var kategori = db.Kategoriler.FirstOrDefault(k => k.KategoriAdi == kategoriAd);
            if (kategori == null)
            {
                kategori = new Kategoriler { KategoriAdi = kategoriAd };
                db.Kategoriler.Add(kategori);
            }
            kitap.Kategoriler.Add(kategori);

            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifKutuphaneci.KullaniciID,
                IlgiliKitapID = kitap.KitapID,
                IslemTipi = "Kitap Güncelleme",
                IslemAciklamasi = $"Kitap güncellendi: {kitap.KitapAdi}",
                IslemTarihi = DateTime.Now
            });
            db.SaveChanges();
            MessageBox.Show("Kitap başarıyla güncellendi.");
            KitaplariListele();
            KitaplariTemizle();

        }
        private void btnSil_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKitapID.Text))
            {
                MessageBox.Show("Lütfen bir kitap seçin.");
                return;
            }

            int kitapId = int.Parse(txtKitapID.Text.Trim());
            bool oduncVar = db.OduncIslemleri.Any(o => o.KitapID == kitapId && o.IadeTarihi == null);
            if (oduncVar)
            {
                MessageBox.Show("Bu kitap halen ödünç olarak verildiği için silinemez.");
                return;
            }

            var kitap = db.Kitaplar.FirstOrDefault(k => k.KitapID == kitapId);
            if (kitap == null)
            {
                MessageBox.Show("Kitap bulunamadı.");
                return;
            }

            kitap.Stok = 0;
            db.Raporlar.Add(new Raporlar
            {
                KullaniciID = aktifKutuphaneci.KullaniciID,
                IlgiliKitapID = kitap.KitapID,
                IslemTipi = "Kitap Silme",
                IslemAciklamasi = $"Kitap silindi (stok 0): {kitap.KitapAdi}",
                IslemTarihi = DateTime.Now
            });

            db.SaveChanges();
            MessageBox.Show("Kitap stoktan düşürüldü (silindi olarak işaretlendi).");
            KitaplariListele();
            KitaplariTemizle();
        }
        private void dgvKitaplar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvKitaplar.Rows[e.RowIndex];

                txtKitapID.Text = row.Cells["KitapID"].Value.ToString();
                txtKitapAdi.Text = row.Cells["KitapAdi"].Value.ToString();
                txtIsbn.Text = row.Cells["ISBN"].Value.ToString();
                txtBasimYili.Text = row.Cells["BasimYili"].Value.ToString();
                txtStok.Text = row.Cells["Stok"].Value.ToString();
                txtSayfaSayisi.Text = row.Cells["SayfaSayisi"].Value.ToString();
                txtYayinevi.Text = row.Cells["Yayinevi"].Value.ToString();
                txtYazarAd.Text = row.Cells["Yazarlar"].Value.ToString().Split(' ')[0];
                txtYazarSoyad.Text = row.Cells["Yazarlar"].Value.ToString().Split(' ').Length > 1 ?
                                     row.Cells["Yazarlar"].Value.ToString().Split(' ')[1] : "";
                txtKategori.Text = row.Cells["Kategoriler"].Value.ToString();
            }
        }
        private void dgvKitaplar_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvKitaplar.ClearSelection();
        }
        private void dgvKitaplar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int stok = Convert.ToInt32(dgvKitaplar.Rows[e.RowIndex].Cells["Stok"].Value);
            if (stok == 0)
            {
                dgvKitaplar.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(192, 110, 90);
                dgvKitaplar.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(82, 45, 23);
            }
        }
        // KİTAP TALEP İŞLEMLERİ PANEL
        private void btnTalepIslem_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlTalepIslemleri.Visible = true;
        }
        private void BekleyenTalepleriListele()
        {
            var liste = db.OduncTalepleri
         .Include(t => t.Uye)
         .Include(t => t.Kitaplar)
         .Where(t => t.TalepDurumu == "Beklemede")
         .OrderByDescending(t => t.TalepTarihi)
         .Select(t => new
         {
             t.TalepID,
             UyeAdi = t.Uye.K_Ad + " " + t.Uye.Soyad,
             t.Kitaplar.KitapAdi,
             t.TalepTarihi,
             t.TalepDurumu
         })
         .ToList();

            dgvBekleyenTalepler.DataSource = liste;
            dgvBekleyenTalepler.Columns["TalepID"].Visible = false;
            dgvBekleyenTalepler.Columns["UyeAdi"].HeaderText = "Üye Adı";
            dgvBekleyenTalepler.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvBekleyenTalepler.Columns["TalepTarihi"].HeaderText = "Talep Tarihi";
            dgvBekleyenTalepler.Columns["TalepDurumu"].HeaderText = "Talep Durumu";
        }
        private void GecmisTalepleriListele()
        {
            string filtre = cmbDurumFiltre.SelectedItem?.ToString();
            int seciliUyeId = 0;
            if (cmbFiltreUye.SelectedItem != null)
            {
                seciliUyeId = (int)cmbFiltreUye.SelectedValue;
            }
            var sorgu = db.OduncTalepleri
                .Include(t => t.Uye)
                .Include(t => t.Kitaplar)
                .AsQueryable();

            if (filtre == "Onaylandı")
            {
                sorgu = sorgu.Where(t => t.TalepDurumu == "Onaylandı");
            }
            else if (filtre == "Reddedildi")
            {
                sorgu = sorgu.Where(t => t.TalepDurumu == "Reddedildi");
            }
            else
            {
                sorgu = sorgu.Where(t => t.TalepDurumu == "Onaylandı" || t.TalepDurumu == "Reddedildi");
            }

            if (seciliUyeId != 0)
            {
                sorgu = sorgu.Where(t => t.UyeID == seciliUyeId);
            }

            var liste = sorgu
                .OrderByDescending(t => t.TalepTarihi)
                .Select(t => new
                {
                    t.TalepID,
                    UyeAdi = t.Uye.K_Ad + " " + t.Uye.Soyad,
                    t.Kitaplar.KitapAdi,
                    t.TalepTarihi,
                    t.TalepDurumu,
                    t.OnayTarihi,
                })
                .ToList();

            dgvGecmisTalepler.DataSource = liste;
            dgvGecmisTalepler.Columns["TalepID"].Visible = false;
            dgvGecmisTalepler.Columns["UyeAdi"].HeaderText = "Üye Adı";
            dgvGecmisTalepler.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvGecmisTalepler.Columns["TalepTarihi"].HeaderText = "Talep Tarihi";
            dgvGecmisTalepler.Columns["TalepDurumu"].HeaderText = "Talep Durumu";
            dgvGecmisTalepler.Columns["OnayTarihi"].HeaderText = "Onay Tarihi";
        }
        private void btnOnayla_Click(object sender, EventArgs e)
        {
            if (dgvBekleyenTalepler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir işlem seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int talepID = Convert.ToInt32(dgvBekleyenTalepler.SelectedRows[0].Cells["TalepID"].Value);
            var talep = db.OduncTalepleri
                .Include(t => t.Uye)
                .Include(t => t.Kitaplar)
                .FirstOrDefault(t => t.TalepID == talepID);

            if (talep == null)
            {
                MessageBox.Show("Talep bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int uyeID = talep.UyeID;
            var uye = talep.Uye;
            var kitap = talep.Kitaplar;


            bool cezaVarMi = db.OduncIslemleri.Any(o =>
                o.KullaniciID == uyeID &&
                o.CezaTutari > 0 &&
                !o.CezaOdendi);

            if (cezaVarMi)
            {
                talep.TalepDurumu = "Reddedildi";
                talep.Aciklama = "Ödenmemiş ceza nedeniyle talep reddedildi.";
                talep.OnaylayanID = aktifKutuphaneci.KullaniciID;
                talep.OnayTarihi = DateTime.Now;

                db.Raporlar.Add(new Raporlar
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IlgiliUyeID = talep.Uye.KullaniciID,
                    IlgiliKitapID = talep.KitapID,
                    IslemTipi = "Talep Reddedildi",
                    IslemAciklamasi = $"Kütüphaneci tarafından reddedildi (ceza).",
                    IslemTarihi = DateTime.Now
                });

                db.SaveChanges();
                MessageBox.Show($"{uye.K_Ad} {uye.Soyad} adlı üyenin ödenmemiş cezası olduğu için talep reddedildi.",
                    "Talep Reddedildi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                BekleyenTalepleriListele();
                GecmisTalepleriListele();
                return;
            }
            if (kitap == null || kitap.Stok <= 0)
            {
                talep.TalepDurumu = "Reddedildi";
                talep.Aciklama = "Kitap stokta olmadığı için talep reddedildi.";
                talep.OnaylayanID = aktifKutuphaneci.KullaniciID;
                talep.OnayTarihi = DateTime.Now;

                db.Raporlar.Add(new Raporlar
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IlgiliUyeID = talep.Uye.KullaniciID,
                    IlgiliKitapID = talep.KitapID,
                    IslemTipi = "Talep Reddedildi",
                    IslemAciklamasi = "Kütüphaneci tarafından reddedildi (stok yok).",
                    IslemTarihi = DateTime.Now
                });

                db.SaveChanges();
                MessageBox.Show($"'{kitap?.KitapAdi ?? "Bilinmeyen Kitap"}' kitabı stokta olmadığı için talep reddedildi.",
                    "Stok Yok", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                BekleyenTalepleriListele();
                GecmisTalepleriListele();
                return;
            }
            int aktifEmanetSayisi = db.OduncIslemleri.Count(x => x.KullaniciID == uyeID && x.IadeTarihi == null);
            int bekleyenTalepSayisi = db.OduncTalepleri.Count(t => t.UyeID == uyeID && t.TalepDurumu == "Beklemede" && t.TalepID != talep.TalepID);

            if ((aktifEmanetSayisi + bekleyenTalepSayisi) >= 3)
            {
                talep.TalepDurumu = "Reddedildi";
                talep.Aciklama = $"Toplamda {aktifEmanetSayisi} emanet ve {bekleyenTalepSayisi} bekleyen talebiniz bulunduğundan, 3 kitap sınırı aşıldığı için bu talep reddedilmiştir.";
                talep.OnaylayanID = aktifKutuphaneci.KullaniciID;
                talep.OnayTarihi = DateTime.Now;

                db.Raporlar.Add(new Raporlar
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IlgiliUyeID = talep.Uye.KullaniciID,
                    IlgiliKitapID = talep.KitapID,
                    IslemTipi = "Talep Reddedildi",
                    IslemAciklamasi = "Kütüphaneci tarafından reddedildi (3 kitap sınırı aşıldı).",
                    IslemTarihi = DateTime.Now
                });

                db.SaveChanges();
                MessageBox.Show($"{uye.K_Ad} {uye.Soyad} adlı üyenin kitap sınırı dolu olduğu için talep reddedildi.",
                    "Talep Reddedildi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                BekleyenTalepleriListele();
                GecmisTalepleriListele();
                return;
            }
            bool ayniKitapEmanette = db.OduncIslemleri.Any(o =>
                o.KullaniciID == uyeID &&
                o.KitapID == talep.KitapID &&
                o.IadeTarihi == null);

            if (ayniKitapEmanette)
            {
                talep.TalepDurumu = "Reddedildi";
                talep.Aciklama = "Aynı kitap zaten üyede emanette olduğu için talep reddedildi.";
                talep.OnaylayanID = aktifKutuphaneci.KullaniciID;
                talep.OnayTarihi = DateTime.Now;

                db.Raporlar.Add(new Raporlar
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IlgiliUyeID = talep.Uye.KullaniciID,
                    IlgiliKitapID = talep.KitapID,
                    IslemTipi = "Talep Reddedildi",
                    IslemAciklamasi = "Kütüphaneci tarafından reddedildi (kitap zaten emanette).",
                    IslemTarihi = DateTime.Now
                });

                db.SaveChanges();
                MessageBox.Show($"{uye.K_Ad} {uye.Soyad} adlı üyede '{kitap.KitapAdi}' kitabı zaten emanette olduğu için talep reddedildi.",
                    "Talep Reddedildi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                BekleyenTalepleriListele();
                GecmisTalepleriListele();
                return;
            }
            try
            {
                kitap.Stok--;

                talep.TalepDurumu = "Onaylandı";
                talep.OnaylayanID = aktifKutuphaneci.KullaniciID;
                talep.OnayTarihi = DateTime.Now;
                talep.Aciklama = "Talep onaylandı ve ödünç verildi.";

                var yeniOdunc = new OduncIslemleri
                {
                    KitapID = talep.KitapID,
                    KullaniciID = talep.UyeID,
                    VerilisTarihi = DateTime.Now,
                    TeslimTarihi = DateTime.Now.AddDays(30),
                    IadeTarihi = null,
                    TalepID = talep.TalepID,
                    IslemYapanID = aktifKutuphaneci.KullaniciID,
                    CezaOdendi = false,
                    CezaTutari = 0,
                    GecikmeGun = 0
                };

                db.OduncIslemleri.Add(yeniOdunc);

                db.Raporlar.Add(new Raporlar
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IlgiliUyeID = talep.Uye.KullaniciID,
                    IlgiliKitapID = talep.KitapID,
                    IslemTipi = "Talep Onaylandı",
                    IslemAciklamasi = $"Kütüphaneci talebi onayladı ve ödünç işlemi oluşturuldu.",
                    IslemTarihi = DateTime.Now
                });

                db.SaveChanges();

                MessageBox.Show($"{uye.K_Ad} {uye.Soyad} adlı üyenin '{kitap.KitapAdi}' kitabı için talebi onaylandı ve ödünç işlemi oluşturuldu.",
                    "Talep Onaylandı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                BekleyenTalepleriListele();
                GecmisTalepleriListele();
                KitaplariListele();
                ListeleOduncKitaplar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Onaylama işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void cmbDurumFiltre_SelectedIndexChanged(object sender, EventArgs e)
        {
            GecmisTalepleriListele();
        }
        private void dgvBekleyenTalepler_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvBekleyenTalepler.ClearSelection();
        }
        private void dgvGecmisTalepler_DataBindingComplete_1(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvGecmisTalepler.ClearSelection();
        }
        private void cmbFiltreUye_SelectedIndexChanged(object sender, EventArgs e)
        {
            GecmisTalepleriListele();
        }
        private void dgvGecmisTalepler_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            var durumCell = grid.Rows[e.RowIndex].Cells["TalepDurumu"];
            if (durumCell?.Value == null) return;

            string durum = durumCell.Value.ToString();

            switch (durum)
            {
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
        //KİTAP ÖDÜNÇ VERME PANEL
        private void btnKitapOduncVerme_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlKitapVer.Visible = true;
        }
        private void dgvKitap_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvKitap.ClearSelection();
        }
        private void dgvKitap_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int stok = Convert.ToInt32(dgvKitap.Rows[e.RowIndex].Cells["Stok"].Value);
            if (stok == 0)
            {
                dgvKitap.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(192, 110, 90);
                dgvKitap.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(82, 45, 23);
            }
        }
        private void btnKitapVer_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbUyeler.SelectedValue == null || (int)cmbUyeler.SelectedValue == 0)
                {
                    MessageBox.Show("Lütfen bir üye seçin.");
                    return;
                }
                if (dgvKitap.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen ödünç verilecek kitabı seçin.");
                    return;
                }

                int secilenUyeID = (int)cmbUyeler.SelectedValue;
                int secilenKitapID = (int)dgvKitap.SelectedRows[0].Cells["KitapID"].Value;

                var kitap = db.Kitaplar.Find(secilenKitapID);
                if (kitap == null)
                {
                    MessageBox.Show("Seçilen kitap bulunamadı.");
                    return;
                }

                if (kitap.Stok <= 0)
                {
                    MessageBox.Show("Kitap stokta yok, ödünç verilemez.");
                    return;
                }

                int aktifKitapSayisi = db.OduncIslemleri
                    .Count(o => o.KullaniciID == secilenUyeID && o.IadeTarihi == null);

                if (aktifKitapSayisi >= 3)
                {
                    MessageBox.Show("Bu üye aynı anda en fazla 3 kitap alabilir.");
                    return;
                }
                bool kitapZatenAlindi = db.OduncIslemleri.Any(o =>
                    o.KullaniciID == secilenUyeID &&
                    o.KitapID == secilenKitapID &&
                    o.IadeTarihi == null);

                if (kitapZatenAlindi)
                {
                    MessageBox.Show("Bu üye zaten bu kitabı aldı.");
                    return;
                }

                if (dtpTeslimTarihi.Value.Date <= dtpVerilisTarihi.Value.Date)
                {
                    MessageBox.Show("Teslim tarihi, veriliş tarihinden sonra olmalıdır.");
                    return;
                }

                bool odenmemisCezaVar = db.OduncIslemleri.Any(o =>
                    o.KullaniciID == secilenUyeID &&
                    o.CezaTutari > 0 &&
                    !o.CezaOdendi);

                if (odenmemisCezaVar)
                {
                    MessageBox.Show("Bu üyenin ödenmemiş cezaları var. Yeni kitap ödünç alamaz.");
                    return;
                }

                bool gecikmeliIade = db.OduncIslemleri.Any(o =>
                    o.KullaniciID == secilenUyeID &&
                    o.GecikmeGun > 0);

                if (gecikmeliIade)
                {
                    MessageBox.Show("Dikkat! Bu üyenin geçmişte gecikmeli iadeleri var.");
                }

                OduncIslemleri yeniIslem = new OduncIslemleri
                {
                    KitapID = secilenKitapID,
                    KullaniciID = secilenUyeID,
                    VerilisTarihi = dtpVerilisTarihi.Value,
                    TeslimTarihi = dtpTeslimTarihi.Value,
                    IadeTarihi = null,
                    GecikmeGun = 0,
                    CezaTutari = 0,
                    CezaOdendi = false,
                    IslemYapanID = aktifKutuphaneci.KullaniciID
                };

                kitap.Stok--;

                db.OduncIslemleri.Add(yeniIslem);
                var rapor = new Raporlar
                {
                    KullaniciID = aktifKutuphaneci.KullaniciID,
                    IlgiliUyeID = secilenUyeID,
                    IlgiliKitapID = secilenKitapID,
                    IslemTipi = "Kitap Ödünç Verildi",
                    IslemAciklamasi = $"Kütüphaneci {aktifKutuphaneci.K_Ad} {aktifKutuphaneci.Soyad}, " +
                      $"Üye {cmbUyeler.Text} adına '{kitap.KitapAdi}' kitabını ödünç verdi.",
                    IslemTarihi = DateTime.Now
                };

                db.Raporlar.Add(rapor);
                db.SaveChanges();

                if (kitap.Stok == 0)
                {
                    MessageBox.Show("Bu kitap stoktan düşüldü. Artık stokta kalmadı.");
                }
                else if (kitap.Stok <= 2)
                {
                    MessageBox.Show("Bu kitap stoktan düşüldü. Stokta az sayıda kaldı.");
                }

                MessageBox.Show("Kitap başarıyla ödünç verildi.");

                KitaplariListele();
                ListeleOduncKitaplar();
                VerilenKitaplariYukle();
                UyeleriListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }
        private void VerilenKitaplariYukle(int? uyeId = null)
        {
            var sorgu = db.OduncIslemleri
        .Include("Kitaplar")
        .Include("Kullanicilar")
        .Where(o => o.IadeTarihi == null);

            if (uyeId.HasValue)
            {
                sorgu = sorgu.Where(o => o.KullaniciID == uyeId.Value);
            }

            var oduncler = sorgu
                .Select(o => new
                {
                    UyeAdi = o.Kullanicilar.K_Ad + " " + o.Kullanicilar.Soyad,
                    o.Kitaplar.KitapAdi,
                    o.VerilisTarihi,
                    o.TeslimTarihi
                })
                .ToList();

            dgvVerilenKitaplar.DataSource = oduncler;

            dgvVerilenKitaplar.Columns["UyeAdi"].HeaderText = "Üye Adı";
            dgvVerilenKitaplar.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvVerilenKitaplar.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
            dgvVerilenKitaplar.Columns["TeslimTarihi"].HeaderText = "Teslim Tarihi";
        }
        private void dgvVerilenKitaplar_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvVerilenKitaplar.ClearSelection();
        }
        private void CezaBilgisiGoster(int uyeId)
        {
            var oduncler = db.OduncIslemleri
                .Where(o => o.KullaniciID == uyeId && o.CezaTutari > 0 && !o.CezaOdendi)
                .ToList();

            decimal toplamCeza = oduncler.Sum(o => o.CezaTutari ?? 0);

            var gecikenler = db.OduncIslemleri
                .Where(o => o.KullaniciID == uyeId && o.IadeTarihi == null)
                .ToList();

            decimal cezaBirimUcret = 2.5m;

            foreach (var item in gecikenler)
            {
                if (item.TeslimTarihi.HasValue && item.TeslimTarihi.Value.Date < DateTime.Now.Date)
                {
                    int gecikmeGun = (DateTime.Now.Date - item.TeslimTarihi.Value.Date).Days;
                    if (gecikmeGun > 0)
                    {
                        toplamCeza += gecikmeGun * cezaBirimUcret;
                    }
                }
            }

            if (toplamCeza > 0)
            {
                txtCezaBilgisi.Text = $"Ödenmemiş / Gecikme Cezası: {toplamCeza} ₺";
                txtCezaBilgisi.ForeColor = Color.FromArgb(101, 67, 33);
            }
            else
            {
                txtCezaBilgisi.Text = "Ceza: Yok";
                txtCezaBilgisi.ForeColor = Color.FromArgb(160, 120, 80);
            }
        }
        private void cmbUyeler_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (cmbUyeler.SelectedValue == null)
            {
                txtCezaBilgisi.Clear();
                return;
            }

            int uyeId;
            bool donusumBasarili = int.TryParse(cmbUyeler.SelectedValue.ToString(), out uyeId);

            if (!donusumBasarili || uyeId == 0)
            {
                txtCezaBilgisi.Clear();
                return;
            }

            CezaBilgisiGoster(uyeId);
        }
        //IADE ALMA PANEL
        private void btnIadeAlma_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlIade.Visible = true;
        }
        private void dgvOduncKitaplar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int islemId = Convert.ToInt32(dgvOduncKitaplar.Rows[e.RowIndex].Cells["IslemID"].Value);

                var seciliIslem = db.OduncIslemleri
                    .Where(o => o.IslemID == islemId)
                    .Include(o => o.Kullanicilar)
                    .Include(o => o.Kitaplar)
                    .FirstOrDefault();

                if (seciliIslem == null || seciliIslem.Kullanicilar == null || seciliIslem.Kitaplar == null)
                {
                    MessageBox.Show("İlişkili kitap veya kullanıcı bilgisi eksik.");
                    return;
                }

                txtIadeKitap.Text = seciliIslem.Kitaplar.KitapAdi;
                txtIadeUyeAdi.Text = seciliIslem.Kullanicilar.K_Ad + " " + seciliIslem.Kullanicilar.Soyad;
                dtpIadeTarihi.Value = DateTime.Today;

                if (seciliIslem.TeslimTarihi == null)
                {
                    txtGecikmeGun.Text = "0";
                    txtCezaTutari.Text = "0.00";
                    return;
                }

                DateTime teslimTarihi = seciliIslem.TeslimTarihi.Value;
                TimeSpan fark = DateTime.Today - teslimTarihi;
                int gecikmeGun = fark.Days > 0 ? fark.Days : 0;
                decimal ceza = gecikmeGun * 2.5m;

                txtGecikmeGun.Text = gecikmeGun.ToString();
                txtCezaTutari.Text = ceza.ToString("0.00");
            }
        }
        private void ListeleOduncKitaplar()
        {
            var sorgu = db.OduncIslemleri
        .Include(o => o.Kitaplar)
        .Include(o => o.Kullanicilar)
        .Where(o => o.IadeTarihi == null);
            if (chkSadeceTalepler.Checked)
            {
                sorgu = sorgu.Where(o => o.TalepID != null);
            }
            var liste = sorgu.Select(o => new
            {
                o.IslemID,
                o.KitapID,
                o.Kitaplar.KitapAdi,
                UyeAdi = o.Kullanicilar.K_Ad + " " + o.Kullanicilar.Soyad,
                o.VerilisTarihi,
                o.TeslimTarihi,
                TalepDurumu = o.TalepID != null ? "Talep Üzerinden" : "Doğrudan",
                UzatmaTalebi = o.UzatmaTalepEdildi ? "Var" : "Yok",
                UzatmaOnay = !o.UzatmaTalepEdildi ? "-" :
                 (o.UzatmaReddedildi.HasValue ?
                    (o.UzatmaReddedildi.Value ? "Reddedildi" : "Onaylandı")
                    : "Beklemede"),
                o.UzatmaTarihi
            }).ToList();

            dgvOduncKitaplar.DataSource = liste;
            dgvOduncKitaplar.Columns["IslemID"].Visible = false;
            dgvOduncKitaplar.Columns["KitapID"].Visible = false;

            dgvOduncKitaplar.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvOduncKitaplar.Columns["UyeAdi"].HeaderText = "Üye Adı";
            dgvOduncKitaplar.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
            dgvOduncKitaplar.Columns["TeslimTarihi"].HeaderText = "Teslim Tarihi";
            dgvOduncKitaplar.Columns["TalepDurumu"].HeaderText = "Talep Durumu";

            bool uzatmaVarmi = liste.Any(o => o.UzatmaTalebi == "Var");

            if (uzatmaVarmi)
            {
                dgvOduncKitaplar.Columns["UzatmaTalebi"].Visible = true;
                dgvOduncKitaplar.Columns["UzatmaOnay"].Visible = true;
                dgvOduncKitaplar.Columns["UzatmaTarihi"].Visible = true;

                dgvOduncKitaplar.Columns["UzatmaTalebi"].HeaderText = "Uzatma Talebi";
                dgvOduncKitaplar.Columns["UzatmaOnay"].HeaderText = "Uzatma Onayı";
                dgvOduncKitaplar.Columns["UzatmaTarihi"].HeaderText = "Uzatma Tarihi";
            }
            else
            {
                dgvOduncKitaplar.Columns["UzatmaTalebi"].Visible = false;
                dgvOduncKitaplar.Columns["UzatmaOnay"].Visible = false;
                dgvOduncKitaplar.Columns["UzatmaTarihi"].Visible = false;
            }
        }
        private void btnIadeAl_Click(object sender, EventArgs e)
        {
            if (dgvOduncKitaplar.SelectedRows.Count == 0)
            {
                MessageBox.Show("İade edilecek işlem seçilmedi.");
                return;
            }

            int islemId = Convert.ToInt32(dgvOduncKitaplar.SelectedRows[0].Cells["IslemID"].Value);

            var islem = db.OduncIslemleri
                .Include(o => o.Kitaplar)
                .Include(o => o.Kullanicilar)
                .FirstOrDefault(o => o.IslemID == islemId);

            if (islem == null)
            {
                MessageBox.Show("İşlem bulunamadı.");
                return;
            }

            int gecikmeGun = Math.Max(0, (DateTime.Today - islem.TeslimTarihi.Value).Days);
            decimal ceza = gecikmeGun * 2.5m;

            var uye = islem.Kullanicilar;

            var uyeDetay = db.UyeDetay.FirstOrDefault(u => u.KullaniciID == uye.KullaniciID);
            if (uyeDetay == null)
            {
                MessageBox.Show("Üye detayları bulunamadı.");
                return;
            }

            if (ceza > 0)
            {
                if (uyeDetay.Bakiye < ceza)
                {
                    MessageBox.Show($"Ceza: {ceza:0.00} ₺\nBakiye yetersiz! ({uyeDetay.Bakiye:0.00} ₺)\nLütfen bakiye yükleyin.");
                    return;
                }

                uyeDetay.Bakiye -= ceza;
                islem.CezaOdendi = true;
                islem.CezaTutari = 0;
            }
            else
            {
                islem.CezaOdendi = false;
                islem.CezaTutari = 0;
            }

            islem.IadeTarihi = DateTime.Today;
            islem.GecikmeGun = gecikmeGun;
            islem.CezaTutari = ceza;
            islem.IslemYapanID = aktifKutuphaneci.KullaniciID;
            islem.Kitaplar.Stok++;

            var yeniRapor = new Raporlar
            {
                KullaniciID = aktifKutuphaneci.KullaniciID,
                IlgiliUyeID = uye.KullaniciID,
                IslemTipi = "İade İşlemleri",
                IslemAciklamasi = $"{uye.K_Ad} {uye.Soyad} adlı üye '{islem.Kitaplar.KitapAdi}' kitabını iade etti. " +
                         $"Gecikme: {gecikmeGun} gün, Ceza: {ceza:0.00} ₺",
                IslemTarihi = DateTime.Now,
                IlgiliKitapID = islem.KitapID
            };

            db.Raporlar.Add(yeniRapor);
            db.SaveChanges();

            MessageBox.Show("İade ve varsa ceza işlemi başarıyla tamamlandı.");
            ListeleOduncKitaplar();
            IadeAlinanKitaplariListele();
            KitaplariListele();
            UyeleriListele();
            AlanlariTemizle();
        }
        private void chkSadeceTalepler_CheckedChanged(object sender, EventArgs e)
        {
            ListeleOduncKitaplar();
        }
        private void IadeAlinanKitaplariListele()
        {
            var iadeListesi = db.OduncIslemleri
                .Where(o => o.IadeTarihi != null)
                .OrderByDescending(o => o.IadeTarihi)
                .Select(o => new
                {
                    o.IslemID,
                    o.Kitaplar.KitapAdi,
                    UyeAdi = o.Kullanicilar.K_Ad + " " + o.Kullanicilar.Soyad,
                    o.VerilisTarihi,
                    o.TeslimTarihi,
                    o.IadeTarihi,
                    o.GecikmeGun,
                    Ceza = o.CezaTutari,
                    CezaDurumu = o.CezaTutari > 0 ? "Tahsil Edildi" : "Ceza Yok"

                })
                .ToList();

            dgvIadeAlinanKitaplar.DataSource = iadeListesi;
            dgvIadeAlinanKitaplar.Columns["IslemID"].Visible = false;
            dgvIadeAlinanKitaplar.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvIadeAlinanKitaplar.Columns["UyeAdi"].HeaderText = "Üye Adı";
            dgvIadeAlinanKitaplar.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
            dgvIadeAlinanKitaplar.Columns["TeslimTarihi"].HeaderText = "Teslim Tarihi";
            dgvIadeAlinanKitaplar.Columns["IadeTarihi"].HeaderText = "İade Tarihi";
            dgvIadeAlinanKitaplar.Columns["GecikmeGun"].HeaderText = "Gecikme (Gün)";
            dgvIadeAlinanKitaplar.Columns["Ceza"].HeaderText = "Ceza";
            dgvIadeAlinanKitaplar.Columns["CezaDurumu"].HeaderText = "Ceza Durumu";
        }
        private void dgvIadeAlinanKitaplar_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvIadeAlinanKitaplar.ClearSelection();
        }
        private void dgvIadeAlinanKitaplar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvIadeAlinanKitaplar.Columns[e.ColumnIndex].Name == "Ceza" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal ceza))
                {
                    e.Value = ceza.ToString("0.00") + " ₺";
                    e.FormattingApplied = true;
                }
            }
        }
        private void dgvIadeAlinanKitaplar_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dgvIadeAlinanKitaplar.Rows[e.RowIndex];

            object cezaObj = row.Cells["Ceza"]?.Value;
            object gecikmeObj = row.Cells["GecikmeGun"]?.Value;
            object odendiObj = row.Cells["CezaDurumu"]?.Value;

            if (cezaObj != null && gecikmeObj != null &&
                decimal.TryParse(cezaObj.ToString(), out decimal ceza) &&
                int.TryParse(gecikmeObj.ToString(), out int gecikme))
            {
                bool cezaOdendi = false;
                if (odendiObj != null)
                    bool.TryParse(odendiObj.ToString(), out cezaOdendi);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(70, 55, 45);

                if (ceza > 0 && cezaOdendi && gecikme >= 10)
                {

                    row.DefaultCellStyle.BackColor = Color.FromArgb(215, 200, 190);
                }
                else if (ceza > 0 && cezaOdendi)
                {

                    row.DefaultCellStyle.BackColor = Color.FromArgb(230, 220, 210);
                }
                else if (ceza == 0)
                {

                    row.DefaultCellStyle.BackColor = Color.FromArgb(250, 245, 240);
                }
            }
        }
        private void dgvOduncKitaplar_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvOduncKitaplar.ClearSelection();
        }
        //GEÇMİŞ EMANETLER PANEL
        private void btnGecmisEmanetler_Click(object sender, EventArgs e)
        {
            GizlePanel();
            pnlGecmisEmanetler.Visible = true;
        }
        private void FiltreleIadeler()
        {
            int seciliUyeId = 0;
            if (cmbUyeSec.SelectedValue != null)
                int.TryParse(cmbUyeSec.SelectedValue.ToString(), out seciliUyeId);

            DateTime baslangic = dtpBaslangic.Value.Date;
            DateTime bitis = dtpBitis.Value.Date.AddDays(1).AddSeconds(-1);

            var sorgu = db.OduncIslemleri.Include(o => o.Kitaplar).Where(o =>
                o.IadeTarihi != null &&
                o.IadeTarihi >= baslangic &&
                o.IadeTarihi <= bitis);

            if (seciliUyeId != 0)
            {
                sorgu = sorgu.Where(o => o.KullaniciID == seciliUyeId);
            }

            var sonuc = sorgu.Select(o => new
            {
                o.IslemID,
                UyeAdi = o.Kullanicilar.K_Ad + " " + o.Kullanicilar.Soyad,
                o.Kitaplar.KitapAdi,
                o.VerilisTarihi,
                o.TeslimTarihi,
                o.IadeTarihi,
                o.GecikmeGun,
                Ceza = o.CezaTutari,
                CezaDurumu = o.CezaTutari > 0 ? "Tahsil Edildi" : "Ceza Yok"
            })
            .OrderByDescending(o => o.VerilisTarihi)
            .ToList();

            dgvGecmisEmanetler.DataSource = sonuc;
            dgvGecmisEmanetler.Columns["IslemID"].Visible = false;
            dgvGecmisEmanetler.Columns["UyeAdi"].HeaderText = "Üye Adı";
            dgvGecmisEmanetler.Columns["KitapAdi"].HeaderText = "Kitap Adı";
            dgvGecmisEmanetler.Columns["VerilisTarihi"].HeaderText = "Veriliş Tarihi";
            dgvGecmisEmanetler.Columns["TeslimTarihi"].HeaderText = "Teslim Tarihi";
            dgvGecmisEmanetler.Columns["IadeTarihi"].HeaderText = "Iade Tarihi";


            bool cezaVarMi = sonuc.Any(e => e.Ceza > 0);

            if (cezaVarMi)
            {
                dgvGecmisEmanetler.Columns["Ceza"].Visible = true;
                dgvGecmisEmanetler.Columns["CezaDurumu"].Visible = true;
                dgvGecmisEmanetler.Columns["GecikmeGun"].Visible = true;
                dgvGecmisEmanetler.Columns["Ceza"].HeaderText = "Ceza (TL)";
                dgvGecmisEmanetler.Columns["CezaDurumu"].HeaderText = "Ceza Durumu";
                dgvGecmisEmanetler.Columns["GecikmeGun"].HeaderText = "Gecikme Gün";
            }
            else
            {
                dgvGecmisEmanetler.Columns["Ceza"].Visible = false;
                dgvGecmisEmanetler.Columns["CezaDurumu"].Visible = false;
                dgvGecmisEmanetler.Columns["GecikmeGun"].Visible = false;
            }
        }
        private void cmbUyeSec_SelectedValueChanged(object sender, EventArgs e)
        {
            FiltreleIadeler();
        }
        private void dtpBaslangic_ValueChanged(object sender, EventArgs e)
        {
            FiltreleIadeler();
        }
        private void dtpBitis_ValueChanged(object sender, EventArgs e)
        {
            FiltreleIadeler();
        }
        private void dgvGecmisEmanetler_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvGecmisEmanetler.ClearSelection();
        }
        //TEMA CLASS
        private void pictureTema_Click(object sender, EventArgs e)
        {
            TemaDegistir(pictureTema);
        }
    }
}




