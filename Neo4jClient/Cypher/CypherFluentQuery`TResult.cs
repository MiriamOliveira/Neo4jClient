﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.DebugQueryText}")]
    public class CypherFluentQuery<TResult> :
        CypherFluentQuery,
        ICypherFluentQuery<TResult>
    {
        public CypherFluentQuery(IGraphClient client, CypherQueryBuilder builder)
            : base(client, builder)
        {}

        public ICypherFluentQuery<TResult> Limit(int? limit)
        {
            var newBuilder = Builder.SetLimit(limit);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQuery<TResult> Skip(int? skip)
        {
            var newBuilder = Builder.SetSkip(skip);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQuery<TResult> OrderBy(params string[] properties)
        {
            var newBuilder = Builder.SetOrderBy(OrderByType.Ascending, properties);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public ICypherFluentQuery<TResult> OrderByDescending(params string[] properties)
        {
            var newBuilder = Builder.SetOrderBy(OrderByType.Descending, properties);
            return new CypherFluentQuery<TResult>(Client, newBuilder);
        }

        public IEnumerable<TResult> Results
        {
            get
            {
                return Client.ExecuteGetCypherResults<TResult>(Query);
            }
        }

        public Task<IEnumerable<TResult>> ResultsAsync
        {
            get
            {
                return Client.ExecuteGetCypherResultsAsync<TResult>(Query);
            }
        }
    }
}
