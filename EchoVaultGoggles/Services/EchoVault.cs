using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace EchoVaultGoggles.Services;

public sealed class EchoVault : IDisposable
{
    private const string URL = "https://echovault.gg/api/v1/";
    private readonly HttpClient http;
    private readonly CancellationTokenSource cts = new();
    private readonly JsonSerializerOptions jsonOptions = new() 
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public enum ProfileStatus { EXIST, HIDDEN, NONE };

    public EchoVault()
    {
        this.http = new HttpClient
        {
            BaseAddress = new Uri(URL), 
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    public async Task<ProfileStatus> HasCharacter(uint world, string name, string key)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get, 
            $"characters/{world}/{Uri.EscapeDataString(name)}"
        );

        if (!string.IsNullOrEmpty(key))
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {key}");

        try
        {
            using var response = await http
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var profile = await response.Content
                        .ReadFromJsonAsync<CharacterProfile>(this.jsonOptions, cts.Token)
                        .ConfigureAwait(false);

                    if (profile is null)
                    {
                        return ProfileStatus.NONE;
                    } 
                    else
                    {
                        if (profile.Hidden == true)
                            return ProfileStatus.HIDDEN;

                        return ProfileStatus.EXIST;
                    }

                default:
                    return ProfileStatus.NONE;
            }

        }
        catch (Exception ex)
        {
            return ProfileStatus.NONE;
        }
    }


    public void Dispose() 
    {
        http.Dispose();
    }
}

public class CharacterProfile 
{    
    [JsonPropertyName("hidden")] public bool? Hidden { get; set; }
}
