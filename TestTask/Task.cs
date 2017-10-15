using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask
{
    public class Task
    {
		//Делегат для задачи
		private Action _task{ get; set;}

		public Task(Action task)
		{
			if (task != null)
				_task = task;
			else
				Console.WriteLine ("Task is null");
			
		}

        public void Execute()
        {
			if (_task != null)
			{
				_task ();
				Console.WriteLine ("Task. Something done...");
			} else
				Console.WriteLine ("Task failed");
        }
    }
}
