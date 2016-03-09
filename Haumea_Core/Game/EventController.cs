﻿using System;
using System.Collections.Generic;

using Haumea_Core.Collections;

namespace Haumea_Core
{
    public class EventController : IEntity
    {
        public static EventController Instance { get; } = new EventController();

        private IList<DateEvent> _listeners;
        private WorldDate _currentDate;

        private EventController()
        {
            _listeners = new SortedList<DateEvent>();
        }

        public void AddEvent(DateTime trigger, Action handler)
        {
            if (trigger > _currentDate.Date)
            {
                _listeners.Add(new DateEvent(trigger, handler));    
            }
            else
            {
                // RESEARCH: I'm not certain this is a good idea. 
                // Without this, events will allways be called before __all__
                // other update methods, but if this happen,
                // the event will be called before some update methods and after some others.
                // It might be better to just discard events that has already occured.
                handler();
            }
        }

        public void Update(WorldDate date)
        {
            _currentDate = date;

            while (_listeners.Count > 0 &&_listeners[0].Trigger <= _currentDate.Date)
            {
                _listeners[0].Handler();
                _listeners.RemoveAt(0);
            }
        }

        public void AddEvent(int years, int days, Action handler)
        {
            // TODO: This doesn't handle leap years?
            TimeSpan offset = new TimeSpan(365 * years + days, 0, 0, 0);
            AddEvent(_currentDate.Date.Add(offset), handler);
        }

        public void AddEvent(int days, Action handler)
        {
            AddEvent(0, days, handler);
        }

        private class DateEvent : IComparable<DateEvent>
        {
            public DateTime Trigger { get; }
            public Action Handler { get; }

            public DateEvent(DateTime trigger, Action handler)
            {
                Trigger = trigger;
                Handler = handler;
            }

            public int CompareTo(DateEvent other)
            {
                return Trigger.CompareTo(other.Trigger);
            }
        }
    }
}
