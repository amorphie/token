
using System.Dynamic;
using System.Reflection;

namespace amorphie.token.core.Extensions;

public static class MapperExtension
{
    public static void MatchAndMap<TSource, TDestination>(this TSource source, TDestination destination)
            where TSource : class, new()
            where TDestination : class, new()
    {
        if (source != null && destination != null)
        {
            List<PropertyInfo> sourceProperties = source.GetType().GetProperties().ToList<PropertyInfo>();
            List<PropertyInfo> destinationProperties = destination.GetType().GetProperties().ToList<PropertyInfo>();

            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                PropertyInfo? destinationProperty = destinationProperties.Find(item => item.Name == sourceProperty.Name);

                if (destinationProperty != null)
                {
                    try
                    {
                        destinationProperty.SetValue(destination, sourceProperty.GetValue(source, null), null);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

    }

    public static TDestination MapTo<TDestination>(this object source)
        where TDestination : class, new()
    {
        var destination = Activator.CreateInstance<TDestination>();
        MatchAndMap(source, destination);

        return destination;
    }

    public static ExpandoObject ConvertToExpando(object obj)
    {
        //Get Properties Using Reflections
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        PropertyInfo[] properties = obj.GetType().GetProperties(flags);

        //Add Them to a new Expando
        ExpandoObject expando = new ExpandoObject();
        foreach (PropertyInfo property in properties)
        {
            if(property.GetValue(obj)! is not null)
                AddProperty(expando, property.Name, property.GetValue(obj)!);
        }

        return expando;
    }

    public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
    {
        //Take use of the IDictionary implementation
        var expandoDict = expando as IDictionary<String, object>;
        if (expandoDict.ContainsKey(propertyName))
            expandoDict[propertyName] = propertyValue;
        else
            expandoDict.Add(propertyName, propertyValue);
    }

}
