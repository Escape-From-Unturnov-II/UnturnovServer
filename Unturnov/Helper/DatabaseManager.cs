using MySql.Data.MySqlClient;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{ 
    public class DatabaseManager
    {
        public static MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(Unturnov.Conf.DatabaseConnectionString);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return connection;
        }



        public void SetItems(string tableName, ref uint storageId, Items storage)
        {
            var items = storage.items;
            var bytes = new List<byte> { (byte)items.Count, storage.height, storage.width };

            foreach (var item in items)
            {
                bytes.AddRange(BitConverter.GetBytes(item.item.id));

                bytes.Add(item.x);
                bytes.Add(item.y);
                bytes.Add(item.rot);

                bytes.Add(item.item.amount);
                bytes.Add(item.item.quality);

                bytes.Add((byte)item.item.state.Length);
                bytes.AddRange(item.item.state);
            }

            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT EXISTS(SELECT * FROM {tableName} WHERE StorageId = @storage)";
                command.Parameters.AddWithValue("@storage", storageId);

                connection.Open();
                var result = command.ExecuteScalar();

                command.Parameters.AddWithValue("@items", bytes.ToArray());
                if (result != null && storageId != 0)
                {
                    Logger.Log($"Saved {items.Count} items");
                    command.CommandText = $"UPDATE {tableName} SET " +
                                          $"Items = @items " +
                                          $"WHERE StorageId = @storage";
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = $"INSERT INTO {tableName} " +
                                          $"(StorageId, Items) VALUES " +
                                          $"(@storage, @items);" +
                                          $"SELECT LAST_INSERT_ID();";
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        storageId = uint.Parse(reader.GetValue(0).ToString());
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public List<ItemJar> GetItems(string tableName, uint storageId, out byte height, out byte width)
        {
            height = 0;
            width = 0;
            List<ItemJar> output = new List<ItemJar>();
            MySqlConnection connection;
            MySqlDataReader reader;

            try
            {
                connection = CreateConnection();
                reader = tryGetItems(connection, tableName, storageId);

                while (reader.Read())
                {

                    var bytes = (byte[])reader.GetValue(0);
                    var readBytes = 1;

                    height = bytes[readBytes++];
                    width = bytes[readBytes++];

                    for (var i = 0; i < bytes[0]; i++)
                    {
                        var id = BitConverter.ToUInt16(bytes, readBytes++);
                        readBytes++;

                        var x = bytes[readBytes++];
                        var y = bytes[readBytes++];
                        var rot = bytes[readBytes++];

                        var amount = bytes[readBytes++];
                        var quality = bytes[readBytes++];

                        var state = new byte[bytes[readBytes++]];
                        for (var k = 0; k < state.Length; k++)
                        {
                            state[k] = bytes[readBytes++];
                        }

                        output.Add(new ItemJar(x, y, rot, new Item(id, amount, quality, state)));
                    }

                }
                reader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return output;
        }

        internal void CheckSchema(string tableName)
        {
            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                command.CommandText = $"SHOW TABLES LIKE '{tableName}'";
                connection.Open();
                if (command.ExecuteScalar() == null)
                {
                    command.CommandText = $"CREATE TABLE {tableName} " +
                                          $"(StorageId INT UNSIGNED AUTO_INCREMENT PRIMARY KEY," +
                                          $"Items BLOB NULL DEFAULT NULL)";
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Database Schema Exception on Table: {tableName}");
            }
        }

        internal MySqlDataReader tryGetItems(MySqlConnection connection, string tableName, uint storageId)
        {
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Items FROM {tableName} WHERE StorageId = @storage";
                command.Parameters.AddWithValue("@storage", storageId);
                connection.Open();
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Items could not be found");
                return null;
            }
        }
    }
}
