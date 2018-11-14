using System;
using System.Runtime.Serialization;

namespace Hashtomin
{
    public sealed class objAlarm
    {
       
            [DataMember]
            public string Id { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public TimeSpan TimeOfDay { get; set; }

          //  [DataMember]
          //  public DayOfWeek[] DaysOfWeek { get; set; }

            public bool IsOneTime()
            {
                return SingleFireTime != DateTimeOffset.MinValue;
            }

            [DataMember]
            public DateTimeOffset SingleFireTime { get; set; }
        

    }
}
