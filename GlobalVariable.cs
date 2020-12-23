using System.Collections.Generic;

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

    public static class GlobalVariable
    {
        /// <summary>
        /// FIFO内存处理队列
        /// </summary>
        public static Queue<MemoryBlock> MemoryQueue_FIFO = new Queue<MemoryBlock>();
        /// <summary>
        /// FIFO内存页框内容,未分配内容写-1
        /// </summary>
        public static Dictionary<int, int> Memory_FIFO = new Dictionary<int, int>();

        /// <summary>
        /// LRU内存处理队列
        /// </summary>
        public static Queue<MemoryBlock> MemoryQueue_LRU = new Queue<MemoryBlock>();
        /// <summary>
        /// LRU内存页框内容,未分配内容写-1
        /// </summary>
        public static Dictionary<int, int> Memory_LRU = new Dictionary<int, int>();
        /// <summary>
        /// OPT内存处理队列
        /// </summary>
        public static Queue<MemoryBlock> MemoryQueue_OPT = new Queue<MemoryBlock>();
        /// <summary>
        /// OPT内存页框内容,未分配内容写-1
        /// </summary>
        public static Dictionary<int, int> Memory_OPT = new Dictionary<int, int>();
    }
}
