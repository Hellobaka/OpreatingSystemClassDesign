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
        private Thread Thread_Arthmetic;
        /// <summary>
        /// 输入的地址数组
        /// </summary>
        private static List<int> InputAddresses { get; set; }
        /// <summary>
        /// 算法进行的步数
        /// </summary>
        private int count_Arthmetic { get; set; } = 0;
        private static bool TLBUsing_Flag = true;
        private List<Queue<MemoryBlock>> MemoryQueue_History { get; set; } = new List<Queue<MemoryBlock>>();
        private List<Dictionary<int, int>> Memory_History = new List<Dictionary<int, int>>();
        private List<string> QueueString_History = new List<string>();
        /// <summary>
        /// 缺页次数
        /// </summary>
        private int pageFaultCount = 0;
        /// <summary>
        /// 使用的时间
        /// </summary>
        private int timeSpent = 0;
        List<Border> queueBorderList = new List<Border>();
        List<TextBlock> queueTextBoxList = new List<TextBlock>();
        OpenFileDialog openFileDialog_Main = new OpenFileDialog();
        #endregion

        private void CloseWindows_Click(object sender, RoutedEventArgs e)
        {
            var parent = Helper.GetYesNoDialogContent("确认要关闭吗，未保存的工作都将丢失");
            Button bt = (parent.Children[1] as StackPanel).Children[0] as Button;
            bt.Click += (s, ev) => { Close(); };
            bt = (parent.Children[1] as StackPanel).Children[1] as Button;
            bt.Click += (s, ev) => { DialogMain.IsOpen = false; };
            DialogMain.DialogContent = parent;
            DialogMain.IsOpen = true;
        }

        private void ColorZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

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
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(openFileDialog_Main.InitialDirectory, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.OSCD");
            SaveConfig(filePath);
            MessageBox.Show("保存完毕");
            System.Diagnostics.Process.Start(openFileDialog_Main.InitialDirectory);
        }
        private void OpenFile()
        {
            DialogMain.IsOpen = false;
            openFileDialog_Main.ShowDialog();
            string filePath = openFileDialog_Main.FileName;
            if (string.IsNullOrEmpty(filePath))
                return;
            MemoryContent_Group.Children.Clear();
            ReadConfig(filePath);
        }
        /// <summary>
        /// 从文件中读取配置，并将全部界面元素更改
        /// </summary>
        /// <param name="filePath"></param>
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
                valueSlider.ArthmeticChoice.SelectedIndex = arthmeticIndex;
                InputAddresses = JsonConvert.DeserializeObject<List<int>>(json["OtherSettings"]["InputAddresses"].ToString());
            }
            catch { }
            //序列输入框
            InputText.Text = json["InputAddresses"].ToString();
            //算法内容展示部分
            //控件放入数组方便循环操作
            string[] nameLs = { "FIFOState", "LRUState", "OPTState" };
            //Helper.ClearDataGridExceptHeader();
            //读取标题行
            string headerReader = json[nameLs[arthmeticIndex]]["Header"].ToString();
            //由于表格的第一列与其他列的宽度不一样,需要进行区别
            bool first = true;
            StackPanel header = Helper.GetDefaultTemplateStackPanel("页框号"
                , Models.StackPanelColor.White);
            MemoryContent_Group.Children.Add(header);
            foreach (var item in headerReader.Split('-'))
            {
                if (first)
                {
                    first = false;
                    continue;
                }
                Helper.AddString2StackPanel(item, header);
            }
            int index = 0;
            //表格行内容
            foreach (var item in json[nameLs[arthmeticIndex]]["Content"] as JArray)
            {
                var array = item.ToString().Split('-').ToList();
                StackPanel tmp = Helper.GetDefaultTemplateStackPanel(array[0].Substring(0, array[0].IndexOf('|'))
                    , Models.StackPanelColor.White);
                foreach (var content in array.GetRange(1, array.Count - 1))
                {
                    string toolTip = content.Split('|')[1];
                    var c = Helper.GetTemplateTextBlock(content.Substring(0, content.IndexOf('|')).Replace("r", "").Replace("v", ""));
                    //高亮字符
                    if (content.Contains("r"))
                    {
                        c.Foreground = Brushes.Red;
                    }
                    else if (content.Contains("v"))
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
                //
                MemoryQueue_History = JsonConvert.DeserializeObject<List<Queue<MemoryBlock>>>(json["MemoryQueue"]["Content"].ToString());
                Memory_History = JsonConvert.DeserializeObject<List<Dictionary<int, int>>>(json["Memory"]["Content"].ToString());
                QueueString_History = JsonConvert.DeserializeObject<List<string>>(json["QueueString"]["Content"].ToString());
                //
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
                InputPanel.IsEnabled = json["OtherSettings"]["InputPanelIsEnabled"].ToObject<bool>();
                valueSlider.IsEnabled = json["OtherSettings"]["ValueSliderIsEnabled"].ToObject<bool>();
                PerviousStep.IsEnabled = json["OtherSettings"]["PerviousStepIsEnabled"].ToObject<bool>();
                NextStep.IsEnabled = json["OtherSettings"]["NextStepIsEnabled"].ToObject<bool>();
                (AutoPlay.Content as PackIcon).Kind = json["OtherSettings"]["PackIconKind"].ToObject<PackIconKind>();
                count_Arthmetic = json["OtherSettings"]["CountArthmetic"].ToObject<int>();
            }
            catch { }
        }
        /// <summary>
        /// 写配置,文件名为当前时间,格式为json
        /// </summary>
        private void SaveConfig(string filePath)
        {
            JObject json = new JObject
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
                        {"Content","" }
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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                //标题行处理
                StringBuilder headerText = new StringBuilder();
                foreach (var item in (MemoryContent_Group.Children[0] as StackPanel).Children)
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
                        else if (element.Foreground == Brushes.Violet)
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
                    for (int q = 0; q < InputAddresses.Count - c.Count; q++)
                    {
                        sb.Append(" | ");
                        sb.Append("-");
                    }

                    sb.Remove(sb.Length - 1, 1);
                    (json[nameLs[i]]["Content"] as JArray).Add(sb.ToString());
                }
            }

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
            json["QueueString"]["Content"] = JsonConvert.SerializeObject(QueueString_History);
            json["MemoryQueue"]["Content"] = JsonConvert.SerializeObject(MemoryQueue_History);
            json["Memory"]["Content"] = JsonConvert.SerializeObject(Memory_History);
            File.WriteAllText(filePath, json.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("SaveFiles"))
            {
                Directory.CreateDirectory("SaveFiles");
            }
            Helper.StackPanel_Main = MemoryContent_Group;
            Helper.SnackBar_Msg = SnackBar_Msg;
            MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel("页框号"
                , Models.StackPanelColor.White));
            for (int i = 1; i <= 4; i++)
            {
                MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel(i.ToString()
                    , Helper.GetStackPanelColor(i)));
            }
            MemoryContent_Group.Children.Add(Helper.GetDefaultTemplateStackPanel("是否缺页"
                , Helper.GetStackPanelColor(MemoryContent_Group.Children.Count)));
            InputAddresses = Helper.ReadAddress(InputText.Text);
            AddColunm2DataGrid();
            EnviromentInit();
            GenerateQueueBorders();

            GlobalVariable.OnNumChange += TimeSettings_OnNumChange;
            GlobalVariable.OnMemoryBlockNumChange += OnMemoryBlockNumChange;
            GlobalVariable.OnArthmeticChange += OnArthmeticChange;
            GlobalVariable.OnTLBUsingStateChange += OnTLBUsingStateChange;

            openFileDialog_Main.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveFiles");

            Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void GenerateQueueBorders()
        {
            ArthmeticQueue.Children.Clear();
            queueTextBoxList = new List<TextBlock>();
            queueBorderList = new List<Border>();
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
                Canvas.SetLeft(border, (queueTextBoxList.Count - 1) * 46);
                ArthmeticQueue.Children.Add(border);
            }
        }

        private void OnTLBUsingStateChange(object sender, EventArgs e)
        {
            TLBUsingState_B.Text = $"是否使用快表：{GlobalVariable.TLBUsingState}";
        }

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
                case Models.ArithmeticType.All:
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
            EnviromentInit();
            GenerateQueueBorders();
        }
        private void CallWinForm()
        {
            string filePath = Path.Combine(openFileDialog_Main.InitialDirectory, $"{DateTime.Now:yyyy-MM-dd.HH-mm-ss}.OSCD");
            SaveConfig(filePath);
            if (!File.Exists("OpreatingSystemClassDesign.exe"))
            {
                ExtractResFile();
            }
            System.Diagnostics.Process.Start("OpreatingSystemClassDesign.exe", filePath);
            DialogMain.IsOpen = false;
        }

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

        private void OnMemoryBlockNumChange(object sender, EventArgs e)
        {
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

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            MessageBox.Show($"捕获到未处理的异常，发生在非UI线程中，单击确定以退出程序：{exception.Message}" +
                $"\n{exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"捕获到未处理的异常，发生在UI线程中，单击确定以忽略：{e.Exception.Message}\n{e.Exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            valueSlider.Margin = new Thickness(14, -358, 0, 0);
        }

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
        private void AddColunm2DataGrid()
        {
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

        private void AutoPlay_Click(object sender, RoutedEventArgs e)
        {
            var p = AutoPlay.Content as PackIcon;
            if (p.Kind == PackIconKind.Play)
            {
                p.Kind = PackIconKind.Pause;
                valueSlider.IsEnabled = false;
                InputPanel.IsEnabled = false;
                if (Thread_Arthmetic != null && Thread_Arthmetic.ThreadState == ThreadState.Suspended)
                    Thread_Arthmetic.Resume();
                else
                {
                    Thread_Arthmetic = GetArthmeticThread(GlobalVariable.ChosenArthmetic);
                    Thread_Arthmetic.Start();
                }
            }
            else
            {
                p.Kind = PackIconKind.Play;
                if (Thread_Arthmetic.ThreadState == ThreadState.Running
                    || Thread_Arthmetic.ThreadState == ThreadState.WaitSleepJoin)
                    Thread_Arthmetic.Suspend();
            }
        }

        private void EnviromentInit()
        {
            //算法环境初始化
            pageFaultCount = 0;
            timeSpent = 0;
            MemoryQueue_History = new List<Queue<MemoryBlock>>();
            Memory_History = new List<Dictionary<int, int>>();
            QueueString_History = new List<string>();
            TotalTime_B.Text = "共用时间：0 ms";
            TotalTime_B.Text = "缺页次数：0 次";
            TotalTime_B.Text = "缺页率：0 %";
            switch (GlobalVariable.ChosenArthmetic)
            {
                case Models.ArithmeticType.FIFO:
                    GlobalVariable.MemoryQueue_FIFO = new Queue<MemoryBlock>();
                    GlobalVariable.Memory_FIFO = new Dictionary<int, int>();
                    for (int i = 0; i < GlobalVariable.MemoryBlockNum; i++)
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

        private void RecoveryState()
        {
            count_Arthmetic = 0;
            AutoPlay.IsEnabled = true;
            PerviousStep.IsEnabled = false;
            NextStep.IsEnabled = true;
            (AutoPlay.Content as PackIcon).Kind = PackIconKind.Play;
            valueSlider.IsEnabled = true;
        }
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

        private void MakeFIFOAuto()
        {
            for (int i = count_Arthmetic; i < InputAddresses.Count; i = count_Arthmetic)
            {
                MakeFIFOByStep(InputAddresses[i]);
            }
            //AddToolTip2Cells(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum, threadSleepTime);
        }
        private void MakeLRUAuto()
        {
            for (int i = count_Arthmetic; i < InputAddresses.Count; i = count_Arthmetic)
            {
                MakeLRUByStep(InputAddresses[i]);
            }
            //AddToolTip2Cells(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum, threadSleepTime);
        }
        private void MakeOPTAuto()
        {
            for (int i = count_Arthmetic; i < InputAddresses.Count; i = count_Arthmetic)
            {
                MakeOPTByStep(i);
            }
            //AddToolTip2Cells(dataGridView_FIFO, Models.ArithmeticType.FIFO, flag, blockNum, threadSleepTime);
        }
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
                    Memory_ScrollView.ScrollToHorizontalOffset(Memory_ScrollView.HorizontalOffset + 40);
                }
                else
                {
                    Helper.AddString2StackPanel("", i + 1);
                }
                //if (count_Type >= 28)
                //{
                //    DGV.FirstDisplayedCell = DGV.Rows[i].Cells[count_Type - 28];
                //}
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
        /// <summary>
        /// 算法每一步骤更新队列展示控件
        /// </summary>
        /// <param name="flag">发生缺页</param>
        /// <param name="blockNum">发生操作的页框号</param>
        private void UpdateQueueControl(bool flag, int blockNum)
        {
            MemoryBlock[] array = new MemoryBlock[10];
            int index = 0;
            switch (GlobalVariable.ChosenArthmetic)
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

            if (count_Arthmetic <= GlobalVariable.MemoryBlockNum)//队列未满,直接放入
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
                    if (GlobalVariable.ChosenArthmetic == Models.ArithmeticType.OPT)
                    {
                        Queue_Log.Text += $" ,之后每项出现的次数如下 :\n";
                        //求指令序列中,当前位置后,所有在队列中出现的页号出现的次数
                        Dictionary<int, int> count = new Dictionary<int, int>();
                        foreach (var item in MemoryQueue_History[MemoryQueue_History.Count - 1])
                        {
                            int numCount = 0;
                            foreach (var num in InputAddresses.GetRange(count_Arthmetic, InputAddresses.Count - count_Arthmetic))
                            {
                                if (Helper.GetPageBlock(num) == item.PageNum)
                                {
                                    numCount++;
                                }
                            }
                            count[item.PageNum] = numCount;
                        }
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
        /// <summary>
        /// 线程休眠处理
        /// </summary>
        /// <param name="flag">缺页标志</param>
        /// <returns>本次休眠时长</returns>
        private static int ThreadSleepByArithmeticResult(bool flag)
        {
            int TLBTimeBlock = TLBUsing_Flag ? GlobalVariable.TLBTime : GlobalVariable.MemoryTime;
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

        private void InputText_LostFocus(object sender, RoutedEventArgs e)
        {
            var c = Helper.ReadAddress(InputText.Text);
            if (c != null)
            {
                InputAddresses = c;
                AddColunm2DataGrid();
            }
        }

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

        private void PerviousStep_Click(object sender, RoutedEventArgs e)
        {
            NextStep.IsEnabled = true;
            count_Arthmetic--;
            if (count_Arthmetic == 0)
            {
                PerviousStep.IsEnabled = false;
                EnviromentInit();
                return;
            }
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
            MemoryQueue_History.RemoveAt(MemoryQueue_History.Count - 1);
            Memory_History.RemoveAt(Memory_History.Count - 1);
            QueueString_History.RemoveAt(QueueString_History.Count - 1);

            for (int i = 1; i <= GlobalVariable.MemoryBlockNum + 1; i++)
            {
                Helper.RemoveLastChild(MemoryContent_Group.Children[i] as StackPanel);
            }
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
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
        private void MakeLRUByStep(int memoryAddress)
        {
            //执行算法
            bool flag = Arithmetic.MakeLRU(memoryAddress, out int blockNum);
            int threadSleepTime = ThreadSleepByArithmeticResult(flag);
            //更新表格
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
    }
}
