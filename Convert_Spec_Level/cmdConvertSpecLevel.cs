using Autodesk.Revit.DB.Architecture;
using SandBox.Classes;
using SandBox.Common;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdConvertSpecLevel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // launch the form to get user input
            frmConvertSpecLevel curForm = new frmConvertSpecLevel()
            {
                Topmost = true,
            };

            curForm.ShowDialog();

            // check if user clicked OK
            if (curForm.DialogResult != true)
            {
                return Result.Cancelled;
            }

            // get user input from the form
            string selectedClient = curForm.GetSelectedClient();
            string selectedSpecLevel = curForm.GetSelectedSpecLevel();
            string selectedMWCabHeight = curForm.GetSelectedMWCabHeight();

            // create a varibale for the Floor Finish value
            string valueFloorFinish = "";

            // get the Family room
            List<Room> familyRooms = Utils.GetRoomByNameContains(curDoc, "Family");

            if (familyRooms.Count == 0)
            {
                TaskDialog.Show("Error", "No Family rooms found in the project.");
            }
            // set floor finish value based on spec level
            else
            {
                if (selectedSpecLevel == "Complete Home")
                {
                    valueFloorFinish = "Carpet";
                }
                else if (selectedSpecLevel == "Complete Home Plus")
                {
                    valueFloorFinish = "HS";
                }
                else
                {
                    TaskDialog.Show("Error", "Invalid Spec Level selected.");
                    return Result.Failed;
                }
            }

            using (Transaction t = new Transaction(curDoc, "Convert Spec Level"))
            {
                t.Start();

                #region Floor Plan Updates
                // set the first floor as the active view

                // change the flooring per the selected spec level

                // notify the user
                // flooring was changed at (list rooms) per the selected spec level

                #endregion

                #region Door Updates

                // set the door schedule as the active view

                // change front door type (will it update the door in all design options?)
                // search for door from Covered Porch

                // change rear door type (how to find it; width + Exterior Entry description?) 

                // notify the user (verify swing parameter doesn't change)
                // front and rear doors were changed per the selected spec level

                #endregion

                #region Cabinet Updates

                // set the Interior Elevations sheet as the active view

                // change the upper cabinet height per the selected spec level

                // change the microwave cabinet height per the selected MW Cabinet height

                // add/remove the Ref Sp cabinet

                // raise/lower the backsplash height
                // add/remove full backsplash

                // notify the user
                // upper cabinets were revised per the selected spec level
                // backsplash height was raised/lowered per the selected spec level

                #region Electrical Plan Updates

                // set the first floor electrical plan as the active view

                // change light fixtures in specified rooms on first floor

                // add/remove the clg fan note

                // add/remove the sprinkler outlet at the Garage
                // add/remove outlet note
                // add/remove dimension (can it be located 5' from the corner?)

                // notify user
                // light fixtures were changed in (list rooms) per the selected spec level
                // Sprinkler outlet was added/removed at the Garage per the selected spec level

                // set the second floor as the active view

                // change light fixtures in specified rooms on second floor

                // add/remove the clg fan note

                // notify user
                // light fixtures were changed in (list rooms) per the selected spec level

                #endregion

                #region Dialogs

                // show a dialog to notify the user that the spec level conversion is complete
                // message should include client name & spec level

                // set the appropriate checklist legend for the selected client and spec level as the actuive view

                #endregion

                t.Commit();
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
