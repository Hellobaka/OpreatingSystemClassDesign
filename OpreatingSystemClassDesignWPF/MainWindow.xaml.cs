using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpreatingSystemClassDesignWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region ---私有变量---
        /// <summary>
        /// 算法的线程，用于挂起与恢复
        /// </summary>
        private Thread Thread_Arthmetic { get; set; }
        /// <summary>
        /// 输入的地址数组
        /// </summary>
        private static List<int> InputAddresses { get; set; }
        /// <summary>
        /// 算法进行的步数 ,默认从 0 开始
        /// </summary>
        private int count_Arthmetic { get; set; } = 0;
        /// <summary>
        /// 内存队列历史状态保存 ,用于实现 上一步 操作
        /// </summary>
        private List<Queue<MemoryBlock>> MemoryQueue_History { get; set; } = new List<Queue<MemoryBlock>>();
        /// <summary>
        /// 内存内容历史状态保存 ,用于实现 上一步 操作
        /// </summary>
        private List<Dictionary<int, int>> Memory_History { get; set; } = new List<Dictionary<int, int>>();
        /// <summary>
        /// 队列展示内容历史状态保存 ,用于实现 上一步 操作
        /// </summary>
        private List<string> QueueString_History { get; set; } = new List<string>();
        /// <summary>
        /// 缺页次数
        /// </summary>
        private int pageFaultCount { get; set; } = 0;
        /// <summary>
        /// 算法执行使用的时间
        /// </summary>
        private int timeSpent { get; set; } = 0;
        /// <summary>
        /// 队列展示模块中 ,用于展示边框颜色的 Border 集合
        /// </summary>
        List<Border> queueBorderList { get; set; } = new List<Border>();
        /// <summary>
        /// 队列展示模块中 ,用于展示文本内容的 TextBlock 集合
        /// </summary>
        List<TextBlock> queueTextBoxList { get; set; } = new List<TextBlock>();
        /// <summary>
        /// 打开文件对话框
        /// </summary>
        OpenFileDialog openFileDialog_Main { get; set; } = new OpenFileDialog();
        #endregion

        #region ---事件集合---
        /// <summary>
        /// 弹出关闭页面确认对话框
        /// </summary>
        private void CloseWindows_Click(object sender, RoutedEventArgs e)
        {
            var parent = Helper.GetYesNoDialogContent("确认要关闭吗，未保存的工作都将丢失");
            //通过子树的方式获取指定按钮
            Button bt = (parent.Children[1] as StackPanel).Children[0] as Button;
            //为按钮绑定单击事件
            bt.Click += (s, ev) => { Close(); };
            bt = (parent.Children[1] as StackPanel).Children[1] as Button;
            //点击了 "否"
            bt.Click += (s, ev) => { DialogMain.IsOpen = false; };
            DialogMain.DialogContent = parent;
            //弹出对话框
            DialogMain.IsOpen = true;
        }
        /// <summary>
        /// 使伪标题栏能够拖动
        /// </summary>
        private void ColorZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("SaveFiles"))
            {
                Directory.CreateDirectory("SaveFiles");
            }
            //向帮助类写入控件内容
            Helper.StackPanel_Main = MemoryContent_Group;
            Helper.SnackBar_Msg = SnackBar_Msg;
            //添加默认表头
            MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel("页框号"
                , Models.StackPanelColor.White));
            //添加默认块号
            for (int i = 1; i <= 4; i++)
            {
                MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel(i.ToString()
                    , Helper.GetStackPanelColor(i)));
            }
            MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel("是否缺页"
                , Helper.GetStackPanelColor(MemoryContent_Group.Children.Count)));
            //读取地址内容
            InputAddresses = Helper.ReadAddress(InputText.Text);
            //更新表格列
            AddColunm2DataGrid();
            //初始化算法环境
            EnviromentInit();
            //队列展示模块初始化
            GenerateQueueBorders();

            //事件绑定
            GlobalVariable.OnNumChange += TimeSettings_OnNumChange;
            GlobalVariable.OnMemoryBlockNumChange += OnMemoryBlockNumChange;
            GlobalVariable.OnArthmeticChange += OnArthmeticChange;
            GlobalVariable.OnTLBUsingStateChange += OnTLBUsingStateChange;

            //打开文件对话框默认目录
            openFileDialog_Main.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveFiles");

            //异常捕获
            Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        /// <summary>
        /// 快表状态变更事件
        /// </summary>
        private void OnTLBUsingStateChange(object sender, EventArgs e)
        {
            TLBUsingState_B.Text = $"是否使用快表：{GlobalVariable.TLBUsingState}";
        }
        /// <summary>
        /// 算法类型变更事件
        /// </summary>
        private void OnArthmeticChange(object sender, EventArgs e)
        {
            Models.ArithmeticType type = (Models.ArithmeticType)sender;
            string text = "";
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    text = "算法类型：先进先出置换算法 (FIFO)";
                    break;
                case Models.ArithmeticType.LRU:
                    text = "算法类型：最近未使用置换算法 (LRU)";
                    break;
                case Models.ArithmeticType.OPT:
                    text = "算法类型：最优置换算法 (OPT)";
                    break;
                case Models.ArithmeticType.All://无法实现 ,参数传递给WinForm版
                    var content = Helper.GetYesNoDialogContent("时间原因无法实现，单击确定打开WinForm版");
                    Button bt = (content.Children[1] as StackPanel).Children[0] as Button;
                    bt.Click += (s, ev) => { CallWinForm(); };
                    bt = (content.Children[1] as StackPanel).Children[1] as Button;
                    bt.Click += (s, ev) => { DialogMain.IsOpen = false; };
                    DialogMain.DialogContent = content;
                    DialogMain.IsOpen = true;
                    GlobalVariable.ChosenArthmetic = Models.ArithmeticType.FIFO;
                    break;
            }
            ArithmeticType_B.Text = text;
            //切换算法 ,初始化算法环境
            EnviromentInit();
            //初始化队列展示模块
            GenerateQueueBorders();
        }
        /// <summary>
        /// 内存块数变更事件
        /// </summary>
        private void OnMemoryBlockNumChange(object sender, EventArgs e)
        {
            //按照变化的数值重新生成主堆栈面板内容
            int count = MemoryContent_Group.Children.Count - 1;
            for (int i = 0; i < count; i++)
            {
                Helper.RemoveLastChild(MemoryContent_Group);
            }
            for (int i = 1; i <= GlobalVariable.MemoryBlockNum; i++)
            {
                MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel(i.ToString()
                    , Helper.GetStackPanelColor(i)));
            }
            MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel("是否缺页"
                , Helper.GetStackPanelColor(MemoryContent_Group.Children.Count)));
            EnviromentInit();
            GenerateQueueBorders();
        }
        /// <summary>
        /// 时间设定变更事件
        /// </summary>
        private void TimeSettings_OnNumChange(object sender, EventArgs e)
        {
            switch (sender as string)
            {
                case "PageFaultTime":
                    PageFaultTime_B.Text = $"缺页中断时长：{GlobalVariable.PageFaultTime} ms";
                    break;
                case "MemoryTime":
                    MemoryTime_B.Text = $"主存读取时长：{GlobalVariable.MemoryTime} ms";
                    break;
                case "TLBTime":
                    TLBTime_B.Text = $"快表读取时长：{GlobalVariable.TLBTime} ms";
                    break;
            }
        }
        /// <summary>
        /// 非UI线程发生的异常捕获
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            MessageBox.Show($"捕获到未处理的异常，发生在非UI线程中，单击确定以退出程序：{exception.Message}" +
                $"\n{exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        /// <summary>
        /// UI线程发生的异常捕获
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"捕获到未处理的异常，发生在UI线程中，单击确定以忽略：{e.Exception.Message}\n{e.Exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        /// <summary>
        /// 窗口大小变更事件
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //保持自定义控件的位置
            valueSlider.Margin = new Thickness(14, -358, 0, 0);
        }
        /// <summary>
        /// 随机生成地址按钮单击
        /// </summary>
        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < GlobalVariable.GeneratorNum; i++)
            {
                int address = rd.Next(0, GlobalVariable.AddressMax);
                //生成完整逻辑地址的标志
                if (GlobalVariable.GenerateLogicAddress)
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
            AddColunm2DataGrid();
        }
        /// <summary>
        /// 自动进行按钮单击
        /// </summary>
        private void AutoPlay_Click(object sender, RoutedEventArgs e)
        {
            //获取此按钮内的图片类型
            var p = AutoPlay.Content as PackIcon;
            if (p.Kind == PackIconKind.Play)//未处于自动进行状态
            {
                p.Kind = PackIconKind.Pause;//变更样式
                //禁止控件以变更变量内容
                valueSlider.IsEnabled = false;
                InputPanel.IsEnabled = false;
                if (Thread_Arthmetic != null && Thread_Arthmetic.ThreadState == ThreadState.Suspended)//线程被挂起 ,唤醒线程
                    Thread_Arthmetic.Resume();
                else
                {
                    //第一次播放 ,初始化线程内容
                    Thread_Arthmetic = GetArthmeticThread(GlobalVariable.ChosenArthmetic);
                    Thread_Arthmetic.Start();
                }
            }
            else
            {
                p.Kind = PackIconKind.Play;
                if (Thread_Arthmetic.ThreadState == ThreadState.Running
                    || Thread_Arthmetic.ThreadState == ThreadState.WaitSleepJoin)//线程正在运行 ,挂起线程
                    Thread_Arthmetic.Suspend();
            }
        }
        /// <summary>
        /// 重置按钮单击
        /// </summary>
        private void ResetState_Click(object sender, RoutedEventArgs e)
        {
            EnviromentInit();
            RecoveryState();
            GenerateQueueBorders();
            Queue_Log.Text = "";
            Helper.ShowSnackBar("界面状态已重置");
            Thread_Arthmetic = null;
            InputPanel.IsEnabled = true;
            GC.Collect();
        }
        /// <summary>
        /// 内存输入文本框失去焦点事件
        /// </summary>
        private void InputText_LostFocus(object sender, RoutedEventArgs e)
        {
            //按照内容更新内存输入数组 ,并更新列
            var c = Helper.ReadAddress(InputText.Text);
            if (c != null)
            {
                InputAddresses = c;
                AddColunm2DataGrid();
            }
        }
        /// <summary>
        /// 内存输入文本框按下回车事件
        /// </summary>
        private void InputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var c = Helper.ReadAddress(InputText.Text);
                if (c != null)
                {
                    InputAddresses = c;
                    AddColunm2DataGrid();
                }
            }
        }
        /// <summary>
        /// 上一步按钮单击
        /// </summary>
        private void PerviousStep_Click(object sender, RoutedEventArgs e)
        {
            NextStep.IsEnabled = true;
            count_Arthmetic--;
            if (count_Arthmetic == 0)//已经到底 ,相当于重新进行一遍算法 ,所以重置算法环境
            {
                PerviousStep.IsEnabled = false;
                EnviromentInit();
                GenerateQueueBorders();
                return;
            }
            //为相对于的算法内存以及队列进行恢复
            //从历史记录中取出上一步的内容
            //必须进行取副本操作 ,不然历史记录会被算法步骤同步更改
            switch (GlobalVariable.ChosenArthmetic)
            {
                case Models.ArithmeticType.FIFO:
                    GlobalVariable.Memory_FIFO = Helper.GetDictClone(Memory_History[count_Arthmetic - 1]);
                    GlobalVariable.MemoryQueue_FIFO = Helper.GetQueueClone(MemoryQueue_History[count_Arthmetic - 1]);
                    break;
                case Models.ArithmeticType.LRU:
                    GlobalVariable.Memory_LRU = Helper.GetDictClone(Memory_History[count_Arthmetic - 1]);
                    GlobalVariable.MemoryQueue_LRU = Helper.GetQueueClone(MemoryQueue_History[count_Arthmetic - 1]);
                    break;
                case Models.ArithmeticType.OPT:
                    GlobalVariable.Memory_OPT = Helper.GetDictClone(Memory_History[count_Arthmetic - 1]);
                    GlobalVariable.MemoryQueue_OPT = Helper.GetQueueClone(MemoryQueue_History[count_Arthmetic - 1]);
                    break;
                case Models.ArithmeticType.All:
                    break;
            }
            //还原队列展示模块
            var str = QueueString_History[count_Arthmetic - 1].Split('-');
            for (int i = 0; i < queueTextBoxList.Count; i++)
            {
                string tmp = str[i];
                if (tmp.Contains("r"))
                {
                    tmp = tmp.Replace("r", "");
                    queueBorderList[i].BorderBrush = Brushes.Red;
                }
                else if (tmp.Contains("v"))
                {
                    tmp = tmp.Replace("v", "");
                    queueBorderList[i].BorderBrush = Brushes.Violet;
                }
                else
                {
                    queueBorderList[i].BorderBrush = Brushes.Black;
                }
                queueTextBoxList[i].Text = tmp;
            }
            //移除历史记录中最后一个元素 ,因为已经被舍弃
            MemoryQueue_History.RemoveAt(MemoryQueue_History.Count - 1);
            Memory_History.RemoveAt(Memory_History.Count - 1);
            QueueString_History.RemoveAt(QueueString_History.Count - 1);
            //从主堆栈面板中除了第一行外 ,每一行除去其最后一个元素
            for (int i = 1; i <= GlobalVariable.MemoryBlockNum + 1; i++)
            {
                Helper.RemoveLastChild(MemoryContent_Group.Children[i] as StackPanel);
            }
        }
        /// <summary>
        /// 下一步按钮单击
        /// </summary>
        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            //本质还是单步执行一步算法
            InputPanel.IsEnabled = false;
            valueSlider.IsEnabled = false;
            switch (GlobalVariable.ChosenArthmetic)
            {
                case Models.ArithmeticType.FIFO:
                    MakeFIFOByStep(InputAddresses[count_Arthmetic]);
                    break;
                case Models.ArithmeticType.LRU:
                    MakeLRUByStep(InputAddresses[count_Arthmetic]);
                    break;
                case Models.ArithmeticType.OPT:
                    MakeOPTByStep(count_Arthmetic);
                    break;
            }
            if (count_Arthmetic == InputAddresses.Count)
            {
                AutoPlay.IsEnabled = false;
                Helper.ShowSnackBar("算法执行完成");
                TotalTime_B.Text = $"共用时间：{timeSpent} ms";
                PageFaultCount_B.Text = $"缺页次数：{pageFaultCount} 次";
                PageFaultRate_B.Text = $"缺页率：{pageFaultCount / (double)InputAddresses.Count * 100:f2} %";
            }
        }
        private void AboutMe_Click(object sender, RoutedEventArgs e)
        {
            AboutMe aboutMe = new AboutMe();
            Frame fm = new Frame();
            fm.Content = aboutMe;
            DialogMain.DialogContent = fm;
            aboutMe.close.Click += (s,re) => { DialogMain.IsOpen = false; };
            DialogMain.IsOpen = true;
        }
        #endregion

        #region ---读取 保存配置文件---
        /// <summary>
        /// 打开文件菜单单击
        /// </summary>
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var content = Helper.GetYesNoDialogContent("这样操作将会覆盖目前界面的设置，确认要打开之前的存档吗？");
            Button bt = (content.Children[1] as StackPanel).Children[0] as Button;
            bt.Click += (s, ev) => { OpenFile(); };
            bt = (content.Children[1] as StackPanel).Children[1] as Button;
            bt.Click += (s, ev) => { DialogMain.IsOpen = false; };
            DialogMain.DialogContent = content;
            DialogMain.IsOpen = true;
        }
        /// <summary>
        /// 保存菜单单击
        /// </summary>
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(openFileDialog_Main.InitialDirectory, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.OSCD");
            SaveConfig(filePath);
            MessageBox.Show("保存完毕");
            //弹出保存的文件夹
            System.Diagnostics.Process.Start(openFileDialog_Main.InitialDirectory);
        }
        /// <summary>
        /// 读取配置
        /// </summary>
        private void OpenFile()
        {
            //关闭对话框
            DialogMain.IsOpen = false;
            //打开文件选择框
            openFileDialog_Main.ShowDialog();
            string filePath = openFileDialog_Main.FileName;
            if (string.IsNullOrEmpty(filePath))
                return;
            MemoryContent_Group.Children.Clear();
            ReadConfig(filePath);
        }
        /// <summary>
        /// 从文件中读取配置，并将全部界面元素更改，对WinForm版存档做了兼容处理
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        private void ReadConfig(string filePath)
        {
            JObject json = JObject.Parse(File.ReadAllText(filePath));
            //时间设定部分
            valueSlider.PageFaultTime_TextBox.Text = json["TimeSettings"]["PageFault"].ToString();
            valueSlider.MemoryTime_TextBox.Text = json["TimeSettings"]["MemoryTime"].ToString();
            valueSlider.TLBTime_TextBox.Text = json["TimeSettings"]["TLBTime"].ToString();
            valueSlider.TLBState_Switch.IsChecked = json["TimeSettings"]["TLBUsing"].ToObject<bool>();
            //杂项设定部分
            valueSlider.MemoryNum_TextBox.Text = json["OtherSettings"]["MemoryBlockNum"].ToString();
            valueSlider.GeneratorNum_TextBox.Text = json["OtherSettings"]["GeneratorNum"].ToString();
            valueSlider.AddressMax_TextBox.Text = json["OtherSettings"]["AddressMax"].ToString();
            valueSlider.GenerateLogicAddress_Switch.IsChecked = json["OtherSettings"]["GenerateLogicAddress"].ToObject<bool>();
            int arthmeticIndex = 0;
            try
            {
                //算法策略
                arthmeticIndex = json["Arthmethic"]["ChoiceIndex"].ToObject<int>();
                if (arthmeticIndex == 3)
                    arthmeticIndex = 0;
                valueSlider.ArthmeticChoice.SelectedIndex = arthmeticIndex;                
                InputAddresses = JsonConvert.DeserializeObject<List<int>>(json["OtherSettings"]["InputAddresses"].ToString());
            }
            catch { }
            //序列输入框
            InputText.Text = json["InputAddresses"].ToString();
            //算法内容展示部分
            string[] nameLs = { "FIFOState", "LRUState", "OPTState" };
            //读取标题行
            string headerReader = json[nameLs[arthmeticIndex]]["Header"].ToString();
            //由于表格的第一列与其他列的宽度不一样,需要进行区别
            bool first = true;
            StackPanel header = Helper.GetDefaultTemplateStackPanel("页框号"
                , Models.StackPanelColor.White);
            foreach (var item in headerReader.Split('-'))
            {
                if (first)//跳过第一个 ,因为已经手动插入了
                {
                    first = false;
                    continue;
                }
                Helper.AddString2StackPanel(item, header);
            }
            MemoryContent_Group.Children.Add(header);
            int index = 1;
            //表格行内容
            foreach (var item in json[nameLs[arthmeticIndex]]["Content"] as JArray)
            {
                var array = item.ToString().Split('-').ToList();
                StackPanel tmp = Helper.GetDefaultTemplateStackPanel(array[0].Substring(0, array[0].IndexOf('|'))
                    , Helper.GetStackPanelColor(index));
                foreach (var content in array.GetRange(1, array.Count - 1))//除去第一个
                {
                    string toolTip = content.Split('|')[1];
                    var c = Helper.GetTemplateTextBlock(content.Substring(0, content.IndexOf('|')).Replace("r", "").Replace("v", ""));
                    //高亮字符
                    if (content.Contains("r"))//红色标识
                    {
                        c.Foreground = Brushes.Red;
                    }
                    else if (content.Contains("v"))//紫色标识
                    {
                        c.Foreground = Brushes.Violet;
                    }
                    tmp.Children.Add(c);
                }
                MemoryContent_Group.Children.Add(tmp);
                index++;
            }
            try
            {
                //显示缺页率等内容的标签
                TotalTime_B.Text = $"共用时间：{json["Result"]["TotalTime"]} ms";
                timeSpent = json["Result"]["TotalTime"].ToObject<int>();
                PageFaultCount_B.Text = $"缺页次数：{json["Result"]["PageFaultCount"]} 次";
                pageFaultCount = json["Result"]["PageFaultCount"].ToObject<int>();
                PageFaultRate_B.Text = $"缺页率：{json["Result"]["PageFaultRate"]} %";
                //各种历史记录保存
                MemoryQueue_History = JsonConvert.DeserializeObject<List<Queue<MemoryBlock>>>(json["MemoryQueue"]["Content"].ToString());
                Memory_History = JsonConvert.DeserializeObject<List<Dictionary<int, int>>>(json["Memory"]["Content"].ToString());
                QueueString_History = JsonConvert.DeserializeObject<List<string>>(json["QueueString"]["Content"].ToString());
                //保存队列展示模块内容
                string[] queueDisplay = json["QueueDisplay"]["Content"].ToString().Split('-');
                for (int i = 0; i < queueDisplay.Length; i++)
                {
                    queueTextBoxList[i].Text = queueDisplay[i];
                    if (queueDisplay[i].Contains("r"))
                    {
                        queueBorderList[i].BorderBrush = Brushes.Red;
                    }
                    else if (queueDisplay[i].Contains("v"))
                    {
                        queueBorderList[i].BorderBrush = Brushes.Violet;
                    }
                }
                Queue_Log.Text = json["QueueDisplay"]["LogContent"].ToString();
                //控件状态保存
                InputPanel.IsEnabled = json["OtherSettings"]["InputPanelIsEnabled"].ToObject<bool>();
                valueSlider.IsEnabled = json["OtherSettings"]["ValueSliderIsEnabled"].ToObject<bool>();
                PerviousStep.IsEnabled = json["OtherSettings"]["PerviousStepIsEnabled"].ToObject<bool>();
                NextStep.IsEnabled = json["OtherSettings"]["NextStepIsEnabled"].ToObject<bool>();
                (AutoPlay.Content as PackIcon).Kind = json["OtherSettings"]["PackIconKind"].ToObject<PackIconKind>();
                //保存算法执行步骤
                count_Arthmetic = json["OtherSettings"]["CountArthmetic"].ToObject<int>();
            }
            catch { }
        }
        /// <summary>
        /// 写配置,文件名为当前时间,格式为json
        /// </summary>
        private void SaveConfig(string filePath)
        {
            JObject json = new JObject//json模板
            {
                {"TimeSettings",new JObject
                    {
                        {"PageFault",GlobalVariable.PageFaultTime },
                        {"MemoryTime",GlobalVariable.MemoryTime },
                        {"TLBTime",GlobalVariable.TLBTime },
                        {"TLBUsing",GlobalVariable.TLBUsingState }
                    }
                },
                {
                    "OtherSettings",new JObject
                    {
                        {"MemoryBlockNum",GlobalVariable.MemoryBlockNum},
                        {"GeneratorNum",GlobalVariable.GeneratorNum },
                        {"AddressMax",GlobalVariable.AddressMax.ToString("X0") },//16进制
                        {"GenerateLogicAddress",GlobalVariable.GenerateLogicAddress },
                        {"InputAddresses",JsonConvert.SerializeObject(InputAddresses) },
                        {"InputPanelIsEnabled",InputPanel.IsEnabled },
                        {"ValueSliderIsEnabled",valueSlider.IsEnabled },
                        {"PerviousStepIsEnabled",PerviousStep.IsEnabled },
                        {"NextStepIsEnabled",NextStep.IsEnabled },
                        {"PackIconKind",JsonConvert.SerializeObject((AutoPlay.Content as PackIcon).Kind) },
                        {"CountArthmetic",count_Arthmetic }
                    }
                },
                {"InputAddresses",InputText.Text},
                {
                    "FIFOState",new JObject()
                    {
                        { "Header",""},
                        { "Content",new JArray() },
                        {"ResultVisable",false },
                        {"ResultContent","" }
                    }
                },
                {
                    "LRUState",new JObject()
                    {
                        { "Header",""},
                        { "Content",new JArray() },
                        {"ResultVisable",false },
                        {"ResultContent","" }
                    }
                },
                {
                    "OPTState",new JObject()
                    {
                        { "Header",""},
                        { "Content",new JArray() },
                        {"ResultVisable",false },
                        {"ResultContent","" }
                    }
                },
                {
                    "Arthmethic",new JObject()
                    {
                        { "ChoiceIndex",valueSlider.ArthmeticChoice.SelectedIndex }
                    }
                },
                {
                    "Result",new JObject()
                    {
                        {"TotalTime",timeSpent },
                        {"PageFaultCount",pageFaultCount },
                        {"PageFaultRate",(pageFaultCount/(double)InputAddresses.Count*100).ToString("f2") }
                    }
                },
                {
                    "QueueDisplay",new JObject()
                    {
                        {"Content","" },
                        {"LogContent","" }
                    }
                },
                {
                    "Memory",new JObject()
                    {
                        {"Content","" }
                    }
                },
                {
                    "MemoryQueue",new JObject()
                    {
                        {"Content","" }
                    }
                },
                {
                    "QueueString",new JObject()
                    {
                        {"Content","" }
                    }
                }
            };
            string[] nameLs = { "FIFOState", "LRUState", "OPTState" };
            StringBuilder sb;
            for (int i = 0; i < 3; i++)//WPF版本用不到 ,兼容角度考虑
            {
                //标题行处理
                StringBuilder headerText = new StringBuilder();
                foreach (var item in (MemoryContent_Group.Children[0] as StackPanel).Children)
                {
                    TextBlock element;
                    if (item as TextBlock == null)//强转失败 ,说明此文本块有 TransitioningContent 包裹
                    {
                        element = (item as TransitioningContent).Content as TextBlock;
                    }
                    else
                    {
                        element = item as TextBlock;
                    }
                    headerText.Append(element.Text);
                    headerText.Append("-");
                }
                headerText.Remove(headerText.Length - 1, 1);//抹去末尾的-
                json[nameLs[i]]["Header"] = headerText.ToString();

                //行处理,放入JArray (json数组)                
                for (int j = 1; j <= GlobalVariable.MemoryBlockNum + 1; j++)
                {
                    var c = (MemoryContent_Group.Children[j] as StackPanel).Children;
                    sb = new StringBuilder();
                    foreach (var item in c)
                    {
                        TextBlock element;
                        if (item as TextBlock == null)
                        {
                            element = (item as TransitioningContent).Content as TextBlock;
                        }
                        else
                        {
                            element = item as TextBlock;
                        }
                        sb.Append(element.Text);
                        if (element.Text == null || element.Text == "")//是空块
                        {
                            sb.Append(" ");
                        }

                        if (element.Foreground == Brushes.Red)//高亮块
                        {
                            sb.Append("r");
                        }
                        else if (element.Foreground == Brushes.Violet)//高亮块
                        {
                            sb.Append("v");
                        }

                        sb.Append("|");
                        if (element.ToolTip == null)
                        {
                            sb.Append(" ");
                        }
                        else
                        {
                            sb.Append(element.ToolTip.ToString());
                        }
                        sb.Append("-");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    (json[nameLs[i]]["Content"] as JArray).Add(sb.ToString());
                }
            }
            //队列展示模块
            sb = new StringBuilder();
            foreach (var item in queueTextBoxList)
            {
                if (string.IsNullOrEmpty(item.Text.Trim()))
                {
                    sb.Append(" ");
                }
                else
                {
                    sb.Append(item.Text);
                }
                sb.Append("-");
            }
            sb.Remove(sb.Length - 1, 1);//抹去末尾的-           
            json["QueueDisplay"]["Content"] = sb.ToString();
            json["QueueDisplay"]["LogContent"] = Queue_Log.Text;
            //各种历史保存
            json["QueueString"]["Content"] = JsonConvert.SerializeObject(QueueString_History);
            json["MemoryQueue"]["Content"] = JsonConvert.SerializeObject(MemoryQueue_History);
            json["Memory"]["Content"] = JsonConvert.SerializeObject(Memory_History);
            File.WriteAllText(filePath, json.ToString());
        }
        #endregion

        #region ---队列展示模块---
        /// <summary>
        /// 队列展示模块 ,生成默认边框内容
        /// </summary>
        private void GenerateQueueBorders()
        {
            //清除队列展示面板内的元素
            ArthmeticQueue.Children.Clear();
            queueTextBoxList = new List<TextBlock>();
            queueBorderList = new List<Border>();
            //多生成两个,表示导出以及进入的内存块
            for (int i = 0; i < GlobalVariable.MemoryBlockNum + 2; i++)
            {
                Border border = new Border()
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1)
                };
                TextBlock block = new TextBlock()
                {
                    FontSize = 36,
                    Width = 46,
                    Height = 46,
                    TextAlignment = TextAlignment.Center
                };
                border.Child = block;
                queueBorderList.Add(border);
                queueTextBoxList.Add(block);
                //设置块内容初始左边距
                Canvas.SetLeft(border, (queueTextBoxList.Count - 1) * 46);
                ArthmeticQueue.Children.Add(border);
            }
        }
        /// <summary>
        /// 算法每一步骤更新队列展示模块
        /// </summary>
        /// <param name="flag">发生缺页</param>
        /// <param name="blockNum">发生操作的页框号</param>
        private void UpdateQueueControl(bool flag, int blockNum)
        {
            MemoryBlock[] array = new MemoryBlock[10];
            int index = 0;
            switch (GlobalVariable.ChosenArthmetic)//获取内存队列副本
            {
                case Models.ArithmeticType.FIFO:
                    array = new MemoryBlock[GlobalVariable.MemoryQueue_FIFO.Count];
                    GlobalVariable.MemoryQueue_FIFO.CopyTo(array, 0);
                    break;
                case Models.ArithmeticType.LRU:
                    array = new MemoryBlock[GlobalVariable.MemoryQueue_LRU.Count];
                    GlobalVariable.MemoryQueue_LRU.CopyTo(array, 0);
                    break;
                case Models.ArithmeticType.OPT:
                    array = new MemoryBlock[GlobalVariable.MemoryQueue_OPT.Count];
                    GlobalVariable.MemoryQueue_OPT.CopyTo(array, 0);
                    break;
            }
            Queue_Log.Text += $"{(flag ? "发生了缺页" : "未发生缺页")} ,";
            //进行对队列展示模块中 TextBlock 内容进行更新
            if (count_Arthmetic <= GlobalVariable.MemoryBlockNum)//队列未满 ,直接放入
            {
                for (int i = 0; i < array.Length; i++)
                {
                    queueTextBoxList[i + 1].Text = array[i].PageNum.ToString("X0");
                }
            }
            else//队列已满,进行替换
            {
                foreach (var item in queueBorderList)//重置控件边框颜色
                {
                    item.BorderBrush = Brushes.Black;
                }
                if (flag)//发生缺页，进行动画
                {
                    //OPT算法需要计算某一块在之后出现的次数
                    if (GlobalVariable.ChosenArthmetic == Models.ArithmeticType.OPT)
                    {
                        Queue_Log.Text += $" ,之后每项出现的次数如下 :\n";
                        //求指令序列中,当前位置后,所有在队列中出现的页号出现的次数
                        Dictionary<int, int> count = new Dictionary<int, int>();
                        //读取上一内存块内容 ,因为更新队列展示模块是在执行完一步算法之后
                        //模拟上一步进行的操作
                        foreach (var item in MemoryQueue_History[MemoryQueue_History.Count - 1])
                        {
                            int numCount = 0;
                            //从输入的内存队列中取这一步之后的全部内容
                            foreach (var num in InputAddresses.GetRange(count_Arthmetic, InputAddresses.Count - count_Arthmetic))
                            {
                                if (Helper.GetPageBlock(num) == item.PageNum)
                                {
                                    numCount++;
                                }
                            }
                            count[item.PageNum] = numCount;
                        }
                        //输出次数计算结果
                        foreach (var item in count)
                        {
                            Queue_Log.Text += $"[{item.Key},{item.Value}] ";
                        }
                        Queue_Log.Text += "\n";
                        //按从小到大排序
                        var ResLs = count.OrderBy(x => x.Value).ToList();
                        Queue_Log.Text += $"按出现次数从小到大排序 ,";

                        index = 1;
                        foreach (var item in ResLs)
                        {
                            queueTextBoxList[index].Text = item.Key.ToString("X0");
                            index++;
                        }
                    }

                    //将容器内第一个方块取出,并放置到容器队列最后
                    Border firstElement = (Border)ArthmeticQueue.Children[0];
                    ArthmeticQueue.Children.Remove(firstElement);
                    queueBorderList.RemoveAt(0);
                    TextBlock firstBlock = firstElement.Child as TextBlock;
                    Queue_Log.Text += $"替换了 {queueTextBoxList[1].Text} ,";
                    firstBlock.Text = "";

                    //将数组同步更新
                    queueTextBoxList.RemoveAt(0);
                    ArthmeticQueue.Children.Add(firstElement);
                    queueBorderList.Add(firstElement);
                    queueTextBoxList.Add(firstBlock);
                    Canvas.SetLeft(firstElement, 46 * (GlobalVariable.MemoryBlockNum + 2));

                    //控件移动
                    Thread thread = new Thread(() =>
                    {
                        //容器内每个元素左移46像素
                        int widthSub = 0;
                        while (widthSub != 46)
                        {
                            ArthmeticQueue.Dispatcher.Invoke(() =>
                            {
                                foreach (var item in queueBorderList)
                                {
                                    Canvas.SetLeft(item, Canvas.GetLeft(item) - 1);
                                }
                            });
                            widthSub++;
                            Thread.Sleep(2);
                        }
                    }); thread.Start();
                }
            }
            if (flag is false)//未发生缺页,从内存队列中取出重复的块,并将边框强调
            {
                for (int i = 1; i < GlobalVariable.MemoryBlockNum + 1; i++)
                {
                    //根据最后一次结果寻找重复块
                    if (queueTextBoxList[i].Text == Memory_History[Memory_History.Count - 1][blockNum].ToString("X0"))
                    {
                        index = i;
                        break;
                    }
                }
                switch (GlobalVariable.ChosenArthmetic)
                {
                    case Models.ArithmeticType.FIFO:
                    case Models.ArithmeticType.OPT:
                        Queue_Log.Text += $"该页已存在于块 {blockNum} 中 ,";
                        queueBorderList[index].BorderBrush = Brushes.Red;
                        break;
                    case Models.ArithmeticType.LRU:
                        Queue_Log.Text += $"该页已存在于块 {blockNum} 中 ,并将此块移动至队列末尾 ,";
                        queueBorderList[index].BorderBrush = Brushes.Red;
                        queueBorderList[4].BorderBrush = Brushes.Violet;

                        index = 1;
                        foreach (var item in GlobalVariable.MemoryQueue_LRU)
                        {
                            queueTextBoxList[index].Text = item.PageNum.ToString("X0");
                            index++;
                        }
                        break;
                }
            }
            //将下一地址填入队列容器中最后一个格子中
            queueTextBoxList[queueTextBoxList.Count - 1].Text = count_Arthmetic
                == InputAddresses.Count ? "" : Helper.GetPageBlock(InputAddresses[count_Arthmetic]).ToString("X0");

            StringBuilder sb = new StringBuilder();
            index = 0;
            foreach (var item in queueTextBoxList)
            {
                if (string.IsNullOrEmpty(item.Text))
                {
                    sb.Append(" ");
                }
                else
                {
                    sb.Append(item.Text);
                }
                if (queueBorderList[index].BorderBrush == Brushes.Red)
                {
                    sb.Append("r");
                }
                else if (queueBorderList[index].BorderBrush == Brushes.Violet)
                {
                    sb.Append("v");
                }
                sb.Append("-");
                index++;
            }
            sb.Remove(sb.Length - 1, 1);
            QueueString_History.Add(sb.ToString());
        }

        #endregion

        #region ---算法相关---
        /// <summary>
        /// 自动进行FIFO算法
        /// </summary>
        private void MakeFIFOAuto()
        {
            for (int i = count_Arthmetic; i < InputAddresses.Count; i = count_Arthmetic)
            {
                MakeFIFOByStep(InputAddresses[i]);
            }
            //AddToolTip2Cells(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum, threadSleepTime);
        }
        /// <summary>
        /// 自动进行LRU算法
        /// </summary>
        private void MakeLRUAuto()
        {
            for (int i = count_Arthmetic; i < InputAddresses.Count; i = count_Arthmetic)
            {
                MakeLRUByStep(InputAddresses[i]);
            }
            //AddToolTip2Cells(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum, threadSleepTime);
        }
        /// <summary>
        /// 自动进行OPT算法
        /// </summary>
        private void MakeOPTAuto()
        {
            for (int i = count_Arthmetic; i < InputAddresses.Count; i = count_Arthmetic)
            {
                MakeOPTByStep(i);
            }
            //AddToolTip2Cells(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum, threadSleepTime);
        }
        /// <summary>
        /// 线程休眠处理
        /// </summary>
        /// <param name="flag">缺页标志</param>
        /// <returns>本次休眠时长</returns>
        private static int ThreadSleepByArithmeticResult(bool flag)
        {
            int TLBTimeBlock = GlobalVariable.TLBUsingState ? GlobalVariable.TLBTime : GlobalVariable.MemoryTime;
            int timeSpent = 0;
            Thread.Sleep(TLBTimeBlock);
            Thread.Sleep(GlobalVariable.MemoryTime);
            timeSpent += GlobalVariable.MemoryTime + TLBTimeBlock;
            if (flag)//缺页
            {
                Thread.Sleep(GlobalVariable.PageFaultTime);
                Thread.Sleep(TLBTimeBlock);
                Thread.Sleep(GlobalVariable.MemoryTime);

                timeSpent += GlobalVariable.PageFaultTime;
                timeSpent += GlobalVariable.MemoryTime + GlobalVariable.TLBTime;
            }
            return timeSpent;
        }
        /// <summary>
        /// 单步执行FIFO算法
        /// </summary>
        /// <param name="memoryAddress">需要操作的内存地址</param>
        /// <returns>本次步骤线程休眠的时间</returns>
        private int MakeFIFOByStep(int memoryAddress)
        {
            //执行算法
            bool flag = Arithmetic.MakeFIFO(memoryAddress, out int blockNum);
            int threadSleepTime = ThreadSleepByArithmeticResult(flag);
            //更新表格
            MemoryContent_Group.Dispatcher.Invoke(() =>
            {
                UpdateDataGrid(Models.ArithmeticType.FIFO, flag, blockNum);
                UpdateQueueControl(flag, blockNum);
                Queue_Log.Text += $"本步共耗时 {threadSleepTime} ms\n";
                Queue_Log.ScrollToVerticalOffset(Queue_Log.VerticalOffset + Queue_Log.ActualHeight);
            });
            if (flag)
            {
                pageFaultCount++;
            }
            timeSpent += threadSleepTime;
            MemoryQueue_History.Add(Helper.GetQueueClone(GlobalVariable.MemoryQueue_FIFO));
            Memory_History.Add(Helper.GetDictClone(GlobalVariable.Memory_FIFO));
            return blockNum;
        }
        /// <summary>
        /// 单步执行LRU算法
        /// </summary>
        /// <param name="memoryAddress">需要操作的内存地址</param>
        /// <returns>本次步骤线程休眠的时间</returns>
        private void MakeLRUByStep(int memoryAddress)
        {
            bool flag = Arithmetic.MakeLRU(memoryAddress, out int blockNum);
            int threadSleepTime = ThreadSleepByArithmeticResult(flag);
            MemoryContent_Group.Dispatcher.Invoke(() =>
            {
                UpdateDataGrid(Models.ArithmeticType.LRU, flag, blockNum);
                UpdateQueueControl(flag, blockNum);
                Queue_Log.Text += $"本步共耗时 {threadSleepTime} ms\n";
                Queue_Log.ScrollToVerticalOffset(Queue_Log.VerticalOffset + Queue_Log.ActualHeight);
            });
            if (flag)
            {
                pageFaultCount++;
            }
            timeSpent += threadSleepTime;
            MemoryQueue_History.Add(Helper.GetQueueClone(GlobalVariable.MemoryQueue_LRU));
            Memory_History.Add(Helper.GetDictClone(GlobalVariable.Memory_LRU));
        }
        /// <summary>
        /// 单步执行OPT算法
        /// </summary>
        /// <param name="memoryAddress">算法进行的步骤</param>
        /// <returns>本次步骤线程休眠的时间</returns>
        private void MakeOPTByStep(int index)
        {
            //执行算法
            bool flag = Arithmetic.MakeOPT(InputAddresses, index, out int blockNum);
            int threadSleepTime = ThreadSleepByArithmeticResult(flag);
            //更新表格
            MemoryContent_Group.Dispatcher.Invoke(() =>
            {
                UpdateDataGrid(Models.ArithmeticType.OPT, flag, blockNum);
                UpdateQueueControl(flag, blockNum);
                Queue_Log.Text += $"本步共耗时 {threadSleepTime} ms\n";
                Queue_Log.ScrollToVerticalOffset(Queue_Log.VerticalOffset + Queue_Log.ActualHeight);
            });
            if (flag)
            {
                pageFaultCount++;
            }
            timeSpent += threadSleepTime;
            MemoryQueue_History.Add(Helper.GetQueueClone(GlobalVariable.MemoryQueue_OPT));
            Memory_History.Add(Helper.GetDictClone(GlobalVariable.Memory_OPT));
        }
        /// <summary>
        /// 初始化算法线程内容
        /// </summary>
        /// <param name="type">算法类型</param>
        public Thread GetArthmeticThread(Models.ArithmeticType type)
        {
            Thread thread;
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    thread = new Thread(() =>
                    {
                        MakeFIFOAuto();
                        CalcResult();
                    });
                    return thread;
                case Models.ArithmeticType.LRU:
                    thread = new Thread(() =>
                    {
                        MakeLRUAuto();
                        CalcResult();
                    });
                    return thread;
                case Models.ArithmeticType.OPT:
                    thread = new Thread(() =>
                    {
                        MakeOPTAuto();
                        CalcResult();
                    });
                    return thread;
                default:
                    return null;
            }
        }
        #endregion

        #region ---控件更新相关---
        /// <summary>
        /// 重新生成主堆栈列表的列
        /// </summary>
        private void AddColunm2DataGrid()
        {
            //无效地址列表
            if (InputAddresses.Count == 0 || InputAddresses == null)
            {
                return;
            }
            else
            {
                var c = MemoryContent_Group.Children[0] as StackPanel;
                Helper.ClearStackPanelContent(c);
                Helper.ClearDataGridExceptHeader();
                for (int i = 0; i < InputAddresses.Count; i++)
                {
                    var item = Helper.GetPageBlock(InputAddresses[i]).ToString("X0");
                    Helper.AddString2StackPanel(item, 0);
                }
            }
        }
        /// <summary>
        /// 算法、环境变量初始化
        /// </summary>
        private void EnviromentInit()
        {
            //初始化历史记录模块     
            MemoryQueue_History = new List<Queue<MemoryBlock>>();
            Memory_History = new List<Dictionary<int, int>>();
            QueueString_History = new List<string>();
            //环境变量
            pageFaultCount = 0;
            timeSpent = 0;
            count_Arthmetic = 0;
            //标签内容初始化
            TotalTime_B.Text = "共用时间：0 ms";
            TotalTime_B.Text = "缺页次数：0 次";
            TotalTime_B.Text = "缺页率：0 %";
            //根据算法类型来初始化算法环境
            switch (GlobalVariable.ChosenArthmetic)
            {
                case Models.ArithmeticType.FIFO:
                    GlobalVariable.MemoryQueue_FIFO = new Queue<MemoryBlock>();
                    GlobalVariable.Memory_FIFO = new Dictionary<int, int>();
                    for (int i = 0; i < GlobalVariable.MemoryBlockNum; i++)//算法内存树 (页表) 需要手动添加
                    {
                        GlobalVariable.Memory_FIFO.Add(i + 1, -1);
                    }
                    break;
                case Models.ArithmeticType.LRU:
                    GlobalVariable.MemoryQueue_LRU = new Queue<MemoryBlock>();
                    GlobalVariable.Memory_LRU = new Dictionary<int, int>();
                    for (int i = 0; i < GlobalVariable.MemoryBlockNum; i++)
                    {
                        GlobalVariable.Memory_LRU.Add(i + 1, -1);
                    }
                    break;
                case Models.ArithmeticType.OPT:
                    GlobalVariable.MemoryQueue_OPT = new Queue<MemoryBlock>();
                    GlobalVariable.Memory_OPT = new Dictionary<int, int>();
                    for (int i = 0; i < GlobalVariable.MemoryBlockNum; i++)
                    {
                        GlobalVariable.Memory_OPT.Add(i + 1, -1);
                    }
                    break;
                case Models.ArithmeticType.All:
                    break;
            }
            Helper.ClearDataGridExceptHeader();
        }
        /// <summary>
        /// 恢复控件状态 (重置)
        /// </summary>
        private void RecoveryState()
        {
            count_Arthmetic = 0;
            AutoPlay.IsEnabled = true;
            PerviousStep.IsEnabled = false;
            NextStep.IsEnabled = true;
            (AutoPlay.Content as PackIcon).Kind = PackIconKind.Play;
            valueSlider.IsEnabled = true;
        }
        /// <summary>
        /// 计算缺页率并更新展示结果内容
        /// </summary>
        private void CalcResult()
        {
            TotalTime_B.Dispatcher.Invoke(() =>
            {
                AutoPlay.IsEnabled = false;
                Helper.ShowSnackBar("算法执行完成");
                TotalTime_B.Text = $"共用时间：{timeSpent} ms";
                PageFaultCount_B.Text = $"缺页次数：{pageFaultCount} 次";
                PageFaultRate_B.Text = $"缺页率：{pageFaultCount / (double)InputAddresses.Count * 100:f2} %";
                //RecoveryState();
            });
        }
        /// <summary>
        /// 更新堆栈面板的内容 ,包括添加新的文本块 ,更新颜色以及添加缺页状态
        /// </summary>
        /// <param name="type">算法类型</param>
        /// <param name="flag">是否缺页</param>
        /// <param name="blockNum">需要进行操作 (发生缺页、已在页表中) 的页块号</param>
        private void UpdateDataGrid(Models.ArithmeticType type, bool flag, int blockNum = 0)
        {
            count_Arthmetic++;
            Dictionary<int, int> Memory_Type = new Dictionary<int, int>();
            switch (type)
            {
                case Models.ArithmeticType.FIFO:
                    Memory_Type = GlobalVariable.Memory_FIFO;
                    break;
                case Models.ArithmeticType.LRU:
                    Memory_Type = GlobalVariable.Memory_LRU;
                    break;
                case Models.ArithmeticType.OPT:
                    Memory_Type = GlobalVariable.Memory_OPT;
                    break;
            }
            //读取算法内存
            for (int i = 0; i < GlobalVariable.MemoryBlockNum; i++)
            {
                int value = Memory_Type[i + 1];
                if (value != -1)
                {
                    //为需要高亮的块号                    
                    var block = Helper.AddString2StackPanel(value.ToString("X0"), i + 1, flag ? i == blockNum - 1 : false);
                    if (flag is false && i == blockNum - 1)
                    {
                        block.Foreground = Brushes.Violet;
                    }
                    //同步滚动条位置
                    Memory_ScrollView.ScrollToHorizontalOffset(Memory_ScrollView.HorizontalOffset + 40);
                }
                else
                {
                    Helper.AddString2StackPanel("", i + 1);
                }
            }
            //缺页标志
            Helper.AddString2StackPanel(flag ? "√" : "×", GlobalVariable.MemoryBlockNum + 1);

            if (count_Arthmetic > 0)
            {
                PerviousStep.IsEnabled = true;
            }
            if (count_Arthmetic == InputAddresses.Count)
            {
                NextStep.IsEnabled = false;
            }
        }
        #endregion

        #region ---杂项方法---
        /// <summary>
        /// 拉起WinForm ,并将当前窗口的状态传递给WinForm
        /// </summary>
        private void CallWinForm()
        {
            string filePath = Path.Combine(openFileDialog_Main.InitialDirectory, $"{DateTime.Now:yyyy-MM-dd.HH-mm-ss}.OSCD");
            SaveConfig(filePath);
            if (!File.Exists("OpreatingSystemClassDesign.exe"))//不存在WinForm版 ,从内嵌资源输出
            {
                ExtractResFile();
            }
            System.Diagnostics.Process.Start("OpreatingSystemClassDesign.exe", filePath);
            DialogMain.IsOpen = false;
        }
        /// <summary>
        /// 从嵌入资源导出 WinForm 版
        /// </summary>
        private static void ExtractResFile()
        {
            string outputFile = "OpreatingSystemClassDesign.exe";
            string resFileName = "OpreatingSystemClassDesignWPF.OpreatingSystemClassDesign.exe";
            Assembly asm = Assembly.GetExecutingAssembly(); //读取嵌入式资源
            BufferedStream inStream = new BufferedStream(asm.GetManifestResourceStream(resFileName));
            FileStream outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[1024];
            int length;
            while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outStream.Write(buffer, 0, length);
            }
            outStream.Flush();
            if (outStream != null)
            {
                outStream.Close();
            }
            if (inStream != null)
            {
                inStream.Close();
            }
        }
        #endregion
    }
}
