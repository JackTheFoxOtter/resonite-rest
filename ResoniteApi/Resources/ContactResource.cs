using SkyFrost.Base;

namespace ResoniteApi.Resources
{
    internal class ContactResource : SkyFrostApiResourceWrapper<Contact>
    {
        public ContactResource(Contact contact) : base(contact) { }
    }
}
