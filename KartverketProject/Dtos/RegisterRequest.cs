using System.ComponentModel.DataAnnotations;

namespace KartverketProject.Dtos
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Please set a username")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Please set an email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please set a password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
