using System;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace SmartTag
{
    public class ElementSmartTaggerApp : IExternalApplication
    {
        static AddInId appId = new AddInId(new Guid("970B1784-EC06-4C1B-A038-EC84FDAD7C9A"));
        public Result OnStartup(UIControlledApplication app)
        {    
            string folderPath3 = @"C:\ProgramData\Autodesk\Revit\Addins\2022\"; 
            string dll = Path.Combine(folderPath3, @"PrintTools.dll");            
            RibbonPanel AdditionalTools = app.CreateRibbonPanel("Smart Tagging");           
            PushButton Tags = (PushButton)AdditionalTools.AddItem(new PushButtonData("Smart Tag", "Smart Tag", dll, "PrintTools.SmartTagCommand"));
            if (File.Exists(Path.Combine(folderPath3, "icons8-pantex-40.png")))
                Tags.LargeImage = new BitmapImage(new Uri(Path.Combine(folderPath3, "icons8-pantex-40.png"), UriKind.Absolute));           
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication app)
        {
            return Result.Succeeded;
        }
    }
}
