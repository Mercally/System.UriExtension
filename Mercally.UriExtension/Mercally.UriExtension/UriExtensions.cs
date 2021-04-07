using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;

namespace System.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        ///     Adds query string value to an existing url, both absolute and relative URI's are supported.
        /// </summary>
        /// <example>
        /// <code>
        ///     // returns "www.domain.com/test?param1=val1&amp;param2=val2&amp;param3=val3"
        ///     new Uri("www.domain.com/test?param1=val1").ExtendQuery(new Dictionary&lt;string, string&gt; { { "param2", "val2" }, { "param3", "val3" } }); 
        /// 
        ///     // returns "/test?param1=val1&amp;param2=val2&amp;param3=val3"
        ///     new Uri("/test?param1=val1").ExtendQuery(new Dictionary&lt;string, string&gt; { { "param2", "val2" }, { "param3", "val3" } }); 
        /// </code>
        /// </example>
        /// <param name="uri"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Uri ExtendQuery(this Uri uri, IDictionary<string, string> values)
        {
            var baseUrl = uri.ToString();
            var queryString = string.Empty;

            if (baseUrl.Contains("?"))
            {
                var urlSplit = baseUrl.Split('?');
                baseUrl = urlSplit[0];
                queryString = urlSplit.Length > 1 ? urlSplit[1] : string.Empty;
            }

            NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);

            foreach (KeyValuePair<string, string> param in values ?? new Dictionary<string, string>())
            {
                queryCollection[param.Key] = param.Value;
            }

            UriKind uriKind = uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative;

            return queryCollection.Count == 0
              ? new Uri(baseUrl, uriKind)
              : new Uri(string.Format("{0}?{1}", baseUrl, queryCollection), uriKind);
        }

        /// <summary>
        ///     Adds query string value to an existing url, both absolute and relative URI's are supported.
        /// </summary>
        /// <example>
        /// <code>
        ///     // returns "www.domain.com/test?param1=val1&amp;param2=val2&amp;param3=val3"
        ///     new Uri("www.domain.com/test?param1=val1").ExtendQuery(new { param2 = "val2", param3 = "val3" }); 
        /// 
        ///     // returns "/test?param1=val1&amp;param2=val2&amp;param3=val3"
        ///     new Uri("/test?param1=val1").ExtendQuery(new { param2 = "val2", param3 = "val3" }); 
        /// </code>
        /// </example>
        /// <param name="uri"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Uri AddStringQuery(this Uri uri, object value)
        {
            IDictionary<string, string> collection = GetDictionaryProperties(value);
            Uri uriResult = ExtendQuery(uri, collection);
            return uriResult;
        }

        private static IDictionary<string, string> GetDictionaryProperties(object value, string lastPropertyName = "")
        {
            PropertyInfo[] properties = value?.GetType()?.GetProperties() ?? new PropertyInfo[0];
            IDictionary<string, string> propertyDicctionary = new Dictionary<string, string>();

            foreach (PropertyInfo prop in properties)
            {
                string propName = lastPropertyName + prop.Name;
                string propValue = prop.GetValue(value)?.ToString();

                if (string.IsNullOrEmpty(propValue))
                    continue;

                if (!prop.PropertyType.Namespace.Contains("System"))
                {
                    IDictionary<string, string> dictionaryProps = GetDictionaryProperties(prop.GetValue(value), propName + ".");
                    propertyDicctionary.AddRange(dictionaryProps);
                }
                else if (prop.PropertyType.Namespace == nameof(System))
                {
                    propertyDicctionary.Add(propName, propValue);
                }
                else
                {
                    // Do nothing
                }
            }

            return propertyDicctionary;
        }

        private static void AddRange(this IDictionary<string, string> dictionary, IDictionary<string, string> values)
        {
            foreach (var item in values)
            {
                dictionary.Add(item);
            }
        }
    }
}