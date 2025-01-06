namespace ApiFramework.Exceptions
{
    public class ApiJsonParsingException : ApiException
    {
        public ApiJsonParsingException(string message) : base(500, message) { }

        public override string ToString()
        {
            return $"Error while parsing JSON: {Message}";
        }
    }
}
