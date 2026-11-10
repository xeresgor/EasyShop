namespace EasyShop.Services;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
