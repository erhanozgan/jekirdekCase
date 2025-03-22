using FluentValidation;
using jekirdekCase.Models;

namespace jekirdekCase.Controllers;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().WithMessage("İsim boş olamaz!");
        RuleFor(c => c.LastName).NotEmpty().WithMessage("Soyisim boş olamaz!");
        RuleFor(c => c.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta girin!");
        RuleFor(c => c.Region).NotEmpty().WithMessage("Bölge boş olamaz!");
    }
}