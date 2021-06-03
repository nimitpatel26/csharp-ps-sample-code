using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;


public class HttpMsgHelper
{

    /// <summary>
    /// Returns a OK HttpResponseMessage with the given string data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>HttpResponseMessage</returns>
    public HttpResponseMessage Ok(string data = "Process was executed successfully.")
    {
        return new HttpResponseMessage
        {
            Content = new StringContent(data),
            StatusCode = HttpStatusCode.OK,
        };

    }

    /// <summary>
    /// Returns a OK HttpResponseMessage withe the object serialized as JSON
    /// </summary>
    /// <param name="data"></param>
    /// <returns>HttpResponseMessage</returns>
    public HttpResponseMessage Ok(Object data)
    {
        HttpResponseMessage response = new HttpResponseMessage
        {
            Content = new StringContent(JsonConvert.SerializeObject(data)),
            StatusCode = HttpStatusCode.OK,
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return response;

    }


    /// <summary>
    /// Returns an error HttpResponseMessage with the given string as content
    /// </summary>
    /// <param name="data"></param>
    /// <returns>HttpResponseMessage</returns>
    public HttpResponseMessage InternalError(string data = "An internal error occured.")
    {
        return new HttpResponseMessage
        {
            Content = new StringContent(data),
            StatusCode = HttpStatusCode.InternalServerError,
        };

    }

}
