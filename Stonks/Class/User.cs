using MySql.Data.MySqlClient;
using System;
using System.Data;
using static Stonks.Module.GameModule;
using static Stonks.Module.SettingModule;

namespace Stonks.Class
{
    internal class User
    {
        public ulong Id { get; }
        public ulong GuildId { get; }
        public ulong UserId { get; }
        public ulong Money { get; }
        public int Round { get; }

        public User(ulong guildid, ulong id)
        {
            UserId = id;
            GuildId = guildid;

            try
            {
                using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
                {
                    sCon.Open();
                    using (var sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = sCon;
                        sqlCom.CommandText = $"SELECT * FROM table_{guildid} WHERE userid=@id";
                        sqlCom.Parameters.AddWithValue("@id", id);
                        sqlCom.CommandType = CommandType.Text;

                        using (MySqlDataReader reader = sqlCom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Id = Convert.ToUInt64(reader["_id"]);
                                Money = Convert.ToUInt64(reader["money"]);
                                Round = Convert.ToInt32(reader["round"]);
                            }

                            if (!reader.HasRows)
                            {
                                addNewUser(guildid, id);
                            }
                        }
                    }

                    sCon.Close();
                }
            }
            catch (Exception)
            {
                addNewGuild(guildid);
                addNewUser(guildid, id);
            }
        }

        public void addMoney(ulong money)
        {
            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();
                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"UPDATE table_{GuildId} SET money=money+@money WHERE userid=@id";
                    sqlCom.Parameters.AddWithValue("@money", money);
                    sqlCom.Parameters.AddWithValue("@id", UserId);
                    sqlCom.CommandType = CommandType.Text;
                    sqlCom.ExecuteNonQuery();
                }
                sCon.Close();
            }
        }

        public void subMoney(ulong money)
        {
            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();
                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"UPDATE table_{GuildId} SET money=money-@money WHERE userid=@id";
                    sqlCom.Parameters.AddWithValue("@money", money);
                    sqlCom.Parameters.AddWithValue("@id", UserId);
                    sqlCom.CommandType = CommandType.Text;
                    sqlCom.ExecuteNonQuery();
                }
                sCon.Close();
            }
        }

        public void setScore(int round)
        {
            using (var sCon = new MySqlConnection(GetSettingInfo().ConnectionString))
            {
                sCon.Open();
                using (var sqlCom = new MySqlCommand())
                {
                    sqlCom.Connection = sCon;
                    sqlCom.CommandText = $"UPDATE table_{GuildId} SET round=@round WHERE userid=@id";
                    sqlCom.Parameters.AddWithValue("@round", round);
                    sqlCom.Parameters.AddWithValue("@id", UserId);
                    sqlCom.CommandType = CommandType.Text;
                    sqlCom.ExecuteNonQuery();
                }
                sCon.Close();
            }
        }
    }
}