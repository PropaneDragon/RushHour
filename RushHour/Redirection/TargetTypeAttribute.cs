using System;

namespace RushHour.Redirection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class TargetTypeAttribute : Attribute
    {
        public TargetTypeAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}