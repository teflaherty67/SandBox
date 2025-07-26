using Autodesk.Revit.DB;
using SandBox.Classes;
using SandBox.Common;

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
            Dictionary<string, List<FamilyInstance>> instancesByGroup = new Dictionary<string, List<FamilyInstance>>();

            // loop through each group of duplicates
            foreach (var curGroup in duplicateGroups)
            {
                string baseFamilyName = curGroup.Key;
                instancesByGroup[baseFamilyName] = new List<FamilyInstance>();

                foreach (Family duplicateFamily in curGroup)
                {
                    // get all instances of the current duplicate family
                    Utils.GetFamilyInstances(curDoc, duplicateFamily, out List<FamilyInstance> instances);
                    // add the instances to this group's list
                    instancesByGroup[baseFamilyName].AddRange(instances);
                }
            }

            // create variables for tracking progress
            int totalInstances = instancesByGroup.Values.Sum(list => list.Count);
            int curInstanceCount = 0;

            // Now you have organized batches:
            // instancesByGroup["EL-Wall Base"] = all instances to move to EL-Wall Base
            // instancesByGroup["EL-No Base"] = all instances to move to EL-No Base  
            // instancesByGroup["LT-No Base"] = all instances to move to LT-No Base

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
                                List<FamilyInstance> instances = group.Value;  // instances for this group

                                // loop through each instance in the current group
                                foreach (FamilyInstance curInstance in instances)
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

                                    // get the current instances family & type info
                                    Family curFam = curInstance.Symbol.Family;
                                    FamilySymbol curType = curInstance.Symbol;

                                    // find the target base family
                                    Family targetFamily = baseFamilies.FirstOrDefault(f => f.Name == baseFamilyName);

                                    // find the matchin type in the target family
                                    // get all types in the target family
                                    // look for a type that matches the current instance's type name
                                    // handle the case where no matching type is found

                                    // change the instance's type to the matching type in the target family
                                    // use curInstance.ChangeTypeId(targetType.Id);

                                    // error handling:
                                    // what if the target family does not have a matching type?
                                    // log issues for later review

                                }
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
                message = $"An error occurred during the family update: {ex.Message}";
                return Result.Failed;
            }

            // report success to user


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
