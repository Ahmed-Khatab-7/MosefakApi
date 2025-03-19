using MosefakApp.Core.Dtos.ChatBot.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosefakApp.Core.Dtos.ChatBot.Validators;
public class ChatRequestValidator : AbstractValidator<ChatRequestDto>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MinimumLength(3).WithMessage("Question must be at least 3 characters.")
            .MaximumLength(300).WithMessage("Question cannot exceed 300 characters.");
    }
}
