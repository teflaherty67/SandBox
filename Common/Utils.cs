
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
        
        public static List<View> GetAllNonTemplateViews(Document curDoc)
        {
            return new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Views)
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.ViewType != ViewType.Legend)
                .ToList();
        }

        #endregion

        #region Categories

        internal static List<Element> GetCategoryByName(Document curDoc, BuiltInCategory bicTags)
        {
            List<Element> m_returnCat = new List<Element>();

            FilteredElementCollector m_colCat = new FilteredElementCollector(curDoc)
                .OfCategory(bicTags)
                .WhereElementIsElementType();

            foreach (Element curCat in m_colCat)
            {
                m_returnCat.Add(curCat);
            }

            return m_returnCat;
        }

        //internal static List<Category> GetCategoryByName(Document curDoc, string catName)
        //{
        //   // loop through categories in current model file
        //   foreach (Category curCat in curDoc.Settings.Categories)
        //    {
        //        if (curCat.Name == catName)
        //            return curCat;
        //    }

        //    return null;
        //}

        internal static List<BuiltInCategory> GetCategoriesByViewType(Document curDoc, View curView)
        {
            // create an empty category list
            List<BuiltInCategory> m_categories = new List<BuiltInCategory>();

            // get the current view
            curView = curDoc.ActiveView;

            // set the form to display based on the current view
            if (curView is Autodesk.Revit.DB.ViewPlan)
            {
                m_categories.Add(BuiltInCategory.OST_Doors);
                m_categories.Add(BuiltInCategory.OST_Grids);
                m_categories.Add(BuiltInCategory.OST_Rooms);
                m_categories.Add(BuiltInCategory.OST_Walls);
                m_categories.Add(BuiltInCategory.OST_Windows);
            }
            else if (curView is Autodesk.Revit.DB.ViewSection)
            {
                List<string> catNamesVSection = new List<string>() { "Grids", "Levels" };
            }
            else if (curView is Autodesk.Revit.DB.ViewSheet)
            {
                List<string> catNamesVSheet = new List<string>() { "Viewports" };
            }

            return null;
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

        public static List<string> GetAllViewTemplateNames(Document m_doc)
        {
            // returns list of view templates
            List<string> viewTempList = new List<string>();
            List<View> viewList = new List<View>();
            viewList = GetAllNonTemplateViews(m_doc);

            // loop through views and check if view is template
            foreach (View v in viewList)
            {
                if (v.IsTemplate == true)
                {
                    //add view template to list
                    viewTempList.Add(v.Name);
                }
            }

            return viewTempList;
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

        internal static View GetViewTemplateByNameContains(Document curDoc, string vtName)
        {
            List<View> m_colVTs = Utils.GetAllViewTemplates(curDoc);

            foreach (View curVT in m_colVTs)
            {
                if (curVT.Name.Contains(vtName))
                    return curVT;
            }

            return null;
        }

        internal static View GetViewTemplateByCategoryEquals(Document curDoc, string vtName)
        {
            List<View> m_colVTs = Utils.GetAllViewTemplates(curDoc);

            foreach (View curVT in m_colVTs)
            {
                if (curVT.Category.Equals(vtName))
                    return curVT;
            }

            return null;
        }

        internal static List<View> GetViewsWithoutTemplates(Document curDoc)
        {
            return new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Views)
                .Cast<View>()
                .Where(v => !v.IsTemplate &&
                           v.ViewType != ViewType.Legend &&
                           v.ViewTemplateId == ElementId.InvalidElementId)
                .ToList();
        }

        #endregion
    }
}
