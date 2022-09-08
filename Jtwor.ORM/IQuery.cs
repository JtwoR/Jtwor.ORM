using System;
using System.Linq.Expressions;

namespace Jtwor.ORM
{
    public interface IQuery<T>
    {
        IQuery<T> Where(Expression<Func<T, bool>> expre);
        IQuery<T> GroupBy(Expression<Func<T, object[]>> expre);
        IQuery<T> Select(Expression<Func<T, object>> expre);
    }
}
