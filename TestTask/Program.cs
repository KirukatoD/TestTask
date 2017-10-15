using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TestTask
{
    class Program
    {
         
        static FixedThreadPool.Priority prior;

        static void Main(string[] args)
        {
            FixedThreadPool fixedThreadPool = new FixedThreadPool(5);
            Task tmpTask = new Task(new Action(TMPTask));

            int i = 0;
            while(fixedThreadPool.Execute(tmpTask, RandomPriority()))
            {
                i++;
                Thread.Sleep(2000);

                if (i >= 100)
                    fixedThreadPool.Stop();

            }

            Console.ReadKey();

        }

        static void TMPTask()
        {
            Console.WriteLine("my task");
        }

        static FixedThreadPool.Priority RandomPriority()
        {
            Random rand = new Random();
            FixedThreadPool.Priority priority = FixedThreadPool.Priority.LOW;
            

            int tmpPrior = rand.Next(0, 3);
            
            switch(tmpPrior)
            {
                case 0:
                    {
                        priority = FixedThreadPool.Priority.HIGH;
                        break;
                    }
                case 1:
                    {
                        priority = FixedThreadPool.Priority.NORMAL;
                        break;
                    }
                case 2:
                    {
                        priority = FixedThreadPool.Priority.LOW;
                        break;
                    }

            }
            return priority;
        }

    }
}
