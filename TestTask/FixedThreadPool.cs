using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace TestTask
{
    public class FixedThreadPool
    {
        private const int higthPriorityTaskNumber = 3;
        private int higthPriorityTaskCounter = 0;

        public enum Priority
        {
            HIGH,
            NORMAL,
            LOW
        }



        private bool IsStopped { get; set; }

        //синхронизация доступа

        //Объект синхронизации доступа к задачам (список taskQueue)
        private object threadQueueLock = new object ();
        //Объект синхронизации для остановки и запуска потока планировщика
        private object threadTaskLock = new object ();


        //Объект синхронизации доступа к IsStoped
        private object threadStoperLock = new object ();
		//барьер остановки работы пула. Срабатывает, при запросе остановки и завершения всех задач
		ManualResetEvent poolStoppedGate = new ManualResetEvent(false);

		//список задач на выполнение
		IList<TaskUnit> taskQueue = new List<TaskUnit>();

        public FixedThreadPool(int threadsNumber)
        {
            if(threadsNumber < 1)
                throw new ArgumentOutOfRangeException("threadsNumber", "Заданное количество потоков меньше единицы.");

            for (int i = 0; i < threadsNumber; i++)
            {
                var threadName = "Thread " + i;
                Thread taskThread = new Thread(ThreadAction);
                taskThread.Name = threadName;

                taskThread.Start();
            }

        }

		public bool Execute(Task task, Priority priority)
        {
			if (task != null) 
			{
				if (IsStopped) 
				{
					System.Console.WriteLine ("Остановка");
					return false;
				}
					
				//добавление задачи в очередь на выполнение
				EnqueueTask (task, priority);
				return true;
			} else
				throw new ArgumentNullException ("task", "задача не задана (NULL)" );
        }

		public void  Stop()
        {
			lock(threadStoperLock)
			{
				IsStopped = true;
			}

			//ожидание выполнения всех задач
			lock (threadTaskLock)
			{
				// Сигнализировать об изменении в условии блокировки по mTaskSchedulerLock.
				Monitor.PulseAll(threadTaskLock);
			}


			poolStoppedGate.WaitOne();

        }

        private void ThreadAction()
        {
			Console.WriteLine ("ThreadAction. Go.");
            lock (threadTaskLock)
            {
                while (true)
                {
                    Monitor.Wait(threadTaskLock);
                    lock (threadQueueLock)
                    {
                        if (!taskQueue.Any())
                        {
                            lock (threadStoperLock)
                            {
                                if (IsStopped)
                                {
                                    poolStoppedGate.Set();
                                    return;
                                }
                            }
                            Console.WriteLine("Задач нет. Ждем новых");
                            continue;
                        }
                    }


                    TaskUnit newTask = DequeueTask();

                    newTask.Task.Execute();
                    Console.WriteLine("Выполнена задача приоритета: " + newTask.Priority, ConsoleColor.Blue);

                    lock (threadQueueLock)
                    {
                        lock (threadStoperLock)
                        {
                            if (IsStopped &&
                                !taskQueue.Any())
                            {
                                Console.WriteLine("Нет задач в очереди. Выход из потока.");
                                // Сигнализировать об окончании выполнения последней задачи.
                                poolStoppedGate.Set();
                                return;
                            }
                        }
                    }
                }
            }

        }

		//добавление задачи в очередь
		private void EnqueueTask(Task task, Priority priority)
		{
			lock (threadQueueLock) 
			{
				TaskUnit taskUnit = new TaskUnit (task, priority); 
				taskQueue.Add (taskUnit);
				Console.WriteLine ("добавлена задача приоритета: " + priority );
			}
			lock (threadTaskLock) 
			{
				Monitor.Pulse (threadTaskLock);
			}
		}

		//изымает задачу из очереди на выполнение
		private TaskUnit DequeueTask()
		{
			TaskUnit tmpNextTask;
			lock (threadQueueLock) 
			{
				tmpNextTask = SearchNextPriorityTask();
				taskQueue.Remove (tmpNextTask);
			}
			lock (threadTaskLock)
			{
				Monitor.Pulse (threadTaskLock);
			}
			return tmpNextTask;
		}



		//выбор следующей задачи по правилам приоритета
		private TaskUnit SearchNextPriorityTask()
		{
			TaskUnit nextTaskU;
			lock (threadQueueLock) 
			{
				if (taskQueue.All (t => t.Priority == Priority.LOW)) {//все ли задачи приоритета LOW
					nextTaskU = taskQueue.First (t => t.Priority == Priority.LOW);
				} else {
					if (taskQueue.Any (t => t.Priority == Priority.HIGH)) {// присутствует ли задача с приоритетом HIGH
						if (higthPriorityTaskCounter < 3) {//меньше 3 задач подряд приоритета HIGH
							higthPriorityTaskCounter++;
							nextTaskU = taskQueue.First (t => t.Priority == Priority.HIGH);
						} else {
							if (taskQueue.Any (t => t.Priority == Priority.NORMAL)) { //присутствует ли задача приоритета NORMAL
								nextTaskU = taskQueue.First (t => t.Priority == Priority.NORMAL);
								higthPriorityTaskCounter = 0;
							} else {
								nextTaskU = taskQueue.First (t => t.Priority == Priority.HIGH);
							}
						}
					} else {
						nextTaskU = taskQueue.First (t => t.Priority == Priority.NORMAL);
						higthPriorityTaskCounter = 0;

					}
				}
			}
			
			return nextTaskU;
		}



        public struct TaskUnit
        {
			private Task _task;
            private Priority _priorityTask;


            public TaskUnit(Task task, Priority piority)
            {
                _task = task;
                _priorityTask = piority;
            }

            public Task Task { get { return _task; } }
            public Priority Priority { get { return _priorityTask; } }
        }
        
    }
}
