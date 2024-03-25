/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace OneImlx.Terminal.Mocks
{
    public class MockHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            if (!clients.ContainsKey(name))
            {
                if (name == "invalid_based_address")
                {
                    clients.Add(name, new HttpClient() { BaseAddress = new Uri("https://api.someone.com/") });
                }
                else if (name == "localhost")
                {
                    clients.Add(name, new HttpClient() { BaseAddress = new Uri(" http://localhost:7097/api/") });
                }
                else if (name == "prod")
                {
                    clients.Add(name, new HttpClient() { BaseAddress = new Uri("https://api.perpetualintelligence.com/") });
                }
                else if (name == "prod_fallback")
                {
                    clients.Add(name, new HttpClient() { BaseAddress = new Uri("https://piapim.azure-api.net/") });
                }
                else
                {
                    clients.Add(Options.DefaultName, new HttpClient());
                }
            }

            return clients[name];
        }

        private Dictionary<string, HttpClient> clients = new();
    }
}