using OutdoorPlanner.Configuration;

namespace OutdoorPlanner.Database
{
    internal class DbWorker
    {
        public static Dictionary<int, int> PrintData(EventContext db)
        {
            var idMap = new Dictionary<int, int>();
            Console.WriteLine("=== ЗАПЛАНИРОВАННЫЕ СОБЫТИЯ ===");
            if (db.Events.Any())
            {
                int counter = 1;
                foreach (var e in db.Events)
                {
                    idMap[counter] = e.Id;
                    Console.WriteLine($"{counter}. Название: {e.Name}, Место: {e.Location}, Дата: {e.DateTime}");
                    counter++;
                }
            }
            else
            {
                Console.WriteLine("Нет запланированных событий.");
            }
            Logger.Info($"Запланировано {idMap.Count} событий");
            return idMap;
        }

        public static void DeleteByDate(EventContext db)
        {
            var eventsToDelete = db.Events
                .AsEnumerable()
                .Where(e => DateTime.Parse(e.DateTime).AddHours(3) < DateTime.Now);

            if (eventsToDelete.Any())
            {
                foreach (var e in eventsToDelete)
                {
                    db.Events.Remove(e);
                    Logger.Info($"Удалено мероприятие {e.Name}");
                }
                Console.WriteLine("Все прошедшие события удалены!");
                db.SaveChanges();
            }
        }

        public static void DeleteById(EventContext db, Dictionary<int, int> idMap, int id)
        {
            var eventToDelete = db.Events.FirstOrDefault(e => e.Id == idMap[id]);

            if (eventToDelete != null)
            {
                db.Events.Remove(eventToDelete);
                db.SaveChanges();
                Logger.Info($"Событие с ID {id} успешно удалено");
                Console.WriteLine($"Событие с ID {id} успешно удалено.");
            }
            else
            {
                Logger.Info($"Событие с ID {id} не найдено");
                throw new ArgumentNullException($"Событие с ID {id} не найдено.");
            }
        }
    }
}
