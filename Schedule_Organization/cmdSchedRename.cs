using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SandBox.Classes;
using SandBox.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSchedRename : IExternalCommand
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

            // create a transaction to rename the schedules
            using (Transaction t1 = new Transaction(curDoc, "Rename Schedules"))
            {
                // start the transaction
                t1.Start();

                // loop through the list and rename
                foreach (ViewSchedule curSched in schedsToRename)
                {
                    // add to the hashset
                    modifiedScheduleIds.Add(curSched.Id);

                    // get the exisitng name
                    string curName = curSched.Name;

                    // extract elevation designation from current name using regex
                    Match elevMatch = Regex.Match(curName, oldPattern);
                    if (elevMatch.Success)
                    {
                        // get first character of the old pattern
                        string elevLetter = elevMatch.Value.Substring(0, 1); // Get first char of the matched pattern

                        // create the new pattern
                        string newPattern = "Elevation " + elevLetter;

                        // replace old pattern with new pattern
                        string newName = curName.Replace(elevMatch.Value, newPattern);

                        // rename the schedule
                        curSched.Name = newName;
                    }
                }

                // commit the transaction
                t1.Commit();
            }

            // get all the schedules again
            List<ViewSchedule> allNewSchedules = Utils.GetAllSchedules(curDoc);

            // create a list to hold schedule without the hyphen
            List<ViewSchedule> schedNeedsHyphen = new List<ViewSchedule>();

            // loop through each schedule
            foreach (ViewSchedule curSched in allNewSchedules)
            {
                
                // check for old naming convention
                string schedName = curSched.Name;
                if (schedName.Contains("Elevation") && !schedName.Contains(" - "))
                {
                    // add it to a list to rename
                    schedNeedsHyphen.Add(curSched);
                }
            }

            // create a transaction to rename the schedules
            using (Transaction t2 = new Transaction(curDoc, "Add Hyphen to Schedules"))
            {
                // start the transaction
                t2.Start();

                // loop through the list and rename
                foreach (ViewSchedule curSched in schedNeedsHyphen)
                {
                    // add to the hashset
                    modifiedScheduleIds.Add(curSched.Id);

                    // get the exisitng name
                    string curName = curSched.Name;

                    // insert hyphen before "Elevation"
                    string newName = curName.Replace("Elevation ", "- Elevation");

                    // rename the schedule
                    curSched.Name = newName;
                }

                // commit the transaction
                t2.Commit();
            }

            // notify the user of completion
            Utils.TaskDialogInformation("Success", "Rename Schedules", $"Renamed {modifiedScheduleIds.Count} schedules.");

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
