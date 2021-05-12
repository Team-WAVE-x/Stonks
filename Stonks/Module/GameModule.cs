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
                    sqlCom.CommandText = $"INSERT INTO TABLE_{guildid} (USERID, MONEY, ROUND) VALUES(@USERID, 0, 0)";
                    sqlCom.Parameters.AddWithValue("@USERID", userid);
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
                    sqlCom.CommandText = $"CREATE TABLE TABLE_{guildid} (_ID INT PRIMARY KEY AUTO_INCREMENT, USERID BIGINT NOT NULL, MONEY BIGINT NOT NULL, ROUND INT NOT NULL) ENGINE = INNODB;";
                    sqlCom.CommandType = CommandType.Text;
                    sqlCom.ExecuteNonQuery();
                }

                sCon.Close();
            }
        }

        public static List<Class.User> getRanking(ulong guildid, int limit)
        {
            List<Class.User> users = new List<Class.User>();

            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"SELECT * FROM TABLE_{guildid} WHERE NOT MONEY=0 ORDER BY MONEY DESC LIMIT @LIMIT;";
                    sqlCom.Parameters.AddWithValue("@LIMIT", limit);
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

            return users;
        }

        public static List<Class.User> getRoundRanking(ulong guildid, int limit)
        {
            List<Class.User> users = new List<Class.User>();

            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();

                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"SELECT * FROM TABLE_{guildid} WHERE NOT ROUND=0 ORDER BY ROUND DESC LIMIT @LIMIT;";
                    sqlCom.Parameters.AddWithValue("@LIMIT", limit);
                    sqlCom.CommandType = CommandType.Text;

                    using (MySqlDataReader reader = sqlCom.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new Class.User(guildid, Convert.ToUInt64(reader["USERID"])));
                        }
                    }
                }

                sCon.Close();
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
                    sqlCom.CommandText = $"SELECT * FROM DICTIONARY AS R1 JOIN (SELECT(RAND() * (SELECT MAX(ID) FROM DICTIONARY)) AS ID) AS R2 WHERE R1.ID >= R2.ID ORDER BY R1.ID ASC LIMIT 1;";
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
                    sqlCom.CommandText = $"SELECT * FROM DICTIONARY WHERE WORD LIKE @WORD;";
                    sqlCom.Parameters.AddWithValue("@WORD", $"{startwith}%");
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
                    sqlCom.CommandText = $"SELECT WORD FROM DICTIONARY WHERE WORD=@WORD LIMIT 1;";
                    sqlCom.Parameters.AddWithValue("@WORD", word);
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
                    sqlCom.CommandText = $"SELECT * FROM DICTIONARY WHERE WORD LIKE @TEXT ORDER BY LENGTH(WORD);";
                    sqlCom.Parameters.AddWithValue("@TEXT", $"%{word}%");
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