using BenchmarkDotNet.Attributes;
using LevelDB;
using LiteDB;
using Npgsql;
using PM.AutomaticManager;
using PM.AutomaticManager.Configs;
using System.Data.SQLite;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    [MemoryDiagnoser]
    [RPlotExporter]
    public class PersistentObjectsBenchmark
    {
        private LiteDatabase _db;
        private DB _levelDb;
        private static NpgsqlConnection _pgConnection;
        private SQLiteConnection _sqliteConnection;
        private PersistentFactory _persistentFactorySSD;
        private RootObject _proxy;

        private const int IntValueToWrite = int.MaxValue;
        private const long LongValueToWrite = long.MaxValue;
        private const short ShortValueToWrite = short.MaxValue;
        private const byte ByteValueToWrite = byte.MaxValue;
        private const double DoubleValueToWrite = double.MaxValue;
        private const float FloatValueToWrite = float.MinValue;
        private const decimal DecimalValueToWrite = decimal.MaxValue;
        private const char CharValueToWrite = char.MaxValue;
        private const bool BoolValueToWrite = true;
        private const string StringValueToWrite = "Test String";

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();
            CleanFolder(configFile.PersistentObjectsFilePath!);
            CleanFolder(configFile.PersistentObjectsFilePathPm!);

            SetupPmDotnet(configFile);
            SetupLiteDB(configFile);
            SetupLevelDB(configFile);
            SetupPostgreSQL(configFile);
            SetupSQLite(configFile);
        }

        #region Setups
        private static void SetupPostgreSQL(ConfigFile configFile)
        {
            _pgConnection = new NpgsqlConnection(configFile.PostgresConnectionString);
            _pgConnection.Open();
            using (var cmd = new NpgsqlCommand("DROP TABLE IF EXISTS test", _pgConnection))
            {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new NpgsqlCommand("CREATE TABLE test (id SERIAL PRIMARY KEY, int_val INTEGER, long_val BIGINT, short_val SMALLINT, byte_val SMALLINT, double_val DOUBLE PRECISION, float_val REAL, decimal_val DECIMAL, char_val CHAR(1), bool_val BOOLEAN, string_val TEXT)", _pgConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void SetupLevelDB(ConfigFile configFile)
        {
            var options = new Options { CreateIfMissing = true };
            _levelDb = new DB(options, configFile.PersistentObjectsFilePathLevelDB!);
            _levelDb.Dispose();
            Directory.Delete(configFile.PersistentObjectsFilePathLevelDB!, true);
            _levelDb = new DB(options, configFile.PersistentObjectsFilePathLevelDB!);
        }

        private void SetupLiteDB(ConfigFile configFile)
        {
            _db = new LiteDatabase($"Filename={configFile.PersistentObjectsFilenameLiteDB!};Connection=shared");
            var collection = _db.GetCollection("test");
            collection.DeleteAll();
            collection.EnsureIndex("Id");
        }

        private void SetupPmDotnet(ConfigFile configFile)
        {
            PmGlobalConfiguration.PmTarget = configFile.PmTarget;
            PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePath!;

            _persistentFactorySSD = new PersistentFactory();
            _proxy = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");
        }

        private void SetupSQLite(ConfigFile configFile)
        {
            _sqliteConnection = new SQLiteConnection(configFile.PersistentObjectsFilenameSQLite!);
            _sqliteConnection.Open();
            using (var cmd = new SQLiteCommand("DROP TABLE IF EXISTS test", _sqliteConnection))
            {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new SQLiteCommand("CREATE TABLE test (id INTEGER PRIMARY KEY, int_val INTEGER, long_val INTEGER, short_val INTEGER, byte_val INTEGER, double_val REAL, float_val REAL, decimal_val TEXT, char_val TEXT, bool_val INTEGER, string_val TEXT)", _sqliteConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region PM.NET
        [Benchmark]
        public void ProxyObjects_Write()
        {
            _proxy.IntVal = IntValueToWrite;
            _proxy.LongVal = LongValueToWrite;
            _proxy.ShortVal = ShortValueToWrite;
            _proxy.ByteVal = ByteValueToWrite;
            _proxy.DoubleVal = DoubleValueToWrite;
            _proxy.FloatVal = FloatValueToWrite;
            _proxy.DecimalVal = DecimalValueToWrite;
            _proxy.CharVal = CharValueToWrite;
            _proxy.BoolVal = BoolValueToWrite;
            _proxy.StringVal = StringValueToWrite;
        }

        [Benchmark]
        public void ProxyObjects_Read()
        {
            var intVal = _proxy.IntVal;
            var longVal = _proxy.LongVal;
            var shortVal = _proxy.ShortVal;
            var byteVal = _proxy.ByteVal;
            var doubleVal = _proxy.DoubleVal;
            var floatVal = _proxy.FloatVal;
            var decimalVal = _proxy.DecimalVal;
            var charVal = _proxy.CharVal;
            var boolVal = _proxy.BoolVal;
            var stringVal = _proxy.StringVal;

            GC.KeepAlive(intVal);
            GC.KeepAlive(longVal);
            GC.KeepAlive(shortVal);
            GC.KeepAlive(byteVal);
            GC.KeepAlive(doubleVal);
            GC.KeepAlive(floatVal);
            GC.KeepAlive(decimalVal);
            GC.KeepAlive(charVal);
            GC.KeepAlive(boolVal);
            GC.KeepAlive(stringVal);
        }
        #endregion

        #region LiteDB
        [Benchmark]
        public void UpsertData_LiteDB()
        {
            var collection = _db.GetCollection("test");
            var document = new BsonDocument
            {
                { "_id", 1 },
                { "IntVal", IntValueToWrite },
                { "LongVal", LongValueToWrite },
                { "ShortVal", ShortValueToWrite },
                { "ByteVal", (int)ByteValueToWrite },
                { "DoubleVal", DoubleValueToWrite },
                { "FloatVal", FloatValueToWrite },
                { "DecimalVal", DecimalValueToWrite },
                { "CharVal", CharValueToWrite.ToString() },
                { "BoolVal", BoolValueToWrite },
                { "StringVal", StringValueToWrite }
            };
            collection.Upsert(document);
        }

        [Benchmark]
        public void ReadData_LiteDB()
        {
            var collection = _db.GetCollection("test");
            var result = collection.FindById(1);

            GC.KeepAlive(result);
        }
        #endregion

        #region LevelDB
        [Benchmark]
        public void UpsertData_LevelDB()
        {
            var key = "1";

            var rootObject = new RootObject
            {
                IntVal = IntValueToWrite,
                LongVal = LongValueToWrite,
                ShortVal = ShortValueToWrite,
                ByteVal = ByteValueToWrite,
                DoubleVal = DoubleValueToWrite,
                FloatVal = FloatValueToWrite,
                DecimalVal = DecimalValueToWrite,
                CharVal = CharValueToWrite,
                BoolVal = BoolValueToWrite,
                StringVal = StringValueToWrite
            };

            var value = System.Text.Json.JsonSerializer.Serialize(rootObject);
            _levelDb.Put(key, value);
        }

        [Benchmark]
        public void ReadData_LevelDB()
        {
            var key = "1";
            var value = _levelDb.Get(key);

            if (value != null)
            {
                var rootObject = System.Text.Json.JsonSerializer.Deserialize<RootObject>(value);
                GC.KeepAlive(rootObject);
            }
        }
        #endregion

        #region PostgreSQL
        [Benchmark]
        public void UpsertData_PostgreSQL()
        {
            using (var cmd = new NpgsqlCommand("INSERT INTO test (id, int_val, long_val, short_val, byte_val, double_val, float_val, decimal_val, char_val, bool_val, string_val) VALUES (1, @intVal, @longVal, @shortVal, @byteVal, @doubleVal, @floatVal, @decimalVal, @charVal, @boolVal, @stringVal) ON CONFLICT (id) DO UPDATE SET int_val = EXCLUDED.int_val, long_val = EXCLUDED.long_val, short_val = EXCLUDED.short_val, byte_val = EXCLUDED.byte_val, double_val = EXCLUDED.double_val, float_val = EXCLUDED.float_val, decimal_val = EXCLUDED.decimal_val, char_val = EXCLUDED.char_val, bool_val = EXCLUDED.bool_val, string_val = EXCLUDED.string_val", _pgConnection))
            {
                cmd.Parameters.AddWithValue("intVal", IntValueToWrite);
                cmd.Parameters.AddWithValue("longVal", LongValueToWrite);
                cmd.Parameters.AddWithValue("shortVal", ShortValueToWrite);
                cmd.Parameters.AddWithValue("byteVal", ByteValueToWrite);
                cmd.Parameters.AddWithValue("doubleVal", DoubleValueToWrite);
                cmd.Parameters.AddWithValue("floatVal", FloatValueToWrite);
                cmd.Parameters.AddWithValue("decimalVal", DecimalValueToWrite);
                cmd.Parameters.AddWithValue("charVal", CharValueToWrite);
                cmd.Parameters.AddWithValue("boolVal", BoolValueToWrite);
                cmd.Parameters.AddWithValue("stringVal", StringValueToWrite);

                cmd.ExecuteNonQuery();
            }
        }

        [Benchmark]
        public void ReadData_PostgreSQL()
        {
            using (var cmd = new NpgsqlCommand("SELECT int_val, long_val, short_val, byte_val, double_val, float_val, decimal_val, char_val, bool_val, string_val FROM test WHERE id = 1", _pgConnection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var intVal = reader.GetInt32(0);
                        var longVal = reader.GetInt64(1);
                        var shortVal = reader.GetInt16(2);
                        var byteVal = reader.GetByte(3);
                        var doubleVal = reader.GetDouble(4);
                        var floatVal = reader.GetFloat(5);
                        var decimalVal = reader.GetDecimal(6);
                        var charVal = reader.GetChar(7);
                        var boolVal = reader.GetBoolean(8);
                        var stringVal = reader.GetString(9);

                        GC.KeepAlive(intVal);
                        GC.KeepAlive(longVal);
                        GC.KeepAlive(shortVal);
                        GC.KeepAlive(byteVal);
                        GC.KeepAlive(doubleVal);
                        GC.KeepAlive(floatVal);
                        GC.KeepAlive(decimalVal);
                        GC.KeepAlive(charVal);
                        GC.KeepAlive(boolVal);
                        GC.KeepAlive(stringVal);
                    }
                }
            }
        }
        #endregion

        #region SQLite
        [Benchmark]
        public void UpsertData_SQLite()
        {
            using (var cmd = new SQLiteCommand("INSERT INTO test (id, int_val, long_val, short_val, byte_val, double_val, float_val, decimal_val, char_val, bool_val, string_val) VALUES (1, @intVal, @longVal, @shortVal, @byteVal, @doubleVal, @floatVal, @decimalVal, @charVal, @boolVal, @stringVal) ON CONFLICT(id) DO UPDATE SET int_val = @intVal, long_val = @longVal, short_val = @shortVal, byte_val = @byteVal, double_val = @doubleVal, float_val = @floatVal, decimal_val = @decimalVal, char_val = @charVal, bool_val = @boolVal, string_val = @stringVal", _sqliteConnection))
            {
                cmd.Parameters.AddWithValue("@intVal", IntValueToWrite);
                cmd.Parameters.AddWithValue("@longVal", LongValueToWrite);
                cmd.Parameters.AddWithValue("@shortVal", ShortValueToWrite);
                cmd.Parameters.AddWithValue("@byteVal", ByteValueToWrite);
                cmd.Parameters.AddWithValue("@doubleVal", DoubleValueToWrite);
                cmd.Parameters.AddWithValue("@floatVal", FloatValueToWrite);
                cmd.Parameters.AddWithValue("@decimalVal", DecimalValueToWrite.ToString());
                cmd.Parameters.AddWithValue("@charVal", CharValueToWrite.ToString());
                cmd.Parameters.AddWithValue("@boolVal", BoolValueToWrite ? 1 : 0);
                cmd.Parameters.AddWithValue("@stringVal", StringValueToWrite);

                cmd.ExecuteNonQuery();
            }
        }

        [Benchmark]
        public void ReadData_SQLite()
        {
            using (var cmd = new SQLiteCommand("SELECT int_val, long_val, short_val, byte_val, double_val, float_val, decimal_val, char_val, bool_val, string_val FROM test WHERE id = 1", _sqliteConnection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var intVal = reader.GetInt32(0);
                        var longVal = reader.GetInt64(1);
                        var shortVal = reader.GetInt16(2);
                        var byteVal = reader.GetByte(3);
                        var doubleVal = reader.GetDouble(4);
                        var floatVal = reader.GetFloat(5);
                        var decimalVal = reader.GetString(6);
                        var charVal = reader.GetString(7)[0];
                        var boolVal = reader.GetInt32(8) == 1;
                        var stringVal = reader.GetString(9);

                        GC.KeepAlive(intVal);
                        GC.KeepAlive(longVal);
                        GC.KeepAlive(shortVal);
                        GC.KeepAlive(byteVal);
                        GC.KeepAlive(doubleVal);
                        GC.KeepAlive(floatVal);
                        GC.KeepAlive(decimalVal);
                        GC.KeepAlive(charVal);
                        GC.KeepAlive(boolVal);
                        GC.KeepAlive(stringVal);
                    }
                }
            }
        }
        #endregion

        private static void CleanFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                foreach (var filename in Directory.GetFiles(folder))
                {
                    File.Delete(filename);
                }
            }
        }
    }
}
