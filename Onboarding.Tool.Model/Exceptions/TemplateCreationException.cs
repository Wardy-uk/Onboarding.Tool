namespace Onboarding.Tool.Model.Exceptions;

public class TemplateCreationException : Exception
{
    public TemplateCreationException(string message, Exception innerException) : base(message, innerException) { }
}