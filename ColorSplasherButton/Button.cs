using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using BIMOnePanelInformation;

namespace ColorSplasher {
    [Transaction(TransactionMode.Manual)]
    public class Button : IExternalApplication {
        const string bimOneTabName = "BIM One";
        const string bimOneToolsPanel = "Tools";
        public Result OnStartup(UIControlledApplication application) 
        {
            BIMOneRibbon.CreateBIMOneRibbon(application);
            RibbonPanel toolsPanel = BIMOneRibbon.GetBIMOneToolsPanel(application);

            string assemblieFolder = Path.GetDirectoryName(typeof(Button).Assembly.Location);
            string commandPath = Path.Combine(assemblieFolder, "ColorSplasher.dll");

            PushButton pushButton = toolsPanel.AddItem(new PushButtonData(
                                    "Color Splasher",
                                    "Color\nSplasher",
                                    commandPath,
                                    "ColorSplasher.Command")) as PushButton;

            string LogoPath = Path.Combine(assemblieFolder, @"Resources\ColorSplasherLogo.gif");
            pushButton.LargeImage = new BitmapImage(new Uri(LogoPath));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) {
            return Result.Succeeded;
        }

    }
}
