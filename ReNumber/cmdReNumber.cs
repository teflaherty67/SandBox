using SandBox.Classes;

namespace SandBox
{
    [Transaction(TransactionMode.Manual)]
    public class cmdReNumber : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // put code needed for form here

            // get the current view
            View curView = curDoc.ActiveView;

            // create list for categories to renumber
            List<string> catList = new List<string>();

            // populate the list based on view type
            if (curView is Autodesk.Revit.DB.ViewPlan)
            {
                List<string> catListVPlan = new List<string> { "Doors", "Grids", "Rooms", "Spaces", "Walls", "Windows" };

                foreach (string curCat in catListVPlan)
                {
                    catList.Add(curCat);
                }
            }
            else if (curView is Autodesk.Revit.DB.ViewSection)
            {
                List<string> catListVSection = new List<string> { "Grids", "Levels" };

                foreach (string curCat in catListVSection)
                {
                    catList.Add(curCat);
                }
            }
            else if (curView is Autodesk.Revit.DB.ViewSheet)
            {
                catList.Add("Viewports");
            }

            // open the form
            frmReNumber curForm = new frmReNumber(catList)
            {
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            curForm.ShowDialog();

            // create and start transaction
            using (Transaction t = new Transaction(curDoc, "Renumber Elements"))
            {
                t.Start();

                // set some variables
                string detailNumber = null;
                string doorMark = null;
                string gridName = null;
                string roomNumber = null;
                string spacesNumber = null;
                string wallMark = null;
                string windowMark = null;


                // get the radio button result





                // get the start number result
                var resultNum = curForm.GetStartNumber();

                if (resultNum.containsLetter)
                {
                    string elemNum = curForm.GetStartNumber().ToString();
                }
                else if (resultNum.containsNumber)
                {
                    // convert the number string to an integer
                }



                // get the cbxExclude result
                if (curForm.GetCheckBoxExclude() == true) // && curForm.GetStartNum.IsLetter == true
                {
                    // skip the letters I and O
                }




                t.Commit();
            }

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
