namespace OrderLunch.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<string> GetAccessTokenFromRefreshTokenAsync(CancellationToken cancellationToken);
    }
}