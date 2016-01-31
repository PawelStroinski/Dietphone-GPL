using System;
using System.Linq;
using Dietphone.Models;
using System.Text.RegularExpressions;

namespace Dietphone
{
    static class TestExtensions
    {
        internal static Guid ToGuid(this string @byteInString)
        {
            return byte.Parse(@byteInString).ToGuid();
        }

        internal static Guid ToGuid(this byte @byte)
        {
            return new Guid(@byte, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}
