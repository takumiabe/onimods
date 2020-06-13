using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DisplayDTU
{

    public class SelectToolHoverTextCardAccessor
    {
        private Type type = typeof(SelectToolHoverTextCard);

        public SelectToolHoverTextCardAccessor(SelectToolHoverTextCard original)
        {
            this.original = original;
        }


        public SelectToolHoverTextCard original { get; private set; }

        public List<KSelectable> overlayValidHoverObjects
        {
            get
            {
                FieldInfo fi = type.GetField("overlayValidHoverObjects", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
                return (List<KSelectable>)fi.GetValue(original);
            }
        }

        public Dictionary<HashedString, Func<bool>> overlayFilterMap
        {
            get
            {
                FieldInfo fi = type.GetField("overlayFilterMap", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
                return (Dictionary<HashedString, Func<bool>>)fi.GetValue(original);
            }
        }

        public int maskOverlay
        {
            get
            {
                FieldInfo fi = type.GetField("maskOverlay", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
                return (int)fi.GetValue(original);
            }
        }

        // static
        public List<Type> hiddenChoreConsumerTypes
        {
            get
            {
                FieldInfo fi = type.GetField("hiddenChoreConsumerTypes", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod);
                return (List<Type>)fi.GetValue(null);
            }
        }

        public string cachedTemperatureString
        {
            get
            {
                FieldInfo fi = type.GetField("cachedTemperatureString", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
                return (string)fi.GetValue(original);
            }
            set
            {
                FieldInfo fi = type.GetField("cachedTemperatureString", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
                fi.SetValue(original, value);
            }
        }
        public float cachedTemperature
        {
            get
            {
                FieldInfo fi = type.GetField("cachedTemperature", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
                return (float)fi.GetValue(original);
            }
            set
            {
                FieldInfo fi = type.GetField("cachedTemperature", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
                fi.SetValue(original, value);
            }
        }

        public bool ShouldShowSelectableInCurrentOverlay(KSelectable kselectable)
        {
            MethodInfo method = type.GetMethod("ShouldShowSelectableInCurrentOverlay", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            if (method == null)
            {
                Debug.Log("method is empty");
            }
            return (bool)method.Invoke(original, new[] { kselectable });
        }

        internal bool ShowStatusItemInCurrentOverlay(StatusItem item)
        {
            MethodInfo method = type.GetMethod("ShowStatusItemInCurrentOverlay", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            return (bool)method.Invoke(original, new[] { item });
        }

        internal bool IsStatusItemWarning(StatusItemGroup.Entry item)
        {
            MethodInfo method = type.GetMethod("IsStatusItemWarning", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            return (bool)method.Invoke(original, new object[] { item });
        }
    }
}
