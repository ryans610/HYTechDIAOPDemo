﻿using System.Net.Http;

namespace DemoBuyingProduct;

public class UserProxy : IUsers
{
    public async Task<bool> IsUserValidAsync(Guid userId)
    {
        // check is valid user
        var userHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://my-users"),
        };
        var userResponse = await userHttpClient.PostAsJsonAsync(
            "api/isUserValid",
            new { userId });
        userResponse.EnsureSuccessStatusCode();
        var isUserValid = await userResponse.Content.ReadAsAsync<bool>();
        return isUserValid;
    }
}
