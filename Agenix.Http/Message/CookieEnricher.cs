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

using System.Net;
using Agenix.Api.Context;

namespace Agenix.Http.Message;

/// Provides functionality to enrich a set of cookies by integrating dynamic information from a specified test context.
/// /
public class CookieEnricher
{
    /// Replaces the dynamic content in the provided list of cookies using context variables.
    /// <param name="cookies">The list of Cookies to be enriched.</param>
    /// <param name="context">The context used to replace dynamic variables within the cookie attributes.</param>
    /// <return>A list of enriched Cookies where dynamic content has been replaced.</return>
    /// /
    public List<Cookie> Enrich(List<Cookie> cookies, TestContext context)
    {
        var enrichedCookies = new List<Cookie>();

        foreach (var cookie in cookies)
        {
            var enrichedCookie = new Cookie(cookie.Name, cookie.Value);

            if (cookie.Value != null)
            {
                enrichedCookie.Value = context.ReplaceDynamicContentInString(cookie.Value);
            }

            if (cookie.Path != null)
            {
                enrichedCookie.Path = context.ReplaceDynamicContentInString(cookie.Path);
            }

            if (cookie.Domain != null)
            {
                enrichedCookie.Domain = context.ReplaceDynamicContentInString(cookie.Domain);
            }

            enrichedCookie.HttpOnly = cookie.HttpOnly;
            enrichedCookie.Secure = cookie.Secure;

            enrichedCookies.Add(enrichedCookie);
        }

        return enrichedCookies;
    }
}
