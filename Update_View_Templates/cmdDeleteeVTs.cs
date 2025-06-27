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

                            // check if first character is letter
                            bool isLetter = !String.IsNullOrEmpty(curName) && Char.IsLetter(curName[0]);

                            // check if first two charactera is number                    
                            bool isNumber = !String.IsNullOrEmpty(curName) && Char.IsNumber(curName[0]);

                            // if yes, delete it
                            if (isLetter == true || isNumber == true)
                            {
                                try
                                {
                                    curDoc.Delete(curVT.Id);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }

                        // get old frame areas schedule template
                        ViewSchedule frameSchedule = new FilteredElementCollector(curDoc)
                            .OfClass(typeof(ViewSchedule))
                            .Cast<ViewSchedule>()
                            .FirstOrDefault(s => s.Name.Equals("-Frame Areas-"));

                        // delete old frame areas schedule template
                        if (frameSchedule != null)
                        {
                            try
                            {
                                curDoc.Delete(frameSchedule.Id);
                            }
                            catch (Exception)
                            {
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
                        t.Start("Transfer View Templates");

                        // transfer the view templates from the source document
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
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Skipping existing template: {sourceTemplate.Name}");
                            }
                        }

                        t.Commit();

                        #endregion               

                        tGroup.Assimilate();
                    }
                }

                using(Transaction tx = new Transaction(curDoc))
                {
                    tx.Start("Assign View Templates");

                    // get all the views in the project
                    List<View> nonTemplateViews = Utils.GetAllNonTemplateViews(curDoc);

                    // get all the new view templates in the project
                    List<View> newVTs = Utils.GetAllViewTemplates(curDoc);

                    // create a variable for the new view template
                    View newViewTemp = null;

                    #region Assign View Templates

                    foreach (View curView in nonTemplateViews)
                    {
                        // assign the appropriate view template
                        if (curView.Name.IndexOf("Annotation", StringComparison.Ordinal) >= 0)
                        {
                            newViewTemp = Utils.GetViewTemplateByNameContains(curDoc, "Annotations");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Name.IndexOf("Dimension", StringComparison.Ordinal) >= 0)
                        {
                            newViewTemp = Utils.GetViewTemplateByNameContains(curDoc, "Dimensions");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Category.Equals("02: Elevations") || curView.Category.Equals("02: Exterior Elevations"))
                        {
                            newViewTemp = Utils.GetViewTemplateByCategoryEquals(curDoc, "03:Exterior Elevations");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Name.IndexOf("Roof", StringComparison.Ordinal) >= 0)
                        {
                            newViewTemp = Utils.GetViewTemplateByNameContains(curDoc, "Roof");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Category.Equals("04:Sections"))
                        {
                            newViewTemp = Utils.GetViewTemplateByCategoryEquals(curDoc, "04:Sections");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Category.Equals("05:Interior Elevations"))
                        {
                            newViewTemp = Utils.GetViewTemplateByCategoryEquals(curDoc, "05:Interior Elevations");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Name.IndexOf("Electrical", StringComparison.Ordinal) >= 0)
                        {
                            newViewTemp = Utils.GetViewTemplateByNameContains(curDoc, "Electrical");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Name.IndexOf("Form", StringComparison.Ordinal) >= 0)
                        {
                            newViewTemp = Utils.GetViewTemplateByNameContains(curDoc, "Form");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Category.Equals("10:Floor Areas"))
                        {
                            newViewTemp = Utils.GetViewTemplateByCategoryEquals(curDoc, "10:Floor Areas");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Category.Equals("11:Frame Areas"))
                        {
                            newViewTemp = Utils.GetViewTemplateByCategoryEquals(curDoc, "11:Frame Areas");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                        else if (curView.Category.Equals("12:Attic Areas"))
                        {
                            newViewTemp = Utils.GetViewTemplateByCategoryEquals(curDoc, "12:Attic Areas");

                            curView.ViewTemplateId = newViewTemp.Id;
                        }
                    }

                    // commit the transaction
                    tx.Commit();

                    #endregion
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
