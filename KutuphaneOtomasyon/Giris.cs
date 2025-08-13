using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KutuphaneOtomasyon
{
    public partial class Giris : TemaUygulama
    {
        KutuphaneEntities db = new KutuphaneEntities();

        public Giris()
        {
            InitializeComponent();

        }
        private void Giris_Load(object sender, EventArgs e)
        {
            txtSifre.UseSystemPasswordChar = true;

        }

        private void btnGiris_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string sifre = txtSifre.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun!");
                return;
            }

            var kullanici = db.Kullanicilar
                             .Include("Roller")
                             .FirstOrDefault(k => k.Email == email && k.Sifre == sifre);

            if (kullanici == null)
            {
                MessageBox.Show("E-posta veya şifre hatalı!");
                return;
            }

            var ilkRol = kullanici.Roller.FirstOrDefault();
            if (ilkRol == null)
            {
                MessageBox.Show("Rol bilgisi bulunamadı!");
                return;
            }

            string rolAdi = ilkRol.RolAdi;
            MessageBox.Show($"Giriş başarılı! Rol: {rolAdi}");

            switch (rolAdi)
            {
                case "Yönetici":
                    new YoneticiPanel(kullanici).Show();
                    break;
                case "Kütüphaneci":
                    var gorevliDetay = db.KutuphaneGorevliDetay
                    .FirstOrDefault(g => g.KullaniciID == kullanici.KullaniciID);
                    
                    if (gorevliDetay == null)
                    {
                        gorevliDetay = new KutuphaneGorevliDetay
                        {
                            KullaniciID = kullanici.KullaniciID,
                            IlkGiris = true 
                        };
                        db.KutuphaneGorevliDetay.Add(gorevliDetay);
                        db.SaveChanges();
                    }

                    bool ilkGirisDurumu = gorevliDetay.IlkGiris;
                    new KutuphaneGorevlisiPanel(kullanici, ilkGirisDurumu).Show();
                    break;
                case "Üye":
                    new UyePanel(kullanici).Show();
                    break;
                default:
                    MessageBox.Show("Tanımsız rol!");
                    return;
            }

            this.Hide();
        }

        private void btnKayit_Click(object sender, EventArgs e)
        {
            Kayit kayit = new Kayit();
            kayit.Show();
            this.Hide();
        }

        private void chkSifreGoster_CheckedChanged(object sender, EventArgs e)
        {
            bool sifreyiGoster = chkSifreGoster.Checked;
            txtSifre.UseSystemPasswordChar = !sifreyiGoster;
        }
        private void txtSifre_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnGiris.PerformClick();
            }
        }
    }
}
