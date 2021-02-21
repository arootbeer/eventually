using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NEventStore;
using NEventStore.Persistence;
using NodaTime;
using Timer = System.Timers.Timer;

namespace Eventually.Infrastructure.EventStore
{
    public class PollingClient : IDisposable
    {
        public enum HandlingResult
        {
            MoveToNext = 0,
            Retry = 1,
            Stop = 2
        }

        private readonly ILogger _logger;

        private readonly Func<ICommit, HandlingResult> _commitCallback;

        private readonly IPersistStreams _persistStreams;

        private readonly int _waitInterval;
        private readonly Thread _pollingThread;
        private readonly Timer _pollingWakeUpTimer;

        private readonly List<Func<IEnumerable<ICommit>>> _pollers = 
            new();

        private long _checkpointToken;

        /// <summary>
        /// Creates an NEventStore Polling Client
        /// </summary>
        /// <param name="persistStreams">The store to check</param>
        /// <param name="callback">Callback to execute at each commit</param>
        /// <param name="logger"></param>
        /// <param name="waitInterval">Interval in Milliseconds to wait when the provider
        /// returns no more commit and the next request</param>
        public PollingClient(
            IPersistStreams persistStreams,
            Func<ICommit, HandlingResult> callback,
            ILogger logger,
            int waitInterval = 100
        )
        {
            _commitCallback = callback ?? throw new ArgumentNullException(nameof(callback), "Cannot use polling client without callback");
            _persistStreams = persistStreams ?? throw new ArgumentNullException(nameof(persistStreams), "PersistStreams cannot be null");

            _logger = logger;
            _waitInterval = waitInterval;
            _pollingThread = new Thread(InnerPollingLoop);
            _pollingThread.Start();
            WakeUpPoller(); // kick off synchronously
            
            _pollingWakeUpTimer = new Timer();
            _pollingWakeUpTimer.Elapsed += (sender, e) => WakeUpPoller();
            _pollingWakeUpTimer.Interval = _waitInterval;

            LastActivityTimestamp = SystemClock.Instance.GetCurrentInstant();
        }

        public CancellationToken CancellationToken { private get; set; }

        /// <summary>
        /// Tells the caller the last tick count when the last activity occurred. This is useful for the caller
        /// to setup Health check that verify if the poller is really active and it is really loading new commits.
        /// This value is obtained with DateTime.UtcNow
        /// </summary>
        public Instant LastActivityTimestamp { get; private set; }

        /// <summary>
        /// If poller encounter an exception it immediately retry, but we need to tell to the caller code
        /// that the last polling encounter an error. This is needed to detect a poller stuck as an example
        /// with deserialization problems.
        /// </summary>
        public string LastPollingError { get; private set; }

        public void Initialize(string bucketId = null, params string[] bucketIds)
        {
            foreach (var id in bucketIds.Prepend(bucketId))
            {
                AddPollingFunction(id);
            }

            InnerPoll();
            if (LastPollingError != null)
            {
                throw new Exception($"Unable to initialize PollingClient: {LastPollingError}");
            }
        }
        
        public async Task StartFrom(Func<string, long> getCheckpoint = null)
        {
            AddPollingFunction(null, getCheckpoint);
            await StartPollingThread();
        }

        public async Task StartFromBucket(string bucketId, Func<string, long> getCheckpoint = null)
        {
            AddPollingFunction(bucketId, getCheckpoint);
            await StartPollingThread();
        }

        /// <summary>
        /// Start the timer that will queue wake up tokens.
        /// </summary>
        private async Task StartPollingThread()
        {
            await Task.Run(_pollingWakeUpTimer.Start, CancellationToken);
        }

        public void AddPollingFunction(string bucketId = null, Func<string, long> getCheckpoint = null)
        {
            getCheckpoint ??= s => 0;
            if (bucketId == null)
                _pollers.Add(() => _persistStreams.GetFrom(getCheckpoint(bucketId)));
            else
                _pollers.Add(() => _persistStreams.GetFrom(bucketId, getCheckpoint(bucketId)));
        }

        public void Stop()
        {
            _stopRequested = true;
            _pollingWakeUpTimer?.Stop();

            WakeUpPoller();
        }

        public void PollNow()
        {
            WakeUpPoller();
        }

        /// <summary>
        /// Add an object to wake up the poller.
        /// </summary>
        private void WakeUpPoller()
        {
            //Avoid adding more than one wake up object.
            if (Interlocked.CompareExchange(ref _isPolling, 1, 0) == 0)
            {
                //If there's not currently a wake up object, add one.
                if (_pollCollection.Count == 0)
                {
                    _pollCollection.Add(new object(), CancellationToken);
                }

                Interlocked.Exchange(ref _isPolling, 0);
            }
        }

        private int _isPolling;
        private bool _stopRequested;

        /// <summary>
        /// This blocking collection is used to Wake up the polling thread
        /// and to ensure that only the polling thread is polling from 
        /// event stream.
        /// </summary>
        private readonly BlockingCollection<object> _pollCollection = new();

        private void InnerPollingLoop(object obj)
        {
            foreach (var request in _pollCollection.GetConsumingEnumerable())
            {
                if (_stopRequested)
                    return;

                if (InnerPoll())
                    return;
            }
        }

        /// <summary>
        /// Added to avoid flooding of logging during polling.
        /// </summary>
        private DateTime _lastPollingErrorLogTimestamp = DateTime.MinValue;

        /// <summary>
        /// This is the inner polling function that does the polling and 
        /// returns true if there were errors that should stop the poller.
        /// </summary>
        /// <returns>Returns true if we need to stop the outer cycle.</returns>
        private bool InnerPoll()
        {
            LastActivityTimestamp = SystemClock.Instance.GetCurrentInstant();
            if (!_pollers.Any() || CancellationToken.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                var commits = _pollers.SelectMany(poller => poller());

                //if we have an error in the provider, the error will be thrown during enumeration
                foreach (var commit in commits)
                {
                    //We need to reset the error, because we read correctly a commit
                    LastPollingError = null;
                    LastActivityTimestamp = SystemClock.Instance.GetCurrentInstant();
                    
                    if (_stopRequested)
                    {
                        return true;
                    }
                    
                    var result = _commitCallback(commit);

                    if (result == HandlingResult.Retry)
                    {
                        _logger.LogDebug($"Commit callback requests retry for checkpointToken {commit.CheckpointToken} - last dispatched {_checkpointToken}");
                        break;
                    }

                    if (result == HandlingResult.Stop)
                    {
                        Stop();
                        return true;
                    }
                    _checkpointToken = commit.CheckpointToken;
                }
                //if we reach here, we had no error contacting the persistence store.
                LastPollingError = null;
            }
            catch (Exception ex)
            {
                // These exceptions are expected to be transient
                LastPollingError = ex.Message;

                // These exceptions are expected to be transient, we log at maximum a log each minute.
                if (DateTime.UtcNow.Subtract(_lastPollingErrorLogTimestamp).TotalMinutes > 1)
                {
                    _logger.LogError("Error in polling client", ex);
                    _lastPollingErrorLogTimestamp = DateTime.UtcNow;
                }

                //A transient reading error is possible, but we need to wait a little bit before retrying.
                Thread.Sleep(1000);
            }

            return false;
        }

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            if (_isDisposed) return;
            if (isDisposing)
            {
                Stop();
            }
            _isDisposed = true;
        }
    }
}