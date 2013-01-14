﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherQueryBuilder
    {
        readonly QueryWriter queryWriter;
        readonly StringBuilder queryTextBuilder;
        readonly IDictionary<string, object> queryParameters;

        string returnText;
        bool returnDistinct;
        CypherResultMode resultMode;
        int? limit;
        int? skip;
        string orderBy;

        public CypherQueryBuilder()
        {
            queryTextBuilder = new StringBuilder();
            queryParameters = new Dictionary<string, object>();
            queryWriter = new QueryWriter(queryTextBuilder, queryParameters);
        }

        public CypherQueryBuilder(
            QueryWriter queryWriter,
            StringBuilder queryTextBuilder,
            IDictionary<string, object> queryParameters)
        {
            this.queryWriter = queryWriter;
            this.queryTextBuilder = queryTextBuilder;
            this.queryParameters = queryParameters;
        }

        CypherQueryBuilder Clone()
        {
            var clonedQueryTextBuilder = new StringBuilder(queryTextBuilder.ToString());
            var clonedParameters = new Dictionary<string, object>(queryParameters);
            var clonedWriter = new QueryWriter(clonedQueryTextBuilder, clonedParameters);
            return new CypherQueryBuilder(
                clonedWriter,
                clonedQueryTextBuilder,
                clonedParameters
            )
            {
                returnText = returnText,
                returnDistinct = returnDistinct,
                resultMode = resultMode,
                limit = limit,
                skip = skip,
                orderBy = orderBy
            };
        }

        public CypherQueryBuilder SetReturn(string identity, bool distinct, CypherResultMode mode = CypherResultMode.Set)
        {
            var newBuilder = Clone();
            newBuilder.returnText = identity;
            newBuilder.returnDistinct = distinct;
            newBuilder.resultMode = mode;
            return newBuilder;
        }

        public CypherQueryBuilder SetReturn(LambdaExpression expression, bool distinct)
        {
            var newBuilder = Clone();
            newBuilder.returnText = CypherReturnExpressionBuilder.BuildText(expression);
            newBuilder.returnDistinct = distinct;
            newBuilder.resultMode = CypherResultMode.Projection;
            return newBuilder;
        }

        public CypherQueryBuilder SetLimit(int? count)
        {
            var newBuilder = Clone();
            newBuilder.limit = count;
            return newBuilder;
        }

        public CypherQueryBuilder SetSkip(int? count)
        {
            var newBuilder = Clone();
            newBuilder.skip = count;
            return newBuilder;
        }

        public CypherQueryBuilder SetOrderBy(OrderByType orderByType, params string[] properties)
        {
            var newBuilder = Clone();
            newBuilder.orderBy = string.Join(", ", properties);

            if (orderByType == OrderByType.Descending)
                newBuilder.orderBy += " DESC";

            return newBuilder;
        }

        public CypherQuery ToQuery()
        {
            var textBuilder = new StringBuilder(queryTextBuilder.ToString());
            var parameters = new Dictionary<string, object>(queryParameters);
            var writer = new QueryWriter(textBuilder, parameters);

            WriteReturnClause(textBuilder);
            WriteOrderByClause(textBuilder);
            WriteSkipClause(textBuilder, parameters);
            WriteLimitClause(textBuilder, parameters);

            return writer.ToCypherQuery(resultMode);
        }

        public static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
        {
            var paramName = string.Format("p{0}", parameters.Count);
            parameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }

        void WriteReturnClause(StringBuilder target)
        {
            if (returnText == null) return;
            target.Append("RETURN ");
            if (returnDistinct) target.Append("distinct ");
            target.Append(returnText);
            target.AppendLine();
        }

        void WriteLimitClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (limit == null) return;
            target.AppendFormat("LIMIT {0}", CreateParameter(paramsDictionary, limit));
            target.AppendLine();
        }

        void WriteSkipClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (skip == null) return;
            target.AppendFormat("SKIP {0}", CreateParameter(paramsDictionary, skip));
            target.AppendLine();
        }

        void WriteOrderByClause(StringBuilder target )
        {
            if (string.IsNullOrEmpty(orderBy)) return;
            target.AppendFormat("ORDER BY {0}", orderBy);
            target.AppendLine();
        }

        public CypherQueryBuilder CallWriter(Action<QueryWriter> callback)
        {
            return CallWriter((w, cp) => callback(w));
        }

        public CypherQueryBuilder CallWriter(Action<QueryWriter, Func<object, string>> callback)
        {
            var newBuilder = Clone();
            callback(
                newBuilder.queryWriter,
                v => CreateParameter(newBuilder.queryParameters, v));
            return newBuilder;
        }
    }
}
