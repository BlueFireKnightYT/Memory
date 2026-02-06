using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Memory
{
    public partial class Form1 : Form
    {
        Random random = new Random();
        List<string> icons = new List<string>() { "b", "b", "e", "e", "l", "l", "j", "j", "p", "p", "f", "f", "+", "+", "*", "*" };

        // persistent background player (COM) and temp file path
        private dynamic backgroundPlayer;
        private string musicTempPath;

        // keep active SFX SoundPlayer + stream alive until playback finishes
        private readonly List<(SoundPlayer player, MemoryStream stream, DateTime started)> activeSfx = new List<(SoundPlayer, MemoryStream, DateTime)>();
        private Timer sfxCleanupTimer;

        Label firstClicked, secondClicked;
        Timer hideTimer;

        int seconds = 0;
        int minutes = 0;

        int score = 0;
        int beurten = 0;

        public Form1()
        {
            InitializeComponent();
            AssignIcons();

            // initialize and loop background music via Windows Media Player COM object
            try
            {
                // write embedded music resource to a temp WAV file
                musicTempPath = Path.Combine(Path.GetTempPath(), "Memory_GroovyMusic.wav");
                using (var res = Properties.Resources.Music)
                using (var fs = new FileStream(musicTempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    res.Position = 0;
                    res.CopyTo(fs);
                }

                // create COM Windows Media Player instance without project reference
                var wmpType = Type.GetTypeFromProgID("WMPlayer.OCX");
                if (wmpType != null)
                {
                    backgroundPlayer = Activator.CreateInstance(wmpType);
                    backgroundPlayer.URL = musicTempPath;
                    backgroundPlayer.settings.setMode("loop", true);
                    backgroundPlayer.controls.play();
                }
            }
            catch (Exception)
            {
                // fallback verification beep if music fails to start
                SystemSounds.Beep.Play();
            }

            // timer to cleanup finished SFX streams
            sfxCleanupTimer = new Timer();
            sfxCleanupTimer.Interval = 1000;
            sfxCleanupTimer.Tick += SfxCleanupTimer_Tick;
            sfxCleanupTimer.Start();

            hideTimer = new Timer();
            hideTimer.Interval = 750;
            hideTimer.Tick += HideTimer_Tick;

            this.FormClosing += Form1_FormClosing;
        }

        private void SfxCleanupTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            // remove/dispose SFX entries older than 3s
            for (int i = activeSfx.Count - 1; i >= 0; i--)
            {
                var entry = activeSfx[i];
                if ((now - entry.started).TotalMilliseconds > 3000)
                {
                    try
                    {
                        entry.player.Stop();
                    }
                    catch { }
                    try
                    {
                        entry.stream.Dispose();
                    }
                    catch { }
                    activeSfx.RemoveAt(i);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // stop and release background player
            try
            {
                if (backgroundPlayer != null)
                {
                    backgroundPlayer.controls.stop();
                    Marshal.ReleaseComObject((object)backgroundPlayer);
                    backgroundPlayer = null;
                }
            }
            catch { }

            // delete temp music file if exists
            try
            {
                if (!string.IsNullOrEmpty(musicTempPath) && File.Exists(musicTempPath))
                {
                    File.Delete(musicTempPath);
                }
            }
            catch { }

            // cleanup SFX
            try
            {
                sfxCleanupTimer?.Stop();
                foreach (var entry in activeSfx)
                {
                    try { entry.player.Stop(); } catch { }
                    try { entry.stream.Dispose(); } catch { }
                }
                activeSfx.Clear();
            }
            catch { }
        }

        private void AssignIcons()
        {
            Label label;
            int randomNumber;

            for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
            {
                if (tableLayoutPanel1.Controls[i] is Label)
                {
                    label = (Label)tableLayoutPanel1.Controls[i];
                }
                else continue;

                randomNumber = random.Next(0, icons.Count);
                label.Text = icons[randomNumber];
                icons.RemoveAt(randomNumber);

                label.ForeColor = Color.SkyBlue;
            }
        }

        private void ChangeText(object sender, EventArgs e)
        {
            // Create a fresh MemoryStream + SoundPlayer for each click so SFX can overlap.
            try
            {
                var res = Properties.Resources.CardFlip; // UnmanagedMemoryStream from Resources.Designer
                var ms = new MemoryStream();
                res.Position = 0;
                res.CopyTo(ms);
                ms.Position = 0;

                var sp = new SoundPlayer(ms);
                sp.Play();

                // keep references until cleanup timer disposes them
                activeSfx.Add((sp, ms, DateTime.UtcNow));
            }
            catch (Exception)
            {
                SystemSounds.Beep.Play();
            }

            if (hideTimer.Enabled)
                return;

            Label clickedLabel = sender as Label;
            if (clickedLabel == null)
                return;

            if (clickedLabel.ForeColor == Color.Black)
                return;

            if (firstClicked == null)
            {
                firstClicked = clickedLabel;
                firstClicked.ForeColor = Color.Black;
                return;
            }

            if (clickedLabel == firstClicked)
                return;

            secondClicked = clickedLabel;
            secondClicked.ForeColor = Color.Black;

            hideTimer.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            seconds++;

            if (seconds > 60)
            {
                seconds = 0;
                minutes++;
            }

            if (score == 8)
            {
                timer1.Stop();
            }

            TimerLbl.Text = "Time: " + minutes.ToString() + ":" + seconds.ToString();
        }

        private void ScoreLbl_Click(object sender, EventArgs e)
        {

        }

        private void TimerLbl_Click(object sender, EventArgs e)
        {

        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            hideTimer.Stop();

            if (firstClicked != null && secondClicked != null)
            {
                if (firstClicked.Text != secondClicked.Text)
                {
                    firstClicked.ForeColor = firstClicked.BackColor;
                    secondClicked.ForeColor = secondClicked.BackColor;
                    beurten++;
                    BeurtenLBL.Text = "Beurt: " + beurten.ToString();
                }
                else
                {
                    score++;
                    ScoreLbl.Text = "Score: " + score.ToString();
                }

                firstClicked = null;
                secondClicked = null;
            }
        }
    }
}
