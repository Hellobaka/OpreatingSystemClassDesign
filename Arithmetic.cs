using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpreatingSystemClassDesign
{
    public static class Arithmetic
    {        /// <summary>
             /// 对内存队列进行先进先出（FIFO）置换
             /// </summary>
             /// <param name="memoryBlock">要进行操作的内存地址</param>
             /// <returns>是否发生缺页中断</returns>
        static bool MakeFIFO(int memoryAddress)
        {
            MemoryBlock pageBlock = new MemoryBlock()
            {
                PageNum = Helper.GetPageBlock(memoryAddress),
                PageFrameNum = -1
            };
            //此页面是否在页框内
            if (GlobalVariable.MemoryQueue.Any(x => x.PageNum == pageBlock.PageNum))
            {
                return false;
            }
            else
            {
                //页框是否已满
                if (GlobalVariable.MemoryQueue.Count >= 4)
                {
                    //取队列第一个内存块的页框号
                    //将页框内此号码的页面数更新为此块
                    GlobalVariable.Memory[GlobalVariable.MemoryQueue.Peek().PageFrameNum] = pageBlock.PageNum;
                    //完善页块信息
                    pageBlock.PageFrameNum = GlobalVariable.MemoryQueue.Dequeue().PageFrameNum;
                }
                foreach (var item in GlobalVariable.Memory)
                {
                    if (item.Value == -1)
                    {
                        GlobalVariable.Memory[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                GlobalVariable.MemoryQueue.Enqueue(pageBlock);
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
                PageNum = Helper.GetPageBlock(memoryAddress),
                PageFrameNum = -1
            };
            if (GlobalVariable.MemoryQueue.Any(x => x.PageNum == pageBlock.PageNum))
            {
                pageBlock.PageFrameNum = Helper.RemoveValueFromMemoryQueue(pageBlock.PageNum).PageFrameNum;
                GlobalVariable.MemoryQueue.Enqueue(pageBlock);
                return false;
            }
            else
            {
                if (GlobalVariable.MemoryQueue.Count >= 4)
                {
                    GlobalVariable.Memory[GlobalVariable.MemoryQueue.Peek().PageFrameNum] = pageBlock.PageNum;
                    pageBlock.PageFrameNum = GlobalVariable.MemoryQueue.Dequeue().PageFrameNum;
                }
                foreach (var item in GlobalVariable.Memory)
                {
                    if (item.Value == -1)
                    {
                        GlobalVariable.Memory[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                GlobalVariable.MemoryQueue.Enqueue(pageBlock);
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
                PageNum = Helper.GetPageBlock(inputAddress[index]),
                PageFrameNum = -1
            };
            //此页面是否在页框内
            if (GlobalVariable.MemoryQueue.Any(x => x.PageNum == pageBlock.PageNum))
            {
                return false;
            }
            else
            {
                //页框是否已满
                if (GlobalVariable.MemoryQueue.Count >= 4)
                {
                    //求指令序列中,当前位置后,所有在队列中出现的页号出现的次数
                    Dictionary<int, int> count = new Dictionary<int, int>();
                    foreach (var item in GlobalVariable.MemoryQueue)
                    {
                        int numCount = 0;
                        foreach (var num in inputAddress.GetRange(index, inputAddress.Count - index))
                        {
                            if (Helper.GetPageBlock(num) == item.PageNum)
                            {
                                numCount++;
                            }
                        }
                        count[item.PageNum] = numCount;
                    }
                    //按从小到大排序
                    var ResLs = count.OrderBy(x => x.Value).ToList();
                    //通过键值反查页框号
                    int memFrameKey = GlobalVariable.Memory.Where(x => x.Value == ResLs[0].Key).First().Key;
                    //将内存中此页框号内的页号替换为目标页号
                    GlobalVariable.Memory[memFrameKey] = pageBlock.PageNum;
                    //从队列中移除此值
                    pageBlock.PageFrameNum = Helper.RemoveValueFromMemoryQueue(ResLs[0].Key).PageFrameNum;
                }
                foreach (var item in GlobalVariable.Memory)
                {
                    if (item.Value == -1)
                    {
                        GlobalVariable.Memory[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                GlobalVariable.MemoryQueue.Enqueue(pageBlock);
                return true;
            }
        }

    }
}
