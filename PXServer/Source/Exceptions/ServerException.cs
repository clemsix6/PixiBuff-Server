namespace PXServer.Source.Exceptions;


public class ServerException : Exception
{
    public int StatusCode { get; }


    public ServerException(string message, int statusCode) : base(message)
    {
        this.StatusCode = statusCode;
    }
}