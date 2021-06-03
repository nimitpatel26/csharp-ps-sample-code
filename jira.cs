using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;


public class JiraCreator
{
    private string _userName = "place jira username";
    private string _token = "place jira token";
    private string _jiraApi = "place jira api";
    private HttpClient _httpClient = new HttpClient();
    private string _base64Auth;

    public JiraCreator()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        _base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(_userName + ":" + _token));

    }

    public JToken GetItmToSend()
    {
        return new JObject()
        {
            {"fields", new JObject() {
                {"project", "place project name here" },
                {"issuetype", "place issue type" },
                {"priority", "place priority here" },
                {"components", "place components here" },
                {"summary", "place ticket summary here" },
                {"description", "place ticket description here" },
                } }

        };
    }


    public string CreateTicket(JiraObj jiraObj)
    {
        string url = _jiraApi + "rest/api/2/issue/";
        string data = GetItmToSend().ToString();

        string strResp = String.Empty;


        try
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
            {
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", $"Basic {_base64Auth}");
                request.Content = new StringContent(data, Encoding.UTF8, "application/json");

                var httpResp = _httpClient.SendAsync(request).Result;
                strResp = httpResp.Content.ReadAsStringAsync().Result;
                httpResp.EnsureSuccessStatusCode();
            }

        }
        catch (Exception ex)
        {

            // failed to create jira ticket
            
        }

        // strResp should have ticket number if successfull

        return strResp;

    }








}
