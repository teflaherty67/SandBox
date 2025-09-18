using SandBox.Classes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using SandBox.Common;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdSchedOrg : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all the schedules in the project
            List<ViewSchedule> allSchedules = Utils.GetAllSchedules(curDoc);

            // get the parameter definitions in the project
            DefinitionGroups defGroup = null;

            // create a variables for parameter group & name
            string groupName = "Identity";
            string paramName = "Elevation Designation";

            // access shared parameter file
            DefinitionFile defFile = curDoc.Application.OpenSharedParameterFile();

            // null check
            if (defFile == null)
            {
                Utils.TaskDialogError("Error", "Browser Organization", "No shared parameter file found");
                return Result.Failed;
            }

            // create a variable to hold parameter definition
            Definition paramDef = null;

            // loop through the groups in the shared parameter file
            foreach (DefinitionGroup group in defFile.Groups)
            {
                // find the group we want
                Definition curDef = group.Definitions.get_Item(paramName);
                if (curDef != null)
                {
                    paramDef = curDef;
                    break;
                }
            }

            // null check
            if (paramDef == null)
            {
                // notify user parameter not found
                Utils.TaskDialogError("Error", "Browser Organization", $"Parameter '{paramName}' not found");                
                return Result.Failed;
            }

            // check if the parameter exists in the project
            bool paramAdded = Utils.ParamAddedToFile(curDoc, paramName);

            // if not, create a transaction to add it
            if (!paramAdded)
            {
                using (Transaction t1 = new Transaction(curDoc, "Add Shared Parameter"))
                {
                    // start the transaction
                    t1.Start();                    

                    // assign the parameter to the Schedules category
                    // 
                    // create category set for Schedules
                    CategorySet catSet = curDoc.Application.Create.NewCategorySet();
                    catSet.Insert(curDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Schedules));

                    // Create instance binding with Identity Data parameter group
                    InstanceBinding binding = curDoc.Application.Create.NewInstanceBinding(catSet);

                    // Insert the binding
                    curDoc.ParameterBindings.Insert(paramDef, binding, GroupTypeId.IdentityData);

                    // commit the transaction
                    t1.Commit();
                }

                // inform the user that the parameter was added
                Utils.TaskDialogInformation("Success", "Browser Organization", $"Added '{paramName}' to the project & assigned it to Schedules.");
            }
            else
            {
                // inform the user that the parameter already exists
                Utils.TaskDialogInformation("Info", "Browser Organization", $"The parameter '{paramName}' already exists in the project.");
            }

            // start a transaction to update the parameter values
            using (Transaction t2 = new Transaction(curDoc, "Update Schedule Browser Organization"))
            {
                // start the transaction
                t2.Start();

                // get current parameter binding
                BindingMap bindingMap = curDoc.ParameterBindings;
                ElementBinding curBinding = bindingMap.get_Item(paramDef) as ElementBinding;

                // edit the binding to include schedules
                if (curBinding != null)
                {
                    // check if schedules are already included
                    CategorySet catSet = curBinding.Categories;
                }

                // loop through all the schedules & set the parameter value

                // create a new browser organization called "elevation"

                // set the parameters of the new browser organization

                // make the new browser organization current

                // commit the transaction
                t2.Commit();
            }

            // inform the user that the browser organization was updated
            Utils.TaskDialogInformation("Success", "Browser Organization", $"The Browser Organization for Schedules/Quantities has been update" +
                $"to organize by the '{paramName}' parameter.");

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
