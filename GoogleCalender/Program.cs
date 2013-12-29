using System;
using System.Linq;

namespace GoogleCalender
{
    class Program
    {
        static void Main()
        {
            Console.Write("UserName: ");
            var userName = Console.ReadLine();

            var password = EnterPassword();

            var calendarService = new CalendarService();

            // authenticate
            calendarService.Authenticate(userName, password);

            // get all
            var allEvents = calendarService.GetAllEvents();
            var @event = allEvents.FirstOrDefault();

            // get by id
            var eventById = calendarService.GetEventById(@event.Id);
            eventById.Content = "UPDATE :D";

            // update
            calendarService.UpdateOrCreateEvent(eventById);

            //create
            calendarService.UpdateOrCreateEvent(new Event
            {
                StartTime = DateTime.Now.AddDays(6),
                EndTime = DateTime.Now.AddDays(7),
                Content = "Create Event",
                Title = "Create Event Title"
            });

            // delete 
            calendarService.DeleteEventById(eventById.Id);

        }

        private static string EnterPassword()
        {
            var password = "";
            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);

            return password;
        }
    }

}
