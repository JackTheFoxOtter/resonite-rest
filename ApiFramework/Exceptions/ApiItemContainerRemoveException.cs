using ApiFramework.Resources.Items;

namespace ApiFramework.Exceptions
{
    public class ApiItemContainerRemoveException : ApiException
    {
        IApiItemContainer Container { get; }
        IApiItem Item { get; }

        public ApiItemContainerRemoveException(IApiItemContainer container, IApiItem item, string message) : base(500, message)
        {
            Container = container;
            Item = item;
        }

        public override string ToString()
        {
            return $"Failed to remove item {Item} from container {Container}: {Message}";
        }
    }
}
