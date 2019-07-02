using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Persistence.DapperSupport
{
    public class DapperContext : IDapperContext
    {
        private string connectionString;
        private IDbTransaction transaction;
        private IDbConnection connection;

        public DapperContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection Connection
        {
            get
            {
                if (this.connection == null)
                    this.connection = new SqlConnection(this.connectionString);
                if (String.IsNullOrEmpty(this.connection.ConnectionString))
                    this.connection.ConnectionString = this.connectionString;
                if (this.connection.State == ConnectionState.Closed)
                    this.connection.Open();
                return this.connection;
            }
        }

        public IDbTransaction BeginTransaction()
        {
            if (transaction == null || transaction.Connection == null)
            {
                transaction = this.Connection.BeginTransaction();
            }
            return transaction;
        }

        public void Commit()
        {
            transaction.Commit();
            transaction.Dispose();
            transaction = null;
        }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Dispose();
                transaction = null;
            }
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        public bool InTransaction()
        {
            return transaction != null && transaction.Connection != null;
        }

        public void Rollback()
        {
            transaction.Rollback();
            transaction.Dispose();
            transaction = null;
        }

        public T Transaction<T>(Func<IDbTransaction, T> query)
        {
            if (this.InTransaction())
            {
                return query(transaction);
            }            
            else
            {
                using (var _connection = Connection)
                {
                    using (var transaction = this.BeginTransaction())
                    {
                        try
                        {
                            var result = query(transaction);
                            transaction.Commit();
                            return result;
                        } catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }

        }
    }
}
