using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DaybreakGames.Census
{
    public class UnderscorePropertyNamesContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            property.PropertyName = Regex.Replace(property.PropertyName, @"(\w)([A-Z])", "$1_$2").ToLower();

            return property;
        }
    }
}
