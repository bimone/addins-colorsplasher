#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using ColorSplasher.Views;
#endregion

namespace ColorSplasher
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //If the host document is not saved
            if (doc == null || string.IsNullOrEmpty(doc.PathName))
            {
                MessageBox.Show(Resource.Error_SaveYourFile, Resource.MsgBox_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return Result.Cancelled;
            }

            if (doc.IsFamilyDocument)
            {
                MessageBox.Show(Resource.Error_FamilyProject,Resource.MsgBox_Warning, MessageBoxButton.OK,MessageBoxImage.Warning);
                return Result.Cancelled;
            }
            ColorSplasherWindow dlg = new ColorSplasherWindow(doc);
            dlg.ShowDialog();
            return Result.Succeeded;
        }
    }
}