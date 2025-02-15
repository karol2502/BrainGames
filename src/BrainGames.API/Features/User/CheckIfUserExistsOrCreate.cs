using BrainGames.API.Models;
using BrainGames.API.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainGames.API.Features.User;

public static class CheckIfUserExistsOrCreate
{
    public class Command(IdToken token) : IRequest
    {
        public IdToken IdToken { get; } = token;
    }
    
    internal sealed class Handler(ILogger<Handler> logger, BrainGamesDbContext dbContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.NameIdentifier == request.IdToken.Sub, cancellationToken: cancellationToken);
            
            if (user is not null)
            {
                logger.LogInformation("User found: {@user}", user);
                return;
            }
            
            var newUser = new Entities.User(request.IdToken.Sub, request.IdToken.Nickname, request.IdToken.Picture);
            logger.LogInformation("Creating new user: {@user}", newUser);
            await dbContext.Users.AddAsync(newUser, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}