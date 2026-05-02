using Application.Authentication;
using Application.Exception;

namespace Api.Authentication;

public class HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public CurrentUser GetCurrentUser()
    {
        return httpContextAccessor.HttpContext?.Items[nameof(CurrentUser)] as CurrentUser
            ?? throw new WrongAuthTokenException("No authentication context");
    }
}
