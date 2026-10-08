using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace CoolEqualizer
{
    public class ModernToggle : Control
    {
        public bool Checked = true;
        public event EventHandler CheckedChanged;

        public ModernToggle()
        {
            this.Size = new Size(60, 30);
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            Invalidate();
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(this.Parent.BackColor);

            Rectangle r = new Rectangle(1, 1, this.Width - 3, this.Height - 3);
            int radius = this.Height / 2 - 2;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(r.X, r.Y, radius * 2, radius * 2, 90, 180);
                path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 180);
                path.CloseFigure();

                if (Checked)
                {
                    using (SolidBrush bg = new SolidBrush(Color.FromArgb(0, 180, 255))) 
                        g.FillPath(bg, path);
                        
                    using (SolidBrush thumb = new SolidBrush(Color.White))
                        g.FillEllipse(thumb, r.Right - (radius * 2) - 2, r.Y + 2, radius * 2 - 4, radius * 2 - 4);
                }
                else
                {
                    using (SolidBrush bg = new SolidBrush(Color.FromArgb(60, 60, 65)))
                        g.FillPath(bg, path);
                        
                    using (SolidBrush thumb = new SolidBrush(Color.FromArgb(160, 160, 160)))
                        g.FillEllipse(thumb, r.X + 2, r.Y + 2, radius * 2 - 4, radius * 2 - 4);
                }
            }
        }
    }

    public class EqPanel : Control
    {
        public float[] Values = new float[16];
        private float[] dragStartValues = new float[16];
        
        private float _preamp = 0f;
        public float Preamp { get { return _preamp; } set { _preamp = value; } }
        private float dragStartPreamp = 0f;

        public string[] FreqLabels = { "31", "50", "80", "125", "200", "315", "500", "800", "1.2k", "2k", "3.1k", "5k", "8k", "12k", "16k", "20k" };
        
        public bool IsStringEffect = false;
        public float LinkSigma = 3.0f; 
        public event EventHandler ValuesChanged;
        public event EventHandler EffectStateChanged;

        private int activeIndex = -1; // -2 means Preamp
        public bool isDragging = false;
        
        private int paddingTop = 65;
        private int paddingBottom = 65;
        private int paddingSide = 130; // EQ grid starts here

        public EqPanel()
        {
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        }

        private float GetValueFromY(int y)
        {
            int drawHeight = this.Height - paddingTop - paddingBottom;
            float ratio = 1f - (float)(y - paddingTop) / drawHeight;
            ratio = Math.Max(0, Math.Min(1, ratio));
            return -15f + ratio * 30f;
        }

        private int GetYFromValue(float val)
        {
            int drawHeight = this.Height - paddingTop - paddingBottom;
            float ratio = (val - (-15f)) / 30f;
            return paddingTop + drawHeight - (int)(ratio * drawHeight);
        }

        private int GetXFromIndex(int index)
        {
            int drawWidth = this.Width - paddingSide - 40;
            return paddingSide + (int)((float)index / 15 * drawWidth);
        }

        private int GetPreampX()
        {
            return 45; // Isolated far left
        }

        private int GetNearestIndex(int x)
        {
            if (x < 85) return -2; // Preamp zone

            int nearest = 0;
            int minDst = int.MaxValue;
            for (int i = 0; i < 16; i++)
            {
                int px = GetXFromIndex(i);
                int dst = Math.Abs(x - px);
                if (dst < minDst)
                {
                    minDst = dst;
                    nearest = i;
                }
            }
            return nearest;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            activeIndex = GetNearestIndex(e.X);
            isDragging = true;
            
            if (e.Button == MouseButtons.Right && activeIndex >= 0)
                IsStringEffect = true;
            else
                IsStringEffect = false;
                
            if (EffectStateChanged != null) EffectStateChanged(this, EventArgs.Empty);

            if (activeIndex == -2)
            {
                dragStartPreamp = Preamp;
            }
            else
            {
                for (int i = 0; i < 16; i++) dragStartValues[i] = Values[i];
            }
            
            UpdateDrag(e.Y);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging && activeIndex != -1)
            {
                UpdateDrag(e.Y);
            }
            else
            {
                int hoverIndex = GetNearestIndex(e.X);
                if (hoverIndex != activeIndex) Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
            activeIndex = -1;
            IsStringEffect = false;
            if (EffectStateChanged != null) EffectStateChanged(this, EventArgs.Empty);
            Invalidate();
            base.OnMouseUp(e);
        }

        private void UpdateDrag(int y)
        {
            float targetVal = GetValueFromY(y);
            targetVal = (float)Math.Round(targetVal * 2) / 2f;
            
            if (activeIndex == -2)
            {
                Preamp = targetVal;
            }
            else
            {
                float delta = targetVal - dragStartValues[activeIndex];

                for (int i = 0; i < 16; i++)
                {
                    if (i == activeIndex)
                    {
                        Values[i] = targetVal;
                    }
                    else if (IsStringEffect)
                    {
                        float distance = Math.Abs(activeIndex - i);
                        float weight = (float)Math.Exp(-(distance * distance) / (2 * LinkSigma * LinkSigma));
                        Values[i] = dragStartValues[i] + delta * weight;
                        Values[i] = Math.Max(-15f, Math.Min(15f, Values[i]));
                        Values[i] = (float)Math.Round(Values[i] * 10) / 10f; 
                    }
                }
            }
            
            if (ValuesChanged != null) ValuesChanged(this, EventArgs.Empty);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (LinearGradientBrush bgBrush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(10, 12, 18), Color.FromArgb(18, 22, 32), 90f))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            int zeroY = GetYFromValue(0f);
            
            using (Pen gridPen = new Pen(Color.FromArgb(20, 255, 255, 255)))
            using (Pen zeroPen = new Pen(Color.FromArgb(100, 0, 255, 255), 2f))
            using (Pen sepPen = new Pen(Color.FromArgb(40, 255, 255, 255), 2f))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(130, 150, 180)))
            {
                gridPen.DashStyle = DashStyle.Dash;
                float[] dbLines = { 15f, 10f, 5f, 0f, -5f, -10f, -15f };
                
                // Draw Separator between PRE and EQ
                g.DrawLine(sepPen, 85, paddingTop - 20, 85, this.Height - paddingBottom + 20);

                // Draw EQ Grid Lines & dB labels
                foreach (float db in dbLines)
                {
                    int y = GetYFromValue(db);
                    // Lines start AT paddingSide
                    g.DrawLine(db == 0f ? zeroPen : gridPen, paddingSide - 5, y, this.Width - 15, y);
                    // Labels sit between separator and grid
                    g.DrawString(db > 0 ? "+" + db : db.ToString(), this.Font, textBrush, 95, y - 8);
                }
                
                // --- PREAMP COLUMN (Isolated) ---
                int preX = GetPreampX();
                // Vertical track
                g.DrawLine(gridPen, preX, paddingTop, preX, this.Height - paddingBottom);
                // Zero line tick for Preamp
                g.DrawLine(zeroPen, preX - 10, zeroY, preX + 10, zeroY);
                
                SizeF szPre = g.MeasureString("PRE", this.Font);
                g.DrawString("PRE", this.Font, new SolidBrush(Color.FromArgb(255, 80, 150)), preX - szPre.Width / 2, this.Height - paddingBottom + 15);
                
                string preGainStr = Preamp > 0 ? "+" + Preamp.ToString("0.0") : Preamp.ToString("0.0");
                SizeF szPre2 = g.MeasureString(preGainStr, this.Font);
                g.DrawString(preGainStr, this.Font, new SolidBrush(Color.FromArgb(255, 80, 150)), preX - szPre2.Width / 2, paddingTop - 35);
                
                int preY = GetYFromValue(Preamp);
                using (SolidBrush preGlow = new SolidBrush(Color.FromArgb(100, 255, 50, 100)))
                    g.FillEllipse(preGlow, preX - 14, preY - 14, 28, 28);
                using (SolidBrush preBrush = new SolidBrush(Color.FromArgb(255, 80, 150)))
                {
                    int pr = (activeIndex == -2 && isDragging) ? 9 : 6;
                    g.FillEllipse(preBrush, preX - pr, preY - pr, pr * 2, pr * 2);
                }
                // --------------------------------

                // Draw EQ Vertical Lines & Bars
                for (int i = 0; i < 16; i++)
                {
                    int x = GetXFromIndex(i);
                    g.DrawLine(gridPen, x, paddingTop, x, this.Height - paddingBottom);
                    
                    int valY = GetYFromValue(Values[i]);
                    if (valY != zeroY)
                    {
                        Rectangle barRect = valY < zeroY 
                            ? new Rectangle(x - 4, valY, 8, zeroY - valY) 
                            : new Rectangle(x - 4, zeroY, 8, valY - zeroY);
                        using (SolidBrush barBrush = new SolidBrush(Color.FromArgb(40, 0, 255, 200)))
                        {
                            g.FillRectangle(barBrush, barRect);
                        }
                    }
                    
                    SizeF sz = g.MeasureString(FreqLabels[i], this.Font);
                    g.DrawString(FreqLabels[i], this.Font, textBrush, x - sz.Width / 2, this.Height - paddingBottom + 15);
                    
                    string gainStr = Values[i].ToString("0.0");
                    SizeF sz2 = g.MeasureString(gainStr, this.Font);
                    g.DrawString(gainStr, this.Font, new SolidBrush(Color.FromArgb(200, 220, 255)), x - sz2.Width / 2, paddingTop - 35);
                }
            }

            Point[] pts = new Point[16];
            for (int i = 0; i < 16; i++)
            {
                pts[i] = new Point(GetXFromIndex(i), GetYFromValue(Values[i]));
            }

            if (pts.Length >= 2)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddCurve(pts, 0.4f);
                path.AddLine(pts[15].X, zeroY, pts[0].X, zeroY);
                path.CloseFigure();

                using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                    new Point(0, paddingTop), new Point(0, this.Height - paddingBottom),
                    Color.FromArgb(90, 0, 220, 255), Color.FromArgb(0, 0, 100, 255)))
                {
                    g.FillPath(fillBrush, path);
                }

                using (Pen glowPen1 = new Pen(Color.FromArgb(30, 0, 255, 255), 10f))
                using (Pen glowPen2 = new Pen(Color.FromArgb(80, 0, 255, 255), 5f))
                using (Pen corePen = new Pen(Color.FromArgb(255, 255, 255, 255), 2.5f))
                {
                    g.DrawCurve(glowPen1, pts, 0.4f);
                    g.DrawCurve(glowPen2, pts, 0.4f);
                    g.DrawCurve(corePen, pts, 0.4f);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                int x = pts[i].X;
                int y = pts[i].Y;
                bool isTarget = (i == activeIndex && isDragging);
                
                if (isTarget)
                {
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(100, 0, 255, 150)))
                        g.FillEllipse(glow, x - 14, y - 14, 28, 28);
                    using (SolidBrush core = new SolidBrush(Color.FromArgb(0, 255, 150)))
                        g.FillEllipse(core, x - 7, y - 7, 14, 14);
                }
                else
                {
                    using (SolidBrush core = new SolidBrush(Color.FromArgb(220, 255, 255)))
                        g.FillEllipse(core, x - 5, y - 5, 10, 10);
                }
            }
            
            using (Pen borderPen = new Pen(Color.FromArgb(50, 80, 120), 2f))
            {
                g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }

    public class MainForm : Form
    {
        private const string ApoDir = @"C:\Program Files\EqualizerAPO\config";
        private const string EqFile = "cool_eq.txt";
        private const string ConfigFile = "config.txt";
        private const string SettingsFile = @"C:\Program Files\EqualizerAPO\config\cool_settings.txt"; 
        private const string PresetsFile = @"C:\Program Files\EqualizerAPO\config\cool_presets.txt";
        
        private EqPanel eqPanel;
        
        private ModernToggle chkEnable;
        private Label lblHint;
        private ComboBox comboPresets;
        private Button btnSavePreset;
        private Button btnDeletePreset;
        
        private int[] realFreqs = { 31, 50, 80, 125, 200, 315, 500, 800, 1250, 2000, 3150, 5000, 8000, 12500, 16000, 20000 };
        private Dictionary<string, float[]> presets = new Dictionary<string, float[]>();

        public MainForm()
        {
            this.Text = "Cool Equalizer (APO)";
            this.Size = new Size(1250, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(10, 12, 18);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            try {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            } catch { }

            InitDefaultPresets();
            LoadCustomPresets();
            InitUI();
            LoadLastSettings();
            EnsureApoInclude();
            ApplyEq();
        }

        private void InitDefaultPresets()
        {
            presets["Flat"] = new float[16];
            presets["Bass Boost"] = new float[] { 6, 6, 5, 5, 4, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            presets["Treble Boost"] = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 4, 5, 6, 6 };
            presets["V-Shape (Rock)"] = new float[] { 5, 5, 4, 3, 1, -1, -2, -3, -2, -1, 1, 3, 4, 5, 6, 6 };
            
            // Hardcoded User Presets
            presets["Nice1"] = new float[] { 14f, 13f, 11.3f, 8.8f, 5.9f, 3f, 0.8f, 0f, 0.8f, 3f, 5.9f, 8.8f, 11.3f, 13f, 14f, 14.6f };
            presets["Max1"] = new float[] { 14.8f, 14.6f, 13f, 10.5f, 6.8f, 2f, -2.6f, -4f, -3.9f, -1.1f, 2.7f, 6.1f, 9f, 10.6f, 11.8f, 12.5f };
            presets["NiceBass2"] = new float[] { 10.5f, 12.5f, 11.5f, 7.5f, 3.5f, -0.1f, -3.5f, -5f, -4.1f, -1.3f, 2.7f, 6.6f, 9.9f, 12.3f, 13.6f, 14.5f };
            presets["GoodBass1"] = new float[] { 10f, 15f, 15f, 8.9f, 4f, 0f, -2.7f, -5f, -4.2f, -1.6f, 1.6f, 4.1f, 5.7f, 6.9f, 7f, 6.6f };
            presets["Perfect"] = new float[] { 10f, 15f, 15f, 9.1f, 4.3f, 0.4f, -2.2f, -4.5f, -3.7f, -1.2f, 1.9f, 4.3f, 5.8f, 7f, 7f, 6.6f };
            presets["Perfect2"] = new float[] { 5f, 10f, 10f, 7f, 4.3f, 0.4f, -2.2f, -4.5f, -3.7f, -1.2f, 1.9f, 4.3f, 5.8f, 7f, 7f, 6.6f };
        }

        private void LoadCustomPresets()
        {
            if (File.Exists(PresetsFile))
            {
                foreach(var line in File.ReadAllLines(PresetsFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        var vals = parts[1].Split(',');
                        if (vals.Length == 16)
                        {
                            float[] presetVals = new float[16];
                            for(int i=0; i<16; i++) float.TryParse(vals[i], out presetVals[i]);
                            presets[parts[0]] = presetVals;
                        }
                    }
                }
            }
        }

        private void SaveCustomPresets()
        {
            List<string> lines = new List<string>();
            string[] defaults = { "Flat", "Bass Boost", "Treble Boost", "V-Shape (Rock)", "Nice1", "Max1", "NiceBass2", "GoodBass1", "Perfect", "Perfect2" };
            foreach(var kv in presets)
            {
                if (Array.IndexOf(defaults, kv.Key) >= 0) continue;
                lines.Add(string.Format("{0}|{1}", kv.Key, string.Join(",", kv.Value)));
            }
            File.WriteAllLines(PresetsFile, lines.ToArray());
        }

        private void SaveLastSettings()
        {
            string vals = string.Join(",", eqPanel.Values);
            string content = string.Format("{0}|{1}|{2}", eqPanel.Preamp, comboPresets.SelectedItem, vals);
            File.WriteAllText(SettingsFile, content);
        }

        private void LoadLastSettings()
        {
            if (File.Exists(SettingsFile))
            {
                try {
                    string[] parts = File.ReadAllText(SettingsFile).Split('|');
                    if (parts.Length >= 3)
                    {
                        float pre = 0;
                        float.TryParse(parts[0], out pre);
                        eqPanel.Preamp = pre;
                        
                        string lastPreset = parts[1];
                        if (presets.ContainsKey(lastPreset)) comboPresets.SelectedItem = lastPreset;

                        var vals = parts[2].Split(',');
                        if (vals.Length == 16)
                        {
                            for(int i=0; i<16; i++) float.TryParse(vals[i], out eqPanel.Values[i]);
                        }
                        
                        eqPanel.Invalidate();
                    }
                } catch {}
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveLastSettings();
            base.OnFormClosing(e);
        }

        private void InitUI()
        {
            Font topFont = new Font("Segoe UI", 11f, FontStyle.Bold);
            Font btnFont = new Font("Segoe UI", 11f);
            
            Panel topBar = new Panel() { Dock = DockStyle.Top, Height = 75, BackColor = Color.FromArgb(15, 18, 26) };
            this.Controls.Add(topBar);

            try {
                if (this.Icon != null)
                {
                    PictureBox pic = new PictureBox();
                    pic.Image = this.Icon.ToBitmap();
                    pic.SizeMode = PictureBoxSizeMode.Zoom;
                    pic.Size = new Size(50, 50);
                    pic.Location = new Point(15, 12);
                    topBar.Controls.Add(pic);
                }
            } catch {}

            chkEnable = new ModernToggle() { Location = new Point(75, 22), Checked = true };
            chkEnable.CheckedChanged += (s, e) => ApplyEq();
            topBar.Controls.Add(chkEnable);

            Label lblEqText = new Label() { Text = "EQ", Location = new Point(140, 26), AutoSize = true, Font = topFont, ForeColor = Color.White };
            topBar.Controls.Add(lblEqText);

            lblHint = new Label() { 
                Text = "⚪ Chuột Trái: Chỉnh đơn  |  🟢 Chuột Phải: Kéo dây đàn", 
                Location = new Point(220, 26), 
                AutoSize = true, 
                Font = topFont,
                ForeColor = Color.Gray
            };
            topBar.Controls.Add(lblHint);

            Label lblPreset = new Label() { Text = "Preset:", Location = new Point(620, 26), AutoSize = true, Font = topFont };
            topBar.Controls.Add(lblPreset);

            comboPresets = new ComboBox() { Location = new Point(685, 23), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = topFont };
            comboPresets.BackColor = Color.FromArgb(30, 35, 45);
            comboPresets.ForeColor = Color.White;
            comboPresets.FlatStyle = FlatStyle.Flat;
            RefreshPresetCombo();
            comboPresets.SelectedIndexChanged += ComboPresets_SelectedIndexChanged;
            topBar.Controls.Add(comboPresets);

            btnSavePreset = new Button() { Text = "Lưu Mới", Location = new Point(920, 21), Width = 100, Height = 34, Font = btnFont, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 150, 255) };
            btnSavePreset.FlatAppearance.BorderSize = 0;
            btnSavePreset.Click += BtnSavePreset_Click;
            topBar.Controls.Add(btnSavePreset);

            btnDeletePreset = new Button() { Text = "Xóa", Location = new Point(1035, 21), Width = 80, Height = 34, Font = btnFont, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(200, 50, 70) };
            btnDeletePreset.FlatAppearance.BorderSize = 0;
            btnDeletePreset.Click += BtnDeletePreset_Click;
            topBar.Controls.Add(btnDeletePreset);

            eqPanel = new EqPanel()
            {
                Location = new Point(15, 90),
                Size = new Size(1200, 450),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            eqPanel.ValuesChanged += (s, e) => {
                comboPresets.SelectedIndex = -1; 
                ApplyEq();
            };
            eqPanel.EffectStateChanged += (s, e) => {
                if (eqPanel.isDragging)
                {
                    if (eqPanel.IsStringEffect)
                    {
                        lblHint.Text = "🟢 Chuột Phải: Đang kéo dây đàn...";
                        lblHint.ForeColor = Color.FromArgb(0, 255, 150);
                    }
                    else
                    {
                        lblHint.Text = "⚪ Chuột Trái: Đang chỉnh đơn...";
                        lblHint.ForeColor = Color.FromArgb(0, 200, 255);
                    }
                }
                else
                {
                    lblHint.Text = "⚪ Chuột Trái: Chỉnh đơn  |  🟢 Chuột Phải: Kéo dây đàn";
                    lblHint.ForeColor = Color.Gray;
                }
            };
            this.Controls.Add(eqPanel);
        }

        private void RefreshPresetCombo()
        {
            string current = comboPresets.SelectedItem != null ? comboPresets.SelectedItem.ToString() : null;
            comboPresets.Items.Clear();
            foreach (var p in presets.Keys) comboPresets.Items.Add(p);
            if (current != null && presets.ContainsKey(current))
                comboPresets.SelectedItem = current;
        }

        private bool isLoadingPreset = false;

        private void ComboPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboPresets.SelectedItem == null) return;
            string sel = comboPresets.SelectedItem.ToString();
            if (presets.ContainsKey(sel))
            {
                isLoadingPreset = true;
                float[] vals = presets[sel];
                for (int i = 0; i < 16; i++) eqPanel.Values[i] = vals[i];
                eqPanel.Invalidate();
                isLoadingPreset = false;
                ApplyEq();
            }
        }

        private void BtnSavePreset_Click(object sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên Preset mới:", "Lưu Preset", "My Custom EQ");
            if (!string.IsNullOrWhiteSpace(name))
            {
                float[] currentVals = new float[16];
                for (int i = 0; i < 16; i++) currentVals[i] = eqPanel.Values[i];
                presets[name] = currentVals;
                SaveCustomPresets();
                RefreshPresetCombo();
                comboPresets.SelectedItem = name;
            }
        }

        private void BtnDeletePreset_Click(object sender, EventArgs e)
        {
            if (comboPresets.SelectedItem == null) return;
            string sel = comboPresets.SelectedItem.ToString();
            if (sel == "Flat" || sel == "Bass Boost" || sel == "Treble Boost" || sel == "V-Shape (Rock)")
            {
                MessageBox.Show("Không thể xóa preset mặc định!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (MessageBox.Show(string.Format("Bạn có chắc chắn muốn xóa preset '{0}'?", sel), "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                presets.Remove(sel);
                SaveCustomPresets();
                RefreshPresetCombo();
            }
        }

        private void EnsureApoInclude()
        {
            try
            {
                string mainPath = Path.Combine(ApoDir, ConfigFile);
                if (File.Exists(mainPath))
                {
                    string content = File.ReadAllText(mainPath);
                    if (!content.Contains("Include: " + EqFile))
                    {
                        File.AppendAllText(mainPath, "\r\nInclude: " + EqFile + "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi phân quyền khi đọc Equalizer APO config.\n" + ex.Message, "Lỗi");
            }
        }

        private void ApplyEq()
        {
            if (isLoadingPreset) return;
            
            try
            {
                string filePath = Path.Combine(ApoDir, EqFile);
                if (!chkEnable.Checked)
                {
                    File.WriteAllText(filePath, "");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                if (eqPanel.Preamp != 0)
                {
                    sb.Append("Preamp: ").Append(eqPanel.Preamp.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)).Append(" dB\r\n");
                }
                
                sb.Append("GraphicEQ: ");
                for (int i = 0; i < 16; i++)
                {
                    string valStr = eqPanel.Values[i].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                    sb.Append(realFreqs[i]).Append(" ").Append(valStr);
                    if (i < 15) sb.Append("; ");
                }
                
                File.WriteAllText(filePath, sb.ToString());
            }
            catch
            {
            }
        }
    }
    
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
