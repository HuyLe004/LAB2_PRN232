using FluentValidation;

namespace PRN232.LMS.Services.Validations
{
    /// <summary>
    /// Abstract base validator class for all validators in the system
    /// Provides a common base for implementing FluentValidation validators
    /// LAB2 Requirement #6: Data Validation - FluentValidation Implementation
    /// </summary>
    public abstract class BaseValidator<T> : AbstractValidator<T> where T : class
    {
    }
}
