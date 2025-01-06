using SkyFrost.Base;

namespace ResoniteApi.Resources
{
    internal class ContactResource : SkyFrostApiResourceWrapper<Contact>
    {
        private static string[] _editableItems =
        {
            "id",
            "contactUsername",
            "contactStatus"
        };

        public ContactResource(Contact skyFrostContact) : base(skyFrostContact) { }

        public override bool CanEditItemCheck(string[] itemPath)
        {
            return _editableItems.Contains(string.Join(".", itemPath));
        }
    }
}
