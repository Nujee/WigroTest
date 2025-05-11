using Mono.Data.Sqlite;
using UnityEngine;
using UnityEditor;

namespace Wigro.Editor
{
    internal sealed class DatabaseEditor
    {
        public static readonly string TableName = "Items";

        private static class Integration
        {
            [MenuItem( "Wigro/Database/Create" )]
            private static async void CreateDatabase()
            {
                Runtime.Settings settings;

                string[] guids = AssetDatabase.FindAssets( $"t:{typeof( Runtime.Settings ).Name}" );
                if ( guids.Length > 0 )
                {
                    var settingsPath = AssetDatabase.GUIDToAssetPath( guids[ 0 ] );

                    settings = AssetDatabase.LoadAssetAtPath<Runtime.Settings>( settingsPath );
                }
                else
                {
                    // 4)
                    settings = ScriptableObject.CreateInstance<Runtime.Settings>();
                    string filePath = System.IO.Path.Combine("Assets", "Resources", "Settings.Asset");

                    AssetDatabase.CreateAsset( settings, filePath );
                    AssetDatabase.SaveAssets();

                    Debug.Log("Settings config is not found. New settings config was created at " + filePath);

                    EditorGUIUtility.PingObject( settings );

                    return;
                }

                string sourceFolder = AssetDatabase.GetAssetPath( settings.Folder );
                if ( string.IsNullOrEmpty( sourceFolder ) )
                    return;

                var assetPath = System.IO.Path.Combine( sourceFolder, "items.bytes" );

                SqliteConnection _sqliteConnection = new( $"URI=file:{assetPath}" );
                await _sqliteConnection.OpenAsync();

                SqliteCommand command = _sqliteConnection.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"CREATE TABLE IF NOT EXISTS {TableName} (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "ItemID TEXT UNIQUE NOT NULL CHECK (ItemID != ''), " +
                    "Rarity INTEGER NOT NULL DEFAULT 0, " +
                    "Flags INTEGER NOT NULL DEFAULT 0)";

                await command.ExecuteNonQueryAsync();

                int randomCount = Random.Range( 5, 55 );
                for ( int i = 0; i < randomCount; ++i )
                {
                    int rarity = Random.Range( 0, 4 );

                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = $"INSERT INTO {TableName} " +
                        $"(ItemID, Rarity, Flags) " +
                        $"VALUES ('Item_{i}', {rarity}, 0)";

                    await command.ExecuteNonQueryAsync();
                }

                AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );
            }
        }
    }

}

