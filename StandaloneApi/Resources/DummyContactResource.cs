using ApiFramework.Resources;

namespace ResoniteApi.Resources
{
    internal class DummyContactResource : ApiResource
    {
        public DummyContactResource(string id, string name, int relation) : base()
        {
            AddItem("ContactId", id, true);
            AddItem("ContactName", name, true);
            AddItem("ContactRelation", relation);
        }
    }
}
