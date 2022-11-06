using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SmartTag
{
    public static class NonFamilyChecking
    {
        public static bool IsDocumentFamily(this Document doc)
        {
            if (doc.IsFamilyDocument == true)
            {
                TaskDialog nonFamilyCheck = new TaskDialog("Ошибка")
                {
                    MainIcon = TaskDialogIcon.TaskDialogIconError,
                    MainInstruction = "Данная команда предназначена для выполнения в проекте. Запуск команды в семействе не предусмотрен. Операция не будет выполнена",
                    Title = "Ошибка",
                    TitleAutoPrefix = false                   
                };
                nonFamilyCheck.Show();
                return true;
            }
            return false;
        }
    }
}
