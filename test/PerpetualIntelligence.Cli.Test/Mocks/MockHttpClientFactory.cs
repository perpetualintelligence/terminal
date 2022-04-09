/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Collections.Generic;
using System.Net.Http;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            if (!clients.ContainsKey(name))
            {
                if (name == "invalid_based_address")
                {
                    clients.Add(name, new HttpClient() { BaseAddress = new System.Uri("https://api.perpetualintelligence.com/invalid/") });
                }
                else
                {
                    clients.Add(name, new HttpClient() { BaseAddress = new System.Uri("https://api.perpetualintelligence.com/security/") });
                }
            }

            return clients[name];
        }

        private Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();
    }
}
