using SandBox.Classes;
using System.Diagnostics.Metrics;
using System.Windows.Controls;

namespace SandBox.Common
{
    internal static class Utils
    {

        internal static RibbonPanel CreateRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel currentPanel = GetRibbonPanelByName(app, tabName, panelName);

            if (currentPanel == null)
                currentPanel = app.CreateRibbonPanel(tabName, panelName);

            return currentPanel;
        }

        internal static RibbonPanel GetRibbonPanelByName(UIControlledApplication app, string tabName, string panelName)
        {
            foreach (RibbonPanel tmpPanel in app.GetRibbonPanels(tabName))
            {
                if (tmpPanel.Name == panelName)
                    return tmpPanel;
            }

            return null;
        }

        #region Views     

        internal static List<View> GetViewsByViewTemplateName(Document curDoc, string templateName)
        {
            // Find the template by name
            View template = Utils.GetViewTemplateByName(curDoc, templateName);
            if (template == null) return new List<View>();

            // Find all views that currently use this template
            return new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Views)
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.ViewTemplateId == template.Id)
                .ToList();
        }

        #endregion

        #region View Templates

        public static List<View> GetAllViewTemplates(Document curDoc)
        {
            List<View> m_returnList = new List<View>();

            // get all views
            FilteredElementCollector viewCollector = new FilteredElementCollector(curDoc)
                .OfClass(typeof(View));

            // loop through views and check if view is template
            foreach (View v in viewCollector)
            {
                if (v.IsTemplate == true)
                {
                    // add view template to list
                    m_returnList.Add(v);
                }
            }

            return m_returnList;
        }      

        public static View GetViewTemplateByName(Document curDoc, string nameViewTemplate)
        {
            List<View> viewTemplateList = GetAllViewTemplates(curDoc);

            foreach (View curVT in viewTemplateList)
            {
                if (curVT.Name == nameViewTemplate)
                {
                    return curVT;
                }
            }

            return null;
        }

        internal static ElementId ImportViewTemplates(Document sourceDoc, View sourceTemplate, Document targetDoc)
        {
            CopyPasteOptions copyPasteOptions = new CopyPasteOptions();

            ElementId sourceTemplateId = sourceTemplate.Id;

            List<ElementId> elementIds = new List<ElementId>();
            elementIds.Add(sourceTemplate.Id);

            ElementTransformUtils.CopyElements(sourceDoc, elementIds, targetDoc, Autodesk.Revit.DB.Transform.Identity, copyPasteOptions);

            return sourceTemplate.Id;
        }


        public static List<clsViewTemplateMapping> GetViewTemplateMap()
        {
            return new List<clsViewTemplateMapping>
            {
                new clsViewTemplateMapping("01-Enlarged Plans", "02-Enlarged Plans"),
                new clsViewTemplateMapping("01-Floor Annotations", "02-Floor Annotations"),
                new clsViewTemplateMapping("01-Floor Dimensions", "02-Floor Dimensions"),
                new clsViewTemplateMapping("01-Key Plans", "02-Key Plans"),
                new clsViewTemplateMapping("02-Elevations", "03-Exterior Elevations"),
                new clsViewTemplateMapping("02-Key Elevations", "03-Key Elevations"),
                new clsViewTemplateMapping("02-Porch Elevations", "03-Porch Elevations"),
                new clsViewTemplateMapping("03-Roof Plan", "04-Roof Plans"),
                new clsViewTemplateMapping("04-Sections", "05-Sections"),
                new clsViewTemplateMapping("04-Sections_3/8\"", "05-Sections_3/8\""),
                new clsViewTemplateMapping("05-Cabinet Layout Plans", "06-Cabinet Layout Plans"),
                new clsViewTemplateMapping("05-Interior Elevations", "06-Interior Elevations"),
                new clsViewTemplateMapping("06-Electrical Plans", "07-Electrical Plans"),
                new clsViewTemplateMapping("07-Enlarged Form/Foundation Plans", "01-Enlarged Form Plans"),
                new clsViewTemplateMapping("07-Form/Foundation Plans", "01-Form Plans")
            };
        }

        internal static void AssignTemplateToView(List<View> allViews, string newTemplateName, Document curDoc, ref int viewsUpdated)
        {
            View newViewTemp = GetViewTemplateByName(curDoc, newTemplateName);

            if (newViewTemp != null)
            {
                foreach (View curView in allViews)
                {
                    curView.ViewTemplateId = newViewTemp.Id;
                    viewsUpdated++;
                }
            }
        }

        internal static List<ViewPlan> GetAllPlanViews(Document curDoc)
        {
            FilteredElementCollector m_Collector = new FilteredElementCollector(curDoc);
            ICollection<Element> m_allViews = m_Collector.OfClass(typeof(ViewPlan)).ToElements();

            List<ViewPlan> m_allPlanViews = new List<ViewPlan>();

            // loop through all views and check if view is a plan view
            foreach (ViewPlan curViewPlan in m_allViews.Cast<ViewPlan>())
            {
                if ((curViewPlan.ViewType == ViewType.FloorPlan || curViewPlan.ViewType == ViewType.CeilingPlan)
                    && !curViewPlan.IsTemplate)
                {
                    // add view to list
                    m_allPlanViews.Add(curViewPlan);
                }
            }

            return m_allPlanViews.OrderBy(v => v.GenLevel.Elevation).ThenBy(v => v.Name).ToList();
        }

        internal static List<View> GetAllViews(Document curDoc)
        {
            FilteredElementCollector m_colviews = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Views);

            List<View> m_views = new List<View>();

            // loop through views and add to list
            foreach (View x in m_colviews.ToElements())
            {
                m_views.Add(x);
            }

            return m_views;
        }

        /// <summary>
        /// Creates a display name for a view showing type and level
        /// </summary>
        public static string GetViewDisplayName(ViewPlan view)
        {
            string viewType = view.ViewType == ViewType.FloorPlan ? "Floor Plan" : "Ceiling Plan";
            string levelName = view.GenLevel?.Name ?? "Unknown Level";
            return $"{view.Name} ({viewType} - {levelName})";
        }

        /// <summary>
        /// Rotates multiple views by the specified angle
        /// </summary>
        public static void RotateViews(Document curDoc, List<ViewPlan> views, double angleRadians, string description)
        {
            if (!views.Any()) return;

            using (Transaction trans = new Transaction(curDoc, $"Rotate {views.Count} Views {description}"))
            {
                trans.Start();

                int successCount = 0;
                int failCount = 0;

                foreach (ViewPlan view in views)
                {
                    if (RotateSingleView(curDoc, view, angleRadians))
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                trans.Commit();

                // Show results
                if (failCount > 0)
                {
                    TaskDialog.Show("Rotation Results",
                        $"Rotation completed:\n" +
                        $"• Successfully rotated: {successCount} views\n" +
                        $"• Failed to rotate: {failCount} views\n\n" +
                        $"Failed views may not have crop regions enabled or may be read-only.",
                        TaskDialogCommonButtons.Ok);
                }
                else
                {
                    TaskDialog.Show("Success",
                        $"Successfully rotated {successCount} view(s) {description}.",
                        TaskDialogCommonButtons.Ok);
                }
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Rotates a single view by finding and rotating its crop box element
        /// </summary>
        private static bool RotateSingleView(Document curDoc, ViewPlan view, double angleRadians)
        {
            try
            {
                // Ensure crop box is active
                if (!view.CropBoxActive)
                {
                    view.CropBoxActive = true;
                    curDoc.Regenerate();
                }

                // Find the crop box element
                Element cropBoxElement = FindCropBoxElement(curDoc, view);

                if (cropBoxElement != null)
                {
                    return RotateCropBoxElement(curDoc, cropBoxElement, angleRadians);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the crop box element for a specific view
        /// </summary>
        private static Element FindCropBoxElement(Document curDoc, ViewPlan view)
        {
            FilteredElementCollector collector = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_CropBoxes)
                .WherePasses(new ElementOwnerViewFilter(view.Id));

            return collector.FirstElement();
        }

        /// <summary>
        /// Rotates a crop box element around its center point
        /// </summary>
        private static bool RotateCropBoxElement(Document curDoc, Element cropBoxElement, double angleRadians)
        {
            try
            {
                BoundingBoxXYZ bbox = cropBoxElement.get_BoundingBox(null);
                if (bbox == null) return false;

                XYZ centerPoint = (bbox.Min + bbox.Max) * 0.5;

                Line rotationAxis = Line.CreateBound(
                    centerPoint,
                    centerPoint + XYZ.BasisZ
                );

                ElementTransformUtils.RotateElement(
                    curDoc,
                    cropBoxElement.Id,
                    rotationAxis,
                    angleRadians
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Rotation Angle Constants

        public static class RotationAngles
        {
            public const double CounterClockwise90 = Math.PI / 2;
            public const double Clockwise90 = -Math.PI / 2;
            public const double Rotate180 = Math.PI;
        }

        #endregion
    }
}