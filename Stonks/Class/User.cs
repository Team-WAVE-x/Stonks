using System;
using System.Data;

using MySql.Data.MySqlClient;

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
                        sqlCom.CommandText = $"SELECT * FROM TABLE_{guildid} WHERE USERID=@ID";
                        sqlCom.Parameters.AddWithValue("@ID", id);
                        sqlCom.CommandType = CommandType.Text;

                        using (MySqlDataReader reader = sqlCom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Id = Convert.ToUInt64(reader["_ID"]);
                                Money = Convert.ToUInt64(reader["MONEY"]);
                                Round = Convert.ToInt32(reader["ROUND"]);
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
                    sqlCom.CommandText = $"UPDATE TABLE_{GuildId} SET MONEY=MONEY+@MONEY WHERE USERID=@ID";
                    sqlCom.Parameters.AddWithValue("@MONEY", money);
                    sqlCom.Parameters.AddWithValue("@ID", UserId);
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
                    sqlCom.CommandText = $"UPDATE TABLE_{GuildId} SET MONEY=MONEY-@MONEY WHERE USERID=@ID";
                    sqlCom.Parameters.AddWithValue("@MONEY", money);
                    sqlCom.Parameters.AddWithValue("@ID", UserId);
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
                    sqlCom.CommandText = $"UPDATE TABLE_{GuildId} SET ROUND=@ROUND WHERE USERID=@id";
                    sqlCom.Parameters.AddWithValue("@ROUND", round);
                    sqlCom.Parameters.AddWithValue("@ID", UserId);
                    sqlCom.CommandType = CommandType.Text;
                    sqlCom.ExecuteNonQuery();
                }

                sCon.Close();
            }
        }
    }
}