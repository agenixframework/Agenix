#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

#region Imports

using System.Text;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

#endregion

namespace Agenix.Sql.Util;

/// Utility class for handling SQL scripts, specifically for reading and processing
/// SQL statements from file resources. It provides capabilities to parse multiple
/// SQL statements and optionally apply custom decorations to the script lines.
public abstract class SqlUtils
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SqlUtils));

    /// <summary>
    ///     Constant representing the SQL comment delimiter.
    /// </summary>
    private static readonly string SqlComment = "--";

    /// <summary>
    ///     The default SQL statement ending character sequence used in SQL scripts.
    /// </summary>
    private static readonly string StmtEnding = ";";

    /// Provides utility methods for processing SQL statements from external file resources.
    /// This class cannot be instantiated.
    /// /
    private SqlUtils()
    {
    }

    /// Reads SQL statements from an external file resource. The file resource may contain
    /// multiple multi-line statements and comments.
    /// <param name="sqlResource">The SQL file resource containing the statements.</param>
    /// <returns>A list of SQL statements parsed from the file resource.</returns>
    public static List<string> CreateStatementsFromFileResource(IResource sqlResource)
    {
        return CreateStatementsFromFileResource(sqlResource, null);
    }

    /// Reads SQL statements from an external file resource. The file resource can contain
    /// multiple multi-line statements and comments. This method can also apply an optional
    /// line decorator to the last lines of the script.
    /// <param name="sqlResource">The SQL file resource containing the statements.</param>
    /// <param name="lineDecorator">Optional line decorator for the last script lines, may be null.</param>
    /// <returns>A list of SQL statements parsed from the file resource.</returns>
    public static List<string> CreateStatementsFromFileResource(IResource sqlResource,
        ILastScriptLineDecorator? lineDecorator)
    {
        var stmts = new List<string>();

        try
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Create statements from SQL file: " + sqlResource.File.FullName);
            }

            var inputStream = sqlResource.InputStream;
            var buffer = new StringBuilder();


            using var reader = new StreamReader(inputStream);
            while (reader.ReadLine() is { } line)
            {
                if (line.Trim().StartsWith(SqlComment) || string.IsNullOrWhiteSpace(line.Trim()))
                {
                    continue;
                }

                if (line.Trim().EndsWith(GetStatementEndingCharacter(lineDecorator)))
                {
                    buffer.Append(lineDecorator != null ? lineDecorator.Decorate(line) : line);


                    var stmt = buffer.ToString().Trim();

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.LogDebug("Found statement: " + stmt);
                    }

                    stmts.Add(stmt);
                    buffer.Clear();
                }
                else
                {
                    buffer.Append(line);
                    // More lines to come for this statement, add line break
                    buffer.Append('\n');
                }
            }
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Resource could not be found - filename: " + sqlResource, e);
        }

        return stmts;
    }

    /// <summary>
    ///     Retrieves the SQL statement ending character sequence.
    ///     If a line decorator is specified, it utilizes the character sequence provided by the decorator.
    ///     Otherwise, it returns the default sequence.
    /// </summary>
    /// <param name="lineDecorator">
    ///     An instance of <see cref="ILastScriptLineDecorator" /> that may provide a custom statement ending character
    ///     sequence.
    ///     If null, the default sequence is used.
    /// </param>
    /// <returns>
    ///     A string representing the SQL statement ending character sequence.
    /// </returns>
    public static string GetStatementEndingCharacter(ILastScriptLineDecorator? lineDecorator)
    {
        return lineDecorator != null ? lineDecorator.GetStatementEndingCharacter() : StmtEnding;
    }

    /// Line decorator for customizing the final lines of a SQL script.
    /// /
    public interface ILastScriptLineDecorator
    {
        /// Line decorator for customizing the final lines of a SQL script.
        /// <param name="line">The last script line finishing a SQL statement.</param>
        /// <returns>A possibly modified version of the last script line.</returns>
        string Decorate(string line);

        /// <summary>
        ///     Retrieves the SQL statement ending character sequence.
        ///     If a line decorator is specified, it utilizes the character sequence provided by the decorator.
        ///     Otherwise, it returns the default sequence.
        /// </summary>
        /// An instance of
        /// <see cref="ILastScriptLineDecorator" />
        /// that may provide a custom statement ending character
        /// sequence.
        /// If null, the default sequence is used.
        /// <returns>
        ///     A string representing the SQL statement ending character sequence.
        /// </returns>
        string GetStatementEndingCharacter();
    }
}
