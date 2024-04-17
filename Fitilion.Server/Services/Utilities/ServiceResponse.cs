using System.Net;

namespace Fitilion.Server.Services.Utilities
{
    public class ServiceResponse<T>
    {
        public T Data { get; }
        public HttpStatusCode StatusCode { get; }

        public ServiceResponse(T data, HttpStatusCode statusCode)
        {
            Data = data;
            StatusCode = statusCode;
        }
    }
}
