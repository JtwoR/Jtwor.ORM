using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Jtwor.ORM
{
    public abstract class DbBaseManager
    {
        protected DbConnection _connection = null;

        public DbBaseManager(DbType dbtype)
        {
            //todo
            switch (dbtype)
            {
                case DbType.MYSQL:
                    _connection = new MySqlConnection("server=127.0.0.1;port=3306;database=demo1;uid=root;pwd=123;");
                    break;
                default:
                    _connection = new MySqlConnection("server=127.0.0.1;port=3306;database=demo1;uid=root;pwd=123;");
                    break;
            }
        }

        public abstract int ExecuteNonQuery(string sql, Dictionary<string, object> _paramDic);

        public abstract List<T> ExecuteReader<T>(string sql, Dictionary<string, object> _paramDic);
    }

    public enum DbType
    {
        MYSQL,
        ORACLE,
        SQLSERVER,
        PG
    }
}
