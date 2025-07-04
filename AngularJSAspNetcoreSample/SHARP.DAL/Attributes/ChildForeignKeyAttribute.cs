using System;

namespace SHARP.DAL.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ChildForeignKeyAttribute : Attribute
    {
    }
}
