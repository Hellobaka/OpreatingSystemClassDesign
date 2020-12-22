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
        /// 内存处理队列
        /// </summary>
        public static Queue<MemoryBlock> MemoryQueue = new Queue<MemoryBlock>();
        /// <summary>
        /// 内存页框内容,未分配内容写-1
        /// </summary>
        public static Dictionary<int, int> Memory = new Dictionary<int, int>
        {
            {1,-1 },
            {3,-1 },
            {4,-1 },
            {7,-1 }
        };
    }
}
