namespace SessionPlanner.Web.Services;

public class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly IAuthService _auth;

    public AuthenticatedHttpHandler(IAuthService auth) => _auth = auth;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _auth.RefreshTokenAsync();
            if (refreshed)
            {
                token = await _auth.GetTokenAsync();
                var retry = new HttpRequestMessage(request.Method, request.RequestUri);
                foreach (var h in request.Headers)
                    retry.Headers.TryAddWithoutValidation(h.Key, h.Value);
                if (request.Content != null)
                    retry.Content = new StringContent(
                        await request.Content.ReadAsStringAsync(cancellationToken),
                        System.Text.Encoding.UTF8,
                        request.Content.Headers.ContentType?.MediaType ?? "application/json");
                retry.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await base.SendAsync(retry, cancellationToken);
            }
        }

        return response;
    }
}
