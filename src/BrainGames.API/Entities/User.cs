namespace BrainGames.API.Entities;

public sealed class User
{
    public User(string sub, string nickname, string picture)
    {
        Avatar = picture;
        Nickname = nickname;
        NameIdentifier = sub;
    }
    
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Avatar { get; set; } 
    public string Nickname { get; set; } 
    public string NameIdentifier { get; set; } 
}
