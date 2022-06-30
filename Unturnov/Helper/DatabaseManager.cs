using MySql.Data.MySqlClient;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
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

        public void SetInventory(string tableName, ulong steamId, StoredInventory inventory)
        {
            var clothing = new List<byte> { (byte)inventory.clothing.Count };
            var items = new List<byte> { (byte)inventory.items.Count };

            foreach (var clothingPair in inventory.clothing)
            {
                clothing.AddRange(BitConverter.GetBytes(clothingPair.Value.id));
                clothing.Add((byte)clothingPair.Key);

                clothing.Add(clothingPair.Value.amount);
                clothing.Add(clothingPair.Value.quality);

                clothing.Add((byte)clothingPair.Value.state.Length);
                clothing.AddRange(clothingPair.Value.state);
            }
            foreach (var itemWrapper in inventory.items)
            {
                items.AddRange(BitConverter.GetBytes(itemWrapper.itemJar.item.id));
                items.Add(itemWrapper.page);
                items.Add(itemWrapper.itemJar.x);
                items.Add(itemWrapper.itemJar.y);
                items.Add(itemWrapper.itemJar.rot);
                
                items.Add(itemWrapper.itemJar.item.amount);
                items.Add(itemWrapper.itemJar.item.quality);

                items.Add((byte)itemWrapper.itemJar.item.state.Length);
                items.AddRange(itemWrapper.itemJar.item.state);
            }

            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT EXISTS(SELECT * FROM {tableName} WHERE PlayerId = @playerId)";
                command.Parameters.AddWithValue("@playerId", steamId);

                connection.Open();
                var existingEntry = command.ExecuteScalar();

                command.Parameters.AddWithValue("@items", items.ToArray());
                command.Parameters.AddWithValue("@clothing", clothing.ToArray());

                // comparing the string result because everithing else seems not to work
                // if (existingEntry != null && !existingEntry.Equals(DBNull.Value))
                if (existingEntry != null && existingEntry.ToString() != "0")
                {
                    command.CommandText = $"UPDATE {tableName} SET " +
                                          $"Clothing = @clothing, " +
                                          $"Items = @items " +
                                          $"WHERE PlayerId = @playerId";
                }
                else
                {
                    command.CommandText = $"INSERT INTO {tableName} " +
                                          $"(PlayerId, Clothing, Items) VALUES " +
                                          $"(@playerId, @clothing, @items)";
                }

                var result = command.ExecuteNonQuery();

                connection.Close();

                if(result > 0)
                {
                    Logger.Log($"Saved {inventory.clothing.Count} clothing items and {inventory.items.Count} items to database: {tableName}/{steamId}");
                }
                else
                {
                    Logger.LogError($"Failed to save inventory of {steamId} to database: {tableName}/{steamId}");
                }
                
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        public StoredInventory GetInventory(string tableName, ulong steamId)
        {
            StoredInventory output = new StoredInventory();
            MySqlConnection connection;
            MySqlDataReader reader;

            try
            {
                connection = CreateConnection();
                reader = tryGetInventory(connection, tableName, steamId);

                while (reader.Read())
                {
                    var bytes = (byte[])reader.GetValue(0);
                    var readBytes = 1;

                    for (var i = 0; i < bytes[0]; i++)
                    {
                        
                        var id = BitConverter.ToUInt16(bytes, readBytes++);
                        readBytes++;
                        InventoryHelper.StorageType clothingType = (InventoryHelper.StorageType)Enum.Parse(typeof(InventoryHelper.StorageType), bytes[readBytes++].ToString());

                        var amount = bytes[readBytes++];
                        var quality = bytes[readBytes++];

                        var state = new byte[bytes[readBytes++]];
                        for (var k = 0; k < state.Length; k++)
                        {
                            state[k] = bytes[readBytes++];
                        }
                        output.clothing.Add(new KeyValuePair<InventoryHelper.StorageType, Item>(clothingType, new Item(id, amount, quality, state)));
                    }

                    bytes = (byte[])reader.GetValue(1);
                    readBytes = 1;

                    for (var i = 0; i < bytes[0]; i++)
                    {
                        var id = BitConverter.ToUInt16(bytes, readBytes++);
                        readBytes++;

                        byte page = bytes[readBytes++];
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
                        ItemJar itemJar = new ItemJar(x, y, rot, new Item(id, amount, quality, state));
                        output.items.Add(new ItemJarWrapper(itemJar, page));
                    }
                }
                Logger.Log($"loaded {output.clothing.Count} clothing items and {output.items?.Count} items from database: {tableName}/{steamId}");

                reader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return output;
        }
        public void RemoveInventory(string tableName, ulong steamId)
        {
            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM {tableName} WHERE PlayerId = @inventoryId";
                command.Parameters.AddWithValue("@inventoryId", steamId);

                connection.Open();
                var result = command.ExecuteNonQuery();
                connection.Close();

                if(result > 0)
                {
                    Logger.Log($"Removed entry {tableName}/{steamId} from database");
                }
                else
                {
                    Logger.LogWarning($"Failed to remove {tableName}/{steamId} from database");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void SetPlayerStats(string tableName, ulong steamId, PlayerStats stats)
        {
            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT EXISTS(SELECT * FROM {tableName} WHERE PlayerId = @playerId)";
                command.Parameters.AddWithValue("@playerId", steamId);

                connection.Open();
                var existingEntry = command.ExecuteScalar();

                command.Parameters.AddWithValue("@health", stats.health);
                command.Parameters.AddWithValue("@food", stats.food);
                command.Parameters.AddWithValue("@water", stats.water);
                command.Parameters.AddWithValue("@virus", stats.virus);
                command.Parameters.AddWithValue("@brokenLegs", stats.brokenLegs);
                command.Parameters.AddWithValue("@bleeding", stats.bleeding);

                // comparing the string result because everithing else seems not to work
                // if (existingEntry != null && !existingEntry.Equals(DBNull.Value))
                if (existingEntry != null && existingEntry.ToString() != "0")
                {
                    command.CommandText = $"UPDATE {tableName} SET " +
                                          $"Health = @health, " +
                                          $"Food = @food, " +
                                          $"Water = @water, " +
                                          $"Virus = @virus, " +
                                          $"BrokenLegs = @brokenLegs, " +
                                          $"Bleeding = @bleeding " +
                                          $"WHERE PlayerId = @playerId";
                }
                else
                {
                    command.CommandText = $"INSERT INTO {tableName} " +
                                          $"(PlayerId, Health, Food, Water, Virus, BrokenLegs, Bleeding) VALUES " +
                                          $"(@playerId, @health, @food, @water, @virus, @brokenLegs, @bleeding)";
                }

                var result = command.ExecuteNonQuery();

                connection.Close();

                if (result > 0)
                {
                    Logger.Log($"Saved stats to database: {tableName}/{steamId}");
                }
                else
                {
                    Logger.LogError($"Failed to save stats to database: {tableName}/{steamId}");
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        public PlayerStats GetPlayerStats(string tableName, ulong steamId)
        {
            PlayerStats output = new PlayerStats();
            MySqlConnection connection;
            MySqlDataReader reader;

            try
            {
                connection = CreateConnection();
                reader = tryGetStats(connection, tableName, steamId);

                while (reader.Read())
                {
                    output.health = reader.GetByte(1);
                    output.food = reader.GetByte(2);
                    output.water = reader.GetByte(3);
                    output.brokenLegs = reader.GetByte(4);
                    output.bleeding = reader.GetByte(5);
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

        internal void CheckItemStorageSchema(string tableName)
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
                                          $"(StorageId INT UNSIGNED AUTO_INCREMENT PRIMARY KEY, " +
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
        internal void CheckInventoryStorageSchema(string tableName)
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
                                          $"(PlayerId BIGINT UNSIGNED PRIMARY KEY, " +
                                          $"Clothing BLOB NULL DEFAULT NULL, " +
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
        internal void CheckPlayerStatsSchema(string tableName)
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
                                          $"(PlayerId BIGINT UNSIGNED PRIMARY KEY, " +
                                          $"Health TINYINT UNSIGNED, " +
                                          $"Food TINYINT UNSIGNED, " +
                                          $"Water TINYINT UNSIGNED, " +
                                          $"Virus TINYINT UNSIGNED," +
                                          $"BrokenLegs TINYINT UNSIGNED," +
                                          $"Bleeding TINYINT UNSIGNED)";
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
                Logger.LogException(ex, $"Items for {storageId} could not be found");
                return null;
            }
        }
        internal MySqlDataReader tryGetInventory(MySqlConnection connection, string tableName, ulong steamId)
        {
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Clothing, Items FROM {tableName} WHERE PlayerId = @playerId";
                command.Parameters.AddWithValue("@playerId", steamId);
                connection.Open();
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Inventory for {steamId} could not be found");
                return null;
            }
        }
        internal MySqlDataReader tryGetStats(MySqlConnection connection, string tableName, ulong steamId)
        {
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Health, Food, Water, Virus, BrokenLegs, Bleeding FROM {tableName} WHERE PlayerId = @playerId";
                command.Parameters.AddWithValue("@playerId", steamId);
                connection.Open();
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Stats for {steamId} could not be found");
                return null;
            }
        }
    }
}
