using System.Security.Claims;
namespace FirstReactBackend;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
{
    public int Id => int.Parse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
