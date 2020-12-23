using System.Collections.Generic;
using System.Linq;

namespace OpreatingSystemClassDesign
{
    public static class Arithmetic
    {
        /// <summary>
        /// 对内存队列进行先进先出（FIFO）置换
        /// </summary>
        /// <param name="memoryAddress">要进行操作的内存地址</param>
        /// <param name="blockNum">被置换出的内存页号</param>
        /// <returns>是否发生缺页中断</returns>
        public static bool MakeFIFO(int memoryAddress,out int blockNum)
        {
            MemoryBlock pageBlock = new MemoryBlock()
            {
                PageNum = Helper.GetPageBlock(memoryAddress),
                PageFrameNum = -1
            };
            blockNum = 0;
            //此页面是否在页框内
            if (GlobalVariable.MemoryQueue_FIFO.Any(x => x.PageNum == pageBlock.PageNum))
            {
                return false;
            }
            else
            {
                //页框是否已满
                if (GlobalVariable.MemoryQueue_FIFO.Count >= MainForm.MemoryBlockNum)
                {
                    //取队列第一个内存块的页框号
                    //将页框内此号码的页面数更新为此块
                    GlobalVariable.Memory_FIFO[GlobalVariable.MemoryQueue_FIFO.Peek().PageFrameNum] = pageBlock.PageNum;
                    //完善页块信息
                    pageBlock.PageFrameNum = GlobalVariable.MemoryQueue_FIFO.Dequeue().PageFrameNum;
                    blockNum = pageBlock.PageFrameNum;
                }
                foreach (var item in GlobalVariable.Memory_FIFO)
                {
                    if (item.Value == -1)
                    {
                        GlobalVariable.Memory_FIFO[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                GlobalVariable.MemoryQueue_FIFO.Enqueue(pageBlock);
                return true;
            }
        }
        /// <summary>
        /// 对内存队列进行最久未使用（LRU）置换
        /// </summary>
        /// <param name="memoryAddress"></param>
        /// <returns></returns>
        public static bool MakeLRU(int memoryAddress,out int blockNum)
        {
            MemoryBlock pageBlock = new MemoryBlock()
            {
                PageNum = Helper.GetPageBlock(memoryAddress),
                PageFrameNum = -1
            };
            blockNum = 0;
            if (GlobalVariable.MemoryQueue_LRU.Any(x => x.PageNum == pageBlock.PageNum))
            {
                pageBlock.PageFrameNum = Helper.RemoveValueFromMemoryQueue(pageBlock.PageNum,ref GlobalVariable.MemoryQueue_LRU).PageFrameNum;
                GlobalVariable.MemoryQueue_LRU.Enqueue(pageBlock);
                return false;
            }
            else
            {
                if (GlobalVariable.MemoryQueue_LRU.Count >= 4)
                {
                    GlobalVariable.Memory_LRU[GlobalVariable.MemoryQueue_LRU.Peek().PageFrameNum] = pageBlock.PageNum;
                    pageBlock.PageFrameNum = GlobalVariable.MemoryQueue_LRU.Dequeue().PageFrameNum;
                    blockNum = pageBlock.PageFrameNum;
                }
                foreach (var item in GlobalVariable.Memory_LRU)
                {
                    if (item.Value == -1)
                    {
                        GlobalVariable.Memory_LRU[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                GlobalVariable.MemoryQueue_LRU.Enqueue(pageBlock);
                return true;
            }
        }
        /// <summary>
        /// 对内存队列进行最佳置换（OPT）置换
        /// </summary>
        /// <param name="memoryAddress"></param>
        /// <returns></returns>
        public static bool MakeOPT(List<int> inputAddress, int index,out int blockNum)
        {
            MemoryBlock pageBlock = new MemoryBlock()
            {
                PageNum = Helper.GetPageBlock(inputAddress[index]),
                PageFrameNum = -1
            };
            blockNum = 0;
            //此页面是否在页框内
            if (GlobalVariable.MemoryQueue_OPT.Any(x => x.PageNum == pageBlock.PageNum))
            {
                return false;
            }
            else
            {
                //页框是否已满
                if (GlobalVariable.MemoryQueue_OPT.Count >= 4)
                {
                    //求指令序列中,当前位置后,所有在队列中出现的页号出现的次数
                    Dictionary<int, int> count = new Dictionary<int, int>();
                    foreach (var item in GlobalVariable.MemoryQueue_OPT)
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
                    int memFrameKey = GlobalVariable.Memory_OPT.Where(x => x.Value == ResLs[0].Key).First().Key;
                    //将内存中此页框号内的页号替换为目标页号
                    GlobalVariable.Memory_OPT[memFrameKey] = pageBlock.PageNum;
                    //从队列中移除此值
                    pageBlock.PageFrameNum = Helper.RemoveValueFromMemoryQueue(ResLs[0].Key,ref GlobalVariable.MemoryQueue_OPT).PageFrameNum;
                    blockNum = pageBlock.PageFrameNum;
                }
                foreach (var item in GlobalVariable.Memory_OPT)
                {
                    if (item.Value == -1)
                    {
                        GlobalVariable.Memory_OPT[item.Key] = pageBlock.PageNum;
                        pageBlock.PageFrameNum = item.Key;
                        break;
                    }
                }
                GlobalVariable.MemoryQueue_OPT.Enqueue(pageBlock);
                return true;
            }
        }

    }
}
