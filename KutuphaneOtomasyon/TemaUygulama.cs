using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KutuphaneOtomasyon
{
    public class TemaUygulama : Form
    {
        protected Tema tema = new Tema();
        protected bool karanlikMod;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tema.OrijinalRenkleriSakla(this);
            karanlikMod = Properties.Settings.Default.KaranlikMod;

            if (karanlikMod)
                tema.TemaUygula(this);
            else
                tema.OrijinalRenkleriGeriYukle(this);
        }

        protected void TemaDegistir(PictureBox picture)
        {
            karanlikMod = !karanlikMod;

            if (karanlikMod)
            {
                tema.TemaUygula(this);
                picture.Image = Properties.Resources.sun2;
            }
            else
            {
                tema.OrijinalRenkleriGeriYukle(this);
                picture.Image = Properties.Resources.moon;
            }

            Properties.Settings.Default.KaranlikMod = karanlikMod;
            Properties.Settings.Default.Save();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TemaUygulama
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "TemaUygulama";
            this.ResumeLayout(false);

        }
    }
}

