using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KutuphaneOtomasyon
{
    public class Tema
    {
        private Dictionary<Control, (Color BackColor, Color ForeColor)> orijinalRenkler = new Dictionary<Control, (Color, Color)>();
        private Dictionary<DataGridView, DataGridViewStyles> orijinalDataGridStyles = new Dictionary<DataGridView, DataGridViewStyles>();
        private Dictionary<Button, ButtonStyles> orijinalButtonStyles = new Dictionary<Button, ButtonStyles>();
        private Dictionary<Chart, ChartStyles> orijinalChartStyles = new Dictionary<Chart, ChartStyles>();

        public struct ChartStyles
        {
            public Color BackColor;
            public Color ForeColor;
            public Color ChartAreaBackColor;
            public Color AxisLineColor;
            public Color AxisLabelForeColor;
            public Color LegendBackColor;
            public Color LegendForeColor;

        }
        public struct DataGridViewStyles
        {
            public Color BackgroundColor;
            public Color DefaultBackColor;
            public Color DefaultForeColor;
            public Color DefaultSelectionBackColor;
            public Color DefaultSelectionForeColor;
            public Color AlternatingBackColor;
            public Color AlternatingForeColor;
            public Color AlternatingSelectionBackColor;
            public Color AlternatingSelectionForeColor;
            public Color HeaderBackColor;
            public Color HeaderForeColor;
            public Color HeaderSelectionBackColor;
            public Color HeaderSelectionForeColor;
            public bool EnableHeadersVisualStyles;
        }
        public struct ButtonStyles
        {
            public Color BackColor;
            public Color ForeColor;
            public FlatStyle FlatStyle;
            public int BorderSize;
            public Color MouseOverBackColor;
            public Color MouseDownBackColor;
        }
        public void OrijinalRenkleriSakla(Control parent)
        {
            SaklaRecursive(parent);
        }
        private void SaklaRecursive(Control ctrl)
        {
            if (!orijinalRenkler.ContainsKey(ctrl))
            {
                orijinalRenkler[ctrl] = (ctrl.BackColor, ctrl.ForeColor);
            }
            switch (ctrl)
            {
                case Button btn when !orijinalButtonStyles.ContainsKey(btn):
                    orijinalButtonStyles[btn] = new ButtonStyles
                    {
                        BackColor = btn.BackColor,
                        ForeColor = btn.ForeColor,
                        FlatStyle = btn.FlatStyle,
                        BorderSize = btn.FlatAppearance.BorderSize,
                        MouseOverBackColor = btn.FlatAppearance.MouseOverBackColor,
                        MouseDownBackColor = btn.FlatAppearance.MouseDownBackColor
                    };
                    break;

                case DataGridView dgv when !orijinalDataGridStyles.ContainsKey(dgv):
                    orijinalDataGridStyles[dgv] = new DataGridViewStyles
                    {
                        BackgroundColor = dgv.BackgroundColor,
                        DefaultBackColor = dgv.DefaultCellStyle.BackColor,
                        DefaultForeColor = dgv.DefaultCellStyle.ForeColor,
                        DefaultSelectionBackColor = dgv.DefaultCellStyle.SelectionBackColor,
                        DefaultSelectionForeColor = dgv.DefaultCellStyle.SelectionForeColor,
                        AlternatingBackColor = dgv.AlternatingRowsDefaultCellStyle.BackColor,
                        AlternatingForeColor = dgv.AlternatingRowsDefaultCellStyle.ForeColor,
                        AlternatingSelectionBackColor = dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor,
                        AlternatingSelectionForeColor = dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor,
                        HeaderBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor,
                        HeaderForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor,
                        HeaderSelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor,
                        HeaderSelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor,
                        EnableHeadersVisualStyles = dgv.EnableHeadersVisualStyles
                    };
                    break;

                case Chart chart when !orijinalChartStyles.ContainsKey(chart) && chart.ChartAreas.Count > 0 :
                    var chartArea = chart.ChartAreas[0];
                   
                    var legend = chart.Legends.Count > 0 ? chart.Legends[0] : null;

                    
                    orijinalChartStyles[chart] = new ChartStyles
                    {
                        BackColor = chart.BackColor,
                        ForeColor = chart.ForeColor,
                        ChartAreaBackColor = chartArea.BackColor,
                        AxisLineColor = chartArea.AxisX.LineColor,
                        AxisLabelForeColor = chartArea.AxisX.LabelStyle.ForeColor,
                        LegendBackColor = legend?.BackColor ?? Color.Empty,
                        LegendForeColor = legend?.ForeColor ?? Color.Empty,
                       
                    };
                    break;
            }
            foreach (Control child in ctrl.Controls)
            {
                SaklaRecursive(child);
            }
        }
        public void TemaUygula(Control parent)
        {
            TemaUygulaRecursive(parent);
        }
        private void TemaUygulaRecursive(Control ctrl)
        {
            switch (ctrl)
            {
                case Form form:
                    form.BackColor = Color.FromArgb(90, 70, 60); 
                    break;
                case TabPage tp:
                    tp.BackColor = Color.FromArgb(102, 79, 66);
                    tp.ForeColor = Color.FromArgb(255, 248, 240);
                    break;

                case Panel panel:
                    panel.BackColor = panel.Name == "pnlMenu"
                        ? Color.FromArgb(139, 106, 85)  
                        : Color.FromArgb(102, 79, 66); 
                    break;

                case Button btn:
                    btn.BackColor = Color.FromArgb(168, 135, 112);
                    btn.ForeColor = Color.FromArgb(255, 248, 240);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(195, 155, 128);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 110, 90);
                    break;

                case Label lbl:
                    lbl.ForeColor = Color.FromArgb(255, 248, 240);
                    break;

                case PictureBox pb:
                    pb.BackColor = pb.Parent?.BackColor ?? Color.FromArgb(102, 79, 66);
                    pb.Invalidate();
                    break;

                case CheckBox cb:
                    cb.ForeColor = Color.FromArgb(255, 248, 240);
                    cb.BackColor = Color.FromArgb(102, 79, 66);
                    break;

                case GroupBox gb:
                    gb.BackColor = Color.FromArgb(102, 79, 66);
                    gb.ForeColor = Color.FromArgb(255, 248, 240);
                    break;

                case TabControl tc:
                    tc.BackColor = Color.FromArgb(102, 79, 66);
                    break;

                case DataGridView dgv:
                    dgv.BackgroundColor = Color.FromArgb(90, 70, 60);
                    dgv.DefaultCellStyle.BackColor = Color.FromArgb(125, 98, 82);
                    dgv.DefaultCellStyle.ForeColor = Color.FromArgb(255, 248, 240);
                    dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(168, 135, 112);
                    dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(255, 248, 240);

                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(112, 88, 74);
                    dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(255, 248, 240);
                    dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(168, 135, 112);
                    dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(255, 248, 240);

                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(139, 106, 85);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(255, 248, 240);
                    dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(168, 135, 112);
                    dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(255, 248, 240);
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.Refresh();
                    break;

                case Chart chart when chart.ChartAreas.Count > 0:

                    var chartArea = chart.ChartAreas[0];
                   
                    var legend = chart.Legends.Count > 0 ? chart.Legends[0] : null;

                    chart.BackColor = Color.FromArgb(102, 79, 66);
                    chart.ForeColor = Color.FromArgb(255, 248, 240);
                 
                    chartArea.BackColor = Color.FromArgb(90, 70, 60);
                    chartArea.AxisX.LineColor = Color.FromArgb(255, 248, 240);
                    chartArea.AxisY.LineColor = Color.FromArgb(255, 248, 240);
                    chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(255, 248, 240);
                    chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(255, 248, 240);
                  
                    if (legend != null)
                    {
                        legend.BackColor = Color.FromArgb(102, 79, 66);
                        legend.ForeColor = Color.FromArgb(255, 248, 240);
                    }
                    
                    break;
            }
            foreach (Control child in ctrl.Controls)
            {
                TemaUygulaRecursive(child);
            }
        }

        public void OrijinalRenkleriGeriYukle(Control parent)
        {
            GeriYukleRecursive(parent);
        }

        private void GeriYukleRecursive(Control ctrl)
        {
            if (orijinalRenkler.ContainsKey(ctrl))
            {
                
                switch (ctrl)
                {
                    case Button btn when orijinalButtonStyles.ContainsKey(btn):
                        var btnStyle = orijinalButtonStyles[btn];
                        btn.BackColor = btnStyle.BackColor;
                        btn.ForeColor = btnStyle.ForeColor;
                        btn.FlatStyle = btnStyle.FlatStyle;
                        btn.FlatAppearance.BorderSize = btnStyle.BorderSize;
                        btn.FlatAppearance.MouseOverBackColor = btnStyle.MouseOverBackColor;
                        btn.FlatAppearance.MouseDownBackColor = btnStyle.MouseDownBackColor;
                        break;

                    case DataGridView dgv when orijinalDataGridStyles.ContainsKey(dgv):
                        var dgvStyle = orijinalDataGridStyles[dgv];
                        dgv.BackgroundColor = dgvStyle.BackgroundColor;
                        dgv.DefaultCellStyle.BackColor = dgvStyle.DefaultBackColor;
                        dgv.DefaultCellStyle.ForeColor = dgvStyle.DefaultForeColor;
                        dgv.DefaultCellStyle.SelectionBackColor = dgvStyle.DefaultSelectionBackColor;
                        dgv.DefaultCellStyle.SelectionForeColor = dgvStyle.DefaultSelectionForeColor;
                        dgv.AlternatingRowsDefaultCellStyle.BackColor = dgvStyle.AlternatingBackColor;
                        dgv.AlternatingRowsDefaultCellStyle.ForeColor = dgvStyle.AlternatingForeColor;
                        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = dgvStyle.AlternatingSelectionBackColor;
                        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = dgvStyle.AlternatingSelectionForeColor;
                        dgv.ColumnHeadersDefaultCellStyle.BackColor = dgvStyle.HeaderBackColor;
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = dgvStyle.HeaderForeColor;
                        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvStyle.HeaderSelectionBackColor;
                        dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgvStyle.HeaderSelectionForeColor;
                        dgv.EnableHeadersVisualStyles = dgvStyle.EnableHeadersVisualStyles;
                        dgv.Refresh();
                        break;

                    case Chart chart when orijinalChartStyles.ContainsKey(chart) && chart.ChartAreas.Count > 0 :
                        var chartStyle = orijinalChartStyles[chart];
                        var chartArea = chart.ChartAreas[0];
                        var legend = chart.Legends.Count > 0 ? chart.Legends[0] : null;
                        
                        chart.BackColor = chartStyle.BackColor;
                        chart.ForeColor = chartStyle.ForeColor;
                        
    
                        chartArea.BackColor = chartStyle.ChartAreaBackColor;
                        chartArea.AxisX.LineColor = chartStyle.AxisLineColor;
                        chartArea.AxisY.LineColor = chartStyle.AxisLineColor;
                        chartArea.AxisX.LabelStyle.ForeColor = chartStyle.AxisLabelForeColor;
                        chartArea.AxisY.LabelStyle.ForeColor = chartStyle.AxisLabelForeColor;

                        if (legend != null)
                        {
                            legend.BackColor = chartStyle.LegendBackColor;
                            legend.ForeColor = chartStyle.LegendForeColor;
                        }
                        
                        break;

                    default:
                        
                        ctrl.BackColor = orijinalRenkler[ctrl].BackColor;
                        ctrl.ForeColor = orijinalRenkler[ctrl].ForeColor;
                        break;
                }
            }
            foreach (Control child in ctrl.Controls)
            {
                GeriYukleRecursive(child);
            }
        }
    }
}