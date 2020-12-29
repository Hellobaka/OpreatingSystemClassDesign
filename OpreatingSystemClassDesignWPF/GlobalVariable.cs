using System;
using System.Collections.Generic;

namespace OpreatingSystemClassDesignWPF
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

    /// <summary>
    /// 全局变量类 内容为各种控制变量
    /// </summary>
    public static class GlobalVariable
    {
        /// <summary>
        /// 表示数字数值变化的委托
        /// </summary>
        public delegate void NumberChanged(object sender, EventArgs e);
        /// <summary>
        /// 普通数值发生改变事件
        /// </summary>
        public static event NumberChanged OnNumChange;
        /// <summary>
        /// 驻留内存页块数量发生改变事件
        /// </summary>
        public static event NumberChanged OnMemoryBlockNumChange;
        /// <summary>
        /// 算法类型改变事件
        /// </summary>
        public static event NumberChanged OnArthmeticChange;
        /// <summary>
        /// 使用快表状态变更事件
        /// </summary>
        public static event NumberChanged OnTLBUsingStateChange;

        #region ---公有静态变量---
        private static int pageFaultTime = 100;
        /// <summary>
        /// 缺页中断所需要的时间
        /// </summary>
        public static int PageFaultTime
        {
            get => pageFaultTime;
            set
            {
                if (pageFaultTime != value)
                {
                    pageFaultTime = value;
                    //触发事件
                    OnNumChange("PageFaultTime", new EventArgs());
                }
            }
        }
        private static int memoryTime = 50;
        /// <summary>
        /// 读取内存一次的时间
        /// </summary>
        public static int MemoryTime
        {
            get => memoryTime;
            set
            {
                if (memoryTime != value)
                {
                    memoryTime = value;
                    OnNumChange("MemoryTime", new EventArgs());
                }
            }
        }
        private static int tlbTime = 5;
        /// <summary>
        /// 读取快表一次的时间
        /// </summary>
        public static int TLBTime
        {
            get => tlbTime;
            set
            {
                if (tlbTime != value)
                {
                    tlbTime = value;
                    OnNumChange("TLBTime", new EventArgs());
                }
            }
        }
        private static int memoryBlockNum = 4;
        /// <summary>
        /// 驻留内存的块数
        /// </summary>
        public static int MemoryBlockNum
        {
            get => memoryBlockNum;
            set
            {
                if (memoryBlockNum != value)
                {
                    memoryBlockNum = value;
                    OnMemoryBlockNumChange("MemoryBlockNum", new EventArgs());
                }
            }
        }

        /// <summary>
        /// 随机生成序列时生成的个数
        /// </summary>
        public static int GeneratorNum = 5;
        /// <summary>
        /// 随机取地址范围上限
        /// </summary>
        public static int AddressMax = 0xFFFF;

        private static bool tLBUsingState = true;

        /// <summary>
        /// 快表（TLB）使用状态
        /// </summary>
        public static bool TLBUsingState
        {
            get => tLBUsingState;
            set
            {
                if (tLBUsingState != value)
                {
                    tLBUsingState = value;
                    OnTLBUsingStateChange("TLBUsingState", new EventArgs());
                }
            }
        }
        /// <summary>
        /// 生成完整逻辑地址
        /// </summary>
        public static bool GenerateLogicAddress = false;
        private static Models.ArithmeticType arithmeticType = Models.ArithmeticType.FIFO;
        /// <summary>
        /// 当前所选择的算法类型
        /// </summary>
        public static Models.ArithmeticType ChosenArthmetic
        {
            get => arithmeticType;
            set
            {
                if (arithmeticType != value)
                {
                    arithmeticType = value;
                    OnArthmeticChange(value, new EventArgs());
                }
            }
        }
        #endregion

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
