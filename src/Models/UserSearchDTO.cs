namespace OrderLunch.Models
{
    public record UserSearchDTO(
        string staffCode,
        string userName,
        string staffName,
        string email,
        string avatar,
        string phoneNumber);
}
