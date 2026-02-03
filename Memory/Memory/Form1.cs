using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Memory
{
    public partial class Form1 : Form
    {
        Random random = new Random();
        List<string> icons = new List<string>() { "b", "b", "e", "e", "l", "l", "j", "j", "p", "p", "f", "f", "+", "+", "*", "*" };

        Label firstClicked, secondClicked;
        Timer hideTimer;

        public Form1()
        {
            InitializeComponent();
            AssignIcons();

            hideTimer = new Timer();
            hideTimer.Interval = 750;
            hideTimer.Tick += HideTimer_Tick;
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

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            hideTimer.Stop();

            if (firstClicked != null && secondClicked != null)
            {
                if (firstClicked.Text != secondClicked.Text)
                {
                    firstClicked.ForeColor = firstClicked.BackColor;
                    secondClicked.ForeColor = secondClicked.BackColor;
                }

                firstClicked = null;
                secondClicked = null;
            }
        }
    }
}
