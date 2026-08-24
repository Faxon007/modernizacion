using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CefEditor
{
    public partial class Form1 : Form
    {
        private TextBox? txtPemPath;
        private TextBox? txtCefPath;
        private TextBox? txtJson;
        private ToolStripStatusLabel? lblStatus;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "CEF2 Config Editor";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(10) };
            
            // 1. CEF2 Path (Primero)
            var lblCef = new Label { Text = "1. Archivo .cef2:", Location = new Point(10, 15), AutoSize = true };
            txtCefPath = new TextBox { Location = new Point(180, 12), Width = 450, Anchor = AnchorStyles.Left | AnchorStyles.Top };
            var btnBrowseCef = new Button { Text = "Examinar...", Location = new Point(640, 10), Width = 100, Anchor = AnchorStyles.Left | AnchorStyles.Top };
            btnBrowseCef.Click += BtnBrowseCef_Click;

            // 2. PEM Path (Segundo)
            var lblPem = new Label { Text = "2. Llave PEM (Clave):", Location = new Point(10, 45), AutoSize = true };
            txtPemPath = new TextBox { Location = new Point(180, 42), Width = 450, Anchor = AnchorStyles.Left | AnchorStyles.Top };
            var btnBrowsePem = new Button { Text = "Examinar...", Location = new Point(640, 40), Width = 100, Anchor = AnchorStyles.Left | AnchorStyles.Top };
            btnBrowsePem.Click += BtnBrowsePem_Click;

            // 3. Actions (Tercero)
            var btnLoad = new Button { Text = "3. Desencriptar / Leer", Location = new Point(180, 75), Width = 150 };
            btnLoad.Click += BtnLoad_Click;
            
            var btnSave = new Button { Text = "Encriptar / Guardar", Location = new Point(340, 75), Width = 150 };
            btnSave.Click += BtnSave_Click;

            pnlTop.Controls.AddRange(new Control[] { lblCef, txtCefPath, btnBrowseCef, lblPem, txtPemPath, btnBrowsePem, btnLoad, btnSave });

            // Status Bar
            var statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel { Text = "Listo." };
            statusStrip.Items.Add(lblStatus);

            // JSON Editor
            txtJson = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 11f),
                WordWrap = false
            };

            this.Controls.Add(txtJson);
            this.Controls.Add(pnlTop);
            this.Controls.Add(statusStrip);
        }

        private void BtnBrowsePem_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "PEM Files (*.pem)|*.pem|All Files (*.*)|*.*" };
            if (ofd.ShowDialog() == DialogResult.OK)
                txtPemPath.Text = ofd.FileName;
        }

        private void BtnBrowseCef_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "CEF2 Files (*.cef2)|*.cef2|All Files (*.*)|*.*", CheckFileExists = false };
            if (ofd.ShowDialog() == DialogResult.OK)
                txtCefPath.Text = ofd.FileName;
        }

        private void BtnLoad_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(txtPemPath.Text)) throw new Exception("Seleccione un archivo PEM válido.");
                if (!File.Exists(txtCefPath.Text)) throw new Exception("Seleccione un archivo CEF2 válido.");

                var pemKey = File.ReadAllText(txtPemPath.Text).Trim();
                var encryptedBase64 = File.ReadAllText(txtCefPath.Text).Trim();

                var json = CryptoHelper.DecryptRawJson(encryptedBase64, pemKey);
                
                // Intentar formatear el JSON para que sea más legible
                try {
                    var parsedJson = System.Text.Json.JsonDocument.Parse(json);
                    json = System.Text.Json.JsonSerializer.Serialize(parsedJson, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                } catch { }

                txtJson.Text = json;
                lblStatus.Text = "Archivo desencriptado exitosamente.";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al desencriptar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error al desencriptar.";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(txtPemPath.Text)) throw new Exception("Seleccione un archivo PEM válido.");
                if (string.IsNullOrWhiteSpace(txtCefPath.Text)) throw new Exception("Seleccione una ruta destino para el archivo CEF2.");
                if (string.IsNullOrWhiteSpace(txtJson.Text)) throw new Exception("El contenido JSON no puede estar vacío.");

                // Validar que sea un JSON correcto antes de encriptar
                try {
                    System.Text.Json.JsonDocument.Parse(txtJson.Text);
                } catch {
                    throw new Exception("El contenido no es un JSON válido.");
                }

                var pemKey = File.ReadAllText(txtPemPath.Text).Trim();
                
                // Formateamos como minified para ahorrar espacio en el CEF2 (opcional, pero buena práctica)
                var parsedJson = System.Text.Json.JsonDocument.Parse(txtJson.Text);
                var minifiedJson = System.Text.Json.JsonSerializer.Serialize(parsedJson, new System.Text.Json.JsonSerializerOptions { WriteIndented = false });

                var encryptedBase64 = CryptoHelper.EncryptRawJson(minifiedJson, pemKey);

                File.WriteAllText(txtCefPath.Text, encryptedBase64);
                
                lblStatus.Text = "Archivo encriptado y guardado exitosamente.";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show("Archivo guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al encriptar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error al encriptar.";
                lblStatus.ForeColor = Color.Red;
            }
        }
    }
}
