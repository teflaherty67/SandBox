using SandBox.Classes;
using SandBox.Common;
using System.Linq;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDeleteVTs : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all the view templates in the project
            List<View> curVTs = Utils.GetAllViewTemplates(curDoc);

            // get views by current view template name
            List<View> viewsEnlargedPlans = Utils.GetViewsByViewTemplateName(curDoc, "01-Enlarged Plans");
            List<View> viewsAnnoPlans = Utils.GetViewsByViewTemplateName(curDoc, "01-Floor Annotations");
            List<View> viewsDimPlans = Utils.GetViewsByViewTemplateName(curDoc, "01-Floor Dimensions");
            List<View> viewsKeyPlans = Utils.GetViewsByViewTemplateName(curDoc, "01-Key Plans");
            List<View> viewsExtrElevs = Utils.GetViewsByViewTemplateName(curDoc, "02-Elevations");
            List<View> viewsKeyElevs = Utils.GetViewsByViewTemplateName(curDoc, "02-Key Elevations");
            List<View> viewsPorchElevs = Utils.GetViewsByViewTemplateName(curDoc, "02-Porch Elevations");
            List<View> viewsRoofPlans = Utils.GetViewsByViewTemplateName(curDoc, "03-Roof Plan");
            List<View> viewsSections = Utils.GetViewsByViewTemplateName(curDoc, "04-Sections");
            List<View> viewsSections3_8 = Utils.GetViewsByViewTemplateName(curDoc, "04-Sections_3/8\"");
            List<View> viewsCabinetPlans = Utils.GetViewsByViewTemplateName(curDoc, "05-Cabinet Layout Plans");
            List<View> viewsIntrElevs = Utils.GetViewsByViewTemplateName(curDoc, "05-Interior Elevations");
            List<View> viewsElecPlans = Utils.GetViewsByViewTemplateName(curDoc, "06-Electrical Plans");
            List<View> viewsEnlargedFormPlans = Utils.GetViewsByViewTemplateName(curDoc, "07-Enlarged Form/Foundation Plans");
            List<View> viewsFormPlans = Utils.GetViewsByViewTemplateName(curDoc, "07-Form/Foundation Plans");

            // create list of all views getting new view templates
            List<View> allViewsToUpdate = new List<View>();

            allViewsToUpdate.AddRange(viewsEnlargedPlans);
            allViewsToUpdate.AddRange(viewsAnnoPlans);
            allViewsToUpdate.AddRange(viewsDimPlans);
            allViewsToUpdate.AddRange(viewsKeyPlans);
            allViewsToUpdate.AddRange(viewsExtrElevs);
            allViewsToUpdate.AddRange(viewsKeyElevs);
            allViewsToUpdate.AddRange(viewsPorchElevs);
            allViewsToUpdate.AddRange(viewsRoofPlans);
            allViewsToUpdate.AddRange(viewsSections);
            allViewsToUpdate.AddRange(viewsSections3_8);
            allViewsToUpdate.AddRange(viewsCabinetPlans);
            allViewsToUpdate.AddRange(viewsIntrElevs);
            allViewsToUpdate.AddRange(viewsElecPlans);
            allViewsToUpdate.AddRange(viewsEnlargedFormPlans);
            allViewsToUpdate.AddRange(viewsFormPlans);

            // create counter variables for final report
            int templatesImported = 0;
            int viewsUpdated = 0;
            int templatesDeleted = 0;
            int totalViews = allViewsToUpdate.Count;

            // set the path to the view template file
            string templateDoc = "S:\\Shared Folders\\Lifestyle USA Design\\Library 2025\\Template\\View Templates.rvt";

            // create a variable for the source document
            Document sourceDoc = null;

            try
            {
                sourceDoc = uidoc.Application.Application.OpenDocumentFile(templateDoc);

                // create a transaction group
                using (TransactionGroup tGroup = new TransactionGroup(curDoc, "Update View Templates"))
                {
                    // create the 1st transaction
                    using (Transaction t = new Transaction(curDoc))
                    {
                        // start the transaction group
                        tGroup.Start();

                        #region Delete View Templates

                        // start the 1st transaction
                        t.Start("Delete View Templates");                        

                        // delete all view templates that start with a letter or a number
                        foreach (View curVT in curVTs)
                        {
                            // get the name of the view template
                            string curName = curVT.Name;

                            // check view template name for deletion criteria
                            if (!string.IsNullOrEmpty(curName))
                            {
                                // check if first character is letter
                                bool isLetter = Char.IsLetter(curName[0]);

                                // check if starts with 01, 02, 03, 04, 05, 06, or 07                    
                                bool isTargetNumber = curName.StartsWith("01") ||
                                                      curName.StartsWith("02") ||
                                                      curName.StartsWith("03") ||
                                                      curName.StartsWith("04") ||
                                                      curName.StartsWith("05") ||
                                                      curName.StartsWith("06") ||
                                                      curName.StartsWith("07");

                                // if yes, delete it
                                if (isLetter == true || isTargetNumber == true)
                                {
                                    try
                                    {
                                        curDoc.Delete(curVT.Id);
                                        templatesDeleted++; // increment the counter
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }

                        // commit the 1st transaction
                        t.Commit();

                        #endregion

                        // create a variable for the target document                  
                        Document targetDoc = uidoc.Document;

                        // get the view templates from the source document
                        List<View> listViewTemplates = new FilteredElementCollector(sourceDoc)
                            .OfClass(typeof(View))
                            .Cast<View>()
                            .Where(v => v.IsTemplate)
                            .ToList();

                        #region Transfer View Templates

                        // start the 2nd transaction
                        t.Start("Transfer View Teamplates");

                        // transfer the vew templates from the source document
                        foreach (View sourceTemplate in listViewTemplates)
                        {
                            // check if template with exact same name already exists
                            View existingTemplate = new FilteredElementCollector(curDoc)
                                .OfClass(typeof(View))
                                .Cast<View>()
                                .FirstOrDefault(v => v.IsTemplate && v.Name.Equals(sourceTemplate.Name));

                            if (existingTemplate == null)
                            {
                                ElementId newTemplateID = Utils.ImportViewTemplates(sourceDoc, sourceTemplate, targetDoc);
                                templatesImported++; // increment the counter
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Skipping existing template: {sourceTemplate.Name}");
                            }
                        }

                        t.Commit();

                        #endregion                        

                        #region Assign View Templates                                               

                        // get all the new view templates in the project
                        List<View> newVTs = Utils.GetAllViewTemplates(curDoc);

                        // get the template map
                        var mapVTs = Utils.GetViewTemplateMap();

                        // start the 3rd transaction 
                        t.Start("Assign View Teamplates");

                        foreach(var curMap  in mapVTs)
                        {
                            var allViews = Utils.GetViewsByViewTemplateName(curDoc, curMap.OldTemplateName);
                            Utils.AssignTemplateToView(allViews, curMap.NewTemplateName, curDoc, ref viewsUpdated);
                        }

                        // commit the 3rd transaction
                        t.Commit();

                        #endregion

                        tGroup.Assimilate();
                    }
                }
            }

            finally
            {
                // Close the source document when done
                if (sourceDoc != null && !sourceDoc.IsFamilyDocument)
                {
                    sourceDoc.Close(false); // false = don't save
                }
            }

            // Show final report
            TaskDialog tdFinalReport = new TaskDialog("Complete");
            tdFinalReport.MainIcon = Icon.TaskDialogIconInformation;
            tdFinalReport.Title = "Update View Templates";
            tdFinalReport.TitleAutoPrefix = false;
            tdFinalReport.MainContent = $"{templatesDeleted} existing view templates have been deleted, {templatesImported} new view templates were added to the project and assigned to {viewsUpdated} out of {totalViews} views.";
            tdFinalReport.CommonButtons = TaskDialogCommonButtons.Close;

            TaskDialogResult tdSchedSuccessRes = tdFinalReport.Show();

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            clsButtonData myButtonData = new clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData.Data;
        }
    }
}
