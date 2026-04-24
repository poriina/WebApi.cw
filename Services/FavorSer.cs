using Newtonsoft.Json;
using WebApi.Models;

namespace WebApi.Services
{
    public class FavorSer
    {
       
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "fav.json");

        public List<favorModel> GetAllItems()
        {
            if (!File.Exists(_filePath))
            {
                return new List<favorModel>(); 
            }

            var json = File.ReadAllText(_filePath); 
            return JsonConvert.DeserializeObject<List<favorModel>>(json) ?? new List<favorModel>(); //перетворення json на список об'єктів
        }

        public favorModel GetItemById(int id)
        {
            var items = GetAllItems(); //для всіх записів
            return items.FirstOrDefault(i => i.Id == id); //для конкретного запису
        }

        public favorModel AddItem(favorModel newItem)
        {
            var items = GetAllItems();


            newItem.Id = items.Any() ? items.Max(i => i.Id) + 1 : 1; //якщо список не порожній, знаходимо максимальний id і додаємо 1. інакше id = 1

            items.Add(newItem); 
            SaveAll(items); 

            return newItem; 
        }

        public bool UpdateItem(int id, favorModel updatedItem)
        {
            var items = GetAllItems();
            var index = items.FindIndex(i => i.Id == id); //шукаємо позицію елемента у списку

            if (index == -1)
            {
                return false; 
            }

            updatedItem.Id = id; 
            items[index] = updatedItem; 
            SaveAll(items);

            return true;
        }

        public bool DeleteItem(int id)
        {
            var items = GetAllItems();
            var item = items.FirstOrDefault(i => i.Id == id); 

            if (item == null)
            {
                return false; 
            }

            items.Remove(item);
            SaveAll(items);

            return true;
        }

        
        private void SaveAll(List<favorModel> items) //метод для збереження списку у файл (private, бо використовується тільки всередині цього класу)
        {
            var json = JsonConvert.SerializeObject(items, Formatting.Indented); //перетворюємо список назад у json
            File.WriteAllText(_filePath, json); //перезаписуємо файл
        }
    }
}