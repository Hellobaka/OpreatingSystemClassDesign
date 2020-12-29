using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;

namespace OpreatingSystemClassDesignWPF
{
    /// <summary>
    /// 核心帮助类 内容为对堆栈面板、模板文本框、清除其中成员等
    /// </summary>
    public static class Helper
    {
        #region ---私有静态变量---
        /// <summary>
        /// 过渡效果动画类型
        /// </summary>
        private static readonly TransitionEffect effect = new TransitionEffect
        {
            Kind = TransitionEffectKind.FadeIn
        };
        #endregion

        #region ---公有静态变量---
        /// <summary>
        /// 与主界面同步的主堆栈面板(存放内存框),方便类内调用更新
        /// </summary>
        public static StackPanel StackPanel_Main;
        /// <summary>
        /// 与主界面同步的消息框,方便类内调用展示消息
        /// </summary>
        public static Snackbar SnackBar_Msg;
        #endregion

        #region ---文本框操作---
        /// <summary>
        /// 获取模板文本块 (字号 16 ,内边距 3 ,文本居中 ,宽度默认 40)
        /// </summary>
        /// <param name="content">其中的内容</param>
        /// <param name="flag">文本颜色标识 :true 为红色 ,false 为黑色</param>
        /// <param name="width">块宽度 ,默认为 40 像素</param>
        public static TextBlock GetTemplateTextBlock(string content, bool flag = false, int width = 40)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = content,
                FontSize = 16,
                Padding = new Thickness(3),
                Width = width,
                TextAlignment = TextAlignment.Center
            };
            textBlock.Foreground = flag ? Brushes.Red : Brushes.Black;
            return textBlock;
        }
        /// <summary>
        /// 获取消息框占用状态
        /// </summary>
        private static bool GetSnackBarState()
        {
            return SnackBar_Msg.IsActive;
        }
        /// <summary>
        /// 通过 SnackBar 展示指定文本信息 ,默认展示 1 s
        /// </summary>
        /// <param name="msg">需要进行展示的文本消息</param>
        /// <param name="time">消息展示时长 (单位 ms)</param>
        public static void ShowSnackBar(string msg, int time = 1000)
        {
            //涉及不断判断状态 ,需要独立线程防止阻塞UI线程的进行
            Thread thread = new Thread(() =>
            {
                bool flag = false;
                do
                {
                    //通过消息框的调度器执行方法 ,避免跨线程报错
                    SnackBar_Msg.Dispatcher.Invoke(() => { flag = GetSnackBarState(); });
                    Thread.Sleep(100);
                } while (flag);//直到消息框空闲
                SnackBar_Msg.Dispatcher.Invoke(() =>
                {
                    SnackBar_Msg.Message = new SnackbarMessage();
                    SnackBar_Msg.Message.Content = msg;
                    SnackBar_Msg.IsActive = true;
                });
                Thread.Sleep(time);
                SnackBar_Msg.Dispatcher.Invoke(() =>
                {
                    SnackBar_Msg.IsActive = false;
                });
            });
            thread.Start();
        }

        #endregion

        #region ---堆栈面板操作---
        /// <summary>
        /// 获取默认横向堆栈面板 (用于填充一行)
        /// </summary>
        /// <param name="color">面板的背景色</param>
        public static StackPanel GetTemplateStackPanel(Models.StackPanelColor color)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            switch (color)
            {
                case Models.StackPanelColor.White:
                    stackPanel.Background = Brushes.White;
                    break;
                case Models.StackPanelColor.WhiteSmoke:
                    stackPanel.Background = Brushes.WhiteSmoke;
                    break;
            }
            return stackPanel;
        }
        /// <summary>
        /// 获取带有一个标题级宽度文本块内容的横向堆栈面板 (面板内第一个child为 75 长度 ,内容为参数的TextBlock) ,并指定背景颜色
        /// </summary>
        /// <param name="content">文本块内容</param>
        /// <param name="color">面板背景颜色</param>
        public static StackPanel GetDefaultTemplateStackPanel(string content, Models.StackPanelColor color)
        {
            var panel = GetTemplateStackPanel(color);
            var block = GetTemplateTextBlock(content, false, 75);
            panel.Children.Add(block);
            return panel;
        }
        /// <summary>
        /// 通过下标获取面板背景颜色 ,规律为 奇数为白色 ,偶数为灰色背景
        /// </summary>
        /// <param name="index">当前位置</param>
        public static Models.StackPanelColor GetStackPanelColor(int index)
        {
            return (index % 2 == 0) ? Models.StackPanelColor.WhiteSmoke : Models.StackPanelColor.White;
        }
        /// <summary>
        /// 通过指定第几行 ,向指定位置的堆栈面板中添加一个默认长度 (40) 内容为参数的文本块
        /// </summary>
        /// <param name="content">文本块内容</param>
        /// <param name="index">主面板中的第几个位置</param>
        /// <param name="flag">文本颜色标识 ,true为红色 ,false为黑色</param>
        public static TextBlock AddString2StackPanel(string content, int index, bool flag = false)
        {
            return AddString2StackPanel(content, StackPanel_Main.Children[index] as StackPanel, flag);
        }
        /// <summary>
        /// 通过直接指定 ,向指定的堆栈面板添加一个默认长度 (40) 内容为参数 ,并且带有过渡动画效果的文本块
        /// </summary>
        /// <param name="content">文本块内容</param>
        /// <param name="stackPanel">直接指定的堆栈面板</param>
        /// <param name="flag">文本颜色标识 ,true为红色 ,false为黑色</param>
        public static TextBlock AddString2StackPanel(string content, StackPanel stackPanel, bool flag = false)
        {
            TransitioningContent transitioning = new TransitioningContent();
            transitioning.OpeningEffect = effect;
            var block = GetTemplateTextBlock(content, flag);
            transitioning.Content = block;
            AddString2StackPanel(transitioning, stackPanel);
            return block;
        }
        /// <summary>
        /// 向指定的堆栈面板中添加自定义形式的控件元素
        /// </summary>
        /// <param name="textBlock">自定义内容的控件 ,基于 UIElement 的都可以</param>
        /// <param name="stackPanel">指定的堆栈面板</param>
        public static void AddString2StackPanel(UIElement textBlock, StackPanel stackPanel)
        {
            stackPanel.Children.Add(textBlock);
        }
        /// <summary>
        /// 向主堆栈面板中指定下标的面板中添加一个数字为内容的默认样式文本块
        /// </summary>
        /// <param name="content">数字内容</param>
        /// <param name="index">指定下标</param>
        public static TextBlock AddString2StackPanel(int content, int index)
        {
            return AddString2StackPanel(content.ToString(), index);
        }
        /// <summary>
        /// 清除堆栈面板的内容 ,除了第一个块
        /// </summary>
        /// <param name="stackPanel">需要进行清除的堆栈面板</param>
        public static void ClearStackPanelContent(StackPanel stackPanel)
        {
            int count = stackPanel.Children.Count;
            for (int i = 1; i < count; i++)
            {
                var item = stackPanel.Children[1];
                item = null;
                stackPanel.Children.RemoveAt(1);
            }
            GC.Collect();
        }
        /// <summary>
        /// 获取DialogHost内容 ,默认生成一个带有参数文本以及 "是" 与 "否" 按钮的StackPanel ,用于填充DialogHost.Content
        /// </summary>
        /// <param name="content">用于显示提示信息</param>
        /// <returns></returns>
        public static StackPanel GetYesNoDialogContent(string content)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Thickness(20)
            };
            TextBlock textBlock = new TextBlock
            {
                Text = content,
            };
            stackPanel.Children.Add(textBlock);
            Button Yes = new Button
            {
                Content = "是",
                Margin = new Thickness(35, 20, 30, 0),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Button No = new Button
            {
                Content = "否",
                Margin = new Thickness(30, 20, 35, 0),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            StackPanel Horizonpanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            Horizonpanel.Children.Add(Yes);
            Horizonpanel.Children.Add(No);
            stackPanel.Children.Add(Horizonpanel);
            return stackPanel;
        }
        /// <summary>
        /// 通过循环获取指定元素的 n 级父元素
        /// </summary>
        /// <param name="uiElement">需要获取父元素的子元素</param>
        /// <param name="count">向上寻找的次数</param>
        /// <returns>寻找到的父元素</returns>
        public static UIElement GetParentLoop(UIElement uiElement, int count)
        {
            for (int i = 0; i < count; i++)
            {
                uiElement = GetParent(uiElement);
            }
            return uiElement;
        }
        /// <summary>
        /// 调用 VisualTreeHelper 的 GetParent 方法获取元素的父元素
        /// </summary>
        /// <param name="uiElement">需要获取父元素的子元素</param>
        /// <returns>父元素</returns>
        public static UIElement GetParent(UIElement uiElement)
        {
            return (UIElement)VisualTreeHelper.GetParent(uiElement);
        }
        /// <summary>
        /// 清空主堆栈面板内容 ,仅剩下最左侧以及顶部的标题文本
        /// </summary>
        public static void ClearDataGridExceptHeader()
        {
            for (int i = 1; i <= StackPanel_Main.Children.Count -1 ; i++)
            {
                Helper.ClearStackPanelContent(StackPanel_Main.Children[i] as StackPanel);
            }
        }
        /// <summary>
        /// 通过下标移除堆栈面板的元素
        /// </summary>
        /// <param name="stackPanel">需要进行移除的堆栈面板</param>
        /// <param name="index">指定下标</param>
        public static void RemoveChildrenAt(StackPanel stackPanel, int index)
        {
            stackPanel.Children.RemoveAt(index);
        }
        /// <summary>
        /// 移除堆栈面板的最后一个元素
        /// </summary>
        /// <param name="stackPanel">需要进行移除的面板</param>
        public static void RemoveLastChild(StackPanel stackPanel)
        {
            RemoveChildrenAt(stackPanel, stackPanel.Children.Count - 1);
        }

        #endregion

        #region ---内存操作---
        /// <summary>
        /// 读取一行地址文本 (限定0xFFFF以下) ,并返回转换后的 int 类型数组，若无法进行转换，将会通过 SnackBar 提示错误字符
        /// </summary>
        public static List<int> ReadAddress(string input)
        {
            List<int> MemoryAddresses = new List<int>();
            var str = input.Split(' ');
            for (int i = 0; i < str.Length; i++)
            {
                str[i] = str[i].Replace("H", "");
            }
            foreach (var item in str)
            {
                try
                {
                    if (item.Length >= 4)
                    {
                        MemoryAddresses.Add(Convert.ToInt32(item, 16));
                    }
                    else if (item.Length == 1)
                    {
                        MemoryAddresses.Add(Convert.ToInt32(item + "000", 16));
                    }
                    else
                    {
                        throw new InvalidCastException();
                    }
                }
                catch
                {
                    ShowSnackBar($"非法字符：{item}", 1000);
                    return null;
                }
            }
            return MemoryAddresses;
        }
        /// <summary>
        /// 返回逻辑或者物理地址的页号
        /// </summary>
        /// <param name="address">逻辑或者物理地址</param>
        /// <returns></returns>
        public static int GetPageBlock(int address)
        {
            return address >> 12;
        }
        /// <summary>
        /// 从内存队列中移除目标 PageNum 为参数的元素，保持队列顺序，并返回被移除的元素
        /// </summary>
        /// <param name="PageNum">需要移除的元素</param>
        /// <param name="MemoryQueue">需要进行操作的内存队列</param>
        public static MemoryBlock RemoveValueFromMemoryQueue(int PageNum, ref Queue<MemoryBlock> MemoryQueue)
        {
            Queue<MemoryBlock> tmpQueue = new Queue<MemoryBlock>();
            MemoryBlock removeItem = new MemoryBlock();
            do
            {
                var c = MemoryQueue.Dequeue();
                if (c.PageNum != PageNum)
                {
                    tmpQueue.Enqueue(c);
                }
                else
                {
                    removeItem = c;
                }
            } while (MemoryQueue.Count != 0);
            MemoryQueue = tmpQueue;
            return removeItem;
        }
        /// <summary>
        /// 获取队列的副本
        /// </summary>
        public static Queue<MemoryBlock> GetQueueClone(Queue<MemoryBlock> queue)
        {
            Queue<MemoryBlock> tmp = new Queue<MemoryBlock>();
            foreach (var item in queue)
            {
                tmp.Enqueue(item);
            }
            return tmp;
        }
        /// <summary>
        /// 获取字典的副本
        /// </summary>
        public static Dictionary<int, int> GetDictClone(Dictionary<int, int> dict)
        {
            Dictionary<int, int> tmp = new Dictionary<int, int>();
            foreach (var item in dict)
            {
                tmp.Add(item.Key, item.Value);
            }
            return tmp;
        }
        #endregion
    }
}
