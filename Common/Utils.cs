using Autodesk.Revit.DB.Architecture;
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

        /// <summary>
        /// Retrieves all rooms from the document whose names contain the specified string,
        /// and filters out rooms with zero or invalid area.
        /// </summary>
        /// <param name="curDoc">The current Revit document.</param>
        /// <param name="nameRoom">The substring to search for in room names.</param>
        /// <returns>A list of rooms with names containing the specified string and valid area.</returns>
        internal static List<Room> GetRoomByNameContains(Document curDoc, string nameRoom)
        {
            // Retrieve all rooms in the document
            List<Room> m_roomList = GetAllRooms(curDoc);

            // Initialize the list to hold the matching rooms
            List<Room> m_returnList = new List<Room>();

            // Iterate through all rooms
            foreach (Room curRoom in m_roomList)
            {
                // Check if the room name contains the specified substring
                if (curRoom != null &&
                curRoom.Name != null &&
                curRoom.Name.IndexOf(nameRoom, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Check if the room has a valid area (greater than 0)
                    if (curRoom.Area > 0)
                    {
                        // Add the room to the result list
                        m_returnList.Add(curRoom);
                    }
                }
            }

            // Return the filtered list of rooms
            return m_returnList;
        }

        public static List<Room> GetAllRooms(Document curDoc)
        {
            return new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .ToList();
        }

        #endregion
    }
}
