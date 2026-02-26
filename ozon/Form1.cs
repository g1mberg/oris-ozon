using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace ozon
{
    public partial class Form1 : Form
    {
        private readonly OzonApiClient _client = new OzonApiClient();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e) => StartSearch();

        private async void StartSearch()
        {
            var query = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            btnSearch.Enabled = false;

            flowPanel.Controls.Clear();
            flowPanel.Height = 0;


            var products = await _client.SearchAsync(query);
            
            flowPanel.SuspendLayout();
            foreach (var p in products)
            {
                var card = new Panel
                {
                    Width = 420,
                    Height = 110,
                    BackColor = Color.White,
                    Margin = new Padding(10),
                    Padding = new Padding(12)
                };

                var lblName = new Label
                {
                    Text = p.Name,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    AutoSize = false,
                    Width = 396,
                    Height = 50,
                    Location = new Point(12, 10)
                };

                var lblPrice = new Label
                {
                    Text = string.IsNullOrEmpty(p.Price) ? "—" : p.Price,
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 91, 204),
                    AutoSize = true,
                    Location = new Point(12, 62)
                };

                var lnk = new LinkLabel
                {
                    Text = "Открыть на Ozon",
                    Font = new Font("Segoe UI", 8.5f),
                    AutoSize = true,
                    Location = new Point(12, 86),
                    Tag = p.Url
                };
                lnk.LinkClicked += (s, _) => Process.Start((string)((LinkLabel)s).Tag);

                card.Controls.Add(lblName);
                card.Controls.Add(lblPrice);
                card.Controls.Add(lnk);
                flowPanel.Controls.Add(card);
            }
            flowPanel.ResumeLayout();
            btnSearch.Enabled = true;
        }
    }
}
