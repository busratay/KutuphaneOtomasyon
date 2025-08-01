using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace KutuphaneOtomasyon
{
    public partial class Kayit : TemaUygulama
    {
        KutuphaneEntities db = new KutuphaneEntities();
        public Kayit()
        {
            InitializeComponent();
        }

        private void Kayit_Load(object sender, EventArgs e)
        {
            txtSifre.UseSystemPasswordChar = true;
        }

        private void btnKayit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAd.Text) ||
                string.IsNullOrWhiteSpace(txtSoyad.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                MessageBox.Show("Tüm alanları doldurun!");
                return;
            }

            string email = txtEmail.Text.Trim();
            string sifre = txtSifre.Text.Trim();

            if (sifre.Length < 8 ||
                !sifre.Any(char.IsUpper) ||
                !sifre.Any(char.IsLower) ||
                !sifre.Any(char.IsDigit) ||
                !sifre.Any(c => !char.IsLetterOrDigit(c)))
            {
                MessageBox.Show("Şifre en az 8 karakter uzunluğunda, büyük harf, küçük harf, rakam ve özel karakter içermelidir.");
                return;
            }

            if (db.Kullanicilar.Any(u => u.Email == email))
            {
                MessageBox.Show("Bu e-posta adresiyle zaten kayıt var!");
                return;
            }

            var yeniKullanici = new Kullanicilar()
            {
                K_Ad = txtAd.Text,
                Soyad = txtSoyad.Text,
                Email = email,
                Sifre = sifre,
                Roller = new List<Roller>()
            };
            var uyeDetay = new UyeDetay()
            {
                KullaniciID = yeniKullanici.KullaniciID, 
                KayitTarihi = DateTime.Now.Date,
                Statu = "Normal"

            };
            var uyeRolu = db.Roller.FirstOrDefault(r => r.RolAdi == "Üye");
            if (uyeRolu != null)
            {
                yeniKullanici.Roller.Add(uyeRolu);
            }

            db.Kullanicilar.Add(yeniKullanici);
            db.UyeDetay.Add(uyeDetay);
            db.SaveChanges();

            MessageBox.Show("Kayıt başarılı! Giriş ekranına yönlendiriliyorsunuz.");

            Giris login = new Giris();
            login.Show();
            this.Hide();
        }

        private void chkSifreGoster_CheckedChanged(object sender, EventArgs e)
        {
            bool sifreyiGoster = chkSifreGoster.Checked;
            txtSifre.UseSystemPasswordChar = !sifreyiGoster;
        }

        private void btnGeriDon_Click(object sender, EventArgs e)
        {
            Giris login = new Giris();
            login.Show();
            this.Hide();
        }
    }
}

