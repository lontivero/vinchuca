using System;
using System.Collections.Generic;
using System.Text;

namespace Vinchuca
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Class)]
    public class RemoveAttribute : Attribute
    {
    }
}
