using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SandBox.Classes;
using SandBox.Common;
using System.Windows;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdFamilyUpdate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // Step 1: Get all families and filter for your target patterns
            List<Family> allFamilies = Utils.GetAllFamilies(curDoc);
            var targetFamilies = allFamilies.Where(f =>
                f.Name.StartsWith("EL-Wall Base") ||
                f.Name.StartsWith("EL-No Base") ||
                f.Name.StartsWith("LT-No Base")).ToList();

            // Step 2: Separate base families from duplicates
            var baseFamilies = targetFamilies.Where(f =>
                f.Name == "EL-Wall Base" ||
                f.Name == "EL-No Base" ||
                f.Name == "LT-No Base").ToList();

            var duplicateFamilies = targetFamilies.Where(f =>
                !baseFamilies.Contains(f)).ToList();

            // Step 3: Group duplicates by their base name
            var duplicateGroups = duplicateFamilies.GroupBy(f => {
                if (f.Name.StartsWith("EL-Wall Base")) return "EL-Wall Base";
                if (f.Name.StartsWith("EL-No Base")) return "EL-No Base";
                if (f.Name.StartsWith("LT-No Base")) return "LT-No Base";
                return "Unknown";
            }).ToList();

            // Create dictionary to organize instances by base family
            Dictionary<string, List<Element>> instancesByGroup = new Dictionary<string, List<Element>>();

            // loop through each group of duplicates
            foreach (var curGroup in duplicateGroups)
            {
                string baseFamilyName = curGroup.Key;
                instancesByGroup[baseFamilyName] = new List<Element>(); // Changed to Element

                foreach (Family duplicateFamily in curGroup)
                {
                    // get all instances of the current duplicate family
                    Utils.GetFamilyInstances(curDoc, duplicateFamily, out List<FamilyInstance> instances);
                    //Utils.GetAllLegendComponents(curDoc, duplicateFamily, out List<LegendComponent> legendComponents);

                    // add the instances to this group's list
                    instancesByGroup[baseFamilyName].AddRange(instances.Cast<Element>());
                    //instancesByGroup[baseFamilyName].AddRange(legendComponents.Cast<Element>());
                }
            }

            // create variables for tracking progress
            int totalInstances = instancesByGroup.Values.Sum(list => list.Count);
            int curInstanceCount = 0;

            // create an empty list for reporting purposes
            List<string> missingTypes = new List<string>();

            // try-catch statement for transaction group
            try
            {
                // create a transaction group to handle all changes
                using (TransactionGroup tGroup = new TransactionGroup(curDoc, "Family Update"))
                {
                    // create the first transaction for the group
                    using (Transaction t = new Transaction(curDoc, "Consolidate Families"))
                    {
                        // start the transaction group
                        tGroup.Start();

                        #region Consolidate Families

                        // intiate the progress bar for the consolidation process
                        ProgressBarHelper consolidateProgressHelper = new ProgressBarHelper();
                        consolidateProgressHelper.ShowProgress(totalInstances);

                        try
                        {
                            // start the first transaction
                            t.Start();

                            foreach (var group in instancesByGroup)
                            {
                                string baseFamilyName = group.Key;              // "EL-Wall Base", etc.
                                List<Element> instances = group.Value;          // Changed to Element

                                // loop through each instance in the current group
                                foreach (Element curElement in instances)      // Changed to Element
                                {
                                    // Check for cancellation
                                    if (consolidateProgressHelper.IsCancelled())
                                    {
                                        // close progress bar
                                        consolidateProgressHelper.CloseProgress();

                                        // rollback the transaction
                                        t.RollBack();
                                        tGroup.RollBack();
                                        return Result.Cancelled;
                                    }

                                    curInstanceCount++;
                                    consolidateProgressHelper.UpdateProgress(curInstanceCount, $"Consolidating {curInstanceCount} of {totalInstances}");

                                    // Handle both FamilyInstance and LegendComponent
                                    FamilySymbol curType = null;
                                    ElementId typeId = curElement.GetTypeId();

                                    if (typeId != ElementId.InvalidElementId)
                                    {
                                        curType = curDoc.GetElement(typeId) as FamilySymbol;
                                    }

                                    if (curType == null) continue; // Skip if we can't get the symbol

                                    // find the target base family
                                    Family targetFamily = baseFamilies.FirstOrDefault(f => f.Name == baseFamilyName);

                                    // null check for target family
                                    if (targetFamily == null)
                                    {
                                        Utils.TaskDialogError("Error", "Family Updates", $"Target family '{baseFamilyName}' not found.");
                                        continue; // Skip this instance - leave it unchanged
                                    }

                                    // Get the type name you're looking for
                                    string typeName = curType.Name;

                                    // Find matching type in target family using your existing method
                                    FamilySymbol targetType = Utils.GetFamilySymbolByName(curDoc, targetFamily.Name, typeName);

                                    if (targetType != null)
                                    {
                                        // Change the instance to use this target type
                                        curElement.ChangeTypeId(targetType.Id);
                                    }
                                    else
                                    {
                                        // Log the missing type
                                        string missingInfo = $"Family: {targetFamily.Name}, Missing Type: {typeName}";
                                        missingTypes.Add(missingInfo);
                                        // Skip this instance - leave it unchanged
                                    }
                                }
                            }

                            // Delete unused duplicate families (moved outside the instance loops)
                            foreach (var duplicateFamily in duplicateFamilies)
                            {
                                // Check if this family still has instances
                                Utils.GetFamilyInstances(curDoc, duplicateFamily, out List<FamilyInstance> remainingInstances);
                                //Utils.GetAllLegendComponents(curDoc, duplicateFamily, out List<LegendComponent> remainingLegendComponents);

                                //if (remainingInstances.Count == 0 && remainingLegendComponents.Count == 0)
                                //{
                                //    // Family is not used, safe to delete
                                //    curDoc.Delete(duplicateFamily.Id);
                                //}
                            }

                            // commit the transaction
                            t.Commit();
                        }
                        finally
                        {
                            // close the progress bar
                            consolidateProgressHelper.CloseProgress();
                        }

                        #endregion

                        // commit the transaction group
                        tGroup.Assimilate();
                    }
                }
            }

            // handle any exceptions that may occur
            catch (Exception ex)
            {
                Utils.TaskDialogError("Error", "Family Updates", $"An error occurred during the electrical family updates: {ex.Message}");
                return Result.Failed;
            }

            // report missing types to user
            if (missingTypes.Count > 0)
            {
                string missingTypesMessage = "The following types were not found in the target families:\n\n";
                missingTypesMessage += string.Join("\n", missingTypes);

                Utils.TaskDialogError("Error", "Family Updates", missingTypesMessage);
            }

            // report success to user
            Utils.TaskDialogInformation("Information", "Family Updates", "The electrical families have been successfully consolidated.");

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
