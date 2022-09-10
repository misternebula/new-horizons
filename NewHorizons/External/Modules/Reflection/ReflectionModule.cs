using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Reflection
{
    [JsonObject]
    public class ReflectionModule
    {
        /// <summary>
        /// Allows modifying fields of components on any GameObject that is part of this planet on Start.
        /// </summary>
        public FieldModule[] Fields;

        /// <summary>
        /// Allows calling methods of components on any GameObject that is part of this planet on Start.
        /// </summary>
        public MethodModule[] Methods;
    }
}
