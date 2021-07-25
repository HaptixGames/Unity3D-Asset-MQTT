
namespace hg.LitJson
{
   [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
   public class JsonPropertyAttribute : System.Attribute
	{
		public string Name;

		public JsonPropertyAttribute(string name)
		{
			Name = name;
		}
	}
}
