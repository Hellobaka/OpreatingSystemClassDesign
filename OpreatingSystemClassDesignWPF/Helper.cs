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
    public static class Helper
    {
        public static StackPanel StackPanel_Main;
        public static Snackbar SnackBar_Msg;
        private static readonly TransitionEffect effect = new TransitionEffect
        {
            Kind = TransitionEffectKind.FadeIn
        };

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
        public static StackPanel GetDefaultTemplateStackPanel(string content, Models.StackPanelColor color)
        {
            var panel = GetTemplateStackPanel(color);
            var block = GetTemplateTextBlock(content, false, 75);
            panel.Children.Add(block);
            return panel;
        }
        public static Models.StackPanelColor GetStackPanelColor(int index)
        {
            return (index % 2 == 0) ? Models.StackPanelColor.WhiteSmoke : Models.StackPanelColor.White;
        }
        public static TextBlock AddString2StackPanel(string content, int index, bool flag = false)
        {
            return AddString2StackPanel(content, StackPanel_Main.Children[index] as StackPanel, flag);
        }
        public static TextBlock AddString2StackPanel(string content, StackPanel stackPanel, bool flag = false)
        {
            TransitioningContent transitioning = new TransitioningContent();
            transitioning.OpeningEffect = effect;
            var block = GetTemplateTextBlock(content, flag);
            transitioning.Content = block;
            AddString2StackPanel(transitioning, stackPanel);
            return block;
        }
        public static void AddString2StackPanel(UIElement textBlock, StackPanel stackPanel)
        {
            stackPanel.Children.Add(textBlock);
        }
        public static TextBlock AddString2StackPanel(int content, int index)
        {
            return AddString2StackPanel(content.ToString(), index);
        }
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
        /// 读取一行地址文本，并转换为int类型数组
        /// </summary>
        /// <returns></returns>
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
        /// 返回页号
        /// </summary>
        /// <param name="address">逻辑或者物理地址</param>
        /// <returns></returns>
        public static int GetPageBlock(int address)
        {
            return address >> 12;
        }
        /// <summary>
        /// 从内存队列中移除目标PageNum为参数的元素，并返回被移除的元素
        /// </summary>
        /// <param name="value">PageNum</param>
        /// <returns></returns>
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
        public static Queue<MemoryBlock> GetQueueClone(Queue<MemoryBlock> queue)
        {
            Queue<MemoryBlock> tmp = new Queue<MemoryBlock>();
            foreach(var item in queue)
            {
                tmp.Enqueue(item);
            }
            return tmp;
        }
        public static Dictionary<int,int> GetDictClone(Dictionary<int, int> dict)
        {
            Dictionary<int, int> tmp = new Dictionary<int, int>();
            foreach (var item in dict)
            {
                tmp.Add(item.Key,item.Value);
            }
            return tmp;
        }
        public static UIElement GetParentLoop(UIElement uiElement, int count)
        {
            for (int i = 0; i < count; i++)
            {
                uiElement = GetParent(uiElement);
            }
            return uiElement;
        }
        public static UIElement GetParent(UIElement uiElement)
        {
            return (UIElement)VisualTreeHelper.GetParent(uiElement);
        }
        public static void ClearDataGridExceptHeader()
        {
            for (int i = 1; i <= GlobalVariable.MemoryBlockNum + 1; i++)
            {
                Helper.ClearStackPanelContent(StackPanel_Main.Children[i] as StackPanel);
            }
        }
        public static void RemoveChildrenAt(StackPanel stackPanel, int index)
        {
            stackPanel.Children.RemoveAt(index);
        }
        public static void RemoveLastChild(StackPanel stackPanel)
        {
            RemoveChildrenAt(stackPanel, stackPanel.Children.Count - 1);
        }
        private static bool GetSnackBarState()
        {
            return SnackBar_Msg.IsActive;
        }
        public static void ShowSnackBar(string msg, int time = 1000)
        {
            Thread thread = new Thread(() =>
            {
                bool flag = false;
                do
                {
                    SnackBar_Msg.Dispatcher.Invoke(() => { flag = GetSnackBarState(); });
                    Thread.Sleep(100);
                } while (flag);
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
    }
}
