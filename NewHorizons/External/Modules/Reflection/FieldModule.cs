using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Reflection
{
    [JsonObject]
    public class FieldModule : BaseReflectionModule
    {
        /// <summary>
        /// Name of the field on the component.
        /// </summary>
        public string fieldName;

        /// <summary>
        /// The value you want to set the attribute to.
        /// </summary>
        public object value;
    }
}
