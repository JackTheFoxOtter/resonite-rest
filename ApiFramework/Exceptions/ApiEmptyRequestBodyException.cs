namespace ApiFramework.Exceptions
{
    public class ApiMissingRequestBodyException : ApiException
    {
        public ApiMissingRequestBodyException() : base(400) { }

        public override string ToString()
        {
            return "Request body cannot be empty.";
        }
    }
}
