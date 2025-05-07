//using System.Collections.Generic;
//using Mono.Data.Sqlite;
//using System.Data;
//using UnityEngine;

//public class InventoryLoader : MonoBehaviour
//{
//    public static Dictionary<int, Item> LoadItemsFromBytes()
//    {
//        var items = new Dictionary<int, ItemConfig>();
//        string dbPath = $"URI=file:{Application.streamingAssetsPath}/Items.bytes";

//        using (var conn = new SqliteConnection(dbPath))
//        {
//            conn.Open();

//            using var cmd = conn.CreateCommand();
//            cmd.CommandText = "SELECT * FROM Items";

//            using IDataReader reader = cmd.ExecuteReader();
//            while (reader.Read())
//            {
//                var item = new Item
//                {
//                    ID = reader.GetInt32(0),
//                    Name = reader.GetString(1),
//                    Rarity = reader.GetString(2)
//                    // Добавьте другие поля по необходимости
//                };

//                if (!items.ContainsKey(item.ID))
//                {
//                    items.Add(item.ID, item);
//                }
//            }
//        }

//        return items;
//    }
//}