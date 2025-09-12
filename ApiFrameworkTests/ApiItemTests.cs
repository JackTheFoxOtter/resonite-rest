using ApiFramework.Resources;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiFrameworkTests
{
    [TestClass]
    public sealed class ApiItemTests
    {
        [TestMethod]
        public void TestBasicValueItems()
        {
            // Create without initial value
            ApiItemValue<int> testItem1 = new();
            Assert.IsNull(testItem1.Parent);
            Assert.IsNull(testItem1.Value);

            // Set value
            testItem1.Value = 42;
            Assert.AreEqual(42, testItem1.Value);

            // Create with initial value
            ApiItemValue<int> testItem2 = new(null, 1337);
            Assert.IsNull(testItem2.Parent);
            Assert.AreEqual(1337, testItem2.Value);

            // Set value to null
            testItem2.Value = null;
            Assert.IsNull(testItem2.Value);

            // Copy value (42) from item1 to item2
            testItem2.UpdateFrom(testItem1);
            Assert.AreEqual(42, testItem2.Value);

            // Create item3 as a copy of item1
            ApiItemValue<int> testItem3 = (ApiItemValue<int>)testItem1.CreateCopy();
            Assert.AreNotSame(testItem1, testItem3);
            Assert.AreEqual(testItem1.Value, testItem3.Value);
        }

        [TestMethod]
        public void TestBasicObjectItems()
        {
            // Create without initial value
            ApiItemObject<string> testItem1 = new();
            Assert.IsNull(testItem1.Value);

            // Set value
            testItem1.Value = "Hello, world!";
            Assert.AreEqual("Hello, world!", testItem1.Value);

            // Create with initial value
            ApiItemObject<string> testItem2 = new(null, "Bwa!");
            Assert.AreEqual("Bwa!", testItem2.Value);

            // Set value to null
            testItem2.Value = null;
            Assert.IsNull(testItem2.Value);

            // Copy value ("Hello, world!") from item1 to item2
            testItem2.UpdateFrom(testItem1);
            Assert.AreEqual("Hello, world!", testItem2.Value);
            Assert.AreNotSame(testItem1.Value, testItem2.Value);

            // Create item3 as a copy of item1
            ApiItemObject<string> testItem3 = (ApiItemObject<string>)testItem1.CreateCopy();
            Assert.AreNotSame(testItem1, testItem3);
            Assert.AreEqual(testItem1.Value, testItem3.Value);
            Assert.AreNotSame(testItem1.Value, testItem3.Value);
        }

        [TestMethod]
        public void TestBasicListItems()
        {
            // Create new list, should be empty
            ApiItemList testList = new();
            Assert.AreEqual(0, testList.Count());

            // Add item directly
            ApiItemObject<string> value0 = new(null, "First");
            testList.Insert(value0);
            Assert.AreEqual(1, testList.Count());
            Assert.AreEqual(value0, testList[0]);

            // Add new item
            ApiItemObject<string> value1 = testList.InsertNew<ApiItemObject<string>>();
            value1.Value = "Second";
            Assert.AreEqual(2, testList.Count());
            Assert.AreEqual(value1, testList[1]);

            // Add copy of value1
            ApiItemObject<string> value2 = testList.InsertCopy(value1);
            value2.Value = "Third";
            Assert.AreEqual(3, testList.Count());
            Assert.AreEqual(value2, testList[2]);
            Assert.AreNotSame(value1, value2);

            // Remove item at index 1
            testList.RemoveAt(1);
            Assert.AreEqual(2, testList.Count());
            Assert.AreEqual(value0, testList[0]);
            Assert.AreEqual(value2, testList[1]);

            // Remove item 0
            testList.RemoveItem(value0);
            Assert.AreEqual(1, testList.Count());
            Assert.AreEqual(value2, testList[0]);

            // Clear remaining item
            testList.Clear();
            Assert.AreEqual(0, testList.Count());
        }

        [TestMethod]
        public void TestBasicDictItems()
        {
            // Create new dict, should be empty
            ApiItemDict testDict = new();
            Assert.AreEqual(0, testDict.Count());

            // Add item directly
            ApiItemObject<string> value0 = new(null, "First");
            testDict.Insert("one", value0);
            Assert.AreEqual(1, testDict.Count());
            Assert.AreEqual(value0, testDict["one"]);

            // Add new item
            ApiItemObject<string> value1 = testDict.InsertNew<ApiItemObject<string>>("two");
            value1.Value = "Second";
            Assert.AreEqual(2, testDict.Count());
            Assert.AreEqual(value1, testDict["two"]);

            // Add copy of value1
            ApiItemObject<string> value2 = testDict.InsertCopy("three", value1);
            Assert.AreEqual(3, testDict.Count());
            Assert.AreEqual(value2, testDict["three"]);
            Assert.AreNotEqual(value1, value2);

            // Remove item with key "two"
            testDict.Remove("two");
            Assert.AreEqual(2, testDict.Count());
            Assert.AreEqual(value0, testDict["one"]);
            Assert.AreEqual(value2, testDict["three"]);

            // Remove item 0
            testDict.RemoveItem(value0);
            Assert.AreEqual(1, testDict.Count());
            Assert.AreEqual(value2, testDict["three"]);

            // Clear remaining item
            testDict.Clear();
            Assert.AreEqual(0, testDict.Count());
        }

        [TestMethod]
        public void TestItemContainers()
        {
            // Create lists & item
            ApiItemList list1 = new();
            ApiItemList list2 = new();
            ApiItemValue<int> item = new(null, 42);
            Assert.IsFalse(list1.Contains(item));
            Assert.IsFalse(list2.Contains(item));
            Assert.IsNull(item.Parent);

            // Parent item to list1
            list1.Insert(item);
            Assert.IsTrue(list1.Contains(item));
            Assert.IsFalse(list2.Contains(item));
            Assert.AreEqual(list1, item.Parent);

            // Parent item to list2
            list2.Insert(item); // Should automatically remove from list1
            Assert.IsFalse(list1.Contains(item));
            Assert.IsTrue(list2.Contains(item));
            Assert.AreEqual(list2, item.Parent);

            // Unparent
            item.SetParent(null);
            Assert.IsFalse(list1.Contains(item));
            Assert.IsFalse(list2.Contains(item));
            Assert.IsNull(item.Parent);
        }

        [TestMethod]
        public void TestItemJSON()
        {
            // Simple items from object to JSON
            ApiItemObject<string> itemObject = new(null, "Example");
            ApiItemValue<int> itemValue = new(null, 42);
            ApiItemList itemList = new();
            ApiItemDict itemDict = new();
            Assert_AreJsonEqual(JsonSerializer.SerializeToNode("Example"), itemObject.ToJson());
            Assert_AreJsonEqual(JsonSerializer.SerializeToNode(42), itemValue.ToJson());
            Assert_AreJsonEqual(JsonSerializer.SerializeToNode(new object[] { }), itemList.ToJson());
            Assert_AreJsonEqual(JsonSerializer.SerializeToNode(new Dictionary<object, object>()), itemDict.ToJson());

            // Simple JSON roundtrips
            string jsonItemObject = @"""Bwa!""";
            string jsonItemValue = @"42.0";
            string jsonItemList = @"[""Test"", 42.0, false]";
            string jsonItemDict = @"{""a"": ""Test"", ""b"": 42.0, ""c"": false}";
            IApiItem itemParsedObject = ApiItem.FromJson(null, jsonItemObject);
            IApiItem itemParsedValue = ApiItem.FromJson(null, jsonItemValue);
            IApiItem itemParsedList = ApiItem.FromJson(null, jsonItemList);
            IApiItem itemParsedDict = ApiItem.FromJson(null, jsonItemDict);
            Assert_AreJsonStringEqual(jsonItemObject, itemParsedObject.ToJsonString());
            Assert_AreJsonStringEqual(jsonItemValue, itemParsedValue.ToJsonString());
            Assert_AreJsonStringEqual(jsonItemList, itemParsedList.ToJsonString());
            Assert_AreJsonStringEqual(jsonItemDict, itemParsedDict.ToJsonString());

            // Complex JSON roundtrip
            string jsonComplex = """
            {
               "title": "Example JSON Object",
               "numInt": 42,
               "numFloat": 3.14,
               "entries":
               [
                   { "index": 0, "content": "Hello, world!", "tags": [ "A", "B", "C" ] },
                   { "index": 1, "content": "This is a test", "tags": [ "A", "C" ] },
                   { "index": 2, "content": "to verify JSON works.", "tags": [ "B", "C" ] }
               ]
            }
            """;
            IApiItem itemParsedComplex = ApiItem.FromJson(null, jsonComplex);
            Assert_AreJsonStringEqual(jsonComplex, itemParsedComplex.ToJsonString());
        }

        private static void Assert_AreJsonEqual(JsonNode? a, JsonNode? b)
        {
            Assert.IsTrue(JsonNode.DeepEquals(a, b));
        }

        private static void Assert_AreJsonStringEqual(string a, string b)
        {
            JsonNode? jsonA = JsonSerializer.Deserialize<JsonNode?>(a);
            JsonNode? jsonB = JsonSerializer.Deserialize<JsonNode?>(b);
            Assert.IsTrue(JsonNode.DeepEquals(jsonA, jsonB));
        }
    }
}
