using System.Linq.Expressions;

namespace HCA.Data.Extensions
{
    public static class ExpressionExtension
    {
        public static Expression<Func<T, bool>> And<T>(
         this Expression<Func<T, bool>> expr1,
         Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            // Combine both expressions with an AND
            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> expr1,
            Func<Expression<Func<T, bool>>> expr2)
        {
            return expr1.And(expr2());
        }
    }
}
