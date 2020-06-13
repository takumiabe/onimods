using System;
using System.Linq;
using System.Reflection;

namespace DisplayDTU
{
    // Not Used
    class EffectorEntryAdapter
    {
        static private Type type;
        static private ConstructorInfo ci;
        static private FieldInfo name_f;
        static private FieldInfo value_f;
        static private FieldInfo count_f;
        static private MethodInfo toString_m;

        static EffectorEntryAdapter()
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            type = assembly.GetType("EffectorEntry");

            ci = type.GetConstructor(new Type[] { typeof(string), typeof(float) });

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod;
            name_f = type.GetField("name", flags);
            value_f = type.GetField("value", flags);
            count_f = type.GetField("count", flags);
            toString_m = type.GetMethod("ToString", flags);
        }

        private object original;

        public EffectorEntryAdapter(string name, float value)
        {
            this.original = ci.Invoke(new object[] { name, value });
        }

        public string name
        {
            get
            {
                return (string)name_f.GetValue(original);
            }
            set
            {
                name_f.SetValue(original, value);
            }
        }
        public float value
        {
            get
            {
                return (float)value_f.GetValue(original);
            }
            set
            {
                value_f.SetValue(original, value);
            }
        }
        public int count
        {
            get
            {
                return (int)count_f.GetValue(original);
            }
            set
            {
                count_f.SetValue(original, value);
            }
        }


        public override string ToString()
        {
            return (string)toString_m.Invoke(original, null);
        }
    }
}
