using System;
using System.Collections.Generic;
using System.Linq;
using Google.GData.Calendar;
using Google.GData.Extensions;

namespace GoogleCalender
{
    public class CalendarService
    {
        private Google.GData.Calendar.CalendarService _calendarService;
        private Uri _calendarUri;
        private bool _isAuthenticated;

        public void Authenticate(string account, string password)
        {
            _calendarService = new Google.GData.Calendar.CalendarService("");
            _calendarService.setUserCredentials(account, password);

            _calendarUri = new Uri(string.Format("https://www.google.com/calendar/feeds/{0}/private/full", account));
            _isAuthenticated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">EventEntry.SelfUri</param>
        /// <returns></returns>
        public Event GetEventById(string id)
        {
            var eventEntry = GetEventEntryById(id);
            return MapToEvent(eventEntry);
        }

        public IEnumerable<Event> GetAllEvents()
        {
            EnsureAuthentication();

            var eventQuery = new EventQuery { Uri = _calendarUri };
            var eventFeed = _calendarService.Query(eventQuery);
            return eventFeed.Entries.Select(x => MapToEvent(x as EventEntry));
        }

        public EventEntry UpdateOrCreateEvent(Event @event)
        {
            EnsureAuthentication();

            if (string.IsNullOrEmpty(@event.Id))
            {
                var eventEntry = new EventEntry(@event.Title);
                eventEntry.Content.Content = @event.Content;
                SetEventEntryTime(@event, eventEntry);

                return _calendarService.Insert(_calendarUri, eventEntry);
            }
            else
            {
                var eventEntry = GetEventEntryById(@event.Id);
                eventEntry.Content.Content = @event.Content;
                eventEntry.Title.Text = @event.Title;
                SetEventEntryTime(@event, eventEntry);

                return _calendarService.Update(eventEntry);
            }
        }


        public void DeleteEventById(string id)
        {
            EnsureAuthentication();

            GetEventEntryById(id).Delete();
        }

        private void EnsureAuthentication()
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("Not authenticated. Please call Authenticate.");
        }

        private EventEntry GetEventEntryById(string id)
        {
            EnsureAuthentication();

            var entryUri = GetEntryUriById(id);
            var eventEntry = _calendarService.Get(entryUri) as EventEntry;
            return eventEntry;
        }

        private string GetEntryUriById(string id)
        {
            return string.Format("{0}/{1}", _calendarUri, id);
        }

        private static void SetEventEntryTime(Event @event, EventEntry eventEntry)
        {
            if (@event.StartTime.HasValue && @event.EndTime.HasValue && @event.StartTime.Value < @event.EndTime.Value)
                eventEntry.Times.Add(new When(@event.StartTime.GetValueOrDefault(), @event.EndTime.GetValueOrDefault()));
        }

        private static Event MapToEvent(EventEntry entry)
        {
            if (entry == null)
                return null;

            var time = entry.Times.FirstOrDefault();

            return new Event
            {
                Id = entry.EventId,
                Title = entry.Title.Text,
                Content = entry.Content.Content,
                StartTime = time == null ? null : (DateTime?)time.StartTime,
                EndTime = time == null ? null : (DateTime?)time.StartTime
            };
        }
    }

    public class Event
    {
        public string Id { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? StartTime { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
    }
}