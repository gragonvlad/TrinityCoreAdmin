﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace TrinityCoreAdmin
{
    public class MySQLConnection : IDisposable
    {
        protected List<MySqlCommand> m_stmts = new List<MySqlCommand>();
        protected static List<MySQLConnection> m_conn = new List<MySQLConnection>();
        public MySql.Data.MySqlClient.MySqlConnection conn;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.conn.Close();
                this.conn = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected MySQLConnection(MySqlConnectionStringBuilder connBuilder)
        {
            conn = new MySql.Data.MySqlClient.MySqlConnection(connBuilder.ConnectionString);
        }

        protected MySQLConnection(string connString)
        {
            conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:SQL-Abfragen auf Sicherheitsrisiken überprüfen")]
        public DataTable Execute(string sql)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return new DataTable();

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        public DataTable Execute(MySqlCommand stmt)
        {
            if (conn.State == ConnectionState.Open)
            {
                using (MySqlDataReader reader = stmt.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
            return null;
        }

        public bool Open()
        {
            try
            {
                conn.Open();
            }
            catch (MySqlException e)
            {
                Logger.LOG_DATABASE.Error("Could not connect to MySQL database at " + conn.DataSource + ": " + e.Message);
                MessageBox.Show("Konnte keine Verbindung zur MySQL-Datenbank herstellen. Bitte überprüfen Sie die Einstellungen.", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (conn.State == ConnectionState.Open)
            {
                m_conn.Add(this);
                Logger.LOG_DATABASE.Info("MySQL server ver: " + conn.ServerVersion);
                Logger.LOG_DATABASE.Info("Connected to MySQL database " + conn.Database + " at " + conn.DataSource);
            }

            return true;
        }

        private void Close()
        {
            conn.Close();
            m_conn.Remove(this);

            Logger.LOG_DATABASE.Info("Closed MySQL connection to database " + conn.Database + " at " + conn.DataSource);
        }

        public static void CloseConnections()
        {
            for (int i = m_conn.Count - 1; i >= 0; i--)
            {
                m_conn[i].Close();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:SQL-Abfragen auf Sicherheitsrisiken überprüfen")]
        protected void PrepareStatement(int index, string sql)
        {
            try
            {
                if (conn.State == ConnectionState.Open)
                {
                    MySqlCommand stmt = new MySqlCommand(sql, conn);
                    stmt.Prepare();

                    m_stmts.Insert(index, stmt);
                }
            }
            catch (Exception e)
            {
                Logger.LOG_DATABASE.Error("Could not prepare statement. Id: " + index + ", sql: " + sql + e.Message);
            }
        }

        protected MySqlCommand GetPreparedStatement(int index)
        {
            MySqlCommand ret = m_stmts[index];
            if (ret == null)
                Logger.LOG_DATABASE.Error("Could not fetch prepared statement " + index.ToString() + " on database " + conn.Database.ToString() + ".");

            return ret;
        }
    }
}