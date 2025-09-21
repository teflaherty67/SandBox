using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SandBox.Classes;
using SandBox.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdUpdateSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // create a hashset to hold all renamed schedules
            HashSet<ElementId> modifiedScheduleIds = new HashSet<ElementId>();

            // list to hold schedules to rename
            List<ViewSchedule> schedsToRename = new List<ViewSchedule>();

            // variable for regex pattern to match old naming convention
            string oldPattern = @"[A-Za-z]-\d{2}/[A-Za-z]/[A-Za-z]/([A-Za-z]/)?\w";

            // get all the schedules in the project
            List<ViewSchedule> allCurSchedules = Utils.GetAllSchedules(curDoc);

            // loop through each schedule
            foreach (ViewSchedule curSched in allCurSchedules)
            {
                // check for old naming convention
                string schedName = curSched.Name;

                if (Regex.IsMatch(schedName, oldPattern))
                {
                    // add it to a list to rename
                    schedsToRename.Add(curSched);
                }
            }

            // create a transaction group to update the schedules
            using (TransactionGroup tgroup = new TransactionGroup(curDoc, "Update Schedules"))
            {
                // start the transaction group
                tgroup.Start();








                // assimilate the transaction group
                tgroup.Assimilate();
            }

            // notify the user


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
