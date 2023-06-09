using ClashN.Handler;
using ClashN.Mode;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ClashN.ViewModels
{
    public class LogsViewModel : ReactiveObject
    {
        [Reactive]
        public int SortingSelected { get; set; }

        [Reactive]
        public bool AutoRefresh { get; set; }

        [Reactive]
        public string MsgFilter { get; set; }

        [Reactive]
        public int LineCount { get; set; }

        // [Reactive]
        public static int LogMaxCount = 100;

        private string? _logFilter;
        public string? LogFilter
        {
            get => _logFilter;
            set
            {
                if (value != _logFilter)
                {
                    _logFilter = value?.ToLower();
                }
            }
        }

        // private IObservableCollection<CoreLogItem> _coreLogs = new ObservableCollectionExtended<CoreLogItem>();
        // public IObservableCollection<CoreLogItem> CoreLogs => _coreLogs;
        private ObservableLogList<CoreLogItem> _coreLogs = new(LogMaxCount);
        public ObservableLogList<CoreLogItem> CoreLogs => _coreLogs; 

        public ObservableLogList<CoreLogItem> SelectedCoreLogs { get; } = new(LogMaxCount);

        public CoreLogHandler CoreLogHandler = new();
        public LogsViewModel()
        {
            //_coreLogs2.Insert(0, new CoreLogItem());
            CoreLogHandler.AddCoreLog = (logItem) => this.AddCoreLogs(logItem);
            CoreLogHandler.Subscribe();
            this._coreLogs.CollectionChanged += this.CoreLogsChanged;
            AutoRefresh = true;
            MsgFilter = string.Empty;
            LineCount = 1000;
        }

        public void AddCoreLogs(CoreLogItem logItem)
        {
            if (logItem == null || AutoRefresh == false)
                return;
            _coreLogs.AddFirst(logItem.Clone());
        }

        public void ClearCoreLogs()
        { _coreLogs.Clear(); }

        //sync SelectedCoreLogs with _coreLogs
        public void CoreLogsChanged(Object sender, NotifyCollectionChangedEventArgs c)
        {
            switch (c.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    CoreLogItem? addedItem = c.NewItems?[0] as CoreLogItem;
                    if(LogFilter == null || addedItem?.Payload.ToLower().Contains(LogFilter) == true)
                    SelectedCoreLogs.AddFirst(addedItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    CoreLogItem? removedItem = c.OldItems?[0] as CoreLogItem;

                    if (SelectedCoreLogs.LastItem == removedItem)
                    {
                        SelectedCoreLogs?.RemoveLast();
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    SelectedCoreLogs.Clear();
                    break;
                default:
                    break;
            }
        }

        public void FilterCoreLogs()
        {
            SelectedCoreLogs.Clear();
            foreach (CoreLogItem item in _coreLogs)
            {
                if (LogFilter == null || item.Payload.ToLower().Contains(LogFilter))
                {
                    SelectedCoreLogs.AddFirst(item);
                }
            }
        }
    }

    /// <summary>
    /// A simple linked list that can be observed for changes, to store the log items.
    /// </summary>
    public class ObservableLogList<T> : IEnumerable, INotifyCollectionChanged
    {
        private LinkedList<T> _list = new();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public int MaxSize;
        public int Count => _list.Count;

        public T? FirstItem => _list.First<T>();
        public T? LastItem => _list.Last<T>();

        public LinkedListNode<T>? Last => _list.Last;

        //construct functions
        public ObservableLogList()
        {
            MaxSize = 100;
        }

        public ObservableLogList(int maxSize)
        {
            MaxSize = maxSize > 0 ? maxSize : 100;
        }

        public void AddFirst(T? item)
        {
            if (item == null)
                return;
            if (Count >= MaxSize)
            {
                this.RemoveLast();
            }
            _list.AddFirst(item);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
            OnCollectionChanged(e);
        }

        public void RemoveLast()
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.LastItem);
            OnCollectionChanged(e);
            _list.RemoveLast();
        }

        public void Clear()
        {
            _list.Clear();
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public ObservableLogList<T> Where(Func<T, bool> predicate)
        {
            var newList = new ObservableLogList<T>();
            foreach (var item in _list)
            {
                if (predicate(item))
                {
                    newList.AddFirst(item);
                }
            }
            return newList;
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}