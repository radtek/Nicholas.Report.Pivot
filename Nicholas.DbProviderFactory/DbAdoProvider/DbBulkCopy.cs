﻿/*==============================================
*CLR版本：4.0.30319.36388
*名称：DbBulkCopy
*命名空间名称：Nicholas.DbProviderFactory.DbAdoProvider
*文件名称：DbBulkCopy
*创建时间：2017/9/5 10:35:48
*作者：Nicholas Leo
*联系方式：nicholasleo1030@163.com
*==============================================*/
using System;
using System.Data;
using Nicholas.DbProviderFactory.DbAdoProvider.Plugins;
using Nicholas.Untilty;
using Nicholas.Entities;

namespace Nicholas.DbProviderFactory.DbAdoProvider
{
    public class DbBulkCopy : IDbBulkCopy, IDisposable
    {
        #region public field
        public BulkCopiedArgs BulkCopiedHandler { get; set; }

        public string DestinationTableName { get; set; }

        public int BulkCopyTimeout { get; set; }

        public int BatchSize { get; set; }

        public string ConnectionString { get; private set; }

        public ProviderType ProviderName { get; private set; }

        public BulkCopyOptions DbBulkCopyOption { get; set; }

        public bool IsTransaction { get; private set; }

        public IDbTransaction dbTrans { get; private set; }

        public IDbConnection dbConn { get; private set; }
        #endregion

        SqlBulk sqlBulkCopy = null;
        Db2Bulk db2BulkCopy = null;
        OracleBulk oracleBulkCopy = null;
        MysqlBulk mySqlBulkCopy = null;

        ~DbBulkCopy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ProviderName == ProviderType.SqlServer) sqlBulkCopy.Dispose();
                else if (ProviderName == ProviderType.DB2) db2BulkCopy.Dispose();
                else if (ProviderName == ProviderType.Oracle) oracleBulkCopy.Dispose();
            }
        }

        protected DbBulkCopy(ProviderType providerType,
           string connectionString)
        {
            this.ConnectionString = connectionString;
            this.ProviderName = providerType;
        }

        public DbBulkCopy(ProviderType providerType,
            string connectionString,
            int bulkcopyTimeout = 1800,
            int batchSize = 102400,
            BulkCopyOptions dbBulkCopyOption = BulkCopyOptions.KeepIdentity)
            : this(providerType, connectionString)
        {
            this.BatchSize = batchSize;
            this.BulkCopyTimeout = bulkcopyTimeout;
            this.DbBulkCopyOption = dbBulkCopyOption;

            if (ProviderName == ProviderType.SqlServer)
            {
                if (sqlBulkCopy == null || this.ConnectionString != ConnectionString)
                {
                    sqlBulkCopy = new SqlBulk(ConnectionString, BulkCopyTimeout, DbBulkCopyOption);
                }
            }
            else if (ProviderName == ProviderType.DB2)
            {
                if (db2BulkCopy == null || this.ConnectionString != ConnectionString)
                {
                    //db2BulkCopy = new Db2Bulk(ConnectionString, BulkCopyTimeout, DbBulkCopyOption);
                    throw new Exception("暂时不支持DB2数据库");
                }
            }
            else if (ProviderName == ProviderType.Oracle)
            {
                if (oracleBulkCopy == null || this.ConnectionString != ConnectionString)
                {
                    oracleBulkCopy = new OracleBulk(ConnectionString, BulkCopyTimeout, DbBulkCopyOption);
                }
            }
            else if (ProviderName == ProviderType.MySql)
            {
                mySqlBulkCopy = new MysqlBulk(ConnectionString, BulkCopyTimeout);
            }
        }

        public DbBulkCopy(ProviderType providerType,
            string connectionString,
            IDbConnection dbConnection,
            int bulkcopyTimeout = 1800,
            int batchSize = 102400,
            BulkCopyOptions dbBulkCopyOption = BulkCopyOptions.KeepIdentity,
            bool isTransaction = true)
            : this(providerType, connectionString)
        {
            this.BatchSize = batchSize;
            this.BulkCopyTimeout = bulkcopyTimeout;
            this.DbBulkCopyOption = dbBulkCopyOption;
            this.IsTransaction = isTransaction;
            this.dbConn = dbConnection;

            if (ProviderName == ProviderType.SqlServer)
            {
                if (sqlBulkCopy != null || this.ConnectionString != connectionString)
                {
                    if (sqlBulkCopy != null)
                        sqlBulkCopy.Dispose();
                }
                if (dbConn.State != ConnectionState.Open) dbConn.Open();

                if (IsTransaction)
                {
                    dbTrans = dbConn.BeginTransaction();
                }
                sqlBulkCopy = new SqlBulk(dbConn, dbTrans, BulkCopyTimeout, DbBulkCopyOption);
            }
            else if (ProviderName == ProviderType.DB2)
            {
                //if (db2BulkCopy != null || this.ConnectionString != connectionString)
                //{
                //    if (db2BulkCopy != null)
                //        db2BulkCopy.Dispose();
                //}

                //if (dbConn.State != ConnectionState.Open) dbConn.Open();

                //if (isTransaction)
                //{
                //    dbTrans = dbConn.BeginTransaction();
                //}
                //db2BulkCopy = new Db2Bulk(dbConn, BulkCopyTimeout, DbBulkCopyOption);

                throw new Exception("暂时不支持DB2数据库");
            }
            else if (ProviderName == ProviderType.Oracle)
            {
                if (oracleBulkCopy != null || this.ConnectionString != connectionString)
                {
                    if (oracleBulkCopy != null)
                        oracleBulkCopy.Dispose();
                }

                if (dbConn.State != ConnectionState.Open) dbConn.Open();

                if (isTransaction)
                {
                    dbTrans = dbConn.BeginTransaction();
                }

                oracleBulkCopy = new OracleBulk(dbConn, BulkCopyTimeout, DbBulkCopyOption);
            }
            else if (ProviderName == ProviderType.MySql)
            {
                if (mySqlBulkCopy != null || this.ConnectionString != connectionString)
                {
                    if (mySqlBulkCopy != null)
                        mySqlBulkCopy.Dispose();
                }
                mySqlBulkCopy = new MysqlBulk(ConnectionString, BulkCopyTimeout);
            }
        }

        public void Close()
        {
            if (ProviderName == ProviderType.DB2) 
                    throw new Exception("暂时不支持DB2数据库"); //db2BulkCopy.Close();
            else if (ProviderName == ProviderType.SqlServer) sqlBulkCopy.Close();
            else if (ProviderName == ProviderType.Oracle) oracleBulkCopy.Close();
            else if (ProviderName == ProviderType.MySql) mySqlBulkCopy.Close();
        }

        public void Open()
        {
            if (!IsTransaction) return;

            if (dbConn.State != ConnectionState.Open) dbConn.Open();
        }

        public void WriteToServer(DataTable dataTable)
        {
            if (ProviderName == ProviderType.SqlServer)
            {
                sqlBulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                sqlBulkCopy.WriteToServer(dataTable, BatchSize);
            }
            else if (ProviderName == ProviderType.DB2)
            {
                //db2BulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                //db2BulkCopy.WriteToServer(dataTable, BatchSize);

                throw new Exception("暂时不支持DB2数据库");
            }
            else if (ProviderName == ProviderType.Oracle)
            {
                oracleBulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                oracleBulkCopy.WriteToServer(dataTable, BatchSize);
            }
            else if (ProviderName == ProviderType.MySql)
            {
                CsvHelper csv = new CsvHelper();
                string fname = dataTable.TableName + DateTime.Now.Ticks;
                bool rt = csv.ExportSvcToFile(dataTable, fname);
                if (rt == false) return;

                int rows = mySqlBulkCopy.WriteToServer(new MysqlBulkLoaderInfo()
                {
                    FileName = fname,
                    FieldTerminator = ",",
                    LineTerminator = "\r\n",
                    TableName = dataTable.TableName,
                    FieldQuotationCharacter = '"',
                    EscapeCharacter = '"',
                });

                if (rows == 0) throw new Exception("导入空数据...");
            }
            else
            {
                throw new Exception("暂时不支持" + ProviderName.ToString() + "的批量方法...");
            }
        }

        public void WriteToServer(DataTable dataTable, DataRowState rowState)
        {
            if (ProviderName == ProviderType.SqlServer)
            {
                sqlBulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                sqlBulkCopy.WriteToServer(dataTable, rowState, BatchSize);
            }
            else if (ProviderName == ProviderType.DB2)
            {
                //db2BulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                //db2BulkCopy.WriteToServer(dataTable, rowState);
                throw new Exception("暂时不支持DB2数据库");
            }
            else if (ProviderName == ProviderType.Oracle)
            {
                oracleBulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                oracleBulkCopy.WriteToServer(dataTable, rowState, BatchSize);
            }
            else
            {
                throw new Exception("暂时不支持" + ProviderName.ToString() + "的批量方法...");
            }
        }

        public void WriteToServer(IDataReader iDataReader, string dstTableName)
        {
            if (ProviderName == ProviderType.SqlServer)
            {
                sqlBulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                sqlBulkCopy.WriteToServer(dstTableName, iDataReader, this.BatchSize);
            }
            else if (ProviderName == ProviderType.DB2)
            {
                //db2BulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                //db2BulkCopy.WriteToServer(dstTableName, iDataReader);
                throw new Exception("暂时不支持DB2数据库");
            }
            else if (ProviderName == ProviderType.Oracle)
            {
                oracleBulkCopy.BulkCopiedHandler = BulkCopiedHandler;
                oracleBulkCopy.WriteToServer(dstTableName, iDataReader, BatchSize);
            }
            else
            {
                throw new Exception("暂时不支持" + ProviderName.ToString() + "的批量方法...");
            }
        }
    }
}
