using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SandBox.Classes;
using System.Diagnostics.Metrics;
using System.Windows.Controls;

namespace SandBox.Common
{
    internal static class Utils
    {
        #region Ribbon Panels
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

        #endregion

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




        #endregion

        #region Rooms

        /// <summary>
        /// Retrieves all <see cref="Room"/> elements from the specified Revit document.
        /// </summary>
        /// <param name="curDoc">The current <see cref="Document"/> from which to collect room elements.</param>
        /// <returns>A list of all <see cref="Room"/> elements found in the document.</returns>
        public static List<Room> GetAllRooms(Document curDoc)
        {
            return new FilteredElementCollector(curDoc)         // Initialize a collector for the given document
                .OfCategory(BuiltInCategory.OST_Rooms)           // Filter elements to include only Rooms
                .Cast<Room>()                                    // Cast the elements to Room type
                .ToList();                                       // Convert the collection to a List<Room> and return
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

        /// <summary>
        /// Retrieves all <see cref="Room"/> elements from the specified level.
        /// </summary>
        /// <param name="curDoc">The current <see cref="Document"/>.</param>
        /// <param name="levelName">The name of the level to match.</param>
        /// <returns>A list of <see cref="Room"/> elements on the specified level.</returns>
        internal static List<Room> GetRoomsByLevel(Document curDoc, string levelName)
        {
            // Get all rooms in the document
            List<Room> allRooms = GetAllRooms(curDoc);

            // Filter by level name
            return allRooms
                .Where(room => room.Level?.Name == levelName)
                .ToList();
        }

        #endregion

        #region Families

        internal static List<Family> GetAllFamilies(Document curDoc)
        {
            List<Family> m_returnList = new List<Family>();

            FilteredElementCollector m_colFamilies = new FilteredElementCollector(curDoc)
                .OfClass(typeof(Family));

            foreach (Family curFamily in m_colFamilies)
            {
                m_returnList.Add(curFamily);
            }

            return m_returnList;
        }

        internal static void GetFamilyInstances(Document curDoc, Family duplicateFamily, out List<FamilyInstance> instances)
        {
            instances = new List<FamilyInstance>();

            // Get all FamilyInstance elements in the document
            var m_allInstances = new FilteredElementCollector(curDoc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .ToList();

            // Filter to find instances that belong to the specified family
            foreach (FamilyInstance curFamInstance in m_allInstances)
            {
                if (curFamInstance.Symbol?.Family?.Id == curFamInstance.Id)
                {
                    instances.Add(curFamInstance);
                }
            }
        }

        public static FamilySymbol GetFamilySymbolByName(Document curDoc, string familyName, string typeName)
        {
            List<Family> m_famList = GetAllFamilies(curDoc);

            // loop through families in current document and look for match
            foreach (Family curFam in m_famList)
            {
                if (curFam.Name == familyName)
                {
                    // get family symbol from family
                    ISet<ElementId> fsList = curFam.GetFamilySymbolIds();

                    // loop through family symbol ids and look for match
                    foreach (ElementId fsID in fsList)
                    {
                        FamilySymbol fs = curDoc.GetElement(fsID) as FamilySymbol;

                        if (fs.Name == typeName)
                        {
                            return fs;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Text Notes

        /// <summary>
        /// Manages ceiling fan notes in specified rooms based on spec level conversion
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <param name="specLevel">The target spec level (Complete Home or Complete Home Plus)</param>
        public static void ManageClgFanNotes(Document curDoc, string specLevel)
        {
            // Define rooms that need note management
            List<string> roomsToUpdate = new List<string>
            {
                "Master Bedroom",
                "Covered Patio",
                "Gameroom",
                "Loft"
            };

            string noteText = "Block & pre-wire for clg fan";

            if (specLevel == "Complete Home Plus")
            {
                // CH to CHP conversion - DELETE notes in all rooms
                DeleteCeilingFanNotes(curDoc, roomsToUpdate, noteText);
            }
            else if (specLevel == "Complete Home")
            {
                // CHP to CH conversion - ADD notes in all rooms EXCEPT Covered Patio
                List<string> roomsForNotes = roomsToUpdate.Where(r => r != "Covered Patio").ToList();
                AddCeilingFanNotes(curDoc, roomsForNotes, noteText);
            }
        }

        /// <summary>
        /// Deletes ceiling fan notes from specified rooms
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <param name="roomNames">List of room names to process</param>
        /// <param name="noteText">The note text to search for and delete</param>
        private static void DeleteCeilingFanNotes(Document curDoc, List<string> roomNames, string noteText)
        {
            int deletedCount = 0;

            foreach (string roomName in roomNames)
            {
                // Get rooms containing this name
                List<Room> rooms = GetRoomByNameContains(curDoc, roomName);

                foreach (Room room in rooms)
                {
                    // Find text notes in this room
                    List<TextNote> notesToDelete = GetTextNotesInRoom(curDoc, room, noteText);

                    // Delete each matching note
                    foreach (TextNote note in notesToDelete)
                    {
                        curDoc.Delete(note.Id);
                        deletedCount++;
                    }
                }
            }

            if (deletedCount > 0)
            {
                TaskDialog.Show("Notes Deleted", $"Deleted {deletedCount} ceiling fan notes.");
            }
        }

        /// <summary>
        /// Adds ceiling fan notes to specified rooms
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <param name="roomNames">List of room names to process</param>
        /// <param name="noteText">The note text to add</param>
        private static void AddCeilingFanNotes(Document curDoc, List<string> roomNames, string noteText)
        {
            int addedCount = 0;

            // Get the default text type
            TextNoteType textType = GetDefaultTextNoteType(curDoc);
            if (textType == null)
            {
                TaskDialog.Show("Error", "No text note type found in the project.");
                return;
            }

            foreach (string roomName in roomNames)
            {
                // Skip Covered Patio
                if (roomName == "Covered Patio")
                    continue;

                // Get rooms containing this name
                List<Room> rooms = GetRoomByNameContains(curDoc, roomName);

                foreach (Room room in rooms)
                {
                    // Check if note already exists
                    List<TextNote> existingNotes = GetTextNotesInRoom(curDoc, room, noteText);
                    if (existingNotes.Count > 0)
                        continue; // Note already exists, skip

                    // Get room center point for note placement
                    XYZ roomCenter = GetRoomCenterPoint(room);
                    if (roomCenter != null)
                    {
                        // Create the text note
                        TextNote.Create(curDoc, curDoc.ActiveView.Id, roomCenter, noteText, textType.Id);
                        addedCount++;
                    }
                }
            }

            if (addedCount > 0)
            {
                TaskDialog.Show("Notes Added", $"Added {addedCount} ceiling fan notes.");
            }
        }

        /// <summary>
        /// Gets text notes in a specific room that contain the specified text
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <param name="room">The room to search in</param>
        /// <param name="searchText">The text to search for</param>
        /// <returns>List of matching text notes</returns>
        private static List<TextNote> GetTextNotesInRoom(Document curDoc, Room room, string searchText)
        {
            List<TextNote> m_textNotes = new List<TextNote>();

            // Get all text notes in the document
            var textNotes = new FilteredElementCollector(curDoc)
                .OfClass(typeof(TextNote))
                .Cast<TextNote>();

            foreach (TextNote curNote in textNotes)
            {
                // Check if note text contains the search text
                if (curNote.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Check if note is in the room (using bounding box intersection)
                    if (IsTextNoteInRoom(curNote, room))
                    {
                        m_textNotes.Add(curNote);
                    }
                }
            }

            return m_textNotes;
        }

        /// <summary>
        /// Checks if a text note is within a room's boundaries
        /// </summary>
        /// <param name="textNote">The text note to check</param>
        /// <param name="room">The room to check against</param>
        /// <returns>True if text note is in room</returns>
        private static bool IsTextNoteInRoom(TextNote textNote, Room room)
        {
            try
            {
                XYZ notePosition = textNote.Coord;
                var roomAtPoint = room.Document.GetRoomAtPoint(notePosition);
                return roomAtPoint != null && roomAtPoint.Id == room.Id;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the default text note type from the document
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <returns>The default text note type or null if not found</returns>
        private static TextNoteType GetDefaultTextNoteType(Document curDoc)
        {
            return new FilteredElementCollector(curDoc)
                .OfClass(typeof(TextNoteType))
                .Cast<TextNoteType>()
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the center point of a room for note placement
        /// </summary>
        /// <param name="room">The room</param>
        /// <returns>The center point or null if not found</returns>
        private static XYZ GetRoomCenterPoint(Room room)
        {
            try
            {
                LocationPoint location = room.Location as LocationPoint;
                return location?.Point;
            }
            catch
            {
                // Fallback: use bounding box center
                var bbox = room.get_BoundingBox(null);
                if (bbox != null)
                {
                    return (bbox.Min + bbox.Max) / 2;
                }
                return null;
            }
        }

        /// <summary>
        /// Retrieves all <see cref="View"/> elements from the specified Revit document.
        /// </summary>
        /// <param name="curDoc">The current <see cref="Document"/>.</param>
        /// <returns>A list of all <see cref="View"/> elements found in the document.</returns>
        public static List<View> GetAllViews(Document curDoc)
        {
            // Collect all elements in the 'Views' category that are not element types
            FilteredElementCollector m_colviews = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType();

            // Initialize the list to hold the views
            List<View> m_views = new List<View>();

            // Cast and add each view to the list
            foreach (View x in m_colviews.ToElements())
            {
                m_views.Add(x);
            }

            // Return the complete list of views
            return m_views;
        }


        /// <summary>
        /// Get a view by name that contains specified string and is associated with specified level
        /// </summary>
        /// <param name="curDoc">Current Revit document</param>
        /// <param name="nameContains">String that view name should contain</param>
        /// <param name="associatedLevel">Name of the associated level</param>
        /// <returns>First matching view or null if not found</returns>
        public static View GetViewByNameContainsAndAssociatedLevel(Document curDoc, string nameContains, string associatedLevel)
        {
            List<View> m_allViews = GetAllViews(curDoc);

            foreach (View curView in m_allViews)
            {
                // Check if the view name contains the specified string
                if (curView.Name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Try to get the Associated Level parameter by name
                    Parameter associatedLevelParam = curView.LookupParameter("Associated Level");

                    if (associatedLevelParam != null && associatedLevelParam.HasValue)
                    {
                        string levelName = associatedLevelParam.AsString();

                        if (levelName == associatedLevel)
                        {
                            return curView; // Return the first matching view
                        }
                    }
                }
            }

            // Return null if no matching view is found
            return null;
        }

        internal static ViewSchedule GetScheduleByNameContains(Document curDoc, string scheduleString)
        {
            List<ViewSchedule> m_scheduleList = GetAllSchedules(curDoc);

            foreach (ViewSchedule curSchedule in m_scheduleList)
            {
                if (curSchedule.Name.Contains(scheduleString))
                    return curSchedule;
            }

            return null;
        }

        internal static List<ViewSchedule> GetAllSchedules(Document curDoc)
        {
            List<ViewSchedule> m_schedList = new List<ViewSchedule>();

            FilteredElementCollector curCollector = new FilteredElementCollector(curDoc);
            curCollector.OfClass(typeof(ViewSchedule));
            curCollector.WhereElementIsNotElementType();

            //loop through views and check if schedule - if so then put into schedule list
            foreach (ViewSchedule curView in curCollector)
            {
                if (curView.ViewType == ViewType.Schedule)
                {
                    if (curView.IsTemplate == false)
                    {
                        if (curView.Name.Contains("<") && curView.Name.Contains(">"))
                            continue;
                        else
                            m_schedList.Add((ViewSchedule)curView);
                    }
                }
            }

            return m_schedList;
        }

        internal static List<View> GetAllViewsByNameContainsAndAssociatedLevel(Document curDoc, string v1, string v2)
        {
            throw new NotImplementedException();
        }

        internal static ViewSheet GetViewSheetByName(Document curDoc, string v)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Front Door

        /// <summary>
        /// Finds the front door based on room relationships
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <returns>The front door instance or null if not found</returns>
        public static FamilyInstance GetFrontDoor(Document curDoc)
            {
                // Get all doors in the document
                List<FamilyInstance> allDoors = GetAllDoors(curDoc);

                foreach (FamilyInstance curDoor in allDoors)
                {
                    // Get the FromRoom and ToRoom properties
                    Room fromRoom = curDoor.FromRoom;
                    Room toRoom = curDoor.ToRoom;

                    if (fromRoom != null && toRoom != null)
                    {
                        string fromRoomName = fromRoom.Name;
                        string toRoomName = toRoom.Name;

                        // Check if this matches front door criteria
                        if (IsFrontDoorMatch(fromRoomName, toRoomName))
                        {
                            return curDoor;
                        }
                    }
                }

                return null; // Front door not found
            }

            /// <summary>
            /// Checks if the room names match front door criteria
            /// </summary>
            /// <param name="fromRoomName">The "From Room: Name" value</param>
            /// <param name="toRoomName">The "To Room: Name" value</param>
            /// <returns>True if this appears to be the front door</returns>
            private static bool IsFrontDoorMatch(string fromRoomName, string toRoomName)
            {
                if (string.IsNullOrEmpty(fromRoomName) || string.IsNullOrEmpty(toRoomName))
                    return false;

                // Check if From Room is Entry or Foyer
                bool fromRoomMatch = fromRoomName.IndexOf("Entry", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    fromRoomName.IndexOf("Foyer", StringComparison.OrdinalIgnoreCase) >= 0;

                // Check if To Room is Covered Porch
                bool toRoomMatch = toRoomName.IndexOf("Covered Porch", StringComparison.OrdinalIgnoreCase) >= 0;

                return fromRoomMatch && toRoomMatch;
            }

            /// <summary>
            /// Gets all door instances in the document
            /// </summary>
            /// <param name="curDoc">The Revit document</param>
            /// <returns>List of all door family instances</returns>
            private static List<FamilyInstance> GetAllDoors(Document curDoc)
            {
                return new FilteredElementCollector(curDoc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .ToList();
            }

            /// <summary>
            /// Updates the front door type based on spec level
            /// </summary>
            /// <param name="curDoc">The Revit document</param>
            /// <param name="specLevel">The spec level selection</param>
            public static void UpdateFrontDoor(Document curDoc, string specLevel)
            {
                // Find the front door
                FamilyInstance frontDoor = GetFrontDoor(curDoc);

                if (frontDoor == null)
                {
                    TaskDialog.Show("Warning", "Front door not found. Looking for door from Entry/Foyer to Covered Porch.");
                    return;
                }

                // Determine the new door type based on spec level
                string newDoorTypeName = GetFrontDoorType(specLevel);
                if (string.IsNullOrEmpty(newDoorTypeName))
                {
                    TaskDialog.Show("Error", "Unable to determine front door type for spec level: " + specLevel);
                    return;
                }

                // Find the new door family symbol
                FamilySymbol newDoorSymbol = FindDoorSymbol(curDoc, newDoorTypeName);
                if (newDoorSymbol == null)
                {
                    TaskDialog.Show("Error", $"Door type '{newDoorTypeName}' not found in the project.");
                    return;
                }

                // Activate the symbol if needed
                if (!newDoorSymbol.IsActive)
                {
                    newDoorSymbol.Activate();
                }

                // Store original swing parameter value
                Parameter swingParam = frontDoor.LookupParameter("Swing");
                string originalSwing = swingParam?.AsString();

                // Change the door type
                frontDoor.Symbol = newDoorSymbol;

                // Verify/restore swing parameter if it changed
                if (swingParam != null && !string.IsNullOrEmpty(originalSwing))
                {
                    string newSwing = swingParam.AsString();
                    if (newSwing != originalSwing)
                    {
                        // Try to restore original swing - this might need adjustment based on parameter type
                        TaskDialog.Show("Warning", $"Door swing may have changed from '{originalSwing}' to '{newSwing}'. Please verify.");
                    }
                }

                TaskDialog.Show("Success", $"Front door updated to '{newDoorTypeName}' for {specLevel} spec level.");
            }

            /// <summary>
            /// Gets the front door type name based on spec level
            /// </summary>
            /// <param name="specLevel">The spec level</param>
            /// <returns>The door type name</returns>
            private static string GetFrontDoorType(string specLevel)
            {
                return specLevel switch
                {
                    "Complete Home" => "Standard Front Door", // Replace with actual type name
                    "Complete Home Plus" => "Premium Front Door", // Replace with actual type name
                    _ => null
                };
            }

            /// <summary>
            /// Finds a door symbol by type name
            /// </summary>
            /// <param name="curDoc">The Revit document</param>
            /// <param name="typeName">The door type name</param>
            /// <returns>The door symbol or null if not found</returns>
            private static FamilySymbol FindDoorSymbol(Document curDoc, string typeName)
            {
                return new FilteredElementCollector(curDoc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .FirstOrDefault(ds => ds.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            }

        internal static void UpdateFrontDoorType(Document curDoc, string selectedSpecLevel)
        {

            throw new NotImplementedException();
        }

        internal static void UpdateRearDoorType(Document curDoc, string selectedSpecLevel)
        {
            throw new NotImplementedException();
        }



        #endregion

        #region Flooring Update

        /// <summary>
        /// Updates the Floor Finish parameter for specified room types in the active view based on the selected specification level.
        /// </summary>
        /// <param name="curDoc">The current Revit document.</param>
        /// <param name="selectedSpecLevel">The specification level ("Complete Home" or "Complete Home Plus") that determines the floor finish value.</param>
        /// <remarks>
        /// This method updates the following room types: Master Bedroom, Family, and Hall.
        /// Complete Home sets floor finish to "Carpet", Complete Home Plus sets it to "HS".
        /// Only processes rooms that are visible in the current active view.
        /// </remarks>
        internal static List<string> UpdateFloorFinishInActiveView(Document curDoc, string selectedSpecLevel)
        {
            // get the active view from the document
            View activeView = curDoc.ActiveView;

            // create a list of rooms to update
            List<string> m_RoomsToUpdateFloorFinish = new List<string>
            {
                "Master Bedroom",
                "Family",
                "Hall"
            };

            // get the room element of the rooms to update
            List<Room> m_RoomstoUpdate = GetRoomsByNameContainsInActiveView(curDoc, m_RoomsToUpdateFloorFinish);

            // create the switch statement to determine the floor finish based on the spec level
            string floorFinish = selectedSpecLevel switch
            {
                "Complete Home" => "Carpet",
                "Complete Home Plus" => "HS",
                _ => null
            };

            // check if the floor finish is null
            if (floorFinish == null)
            {
                TaskDialog.Show("Error", "Invalid Spec Level selected.");
                return new List<string>();
            }           

            // create an empty list to hold the room names fuond in the active view
            List<string> m_updatedRoomNames = new List<string>();
           
            // loop through the rooms to update
            foreach (Room curRoom in m_RoomstoUpdate)
            {
                // get the floor finish parameter
                Parameter paramFloorFinish = curRoom.get_Parameter(BuiltInParameter.ROOM_FINISH_FLOOR);
                // check if the parameter is not null and has a value
                if (paramFloorFinish != null && !paramFloorFinish.IsReadOnly)
                {
                    // set the value of the floor finish parameter to the new value
                    paramFloorFinish.Set(floorFinish);

                    // add the room name to the ist
                    Parameter paramRoomName = curRoom.get_Parameter(BuiltInParameter.ROOM_NAME);
                    m_updatedRoomNames.Add(paramRoomName.AsString());
                }
            }

            // return the updated room names list
            return m_updatedRoomNames;
        }

        /// <summary>
        /// Gets all rooms in the active view whose names contain any of the specified strings.
        /// </summary>
        /// <param name="curDoc">The current Revit document.</param>
        /// <param name="RoomsToUpdate">List of room name strings to search for (case-insensitive matching).</param>
        /// <returns>A list of Room elements in the active view whose names contain any of the specified strings.</returns>
        private static List<Room> GetRoomsByNameContainsInActiveView(Document curDoc, List<string> RoomsToUpdate)
        {
            // get the active view from the document
            View activeView = curDoc.ActiveView;

            // create a lsit to hold the matching rooms
            List<Room> m_matchingRooms = new List<Room>();

            // get all the rooms in the active view
            FilteredElementCollector m_colRooms = new FilteredElementCollector(curDoc, activeView.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            // loop through the rooms and check if the room name contains the string in the list
            foreach (Room curRoom in m_colRooms)
            {
                // check if the room name contains any of the strings in the list
                if (RoomsToUpdate.Any(roomName => curRoom.Name.IndexOf(roomName, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    // if so, add the room to the matching rooms list
                    m_matchingRooms.Add(curRoom);
                }
            }

            // return the matching rooms
            return m_matchingRooms;
        }

        #endregion

        #region Task Dialog

        /// <summary>
        /// Displays a warning dialog to the user with custom title and message
        /// </summary>
        /// <param name="tdName">The internal name of the TaskDialog</param>
        /// <param name="tdTitle">The title displayed in the dialog header</param>
        /// <param name="textMessage">The main message content to display to the user</param>
        internal static void TaskDialogWarning(string tdName, string tdTitle, string textMessage)
        {
            // Create a new TaskDialog with the specified name
            TaskDialog m_Dialog = new TaskDialog(tdName);

            // Set the warning icon to indicate this is a warning message
            m_Dialog.MainIcon = Icon.TaskDialogIconWarning;

            // Set the custom title for the dialog
            m_Dialog.Title = tdTitle;

            // Disable automatic title prefixing to use our custom title exactly as specified
            m_Dialog.TitleAutoPrefix = false;

            // Set the main message content that will be displayed to the user
            m_Dialog.MainContent = textMessage;

            // Add a Close button for the user to dismiss the dialog
            m_Dialog.CommonButtons = TaskDialogCommonButtons.Close;

            // Display the dialog and capture the result (though we don't use it for warnings)
            TaskDialogResult m_DialogResult = m_Dialog.Show();
        }

        /// <summary>
        /// Displays an information dialog to the user with custom title and message
        /// </summary>
        /// <param name="tdName">The internal name of the TaskDialog</param>
        /// <param name="tdTitle">The title displayed in the dialog header</param>
        /// <param name="textMessage">The main message content to display to the user</param>
        internal static void TaskDialogInformation(string tdName, string tdTitle, string textMessage)
        {
            // Create a new TaskDialog with the specified name
            TaskDialog m_Dialog = new TaskDialog(tdName);

            // Set the warning icon to indicate this is a warning message
            m_Dialog.MainIcon = Icon.TaskDialogIconInformation;

            // Set the custom title for the dialog
            m_Dialog.Title = tdTitle;

            // Disable automatic title prefixing to use our custom title exactly as specified
            m_Dialog.TitleAutoPrefix = false;

            // Set the main message content that will be displayed to the user
            m_Dialog.MainContent = textMessage;

            // Add a Close button for the user to dismiss the dialog
            m_Dialog.CommonButtons = TaskDialogCommonButtons.Close;

            // Display the dialog and capture the result (though we don't use it for warnings)
            TaskDialogResult m_DialogResult = m_Dialog.Show();
        }

        /// <summary>
        /// Displays an error dialog to the user with custom title and message
        /// </summary>
        /// <param name="tdName">The internal name of the TaskDialog</param>
        /// <param name="tdTitle">The title displayed in the dialog header</param>
        /// <param name="textMessage">The main message content to display to the user</param>
        internal static void TaskDialogError(string tdName, string tdTitle, string textMessage)
        {
            // Create a new TaskDialog with the specified name
            TaskDialog m_Dialog = new TaskDialog(tdName);

            // Set the warning icon to indicate this is a warning message
            m_Dialog.MainIcon = Icon.TaskDialogIconError;

            // Set the custom title for the dialog
            m_Dialog.Title = tdTitle;

            // Disable automatic title prefixing to use our custom title exactly as specified
            m_Dialog.TitleAutoPrefix = false;

            // Set the main message content that will be displayed to the user
            m_Dialog.MainContent = textMessage;

            // Add a Close button for the user to dismiss the dialog
            m_Dialog.CommonButtons = TaskDialogCommonButtons.Close;

            // Display the dialog and capture the result (though we don't use it for warnings)
            TaskDialogResult m_DialogResult = m_Dialog.Show();
        }

        #endregion
    }
}
