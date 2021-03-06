﻿using System;
using System.Windows.Forms;
using WebMConverter.Dialogs;

namespace WebMConverter
{
    public partial class TrimForm : Form
    {
        private int trimStart = -1;
        private int trimEnd = -1;

        public TrimFilter GeneratedFilter;

        public TrimForm(TrimFilter FilterToEdit = null)
        {
            InitializeComponent();

            trackVideoTimeline.Maximum = Program.VideoSource.NumberOfFrames - 1;
            trackVideoTimeline.TickFrequency = trackVideoTimeline.Maximum / 60;
            labelTimeStamp.Text = Program.FrameToTimeStamp(trackVideoTimeline.Value);

            trackVideoTimeline.Focus();

            if (FilterToEdit == null)
            {
                trimStart = 0;
                trimEnd = Program.VideoSource.NumberOfFrames - 1;
            }
            else
            {
                trimStart = FilterToEdit.TrimStart;
                trimEnd = FilterToEdit.TrimEnd;
            }

            labelTrimStart.Text = string.Format("{0} ({1})", Program.FrameToTimeStamp(trimStart), trimStart.ToString());
            labelTrimEnd.Text = string.Format("{0} ({1})", Program.FrameToTimeStamp(trimEnd), trimEnd.ToString());

            trackVideoTimeline.Value = trimStart;
            previewFrame.Frame = trimStart;
            labelTimeStamp.Text = string.Format("{0} ({1})", Program.FrameToTimeStamp(trackVideoTimeline.Value), trackVideoTimeline.Value);

            checktrims();

            trackVideoTimeline.MouseWheel += trackVideoTimeline_MouseWheel;
        }

        private void buttonTrimStart_Click(object sender, EventArgs e)
        {
            trimStart = trackVideoTimeline.Value;
            labelTrimStart.Text = string.Format("{0} ({1})", Program.FrameToTimeStamp(trimStart), trimStart);
            checktrims();
            trackVideoTimeline_Focus(sender, e);
        }

        private void buttonTrimEnd_Click(object sender, EventArgs e)
        {
            trimEnd = trackVideoTimeline.Value;
            labelTrimEnd.Text = string.Format("{0} ({1})", Program.FrameToTimeStamp(trimEnd), trimEnd);
            checktrims();
            trackVideoTimeline_Focus(sender, e);
        }

        private void checktrims()
        {
            if (trimEnd < trimStart)
            {
                buttonConfirm.Enabled = false;
                return;
            }
            buttonConfirm.Enabled = true;

            int trimLength = trimEnd - trimStart;
            labelTrimDuration.Text = string.Format("{0} ({1})", Program.FrameToTimeStamp(trimLength), trimLength); // Using trimLength is actually kind of invalid, but meh.
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            GeneratedFilter = new TrimFilter(trimStart, trimEnd);

            Close();
        }

        private void toolStripMenuGoToFrame_Click(object sender, EventArgs e)
        {
            using (var dialog = new InputDialog<int>("Frame", trackVideoTimeline.Value))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SetFrame(dialog.Value);
                }
            }
        }

        private void ToolStripMenuGoToTime_Click(object sender, EventArgs e)
        {
            using (var dialog = new InputDialog<TimeSpan>("Time", Program.FrameToTimeSpan(trackVideoTimeline.Value)))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SetFrame(Program.TimeSpanToFrame(dialog.Value));
                }
            }
        }

        void trackVideoTimeline_MouseWheel(object sender, MouseEventArgs e)
        {
            int modifier = 0;
            if (e.Delta > 0)
                modifier = -1;
            else if (e.Delta < 0)
                modifier = 1;

            if (modifier != 0)
            {
                SetFrame(modifier, true);
            }

            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void TrimForm_KeyDown(object sender, KeyEventArgs e)
        {
            int modifier = 0;
            switch (e.KeyData)
            {
                case Keys.Left:
                    modifier = -1;
                    break;
                case Keys.Right:
                    modifier = 1;
                    break;
            }
            
            if (modifier != 0)
            {
                SetFrame(modifier, true);
                e.Handled = true;
            }
        }

        void SetFrame(int frame, bool modifier = false)
        {
            if (modifier)
                frame += trackVideoTimeline.Value;

            trackVideoTimeline.Value = Math.Max(0, Math.Min(trackVideoTimeline.Maximum, frame)); // Make sure we don't go out of bounds.
        }

        private void trackVideoTimeline_ValueChanged(object sender, EventArgs e)
        {
            previewFrame.Frame = trackVideoTimeline.Value;
            labelTimeStamp.Text = string.Format("{0} ({1})", Program.FrameToTimeStamp(trackVideoTimeline.Value), trackVideoTimeline.Value);
        }

        private void trackVideoTimeline_Focus(object sender, EventArgs e)
        {
            trackVideoTimeline.Focus();
        }
    }

    public class TrimFilter
    {
        public readonly int TrimStart;
        public readonly int TrimEnd;

        public TrimFilter(int TrimStart, int TrimEnd)
        {
            this.TrimStart = TrimStart;
            this.TrimEnd = TrimEnd;
        }

        public override string ToString()
        {
            return string.Format("Trim({0}, {1})", TrimStart, TrimEnd);
        }

        internal double GetDuration()
        {
            double firsttime, lasttime;

            firsttime = Program.FrameToTimeSpan(TrimStart).TotalSeconds;
            lasttime = Program.FrameToTimeSpan(TrimEnd).TotalSeconds;

            return lasttime - firsttime;
        }
    }
}
