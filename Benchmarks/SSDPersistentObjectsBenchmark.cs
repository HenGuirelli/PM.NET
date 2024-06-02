using BenchmarkDotNet.Attributes;
using LevelDB;
using LiteDB;
using Microsoft.Data.Sqlite;
using Npgsql;
using PM.AutomaticManager;
using PM.AutomaticManager.Configs;
using System.Text;

namespace Benchmarks
{
    [MemoryDiagnoser]
    [RPlotExporter]
    public class Test
    {
        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public int Test1()
        {
            var a = 1;
            var b = 2;
            return a + b;
        }
    }

    [MemoryDiagnoser]
    [RPlotExporter]
    public class SSDPersistentObjectsBenchmark
    {
        private PersistentFactory? _persistentFactorySSD;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private RootObject _proxySSD;
        private LiteDatabase _db;
        private DB _levelDb;
        private static NpgsqlConnection _pgConnection;
        private SqliteConnection _sqliteConnection;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private const string ValueToWrite = "TextValue";

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();
            CleanFolder(configFile.PersistentObjectsFilePathSSD!);
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
            using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS test (id SERIAL PRIMARY KEY, name TEXT NOT NULL)", _pgConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void SetupLevelDB(ConfigFile configFile)
        {
            var options = new Options { CreateIfMissing = true };
            _levelDb = new DB(options, configFile.PersistentObjectsFilePathLevelDB!);
        }

        private void SetupLiteDB(ConfigFile configFile)
        {
            _db = new LiteDatabase($"Filename={configFile.PersistentObjectsFilenameLiteDB!};Connection=shared");
            var collection = _db.GetCollection("test");
            collection.EnsureIndex("Id");
        }

        private void SetupPmDotnet(ConfigFile configFile)
        {
            PmGlobalConfiguration.PmTarget = PM.Core.PmTargets.TraditionalMemoryMappedFile;
            if (configFile.PersistentObjectsFilePathSSD != null)
            {
                PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePathSSD!;
            }
            if (configFile.PersistentObjectsFilePathPm != null)
            {
                PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePathPm!;
            }

            _persistentFactorySSD = new PersistentFactory();
            _proxySSD = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");
        }

        private void SetupSQLite(ConfigFile configFile)
        {
            _sqliteConnection = new SqliteConnection($"Data Source={configFile.PersistentObjectsFilenameSQLite!}");
            _sqliteConnection.Open();
            using (var cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS test (id INTEGER PRIMARY KEY, name TEXT NOT NULL)", _sqliteConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region ProxyObjects SSD
        [Benchmark]
        public void ProxyObjects_Write_SSD()
        {
            _proxySSD.Text = ValueToWrite;
        }

        [Benchmark]
        public void ProxyObjects_Read_SSD()
        {
            var rand = _proxySSD.Text;
            GC.KeepAlive(rand);
        }
        #endregion

        #region LiteDB
        [Benchmark]
        public void UpsertData()
        {
            var collection = _db.GetCollection("test");
            collection.Upsert(
                new BsonDocument {
                    { "_id", 1 },
                    {  "Name", ValueToWrite }
                });
        }

        [Benchmark]
        public void ReadData()
        {
            var collection = _db.GetCollection("test");
            var result = collection.FindById(1).ToList();

            GC.KeepAlive(result);
        }
        #endregion

        #region LevelDB
        [Benchmark]
        public void UpsertData_LevelDB()
        {
            var key = Encoding.UTF8.GetBytes("1");
            var value = Encoding.UTF8.GetBytes(ValueToWrite);
            _levelDb.Put(key, value);
        }

        [Benchmark]
        public void ReadData_LevelDB()
        {
            var key = Encoding.UTF8.GetBytes("1");
            var value = _levelDb.Get(key);

            GC.KeepAlive(value);
        }
        #endregion

        #region PostgreSQL
        [Benchmark]
        public void UpsertData_PostgreSQL()
        {
            using (var cmd = new NpgsqlCommand("INSERT INTO test (id, name) VALUES (1, 'Test Name') ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name", _pgConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        [Benchmark]
        public void ReadData_PostgreSQL()
        {
            using (var cmd = new NpgsqlCommand("SELECT name FROM test WHERE id = 1", _pgConnection))
            {
                var result = cmd.ExecuteScalar();

                GC.KeepAlive(result);
            }
        }
        #endregion

        #region SQLite
        [Benchmark]
        public void UpsertData_SQLite()
        {
            using (var cmd = new SqliteCommand("INSERT INTO test (id, name) VALUES (1, 'Test Name') ON CONFLICT(id) DO UPDATE SET name=excluded.name", _sqliteConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        [Benchmark]
        public void ReadData_SQLite()
        {
            using (var cmd = new SqliteCommand("SELECT name FROM test WHERE id = 1", _sqliteConnection))
            {
                var result = cmd.ExecuteScalar();
                GC.KeepAlive(result);
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
