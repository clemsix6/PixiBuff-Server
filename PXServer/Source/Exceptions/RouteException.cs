namespace PXServer.Source.Exceptions;


public class RouteException : Exception
{
    public int StatusCode { get; }


    public RouteException(string message, int statusCode) : base(message)
    {
        this.StatusCode = statusCode;
    }
}