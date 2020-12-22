using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpreatingSystemClassDesign
{
    public static class Models
    {
        public enum MoveMode
        {
            MinimalMode,
            None
        }
        public enum ArithmeticType
        {
            FIFO,
            LRU,
            OPT
        }
    }
}
