using ApiFramework;
using SkyFrost.Base;

namespace ResoniteApi.Resources.Users
{
    internal class UserResourceEnumerable : ApiResourceEnumerable<UserResource>
    {
        private IEnumerator<UserResource> _enumerator;

        protected override IEnumerator<UserResource> Enumerator
        {
            get { return _enumerator; }
            set { _enumerator = value; }
        }

        public UserResourceEnumerable(IEnumerable<UserResource> Users) : base(Users) { }

        public UserResourceEnumerable(IEnumerable<User> skyFrostUsers) : base(from skyFrostUser in skyFrostUsers select new UserResource(skyFrostUser)) { }
    }
}
