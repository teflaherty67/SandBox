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

        internal static List<View> GetAllViews(Document curDoc)
        {
            {
                FilteredElementCollector m_colviews = new FilteredElementCollector(curDoc);
                m_colviews.OfCategory(BuiltInCategory.OST_Views);

                List<View> m_views = new List<View>();
                foreach (View x in m_colviews.ToElements())
                {
                    if (x.IsTemplate == false)

                        m_views.Add(x);
                }

                return m_views;
            }
        }

        internal static List<View> GetAllViewsByNameContains(Document curDoc, string v)
        {
            throw new NotImplementedException();
        }

        internal static List<View> GetAllViewsByNameContains(Document curDoc, string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public static List<View> GetAllNonTemplateViews(Document curDoc)
        {
            FilteredElementCollector m_colviews = new FilteredElementCollector(curDoc);
            m_colviews.OfCategory(BuiltInCategory.OST_Views);

            List<View> m_returnViews = new List<View>();
            foreach (View curView in m_colviews.ToElements())
            {
                m_returnViews.Add(curView);
            }

            return m_returnViews;
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
            List<View> returnList = new List<View>();
            List<View> viewList = GetAllNonTemplateViews(curDoc);

            //loop through views and check if is view template
            foreach (View v in viewList)
            {
                if (v.IsTemplate == true)
                {
                    //add view template to list
                    returnList.Add(v);
                }
            }

            return returnList;
        }

        public static List<string> GetAllViewTemplateNames(Document m_doc)
        {
            //returns list of view templates
            List<string> viewTempList = new List<string>();
            List<View> viewList = new List<View>();
            viewList = GetAllNonTemplateViews(m_doc);

            //loop through views and check if is view template
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

        public static View GetViewTemplateByName(Document curDoc, string viewTemplateName)
        {
            List<View> viewTemplateList = GetAllViewTemplates(curDoc);

            foreach (View v in viewTemplateList)
            {
                if (v.Name == viewTemplateName)
                {
                    return v;
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

        #endregion
    }
}
