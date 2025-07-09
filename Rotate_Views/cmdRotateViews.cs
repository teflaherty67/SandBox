using SandBox.Classes;
using SandBox.Common;
using System.Linq;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdRotateViews : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all plan views in the document
            List<ViewPlan> allPlanViews = Utils.GetAllPlanViews(curDoc);

            // alert the user if no plan views were found
            if (allPlanViews.Count == 0)
            {
                // Show final report
                TaskDialog tdNoViews = new TaskDialog("Complete");
                tdNoViews.MainIcon = Icon.TaskDialogIconInformation;
                tdNoViews.Title = "No Views Found";
                tdNoViews.TitleAutoPrefix = false;
                tdNoViews.MainContent = "No floor plans or ceiling plans found in the project.";
                tdNoViews.CommonButtons = TaskDialogCommonButtons.Close;

                TaskDialogResult tdSchedSuccessRes = tdNoViews.Show();
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            clsButtonData myButtonData = new clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData.Data;
        }
    }
}


