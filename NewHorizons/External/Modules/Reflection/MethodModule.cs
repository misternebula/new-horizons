using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Reflection
{
    [JsonObject]
    public class MethodModule : BaseReflectionModule
    {
        /// <summary>
        /// Name of the method on the component.
        /// </summary>
        public string methodName;

        /// <summary>
        /// The values you want to pass through to the method. Optional.
        /// </summary>
        public object[] parameters;
    }
}
