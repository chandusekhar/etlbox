﻿using ETLBox.Connection;
using ETLBox.Exceptions;
using System;

namespace ETLBox.ControlFlow.Tasks
{
    /// <summary>
    /// Will create a database if the database doesn't exists. In MySql or MariaDb, this will create a schema.
    /// </summary>
    /// <example>
    /// <code>
    /// CreateDatabaseTask.Create("DemoDB");
    /// </code>
    /// </example>
    public sealed class CreateDatabaseTask : ControlFlowTask
    {
        /// <inheritdoc/>
        public override string TaskName { get; set; } = $"Create database";

        /// <summary>
        /// Runs the sql code to create the database.
        /// Throws an exception if the database already exists. 
        /// </summary>
        public void Create() {
            ThrowOnError = true;
            Execute();
        }

        /// <summary>
        /// Runs the sql code to create the database if the database doesn't exist yet.
        /// </summary>
        public void CreateIfNotExists() => Execute();

        /// <summary>
        /// The name of the database (In MySql: The schema name)
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Sql Server only: The recovery model of the database
        /// </summary>
        public RecoveryModel RecoveryModel { get; set; } = RecoveryModel.Simple;

        /// <summary>
        /// The default collation for the database
        /// </summary>
        public string Collation { get; set; }

        /// <summary>
        /// The sql code that is used to generate the database
        /// </summary>
        public string Sql {
            get {
                if (ConnectionType == ConnectionManagerType.SqlServer) {
                    return
        $@"
USE [master]

CREATE DATABASE {QB}{DatabaseName}{QE} {CollationString} 
{RecoveryString}

--wait for database to enter 'ready' state
DECLARE @dbReady BIT = 0
WHILE (@dbReady = 0)
BEGIN
SELECT @dbReady = CASE WHEN DATABASEPROPERTYEX('{DatabaseName}', 'Collation') IS NULL THEN 0 ELSE 1 END                    
END
";
                } else {
                    return $@"CREATE DATABASE {QB}{DatabaseName}{QE} {CollationString}";
                }
            }
        }

        public CreateDatabaseTask() {
        }

        public CreateDatabaseTask(string databaseName) : this() {
            DatabaseName = databaseName;
        }

        public CreateDatabaseTask(string databaseName, string collation) : this(databaseName) {
            Collation = collation;
        }

        internal bool ThrowOnError { get; set; }

        internal void Execute() {
            if (!DbConnectionManager.SupportDatabases)
                throw new NotSupportedException($"This task is not supported with the current connection manager ({ConnectionType})");

            bool doesExist = new IfDatabaseExistsTask(DatabaseName) { DisableLogging = true, ConnectionManager = ConnectionManager }.Exists();
            if (doesExist && ThrowOnError) throw new ETLBoxException($"Database {DatabaseName} already exists - can't create the database!");
            if (!doesExist)
                new SqlTask(this, Sql).ExecuteNonQuery();
        }

        string RecoveryModelAsString {
            get {
                if (RecoveryModel == RecoveryModel.Simple)
                    return "SIMPLE";
                else if (RecoveryModel == RecoveryModel.BulkLogged)
                    return "BULK";
                else if (RecoveryModel == RecoveryModel.Full)
                    return "FULL";
                else return string.Empty;
            }
        }
        bool HasCollation => !String.IsNullOrWhiteSpace(Collation);
        string CollationString {
            get {
                if (!HasCollation) return string.Empty;
                if (ConnectionType == ConnectionManagerType.Postgres)
                    return "LC_COLLATE '" + Collation + "'";
                else
                    return "COLLATE " + Collation;
            }
        }
        string RecoveryString => RecoveryModel != RecoveryModel.Default ?
            $"ALTER DATABASE [{DatabaseName}] SET RECOVERY {RecoveryModelAsString} WITH no_wait"
            : string.Empty;

        /// <summary>
        /// Creates a database. In MySql or MariaDb, this will create a schema.
        /// Will throw an exception if the database already exists.
        /// Make sure that your default connection string points to the server itself and to an existing database (e.g. a system database).
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        public static void Create(string databaseName)
            => new CreateDatabaseTask(databaseName) { ThrowOnError = true }.Execute();

        /// <summary>
        /// Creates a database. In MySql or MariaDb, this will create a schema.
        /// Will throw an exception if the database already exists.
        /// Make sure that your default connection string points to the server itself and to an existing database (e.g. a system database).
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="collation">The default collation of the database.</param>
        public static void Create(string databaseName, string collation)
            => new CreateDatabaseTask(databaseName, collation) { ThrowOnError = true }.Execute();

        /// <summary>
        /// Creates a database. In MySql or MariaDb, this will create a schema.
        /// Will throw an exception if the database already exists.
        /// </summary>
        /// <param name="connectionManager">The connection manager of the server you want to connect. Make sure this points to a database
        /// that does exist (e.g. a system database)</param>
        /// <param name="databaseName">The name of the database</param>
        public static void Create(IConnectionManager connectionManager, string databaseName)
            => new CreateDatabaseTask(databaseName) { ConnectionManager = connectionManager, ThrowOnError = true }.Execute();

        /// <summary>
        /// Creates a database. In MySql or MariaDb, this will create a schema.
        /// Will throw an exception if the database already exists.
        /// </summary>
        /// <param name="connectionManager">The connection manager of the server you want to connect. Make sure this points to a database
        /// that does exist (e.g. a system database)</param>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="collation">The default collation of the database.</param>
        public static void Create(IConnectionManager connectionManager, string databaseName, string collation)
            => new CreateDatabaseTask(databaseName, collation) { ConnectionManager = connectionManager, ThrowOnError = true }.Execute();

        /// <summary>
        /// Creates a database if the database doesn't exists. In MySql or MariaDb, this will create a schema.
        /// Make sure that your default connection string points to the server itself and to an existing database (e.g. a system database).
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        public static void CreateIfNotExists(string databaseName) => new CreateDatabaseTask(databaseName).Execute();

        /// <summary>
        /// Creates a database if the database doesn't exists. In MySql or MariaDb, this will create a schema.
        /// Make sure that your default connection string points to the server itself and to an existing database (e.g. a system database).
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="collation">The default collation of the database.</param>
        public static void CreateIfNotExists(string databaseName, string collation) => new CreateDatabaseTask(databaseName, collation).Execute();

        /// <summary>
        /// Creates a database if the database doesn't exists. In MySql or MariaDb, this will create a schema.
        /// </summary>
        /// <param name="connectionManager">The connection manager of the server you want to connect. Make sure this points to a database
        /// that does exist (e.g. a system database)</param>
        /// <param name="databaseName">The name of the database</param>
        public static void CreateIfNotExists(IConnectionManager connectionManager, string databaseName)
            => new CreateDatabaseTask(databaseName) { ConnectionManager = connectionManager }.Execute();

        /// <summary>
        /// Creates a database if the database doesn't exists. In MySql or MariaDb, this will create a schema.
        /// </summary>
        /// <param name="connectionManager">The connection manager of the server you want to connect. Make sure this points to a database
        /// that does exist (e.g. a system database)</param>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="collation">The default collation of the database.</param>
        public static void CreateIfNotExists(IConnectionManager connectionManager, string databaseName, string collation)
            => new CreateDatabaseTask(databaseName, collation) { ConnectionManager = connectionManager }.Execute();
    }


    /// <summary>
    /// The sql server recovery models.
    /// </summary>
    public enum RecoveryModel
    {
        Default, Simple, BulkLogged, Full
    }

}
