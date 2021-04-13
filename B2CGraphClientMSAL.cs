using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace B2CGraphShell
{
    public class B2CGraphClientMSAL
    {
        private readonly string tenant; 
        
        readonly IConfidentialClientApplication app;

        public B2CGraphClientMSAL(string clientId, string clientSecret, string tenant, string authority)
        {
            // The client_id, client_secret, tenant and authority are pulled in from the app.config file

            this.tenant = tenant;
            
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Using tenant " + tenant);
            Console.WriteLine("");

            app = ConfidentialClientApplicationBuilder.Create(clientId)
           .WithClientSecret(clientSecret)
           .WithAuthority(new Uri(authority))
           .Build();
        }

        public async Task<string> GetUserByObjectId(string objectId)
        {
            return await SendGraphGetRequest("/users/" + objectId, null);
        }

        public async Task<string> GetAllUsers(string query)
        {
            return await SendGraphGetRequest("/users", query);
        }

        public async Task<string> CreateUser(string json)
        {
            return await SendGraphPostRequest("/users", json);
        }

        public async Task<string> UpdateUser(string objectId, string json)
        {
            return await SendGraphPatchRequest("/users/" + objectId, json);
        }

        public async Task<string> DeleteUser(string objectId)
        {
            return await SendGraphDeleteRequest("/users/" + objectId);
        }

        public async Task<string> RegisterExtension(string objectId, string body)
        {
            return await SendGraphPostRequest("/applications/" + objectId + "/extensionProperties", body);
        }

        public async Task<string> UnregisterExtension(string appObjectId, string extensionObjectId)
        {
            return await SendGraphDeleteRequest("/applications/" + appObjectId + "/extensionProperties/" + extensionObjectId);
        }

        public async Task<string> GetExtensions(string appObjectId)
        {
            return await SendGraphGetRequest("/applications/" + appObjectId + "/extensionProperties", null);
        }

        public async Task<string> GetApplications(string query)
        {
            return await SendGraphGetRequest("/applications", query);
        }

        private async Task<string> SendGraphDeleteRequest(string api)
        {
            // With client credentials flows, the scope is always of the shape "resource/.default" because the
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator.

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            AuthenticationResult result;

            try
            {
                result = await app.AcquireTokenForClient(scopes)
                                 .ExecuteAsync();
            }

            catch (MsalUiRequiredException ex)
            {
                // The application doesn't have sufficient permissions.
                // - Did you declare enough app permissions during app creation?
                // - Did the tenant admin grant permissions to the application?

                throw new Exception("MsalUiRequiredException on SendGraphDeleteRequest " + ex);
            }

            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be in the form "https://resourceurl/.default"
                // Mitigation: Change the scope to be as expected.

                throw new Exception("MsalUiRequiredException on SendGraphDeleteRequest  " + ex);
            }

            catch (Exception ex)
            {
                throw new Exception("Exception on SendGraphDeleteRequest " + ex);
            }

            HttpClient http = new HttpClient();
            string url = Globals.msGraphEndpoint + tenant + api;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DELETE " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("");

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("DELETE"), url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync();

        }

        private async Task<string> SendGraphPatchRequest(string api, string json)
        {
            // With client credentials flows, the scope is always of the shape "resource/.default" because the
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator.

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            AuthenticationResult result;

            try
            {
                result = await app.AcquireTokenForClient(scopes)
                                 .ExecuteAsync();
            }

            catch (MsalUiRequiredException ex)
            {
                // The application doesn't have sufficient permissions.
                // - Did you declare enough app permissions during app creation?
                // - Did the tenant admin grant permissions to the application?

                throw new Exception("MsalUiRequiredException on SendGraphPatchRequest " + ex);
            }

            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be in the form "https://resourceurl/.default"
                // Mitigation: Change the scope to be as expected.

                throw new Exception("MsalServiceException on SendGraphPatchRequest " + ex);
            }

            catch (Exception ex)
            {
                throw new Exception("Exception on SendGraphPatchRequest " + ex);
            }

            HttpClient http = new HttpClient();
            string url = Globals.msGraphEndpoint + tenant + api;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("PATCH " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("Content-Type: application/json");
            Console.WriteLine("");
            Console.WriteLine(json);
            Console.WriteLine("");

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync();

        }

        private async Task<string> SendGraphPostRequest(string api, string json)
        {
            // With client credentials flows, the scope is always of the shape "resource/.default" because the
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            AuthenticationResult result;

            try
            {
                result = await app.AcquireTokenForClient(scopes)
                                 .ExecuteAsync();
            }

            catch (MsalUiRequiredException ex)
            {
                // The application doesn't have sufficient permissions.
                // - Did you declare enough app permissions during app creation?
                // - Did the tenant admin grant permissions to the application?

                throw new Exception("MsalUiRequiredException on SendGraphPostRequest " + ex);
            }

            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be in the form "https://resourceurl/.default"
                // Mitigation: Change the scope to be as expected.

                throw new Exception("MsalServiceException on SendGraphPostRequest " + ex);
            }

            catch (Exception ex)
            {
                throw new Exception("Exception on SendGraphPostRequest " + ex);
            }

            HttpClient http = new HttpClient();
            string url = Globals.msGraphEndpoint + tenant + api;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("POST " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("Content-Type: application/json");
            Console.WriteLine("");
            Console.WriteLine(json);
            Console.WriteLine("");

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("POST"), url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SendGraphGetRequest(string api, string query)
        {

            // With client credentials flows, the scope is always of the shape "resource/.default" because the
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator.

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            AuthenticationResult result;

            try
            {
                result = await app.AcquireTokenForClient(scopes)
                                 .ExecuteAsync();
            }

            catch (MsalUiRequiredException ex)
            {
                // The application doesn't have sufficient permissions.
                // - Did you declare enough app permissions during app creation?
                // - Did the tenant admin grant permissions to the application?

                throw new Exception("MsalUiRequiredException on SendGraphGetRequest " + ex);
            }

            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be in the form "https://resourceurl/.default"
                // Mitigation: Change the scope to be as expected.

                throw new Exception("MsalServiceException on SendGraphGetRequest " + ex);
            }

            catch (Exception ex)
            {
                throw new Exception("Exception on SendGraphGetRequest " + ex);
            }

            HttpClient httpClient = new HttpClient();
            string url = Globals.msGraphEndpoint + tenant + api;

            if (!string.IsNullOrEmpty(query))
                if (query.Contains("filter"))
                    url += "?" + query;
                else
                    url += "/" + query;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("GET " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the web API

            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            string returnResult = await response.Content.ReadAsStringAsync();

            return returnResult;
        }
    }

}
