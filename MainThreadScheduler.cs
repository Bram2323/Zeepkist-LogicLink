using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogicLink
{
    public static class MainThreadScheduler
    {
        private static Queue<ActionItem> actionsQueue = [];


        public static void Update()
        {
            if (actionsQueue.Count == 0) return;

            lock (actionsQueue)
            {
                ActionItem item = actionsQueue.Dequeue();
                item.action();
                item.finished = true;
            }
        }

        public static void RunAction(Action action)
        {
            ActionItem item = new(action);
            RunItem(item);
        }

        public static T RunFunc<T>(Func<T> func)
        {
            T result = default;

            ActionItem item = new(() => result = func());
            RunItem(item);

            return result;
        }

        public static void RunItem(ActionItem item)
        {
            lock (actionsQueue) actionsQueue.Enqueue(item);
            while (!item.finished) Thread.Sleep(10);
            Thread.Sleep(100);
        }


        public class ActionItem(Action action)
        {
            public readonly Action action = action;
            public bool finished = false;
        }
    }
}
