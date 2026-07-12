using FluentValidation;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;

namespace FaithFlow.Backend.DTOs.Validators;

public class RequestCreateDtoValidator : AbstractValidator<RequestCreateDto>
{
    public RequestCreateDtoValidator(IRequestTypeRepository requestTypeRepository)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot be longer than 200 characters");

        RuleFor(x => x.Content)
            .MaximumLength(2000).WithMessage("Content cannot be longer than 2000 characters");

        RuleFor(x => x.RequestTypeId)
            .GreaterThan(0).WithMessage("Request type is required")
            .MustAsync(async (id, cancellation) => await requestTypeRepository.ExistsAsync(id))
            .WithMessage("Invalid request type");

        RuleForEach(x => x.GroupIds)
            .GreaterThan(0).WithMessage("Invalid group");
    }
}
