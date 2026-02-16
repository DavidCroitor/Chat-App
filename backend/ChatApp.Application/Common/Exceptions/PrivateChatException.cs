namespace ChatApp.Application.Common.Exceptions;

public class PrivateChatException : Exception
{
    public PrivateChatException(string message) : base(message) { }
}