using System;
using System.Text;
using System.Windows.Forms;

namespace OpreatingSystemClassDesign
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void RandomGenerate_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Random rd = new Random();
            for(int i = 0; i < 20; i++)
            {
                sb.Append(rd.Next(0,10));
                sb.Append(" ");
            }
            sb.Remove(sb.Length - 1, 1);
            InputText.Text = sb.ToString();
        }
        private void PageFaultTrack_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = sender as TrackBar;
            switch (tb.Name)
            {
                case "PageFaultTrack":
                    PageFaultTime.Text = tb.Value.ToString();
                    break;
                case "MemoryTrack":
                    MemoryTime.Text = tb.Value.ToString();
                    break;
                case "TLBTrack":
                    TLBTime.Text = tb.Value.ToString();
                    break;
                case "GeneraterNumTarck":
                    GeneraterNum.Text = tb.Value.ToString();
                    break;
                case "MemoryBlockTrack":
                    MemroyBlockNum.Text = tb.Value.ToString();
                    break;
            }
        }

        private void PageFaultTime_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            switch (tb.Name)
            {
                case "PageFaultTime":
                    TrackMoveByTextChanged(tb,PageFaultTrack,MoveMode.None);
                    break;
                case "MemoryTime":
                    TrackMoveByTextChanged(tb, MemoryTrack, MoveMode.None);
                    break;
                case "TLBTime":
                    TrackMoveByTextChanged(tb, TLBTrack, MoveMode.None);
                    break;
                case "GeneraterNum":
                    TrackMoveByTextChanged(tb, GeneraterNumTarck, MoveMode.None);
                    break;
                case "MemroyBlockNum":
                    TrackMoveByTextChanged(tb, MemoryBlockTrack, MoveMode.None);
                    break;
            }            
        }
        enum MoveMode
        {
            MinimalMode,
            None
        }
        private void TrackMoveByTextChanged(TextBox tb,TrackBar tr,MoveMode mode)
        {
            if (int.TryParse(tb.Text, out int value))
            {
                if (value <= tr.Maximum && value >= tr.Minimum)
                {
                    tr.Value = value;
                }
                else
                {
                    if (mode == MoveMode.MinimalMode)
                    {
                        tr.Value = tr.Minimum;
                        tb.Text = tr.Minimum.ToString();
                    }
                }
            }
            else
            {
                tb.Text = tr.Minimum.ToString();
            }
        }

        private void PageFaultTime_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            switch (tb.Name)
            {
                case "PageFaultTime":
                    TrackMoveByTextChanged(tb, PageFaultTrack, MoveMode.MinimalMode);
                    break;
                case "MemoryTime":
                    TrackMoveByTextChanged(tb, MemoryTrack, MoveMode.MinimalMode);
                    break;
                case "TLBTime":
                    TrackMoveByTextChanged(tb, TLBTrack, MoveMode.MinimalMode);
                    break;
                case "GeneraterNum":
                    TrackMoveByTextChanged(tb, GeneraterNumTarck, MoveMode.MinimalMode);
                    break;
                case "MemroyBlockNum":
                    TrackMoveByTextChanged(tb, MemoryBlockTrack, MoveMode.MinimalMode);
                    break;
            }
        }
    }
}
