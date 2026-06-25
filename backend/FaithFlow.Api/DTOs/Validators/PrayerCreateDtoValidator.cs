using FluentValidation;
using FaithFlow.Backend.DTOs;

namespace FaithFlow.Backend.DTOs.Validators
{
    public class PrayerCreateDtoValidator : AbstractValidator<PrayerCreateDto>
    {
        public PrayerCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot be longer than 200 characters");

            RuleFor(x => x.Content)
                .MaximumLength(2000).WithMessage("Content cannot be longer than 2000 characters");
        }
    }
}