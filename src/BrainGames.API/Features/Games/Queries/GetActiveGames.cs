using MediatR;

namespace BrainGames.API.Features.Games.Queries;

public static class GetActiveGames
{
    public class Query : IRequest<List<string>>;
    
    internal sealed class Handler : IRequestHandler<Query, List<string>>
    {
        public Task<List<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(GameFactory.GameNames);
        }
    }
}