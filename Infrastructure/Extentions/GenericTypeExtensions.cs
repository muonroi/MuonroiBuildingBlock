namespace MuonroiBuildingBlock.Infrastructure.Extentions
{
    public static class GenericTypeExtensions
    {
        public static string GetGenericTypeName(this Type type)
        {
            string empty = string.Empty;
            if (type.IsGenericType)
            {
                string text = string.Join(",", (from t in type.GetGenericArguments()
                                                select t.Name).ToArray());
                return type.Name.Remove(type.Name.IndexOf('`')) + "<" + text + ">";
            }

            return type.Name;
        }

        public static string GetGenericTypeName(this object @object)
        {
            return @object.GetType().GetGenericTypeName();
        }
    }
}