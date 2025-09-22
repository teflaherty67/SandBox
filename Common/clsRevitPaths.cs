using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandBox.Common
{
    public static class RevitPaths
    {
#if REVIT2025

        public const string SharedParamFile = @"S:\Shared Folders\Lifestyle USA Design\Library 2025\Parameter.txt";

#endif

#if REVIT2026

    public const string SharedParamFile = @"S:\Shared Folders\Lifestyle USA Design\Library 2026\Parameter.txt"

#endif
    }
}
