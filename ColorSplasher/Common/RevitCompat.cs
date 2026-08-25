using Autodesk.Revit.DB;

namespace ColorSplasher
{
    internal static class RevitCompat
    {
        // ElementId.IntegerValue was replaced by ElementId.Value in Revit 2024.
        public static long ToLong(this ElementId id)
        {
#if REVIT2023 || REVIT2024
            return id.IntegerValue;
#else
            return id.Value;
#endif
        }
    }
}
