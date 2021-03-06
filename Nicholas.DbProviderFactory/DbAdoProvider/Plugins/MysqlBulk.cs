﻿/*==============================================
*CLR版本：4.0.30319.36388
*名称：MysqlBulk
*命名空间名称：Nicholas.DbProviderFactory.DbAdoProvider.Plugins
*文件名称：MysqlBulk
*创建时间：2017/9/5 10:58:20
*作者：Nicholas Leo
*联系方式：nicholasleo1030@163.com
*==============================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Nicholas.DbProviderFactory.DbAdoProvider.Plugins
{
    internal class MysqlBulk : IDisposable
    {
        MySqlBulkLoader mysqlBulkCopy = null;
        public string ConnectionString { get; private set; }

        public int ExecuteTimeout { get; private set; }

        public MysqlBulk(string ConnectionString, int timeout = 1800)
        {
            this.ConnectionString = ConnectionString;
            this.ExecuteTimeout = timeout;
        }

        public void Dispose()
        {
            if (mysqlBulkCopy != null)
            {
                if (mysqlBulkCopy.Connection != null)
                {
                    mysqlBulkCopy.Connection.Dispose();
                }
                mysqlBulkCopy = null;
            }
        }

        public void Close()
        {
            if (mysqlBulkCopy != null
                && mysqlBulkCopy.Connection != null)
                mysqlBulkCopy.Connection.Close();
        }

        public int WriteToServer(MysqlBulkLoaderInfo bulkLoaderInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();

                mysqlBulkCopy = new MySqlBulkLoader(conn)
                {
                    Timeout = ExecuteTimeout,
                    TableName = bulkLoaderInfo.TableName,
                    FieldTerminator = bulkLoaderInfo.FieldTerminator,
                    LineTerminator = bulkLoaderInfo.LineTerminator,
                    LinePrefix = bulkLoaderInfo.LinePrefix,
                    FileName = bulkLoaderInfo.FileName,
                    FieldQuotationCharacter = bulkLoaderInfo.FieldQuotationCharacter,
                    EscapeCharacter = bulkLoaderInfo.EscapeCharacter,
                    CharacterSet = bulkLoaderInfo.CharacterSet,
                    NumberOfLinesToSkip = bulkLoaderInfo.NumberOfLinesToSkip,
                };
                if (bulkLoaderInfo.Columns != null)
                {
                    mysqlBulkCopy.Columns.AddRange(bulkLoaderInfo.Columns);
                }
                return mysqlBulkCopy.Load();
            }
        }
    }

    public class MysqlBulkLoaderInfo
    {
        public string TableName { get; set; }
        public string FieldTerminator { get; set; } //= "\t";
        public string LineTerminator { get; set; } //= "\n";
        public string LinePrefix { get; set; }
        public string FileName { get; set; }
        public string CharacterSet { get; set; } //= "utf-8";
        public char EscapeCharacter { get; set; }

        public char FieldQuotationCharacter { get; set; }

        public int NumberOfLinesToSkip { get; set; }// =1;

        public string[] Columns { get; set; }
    }
}