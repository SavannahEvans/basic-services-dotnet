﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// MetasysObject is a structure that holds information about a Metasys object.
    /// </summary>
    public struct MetasysObject
    {
        private CultureInfo _CultureInfo;

        /// <summary>The item reference of the Metasys object.</summary>
        public string ItemReference { private set; get; }

        /// <summary>The id of the Metasys object.</summary>
        public Guid Id { private set; get; }

        /// <summary>The name of the Metasys object.</summary>
        public string Name { private set; get; }   

        /// <summary>The description of the Metasys object.</summary>
        public string Description { private set; get; }

        /// <summary>The direct children objects of the Metasys object.</summary>
        public IEnumerable<MetasysObject> Children { private set; get; }

        /// <summary>
        /// The number of direct children objects.
        /// </summary>
        /// <value>The number of children or -1 if there is no children data.</value>
        public int ChildrenCount { private set; get; }

        internal MetasysObject(JToken token, IEnumerable<MetasysObject> children = null, CultureInfo cultureInfo = null)
        {
            _CultureInfo = cultureInfo;           
            Children = children;
            if (Children != null)
            {
                ChildrenCount = Children.ToList().Count;
            }
            else
            {
                ChildrenCount = -1;
            }

            try
            {
                Id = new Guid(token["id"].Value<string>());
            }
            catch
            {
                Id = Guid.Empty;
            }

            string placeholder = "";
            try
            {
                ItemReference = token["itemReference"].Value<string>();
            }
            catch
            {
                ItemReference = placeholder;
            }

            try
            {
                Name = token["name"].Value<string>();
            }
            catch
            {
                Name = placeholder;
            }

            try
            {
                Description = token["description"].Value<string>();
            }
            catch
            {
                Description = placeholder;
            }
        }

        /// <summary>
        /// Overwrites the ToString method to print out an object's information.
        /// </summary>
        /// <returns>A string representation of the Command.</returns>
        public override string ToString()
        {
            return string.Concat("Id: ", Id, "\n",
                "ItemReference: ", ItemReference, "\n",
                "Name: ", Name, "\n",             
                "Description: ", Description, "\n",
                "Number of Children: ", ChildrenCount);
        }
    }
}
