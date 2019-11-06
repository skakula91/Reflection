using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;


namespace TestObjectGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestObjectGenerator());
        }
    }


    public class Convert
    {
        public object GetValue(Object objectToReflect, int count = 0)
        {
            var interfaceTypes = objectToReflect.GetType().GetInterfaces();
            var val = IsEnumerable(interfaceTypes, objectToReflect, null, count);
            var test = PopulateObj(val.ObjectToReflect, count);
            return test;
        }

        public Object PopulateObj(Object objectToReflect, int count = 0)
        {
            object Result = null;
            if (objectToReflect.GetType().IsGenericType)
            {
                var lst = (IList)objectToReflect;
                //var listRef = typeof(List<>);
                //Type[] listParam = { lst[0].GetType() };
                //Result = Activator.CreateInstance(listRef.MakeGenericType(listParam));
                for (int i = 0; i < count; i++)
                {
                    var obj = lst[i];
                    foreach (var propertyInfo in obj.GetType().GetProperties())
                    {
                        var interfaceTypes = propertyInfo.PropertyType.GetInterfaces();
                        var val = IsEnumerable(interfaceTypes, obj, propertyInfo, count);
                        //if (val.IsEnumerable)
                        //{
                        //    Result.GetType().GetMethod("Add")?.Invoke(Result, new[] { val.ObjectToReflect });
                        //}                      
                    }
                }

                objectToReflect = lst;
            }
            else
            {
                foreach (var propertyInfo in objectToReflect.GetType().GetProperties())
                {
                    var interfaceTypes = propertyInfo.PropertyType.GetInterfaces();
                    IsEnumerable(interfaceTypes, objectToReflect, propertyInfo, count);
                    GetPropValue(propertyInfo, objectToReflect);
                }
            }

            return objectToReflect;
        }
        public Object GetPropValue(PropertyInfo p, object obj)
        {
            if (obj == null) { return null; }
            Type type = obj.GetType();
            string value;
            bool isParsed;
            switch (p.PropertyType.FullName)
            {
                case "System.String":
                    //case typeof(Nullable<Int64>):
                    string stringval = Data.ReadText(p.Name);
                    stringval = stringval ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    p.SetValue(obj, stringval, null);
                    break;
                case "System.Int32":
                    value = Data.ReadText(p.Name);
                    value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    isParsed = Int32.TryParse(value, out var intresult);
                    p.SetValue(obj, isParsed ? intresult : GenerateDefault.GetIntValue(), null);
                    break;
                case "System.Int64":
                    value = Data.ReadText(p.Name);
                    value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    isParsed = Int64.TryParse(value, out var longresult);
                    p.SetValue(obj, isParsed ? longresult : GenerateDefault.GetLongValue(), null);
                    break;
                case "System.Boolean":
                    value = Data.ReadText(p.Name);
                    value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    isParsed = bool.TryParse(value, out var boolresult);
                    p.SetValue(obj, isParsed && boolresult, null);
                    break;
                case "System.DateTime":
                    var date = DateTime.Now;
                    p.SetValue(obj, date, null);
                    break;
                default:
                    if (p.PropertyType == typeof(Nullable<Int64>))
                    {
                        value = Data.ReadText(p.Name);
                        value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                        isParsed = Int64.TryParse(value, out var longNullableresult);
                        p.SetValue(obj, isParsed ? longNullableresult : GenerateDefault.GetLongValue(), null);
                    }
                    else if (p.PropertyType == typeof(Nullable<Int32>))
                    {
                        value = Data.ReadText(p.Name);
                        value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                        isParsed = Int32.TryParse(value, out var intNullaleresult);
                        p.SetValue(obj, isParsed ? intNullaleresult : GenerateDefault.GetIntValue(), null);
                    }
                    else if (p.PropertyType == typeof(Nullable<bool>))
                    {
                        value = Data.ReadText(p.Name);
                        value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                        isParsed = bool.TryParse(value, out var boolNullableresult);
                        p.SetValue(obj, isParsed && boolNullableresult, null);
                    }
                    else if(p.PropertyType == typeof(Nullable<DateTime>))
                    {
                        date = DateTime.Now;
                        p.SetValue(obj, date, null);
                    }
                    break;
            }
            return obj;
        }

        private ObjectModel IsEnumerable(Type[] interfaceTypes, object objectToReflect, PropertyInfo p = null, int count = 0)
        {
            foreach (Type interfaceType in interfaceTypes)
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition()
                    == typeof(IList<>))
                {
                    Type objTyp = interfaceType.GetGenericArguments()[0];
                    var listRef = typeof(List<>);
                    Type[] listParam = { objTyp };
                    object result = Activator.CreateInstance(listRef.MakeGenericType(listParam));
                    while (count > 0)
                    {
                        var instance = Activator.CreateInstance(objTyp);
                        foreach (var prop in instance.GetType().GetProperties())
                        {
                            var interfaceChildTypes = prop.PropertyType.GetInterfaces();
                            if (prop.PropertyType.IsClass && !prop.PropertyType.IsGenericType && !prop.GetType().Namespace.StartsWith("System"))
                            {
                                var childInstance = Activator.CreateInstance(prop.PropertyType);
                                foreach (var childProp in ((System.Reflection.TypeInfo)childInstance.GetType()).DeclaredFields)
                                {
                                    GetFieldValue(childProp, childInstance);
                                }
                                prop.SetValue(instance, childInstance, null);
                            }
                            //var obj = IsEnumerableChild(interfaceChildTypes,instance,prop,count);
                            //if (obj.IsEnumerable &&  p != null)
                            //{
                            //   p.SetValue(instance,obj.ObjectToReflect,null);
                            //}
                            //else
                            //{
                            GetPropValue(prop, instance);
                            // }
                        }
                        result.GetType().GetMethod("Add")?.Invoke(result, new[] { instance });
                        count--;
                    }

                    if (p != null)
                    {
                        p.SetValue(objectToReflect, result, null);
                    }
                    else
                    {
                        objectToReflect = result;
                    }
                    return new ObjectModel() { ObjectToReflect = objectToReflect, IsEnumerable = true };
                }
            }

            return new ObjectModel() { ObjectToReflect = objectToReflect, IsEnumerable = false };
        }
        private ObjectModel IsEnumerableChild(Type[] interfaceTypes, object objectToReflect, PropertyInfo p = null, int count = 0)
        {
            var interfaces = interfaceTypes.FirstOrDefault(interfaceType => interfaceType.IsGenericType &&
                                                                   interfaceType.GetGenericTypeDefinition()
                                                                   == typeof(IList<>));
            if (interfaces != null)
            {
                Type objTyp = interfaces.GetGenericArguments()[0];
                var listRef = typeof(List<>);
                Type[] listParam = { objTyp };
                object result = Activator.CreateInstance(listRef.MakeGenericType(listParam));
                GetPropValue(p, objectToReflect);
                return new ObjectModel() { ObjectToReflect = objectToReflect, IsEnumerable = true };
            }
            else
            {
                return new ObjectModel() { ObjectToReflect = objectToReflect, IsEnumerable = false };
            }
        }
        public Object GetFieldValue(FieldInfo p, object obj)
        {
            if (obj == null) { return null; }
            Type type = obj.GetType();
            string value;
            bool isParsed;
            switch (p.FieldType.FullName)
            {
                case "System.String":
                    //case typeof(Nullable<Int64>):
                    string stringval = Data.ReadText(p.Name);
                    stringval = stringval ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    p.SetValue(obj, stringval);
                    break;
                case "System.Int32":
                    value = Data.ReadText(p.Name);
                    value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    isParsed = Int32.TryParse(value, out var intresult);
                    p.SetValue(obj, isParsed ? intresult : GenerateDefault.GetIntValue());
                    break;
                case "System.Int64":
                    value = Data.ReadText(p.Name);
                    value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    isParsed = Int64.TryParse(value, out var longresult);
                    p.SetValue(obj, isParsed ? longresult : GenerateDefault.GetLongValue());
                    break;
                case "System.Boolean":
                    value = Data.ReadText(p.Name);
                    value = value ?? Data.ReadText(p.DeclaringType?.Name + p.Name);
                    isParsed = bool.TryParse(value, out var boolresult);
                    p.SetValue(obj, isParsed && boolresult);
                    break;
                case "System.DateTime":
                    var date = DateTime.Now;
                    p.SetValue(obj, date);
                    break;
            }
            return obj;
        }

    }
    


    public class ObjectModel
    {
        public bool IsEnumerable { get; set; }
        public Object ObjectToReflect { get; set; }
    }

    public class TestObject
    {
        public  int ProductId { get; set; }
        public  string ProductName { get; set; }
        public List<PricingPlan> PricingPlans { get; set; }
    }

    public class PricingPlan
    {
        public  int PricingPlanId { get; set; }
        public  string PricingPlanName { get; set; }
    }

}
