/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 11:06

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace JPB.Communication.Shared
{
    public class SeriellTaskFactory
    {
        private Thread _thread;

        private bool _working;
        private object _syncRoot;


        public SeriellTaskFactory()
        {
            ConcurrentQueue = new ConcurrentQueue<Action>();
            _syncRoot = new object();
        }

        public ConcurrentQueue<Action> ConcurrentQueue { get; set; }

        public void Add(Action action)
        {
            ConcurrentQueue.Enqueue(action);
            StartScheduler();
        }

        private void StartScheduler()
        {
            lock (_syncRoot)
            {
                if (_working)
                    return;
                _working = true;
            }
            _thread = new Thread(Worker);
            _thread.SetApartmentState(ApartmentState.MTA);
            _thread.Start();
        }

        internal void Worker()
        {
            try
            {
                while (ConcurrentQueue.Any())
                {
                    Action action;
                    if (ConcurrentQueue.TryDequeue(out action))
                        action();
                }
            }
            finally
            {
                _working = false;
            }
        }
    }
}