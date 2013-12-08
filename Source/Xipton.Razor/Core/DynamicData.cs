#region  Microsoft Public License
/* This code is part of Xipton.Razor v3.0
 * (c) Jaap Lamfers, 2013 - jaap.lamfers@xipton.net
 * Licensed under the Microsoft Public License (MS-PL) http://www.microsoft.com/en-us/openness/licenses.aspx#MPL
 */
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Xipton.Razor.Core
{
    /// <summary>
    /// This class is used for both the template's Model (with anonymous model types only) and ViewBag property
    /// </summary>
    public class DynamicData : DynamicObject
    {
        private readonly ConcurrentDictionary<string, dynamic>
            _data;

        public DynamicData(object target)
        : this(target == null ? null : (IDictionary<string,object>)target.GetType()
                .GetProperties()
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                .ToDictionary(property => property.Name, property => property.GetValue(target, null)))
        {
        }

        public DynamicData(IDictionary data)
        {
            _data = new ConcurrentDictionary<string, object>();
            if (data == null) 
                return;

            foreach (var key in data.Keys)
                _data[key.ToString()] = ToDynamic(data[key]);
        }

        public DynamicData(IEnumerable<KeyValuePair<string, object>> data = null)
        {
            _data = new ConcurrentDictionary<string, object>();
            if (data != null)
                foreach (var pair in data)
                    _data[pair.Key] = ToDynamic(pair.Value);
        }

        /// <summary>
        /// The model only needs to be wrapped into a IDynamicMetaObjectProvider if it is an emitted anonymous type.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static dynamic ToDynamic(object model) {
            return model == null || !Attribute.IsDefined(model.GetType(), typeof(CompilerGeneratedAttribute))
                       ? model
                       : (model is IDynamicMetaObjectProvider
                              ? model
                              : new DynamicData(model));
        }


        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _data.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _data.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _data[binder.Name] = ToDynamic(value);
            return true;
        }


    }
}