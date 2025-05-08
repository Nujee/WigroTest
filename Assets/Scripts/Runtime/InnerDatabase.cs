using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wigro.Runtime
{
    public sealed class InnerDatabase
    {
        public List<ItemData> ItemDatas { get; private set; }

        public InnerDatabase(Settings settings) => ItemDatas = LoadFromOuterDatabase(settings);

        // 9)
        public List<ItemData> LoadFromOuterDatabase(Settings settings)
        {
            string dbPath = Path.Combine(Application.streamingAssetsPath, "items.bytes");

#if UNITY_EDITOR
            dbPath = Path.Combine(Application.dataPath, "Database/items.bytes");
#endif

            string connectionString = $"URI=file:{dbPath}";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Items";

            using var reader = command.ExecuteReader();

            var itemDatas = new List<ItemData>();

            // 10)
            int count = 0;
            while (reader.Read() && count < settings.Amount)
            {
                var item = new ItemData
                {
                    Id = reader.GetInt32(0),
                    ItemId = reader.GetString(1),
                    Rarity = reader.GetInt32(2),
                    Flags = reader.GetInt32(3)
                };

                itemDatas.Add(item);
                count++;
            }

            return itemDatas;
        }
    }
}