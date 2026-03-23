using FluentValidation;
using MES.Core.DTOs;

namespace MES.API.Validators;

public class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number wajib diisi")
            .MaximumLength(50).WithMessage("Order number maksimal 50 karakter");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Nama produk wajib diisi")
            .MaximumLength(100).WithMessage("Nama produk maksimal 100 karakter");

        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Kode produk wajib diisi")
            .MaximumLength(50).WithMessage("Kode produk maksimal 50 karakter");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity harus lebih dari 0");
    }
}