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

            // create a transaction group
            using (TransactionGroup transGroup = new TransactionGroup(curDoc, "Convert Spec Level"))
            {
                // start the transaction group
                transGroup.Start();

                #region Floor Finish Update

                // get first floor annotation view & set as active view
                View curView = Utils.GetViewByNameContainsAndAssociatedLevel(curDoc, "Annotation", "First Floor");

                if (curView != null)
                {
                    uidoc.ActiveView = curView;
                }
                else
                {
                    TaskDialog.Show("Error", "No view found with name containing 'Annotation' and associated with 'First Floor'");
                    transGroup.RollBack();
                    return Result.Failed;
                }

                // create & start transaction for updating floor finish
                using (Transaction t = new Transaction(curDoc, "Update Floor Finish"))
                {
                    // start the first transaction
                    t.Start();

                    // change the flooring for the specified rooms per the selected spec level
                    List<string> listUpdatedRooms = Utils.UpdateFloorFinishInActiveView(curDoc, selectedSpecLevel);

                    // create a list of the rooms updated
                    string listRooms;
                    if (listUpdatedRooms.Count == 1)
                    {
                        listRooms = listUpdatedRooms[0];
                    }
                    else if (listUpdatedRooms.Count == 2)
                    {
                        listRooms = $"{listUpdatedRooms[0]} and {listUpdatedRooms[1]}";
                    }
                    else
                    {
                        listRooms = string.Join(", ", listUpdatedRooms.Take(listUpdatedRooms.Count - 1)) + $", and {listUpdatedRooms.Last()}";
                    }

                    // notify the user
                    TaskDialog tdFloorUpdate = new TaskDialog("Complete");
                    tdFloorUpdate.MainIcon = Icon.TaskDialogIconInformation;
                    tdFloorUpdate.Title = "Spec Conversion";
                    tdFloorUpdate.TitleAutoPrefix = false;
                    tdFloorUpdate.MainContent = $"Flooring was changed at {listRooms} per the specified spec level.";
                    tdFloorUpdate.CommonButtons = TaskDialogCommonButtons.Close;

                    TaskDialogResult tdFloorUpdateSuccess = tdFloorUpdate.Show();

                    // commit the transaction
                    t.Commit();                        
                }

                #endregion

                #region Door Updates

                // get the door schedule & set it as the active view
                View curSched = Utils.GetScheduleByNameContains(curDoc, "Door Schedule");

                if (curSched != null)
                {
                    uidoc.ActiveView = curSched;
                }
                else
                {
                    TaskDialog.Show("Error", "No Door Schedule found");
                    transGroup.RollBack();
                    return Result.Failed;
                }

                //create & start transaction for updating doors
                using (Transaction t = new Transaction(curDoc, "Update Doors"))
                    {
                        // start the second transaction
                        t.Start();

                        // update front door type
                        Utils.UpdateFrontDoorType(curDoc, selectedSpecLevel);

                        // update rear door type
                        Utils.UpdateRearDoorType(curDoc, selectedSpecLevel);

                        // notify the user
                        TaskDialog tdDrUpdate = new TaskDialog("Complete");
                        tdDrUpdate.MainIcon = Icon.TaskDialogIconInformation;
                        tdDrUpdate.Title = "Spec Conversion";
                        tdDrUpdate.TitleAutoPrefix = false;
                        tdDrUpdate.MainContent = "The front and rear doors were replaced per the specified spec level.";
                        tdDrUpdate.CommonButtons = TaskDialogCommonButtons.Close;

                        TaskDialogResult tdDrUpdateSuccess = tdDrUpdate.Show();

                        // commit the transaction
                        t.Commit();
                    }

                #endregion

                #region Cabinet Updates

                // get the Interior Elevations sheet & set it as the active view
                ViewSheet sheetIntr = Utils.GetViewSheetByName(curDoc, "Interior Elevations");

                if (sheetIntr != null)
                {
                    uidoc.ActiveView = sheetIntr;
                }
                else
                {
                    TaskDialog.Show("Error", "No Interior Elevation sheet found");
                    transGroup.RollBack();
                    return Result.Failed;
                }

                // create & start transaction for updating cabinets
                using (Transaction t = new Transaction(curDoc, "Update Cabinets"))
                {
                    // start the third transaction
                    t.Start();

                    // change the upper cabinet height per the selected spec level

                    // change the microwave cabinet height per the selected MW Cabinet height

                    // add/remove the Ref Sp cabinet

                    // raise/lower the backsplash height
                    // add/remove full backsplash

                    // notify the user
                    // upper cabinets were revised per the selected spec level
                    // backsplash height was raised/lowered per the selected spec level

                    // commit the transaction
                    t.Commit();
                }

                #endregion

                #region First Floor Electrical Updates

                // get all views with Electrical in the name & associated with the First Floor
                List<View> firstFloorElecViews = Utils.GetAllViewsByNameContainsAndAssociatedLevel(curDoc, "Electrical", "First Floor");

                // get the first view in the list and set it as the active view
                if (firstFloorElecViews.Any())
                {
                    uidoc.ActiveView = firstFloorElecViews.First();
                }
                else
                {
                    TaskDialog.Show("Error", "No Electrical views found for First Floor");
                    transGroup.RollBack();
                    return Result.Failed;
                }

                // start the transaction for first floor electrical updates
                using (Transaction t = new Transaction(curDoc, "Update First Floor Electrical"))
                {
                    // start the fourth transaction
                    t.Start();

                    // replace the light fixtures in the specified rooms per the selected spec level
                    Utils.UpdateLightingFixturesInActiveView(curDoc, selectedSpecLevel);

                    // add/remove the sprinkler outlet at Garage

                    // loop through all the views & add/remove the clg fan note & sprinkler outlet note
                    foreach (View curElecView in firstFloorElecViews)
                    {
                        // add/remove ceiling fan note

                        // add/remove sprinkler outlet note                        
                    }

                    // notify the user
                    // first floor electrical plan was updated per the selected spec level

                    // commit the transaction
                    t.Commit();
                }

                #endregion

                #region Second Floor Electrical Updates

                // Get all views with Electrical in the name & associated with the Second Floor
                List<View> secondFloorElecViews = Utils.GetAllViewsByNameContainsAndAssociatedLevel(curDoc, "Electrical", "Second Floor");

                // Null check (exit if no views found)
                if (secondFloorElecViews.Any())
                {
                    // Get the first view in the list and set it as the active view
                    uidoc.ActiveView = secondFloorElecViews.First();

                    // Start the transaction for second floor electrical updates
                    using (Transaction t = new Transaction(curDoc, "Update Second Floor Electrical"))
                    {
                        t.Start();

                        // replace the light fixtures in the specified rooms per the selected spec level

                        // loop through all the views & add/remove the clg fan note
                        foreach (View elecView in secondFloorElecViews)
                        {
                            // add/remove ceiling fan note
                        }

                        // notify the user
                        // second floor electrical plan was updated per the selected spec level

                        // commit the transaction
                        t.Commit();
                    }
                }

                #endregion

                transGroup.Assimilate();
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
