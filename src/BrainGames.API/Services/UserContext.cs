using System.Security.Claims;
using BrainGames.API.Models;
using BrainGames.API.Persistence;

namespace BrainGames.API.Services;

public interface IUserContext
{
    CurrentUser? GetCurrentUser();
}

public class UserContext(IHttpContextAccessor httpContextAccessor, BrainGamesDbContext dbContext) : IUserContext
{
    public CurrentUser? GetCurrentUser()
    {
        var user = httpContextAccessor?.HttpContext?.User;
        if (user == null)
        {
            throw new InvalidOperationException("User context is not present");
        }

        if (user.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        var nameIdentifier = user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
        var userEntity = dbContext.Users.FirstOrDefault(u => u.NameIdentifier == nameIdentifier)
            ?? throw new InvalidOperationException("User not found");

        return new CurrentUser(userEntity.Id, nameIdentifier);
    }
}