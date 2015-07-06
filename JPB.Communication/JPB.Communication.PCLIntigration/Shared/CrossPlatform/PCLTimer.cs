﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.Communication.Shared.CrossPlatform
{
    internal delegate void TimerCallback(object state);

    class PclTimer : CancellationTokenSource, IDisposable
    {
        internal PclTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
            {
                var tuple = (Tuple<TimerCallback, object>)s;

                while (true)
                {
                    if (IsCancellationRequested)
                        break;
                    Task.Run(() => tuple.Item1(tuple.Item2));
                    await Task.Delay(period);
                }

            }, Tuple.Create(callback, state), CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.Default);
        }

        public new void Dispose() { base.Cancel(); }
    }
}
