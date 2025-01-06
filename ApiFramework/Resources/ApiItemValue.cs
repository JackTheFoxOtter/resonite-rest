using ApiFramework.Exceptions;
using ApiFramework.Interfaces;
using Newtonsoft.Json.Linq;

namespace ApiFramework.Resources
{
    public class ApiItemValue<T> : ApiItem
    {
        private T? _value;

        public ApiItemValue(IApiItemContainer parent, bool canEdit, T? value) : base(parent, canEdit)
        {
            _value = value;
        }

        public T? Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!CanEdit()) throw new ApiResourceItemReadOnlyException(ToString());
                _value = value;
            }
        }

        public override JToken ToJsonRepresentation()
        {
            return new JValue(Value);
        }
    }
}
