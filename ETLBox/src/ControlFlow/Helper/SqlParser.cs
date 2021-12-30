﻿using ETLBox.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TSQL;
using TSQL.Statements;

namespace ETLBox.Helper
{
    /// <summary>
    /// Helper class for parsing sql statements
    /// </summary>
    public static class SqlParser
    {
        /// <summary>
        /// This method attempts to parse column names from any sql statement.
        /// E.g. SELECT 1 AS 'Test', Col2, t2.Col3 FROM table1 t1 INNER JOIN t2 ON t1.Id = t2.Id
        /// will return Test, Col2 and Col3 als column names.
        /// </summary>
        /// <param name="sql">The sql code from which the column names should be parsed</param>
        /// <returns>The names of the columns in the sql</returns>
        public static List<string> ParseColumnNames(string sql) {
            try {
                var statement = TSQLStatementReader.ParseStatements(sql).FirstOrDefault() as TSQLSelectStatement;

                List<string> result = new List<string>();
                int functionStartCount = 0;
                string prevToken = string.Empty;
                foreach (var token in statement.Select.Tokens) {
                    if (token.Type == TSQL.Tokens.TSQLTokenType.Character &&
                        token.Text == "(")
                        functionStartCount++;
                    else if (token.Type == TSQL.Tokens.TSQLTokenType.Character &&
                        token.Text == ")")
                        functionStartCount--;
                    if (token.Type == TSQL.Tokens.TSQLTokenType.Identifier || token.Type == TSQL.Tokens.TSQLTokenType.StringLiteral)
                        prevToken = token.Text;
                    if (token.Type == TSQL.Tokens.TSQLTokenType.Character &&
                        functionStartCount <= 0 &&
                        token.Text == ","
                        )
                        result.Add(prevToken);
                }
                if (prevToken != string.Empty)
                    result.Add(prevToken);
                return result;
            } catch (Exception) {
                throw new ETLBoxException("The attempt to read the column names from the given sql statement failed. " +
                    "Please provide a TableDefinition with at least column name and preferably the data type. ");
            }
        }
    }
}
