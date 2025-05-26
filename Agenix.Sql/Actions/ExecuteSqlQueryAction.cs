using System.Data;
using System.Text;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Validation.Matcher;
using Microsoft.Extensions.Logging;
using Spring.Dao;
using Spring.Data;
using Spring.Data.Common;
using Spring.Transaction.Support;

namespace Agenix.Sql.Actions;

/// <summary>
///     Action executes SQL queries and offers result set validation. The class enables you to query data result sets from
///     a database. Validation will happen on column basis inside the result set.
/// </summary>
/// <param name="builder"></param>
public class ExecuteSqlQueryAction(ExecuteSqlQueryAction.Builder builder)
    : AbstractDatabaseConnectingTestAction(builder.GetName() ?? "sql-query", builder.GetDescription(),
        builder.dbProvider,
        builder.adoTemplate,
        builder.sqlResourcePath,
        builder.transactionIsolationLevel,
        builder.transactionManager,
        builder.transactionTimeout,
        builder.statements)
{
    /// Logger for ExecuteSQLQueryAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ExecuteSqlQueryAction));

    /// NULL value representation in SQL.
    private static readonly string NullValue = "NULL";

    /// Dictionary containing the expected values for each database column. Keys represent the column names and values are lists of expected data for those columns.
    protected readonly Dictionary<string, List<string>> _controlResultSet = builder._controlResultSet;

    /// Map of test variables to be created from database values, where keys represent the column names and values represent the variable names to which the database values will be assigned.
    private readonly Dictionary<string, string> _extractVariables = builder._extractVariables;

    /// Dictionary containing the expected values for each database column. Keys represent the column names and values are lists of expected data for those columns.
    public Dictionary<string, List<string>> ControlResultSet => _controlResultSet;

    /// Map of test variables to be created from database values, where keys represent the column names and values represent the variable names to which the database values will be assigned.
    public Dictionary<string, string> ExtractVariables => _extractVariables;

    protected void ExecuteStatements(List<string> newStatements, List<Dictionary<string, object>> allResultRows,
        Dictionary<string, List<string>> columnValuesMap, TestContext context)
    {
        if (AdoTemplate == null) throw new AgenixSystemException("No AdoTemplate configured for query execution!");

        foreach (var statement in newStatements)
        {
            ValidateSqlStatement(statement);

            var toExecute = context.ReplaceDynamicContentInString(statement.Trim().EndsWith(';')
                ? statement.Trim()[..(statement.Trim().Length - 1)]
                : statement.Trim());

            Log.LogDebug($"Executing SQL query: {toExecute}");

            var results = AdoTemplate.QueryWithRowMapper(CommandType.Text, toExecute, new DictionaryRowMapper())
                .Cast<Dictionary<string, object>>().ToList();

            Log.LogDebug("SQL query execution successful");

            allResultRows.AddRange(results);
            FillColumnValuesMap(results, columnValuesMap);
        }
    }

    /// <summary>
    ///     Populates the test context with variables extracted from a map of column values, using the specified variable
    ///     mapping configuration.
    /// </summary>
    /// <param name="columnValuesMap">
    ///     A dictionary where keys are column names and values are lists of string representations
    ///     of column values. The column names can be in any case format.
    /// </param>
    /// <param name="context">
    ///     An instance of <c>TestContext</c> where the variables will be set. The context is updated with
    ///     new variables based on the provided column values map.
    /// </param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when a column specified in the variable mapping configuration cannot be
    ///     found in the column values map.
    /// </exception>
    private void FillContextVariables(Dictionary<string, List<string>> columnValuesMap, TestContext context)
    {
        foreach (var variableEntry in _extractVariables)
        {
            var columnName = variableEntry.Key;
            if (columnValuesMap.ContainsKey(columnName.ToLower()))
                context.SetVariable(variableEntry.Value, ConstructVariableValue(columnValuesMap[columnName.ToLower()]));
            else if (columnValuesMap.ContainsKey(columnName.ToUpper()))
                context.SetVariable(variableEntry.Value, ConstructVariableValue(columnValuesMap[columnName.ToUpper()]));
            else
                throw new AgenixSystemException(
                    $"Failed to create variables from database values! Unable to find column '{columnName}' in database result set");
        }
    }

    /// <summary>
    ///     Populates a map with column names as keys and a list of string representations of column values as values, using
    ///     the provided result set.
    /// </summary>
    /// <param name="results">
    ///     A list of dictionaries where each dictionary represents a row from the result set, with column
    ///     names as keys and their corresponding cell values as values.
    /// </param>
    /// <param name="columnValuesMap">
    ///     A dictionary to be filled with column names as keys and lists of string representations
    ///     of the corresponding column values for all rows as values. If a column value is a byte array, it is converted to a
    ///     Base64 string.
    /// </param>
    private void FillColumnValuesMap(List<Dictionary<string, object>> results,
        Dictionary<string, List<string>> columnValuesMap)
    {
        foreach (var row in results)
        foreach (var column in row)
        {
            var columnName = column.Key;
            string columnValue;

            if (!columnValuesMap.ContainsKey(columnName)) columnValuesMap[columnName] = [];

            if (column.Value is byte[] byteArray)
                columnValue = Convert.ToBase64String(byteArray);
            else
                columnValue = (column.Value == null ? null : column.Value.ToString())!;

            columnValuesMap[columnName].Add(columnValue);
        }
    }

    /// <summary>
    ///     Constructs a single string value by concatenating the given list of row values, separating each value with a
    ///     semicolon.
    /// </summary>
    /// <param name="rowValues">
    ///     A list of string values representing a row in the result set, where each value corresponds to a
    ///     column.
    /// </param>
    /// <returns>
    ///     A concatenated string with semicolon-separated values from the row, substituting any null values with the
    ///     string "NULL". If the list is null or empty, returns an empty string.
    /// </returns>
    private string ConstructVariableValue(List<string> rowValues)
    {
        if (rowValues == null || rowValues.Count == 0) return string.Empty;

        if (rowValues.Count == 1) return rowValues[0] == null ? NullValue : rowValues[0];

        var result = new StringBuilder();

        using (var it = rowValues.GetEnumerator())
        {
            if (it.MoveNext()) result.Append(it.Current);
            while (it.MoveNext())
            {
                var nextValue = it.Current;
                result.Append(';').Append(nextValue == null ? NullValue : nextValue);
            }
        }

        return result.ToString();
    }

    /// <summary>
    ///     Validates the result set of the SQL query using the specified column values map and all result rows.
    /// </summary>
    /// <param name="columnValuesMap">
    ///     A dictionary containing column names and their associated list of values from the
    ///     executed SQL result set.
    /// </param>
    /// <param name="allResultRows">
    ///     A list of dictionaries representing all rows and their column data returned from the SQL
    ///     query execution.
    /// </param>
    /// <param name="context">
    ///     The test execution context providing essential information and utilities for performing
    ///     validation within the environment.
    /// </param>
    private void PerformValidation(Dictionary<string, List<string>> columnValuesMap,
        List<Dictionary<string, object>> allResultRows,
        TestContext context)
    {
        // Now apply control set validation if specified
        if (!_controlResultSet.Any()) return;

        PerformControlResultSetValidation(columnValuesMap, context);
        Log.LogDebug("SQL query validation successful: All values OK");
    }

    /// <summary>
    ///     Validates the result set of the SQL query against the control result set on a column-by-column basis.
    /// </summary>
    /// <param name="columnValuesMap">
    ///     A dictionary mapping column names to their corresponding list of values obtained from the
    ///     executed SQL result set.
    /// </param>
    /// <param name="context">
    ///     The context in which the validation is executed, providing environmental information and
    ///     utilities.
    /// </param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when the expected column is not found in the result set or when the number
    ///     of rows in a column does not match the expected count.
    /// </exception>
    private void PerformControlResultSetValidation(Dictionary<string, List<string>> columnValuesMap,
        TestContext context)
    {
        foreach (var controlEntry in _controlResultSet)
        {
            var columnName = controlEntry.Key;

            if (columnValuesMap.ContainsKey(columnName.ToLower()))
                columnName = columnName.ToLower();
            else if (columnValuesMap.ContainsKey(columnName.ToUpper()))
                columnName = columnName.ToUpper();
            else if (!columnValuesMap.ContainsKey(columnName))
                throw new AgenixSystemException($"Could not find column '{columnName}' in SQL result set");

            var resultColumnValues = columnValuesMap[columnName];
            var controlColumnValues = controlEntry.Value;

            // First, check the size of column values (representing the number of allResultRows in the result set)
            if (resultColumnValues.Count != controlColumnValues.Count)
                throw new AgenixSystemException(
                    $"Validation failed for column: '{columnName}' expected rows count: {controlColumnValues.Count} but was {resultColumnValues.Count}");

            using var it = resultColumnValues.GetEnumerator();
            foreach (var controlValue in controlColumnValues)
            {
                it.MoveNext();
                var resultValue = it.Current;
                // Check if controlValue is variable or function (and resolve it)
                var resolvedControlValue = context.ReplaceDynamicContentInString(controlValue);

                ValidateSingleValue(columnName, resolvedControlValue, resultValue, context);
            }
        }
    }

    /// <summary>
    ///     Does some simple validation on the SQL statement.
    /// </summary>
    /// <param name="statement"></param>
    /// <exception cref="AgenixSystemException"></exception>
    protected void ValidateSqlStatement(string statement)
    {
        var trimmedStatement = statement.ToLower().Trim();
        if (!(trimmedStatement.StartsWith("select") || trimmedStatement.StartsWith("with")))
            throw new AgenixSystemException($"Missing SELECT or WITH keyword in statement: {trimmedStatement}");
    }

    /// <summary>
    ///     Validates a single value from a database query result set against a specified control value.
    ///     This method checks if the control value is ignored, a matcher expression, or if it matches the expected value.
    /// </summary>
    /// <param name="columnName">The name of the column being validated.</param>
    /// <param name="controlValue">The expected value to validate against the result value, or a predefined placeholder.</param>
    /// <param name="resultValue">The actual value obtained from the database query result set, which can be null.</param>
    /// <param name="context">
    ///     The context within which the validation is being executed, providing access to the test
    ///     environment.
    /// </param>
    /// <exception cref="ValidationException">
    ///     Thrown if the validation fails due to a mismatch between the control and result
    ///     values.
    /// </exception>
    protected void ValidateSingleValue(string columnName, string controlValue, string? resultValue, TestContext context)
    {
        // Check if value is ignored
        if (controlValue.Equals(AgenixSettings.IgnorePlaceholder))
        {
            Log.LogDebug($"Ignoring column value '{columnName} (resultValue)'");
            return;
        }

        if (ValidationMatcherUtils.IsValidationMatcherExpression(controlValue))
        {
            ValidationMatcherUtils.ResolveValidationMatcher(columnName, resultValue, controlValue, context);
            return;
        }

        if (resultValue == null)
        {
            if (!IsAgenixNullValue(controlValue))
                throw new ValidationException(
                    $"Validation failed for column: '{columnName}' found value: NULL expected value: {controlValue}");
            Log.LogDebug($"Validating database value for column: '{columnName}' value as expected: NULL - value OK");
            return;
        }

        if (resultValue.Equals(controlValue))
            Log.LogDebug($"Validation successful for column: '{columnName}' expected value: {controlValue} - value OK");
        else
            throw new ValidationException(
                $"Validation failed for column: '{columnName}' found value: '{resultValue}' expected value: {(string.IsNullOrEmpty(controlValue) ? "NULL" : controlValue)}");
    }

    /// <summary>
    ///     Determines if the specified control value is considered a NULL value in the context of this SQL query action.
    ///     A value is considered NULL if it matches the string representation of a SQL NULL value or if it is an empty string.
    /// </summary>
    /// <param name="controlValue">The control value to check against the representation of SQL NULL value.</param>
    /// <returns>True if the control value is considered NULL; otherwise, false.</returns>
    private bool IsAgenixNullValue(string controlValue)
    {
        return controlValue.Equals(NullValue, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(controlValue);
    }

    /// <summary>
    ///     Executes the SQL statements defined for the test action within the given context. This method determines the source
    ///     of SQL statements,
    ///     executes them, performs necessary validations on the result set, and populates the test context with any resulting
    ///     variables.
    /// </summary>
    /// <param name="context">The test context providing data and configuration needed to execute the SQL statements.</param>
    /// <exception cref="DataAccessException">
    ///     Thrown when there is a failure in executing any SQL statement due to data access
    ///     issues.
    /// </exception>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when a <see cref="DataAccessException" /> occurs, encapsulating it for
    ///     higher-level handling.
    /// </exception>
    public override void DoExecute(TestContext context)
    {
        List<string> statementsToUse;
        statementsToUse = statements.Count == 0 ? CreateStatementsFromFileResource(context) : statements;

        try
        {
            // For control result set validation
            var columnValuesMap = new Dictionary<string, List<string>>();
            // For script validation
            var allResultRows = new List<Dictionary<string, object>>();

            if (TransactionManager != null)
            {
                Log.LogDebug($"Using transaction manager: {TransactionManager.GetType().Name}");

                var transactionTemplate = new TransactionTemplate(TransactionManager)
                {
                    TransactionTimeout = int.Parse(context.ReplaceDynamicContentInString(TransactionTimeout)),
                    TransactionIsolationLevel = (IsolationLevel)Enum.Parse(typeof(IsolationLevel),
                        context.ReplaceDynamicContentInString(TransactionIsolationLevel))
                };

                transactionTemplate.Execute(status =>
                {
                    ExecuteStatements(statementsToUse, allResultRows, columnValuesMap, context);
                    return null;
                });
            }
            else
            {
                ExecuteStatements(statementsToUse, allResultRows, columnValuesMap, context);
            }

            // Perform validation
            PerformValidation(columnValuesMap, allResultRows, context);

            // Fill context variables
            FillContextVariables(columnValuesMap, context);
        }
        catch (DataAccessException e)
        {
            Log.LogError(e, "Failed to execute SQL statement");
            throw new AgenixSystemException(e.Message, e);
        }
    }

    /// <summary>
    ///     Implements IRowMapper to map rows from a data reader to a dictionary, where each key-value pair represents a column
    ///     name and the corresponding value from the data reader.
    /// </summary>
    public class DictionaryRowMapper : IRowMapper
    {
        public object MapRow(IDataReader dataReader, int rowNum)
        {
            var result = new Dictionary<string, object>();

            for (var i = 0; i < dataReader.FieldCount; i++) result[dataReader.GetName(i)] = dataReader.GetValue(i);

            return result;
        }
    }

    /// <summary>
    ///     Action Builder
    /// </summary>
    public class Builder : AbstractDatabaseBuilder<ExecuteSqlQueryAction, Builder>
    {
        internal readonly Dictionary<string, List<string>> _controlResultSet = new();
        internal readonly Dictionary<string, string> _extractVariables = new();

        /// <summary>
        ///     Creates and returns a new instance of the ExecuteSQLQueryAction.Builder class.
        /// </summary>
        /// <returns>A new instance of the ExecuteSQLQueryAction.Builder class.</returns>
        public static Builder Query()
        {
            return new Builder();
        }

        /// <summary>
        ///     Creates and returns a new instance of the ExecuteSQLQueryAction.Builder class.
        /// </summary>
        /// <returns>A new instance of the ExecuteSQLQueryAction.Builder class.</returns>
        public static Builder Query(IDbProvider iDbProvider)
        {
            var builder = new Builder();
            builder.DbProvider(iDbProvider);
            return builder;
        }

        /// <summary>
        ///     Validates the specified column against the provided set of values.
        /// </summary>
        /// <param name="column">The name of the column to be validated.</param>
        /// <param name="values">A set of values to validate the column against.</param>
        /// <returns>An instance of the builder for method chaining.</returns>
        public Builder Validate(string column, params string[] values)
        {
            _controlResultSet[column] = values.ToList();
            return this;
        }

        /// <summary>
        ///     User can extract column values to test variables. Map holds column names (keys) and respective target variable
        ///     names (values).
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public Builder Extract(string columnName, string variableName)
        {
            _extractVariables[columnName] = variableName;
            return this;
        }

        public override ExecuteSqlQueryAction Build()
        {
            return new ExecuteSqlQueryAction(this);
        }
    }
}