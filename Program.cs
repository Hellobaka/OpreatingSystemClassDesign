using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpreatingSystemClassDesign
{
    public class MemoryBlock
    {
        /// <summary>
        /// 页框号，未分配写-1
        /// </summary>
        public int PageFrameNum;
        /// <summary>
        /// 地址的页号
        /// </summary>
        public int PageNum;
    }
    class Program
    {
        /// <summary>
        /// 内存处理队列
        /// </summary>
        private static Queue<MemoryBlock> MemoryQueue = new Queue<MemoryBlock>();
        /// <summary>
        /// 内存页框内容,未分配内容写-1
        /// </summary>
        private static Dictionary<int, int> Memory = new Dictionary<int, int>
        {
            {1,-1 },
            {3,-1 },
            {4,-1 },
            {7,-1 }
        };
        static void Main(string[] args)
        {
            MainForm fm = new MainForm();
            fm.Show();
            List<int> InputAddress = ReadAddress();
            Console.WriteLine($"序号\t输入\t1\t3\t4\t7\t是否缺页");
            int pageFaultCount = 0;
            for (int i = 0; i < InputAddress.Count; i++)
            {
                Console.Write($"{i + 1}\t{GetPageBlock(InputAddress[i]):X0}\t");
                bool flag = MakeOPT(InputAddress, i);
                //bool flag = MakeLRU(InputAddress[i]);
                DoUI(flag);
                if (flag)
                    pageFaultCount++;
            }
            Console.WriteLine($"缺页次数：{pageFaultCount}/{InputAddress.Count}\t" +
                $"缺页率：{pageFaultCount / (double)InputAddress.Count * 100:f2}%");
            Console.ReadKey();
        }
        /// <summary>
        /// 生成UI
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static void DoUI(bool pageFault)
        {
            int count = 0;
            foreach (var item in Memory)
            {
                if (item.Value != -1)
                    count++;
            }
            int[] keyArray = { 1, 3, 4, 7 };
            for (int i = 0; i < 4; i++)
            {
                if (i < count)
                {
                    Console.Write($"{Memory[keyArray[i]]:X0}\t");
                }
                else
                {
                    Console.Write(" \t");
                }
            }
            Console.WriteLine(pageFault ? "√" : "×");
        }

        /// <summary>
        /// 读取一行地址文本，并转换为int类型数组
        /// </summary>
        /// <returns></returns>
        private static List<int> ReadAddress()
        {
            List<int> MemoryAddresses = new List<int>();
            var str = Console.ReadLine().Split('、');
            for (int i = 0; i < str.Length; i++)
            {
                str[i] = str[i].Replace("H", "");
            }
            foreach (var item in str)
            {
                if (item.Length >= 4)
                {
                    MemoryAddresses.Add(Convert.ToInt32(item, 16));
                }
                else
                {
                    MemoryAddresses.Add(Convert.ToInt32(item + "000", 16));
                }
            }
            return MemoryAddresses;
        }

        /// <summary>
        /// 返回页号
        /// </summary>
        /// <param name="address">逻辑或者物理地址</param>
        /// <returns></returns>
        static int GetPageBlock(int address)
        {
            return address >> 12;
        }
        /// <summary>
        /// 对内存队列进行先进先出（FIFO）置换
        /// </summary>
        /// <param name="memoryBlock">要进行操作的内存地址</param>
        /// <returns>是否发生缺页中断</returns>
        static bool MakeFIFO(int memoryAddress)
        {
            MemoryBlock pageBlock = new MemoryBlock()
            {
                PageNum = GetPageBlock(memoryAddress),
                PageFrameNum = -1
            };
            //此页面是否在页框内
            if (MemoryQueue.Any(x => x.PageNum == pageBlock.PageNum))
            {
                return false;
            }
            else
            {
                //页框是否已满
                if (MemoryQueue.Count >= 4)
                {
                    //取队列第一个内存块的页框号
                    //将页框内此号码的页面数更新为此块
                    Memory[MemoryQueue.Peek().PageFrameNum] = pageBlock.PageNum;
                    //完善页块信息
                    pageBlock.PageFrameNum = MemoryQueue.Dequeue().PageFrameNum;
                }
                foreach (var item in Memory)
                {
                    if (item.Value == -1)
                    {
                        Memory[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                MemoryQueue.Enqueue(pageBlock);
                return true;
            }
        }
        /// <summary>
        /// 对内存队列进行最久未使用（LRU）置换
        /// </summary>
        /// <param name="memoryAddress"></param>
        /// <returns></returns>
        static bool MakeLRU(int memoryAddress)
        {
            MemoryBlock pageBlock = new MemoryBlock()
            {
                PageNum = GetPageBlock(memoryAddress),
                PageFrameNum = -1
            };
            if (MemoryQueue.Any(x => x.PageNum == pageBlock.PageNum))
            {
                pageBlock.PageFrameNum = RemoveValueFromMemoryQueue(pageBlock.PageNum).PageFrameNum;
                MemoryQueue.Enqueue(pageBlock);
                return false;
            }
            else
            {
                if (MemoryQueue.Count >= 4)
                {
                    Memory[MemoryQueue.Peek().PageFrameNum] = pageBlock.PageNum;
                    pageBlock.PageFrameNum = MemoryQueue.Dequeue().PageFrameNum;
                }
                foreach (var item in Memory)
                {
                    if (item.Value == -1)
                    {
                        Memory[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                MemoryQueue.Enqueue(pageBlock);
                return true;
            }
        }
        /// <summary>
        /// 对内存队列进行最佳置换（OPT）置换
        /// </summary>
        /// <param name="memoryAddress"></param>
        /// <returns></returns>
        static bool MakeOPT(List<int> inputAddress, int index)
        {
            MemoryBlock pageBlock = new MemoryBlock()
            {
                PageNum = GetPageBlock(inputAddress[index]),
                PageFrameNum = -1
            };
            //此页面是否在页框内
            if (MemoryQueue.Any(x => x.PageNum == pageBlock.PageNum))
            {
                return false;
            }
            else
            {
                //页框是否已满
                if (MemoryQueue.Count >= 4)
                {
                    //求指令序列中,当前位置后,所有在队列中出现的页号出现的次数
                    Dictionary<int, int> count = new Dictionary<int, int>();
                    foreach (var item in MemoryQueue)
                    {
                        int numCount = 0;
                        foreach (var num in inputAddress.GetRange(index, inputAddress.Count - index))
                        {
                            if (GetPageBlock(num) == item.PageNum)
                            {
                                numCount++;
                            }
                        }
                        count[item.PageNum] = numCount;
                    }
                    //按从小到大排序
                    var ResLs = count.OrderBy(x => x.Value).ToList();
                    //通过键值反查页框号
                    int memFrameKey = Memory.Where(x => x.Value == ResLs[0].Key).First().Key;
                    //将内存中此页框号内的页号替换为目标页号
                    Memory[memFrameKey] = pageBlock.PageNum;
                    //从队列中移除此值
                    pageBlock.PageFrameNum = RemoveValueFromMemoryQueue(ResLs[0].Key).PageFrameNum;
                }
                foreach (var item in Memory)
                {
                    if (item.Value == -1)
                    {
                        Memory[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                MemoryQueue.Enqueue(pageBlock);
                return true;
            }
        }
        /// <summary>
        /// 从内存队列中移除目标PageNum为参数的元素，并返回被移除的元素
        /// </summary>
        /// <param name="value">PageNum</param>
        /// <returns></returns>
        private static MemoryBlock RemoveValueFromMemoryQueue(int PageNum)
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
    }
}
