using Microsoft.ML.Runtime.Api;
using MLTestCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MLGeneticAlgorithm.Service
{
    public static class TypeService
    {
        public static List<object> CloneObjectsToNewObjectType<t>(List<t> objs, List<FieldInfo> fieldsToApply)
        {
            var tb = GetTypeBuilder<t>();

            List<object> results = new List<object>();
            for(int i = 0; i < objs.Count; i++)
            {
                var newObj = CloneObjectToNewObjectType(tb, objs[i], fieldsToApply, i == 0);
                results.Add(newObj);
            }

            var typeToApplyTo = results.First().GetType();
            Console.WriteLine(typeToApplyTo.Name);

            return results;
        }

        public static object CloneObjectToNewObjectType<t>(t obj, List<FieldInfo> fieldsToApply)
        {
            var tb = GetTypeBuilder<t>();
            
            return CloneObjectToNewObjectType(tb, obj, fieldsToApply, true);
        }

        private static object CloneObjectToNewObjectType<t>(TypeBuilder tb, t parentObject, List<FieldInfo> fieldsToApply, bool firstRun)
        {
            var newType = tb.CreateType();
            var instance = Activator.CreateInstance(newType);

            if (fieldsToApply?.Any() == true && firstRun)
            {
                ApplyAttribute<ColumnAttribute>(instance.GetType(), fieldsToApply);
            }

            foreach (var field in instance.GetType().GetFields())
            {
                field.SetValue(instance, field.GetValue(parentObject));
            }
            
            return instance;
        }

        public static void ApplyAttribute<tr>(Type typeToApplyTo, List<FieldInfo> fieldsToApplyTo) where tr : Attribute
        {
            var fieldsFromObject = FastDeepCloner.DeepCloner.GetFastDeepClonerFields(typeToApplyTo);
            for (int i = 0; i < fieldsToApplyTo.Count; i++)
            {
                var matchingField = fieldsFromObject.First(s => s.Name == fieldsToApplyTo[i].Name);
                if(matchingField != null)
                {
                    matchingField.Attributes.Add(new ColumnAttribute(i.ToString(), i == fieldsToApplyTo.Count - 1 ? "Label" : null));
                }
            }
        }

        private static TypeBuilder GetTypeBuilder<t>()
        {
            var type = typeof(t);
            var execAssembly = Assembly.GetExecutingAssembly();
            var ab = AssemblyBuilder.DefineDynamicAssembly(execAssembly.GetName(), AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule(execAssembly.GetName().FullName);
            var newTypeName = type.Name + Guid.NewGuid();
            return mb.DefineType(newTypeName, TypeAttributes.Public, type);
        }
    }
}
