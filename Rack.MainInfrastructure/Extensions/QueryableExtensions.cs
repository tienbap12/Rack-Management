using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Executes the query in batches to avoid excessive memory consumption for large result sets
        /// </summary>
        public static async Task<List<T>> ToListBatchedAsync<T>(
            this IQueryable<T> query,
            int batchSize = 1000,
            CancellationToken cancellationToken = default)
        {
            var result = new List<T>();
            int skip = 0;

            while (true)
            {
                var batch = await query
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync(cancellationToken);

                if (batch.Count == 0)
                    break;

                result.AddRange(batch);
                skip += batchSize;

                if (batch.Count < batchSize)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Performs a batched count operation to avoid timeout on large tables
        /// </summary>
        public static async Task<int> CountBatchedAsync<T>(
            this IQueryable<T> query,
            int batchSize = 10000,
            CancellationToken cancellationToken = default)
        {
            int totalCount = 0;
            int skip = 0;

            while (true)
            {
                var batch = await query
                    .Skip(skip)
                    .Take(batchSize)
                    .CountAsync(cancellationToken);

                totalCount += batch;

                if (batch < batchSize)
                    break;

                skip += batchSize;
            }

            return totalCount;
        }

        /// <summary>
        /// Optimized version of FirstOrDefault that uses Take(1).FirstOrDefault() for better query plans
        /// </summary>
        public static async Task<T?> FirstOrDefaultOptimizedAsync<T>(
            this IQueryable<T> query,
            CancellationToken cancellationToken = default)
        {
            return await query
                .Take(1)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Applies a WHERE EXISTS filter using a related entity in a more efficient way
        /// </summary>
        public static IQueryable<T> WhereExists<T, TRelated>(
            this IQueryable<T> query,
            IQueryable<TRelated> related,
            Expression<Func<T, TRelated, bool>> joinPredicate)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var subqueryParam = Expression.Parameter(typeof(TRelated), "s");

            var subqueryPredicate = ExpressionReplacer.Replace(
                joinPredicate.Body,
                joinPredicate.Parameters[0],
                parameter,
                joinPredicate.Parameters[1],
                subqueryParam);

            var subqueryLambda = Expression.Lambda<Func<TRelated, bool>>(subqueryPredicate, subqueryParam);

            var existsExpression = Expression.Call(
                typeof(Queryable),
                "Any",
                new[] { typeof(TRelated) },
                related.Expression,
                Expression.Quote(subqueryLambda));

            var lambda = Expression.Lambda<Func<T, bool>>(existsExpression, parameter);

            return query.Where(lambda);
        }

        /// <summary>
        /// Applies SQL Server-specific optimization hints
        /// </summary>
        public static IQueryable<T> WithHint<T>(this IQueryable<T> query, string hint)
        {
            return query.TagWith($"WITH ({hint})");
        }

        /// <summary>
        /// Applies NOLOCK hint for read operations (use with caution - only for reporting queries where dirty reads are acceptable)
        /// </summary>
        public static IQueryable<T> WithNoLock<T>(this IQueryable<T> query)
        {
            return query.TagWith("WITH (NOLOCK)");
        }
    }

    /// <summary>
    /// Helper class for expression manipulation
    /// </summary>
    internal class ExpressionReplacer : ExpressionVisitor
    {
        private readonly Expression _oldExpr;
        private readonly Expression _newExpr;
        private readonly ParameterExpression _oldParam;
        private readonly ParameterExpression _newParam;

        private ExpressionReplacer(
            Expression oldExpr,
            Expression newExpr,
            ParameterExpression oldParam,
            ParameterExpression newParam)
        {
            _oldExpr = oldExpr;
            _newExpr = newExpr;
            _oldParam = oldParam;
            _newParam = newParam;
        }

        public static Expression Replace(
            Expression expr,
            Expression oldExpr,
            Expression newExpr,
            ParameterExpression oldParam,
            ParameterExpression newParam)
        {
            return new ExpressionReplacer(oldExpr, newExpr, oldParam, newParam).Visit(expr);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParam ? _newParam : base.VisitParameter(node);
        }

        public override Expression Visit(Expression node)
        {
            if (node == _oldExpr)
                return _newExpr;

            return base.Visit(node);
        }
    }
}