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

        /// <summary>
        /// Updates lighting fixtures in specified rooms based on the given specification level.
        /// </summary>
        /// <param name="curDoc">The current <see cref="Document"/>.</param>
        /// <param name="specLevel">The specification level (e.g., "Complete Home", "Complete Home Plus").</param>
        internal static void UpdateLightingFixtures(Document curDoc, string specLevel)
        {
            // Define rooms that need lighting fixture updates
            List<string> roomsToUpdate = new List<string>
            {
                "Master Bedroom",
                "Covered Patio",
                "Gameroom",
                "Loft"
            };

            // Determine target family type based on spec level
            string targetFamilyType = specLevel switch
            {
                "Complete Home" => "LED",
                "Complete Home Plus" => "Ceiling Fan",
                _ => null
            };

            if (targetFamilyType == null)
            {
                TaskDialog.Show("Error", "Invalid Spec Level selected.");
                return;
            }

            // Find target family symbol
            FamilySymbol targetFamilySymbol = FindFamilySymbol(curDoc, "LT-No Base", targetFamilyType);
            if (targetFamilySymbol == null)
            {
                TaskDialog.Show("Error", $"Family symbol '{targetFamilyType}' not found.");
                return;
            }

            // Activate the family symbol if not already active
            if (!targetFamilySymbol.IsActive)
            {
                targetFamilySymbol.Activate();
            }

            // Counters
            int updatedCount = 0;
            int roomsNotFound = 0;

            // Iterate through each room to update lighting fixtures
            foreach (string roomName in roomsToUpdate)
            {
                // Get the room by name
                List<Room> rooms = GetRoomByNameContains(curDoc, roomName);

                // If no room found, show an error message and continue to the next room
                if (rooms.Count == 0)
                {
                    roomsNotFound++;
                    continue;
                }

                // Process each matched room
                foreach (Room room in rooms)
                {
                    // Find the lighting fixture of the specified family in the room
                    List<FamilyInstance> lightingFixtures = GetLightFixtureInRoom(curDoc, room, "LT-No Base");

                    // Update the lighting fixture type
                    foreach (FamilyInstance curFixture in lightingFixtures)
                    {
                        // Change the family type of the fixture
                        curFixture.Symbol = targetFamilySymbol;
                        updatedCount++;
                    }
                }
            }

            // Show summary message
            string message = $"Updated {updatedCount} light fixtures to '{targetFamilyType}'.";
            if (roomsNotFound > 0)
            {
                message += $"\n{roomsNotFound} room type(s) not found in the project.";
            }

            TaskDialog.Show("Light Fixture Update Complete", message);
        }
        
        /// <summary>
        /// Finds a family symbol by family name and type name
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <param name="familyName">The family name (e.g., "LT-No Base")</param>
        /// <param name="typeName">The type name (e.g., "LED" or "Ceiling Fan")</param>
        /// <returns>The family symbol or null if not found</returns>
        private static FamilySymbol FindFamilySymbol(Document curDoc, string familyName, string typeName)
        {
            return new FilteredElementCollector(curDoc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs => fs.Family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase) &&
                                     fs.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all light fixtures (family instances) in a specific room
        /// </summary>
        /// <param name="curDoc">The Revit document</param>
        /// <param name="room">The room to search in</param>
        /// <param name="familyName">The family name to filter by (optional)</param>
        /// <returns>List of family instances in the room</returns>
        private static List<FamilyInstance> GetLightFixtureInRoom(Document curDoc, Room room, string familyName = null)
        {
            List<FamilyInstance> m_lightFixtures = new List<FamilyInstance>();

            // Get all lighting fixtures in the document
            var familyInstances = new FilteredElementCollector(curDoc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_LightingFixtures)
                .Cast<FamilyInstance>();

            foreach (FamilyInstance curInstance in familyInstances)
            {
                // Check the Room parameter
                if (curInstance.Room != null && curInstance.Room.Id == room.Id)
                {
                    // Filter by family name if specified
                    if (string.IsNullOrEmpty(familyName) ||
                        curInstance.Symbol.Family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase))
                    {
                        m_lightFixtures.Add(curInstance);
                    }
                }
            }

            return m_lightFixtures;
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


        #endregion
    }
}
