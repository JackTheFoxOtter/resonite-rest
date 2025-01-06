using SkyFrost.Base;

namespace ResoniteApi.Resources
{
    internal class UserResource : SkyFrostApiResourceWrapper<User>
    {
        public UserResource(User skyFrostUser) : base(skyFrostUser) { }

        public override bool CanEditItemCheck(string[] itemPath)
        {
            return false;
        }
    }
}
