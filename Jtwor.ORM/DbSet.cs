using Jtwor.ORM.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Jtwor.ORM
{
    public class DbSet<T> : IQuery<T>
    {
        private DbHelper _db = new DbHelper();

        private string _SelectStr = string.Empty;
        private string _WhereStr = string.Empty;
        private string _GroupByStr = string.Empty;
        private string _TableNameStr = string.Empty;

        private List<LambdaExpression> _whereExpressions = new List<LambdaExpression>();
        private Expression<Func<T, object[]>> _groupExpressions = null;
        private Expression<Func<T, object>> _selectExpressions = null;

        private Dictionary<string, object> _paramDic = new Dictionary<string, object>();
        private Dictionary<string, int> _paramCheck = null;

        #region Where 条件
        public IQuery<T> Where(Expression<Func<T, bool>> expre)
        {

            _whereExpressions.Add(expre);
            GetWhereStr();
            return this;

        }

        private string GetWhereStr()
        {
            _paramCheck = new Dictionary<string, int>();

            StringBuilder sb = new StringBuilder();

            foreach (LambdaExpression expre in _whereExpressions)
            {

                string andStr = string.Empty;
                switch (expre.Body.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.OrElse:
                        andStr = sb.Length > 0 ? " AND " : string.Empty;
                        break;
                }
                sb.Append($"{andStr}{GetWhereStrEx(expre.Body)}");
            }

            _WhereStr = sb.ToString();

            return sb.ToString();
        }

        private string GetWhereStrEx(Expression expression)
        {
            StringBuilder sb = new StringBuilder();

            switch (expression.NodeType)
            {
                case ExpressionType.NotEqual:

                    break;
                case ExpressionType.Equal:
                    {
                        var body = expression as BinaryExpression;
                        string name = null;

                        //参数化
                        if (body.Left is MemberExpression)
                        {
                            name=(body.Left as MemberExpression).Member.Name;
                        }
                        if (body.Right is MemberExpression)
                        {
                            name = (body.Right as MemberExpression).Member.Name;
                        }

                        if (!_paramCheck.ContainsKey(name)) { _paramCheck.Add(name, 0); }
                        else { _paramCheck[name]++; }

                        if (body.Left is ConstantExpression)
                        {
                            _paramDic.Add($@"@{name}{_paramCheck[name]}", (body.Left as ConstantExpression).Value);
                        }
                        else if (body.Right is ConstantExpression)
                        {
                            _paramDic.Add($@"@{name}{_paramCheck[name]}", (body.Right as ConstantExpression).Value);
                        }

                        sb.Append($@"{name} = @{name}{_paramCheck[name]}");
                    }
                    break;
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        var body = expression as BinaryExpression;
                        if (body.Left.NodeType == ExpressionType.OrElse || body.Left.NodeType == ExpressionType.AndAlso)
                        {
                            sb.Append("(");
                            sb.Append(GetWhereStrEx(body.Left));
                            sb.Append(")");
                        }
                        else
                        {
                            sb.Append(GetWhereStrEx(body.Left));
                        }

                        if (expression.NodeType == ExpressionType.AndAlso) sb.Append(" AND ");
                        if (expression.NodeType == ExpressionType.OrElse) sb.Append(" OR ");

                        if (body.Right.NodeType == ExpressionType.OrElse || body.Right.NodeType == ExpressionType.AndAlso)
                        {
                            sb.Append("(");
                            sb.Append(GetWhereStrEx(body.Right));
                            sb.Append(")");
                        }
                        else
                        {
                            sb.Append(GetWhereStrEx(body.Right));
                        }
                    }
                    break;
                case ExpressionType.And:
                    {

                    }
                    break;

            }

            return sb.ToString();
        }

        private string ValueFormat(object value) {

            string result = Type.GetTypeCode(value.GetType()) switch
            {
                TypeCode.String => $"\"{value.ToString()}\"",
                _=>value.ToString()
            };

            return result;
        }
        #endregion

        #region GroupBy 分组
        public IQuery<T> GroupBy(Expression<Func<T, object[]>> expre)
        {

            _groupExpressions = expre;

            GetGroupByStr();

            return this;
        }

        private string GetGroupByStr()
        {
            List<string> names = new List<string>();

            foreach (var p in (_groupExpressions.Body as NewArrayExpression).Expressions)
            {
                names.Add((p as MemberExpression).Member.Name);
            }

            _GroupByStr = string.Join(",", names);

            return _GroupByStr;
        }
        #endregion

        #region Select 
        public IQuery<T> Select(Expression<Func<T, object>> expre)
        {

            _selectExpressions = expre;

            _SelectStr = GetSelectStr();

            return this;
        }

        private string GetSelectStr()
        {

            List<string> selCol = new List<string>();
            switch (_selectExpressions.Body.NodeType)
            {
                case ExpressionType.New:
                    {

                        NewExpression obj = _selectExpressions.Body as NewExpression;
                        for (int i = 0; i < obj.Arguments.Count; i++)
                        {
                            selCol.Add($"{(obj.Arguments[i] as MemberExpression).Member.Name} AS {obj.Members[i].Name} ");
                        }

                    }
                    break;
                case ExpressionType.MemberInit:
                    {
                        MemberInitExpression obj = (_selectExpressions.Body as MemberInitExpression);
                        for (int i = 0; i < obj.Bindings.Count; i++)
                        {
                            selCol.Add($"{((obj.Bindings[i] as MemberAssignment).Expression as MemberExpression).Member.Name} AS {obj.Bindings[i].Member.Name} ");
                        }
                    }
                    break;
            }

            return string.Join(",", selCol);
        }
        #endregion

        #region TableName
        private string GetTableNameStr()
        {
            Type type = typeof(T);
            var check = type.GetCustomAttributes(typeof(Table),true);
            if (check.Length > 0) {
                Attribute obj = (Attribute)check.FirstOrDefault();
                Table table = (Table)obj;
                if (!string.IsNullOrEmpty(table.GetName()))
                {
                    _TableNameStr = table.GetName();
                    return _TableNameStr;
                }
            }

            _TableNameStr = type.Name;
            return _TableNameStr;
        }
        #endregion

        #region 查询
        public List<T> ToList()
        {
            StringBuilder sb = new StringBuilder();

            if (_SelectStr.Length > 0)
            {
                sb.Append($@"SELECT {_SelectStr} ");
            }
            else
            {
                sb.Append($@"SELECT  * ");
            }

            GetTableNameStr();
            if (_TableNameStr.Length > 0)
            {
                sb.Append($@"From {_TableNameStr} ");
            }
            else
            {
                //todo
                throw new Exception();
            }

            if (_WhereStr.Length > 0) sb.Append($@"WHERE {_WhereStr} ");
            if (_GroupByStr.Length > 0) sb.Append($@"GROUP BY {_GroupByStr} ");

            return _db.ExecuteReader<T>(sb.ToString(), _paramDic);
        }
        #endregion


        public string CheckSql()
        {
            GetTableNameStr();

            StringBuilder sb = new StringBuilder();

            sb.Append("\n");
            sb.Append("Select ");
            sb.Append(_SelectStr);
            sb.Append("\n");
            sb.Append("From ");
            sb.Append(_TableNameStr);
            sb.Append("\n");
            sb.Append("Where ");
            sb.Append(_WhereStr);
            sb.Append("\n");
            sb.Append("Group By ");
            sb.Append(_GroupByStr);

            return sb.ToString();
        }
    }
}
