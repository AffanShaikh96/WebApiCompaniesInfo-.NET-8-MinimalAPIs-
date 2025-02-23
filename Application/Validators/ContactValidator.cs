using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ContactValidator : AbstractValidator<Contact>
    {
        public ContactValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Contact name is required.")
                .MaximumLength(50).WithMessage("Contact name cannot exceed 50 characters.");

            RuleFor(x => x.CompanyId)
                .GreaterThanOrEqualTo(1).WithMessage("CompanyId is required.");


            RuleFor(x => x.CountryId)
                .GreaterThanOrEqualTo(1).WithMessage("CountryId is required.");
        }
    }
}
