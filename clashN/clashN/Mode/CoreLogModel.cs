namespace ClashN.Mode
{
    public class CoreLogItem
    {
        public DateTime Time { get; set; }
        //public string Time { get; set; }
        public string Level { get; set; }
        public string Payload { get; set; }

        public CoreLogItem()
        {
            Time = DateTime.Now;
            Level = string.Empty;
            Payload = string.Empty;
        }

        public CoreLogItem(DateTime time, string level, string payload)
        {
            Time = time;
            Level = level;
            Payload = payload;
        }

        public CoreLogItem Clone()
        {
            return new CoreLogItem(Time, Level, Payload);
        }
    }
}
