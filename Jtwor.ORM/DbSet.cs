using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Jtwor.ORM
{
    public class DbSet<T> : IQuery<T>
    {

        private string _SelectStr = string.Empty;
        private string _WhereStr = string.Empty;
        private string _GroupByStr = string.Empty;

        private List<LambdaExpression> _whereExpressions = new List<LambdaExpression>();
        private Expression<Func<T, object[]>> _groupExpressions = null;
        private Expression<Func<T, object>> _selectExpressions = null;

        #region Where 条件
        public IQuery<T> Where(Expression<Func<T, bool>> expre)
        {

            _whereExpressions.Add(expre);
            GetWhereStr();
            return this;

        }

        private string GetWhereStr()
        {

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

                        if (body.Left is MemberExpression)
                        {
                            sb.Append((body.Left as MemberExpression).Member.Name);
                        }
                        else if (body.Left is ConstantExpression)
                        {
                            sb.Append((body.Left as ConstantExpression).Value);
                        }
                        sb.Append(" = ");
                        if (body.Right is MemberExpression)
                        {
                            sb.Append((body.Right as MemberExpression).Member.Name);
                        }
                        else if (body.Right is ConstantExpression)
                        {
                            sb.Append((body.Right as ConstantExpression).Value);
                        }
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

        public string CheckSql()
        {

            StringBuilder sb = new StringBuilder();

            sb.Append("\n");
            sb.Append("Select ");
            sb.Append(_SelectStr);
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
