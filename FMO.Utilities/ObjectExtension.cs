namespace FMO.Utilities
{
    public static class ObjectExtension
    {
        public static string PrintProperties(this object obj)
        {
            if (obj is null) return "Null Object";

            var type = obj.GetType();
            string info = string.Join(",\n\t", type.GetProperties().Select(x => $"{x.Name}:{x.GetValue(obj)?.ToString() ?? "null"}"));
            return $"{type.Name} \n\t {info} \n\n";             
        }
    }
}
