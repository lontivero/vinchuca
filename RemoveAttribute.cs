using System;
using System.Collections.Generic;
using System.Text;

namespace DreamBot
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Class)]
    public class RemoveAttribute : Attribute
    {
    }
}
