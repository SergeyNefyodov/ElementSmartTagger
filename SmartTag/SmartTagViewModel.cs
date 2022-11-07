using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;

namespace SmartTag
{
    internal class SmartTagViewModel
    {
        private static ExternalCommandData _commandData;
        private static UIDocument uidoc
        {
            get { return _commandData.Application.ActiveUIDocument; }
        }
       
        private static Document doc
        {
            get { return uidoc.Document; }
        }
        private static View view
        {
            get { return doc.ActiveView; }
        }
            
        private static Dictionary<BuiltInCategory, BuiltInCategory> usedCategories = new Dictionary<BuiltInCategory, BuiltInCategory>()
        {
            {BuiltInCategory.OST_DuctTags, BuiltInCategory.OST_DuctCurves },
            {BuiltInCategory.OST_DuctTerminalTags, BuiltInCategory.OST_DuctTerminal },
            {BuiltInCategory.OST_PipeTags, BuiltInCategory.OST_PipeCurves},
            {BuiltInCategory.OST_PipeAccessoryTags, BuiltInCategory.OST_PipeAccessory },
            {BuiltInCategory.OST_MechanicalEquipmentTags, BuiltInCategory.OST_MechanicalEquipment },
            {BuiltInCategory.OST_WallTags, BuiltInCategory.OST_Walls},
            {BuiltInCategory.OST_FloorTags, BuiltInCategory.OST_Floors },
            {BuiltInCategory.OST_GenericModelTags, BuiltInCategory.OST_GenericModel },
            {BuiltInCategory.OST_WindowTags, BuiltInCategory.OST_Windows },
            {BuiltInCategory.OST_DoorTags ,BuiltInCategory.OST_Doors },
        };
        public List<Category> MarkTypes { get; set; }
        public List<FamilySymbol> symbolsTags 
        {
            get
            {
                if (SelectedCategory == null)
                { return null; }
                else
                {
                    var _symbolsDucts = new FilteredElementCollector(doc)
                .OfCategoryId(SelectedCategory.Id)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .ToList();
                    return _symbolsDucts;
                }
            }
        }       
        public Category SelectedCategory { get; set; }
        public FamilySymbol SelectedTag { get; set; }        

        public string MinimumLength { get; set; }
        public string J { get; set; } = "4";
        public string K { get; set; } = "4";
        public bool IgnoreVertical { get; set; }
        public bool UseSelecting { get; set; }
        public bool HasLeader { get; set; } = true;
        public bool ManyReferences { get; set; } = false;
        public bool AutoCreation { get; set; } = false;
        public DelegateCommand CreateTags { get; }
        public SmartTagViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;                    
            List<Category> markTypes = new List<Category>();
            foreach (var bic in usedCategories.Keys)
            {
                markTypes.Add(Category.GetCategory(doc, bic));
            }
            markTypes.Add(Category.GetCategory(doc, BuiltInCategory.OST_MultiCategoryTags));
            MarkTypes = markTypes;
            CreateTags = new DelegateCommand(chosenCommand);
        }
        private void chosenCommand()
        {
            if (ManyReferences)
            {
                createMultiTag();
            }
            else
            {
                createTags();
            }
        }
        private void createTags()
        {            
            if (SelectedCategory == null)
            {
                TaskDialog errorDialog = new TaskDialog("Ошибка")
                {
                    MainIcon = TaskDialogIcon.TaskDialogIconError,
                    MainInstruction = "Вы не выбрали категорию для маркировки",
                };
                errorDialog.Show();
                return;
            }
            if (SelectedTag == null)
            {
                TaskDialog errorDialog = new TaskDialog("Ошибка")
                {
                    MainIcon = TaskDialogIcon.TaskDialogIconError,
                    MainInstruction = "Вы не выбрали марку",
                };
                errorDialog.Show();
                return;
            }            
            int scale = view.Scale;
            List<Element> workingList = new List<Element>();
            int minLength = 0;
            if (!string.IsNullOrEmpty(MinimumLength))
            { int.TryParse(MinimumLength, out minLength); }// парсим длину воздуховодов, которые надо игнорировать
            using (Transaction t = new Transaction(doc, "Умная маркировка"))
            {
                t.Start();
                ElementId tagSymbolId = SelectedTag.Id;
                int.TryParse(J, out int j);
                int.TryParse(K, out int k);// это числа, которые умножаются на масштаб и показывают, как мы двигаем марку. Выбираются подбором                
                if (SelectedTag != null)
                {
                    workingList = MakeWorkingList();                                     
                }                
                foreach (Element el in workingList)
                {
                    bool b = false;
                    foreach (ElementId ID in el.GetDependentElements(new ElementOwnerViewFilter(view.Id)))
                    {
                        if (doc.GetElement(ID) is IndependentTag) // если элемент замаркирован на этом виде, мы его не маркируем
                            b = true;
                    }
                    if (b)
                        continue;
                    Parameter pLength = el.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH); // если мы не поставили галочку, n будет равно 0
                    if (pLength != null)
                    {
                        if (pLength.AsDouble() * 304.8 < minLength) 
                            continue;
                    }
                    if (el is Duct && IgnoreVertical == true) // проверяем вертикальность
                    {
                        XYZ point1 = ((el.Location as LocationCurve).Curve as Line).Tessellate()[0];
                        XYZ point2 = ((el.Location as LocationCurve).Curve as Line).Tessellate()[1];
                        if (Abs(point1.Z - point2.Z) > (50 / 304.8)) 
                            continue;
                    }
                    Reference myRef = new Reference(el);
                    Element element = doc.GetElement(myRef);
                    if (view is View3D != false)                    
                    {
                        k = 0;
                        j *= 3; // на 3D видах с марками немножко дичь -- там оси по другому направлены
                    }
                    XYZ centerPoint = (element.get_BoundingBox(view).Max + element.get_BoundingBox(view).Min) / 2;
                    if (element.Location is LocationPoint)
                        centerPoint = (element.Location as LocationPoint).Point;
                    IndependentTag newTag = IndependentTag.Create(doc, tagSymbolId, view.Id, myRef, true, TagOrientation.Horizontal, centerPoint);
                    newTag.TagHeadPosition = centerPoint - new XYZ(scale * 12 / 304.8, scale * 5 / 304.8, 0);
                    newTag.LeaderEndCondition = LeaderEndCondition.Free;
                    newTag.LeaderEnd = centerPoint; // создали марку
                    doc.Regenerate();
                    double d = Abs(newTag.get_BoundingBox(view).Max.X - newTag.get_BoundingBox(view).Min.X) / 2;
                    if (!HasLeader)
                        d = 0;
                    if (AutoCreation)
                    {
                        Outline outline = new Outline(newTag.get_BoundingBox(view).Min, newTag.get_BoundingBox(view).Max + new XYZ(-scale * 2 / 304.8, -scale * 2 / 304.8, 15)); // создаём объект
                        var filter = new BoundingBoxIntersectsFilter(outline); // создаём объект, у которого будем считать пересечки с другими элементами
                        var fic2 = new FilteredElementCollector(doc, view.Id);
                        int i = fic2.WherePasses(filter).Count(),
                            i1 = 0,
                            i2 = 0,
                            i3 = 0; // а сюда будем записывать число пересечек
                        newTag.TagHeadPosition = centerPoint - new XYZ(scale * (-j) / 304.8 - d, scale * (-k) / 304.8, 0);
                        doc.Regenerate();
                        Outline outline1 = new Outline(newTag.get_BoundingBox(view).Min + new XYZ(scale * 2 / 304.8, scale * 2 / 304.8, 0), newTag.get_BoundingBox(view).Max + new XYZ(0, 0, 15));
                        var filter1 = new BoundingBoxIntersectsFilter(outline1);
                        var fic3 = new FilteredElementCollector(doc, view.Id);
                        i1 = fic3.WherePasses(filter1).Count(); // а сюда будем записывать число пересечек
                        newTag.TagHeadPosition = centerPoint - new XYZ(scale * k / 304.8 + d, scale * (-j) / 304.8, 0);
                        doc.Regenerate();
                        var outline2 = new Outline(newTag.get_BoundingBox(view).Min + new XYZ(-scale * 2 / 304.8, scale * 2 / 304.8, 0), newTag.get_BoundingBox(view).Max + new XYZ(0, 0, 15));
                        var filter2 = new BoundingBoxIntersectsFilter(outline2);
                        var fic4 = new FilteredElementCollector(doc, view.Id);
                        i2 = fic4.WherePasses(filter2).Count(); // а сюда будем записывать число пересечек
                        newTag.TagHeadPosition = centerPoint - new XYZ(scale * (-k) / 304.8 - d, scale * j / 304.8, 0);
                        doc.Regenerate();
                        var outline3 = new Outline(newTag.get_BoundingBox(view).Min + new XYZ(scale * 2 / 304.8, -scale * 2 / 304.8, 0), newTag.get_BoundingBox(view).Max + new XYZ(0, 0, 15));
                        var filter3 = new BoundingBoxIntersectsFilter(outline3);
                        var fic5 = new FilteredElementCollector(doc, view.Id);
                        i3 = fic5.WherePasses(filter3).Count(); // а сюда будем записывать число пересечек
                        if (view is View3D == false)
                        {
                            if (i <= i1 && i <= i2 && i <= i3)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * k / 304.8 + d, scale * j / 304.8, 0);
                            else if (i1 <= i && i1 <= i2 && i1 <= i3)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * (-k) / 304.8 - d, scale * (-j) / 304.8, 0);
                            else if (i2 <= i1 && i2 <= i && i2 <= i3)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * j / 304.8 + d, scale * (-k) / 304.8, 0);
                            else if (i3 <= i1 && i3 <= i2 && i3 <= i)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * (-j) / 304.8 - d, scale * k / 304.8, 0);
                        }
                        else
                        {
                            if (i <= i1 && i <= i2 && i <= i3)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * j / 304.8 + d, scale * k / 304.8, 0);
                            else if (i1 <= i && i1 <= i2 && i1 <= i3)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * (-j) / 304.8 - d, scale * (-k) / 304.8, 0);
                            else if (i2 <= i1 && i2 <= i && i2 <= i3)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * k / 304.8, scale * (-j) / 304.8 - d, 0);
                            else if (i3 <= i1 && i3 <= i2 && i3 <= i)
                                newTag.TagHeadPosition = centerPoint - new XYZ(scale * (-k) / 304.8, scale * j / 304.8 + d, 0);
                        }
                    }
                    else
                    {
                        newTag.TagHeadPosition = centerPoint + new XYZ(scale * j / 304.8, scale * k / 304.8, 0);
                    }                    
                    newTag.HasLeader = HasLeader;
                }
                t.Commit();
            }
            RaiseCloseRequest();
        }

        private List<Element> MakeWorkingList()
        {
            List<Element>  workingList = new List<Element> ();
            var tagBic = (BuiltInCategory)SelectedTag.Category.Id.IntegerValue;
            var bic = usedCategories[tagBic];
            if (!UseSelecting)
            {                
                var collector_ = new FilteredElementCollector(doc, view.Id);
                List<Element> elems = collector_.OfCategory(bic).ToElements().ToList();
                workingList = elems;
            }
            else if (SelectedTag.Category.Id == Category.GetCategory(doc, BuiltInCategory.OST_MultiCategoryTags).Id && UseSelecting)
            {
                var referenceList = uidoc.Selection.GetElementIds();
                foreach (var reference in referenceList)
                {
                    Element element = doc.GetElement(reference);
                    workingList.Add(doc.GetElement(reference));
                    UseSelecting = false;
                }
            }            
            else
            {
                var referenceList = uidoc.Selection.GetElementIds();
                foreach (var reference in referenceList)
                {
                    Element element = doc.GetElement(reference);
                    if (element.Category.Id.IntegerValue == (int)bic)
                    {
                        workingList.Add(doc.GetElement(reference));
                    }
                }
            }
            return workingList;
        }

        private void createMultiTag()
        {            
            int scale = view.Scale;
            var tagSymbolId = SelectedTag.Id;
            using (TransactionGroup transactionGroup = new TransactionGroup(doc, "Маркировка нескольких элементов"))
            {
                transactionGroup.Start();
                using (Transaction t = new Transaction(doc, "Маркировка нескольких элементов"))
                {                    
                    while (1 < 2)
                    {
                        try
                        {
                            t.Start();
                            IList<Reference> references = uidoc.Selection.PickObjects(ObjectType.Element, "Выберите элементы для создания марки");
                            Element firstElement = doc.GetElement(references[0]);
                            XYZ centerPoint = null;
                            if (firstElement.Location is LocationPoint)
                            {
                                centerPoint = (firstElement.Location as LocationPoint).Point;
                            }
                            else if (firstElement.Location is LocationCurve)
                            {
                                centerPoint = ((firstElement.Location as LocationCurve).Curve.Tessellate()[0] + (firstElement.Location as LocationCurve).Curve.Tessellate()[1]) / 2;
                            }
                            IndependentTag newTag = IndependentTag.Create(doc, tagSymbolId, view.Id, references[0], HasLeader, TagOrientation.Horizontal, centerPoint);
                            newTag.TagHeadPosition = centerPoint + new XYZ(scale * 12 / 304.8, scale * 5 / 304.8, 0);
                            if (HasLeader)
                            {
                                newTag.LeaderEndCondition = LeaderEndCondition.Free;
                                newTag.LeaderEnd = centerPoint;
                            }
                            if (references.Count > 1)
                            {
                                references.RemoveAt(0);
                                for (int i = references.Count-1; i>0; i--)
                                {
                                    Element element = doc.GetElement(references[i]);
                                    if (element.Category.Id != firstElement.Category.Id)
                                        references.RemoveAt(i);

                                }
                                if (references.Count > 0)
                                {
                                    newTag.AddReferences(references);
                                    newTag.TagHeadPosition = centerPoint + new XYZ(scale * 20 / 304.8, scale * 5 / 304.8, 0);
                                }
                            }
                            t.Commit();
                        }
                        catch 
                        {                            
                            transactionGroup.Assimilate();
                            break;
                        }
                    }                    
                }                
            }
        }
        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
