using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OpreatingSystemClassDesign
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        #region ---私有变量---
        private Thread Thread_FIFO;
        private Thread Thread_LRU;
        private Thread Thread_OPT;
        private List<int> InputAddresses;
        private int count_FIFO = 1;
        #endregion
        #region ---公有静态变量---
        public static int PageFaultTime = 500;
        public static int MemoryTime = 5;
        public static int TLBTime = 5;
        public static int MemoryBlockNum = 4;
        public static int GeneratorNum = 5;
        #endregion
        private void MainForm_Load(object sender, EventArgs e)
        {
            DataGridViewColumn column = new DataGridViewColumn()
            {
                Width = 60,
                Name = Guid.NewGuid().ToString(),
                HeaderText = "页框号",
                CellTemplate = new DataGridViewTextBoxCell()
            };
            dataGridView_FIFO.Columns.Add(column);
            for (int i = 0; i < 4; i++)
            {
                dataGridView_FIFO.Rows.Add((i + 1).ToString());
            }
            dataGridView_FIFO.Rows.Add("缺页情况");
            CellsUnselected();
        }

        private void CellsUnselected()
        {
            foreach (DataGridViewCell item in dataGridView_FIFO.SelectedCells)
            {
                item.Selected = false;
            }
        }

        private void RandomGenerate_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < GeneratorNum; i++)
            {
                if (GenerateLogicAddress.Checked)
                {
                    string tmp = rd.Next(0, 0xFFFF).ToString("X0") + "H";
                    if (tmp.Length < 5)
                    {
                        tmp = new string('0', 5 - tmp.Length) + tmp;
                    }
                    sb.Append(tmp);
                }
                else
                {
                    sb.Append(rd.Next(0,15).ToString("X0"));
                }
                sb.Append(" ");
            }
            sb.Remove(sb.Length - 1, 1);
            InputText.Text = sb.ToString();
            InputAddresses = Helper.ReadAddress(InputText.Text);
            UpdateDataGrid();
        }

        private void UpdateDataGrid()
        {
            while (dataGridView_FIFO.Columns.Count != 1)
            {
                dataGridView_FIFO.Columns.RemoveAt(1);
            }
            List<int> tmpList = new List<int>();
            foreach (var item in InputAddresses)
            {
                tmpList.Add(Helper.GetPageBlock(item));
            }
            foreach (var item in tmpList)
            {
                DataGridViewColumn column = new DataGridViewColumn()
                {
                    Width = 30,
                    Name = Guid.NewGuid().ToString(),
                    HeaderText = item.ToString("X0"),
                    CellTemplate = new DataGridViewTextBoxCell()
                };
                dataGridView_FIFO.Columns.Add(column);
            }
            CellsUnselected();
        }

        private void PageFaultTrack_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = sender as TrackBar;
            switch (tb.Name)
            {
                case "PageFaultTrack":
                    PageFaultTime = tb.Value;
                    PageFaultTime_TextBox.Text = tb.Value.ToString();
                    break;
                case "MemoryTrack":
                    MemoryTime = tb.Value;
                    MemoryTime_TextBox.Text = tb.Value.ToString();
                    break;
                case "TLBTrack":
                    TLBTime = tb.Value;
                    TLBTime_TextBox.Text = tb.Value.ToString();
                    break;
                case "GeneratorNumTarck":
                    GeneratorNum = tb.Value;
                    GeneratorNum_TextBox.Text = tb.Value.ToString();
                    break;
                case "MemoryBlockTrack":
                    MemoryBlockNum = tb.Value;
                    MemroyBlockNum_TextBox.Text = tb.Value.ToString();
                    break;
            }
        }

        private void PageFaultTime_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            switch (tb.Name)
            {
                case "PageFaultTime_TextBox":
                    TrackMoveByTextChanged(tb, PageFaultTrack, Models.MoveMode.None);
                    break;
                case "MemoryTime_TextBox":
                    TrackMoveByTextChanged(tb, MemoryTrack, Models.MoveMode.None);
                    break;
                case "TLBTime_TextBox":
                    TrackMoveByTextChanged(tb, TLBTrack, Models.MoveMode.None);
                    break;
                case "GeneratorNum_TextBox":
                    TrackMoveByTextChanged(tb, GeneratorNumTarck, Models.MoveMode.None);
                    break;
                case "MemroyBlockNum_TextBox":
                    TrackMoveByTextChanged(tb, MemoryBlockTrack, Models.MoveMode.MinimalMode);
                    dataGridView_FIFO.Rows.Clear();
                    for (int i = 0; i < MemoryBlockNum; i++)
                    {
                        dataGridView_FIFO.Rows.Add((i + 1).ToString());
                    }
                    dataGridView_FIFO.Rows.Add("缺页情况");
                    CellsUnselected();
                    break;
            }
        }
        private void TrackMoveByTextChanged(TextBox tb, TrackBar tr, Models.MoveMode mode)
        {
            if (int.TryParse(tb.Text, out int value))
            {
                if (value <= tr.Maximum && value >= tr.Minimum)
                {
                    tr.Value = value;
                }
                else
                {
                    if (mode == Models.MoveMode.MinimalMode)
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
                case "PageFaultTime_TextBox":
                    TrackMoveByTextChanged(tb, PageFaultTrack, Models.MoveMode.MinimalMode);
                    break;
                case "MemoryTime_TextBox":
                    TrackMoveByTextChanged(tb, MemoryTrack, Models.MoveMode.MinimalMode);
                    break;
                case "TLBTime_TextBox":
                    TrackMoveByTextChanged(tb, TLBTrack, Models.MoveMode.MinimalMode);
                    break;
                case "GeneratorNum_TextBox":
                    TrackMoveByTextChanged(tb, GeneratorNumTarck, Models.MoveMode.MinimalMode);
                    break;
                case "MemroyBlockNum_TextBox":
                    TrackMoveByTextChanged(tb, MemoryBlockTrack, Models.MoveMode.MinimalMode);
                    break;
            }
        }

        private void FIFO_Start_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridView_FIFO.Rows)
            {
                for (int i = 1; i < item.Cells.Count; i++)
                {
                    item.Cells[i].Value = "";
                }
            }
            DisableState();
            count_FIFO = 1;
            GlobalVariable.MemoryQueue_FIFO = new Queue<MemoryBlock>();
            GlobalVariable.Memory_FIFO = new Dictionary<int, int>();
            for (int i = 0; i < MemoryBlockNum; i++)
            {
                GlobalVariable.Memory_FIFO.Add(i + 1, -1);
            }
            if (InputAddresses == null || InputAddresses.Count == 0)
            {
                if(string.IsNullOrWhiteSpace(InputText.Text))
                {
                    RandomGenerate.PerformClick();
                }
                else
                {
                    InputAddresses = Helper.ReadAddress(InputText.Text);
                    UpdateDataGrid();
                }
            }
            int pageFaultCount = 0;
            int timeSpent = 0;
            Thread_FIFO = new Thread(() =>
            {   
                foreach (var item in InputAddresses)
                {
                    bool flag = Arithmetic.MakeFIFO(item, out int blockNum);
                    UpdateDGV(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum);
                    if(flag)
                    {
                        pageFaultCount++;
                    }
                }
                groupBox1.Invoke(new MethodInvoker(()=>
                {
                    EnableState();
                    FIFO_Result.Text = $"共用时间：{timeSpent}ms 缺页次数：{pageFaultCount}/{InputAddresses.Count} " +
                            $"缺页率：{pageFaultCount / (double)InputAddresses.Count * 100:f2}%";
                    FIFO_Result.Visible = true;
                }));
            });
            Thread_FIFO.Start();            
        }
        private void DisableState()
        {
            TimeSetting_GroupBox.Enabled = false;
            OtherSettings_GroupBox.Enabled = false;
            FIFO_Start.Enabled = false;
            FIFO_Pause.Enabled = true;
        }
        private void EnableState()
        {
            TimeSetting_GroupBox.Enabled = true;
            OtherSettings_GroupBox.Enabled = true;
            FIFO_Start.Enabled = true;
            FIFO_Pause.Enabled = false;
        }
        private void UpdateDGV(DataGridView DGV, Models.ArithmeticType type, bool flag, int blockNum = 0)
        {
            int count = 0;
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    foreach (var item in GlobalVariable.Memory_FIFO)
                    {
                        if (item.Value != -1)
                            count++;
                    }
                    for (int i = 0; i < MemoryBlockNum; i++)
                    {
                        if (i < count)
                        {
                            dataGridView_FIFO.Rows[i].Cells[count_FIFO].Value = GlobalVariable.Memory_FIFO[i + 1].ToString("X0");
                        }
                        if (i == blockNum - 1)
                        {
                            dataGridView_FIFO.Rows[i].Cells[count_FIFO].Style.ForeColor = Color.Red;
                        }
                    }
                    if(flag)
                    {
                        dataGridView_FIFO.Rows[MemoryBlockNum].Cells[count_FIFO].Value = "√";
                    }
                    else
                    {
                        dataGridView_FIFO.Rows[MemoryBlockNum].Cells[count_FIFO].Value = "×";
                    }
                    count_FIFO++;
                    break;
                case Models.ArithmeticType.LRU:

                    break;
                case Models.ArithmeticType.OPT:

                    break;
            }
        }

        private void InputText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                InputAddresses = Helper.ReadAddress(InputText.Text);
                UpdateDataGrid();
            }
        }

        private void dataGridView_FIFO_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CellsUnselected();
        }
    }
}
