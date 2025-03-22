using System.ComponentModel.DataAnnotations;

namespace jekirdekCase.Models;


    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Region { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
