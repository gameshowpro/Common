using System.Runtime.Serialization;

namespace GameshowPro.Common.JsonNet;

/// <summary>
/// Subclass of <see cref="Newtonsoft.Json.Serialization.DefaultContractResolver"/> which uses the constructor marked with <see cref="System.Text.Json.Serialization.JsonConstructorAttribute"/> if present and always presumes <see cref="MemberSerialization.OptIn"/>.
/// </summary>
internal class DefaultContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
{
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        JsonObjectContract contract = base.CreateObjectContract(objectType);
        ConstructorInfo constructorInfo;
        if (contract.OverrideCreator == null)
        {
            IEnumerator<ConstructorInfo> en = 
                objectType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(c => c.IsDefined(typeof(System.Text.Json.Serialization.JsonConstructorAttribute), true))
                    .GetEnumerator();

            if (en.MoveNext())
            {
                constructorInfo = en.Current;
                if (en.MoveNext())
                {
                    throw new JsonException("Multiple constructors with the JsonConstructorAttribute.");
                }
                contract.CreatorParameters.Clear();
                contract.OverrideCreator = a => constructorInfo.Invoke(a);
                CreateConstructorParameters(constructorInfo, contract.Properties).ForEach(contract.CreatorParameters.Add);
            }
        }
        return contract;
    }

    /// <summary>
    /// Override <see cref="Newtonsoft.Json.Serialization.DefaultContractResolver"/> to presume OptIn serialization.
    /// </summary>
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        if (!property.Ignored
            && !member.IsDefined(typeof(JsonPropertyAttribute), true)
            && !member.IsDefined(typeof(DataMemberAttribute), true)
            && !member.IsDefined(typeof(System.Text.Json.Serialization.JsonAttribute), true)
        )
        {
            property.Ignored = true;
        }
        return property;
    }
}
