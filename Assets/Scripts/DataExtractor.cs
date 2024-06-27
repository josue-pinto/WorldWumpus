using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System.Collections.Generic;

public class DataExtractor : MonoBehaviour
{
    private string dbFilePath;
    private string connectionString;

    void Start()
    {
        dbFilePath = Path.Combine(Application.dataPath, "PlayerActionsLog.db");
        connectionString = "URI=file:" + dbFilePath;

        CreateDatabaseAndTable(); // Adicione esta linha para garantir a criação da tabela

        List<PlayerAction> data = GetDataFromDatabase();
        string jsonData = JsonUtility.ToJson(new PlayerActionList(data));

        // Salve o JSON em um arquivo para ser usado no HTML
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "data.json"), jsonData);
    }

    void CreateDatabaseAndTable()
    {
        if (!File.Exists(dbFilePath))
        {
            SqliteConnection.CreateFile(dbFilePath);
        }

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS PlayerActions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT,
                    Perception TEXT,
                    Action TEXT,
                    Direction TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                )";
                command.ExecuteNonQuery();
            }
        }
    }

    List<PlayerAction> GetDataFromDatabase()
    {
        List<PlayerAction> dataList = new List<PlayerAction>();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT PlayerId, Perception, Action, Direction, Timestamp FROM PlayerActions";
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PlayerAction action = new PlayerAction
                        {
                            PlayerId = reader.GetString(0),
                            Perception = reader.GetString(1),
                            Action = reader.GetString(2),
                            Direction = reader.GetString(3),
                            Timestamp = reader.GetString(4)
                        };
                        dataList.Add(action);
                    }
                }
            }
        }

        return dataList;
    }
}

[System.Serializable]
public class PlayerAction
{
    public string PlayerId;
    public string Perception;
    public string Action;
    public string Direction;
    public string Timestamp;
}

[System.Serializable]
public class PlayerActionList
{
    public List<PlayerAction> data;

    public PlayerActionList(List<PlayerAction> data)
    {
        this.data = data;
    }
}
