﻿using FreeSql;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeSql
{
    class DbContextScopedFreeSql : IFreeSql
    {
        public IFreeSql _originalFsql;
        Func<DbContext> _resolveDbContext;
        Func<IUnitOfWork> _resolveUnitOfWork;
        DbContextScopedFreeSql() { }

        public static DbContextScopedFreeSql Create(IFreeSql fsql, Func<DbContext> resolveDbContext, Func<IUnitOfWork> resolveUnitOfWork)
        {
            if (fsql == null) return null;
            var scopedfsql = fsql as DbContextScopedFreeSql;
            if (scopedfsql == null) return new DbContextScopedFreeSql { _originalFsql = fsql, _resolveDbContext = resolveDbContext, _resolveUnitOfWork = resolveUnitOfWork };
            return Create(scopedfsql._originalFsql, resolveDbContext, resolveUnitOfWork);
        }

        public IAdo Ado => _originalFsql.Ado;
        public IAop Aop => _originalFsql.Aop;
        public ICodeFirst CodeFirst => _originalFsql.CodeFirst;
        public IDbFirst DbFirst => _originalFsql.DbFirst;
        public GlobalFilter GlobalFilter => _originalFsql.GlobalFilter;
        public void Dispose() { }

        public void Transaction(Action handler) => _originalFsql.Transaction(handler);
        public void Transaction(TimeSpan timeout, Action handler) => _originalFsql.Transaction(timeout, handler);
        public void Transaction(IsolationLevel isolationLevel, TimeSpan timeout, Action handler) => _originalFsql.Transaction(isolationLevel, timeout, handler);

        public ISelect<T1> Select<T1>() where T1 : class
        {
            _resolveDbContext()?.ExecCommand();
            return _originalFsql.Select<T1>().WithTransaction(_resolveUnitOfWork()?.GetOrBeginTransaction(false));
        }
        public ISelect<T1> Select<T1>(object dywhere) where T1 : class => Select<T1>().WhereDynamic(dywhere);

        public IDelete<T1> Delete<T1>() where T1 : class
        {
            _resolveDbContext()?.ExecCommand();
            return _originalFsql.Delete<T1>().WithTransaction(_resolveUnitOfWork()?.GetOrBeginTransaction());
        }
        public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => Delete<T1>().WhereDynamic(dywhere);

        public IUpdate<T1> Update<T1>() where T1 : class
        {
            var db = _resolveDbContext();
            db?.ExecCommand();
            var update = _originalFsql.Update<T1>().WithTransaction(_resolveUnitOfWork()?.GetOrBeginTransaction());
            if (db?.Options.NoneParameter != null) update.NoneParameter(db.Options.NoneParameter.Value);
            return update;
        }
        public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => Update<T1>().WhereDynamic(dywhere);

        public IInsert<T1> Insert<T1>() where T1 : class
        {
            var db = _resolveDbContext();
            db?.ExecCommand();
            var insert = _originalFsql.Insert<T1>().WithTransaction(_resolveUnitOfWork()?.GetOrBeginTransaction());
            if (db?.Options.NoneParameter != null) insert.NoneParameter(db.Options.NoneParameter.Value);
            return insert;
        }
        public IInsert<T1> Insert<T1>(T1 source) where T1 : class => Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => Insert<T1>().AppendData(source);
        public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => Insert<T1>().AppendData(source);

    }
}
