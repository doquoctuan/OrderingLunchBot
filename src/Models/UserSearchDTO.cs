namespace OrderLunch.Models
{
    public record UserSearchDTO(
        string StaffCode,
        string UserName,
        string StaffName,
        string Email,
        string Avatar,
        string PhoneNumber);
}
