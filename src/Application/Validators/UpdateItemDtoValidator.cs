using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UpdateItemDtoValidator : AbstractValidator<UpdateItemDto>
{
    public UpdateItemDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative.");
    }
}
