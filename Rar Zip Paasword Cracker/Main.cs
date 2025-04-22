using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Rar_Zip_Paasword_Cracker
{
    public partial class Main : Form
    {
        [DllImport("user32")]
        public static extern void LockWorkStation();
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
      (
          int nLeftRect,
          int nTopRect,
          int nRightRect,
          int nBottomRect,
          int nWidthEllipse,
          int nHeightEllipse
      );

        public Main()
        {
            InitializeComponent();

            // Enable drag-and-drop functionality
            Wordlist.AllowDrop = true;
            Archive.AllowDrop = true;

            // Assign drag-and-drop event handlers
            Wordlist.DragEnter += new DragEventHandler(Wordlist_DragEnter);
            Wordlist.DragDrop += new DragEventHandler(Wordlist_DragDrop);

            Archive.DragEnter += new DragEventHandler(Archive_DragEnter);
            Archive.DragDrop += new DragEventHandler(Archive_DragDrop);
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            new Guna.UI2.WinForms.Guna2ShadowForm(this)
            {
                ShadowColor = Color.Red
            };
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Archive.Text) || !File.Exists(Wordlist.Text))
            {
                MessageBox.Show("Invalid archive or wordlist file.");
                return;
            }

            // Create the "Cracked" folder in the application's directory
            string crackedFolderPath = Path.Combine(Application.StartupPath, "Cracked");
            if (!Directory.Exists(crackedFolderPath))
            {
                Directory.CreateDirectory(crackedFolderPath);
            }

            // Read the wordlist
            string[] passwords = File.ReadAllLines(Wordlist.Text);
            bool foundPassword = false;
            string crackedPassword = "";

            // Determine the archive format based on the file extension
            string fileExtension = Path.GetExtension(Archive.Text).ToLower();

            // Use parallel processing to speed up the password cracking
            Parallel.ForEach(passwords, password =>
            {
                try
                {
                    Process proc = new Process();
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;

                    // Automatically locate the required executable
                    switch (fileExtension)
                    {
                        case ".zip":
                        case ".7z":
                            proc.StartInfo.FileName = FindExecutable("7z.exe");
                            if (string.IsNullOrEmpty(proc.StartInfo.FileName))
                            {
                                MessageBox.Show("7z.exe not found on the system.");
                                return;
                            }
                            proc.StartInfo.Arguments = $"x \"{Archive.Text}\" -p{password} -o\"{crackedFolderPath}\" -y";
                            break;

                        case ".rar":
                            proc.StartInfo.FileName = FindExecutable("rar.exe");
                            if (string.IsNullOrEmpty(proc.StartInfo.FileName))
                            {
                                MessageBox.Show("rar.exe not found on the system.");
                                return;
                            }
                            proc.StartInfo.Arguments = $"x -p{password} \"{Archive.Text}\" \"{crackedFolderPath}\\\"";
                            break;

                        default:
                            MessageBox.Show("Unsupported archive format.");
                            return;
                    }

                    proc.Start();
                    proc.WaitForExit();

                    if (proc.ExitCode == 0) // If the extraction was successful
                    {
                        foundPassword = true;
                        crackedPassword = password;
                        lock (this) // Use a lock to ensure thread safety
                        {
                            if (foundPassword)
                            {
                                MessageBox.Show($"Password cracked: {crackedPassword}\nFiles extracted to: {crackedFolderPath}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing password '{password}': {ex.Message}");
                }
            });
        }


        private void Wordlist_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Wordlist_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                Wordlist.Text = files[0];
            }
        }

        private void Archive_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Archive_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                Archive.Text = files[0];
            }
        }

        private string FindExecutable(string executableName)
        {
            // Check if the executable is in the PATH environment variable
            string[] paths = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string path in paths)
            {
                string fullPath = Path.Combine(path, executableName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // Search common installation directories
            string[] commonDirs = {
                @"C:\Program Files\7-Zip",
                @"C:\Program Files (x86)\7-Zip",
                @"C:\Program Files\WinRAR",
                @"C:\Program Files (x86)\WinRAR"
            };

            foreach (string dir in commonDirs)
            {
                string fullPath = Path.Combine(dir, executableName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // Return null if not found
            return null;
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            this.Archive.Text = "";
            this.Wordlist.Text = "";
        }
    }
}