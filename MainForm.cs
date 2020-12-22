using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        /// <summary>
        /// FIFO算法的线程，用于挂起与恢复
        /// </summary>
        private Thread Thread_FIFO;
        /// <summary>
        /// LRU算法的线程，用于挂起与恢复
        /// </summary>
        private Thread Thread_LRU;
        /// <summary>
        /// OPT算法的线程，用于挂起与恢复
        /// </summary>
        private Thread Thread_OPT;
        /// <summary>
        /// 输入的地址数组
        /// </summary>
        private List<int> InputAddresses;
        /// <summary>
        /// FIFO算法进行的步数
        /// </summary>
        private int count_FIFO = 1;
        /// <summary>
        /// LRU算法已经进行的步数
        /// </summary>
        private int count_LRU = 1;
        /// <summary>
        /// OPT算法已经进行的步数
        /// </summary>
        private int count_OPT = 1;
        #endregion
        #region ---公有静态变量---
        /// <summary>
        /// 缺页中断所需要的时间
        /// </summary>
        public static int PageFaultTime = 100;
        /// <summary>
        /// 读取内存一次的时间
        /// </summary>
        public static int MemoryTime = 5;
        /// <summary>
        /// 读取快表一次的时间
        /// </summary>
        public static int TLBTime = 5;
        /// <summary>
        /// 驻留内存的块数
        /// </summary>
        public static int MemoryBlockNum = 4;
        /// <summary>
        /// 随机生成序列时生成的个数
        /// </summary>
        public static int GeneratorNum = 5;
        /// <summary>
        /// 随机取地址范围上限
        /// </summary>
        public static int AddressMax = 0xFFFF;
        #endregion
        private void MainForm_Load(object sender, EventArgs e)
        {
            //根据默认添加驻留内存的页面
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
            //取消单元格的选中
            CellsUnselected();
        }
        /// <summary>
        /// 取消单元格的选中
        /// </summary>
        private void CellsUnselected()
        {
            foreach (DataGridViewCell item in dataGridView_FIFO.SelectedCells)
            {
                item.Selected = false;
            }
        }
        /// <summary>
        /// 随机生成序列
        /// </summary>
        private void RandomGenerate_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < GeneratorNum; i++)
            {
                int address = rd.Next(0, AddressMax);
                //生成完整逻辑地址的标志
                if (GenerateLogicAddress.Checked)
                {
                    string tmp = address.ToString("X0") + "H";
                    //补0
                    if (tmp.Length < 5)
                    {
                        tmp = new string('0', 5 - tmp.Length) + tmp;
                    }
                    sb.Append(tmp);
                }
                else
                {
                    sb.Append(Helper.GetPageBlock(address).ToString("X0"));
                }
                sb.Append(" ");
            }
            //移除末尾的空格
            sb.Remove(sb.Length - 1, 1);
            InputText.Text = sb.ToString();
            InputAddresses = Helper.ReadAddress(InputText.Text);
            //更新表格
            UpdateDataGrid();
        }
        /// <summary>
        /// 根据输入地址数组更新表格信息
        /// </summary>
        private void UpdateDataGrid()
        {
            //除了第一列(页框号)之外移除其他列
            while (dataGridView_FIFO.Columns.Count != 1)
            {
                dataGridView_FIFO.Columns.RemoveAt(1);
            }
            //逻辑地址取页号
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
        /// <summary>
        /// 滑块移动通用事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageFaultTrack_Scroll(object sender, EventArgs e)
        {
            //滑块移动时,将文本框的内容与滑块值同步
            TrackBar tb = sender as TrackBar;
            //先赋值再更新文本框的值
            //否则TextChanged事件会优先触发,发生错误
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
                case "AddressMaxTrack":
                    AddressMax = tb.Value;
                    AddressMax_TextBox.Text = tb.Value.ToString("X2");
                    break;
            }
        }
        /// <summary>
        /// 文本框文本改变通用事件
        /// </summary>
        private void PageFaultTime_TextChanged(object sender, EventArgs e)
        {
            //文本改变时,将文本框的内容与滑块值同步
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
                    //改变驻留内存块数
                    //更新表格内容
                    dataGridView_FIFO.Rows.Clear();
                    for (int i = 0; i < MemoryBlockNum; i++)
                    {
                        dataGridView_FIFO.Rows.Add((i + 1).ToString());
                    }
                    dataGridView_FIFO.Rows.Add("缺页情况");
                    CellsUnselected();
                    break;
                case "AddressMax_TextBox":
                    TrackMoveByTextChanged(tb, AddressMaxTrack, Models.MoveMode.None,true);
                    break;
            }
        }
        /// <summary>
        /// 滑块值与文本内容同步,并对文本内容进行校验,防止越界以及无效字符
        /// </summary>
        /// <param name="tb">需要同步的文本框</param>
        /// <param name="tr">需要同步的滑块</param>
        /// <param name="mode">同步方式</param>
        private void TrackMoveByTextChanged(TextBox tb, TrackBar tr, Models.MoveMode mode, bool HexFlag=false)
        {
            if (int.TryParse(tb.Text, out int value) || HexFlag)
            {
                if(!int.TryParse(tb.Text,NumberStyles.HexNumber,null, out value))
                {
                    tb.Text = tr.Minimum.ToString();
                    return;
                }
                //未越界
                if (value <= tr.Maximum && value >= tr.Minimum)
                {
                    tr.Value = value;
                }
                else
                {
                    //如果选择越界时保存为滑块规定的最小值
                    if (mode == Models.MoveMode.MinimalMode)
                    {
                        tr.Value = tr.Minimum;
                        tb.Text = tr.Minimum.ToString();
                    }
                }
            }
            else//无效字符,直接赋值为最小值
            {
                tb.Text = tr.Minimum.ToString();
            }
        }
        /// <summary>
        /// 设置类文本框焦点移除通用事件
        /// </summary>
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
        /// <summary>
        /// FIFO算法开始按钮被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FIFO_Start_Click(object sender, EventArgs e)
        {
            //将内容清空
            foreach (DataGridViewRow item in dataGridView_FIFO.Rows)
            {
                for (int i = 1; i < item.Cells.Count; i++)
                {
                    item.Cells[i].Value = "";
                }
            }
            //禁用时间设置以及其他设置
            DisableState();
            //重置算法进行步数
            count_FIFO = 1;
            //算法环境初始化
            GlobalVariable.MemoryQueue_FIFO = new Queue<MemoryBlock>();
            GlobalVariable.Memory_FIFO = new Dictionary<int, int>();
            for (int i = 0; i < MemoryBlockNum; i++)
            {
                GlobalVariable.Memory_FIFO.Add(i + 1, -1);
            }
            //输入地址数组为空
            if (InputAddresses == null || InputAddresses.Count == 0)
            {
                //文本框是空的,随机一个出来
                if (string.IsNullOrWhiteSpace(InputText.Text))
                {
                    RandomGenerate.PerformClick();
                }
                else//文本框内有数据,直接读入
                {
                    InputAddresses = Helper.ReadAddress(InputText.Text);
                    UpdateDataGrid();
                }
            }
            //缺页次数
            int pageFaultCount = 0;
            //使用的时间
            int timeSpent = 0;
            Thread_FIFO = new Thread(() =>
            {
                foreach (var item in InputAddresses)
                {
                    //执行算法
                    bool flag = Arithmetic.MakeFIFO(item, out int blockNum);
                    //更新表格
                    dataGridView_FIFO.Invoke(new MethodInvoker
                        (() => UpdateDGV(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum)));
                    Thread.Sleep(MemoryTime);
                    Thread.Sleep(TLBTime);
                    timeSpent += MemoryTime + TLBTime;
                    if (flag)//缺页
                    {
                        pageFaultCount++;

                        Thread.Sleep(PageFaultTime);
                        Thread.Sleep(MemoryTime);
                        Thread.Sleep(TLBTime);

                        timeSpent += PageFaultTime;
                        timeSpent += MemoryTime + TLBTime;
                    }
                }
                //显示最终结果
                groupBox1.Invoke(new MethodInvoker(() =>
                {
                    FIFO_Result.Text = $"共用时间：{timeSpent}ms 缺页次数：{pageFaultCount} / {InputAddresses.Count} " +
                            $"缺页率：{pageFaultCount / (double)InputAddresses.Count * 100:f2}%";
                    EnableState();
                }));
            });
            Thread_FIFO.Start();
        }
        /// <summary>
        /// 禁用时间设置以及其他设置
        /// </summary>
        private void DisableState()
        {
            TimeSetting_GroupBox.Enabled = false;
            OtherSettings_GroupBox.Enabled = false;
            FIFO_Start.Enabled = false;
            FIFO_Pause.Enabled = true;
            FIFO_Result.Visible = false;
        }
        /// <summary>
        /// 恢复设置
        /// </summary>
        private void EnableState()
        {
            TimeSetting_GroupBox.Enabled = true;
            OtherSettings_GroupBox.Enabled = true;
            FIFO_Start.Enabled = true;
            FIFO_Pause.Enabled = false;
            FIFO_Result.Visible = true;
        }
        /// <summary>
        /// 更新表格
        /// </summary>
        /// <param name="DGV">需要更新的表格体</param>
        /// <param name="type">算法类型</param>
        /// <param name="flag">是否缺页</param>
        /// <param name="blockNum">需要进行高亮的内存块号</param>
        private void UpdateDGV(DataGridView DGV, Models.ArithmeticType type, bool flag, int blockNum = 0)
        {
            int count = 0;
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    //队列中有效的个数
                    foreach (var item in GlobalVariable.Memory_FIFO)
                    {
                        if (item.Value != -1)
                            count++;
                    }
                    //读取算法内存
                    for (int i = 0; i < MemoryBlockNum; i++)
                    {
                        if (i < count)//仍旧是有效的值,将内存的值写入表格
                        {
                            DGV.Rows[i].Cells[count_FIFO].Value = GlobalVariable.Memory_FIFO[i + 1].ToString("X0");
                        }
                        if (i == blockNum - 1)//为需要高亮的块号
                        {
                            DGV.Rows[i].Cells[count_FIFO].Style.ForeColor = Color.Red;
                        }
                    }
                    if (flag)//缺页标志
                    {
                        DGV.Rows[MemoryBlockNum].Cells[count_FIFO].Value = "√";
                    }
                    else
                    {
                        DGV.Rows[MemoryBlockNum].Cells[count_FIFO].Value = "×";
                    }
                    count_FIFO++;
                    break;
                case Models.ArithmeticType.LRU:

                    break;
                case Models.ArithmeticType.OPT:

                    break;
            }
        }
        /// <summary>
        /// 回车确认
        /// </summary>
        private void InputText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                InputAddresses = Helper.ReadAddress(InputText.Text);
                UpdateDataGrid();
            }
        }
        /// <summary>
        /// 禁用单元格单击
        /// </summary>
        private void dataGridView_FIFO_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CellsUnselected();
        }
        /// <summary>
        /// FIFO线程挂起按钮被单击
        /// </summary>
        private void FIFO_Pause_Click(object sender, EventArgs e)
        {
            if (Thread_FIFO.ThreadState == ThreadState.WaitSleepJoin)
            {
                Thread_FIFO.Suspend();//???或许需要找个替代方法,但这个方法仍旧有效
                FIFO_Pause.Text = "继续";
            }
            else
            {
                Thread_FIFO.Resume();
                FIFO_Pause.Text = "暂停";
            }
        }
    }
}
