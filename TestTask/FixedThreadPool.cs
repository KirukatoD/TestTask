using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask
{
    public class FixedThreadPool
    {
        

        enum Prioryty
        {
            HIGH,
            NORMAL,
            LOW
        }

        public FixedThreadPool(int threads)
        {

        }

        bool Execute(Task task, Prioryty priority)
        {

            return false;
        }

        void  Stop()
        {

        }
    }
}
