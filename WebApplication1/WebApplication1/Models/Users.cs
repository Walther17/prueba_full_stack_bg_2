using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(30, MinimumLength = 4)]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$",
            ErrorMessage = "Username solo permite letras, números, . _ - y sin espacios")]
        public string Username { get; set; }

        [Required, StringLength(64, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
            ErrorMessage = "Password debe tener al menos 1 letra y 1 número")]
        public string Password { get; set; }

        [Required]
        public NameDto Name { get; set; }

        [Required]
        public AddressDto Address { get; set; }

        [Required]
        [RegularExpression(@"^[0-9()+ -]{7,20}$")]
        public string Phone { get; set; }
    }

    public class NameDto
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public string Firstname { get; set; }

        [Required, StringLength(50, MinimumLength = 2)]
        public string Lastname { get; set; }
    }

    public class AddressDto
    {
        [Required, StringLength(60, MinimumLength = 2)]
        public string City { get; set; }

        [Required, StringLength(100, MinimumLength = 2)]
        public string Street { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Number { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9 -]{3,12}$")]
        public string Zipcode { get; set; }
    }
}