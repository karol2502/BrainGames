namespace BrainGames.API.Entities;

public sealed class User
{
    public User()
    {
    }
    
    public User(string sub, string nickname, string picture)
    {
        Avatar = picture;
        Nickname = nickname;
        NameIdentifier = sub;
    }

    public string Id { get; } = Guid.NewGuid().ToString();
    public string Avatar { get; init; } = null!;
    public string Nickname { get; init; } = null!;
    public string NameIdentifier { get; init; } = null!;
}
