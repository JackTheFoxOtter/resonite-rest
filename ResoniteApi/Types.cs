using FrooxEngine;
using Newtonsoft.Json;
using SkyFrost.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ResoniteApi
{
    internal class ApiItem
    {
        private readonly string _itemName;
        private readonly bool _itemReadOnly;
        private object _itemValue;

        public ApiItem(string name, object value, bool readOnly)
        {
            _itemName = name;
            _itemValue = value;
            _itemReadOnly = readOnly;
        }

        public string Name => _itemName;
        public bool ReadOnly => _itemReadOnly;
        public object Value
        {
            get 
            { 
                return _itemValue; 
            }
            set
            {
                if (_itemReadOnly) throw new ApiValueReadOnlyException(_itemName);
                _itemValue = value;
            }
        }
    }

    internal abstract class ApiResource
    {
        private readonly Dictionary<string, ApiItem> _items;

        public ApiResource()
        {
            _items = new Dictionary<string, ApiItem>();
        }

        protected void AddItem(string name, object item)
        {
            AddItem(name, item, false);
        }

        protected void AddItem(string name, object item, bool readOnly)
        {
            _items.Add(name, new ApiItem(name, item, readOnly));
        }

        public ApiItem this[string name]
        {
            get
            {
                return _items[name];
            }
        }

        public Dictionary<string, object> GetJsonRepresentation()
        {
            return _items.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
        }
    }

    internal abstract class ApiResourceEnumerable<T> : IEnumerable<T> where T : ApiResource
    {
        protected abstract IEnumerator<T> Enumerator { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator;
        }

        public IEnumerable<Dictionary<string, object>> GetJsonRepresentation()
        {
            return from resource in this select resource.GetJsonRepresentation();
        }
    }

    internal class ContactResource : ApiResource
    {
        public ContactResource(Contact contact) : base()
        {
            AddItem("ContactUserId", contact.ContactUserId, true);
            AddItem("ContactUserName", contact.ContactUsername, true);
            AddItem("ContactStatus", contact.ContactStatus);
        }
    }

    internal class ContactResourceEnumerable : ApiResourceEnumerable<ContactResource>
    {
        private IEnumerator<ContactResource> _enumerator;

        public ContactResourceEnumerable(IEnumerable<Contact> contacts)
        {
            _enumerator = (from contact in contacts select new ContactResource(contact)).GetEnumerator();
        }
        protected override IEnumerator<ContactResource> Enumerator => _enumerator;
    }
}
