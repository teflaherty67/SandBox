﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SandBox.Classes;
using SandBox.Common;
using System.Linq;
using System.Windows.Controls;

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
            List<ViewSchedule> allNewSchedules = Utils.GetAllSchedules(curDoc);

            // create a variable for parameter name            
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
                using (Transaction t3 = new Transaction(curDoc, "Add Shared Parameter"))
                {
                    // start the transaction
                    t3.Start();                    

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
                    t3.Commit();
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
            using (Transaction t4 = new Transaction(curDoc, "Update Parameter Value"))
            {
                // start the transaction
                t4.Start();

                // get current parameter binding
                BindingMap bindingMap = curDoc.ParameterBindings;
                ElementBinding curBinding = bindingMap.get_Item(paramDef) as ElementBinding;

                // edit the binding to include schedules
                if (curBinding != null)
                {
                    // check if schedules are already included
                    CategorySet catSet = curBinding.Categories;
                    Category catSchedule = curDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Schedules);

                    // if Schedules not included...
                    if (!catSet.Contains(catSchedule))
                    {
                        // add Schedules to the current category set
                        catSet.Insert(catSchedule);

                        // create new binding with updated category set
                        InstanceBinding newBinding = curDoc.Application.Create.NewInstanceBinding(catSet);

                        // replace existing binding with new binding
                        curDoc.ParameterBindings.ReInsert(paramDef, newBinding, GroupTypeId.IdentityData);
                    }
                }

                // loop through all the schedules & set the parameter value
                foreach (ViewSchedule curSchedule in allNewSchedules)
                {
                    if (curSchedule != null)
                    {
                        if(curSchedule.Name.Contains("Elevation"))
                        {
                            // extract the elevation name
                            string[] partsElev = curSchedule.Name.Split('-');
                            string partsName = partsElev[1].Trim();
                            string elevName = partsName.Substring(0, 11);

                            // set the parameter value
                            curSchedule.LookupParameter(paramName).Set(elevName);
                        }
                        else
                        {
                            // set the parameter value
                            curSchedule.LookupParameter(paramName).Set("Shared");
                        }
                    }
                }               

                // commit the transaction
                t4.Commit();
            }

            // inform the user that the browser organization was updated
            //
            // create variable for website URL
            string urlSchedOrg = "https://lifestyle-usa-design.atlassian.net/wiki/spaces/MFS/pages/720897/Browser+Organization+Schedules";

            // pass the URL to the form
            frmSchedOrg curForm = new frmSchedOrg(urlSchedOrg);

            // launch the form
            curForm.ShowDialog();

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
