using System;
using System.Collections.Generic;

namespace OpreatingSystemClassDesign
{
    public static class Helper
    {
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
        public static int GetPageBlock(int address)
        {
            return address >> 12;
        }
        /// <summary>
        /// 从内存队列中移除目标PageNum为参数的元素，并返回被移除的元素
        /// </summary>
        /// <param name="value">PageNum</param>
        /// <returns></returns>
        public static MemoryBlock RemoveValueFromMemoryQueue(int PageNum,ref Queue<MemoryBlock> MemoryQueue)
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
