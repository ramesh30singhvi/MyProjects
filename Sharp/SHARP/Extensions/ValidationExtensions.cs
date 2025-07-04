using FluentValidation;
using SHARP.Constants;

namespace SHARP.Extensions
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> IsEmailAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(ValidationRegularExpressions.EMAIL);
        }
    }
}
