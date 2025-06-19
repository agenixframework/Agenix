#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Text.Json;
using System.Text.RegularExpressions;
using Agenix.Api.Message;

namespace Agenix.GraphQL.Message;

/// <summary>
///     Utility class providing helper methods for GraphQL message operations.
/// </summary>
public static class GraphQLMessageUtils
{
    /// <summary>
    ///     Creates a GraphQL message with variables.
    /// </summary>
    /// <param name="query">The GraphQL operation string.</param>
    /// <param name="variables">The variables dictionary.</param>
    /// <returns>A new GraphQLMessage with the specified query and variables.</returns>
    public static GraphQLMessage CreateWithVariables(string query, Dictionary<string, object?> variables)
    {
        return new GraphQLMessage(query, variables);
    }

    /// <summary>
    ///     Creates a GraphQL message from a JSON payload.
    /// </summary>
    /// <param name="jsonPayload">The JSON string containing the GraphQL request.</param>
    /// <returns>A GraphQLMessage parsed from the JSON.</returns>
    public static GraphQLMessage FromJsonPayload(string jsonPayload)
    {
        return GraphQLMessage.FromJson(jsonPayload);
    }

    /// <summary>
    ///     Validates if a string is a valid GraphQL operation.
    /// </summary>
    /// <param name="graphqlString">The GraphQL string to validate.</param>
    /// <returns>True if the string appears to be a valid GraphQL operation, false otherwise.</returns>
    public static bool IsValidGraphQLOperation(string graphqlString)
    {
        if (string.IsNullOrWhiteSpace(graphqlString))
        {
            return false;
        }

        var trimmed = graphqlString.Trim();

        // Check for basic GraphQL structure patterns
        var patterns = new[]
        {
            @"^\s*(query|mutation|subscription)\s*(\w+)?\s*(\([^)]*\))?\s*\{",
            @"^\s*\{\s*\w+", // Shorthand query syntax
            @"^\s*fragment\s+\w+\s+on\s+\w+"
        };

        return patterns.Any(pattern => Regex.IsMatch(trimmed, pattern, RegexOptions.IgnoreCase));
    }

    /// <summary>
    ///     Extracts the operation name from a GraphQL string.
    /// </summary>
    /// <param name="graphqlString">The GraphQL operation string.</param>
    /// <returns>The operation name if found, null otherwise.</returns>
    public static string? ExtractOperationName(string graphqlString)
    {
        if (string.IsNullOrWhiteSpace(graphqlString))
        {
            return null;
        }

        var match = Regex.Match(graphqlString, @"^\s*(query|mutation|subscription)\s+(\w+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[2].Value : null;
    }

    /// <summary>
    ///     Extracts variable definitions from a GraphQL operation string.
    /// </summary>
    /// <param name="graphqlString">The GraphQL operation string.</param>
    /// <returns>A list of variable names found in the operation.</returns>
    public static List<string> ExtractVariableNames(string graphqlString)
    {
        if (string.IsNullOrWhiteSpace(graphqlString))
        {
            return new List<string>();
        }

        var matches = Regex.Matches(graphqlString, @"\$(\w+)", RegexOptions.IgnoreCase);
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    /// <summary>
    ///     Formats a GraphQL operation string for better readability.
    /// </summary>
    /// <param name="graphqlString">The GraphQL string to format.</param>
    /// <returns>A formatted GraphQL string.</returns>
    public static string FormatGraphQL(string graphqlString)
    {
        if (string.IsNullOrWhiteSpace(graphqlString))
        {
            return string.Empty;
        }

        // Basic formatting - remove extra whitespace and add proper line breaks
        var formatted = graphqlString.Trim();
        formatted = Regex.Replace(formatted, @"\s+", " ");
        formatted = Regex.Replace(formatted, @"\s*{\s*", " {\n  ");
        formatted = Regex.Replace(formatted, @"\s*}\s*", "\n}");
        formatted = Regex.Replace(formatted, @"\s*,\s*", ",\n  ");

        return formatted;
    }

    /// <summary>
    ///     Converts a regular message to a GraphQL message.
    /// </summary>
    /// <param name="message">The source message to convert.</param>
    /// <returns>A new GraphQLMessage with content from the source message.</returns>
    /// Copies settings from a source message to a target HTTP message, converting if necessary.
    /// @param from The source message, which can be either an IMessage or HttpMessage.
    /// @param to The target HTTP message to which settings will be applied.
    /// /
    public static void Copy(IMessage from, GraphQLMessage to)
    {
        GraphQLMessage source;
        if (from is GraphQLMessage httpMessage)
        {
            source = httpMessage;
        }
        else
        {
            source = new GraphQLMessage(from);
        }

        Copy(source, to);
    }

    /// <summary>
    /// Copies the properties and headers from one GraphQLMessage instance to another.
    /// </summary>
    /// <param name="from">the source GraphQLMessage from which properties are to be copied</param>
    /// <param name="to">the target GraphQLMessage to which properties are to be copied</param>
    private static void Copy(GraphQLMessage from, GraphQLMessage to)
    {
        to.Name = from.Name;
        to.SetType(from.GetType());
        to.Payload = from.Payload;

        foreach (var entry in from.GetHeaders().Where(entry =>
                     !entry.Key.Equals(MessageHeaders.Id) && !entry.Key.Equals(MessageHeaders.Timestamp)))
        {
            to.Header(entry.Key, entry.Value);
        }

        foreach (var headerData in from.GetHeaderData())
        {
            to.AddHeaderData(headerData);
        }

        // Copy cookies
        if (from.Cookies != null)
        {
            to.Cookies = new Dictionary<string, string>(from.Cookies);
        }

        // Copy extensions
        if (from.Extensions != null)
        {
            to.Extensions = new Dictionary<string, object>(from.Extensions);
        }
    }


    /// <summary>
    ///     Creates a GraphQL error response message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="path">The path where the error occurred (optional).</param>
    /// <param name="extensions">Additional error extensions (optional).</param>
    /// <returns>A GraphQLMessage representing an error response.</returns>
    public static GraphQLMessage CreateErrorResponse(string errorMessage, List<string>? path = null,
        Dictionary<string, object>? extensions = null)
    {
        var errorResponse = new Dictionary<string, object>
        {
            ["errors"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["message"] = errorMessage,
                    ["path"] = path ?? new List<string>(),
                    ["extensions"] = extensions ?? new Dictionary<string, object>()
                }
            }
        };

        var message = new GraphQLMessage
        {
            Payload = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true })
        };

        message.SetHeader(GraphQLMessageHeaders.ContentType, "application/json");
        return message;
    }

    /// <summary>
    ///     Checks if a GraphQL message represents an error response.
    /// </summary>
    /// <param name="message">The GraphQL message to check.</param>
    /// <returns>True if the message contains errors, false otherwise.</returns>
    public static bool IsErrorResponse(GraphQLMessage message)
    {
        if (message.Payload is not string stringPayload)
        {
            return false;
        }

        try
        {
            var jsonDocument = JsonDocument.Parse(stringPayload);
            return jsonDocument.RootElement.TryGetProperty("errors", out _);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Extracts error messages from a GraphQL error response.
    /// </summary>
    /// <param name="message">The GraphQL error response message.</param>
    /// <returns>A list of error messages.</returns>
    public static List<string> ExtractErrorMessages(GraphQLMessage message)
    {
        var errorMessages = new List<string>();

        if (message.Payload is not string stringPayload)
        {
            return errorMessages;
        }

        try
        {
            var jsonDocument = JsonDocument.Parse(stringPayload);
            if (jsonDocument.RootElement.TryGetProperty("errors", out var errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in errorsElement.EnumerateArray())
                {
                    if (error.TryGetProperty("message", out var messageElement))
                    {
                        var errorMessage = messageElement.GetString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            errorMessages.Add(errorMessage);
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return errorMessages;
    }

    /// <summary>
    ///     Detects the GraphQL operation type from a query string.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>The detected operation type.</returns>
    private static GraphQLOperationType DetectOperationType(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GraphQLOperationType.QUERY;
        }

        var trimmedQuery = query.Trim().ToLowerInvariant();

        if (trimmedQuery.StartsWith("mutation"))
        {
            return GraphQLOperationType.MUTATION;
        }

        if (trimmedQuery.StartsWith("subscription"))
        {
            return GraphQLOperationType.SUBSCRIPTION;
        }

        return GraphQLOperationType.QUERY;
    }

    /// <summary>
    ///     Creates a GraphQL message for an introspection query.
    /// </summary>
    /// <returns>A GraphQLMessage containing the standard introspection query.</returns>
    public static GraphQLMessage CreateIntrospectionQuery()
    {
        const string introspectionQuery = """
                                          query IntrospectionQuery {
                                            __schema {
                                              queryType { name }
                                              mutationType { name }
                                              subscriptionType { name }
                                              types {
                                                ...FullType
                                              }
                                              directives {
                                                name
                                                description
                                                locations
                                                args {
                                                  ...InputValue
                                                }
                                              }
                                            }
                                          }

                                          fragment FullType on __Type {
                                            kind
                                            name
                                            description
                                            fields(includeDeprecated: true) {
                                              name
                                              description
                                              args {
                                                ...InputValue
                                              }
                                              type {
                                                ...TypeRef
                                              }
                                              isDeprecated
                                              deprecationReason
                                            }
                                            inputFields {
                                              ...InputValue
                                            }
                                            interfaces {
                                              ...TypeRef
                                            }
                                            enumValues(includeDeprecated: true) {
                                              name
                                              description
                                              isDeprecated
                                              deprecationReason
                                            }
                                            possibleTypes {
                                              ...TypeRef
                                            }
                                          }

                                          fragment InputValue on __InputValue {
                                            name
                                            description
                                            type { ...TypeRef }
                                            defaultValue
                                          }

                                          fragment TypeRef on __Type {
                                            kind
                                            name
                                            ofType {
                                              kind
                                              name
                                              ofType {
                                                kind
                                                name
                                                ofType {
                                                  kind
                                                  name
                                                  ofType {
                                                    kind
                                                    name
                                                    ofType {
                                                      kind
                                                      name
                                                      ofType {
                                                        kind
                                                        name
                                                        ofType {
                                                          kind
                                                          name
                                                        }
                                                      }
                                                    }
                                                  }
                                                }
                                              }
                                            }
                                          }
                                          """;
        var graphQlMessage = new GraphQLMessage(introspectionQuery);
        graphQlMessage.SetOperationName("IntrospectionQuery");
        return graphQlMessage;
    }
}
