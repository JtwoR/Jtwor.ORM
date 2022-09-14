using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jtwor.ORM
{
    public class MysqlHelper : DbBaseManager
    {
        public MysqlHelper():base(DbType.MYSQL) { 
        
        }

        public override int ExecuteNonQuery(string sql, Dictionary<string, object> _paramDic)
        {
            throw new NotImplementedException();
        }

        public override List<T> ExecuteReader<T>(string sql, Dictionary<string, object> _paramDic)
        {
            List<T> result = new List<T>();
            using (_connection)
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, _connection as MySqlConnection)) {
                    
                        _connection.Open();
                        foreach (var key in _paramDic.Keys)
                        {
                            cmd.Parameters.AddWithValue(key,_paramDic[key]);
                        }
                        MySqlDataReader reader = cmd.ExecuteReader();
                        Type type = typeof(T);
                        //List<string> col_name = new List<string>();

                        while (reader.Read())
                        {
                            JObject obj = new JObject();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                //col_name.Add(reader.GetName(i));
                                obj.Add(new JProperty(reader.GetName(i), reader.GetValue(i)));
                            }
                            result.Add(JsonConvert.DeserializeObject<T>(obj.ToString()));

                        }
                        _connection.Close();
                    
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return result;
        }
    }
}
