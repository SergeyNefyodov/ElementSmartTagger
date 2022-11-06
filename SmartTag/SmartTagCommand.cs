using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SmartTag

{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class SmartTagCommand : IExternalCommand
    {
        static AddInId appId = new AddInId(new Guid("E0EB4A49-E79B-49E1-BBA3-B5BE6165D6E3"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            if (doc.IsDocumentFamily())
                return Result.Failed;
            TagForm tagForm = new TagForm(commandData);
            tagForm.ShowDialog();
            return Result.Succeeded;
        }
    }
}
