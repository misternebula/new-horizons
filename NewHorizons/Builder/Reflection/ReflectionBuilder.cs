using NewHorizons.External.Modules.Reflection;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Reflection
{
    public static class ReflectionBuilder
    {
        public static void Make(GameObject root, ReflectionModule reflection)
        {
            if (reflection.Methods != null)
                foreach (var method in reflection.Methods)
                    Apply(root, method);

            if (reflection.Fields != null)
                foreach (var field in reflection.Fields)
                    Apply(root, field);
        }

        private static void Apply(GameObject root, BaseReflectionModule module)
        {
            var gameObject = root.transform.Find(module.path);

            if (gameObject == null)
            {
                Logger.LogError($"Could not find path {module.path} on {root.name}");
                return;
            }

            var componentType = Type.GetType(module.componentType);

            if (componentType == null)
            {
                Logger.LogError($"Could not find valid Type with name {module.componentType}");
                return;
            }

            var component = gameObject.GetComponent(componentType);
            
            if (component == null)
            {
                Logger.LogError($"Could not find component of Type {module.componentType} on GameObject {gameObject.name}");
                return;
            }

            if (module is FieldModule fieldModule)
            {
                try
                {
                    componentType.GetField(fieldModule.fieldName).SetValue(component, fieldModule.value);
                }
                catch(Exception e)
                {
                    Logger.LogError($"Failed to set field [{fieldModule.fieldName}] to [{fieldModule.value}] for [{gameObject.name}].[{fieldModule.componentType}]:{e}");
                }
            }
            else if (module is MethodModule methodModule)
            {
                try
                {
                    componentType.GetMethod(methodModule.methodName).Invoke(component, methodModule.parameters);
                }
                catch(Exception e)
                {
                    Logger.LogError($"Failed to invoke [{methodModule.methodName}] with params [{string.Join(",", methodModule.parameters)}] for [{gameObject.name}].[{methodModule.componentType}]:{e}");
                }
            }
        }
    }
}
