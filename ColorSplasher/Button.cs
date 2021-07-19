using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ColorSplasher
{
    [Transaction(TransactionMode.Manual)]
    public class Button : IExternalApplication
    {
        private const string TabLabel = "BIM One";
        public Result OnStartup(UIControlledApplication application)
        {
        
            var assemblieFolder = Path.GetDirectoryName(typeof(Button).Assembly.Location);
            var commandPath = Path.Combine(assemblieFolder, "ColorSplasher.dll");

            var toolsPanel = GetOrCreateRibbonPanel(application);
            
            var pushButton = toolsPanel.AddItem(new PushButtonData(
                                    "Color Splasher",
                                    "Color\nSplasher",
                                    commandPath,
                                    "ColorSplasher.Command")) as PushButton;

            var buttonImage = Path.Combine(assemblieFolder, @"Resources\button.png");
            if (!File.Exists(buttonImage))
                buttonImage = Path.Combine(Directory.GetParent(assemblieFolder).FullName, @"Resources\button.png");

            pushButton.LargeImage = new BitmapImage(new Uri(buttonImage));
            pushButton.ToolTip = "Visualize and verify the information in a model with color";
            pushButton.ToolTipImage = new BitmapImage(new Uri(buttonImage));
            pushButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://github.com/bimone/addins-colorsplasher/wiki"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        
        private RibbonPanel GetOrCreateRibbonPanel(UIControlledApplication application)
        {
            var ribbonPanel = application.GetRibbonPanels(Tab.AddIns).Find(x => x.Name == TabLabel);
            if (ribbonPanel == null)
                ribbonPanel = application.CreateRibbonPanel(Tab.AddIns, TabLabel);

            return ribbonPanel;
        }

    }
}
