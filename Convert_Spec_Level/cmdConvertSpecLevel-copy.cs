using Autodesk.Revit.DB.Architecture;
using SandBox.Classes;
using SandBox.Common;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdConvertSpecLevel_copy : IExternalCommand
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

                #region Light Fixture Updates

                // change LED to Ceilling fan in specified rooms
                Utils.UpdateLightingFixtures(curDoc, selectedSpecLevel);

                // remove/add clg fan note
                Utils.ManageClgFanNotes(curDoc, selectedSpecLevel);


                #endregion

                // create vaiable for Family
                Room roomFamily = familyRooms.FirstOrDefault();

                // get the Floor Finish parameter
                Parameter paramFloor = roomFamily.LookupParameter("Floor Finish");

                // set the value of Floor Finish parameter based on spec level
                if (paramFloor != null && !paramFloor.IsReadOnly)
                {
                    paramFloor.Set(valueFloorFinish);
                }
                else
                {
                    TaskDialog.Show("Error", "Floor Finish parameter not found or value is empty.");
                }

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
