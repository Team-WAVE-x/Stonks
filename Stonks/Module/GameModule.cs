using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using static Stonks.Module.SettingModule;

namespace Stonks.Module
{
    internal class GameModule
    {
        public static void addNewUser(ulong guildid, ulong userid)
        {
            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"INSERT INTO table_{guildid} (userid, money, round) VALUES(@userid, 0, 0)";
                    sqlCom.Parameters.AddWithValue("@userid", userid);
                    sqlCom.CommandType = CommandType.Text;
                    sqlCom.ExecuteNonQuery();
                }

                sCon.Close();
            }
        }

        public static void addNewGuild(ulong guildid)
        {
            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"CREATE TABLE table_{guildid} (_id INT PRIMARY KEY AUTO_INCREMENT,userid BIGINT NOT NULL,money BIGINT NOT NULL,round INT NOT NULL) ENGINE = INNODB;";
                    sqlCom.CommandType = CommandType.Text;
                    sqlCom.ExecuteNonQuery();
                }

                sCon.Close();
            }
        }

        public static List<Class.User> getRanking(ulong guildid, int limit)
        {
            List<Class.User> users = new List<Class.User>();

            try
            {
                using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
                {
                    sCon.Open();

                    using (var sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = sCon;
                        sqlCom.CommandText = $"SELECT * FROM table_{guildid} WHERE NOT money=0 ORDER BY money DESC LIMIT @limit;";
                        sqlCom.Parameters.AddWithValue("@limit", limit);
                        sqlCom.CommandType = CommandType.Text;

                        using (MySqlDataReader reader = sqlCom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new Class.User(guildid, Convert.ToUInt64(reader["userid"])));
                            }
                        }
                    }

                    sCon.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("SQL ERROR!");
            }

            return users;
        }

        public static List<Class.User> getRoundRanking(ulong guildid, int limit)
        {
            List<Class.User> users = new List<Class.User>();

            try
            {
                using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
                {
                    sCon.Open();

                    using (var sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = sCon;
                        sqlCom.CommandText = $"SELECT * FROM table_{guildid} WHERE NOT round=0 ORDER BY round DESC LIMIT @limit;";
                        sqlCom.Parameters.AddWithValue("@limit", limit);
                        sqlCom.CommandType = CommandType.Text;

                        using (MySqlDataReader reader = sqlCom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new Class.User(guildid, Convert.ToUInt64(reader["userid"])));
                            }
                        }
                    }

                    sCon.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("SQL ERROR!");
            }

            return users;
        }

        public static string getRandomWords()
        {
            string result = string.Empty;

            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"SELECT * FROM DICTIONARY AS r1 JOIN (SELECT(RAND() * (SELECT MAX(ID) FROM DICTIONARY)) AS ID) AS r2 WHERE r1.ID >= r2.ID ORDER BY r1.ID ASC LIMIT 1;";
                    sqlCom.CommandType = CommandType.Text;

                    using (MySqlDataReader reader = sqlCom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = Convert.ToString(reader["WORD"]);
                        }
                    }
                }

                sCon.Close();

                return result;
            }
        }

        public static List<string> getStartWords(string startwith)
        {
            List<string> result = new List<string>();

            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"SELECT * FROM DICTIONARY WHERE WORD LIKE @word;";
                    sqlCom.Parameters.AddWithValue("@word", $"{startwith}%");
                    sqlCom.CommandType = CommandType.Text;

                    using (MySqlDataReader reader = sqlCom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(Convert.ToString(reader["WORD"]));
                        }
                    }
                }

                sCon.Close();

                return result;
            }
        }

        public static bool isWordExist(string word)
        {
            bool result = false;

            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"SELECT WORD FROM DICTIONARY WHERE WORD=@word LIMIT 1;";
                    sqlCom.Parameters.AddWithValue("@word", word);
                    sqlCom.CommandType = CommandType.Text;

                    using (MySqlDataReader reader = sqlCom.ExecuteReader())
                    {
                        result = reader.HasRows;
                    }
                }

                sCon.Close();

                return result;
            }
        }

        public static List<string> searchWord(string word)
        {
            List<string> words = new List<string>();

            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"SELECT * FROM DICTIONARY WHERE WORD LIKE @text ORDER BY LENGTH(WORD);";
                    sqlCom.Parameters.AddWithValue("@text", $"%{word}%");
                    sqlCom.CommandType = CommandType.Text;

                    using (MySqlDataReader reader = sqlCom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            words.Add(Convert.ToString(reader["WORD"]));
                        }
                    }
                }

                sCon.Close();

                return words;
            }
        }
    }
}