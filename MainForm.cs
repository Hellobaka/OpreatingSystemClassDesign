using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

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
        /// <summary>
        /// 启动全部线程的标志
        /// </summary>
        private bool AllThread_Flag = false;
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
            DataGridView[] ls = { dataGridView_FIFO, dataGridView_LRU, dataGridView_OPT };
            foreach (var item in ls)
            {
                //https://www.cnblogs.com/samshen1991/p/7298422.html
                //设置双缓冲解决表格刷新过慢的问题
                Type type = item.GetType();
                System.Reflection.PropertyInfo pi = type.GetProperty("DoubleBuffered",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                pi.SetValue(item, true, null);

                //根据默认添加驻留内存的页面
                DataGridViewColumn column = new DataGridViewColumn()
                {
                    Width = 60,
                    Name = Guid.NewGuid().ToString(),
                    HeaderText = "页框号",
                    CellTemplate = new DataGridViewTextBoxCell()
                };
                item.Columns.Add(column);
                for (int i = 0; i < 4; i++)
                {
                    item.Rows.Add((i + 1).ToString());
                }
                item.Rows.Add("缺页情况");
            }
            //取消单元格的选中
            CellsUnselected();
            openFileDialog_Main.InitialDirectory = Application.StartupPath;
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
            foreach (DataGridViewCell item in dataGridView_LRU.SelectedCells)
            {
                item.Selected = false;
            }
            foreach (DataGridViewCell item in dataGridView_OPT.SelectedCells)
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
            UpdateDataGrid(Models.ArithmeticType.FIFO);
            UpdateDataGrid(Models.ArithmeticType.LRU);
            UpdateDataGrid(Models.ArithmeticType.OPT);
        }
        /// <summary>
        /// 根据输入地址数组创建新的表格表头
        /// </summary>
        private void UpdateDataGrid(Models.ArithmeticType type)
        {
            DataGridView DGV = new DataGridView();
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    DGV = dataGridView_FIFO;
                    break;
                case Models.ArithmeticType.LRU:
                    DGV = dataGridView_LRU;
                    break;
                case Models.ArithmeticType.OPT:
                    DGV = dataGridView_OPT;
                    break;
                case Models.ArithmeticType.All:
                    UpdateDataGrid(Models.ArithmeticType.FIFO);
                    UpdateDataGrid(Models.ArithmeticType.LRU);
                    UpdateDataGrid(Models.ArithmeticType.OPT);
                    return;
            }
            //除了第一列(页框号)之外移除其他列
            while (DGV.Columns.Count != 1)
            {
                DGV.Columns.RemoveAt(1);
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
                DGV.Columns.Add(column);
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
                    MemoryBlockNum_TextBox.Text = tb.Value.ToString();
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
                    PageFaultTime = PageFaultTrack.Value;
                    break;
                case "MemoryTime_TextBox":
                    TrackMoveByTextChanged(tb, MemoryTrack, Models.MoveMode.None);
                    MemoryTime = MemoryTrack.Value;
                    break;
                case "TLBTime_TextBox":
                    TrackMoveByTextChanged(tb, TLBTrack, Models.MoveMode.None);
                    TLBTime = TLBTrack.Value;
                    break;
                case "GeneratorNum_TextBox":
                    TrackMoveByTextChanged(tb, GeneratorNumTarck, Models.MoveMode.None);
                    GeneratorNum = GeneratorNumTarck.Value;
                    break;
                case "MemoryBlockNum_TextBox":
                    TrackMoveByTextChanged(tb, MemoryBlockTrack, Models.MoveMode.MinimalMode);
                    MemoryBlockNum = MemoryBlockTrack.Value;
                    //改变驻留内存块数
                    //更新表格内容
                    DataGridView[] ls = { dataGridView_FIFO, dataGridView_LRU, dataGridView_OPT };
                    foreach (var item in ls)
                    {
                        item.Rows.Clear();
                        for (int i = 0; i < MemoryBlockNum; i++)
                        {
                            item.Rows.Add((i + 1).ToString());
                        }
                        item.Rows.Add("缺页情况");
                    }
                    CellsUnselected();
                    break;
                case "AddressMax_TextBox":
                    TrackMoveByTextChanged(tb, AddressMaxTrack, Models.MoveMode.None, true);
                    AddressMax = AddressMaxTrack.Value;
                    break;
            }
        }
        /// <summary>
        /// 滑块值与文本内容同步,并对文本内容进行校验,防止越界以及无效字符
        /// </summary>
        /// <param name="tb">需要同步的文本框</param>
        /// <param name="tr">需要同步的滑块</param>
        /// <param name="mode">同步方式</param>
        /// <param name="HexFlag">十六进制标志</param>
        private void TrackMoveByTextChanged(TextBox tb, TrackBar tr, Models.MoveMode mode, bool HexFlag = false)
        {
            if (int.TryParse(tb.Text, out int value) || HexFlag)
            {
                if (HexFlag && !int.TryParse(tb.Text, NumberStyles.HexNumber, null, out value))
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
            AllThread_Flag = false;
            //将内容清空
            foreach (DataGridViewRow item in dataGridView_FIFO.Rows)
            {
                for (int i = 1; i < item.Cells.Count; i++)
                {
                    item.Cells[i].Value = "";
                }
            }
            //禁用时间设置以及其他设置
            DisableState(Models.ArithmeticType.FIFO);
            ArithmeticInit(Models.ArithmeticType.FIFO);
            InputRandomOrDefault();
            UpdateDataGrid(Models.ArithmeticType.FIFO);
            Thread_FIFO.Start();
        }
        /// <summary>
        /// 算法环境初始化,进行内存队列的清空、内存树的初始化以及算法线程的实例化
        /// </summary>
        /// <param name="type">算法的类型</param>
        private void ArithmeticInit(Models.ArithmeticType type)
        {
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    //重置算法进行步数
                    count_FIFO = 1;
                    //算法环境初始化
                    GlobalVariable.MemoryQueue_FIFO = new Queue<MemoryBlock>();
                    GlobalVariable.Memory_FIFO = new Dictionary<int, int>();
                    for (int i = 0; i < MemoryBlockNum; i++)
                    {
                        GlobalVariable.Memory_FIFO.Add(i + 1, -1);
                    }
                    //算法线程实例化
                    Thread_FIFO = new Thread(() =>
                    {
                        MakeFIFO(out int pageFaultCount, out int timeSpent);
                        //显示最终结果
                        FIFO_Result.Invoke(new MethodInvoker(() =>
                        {
                            FIFO_Result.Text = $"共用时间：{timeSpent}ms 缺页次数：{pageFaultCount} / {InputAddresses.Count} " +
                                    $"缺页率：{pageFaultCount / (double)InputAddresses.Count * 100:f2}%";
                            EnableState(Models.ArithmeticType.FIFO);
                        }));
                    });
                    break;
                case Models.ArithmeticType.LRU:
                    count_LRU = 1;
                    GlobalVariable.MemoryQueue_LRU = new Queue<MemoryBlock>();
                    GlobalVariable.Memory_LRU = new Dictionary<int, int>();
                    for (int i = 0; i < MemoryBlockNum; i++)
                    {
                        GlobalVariable.Memory_LRU.Add(i + 1, -1);
                    }
                    Thread_LRU = new Thread(() =>
                    {
                        MakeLRU(out int pageFaultCount, out int timeSpent);
                        LRU_Result.Invoke(new MethodInvoker(() =>
                        {
                            LRU_Result.Text = $"共用时间：{timeSpent}ms 缺页次数：{pageFaultCount} / {InputAddresses.Count} " +
                                    $"缺页率：{pageFaultCount / (double)InputAddresses.Count * 100:f2}%";
                            EnableState(Models.ArithmeticType.LRU);
                        }));
                    });
                    break;
                case Models.ArithmeticType.OPT:
                    count_OPT = 1;
                    GlobalVariable.MemoryQueue_OPT = new Queue<MemoryBlock>();
                    GlobalVariable.Memory_OPT = new Dictionary<int, int>();
                    for (int i = 0; i < MemoryBlockNum; i++)
                    {
                        GlobalVariable.Memory_OPT.Add(i + 1, -1);
                    }
                    Thread_OPT = new Thread(() =>
                    {
                        MakeOPT(out int pageFaultCount, out int timeSpent);
                        OPT_Result.Invoke(new MethodInvoker(() =>
                        {
                            OPT_Result.Text = $"共用时间：{timeSpent}ms 缺页次数：{pageFaultCount} / {InputAddresses.Count} " +
                                    $"缺页率：{pageFaultCount / (double)InputAddresses.Count * 100:f2}%";
                            EnableState(Models.ArithmeticType.OPT);
                        }));
                    });
                    break;
                case Models.ArithmeticType.All://所有算法同时进行,依次进行初始化
                    ArithmeticInit(Models.ArithmeticType.FIFO);
                    ArithmeticInit(Models.ArithmeticType.LRU);
                    ArithmeticInit(Models.ArithmeticType.OPT);
                    break;
            }
        }
        /// <summary>
        /// 将输入地址数组默认读取或者是随机输出一个
        /// </summary>
        private void InputRandomOrDefault()
        {
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
                }
            }
        }

        private void MakeFIFO(out int pageFaultCount, out int timeSpent)
        {
            //缺页次数
            pageFaultCount = 0;
            //使用的时间
            timeSpent = 0;
            foreach (var item in InputAddresses)
            {
                //执行算法
                bool flag = Arithmetic.MakeFIFO(item, out int blockNum);
                //更新表格
                dataGridView_FIFO.Invoke(new MethodInvoker
                    (() => UpdateDGV(Models.ArithmeticType.FIFO, flag, blockNum)));
                if (flag)
                {
                    pageFaultCount++;
                }
                int threadSleepTime = ThreadSleepByArithmeticResult(flag);
                timeSpent += threadSleepTime;
                AddToolTip2Cells(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum, threadSleepTime);
            }
        }
        /// <summary>
        /// 向单元格添加提示文本
        /// </summary>
        /// <param name="GDV">需要进行添加的表格</param>
        /// <param name="type">算法类型</param>
        /// <param name="flag">是否缺页</param>
        /// <param name="blockNum">被替换的页框号</param>
        /// <param name="threadSleepTime">本次线程休眠时间</param>
        private void AddToolTip2Cells(DataGridView GDV, Models.ArithmeticType type, bool flag, int blockNum, int threadSleepTime)
        {
            int count_Type = 0;
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    count_Type = count_FIFO;
                    break;
                case Models.ArithmeticType.LRU:
                    count_Type = count_LRU;
                    break;
                case Models.ArithmeticType.OPT:
                    count_Type = count_OPT;
                    break;
                case Models.ArithmeticType.All:
                    break;
            }
            StringBuilder sb = new StringBuilder();
            if (flag)
            {
                sb.Append($"发生缺页，");
                if (blockNum == 0)
                {
                    sb.Append("添加了新的一块，");
                }
                else
                {
                    int replacedBlock = Convert.ToInt32(GDV.Rows[blockNum - 1].Cells[count_Type - 2].Value.ToString(), 16);
                    sb.Append($"替换了{replacedBlock}，");
                }
            }
            else
            {
                sb.Append("未发生缺页，");
            }
            sb.Append($"共耗时 {threadSleepTime} ms");
            for (int i = 0; i < MemoryBlockNum; i++)
            {
                GDV.Rows[i].Cells[count_Type - 1].ToolTipText = sb.ToString();
            }
        }

        private void MakeLRU(out int pageFaultCount, out int timeSpent)
        {
            pageFaultCount = 0;
            timeSpent = 0;
            foreach (var item in InputAddresses)
            {
                bool flag = Arithmetic.MakeLRU(item, out int blockNum);
                dataGridView_LRU.Invoke(new MethodInvoker
                    (() => UpdateDGV(Models.ArithmeticType.LRU, flag, blockNum)));
                if (flag)
                {
                    pageFaultCount++;
                }
                int threadSleepTime = ThreadSleepByArithmeticResult(flag);
                timeSpent += threadSleepTime;
                AddToolTip2Cells(dataGridView_LRU, Models.ArithmeticType.LRU, flag, blockNum, threadSleepTime);
            }
        }
        private void MakeOPT(out int pageFaultCount, out int timeSpent)
        {
            pageFaultCount = 0;
            timeSpent = 0;
            for (int i = 0; i < InputAddresses.Count; i++)
            {
                bool flag = Arithmetic.MakeOPT(InputAddresses, i, out int blockNum);
                dataGridView_OPT.Invoke(new MethodInvoker
                    (() => UpdateDGV(Models.ArithmeticType.OPT, flag, blockNum)));
                if (flag)
                {
                    pageFaultCount++;
                }
                int threadSleepTime = ThreadSleepByArithmeticResult(flag);
                timeSpent += threadSleepTime;
                AddToolTip2Cells(dataGridView_OPT, Models.ArithmeticType.OPT, flag, blockNum, threadSleepTime);
            }
        }
        /// <summary>
        /// 线程休眠处理
        /// </summary>
        /// <param name="flag">缺页标志</param>
        /// <returns>本次休眠时长</returns>
        private static int ThreadSleepByArithmeticResult(bool flag)
        {
            int timeSpent = 0;
            Thread.Sleep(MemoryTime);
            Thread.Sleep(TLBTime);
            timeSpent += MemoryTime + TLBTime;
            if (flag)//缺页
            {
                Thread.Sleep(PageFaultTime);
                Thread.Sleep(MemoryTime);
                Thread.Sleep(TLBTime);

                timeSpent += PageFaultTime;
                timeSpent += MemoryTime + TLBTime;
            }
            return timeSpent;
        }

        /// <summary>
        /// 禁用时间设置以及其他设置
        /// </summary>
        private void DisableState(Models.ArithmeticType type)
        {
            TimeSetting_GroupBox.Enabled = false;
            OtherSettings_GroupBox.Enabled = false;
            InputText.Enabled = false;
            RandomGenerate.Enabled = false;
            switch (type)
            {
                //将输入序列、随机生成序列按钮、时间设置组、杂项设置组以及除了本算法之外的控制按钮全部禁用
                case Models.ArithmeticType.FIFO:
                    FIFO_Start.Enabled = false;
                    FIFO_Pause.Enabled = true;
                    FIFO_Result.Visible = false;
                    panel_ControlLRU.Enabled = false;
                    panel_ControlOPT.Enabled = false;
                    panel_ControlAll.Enabled = false;
                    break;
                case Models.ArithmeticType.LRU:
                    LRU_Start.Enabled = false;
                    LRU_Pause.Enabled = true;
                    LRU_Result.Visible = false;
                    panel_ControlFIFO.Enabled = false;
                    panel_ControlOPT.Enabled = false;
                    panel_ControlAll.Enabled = false;
                    break;
                case Models.ArithmeticType.OPT:
                    OPT_Start.Enabled = false;
                    OPT_Pause.Enabled = true;
                    OPT_Result.Visible = false;
                    panel_ControlFIFO.Enabled = false;
                    panel_ControlLRU.Enabled = false;
                    panel_ControlAll.Enabled = false;
                    break;
                case Models.ArithmeticType.All:
                    ALL_Start.Enabled = false;
                    ALL_Pause.Enabled = true;

                    FIFO_Result.Visible = false;
                    LRU_Result.Visible = false;
                    OPT_Result.Visible = false;

                    panel_ControlFIFO.Enabled = false;
                    panel_ControlLRU.Enabled = false;
                    panel_ControlOPT.Enabled = false;
                    break;
            }
        }
        /// <summary>
        /// 恢复设置启用状态
        /// </summary>
        /// <param name="type">算法类型</param>
        private void EnableState(Models.ArithmeticType type)
        {
            TimeSetting_GroupBox.Enabled = true;
            OtherSettings_GroupBox.Enabled = true;

            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    FIFO_Start.Enabled = true;
                    FIFO_Pause.Enabled = false;
                    FIFO_Result.Visible = true;

                    panel_ControlLRU.Enabled = true;
                    panel_ControlOPT.Enabled = true;
                    panel_ControlAll.Enabled = true;
                    if (AllThread_Flag && (!Thread_LRU.IsAlive && !Thread_OPT.IsAlive))//适用于全部算法线程启用时,判断所有线程是否结束
                    {
                        ALL_Start.Enabled = true;
                        ALL_Pause.Enabled = false;
                    }
                    break;
                case Models.ArithmeticType.LRU:
                    LRU_Start.Enabled = true;
                    LRU_Pause.Enabled = false;
                    LRU_Result.Visible = true;
                    panel_ControlFIFO.Enabled = true;
                    panel_ControlOPT.Enabled = true;
                    panel_ControlAll.Enabled = true;
                    if (AllThread_Flag && (!Thread_FIFO.IsAlive && !Thread_OPT.IsAlive))
                    {
                        ALL_Start.Enabled = true;
                        ALL_Pause.Enabled = false;
                    }

                    break;
                case Models.ArithmeticType.OPT:
                    OPT_Start.Enabled = true;
                    OPT_Pause.Enabled = false;
                    OPT_Result.Visible = true;
                    panel_ControlFIFO.Enabled = true;
                    panel_ControlLRU.Enabled = true;
                    panel_ControlAll.Enabled = true;
                    if (AllThread_Flag && (!Thread_FIFO.IsAlive && !Thread_LRU.IsAlive))
                    {
                        ALL_Start.Enabled = true;
                        ALL_Pause.Enabled = false;
                    }
                    break;
            }
            InputText.Enabled = true;
            RandomGenerate.Enabled = true;
        }
        /// <summary>
        /// 读取内存树,并将其中的内容更新至表格中 算法执行的步数的列 中
        /// </summary>
        /// <param name="type">算法类型</param>
        /// <param name="flag">是否缺页</param>
        /// <param name="blockNum">需要进行高亮的内存块号</param>
        private void UpdateDGV(Models.ArithmeticType type, bool flag, int blockNum = 0)
        {
            //队列中有效的个数
            int count = 0;
            int count_Type = 0;
            DataGridView DGV = new DataGridView();
            Dictionary<int, int> Memory_Type = new Dictionary<int, int>();
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    Memory_Type = GlobalVariable.Memory_FIFO;
                    count_FIFO++;
                    count_Type = count_FIFO - 1;
                    DGV = dataGridView_FIFO;
                    break;
                case Models.ArithmeticType.LRU:
                    Memory_Type = GlobalVariable.Memory_LRU;
                    count_LRU++;
                    count_Type = count_LRU - 1;
                    DGV = dataGridView_LRU;
                    break;
                case Models.ArithmeticType.OPT:
                    Memory_Type = GlobalVariable.Memory_OPT;
                    count_OPT++;
                    count_Type = count_OPT - 1;
                    DGV = dataGridView_OPT;
                    break;
            }
            foreach (var item in Memory_Type)
            {
                if (item.Value != -1)
                    count++;
            }
            //读取算法内存
            for (int i = 0; i < MemoryBlockNum; i++)
            {
                if (i < count)//仍旧是有效的值,将内存的值写入表格
                {
                    DGV.Rows[i].Cells[count_Type].Value = Memory_Type[i + 1].ToString("X0");
                }
                if (i == blockNum - 1)//为需要高亮的块号
                {
                    DGV.Rows[i].Cells[count_Type].Style.ForeColor = Color.Red;
                }
                if (count_Type >= 28)
                {
                    DGV.FirstDisplayedCell = DGV.Rows[i].Cells[count_Type - 28];
                }
            }
            if (flag)//缺页标志
            {
                DGV.Rows[MemoryBlockNum].Cells[count_Type].Value = "√";
            }
            else
            {
                DGV.Rows[MemoryBlockNum].Cells[count_Type].Value = "×";
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
                UpdateDataGrid(Models.ArithmeticType.FIFO);
                UpdateDataGrid(Models.ArithmeticType.LRU);
                UpdateDataGrid(Models.ArithmeticType.OPT);
            }
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

        private void LRU_Start_Click(object sender, EventArgs e)
        {
            AllThread_Flag = false;
            //将内容清空
            foreach (DataGridViewRow item in dataGridView_LRU.Rows)
            {
                for (int i = 1; i < item.Cells.Count; i++)
                {
                    item.Cells[i].Value = "";
                }
            }
            //禁用时间设置以及其他设置
            DisableState(Models.ArithmeticType.LRU);
            ArithmeticInit(Models.ArithmeticType.LRU);
            InputRandomOrDefault();
            UpdateDataGrid(Models.ArithmeticType.LRU);
            Thread_LRU.Start();
        }

        private void LRU_Pause_Click(object sender, EventArgs e)
        {
            if (Thread_LRU.ThreadState == ThreadState.WaitSleepJoin)
            {
                Thread_LRU.Suspend();
                LRU_Pause.Text = "继续";
            }
            else
            {
                Thread_LRU.Resume();
                LRU_Pause.Text = "暂停";
            }
        }

        private void OPT_Pause_Click(object sender, EventArgs e)
        {
            if (Thread_OPT.ThreadState == ThreadState.WaitSleepJoin)
            {
                Thread_OPT.Suspend();
                OPT_Pause.Text = "继续";
            }
            else
            {
                Thread_OPT.Resume();
                OPT_Pause.Text = "暂停";
            }
        }

        private void ALL_Pause_Click(object sender, EventArgs e)
        {
            if (Thread_FIFO.ThreadState == ThreadState.WaitSleepJoin
                || Thread_FIFO.ThreadState == ThreadState.Running)
            {
                Thread_FIFO.Suspend();
                if (Thread_LRU.IsAlive)
                    Thread_LRU.Suspend();
                if (Thread_OPT.IsAlive)
                    Thread_OPT.Suspend();
                ALL_Pause.Text = "继续";
            }
            else
            {
                Thread_FIFO.Resume();
                Thread_LRU.Resume();
                Thread_OPT.Resume();
                ALL_Pause.Text = "暂停";
            }
        }

        private void OPT_Start_Click(object sender, EventArgs e)
        {
            AllThread_Flag = false;
            //将内容清空
            foreach (DataGridViewRow item in dataGridView_OPT.Rows)
            {
                for (int i = 1; i < item.Cells.Count; i++)
                {
                    item.Cells[i].Value = "";
                }
            }
            //禁用时间设置以及其他设置
            DisableState(Models.ArithmeticType.OPT);
            ArithmeticInit(Models.ArithmeticType.OPT);
            InputRandomOrDefault();
            UpdateDataGrid(Models.ArithmeticType.OPT);

            Thread_OPT.Start();
        }

        private void ALL_Start_Click(object sender, EventArgs e)
        {
            AllThread_Flag = true;
            DataGridView[] ls = { dataGridView_FIFO, dataGridView_LRU, dataGridView_OPT };
            foreach (var item in ls)
            {
                foreach (DataGridViewRow Rows in item.Rows)
                {
                    for (int i = 1; i < Rows.Cells.Count; i++)
                    {
                        Rows.Cells[i].Value = "";
                    }
                }
            }
            InputRandomOrDefault();
            DisableState(Models.ArithmeticType.All);
            ArithmeticInit(Models.ArithmeticType.All);
            UpdateDataGrid(Models.ArithmeticType.All);

            Thread_FIFO.Start();
            Thread_LRU.Start();
            Thread_OPT.Start();
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveConfig();
            MessageBox.Show("保存完毕");
            System.Diagnostics.Process.Start(Application.StartupPath);
        }
        /// <summary>
        /// 写配置,文件名为当前时间,格式为json
        /// </summary>
        private void SaveConfig()
        {
            JObject json = new JObject
            {
                {"TimeSettings",new JObject
                    {
                        {"PageFault",PageFaultTime },
                        {"MemoryTime",MemoryTime },
                        {"TLBTime",TLBTime }
                    }
                },
                {
                    "OtherSettings",new JObject
                    {
                        {"MemoryBlockNum",MemoryBlockNum},
                        {"GeneratorNum",GeneratorNum },
                        {"AddressMax",AddressMax.ToString("X0") },//16进制
                        {"GenerateLogicAddress",GenerateLogicAddress.Checked }
                    }
                },
                {"InputAddresses",InputText.Text},
                {
                    "FIFOState",new JObject()
                    {
                        { "Header",""},
                        { "Content",new JArray() },
                        {"ResultVisable",FIFO_Result.Visible },
                        {"ResultContent",FIFO_Result.Text }
                    }
                },
                {
                    "LRUState",new JObject()
                    {
                        { "Header",""},
                        { "Content",new JArray() },
                        {"ResultVisable",LRU_Result.Visible },
                        {"ResultContent",LRU_Result.Text }
                    }
                },
                {
                    "OPTState",new JObject()
                    {
                        { "Header",""},
                        { "Content",new JArray() },
                        {"ResultVisable",OPT_Result.Visible },
                        {"ResultContent",OPT_Result.Text }
                    }
                }
            };
            //控件放入数组,方便循环操作
            DataGridView[] ls = { dataGridView_FIFO, dataGridView_LRU, dataGridView_OPT };
            string[] nameLs = { "FIFOState", "LRUState", "OPTState" };
            int index = 0;
            foreach (var dgv in ls)
            {
                //标题行处理
                StringBuilder headerText = new StringBuilder();
                foreach (DataGridViewColumn item in dgv.Columns)
                {
                    headerText.Append(item.HeaderText);
                    headerText.Append("-");
                }
                headerText.Remove(headerText.Length - 1, 1);//抹去末尾的-
                json[nameLs[index]]["Header"] = headerText.ToString();
                //行处理,放入JArray (json数组)
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (DataGridViewCell item in dgv.Rows[i].Cells)
                    {
                        sb.Append(item.Value);
                        if (item.Value == null || item.Value.ToString() == "")//是空块
                        {
                            sb.Append(" ");
                        }
                        if (item.Style.ForeColor == Color.Red)//高亮块
                        {
                            sb.Append("r");
                        }
                        sb.Append("|");
                        if (string.IsNullOrEmpty(item.ToolTipText))
                        {
                            sb.Append(" ");
                        }
                        else
                        {
                            sb.Append(item.ToolTipText);
                        }
                        sb.Append("-");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    (json[nameLs[index]]["Content"] as JArray).Add(sb.ToString());
                }
                index++;
            }
            File.WriteAllText($"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.OSCD", json.ToString());
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("这样操作将会覆盖目前界面的设置，确认要打开之前的存档吗？"
                , "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                //呼起文件选择框
                openFileDialog_Main.ShowDialog();
                string filePath = openFileDialog_Main.FileName;
                if (string.IsNullOrEmpty(filePath))
                    return;
                ReadConfig(filePath);
            }
        }
        /// <summary>
        /// 从文件中读取配置，并将全部界面元素更改
        /// </summary>
        /// <param name="filePath"></param>
        private void ReadConfig(string filePath)
        {
            JObject json = JObject.Parse(File.ReadAllText(filePath));
            //时间设定部分
            PageFaultTime_TextBox.Text = json["TimeSettings"]["PageFault"].ToString();
            MemoryTime_TextBox.Text = json["TimeSettings"]["MemoryTime"].ToString();
            TLBTime_TextBox.Text = json["TimeSettings"]["TLBTime"].ToString();
            //杂项设定部分
            MemoryBlockNum_TextBox.Text = json["OtherSettings"]["MemoryBlockNum"].ToString();
            GeneratorNum_TextBox.Text = json["OtherSettings"]["GeneratorNum"].ToString();
            AddressMax_TextBox.Text = json["OtherSettings"]["AddressMax"].ToString();
            GenerateLogicAddress.Checked = json["OtherSettings"]["GenerateLogicAddress"].ToObject<bool>();
            //序列输入框
            InputText.Text = json["InputAddresses"].ToString();
            //算法内容展示部分
            //控件放入数组方便循环操作
            DataGridView[] ls = { dataGridView_FIFO, dataGridView_LRU, dataGridView_OPT };
            string[] nameLs = { "FIFOState", "LRUState", "OPTState" };
            Label[] labelLs = { FIFO_Result, LRU_Result, OPT_Result };
            for (int i = 0; i < 3; i++)
            {
                ls[i].Columns.Clear();
                //读取标题行
                string headerReader = json[nameLs[i]]["Header"].ToString();
                //由于表格的第一列与其他列的宽度不一样,需要进行区别
                bool first = true;
                foreach (var item in headerReader.Split('-'))
                {
                    DataGridViewColumn column = new DataGridViewColumn
                    {
                        HeaderText = item,
                        Width = first ? 60 : 30
                    };
                    if (first) first = false;
                    ls[i].Columns.Add(column);
                }
                //表格行内容
                foreach (var item in json[nameLs[i]]["Content"] as JArray)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    foreach (var content in item.ToString().Split('-'))
                    {
                        string toolTip = content.Split('|')[1];
                        DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell
                        {
                            Value = content.Substring(0, content.IndexOf('|')).Replace("r", ""),
                            ToolTipText = toolTip
                        };
                        //高亮字符
                        cell.Style.ForeColor = content.Contains("r") ? Color.Red : Color.Black;
                        row.Cells.Add(cell);
                    }
                    ls[i].Rows.Add(row);
                }
                //显示缺页率等内容的标签
                labelLs[i].Visible = json[nameLs[i]]["ResultVisable"].ToObject<bool>();
                labelLs[i].Text = json[nameLs[i]]["ResultContent"].ToString();
            }
        }
    }
}
