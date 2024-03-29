﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vodamep.ReportBase
{
    internal class ReportSendClient
    {
        private readonly Uri _address;

        public ReportSendClient(Uri address)
        {
            _address = address;
        }
        public async Task<SendResult> Send(IReport report, string username, string password)
        {
            using (var client = new HttpClient())
            using (var data = report.WriteToStream())
            {
                var url = new Uri(_address, $"{report.ReportType}/{report.FromD.Year}/{report.FromD.Month}");
                var content = new ByteArrayContent(data.ToArray());

                if (!string.IsNullOrEmpty(username))
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }

                var response = await client.PutAsync(url, content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new SendResult() { IsValid = false, ErrorMessage = "Unauthorized" };

                SendResult result = null;

                var responseMsg = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseMsg))
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<SendResult>(responseMsg);
                    }
                    catch (Exception e)
                    {
                        result = new SendResult() { IsValid = false, ErrorMessage = e.Message };
                    }
                }
                else
                {
                    result = new SendResult() { IsValid = false, ErrorMessage = $"{response.StatusCode}: Empty" };
                }

                return result;
            }
        }
    }
}
