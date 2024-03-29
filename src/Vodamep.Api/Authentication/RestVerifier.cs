﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using Connexia.Service.Client;

namespace Vodamep.Api.Authentication
{
    public class RestVerifier
    {
        private readonly AuthenticationClient client;

        public RestVerifier(string url)
        {
            client = new AuthenticationClient();
            client.BaseUrl = url;
        }

        public async Task<bool> Verify((string username, string password) credentials)
        {
            AuthenticationResponse result = await  client.AuthenticateAsync(credentials.username, credentials.password, "DATA");
            return !string.IsNullOrWhiteSpace(result?.Username);
        }
    }
}
