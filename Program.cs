using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace PrinterUnlock
{
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

    public class MainForm : Form
    {
        private readonly ListView printers = new ListView();
        private readonly TextBox details = new TextBox();
        private readonly Label status = new Label();

        public MainForm()
        {
            Text = "PrinterUnlock - Windows 10 x64 Diagnostic";
            Width = 1040;
            Height = 700;
            MinimumSize = new Size(850, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(25, 28, 34);
            ForeColor = Color.White;

            var header = new Label {
                Text = "PRINTERUNLOCK",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 16)
            };
            Controls.Add(header);

            var subtitle = new Label {
                Text = "MG25xxx research • USB / Windows printer diagnostics • no firmware changes",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Silver,
                AutoSize = true,
                Location = new Point(25, 57)
            };
            Controls.Add(subtitle);

            var refresh = MakeButton("Scan again", 22, 92, 130);
            refresh.Click += (s, e) => Scan();
            Controls.Add(refresh);

            var export = MakeButton("Export report", 164, 92, 130);
            export.Click += (s, e) => ExportReport();
            Controls.Add(export);

            printers.Location = new Point(22, 140);
            printers.Size = new Size(475, 455);
            printers.View = View.Details;
            printers.FullRowSelect = true;
            printers.GridLines = true;
            printers.BackColor = Color.FromArgb(37, 42, 50);
            printers.ForeColor = Color.White;
            printers.Columns.Add("Printer", 255);
            printers.Columns.Add("Status", 190);
            printers.SelectedIndexChanged += (s, e) => ShowSelected();
            Controls.Add(printers);

            details.Location = new Point(515, 140);
            details.Size = new Size(485, 455);
            details.Multiline = true;
            details.ScrollBars = ScrollBars.Vertical;
            details.ReadOnly = true;
            details.Font = new Font("Consolas", 9);
            details.BackColor = Color.FromArgb(17, 20, 25);
            details.ForeColor = Color.Gainsboro;
            Controls.Add(details);

            status.Location = new Point(24, 615);
            status.Size = new Size(970, 28);
            status.ForeColor = Color.LightGreen;
            status.Text = "Ready. Connect the Canon printer by USB, turn it on, then scan.";
            Controls.Add(status);

            var note = new Label {
                Text = "This tool collects diagnostics only. It cannot override cartridge checks enforced inside printer firmware.",
                Location = new Point(24, 644),
                Size = new Size(970, 24),
                ForeColor = Color.Khaki,
                AutoSize = false
            };
            Controls.Add(note);
            Scan();
        }

        private Button MakeButton(string text, int x, int y, int w)
        {
            return new Button {
                Text = text, Location = new Point(x, y), Size = new Size(w, 32),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(55, 65, 78),
                ForeColor = Color.White
            };
        }

        private void Scan()
        {
            printers.Items.Clear();
            details.Clear();
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT Name, PrinterStatus, WorkOffline, DriverName, PortName, PNPDeviceID, Default FROM Win32_Printer"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject p in results)
                    {
                        string name = S(p, "Name");
                        string st = PrinterStatusText(p["PrinterStatus"], p["WorkOffline"]);
                        var item = new ListViewItem(name);
                        item.SubItems.Add(st);
                        item.Tag = p.Properties.Cast<PropertyData>().ToDictionary(
                            x => x.Name, x => p[x.Name] == null ? "" : p[x.Name].ToString());
                        printers.Items.Add(item);
                    }
                }

                status.Text = "Printer scan complete. " + printers.Items.Count + " Windows printer(s) found.";
                status.ForeColor = Color.LightGreen;
                if (printers.Items.Count > 0) printers.Items[0].Selected = true;
                else details.Text = "No installed Windows printers were found.\r\n\r\nCheck the USB cable, power, and Windows Device Manager.";
            }
            catch (Exception ex)
            {
                status.Text = "Scan error: " + ex.Message;
                status.ForeColor = Color.Orange;
            }

            AppendUsbCanonDevices();
        }

        private void AppendUsbCanonDevices()
        {
            try
            {
                var sb = new StringBuilder(details.Text);
                sb.AppendLine();
                sb.AppendLine("=== CANON / USB PLUG-AND-PLAY DEVICES ===");
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT Name, PNPDeviceID, Status, Manufacturer, ClassGuid FROM Win32_PnPEntity"))
                using (var results = searcher.Get())
                {
                    int count = 0;
                    foreach (ManagementObject d in results)
                    {
                        string name = S(d, "Name");
                        string id = S(d, "PNPDeviceID");
                        string maker = S(d, "Manufacturer");
                        if ((name + " " + id + " " + maker).IndexOf("Canon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            id.StartsWith("USB\\", StringComparison.OrdinalIgnoreCase) &&
                            name.IndexOf("printer", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            count++;
                            sb.AppendLine("Name: " + name);
                            sb.AppendLine("Manufacturer: " + maker);
                            sb.AppendLine("Status: " + S(d, "Status"));
                            sb.AppendLine("PNP ID: " + id);
                            sb.AppendLine("Class GUID: " + S(d, "ClassGuid"));
                            sb.AppendLine();
                        }
                    }
                    if (count == 0) sb.AppendLine("No Canon-labelled PnP device found. Check Device Manager > Universal Serial Bus controllers / Printers.");
                }
                details.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                details.AppendText("\r\nUSB device query failed: " + ex.Message);
            }
        }

        private void ShowSelected()
        {
            if (printers.SelectedItems.Count == 0) return;
            var data = printers.SelectedItems[0].Tag as Dictionary<string, string>;
            if (data == null) return;
            var sb = new StringBuilder();
            sb.AppendLine("=== SELECTED PRINTER ===");
            foreach (var key in new[] { "Name", "DriverName", "PortName", "PrinterStatus", "WorkOffline", "Default", "PNPDeviceID" })
                sb.AppendLine(key + ": " + (data.ContainsKey(key) ? data[key] : ""));
            sb.AppendLine();
            sb.AppendLine("=== INTERPRETATION ===");
            sb.AppendLine("This is Windows queue/Plug-and-Play information, not a direct readout of cartridge firmware.");
            sb.AppendLine("If the printer reports a missing-cartridge error before accepting a job, a Windows driver alone may not bypass it.");
            details.Text = sb.ToString();
            AppendUsbCanonDevices();
        }

        private void ExportReport()
        {
            using (var dlg = new SaveFileDialog { Filter = "Text report (*.txt)|*.txt", FileName = "PrinterUnlock_Report.txt" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("PrinterUnlock diagnostic report");
                    sb.AppendLine("Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sb.AppendLine("OS: " + Environment.OSVersion);
                    sb.AppendLine("64-bit OS: " + Environment.Is64BitOperatingSystem);
                    sb.AppendLine();
                    sb.AppendLine(details.Text);
                    File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                    status.Text = "Report saved: " + dlg.FileName;
                }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, "Export error"); }
            }
        }

        private static string S(ManagementBaseObject o, string prop)
        {
            try { return o[prop] == null ? "" : o[prop].ToString(); }
            catch { return ""; }
        }

        private static string PrinterStatusText(object status, object offline)
        {
            if (offline != null && offline.ToString().Equals("True", StringComparison.OrdinalIgnoreCase))
                return "Offline";
            if (status == null) return "Unknown";
            switch (status.ToString())
            {
                case "3": return "Idle / ready";
                case "4": return "Printing";
                case "5": return "Warming up";
                case "6": return "Stopped";
                case "7": return "Offline";
                default: return "Status code " + status;
            }
        }
    }
}
