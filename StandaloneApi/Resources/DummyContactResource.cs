using ApiFramework.Resources;

namespace ResoniteApi.Resources
{
    internal class DummyContactResource : ApiResource
    {

        public DummyContactResource(string id, string name, int relation) : base()
        {
            ApiItemDict rootItem = (ApiItemDict) RootItem;
            rootItem.InsertValue("ContactId", id, false);
            rootItem.InsertValue("ContactName", name, false);
            rootItem.InsertValue("ContactRelation", relation, true);
        }

        public override string GetResourceName()
        {
            return "DummyContactResource";
        }
    }
}
