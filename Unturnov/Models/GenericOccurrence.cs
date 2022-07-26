using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    internal class GenericOccurrence<T>
    {
        internal float lastTime;
        internal T occurredEvent;
        internal GenericOccurrence(float lastTime, T occurredEvent)
        {
            this.lastTime = lastTime;
            this.occurredEvent = occurredEvent;
        }
    }
}
