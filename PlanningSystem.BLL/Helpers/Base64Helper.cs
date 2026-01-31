using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.BLL.Helpers
{
    internal class Base64Helper
    {
        static public string To(byte[] data)
        {
            return Convert.ToBase64String(data).Replace("=", "");
        }
        static public byte[] From(string str)
        {
            var paddingLength = 4 - (str.Length % 4);
            var paddingString = "";
            if (paddingLength < 4 && paddingLength > 0)
                paddingString = "====".Substring(0, paddingLength);
            return Convert.FromBase64String(str + paddingString);
        }
    }
}
