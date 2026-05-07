using Domain.Enums;
using FluentValidation;

namespace Application.Features.Bags.Commands.Use;

public class UseBagCommandValidator : AbstractValidator<UseBagCommand>
{
    public UseBagCommandValidator()
    {
        RuleFor(c => c.BagId).NotEmpty();

        RuleFor(c => c.Reason)
            .IsInEnum()
            .WithMessage("Geçerli bir kullanım sebebi seçilmelidir.");

        RuleFor(c => c.Note)
            .NotEmpty()
            .MinimumLength(3)
            .When(c => c.Reason == BagUseReason.Other)
            .WithMessage("Diğer seçildiğinde açıklama (en az 3 karakter) zorunludur.");
    }
}
