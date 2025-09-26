using System;
using ApiFramework.Enums;
using ApiFramework.Resources.Items;
using ApiFramework.Resources.Properties;

namespace ApiPropertyTests
{
    [TestClass]
    public sealed class ApiPropertyTests
    {
        [TestMethod]
        public void TestBasicProperties()
        {
            ApiPropertyPath path1 = new("Prop1");
            ApiItemValue<int> item1 = new(null, 42);
            ApiProperty prop1 = new(path1, item1, EditPermission.CreateModifyDelete);
            
        }
    }
}
