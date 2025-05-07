//// DatabaseManager.cs
//using System.Collections.Generic;
//using Mono.Data.Sqlite;
//using UnityEngine;

//public static class ItemsLoader
//{
//    public static Dictionary<int, Item> LoadItemsFromDatabase()
//    {
//        var items = new Dictionary<string, Item>();

//        // Путь к файлу БД
//        string dbPath = $"URI=file:{Application.streamingAssetsPath}/items.bytes";

//        using (var connection = new SqliteConnection(dbPath))
//        {
//            connection.Open();

//            // Чтение данных из таблицы Items
//            using (var command = connection.CreateCommand())
//            {
//                command.CommandText = $"SELECT * FROM {DatabaseEditor.TableName}";

//                using (var reader = command.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        var item = new Item
//                        {
//                            ItemID = reader["ItemID"].ToString(),
//                            Rarity = int.Parse(reader["Rarity"].ToString()),
//                            Flags = int.Parse(reader["Flags"].ToString())
//                        };

//                        items.Add(item.ItemID, item);
//                    }
//                }
//            }
//        }

//        return items;
//    }
//}