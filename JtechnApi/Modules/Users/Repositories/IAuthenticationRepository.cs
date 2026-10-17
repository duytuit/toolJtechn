using System.Security.Claims;

namespace JtechnApi.Users.Repositories
{
    public interface IAuthenticationRepository
    {
        Task<List<Claim>> GetLoginClaims(string username);
    }
}
