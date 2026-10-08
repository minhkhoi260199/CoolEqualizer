using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CoolEqualizerInstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Require Admin rights for writing to Program Files
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent()) {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated) {
                try {
                    ProcessStartInfo psi = new ProcessStartInfo(Application.ExecutablePath);
                    psi.Verb = "runas";
                    Process.Start(psi);
                } catch { 
                    MessageBox.Show("This installer requires Administrator privileges to install Equalizer APO.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return; // Exit current process
            }

            Application.Run(new InstallerForm());
        }
    }

    public class InstallerForm : Form
    {
        private Button btnInstall;
        private Label lblStatus;
        private ProgressBar pb;

        public InstallerForm()
        {
            this.Text = "Cool Equalizer (APO) - All In One Setup";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 25, 35);
            this.ForeColor = Color.White;
            
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch {}

            Label lblTitle = new Label() { Text = "Cool Equalizer Installation", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitle);
            
            Label lblDesc = new Label() { Text = "This will install Equalizer APO and the Cool Equalizer GUI.", Font = new Font("Segoe UI", 10), Location = new Point(23, 60), AutoSize = true, ForeColor = Color.LightGray };
            this.Controls.Add(lblDesc);

            pb = new ProgressBar() { Location = new Point(25, 100), Size = new Size(430, 15), Style = ProgressBarStyle.Continuous };
            this.Controls.Add(pb);

            lblStatus = new Label() { Text = "Ready to install.", Location = new Point(22, 125), AutoSize = true, Font = new Font("Segoe UI", 9) };
            this.Controls.Add(lblStatus);

            btnInstall = new Button() { Text = "Install Now", Location = new Point(335, 150), Size = new Size(120, 40), BackColor = Color.FromArgb(0, 150, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;
            this.Controls.Add(btnInstall);
        }

        private async void BtnInstall_Click(object sender, EventArgs e)
        {
            btnInstall.Enabled = false;
            
            await Task.Run(() => {
                try {
                    UpdateStatus("Creating installation directory...", 10);
                    string targetDir = @"C:\Program Files\EqualizerAPO";
                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                    UpdateStatus("Extracting Cool Equalizer...", 30);
                    string exePath = Path.Combine(targetDir, "CoolEqualizer.exe");
                    ExtractResource("CoolEqualizer.exe", exePath);

                    UpdateStatus("Creating Desktop Shortcut...", 50);
                    CreateShortcut(exePath);

                    UpdateStatus("Extracting Equalizer APO Installer...", 70);
                    string apoInstaller = Path.Combine(Path.GetTempPath(), "EqualizerAPO_Setup.exe");
                    ExtractResource("EqualizerAPO-x64-1.4.2.exe", apoInstaller);

                    UpdateStatus("Launching APO Installer...", 90);
                    Process.Start(apoInstaller);
                    
                    UpdateStatus("Done! Please complete the APO installation.", 100);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Install Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            
            MessageBox.Show("Cool Equalizer has been placed on your Desktop and Start Menu.\n\nPlease proceed with the Equalizer APO installation window that just opened.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        private void UpdateStatus(string text, int progress)
        {
            this.Invoke((MethodInvoker)delegate {
                lblStatus.Text = text;
                pb.Value = progress;
            });
        }

        private void ExtractResource(string resourceName, string outPath)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string fullResName = "";
            foreach(string name in asm.GetManifestResourceNames()) {
                if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase)) {
                    fullResName = name;
                    break;
                }
            }
            if (fullResName == "") throw new Exception("Resource not found: " + resourceName);
            
            using (Stream s = asm.GetManifestResourceStream(fullResName))
            using (FileStream fs = new FileStream(outPath, FileMode.Create))
            {
                s.CopyTo(fs);
            }
        }

        private void CreateShortcut(string targetPath)
        {
            try {
                Type t = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(t);
                
                // Desktop Shortcut
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                dynamic shortcutDesk = shell.CreateShortcut(desktop + "\\Cool Equalizer.lnk");
                shortcutDesk.TargetPath = targetPath;
                shortcutDesk.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcutDesk.IconLocation = targetPath;
                shortcutDesk.Save();

                // Start Menu Shortcut
                string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms); // All Users Start Menu
                dynamic shortcutStart = shell.CreateShortcut(startMenu + "\\Cool Equalizer.lnk");
                shortcutStart.TargetPath = targetPath;
                shortcutStart.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcutStart.IconLocation = targetPath;
                shortcutStart.Save();
            } catch { }
        }
    }
}
