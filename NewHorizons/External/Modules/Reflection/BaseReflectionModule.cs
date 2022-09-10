namespace NewHorizons.External.Modules.Reflection
{
    public class BaseReflectionModule
    {
        /// <summary>
        /// Path from the root body to the GameObject with the component. 
        /// Does not include the root body name (e.x., don't include BrittleHollow_Body for Brittle Hollow)
        /// </summary>
        public string path;

        /// <summary>
        /// Type name of the component on the GameObject you want to modify.
        /// </summary>
        public string componentType;
    }
}
