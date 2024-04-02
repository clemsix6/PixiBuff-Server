namespace PXServer.Source.Exceptions;


public class StartupException : Exception
{
    public StartupException(string message) : base(message)
    {
    }
}