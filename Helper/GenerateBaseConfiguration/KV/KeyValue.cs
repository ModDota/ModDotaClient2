using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateBaseConfiguration.KV
{
    /// <summary>
    /// Taken from https://github.com/RoyAwesome/KVLib because nuget was out of date
    /// and then commented more fully.
    /// 
    /// Representation of Valve's Key Value format
    /// 
    /// A KeyValue contains a string key, and either a value or a list of children. 
    /// 
    /// Example Key-Value tree:
    /// 
    /// "Example"
    /// {
    ///     "ExampleParent"
    ///     {
    ///         "ExampleValue1" "A String"
    ///         "ExampleValueInt" "12"
    ///     }
    ///     
    ///     "ExampleValue" "3.14 12 15" 
    /// }
    /// </summary>
    public class KeyValue : ICloneable
    {
        /// <summary>
        /// The key of the KV Object.  Read-only
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// List of children leafs
        /// </summary>
        public List<KeyValue> Children
        {
            get
            {
                if (children == null) return new List<KeyValue>();
                else return children;
            }
            internal set
            {
                children = value.ToList();
            }
        }

        /// <summary>
        /// True if the object has children leafs. 
        /// </summary>
        public bool HasChildren
        {
            get
            {
                return children != null;
            }
        }

        /// <summary>
        /// The parent node of this KeyValue; null if no parent
        /// </summary>
        public KeyValue Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// The internal storage for the value.
        /// </summary>
        private string _value;

        /// <summary>
        /// The value of the KeyValue node
        /// </summary>
        internal string Value
        {
            get
            {
                return GetString();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// The child nodes of this keyvalue node; null if no children
        /// </summary>
        List<KeyValue> children = null;

        /// <summary>
        /// Access the KeyValue tree by key
        /// </summary>
        /// <param name="key">The Key of the child to access</param>
        /// <returns>The KeyValue matching the request, or null if not found or no children.</returns>
        public KeyValue this[string key]
        {
            get
            {
                if (children == null) return null;
                return children.FirstOrDefault(x => x.Key == key);
            }
            set
            {
                AddChild(value);
            }
        }

        /// <summary>
        /// Key Value Constructor
        /// </summary>
        /// <param name="key">The key of the Key-Value pair</param>
        public KeyValue(string key)
        {
            Key = key;

        }

        /// <summary>
        /// Construct a KeyValue with a set of children
        /// </summary>
        /// <param name="key">The key of the Key-Value pair</param>
        /// <param name="children">The set of children of this KeyValue node</param>
        internal KeyValue(string key, IEnumerable<KeyValue> children)
            : this(key)
        {
            AddChildren(children);
        }

        #region Getters
        /// <summary>
        /// Try to get an int.
        /// </summary>
        /// <param name="value">The value to write to</param>
        /// <returns>True on success, False on failure</returns>
        public bool TryGet(out int value)
        {
            return int.TryParse(_value, out value);
        }
        /// <summary>
        /// Try to get a float.
        /// </summary>
        /// <param name="value">The value to write to</param>
        /// <returns>True on success, False on failure</returns>
        public bool TryGet(out float value)
        {
            return float.TryParse(_value, out value);
        }
        /// <summary>
        /// Try to get a bool.
        /// </summary>
        /// <param name="value">The value to write to</param>
        /// <returns>True on success, False on failure</returns>
        public bool TryGet(out bool value)
        {
            value = default(bool);
            int a;
            if (!int.TryParse(_value, out a)) return false;
            value = (a != 0);
            return true;

        }
        /// <summary>
        /// Get the integer value of the node.
        /// </summary>
        /// <returns>The integer value of the node, or 0 if fails.</returns>
        public int GetInt()
        {
            int v;
            bool success = int.TryParse(_value, out v);
            return success ? v : 0;
        }
        /// <summary>
        /// Get the float value of the node.
        /// </summary>
        /// <returns>The float value of the node, or 0 if fails.</returns>
        public float GetFloat()
        {
            float v;
            bool success = float.TryParse(_value, out v);
            return success ? v : 0;
        }
        /// <summary>
        /// Get the bool value of the node.
        /// </summary>
        /// <returns>The bool value of the node, or 0 if fails.</returns>
        public bool GetBool()
        {
            bool v;
            bool success = TryGet(out v);
            return success ? v : false;
        }
        /// <summary>
        /// Get the string value of the node.
        /// </summary>
        /// <returns>The string value of the node, or null if no value.</returns>
        public string GetString()
        {
            return _value;
        }
        #endregion

        #region Setters
        /// <summary>
        /// Set the value of the Key-Value leaf
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The KeyValue object</returns>
        public KeyValue Set(int value)
        {
            children = null;
            _value = value.ToString();

            return this;
        }
        /// <summary>
        /// Set the value of the Key-Value leaf
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The KeyValue object</returns>
        public KeyValue Set(float value)
        {
            children = null;
            _value = value.ToString();

            return this;
        }
        /// <summary>
        /// Set the value of the Key-Value leaf
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The KeyValue object</returns>
        public KeyValue Set(string value)
        {

#if CHECKED
            if(value == null) throw new ArgumentNullException("value");     
#endif
            value = value ?? "";
            children = null;
            _value = value;

            return this;
        }
        /// <summary>
        /// Set the value of the Key-Value leaf
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The KeyValue object</returns>
        public KeyValue Set(bool value)
        {
            children = null;
            _value = value ? "1" : "0";

            return this;
        }

        /// <summary>
        /// Add a single child to the list of children
        /// </summary>
        /// <param name="value">The child node to be added.</param>
        public void AddChild(KeyValue value)
        {
            if (children == null)
            {
                _value = "";
                children = new List<KeyValue>();
            }
            value.Parent = this;
            children.Add(value);
        }

        /// <summary>
        /// Add a collection of KeyValue objects as children
        /// </summary>
        /// <param name="KVList">A collection of KeyValues to be added as children.</param>
        public void AddChildren(IEnumerable<KeyValue> KVList)
        {
            if (children == null)
            {
                _value = "";
                children = KVList.ToList();
                foreach (KeyValue kv in children)
                {
                    kv.Parent = this;
                }
                return;
            }
            else
            {
                children = children.Select(x => { x.Parent = this; return x; }).Union(KVList).ToList();
            }
        }
        /// <summary>
        /// Remove a node from the list of children.
        /// </summary>
        /// <param name="child">The child node to be removed.</param>
        public void RemoveChild(KeyValue child)
        {
            if (children == null) return;
            if (!children.Contains(child)) return;
            children.Remove(child);
            // Protect people who do othernode.AddChild(foo); thisnode.RemoveChild(foo);
            if(child.Parent == this)
                child.Parent = null;
        }
        #endregion

        /// <summary>
        /// Adds two Key-Value objects together.  The left hand KVObject becomes a child of the right hand object
        /// </summary>
        /// <param name="rhs">The new child node.</param>
        /// <param name="lhs">The new parent node.</param>
        /// <returns>The parent node.</returns>
        public static KeyValue operator +(KeyValue rhs, KeyValue lhs)
        {
            rhs.AddChild(lhs);
            return rhs;
        }
        /// <summary>
        /// Sets the value of the right hand kv object to be the left hand int
        /// </summary>
        /// <param name="rhs">The KeyValue object to have its value set.</param>
        /// <param name="lhs">The integer value for the object.</param>
        /// <returns>The KeyValue object which has been modified.</returns>
        public static KeyValue operator +(KeyValue rhs, int lhs)
        {
            return rhs.Set(lhs);
        }

        /// <summary>
        /// Sets the value of the right hand kv object to be the left hand float
        /// </summary>
        /// <param name="rhs">The KeyValue object to have its value set.</param>
        /// <param name="lhs">The float value for the object.</param>
        /// <returns>The KeyValue object which has been modified.</returns>
        public static KeyValue operator +(KeyValue rhs, float lhs)
        {
            return rhs.Set(lhs);
        }

        /// <summary>
        /// Sets the value of the right hand kv object to be the left hand string
        /// </summary>
        /// <param name="rhs">The KeyValue object to have its value set.</param>
        /// <param name="lhs">The string value for the object.</param>
        /// <returns>The KeyValue object which has been modified.</returns>
        public static KeyValue operator +(KeyValue rhs, string lhs)
        {
            return rhs.Set(lhs);
        }

        /// <summary>
        /// Sets the value of the right hand kv object to be the left hand bool
        /// </summary>
        /// <param name="rhs">The KeyValue object to have its value set.</param>
        /// <param name="lhs">The bool value for the object.</param>
        /// <returns>The KeyValue object which has been modified.</returns>
        public static KeyValue operator +(KeyValue rhs, bool lhs)
        {
            return rhs.Set(lhs);
        }

        /// <summary>
        /// ToString method with an indentation paremeter.
        /// </summary>
        /// <param name="indent">The number of spaces to indent.</param>
        /// <returns>A string representation of the KeyValue structure.</returns>
        public string ToString(int indent)
        {
            if (children == null)
            {
                return string.Format("{0}\"{1}\"\t\"{2}\"", indent > 0 ? "\t" : "", Key, _value);
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                builder.Append("\t");
            }
            builder.AppendLine(string.Format("\"{0}\"", Key));
            for (int i = 0; i < indent; i++)
            {
                builder.Append("\t");
            }
            builder.AppendLine("{");
            foreach (KeyValue child in Children)
            {
                if (!child.HasChildren)
                {
                    for (int i = 0; i < indent; i++)
                    {
                        builder.Append("\t");
                    }
                }
                builder.AppendLine(child.ToString(indent + 1));
            }
            for (int i = 0; i < indent; i++)
            {
                builder.Append("\t");
            }
            builder.AppendLine("}");
            return builder.ToString();
        }
        /// <summary>
        /// An override for the base ToString method.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return ToString(0);
        }

        /// <summary>
        /// Clears the children of this Key-Value object
        /// </summary>
        public void ClearChildren()
        {
            this.children.Clear();
            this.children = null;
        }
        /// <summary>
        /// Creates a deep duplicate of this Key-Value object.
        /// </summary>
        /// <returns>A new KeyValue object that's a deep copy of this one.</returns>
        public object Clone()
        {
            KeyValue kv;
            DeepCopy(this, out kv);

            return kv;
        }
        /// <summary>
        /// Null out the parent entry of all children.
        /// </summary>
        public void clearChildParents()
        {
            foreach(KeyValue child in children)
            {
                child.Parent = null;
            }
        }
        /// <summary>
        /// Make a deep copy of a Keyvalue object.
        /// </summary>
        /// <param name="original">The source object.</param>
        /// <param name="Copy">The destination object.</param>
        private static void DeepCopy(KeyValue original, out KeyValue Copy)
        {
            Copy = new KeyValue(original.Key);
            if (original.HasChildren)
            {
                foreach (KeyValue child in original.Children)
                {
                    //Copy the child
                    KeyValue kv = new KeyValue(child.Key);
                    DeepCopy(child, out kv);
                    Copy += kv;
                }
            }
            else
            {
                Copy += original.GetString();
            }

        }
    }
}
