using ClashN.Mode;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ClashN.Handler
{   
    /// <summary>
    /// The handler is invoked when the core process generates data to stdout. 
    ///If the data matches the pattern, the method to add the log item to the log list will be invoked.
    /// </summary>
    public class CoreLogHandler
    {

        private static string pattern = @"time=""(.+?)""\s+level=(.+?)\s+msg=""(.+?)""";
        // private static string newPattern = @"(.+?)\s+(.+?)\s+(.+)";

        private static CoreLogItem _latestLogItem = new();

        public delegate void CoreLogReceivedEventHandler(object? sender, CoreLogItem e);
        public static event CoreLogReceivedEventHandler? CoreLogReceived;

        public Action<CoreLogItem> AddCoreLog { get; set; } = (logItem) => { };

        // public CoreLogHandler(Action<LogItem> CoreLogEnqueue)
        // {
        //     this.CoreLogEnqueue = CoreLogEnqueue;
        //     Subscribe();
        // }

        public static DataReceivedEventHandler CoreDataReceived => (sender, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            Match matchPattern = Regex.Match(e.Data, pattern);
            if (matchPattern.Success)
            {
                 _latestLogItem.Time = DateTime.Parse(matchPattern.Groups[1].Value);
                //_latestLogItem.Time = matchPattern.Groups[1].Value;
                _latestLogItem.Level = matchPattern.Groups[2].Value;
                _latestLogItem.Payload = matchPattern.Groups[3].Value;
                OnCoreLogReceived(null, _latestLogItem);
                return;
            };

            // Match matchNewPattern = Regex.Match(e.Data, newPattern);
            // if (matchNewPattern.Success)
            // {
            //     _latestLogItem.Time = DateTime.Parse(matchNewPattern.Groups[1].Value);
            //     //_latestLogItem.Time = matchNewPattern.Groups[1].Value;
            //     _latestLogItem.Level = matchNewPattern.Groups[2].Value;
            //     _latestLogItem.Payload = matchNewPattern.Groups[3].Value;
            //     OnCoreLogReceived(null, _latestLogItem);
            //     return;
            // };
        };

        public static void OnCoreLogReceived(object? sender, CoreLogItem e)
        {
            CoreLogReceived?.Invoke(sender, e);
        }

        public void Subscribe()
        {
            CoreLogReceived += HandleCoreLogReceived;
        }
        public void Unsubscribe()
        {
            CoreLogReceived -= HandleCoreLogReceived;
        }

        private void HandleCoreLogReceived(object? sender, CoreLogItem e)
        {
            try
            {
                AddCoreLog(e);
            }
            catch (System.Exception)
            {
                
                throw new NotImplementedException();
            }
        }
    }
}