using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Jtwor.ORM
{
    public class DbHelper
    {
        private DbBaseManager _db = null;

        public DbHelper() {
            _db = new MysqlHelper();
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> _paramDic) {
            return _db.ExecuteNonQuery(sql,_paramDic);
        }

        public List<T> ExecuteReader<T>(string sql, Dictionary<string, object> _paramDic) {
            return _db.ExecuteReader<T>(sql, _paramDic);
        }
    }

        
}
