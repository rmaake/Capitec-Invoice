using System.ComponentModel.DataAnnotations;

namespace Capitec.Api.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OptionalPhoneAttribute : ValidationAttribute
    {
        private readonly PhoneAttribute validator = new();

        public override bool IsValid(object? value) =>
            value is null || (value is string text && (string.IsNullOrWhiteSpace(text) || validator.IsValid(text)));
    }
}
