using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AblApi.Common.Extensions;
using AblApi.Core.AppJwtToken.Dtos;

namespace AblApi.Api.DebugUtil;

public class DebugHttpClient : HttpClient
{
    // used for local debugging, id / token needs to be updated everytime the SqLite DB is recreated
    private readonly string BaseUrl = "http://localhost:44331/";
    private readonly string UserId = "4f35c3ed-4c12-4f73-a417-b1334d03fc01";
    private readonly string Token = "840B97F4621D15C32411EE197222663A60A001710A7F3487DA926AF7129F17FF";

    public DebugHttpClient()
    {
        BaseAddress = new Uri(BaseUrl);

        // request Jwt token
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}auth");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserId}:{Token}"))
        );
        var response = new HttpClient().Send(request);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Invalid Userid / Token");
        }
        var responseDto = JsonSerializer.Deserialize<JwtTokenDto>(response.Content.ReadAsString()) ?? throw new InvalidOperationException("Received invalid response");

        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseDto.Token);
    }    
}
