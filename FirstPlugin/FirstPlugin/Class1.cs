using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;

namespace FirstPlugin
{
    public class Class1 : IExternalApplication
    {

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("FirstRibbonPanel");

            //button to trigger command
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("cmdMyTest", "My Test", thisAssemblyPath, "FirstPlugin.MyTest");

            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;

            //tool tip on hover
            pushButton.ToolTip = "Adds 10 grids horizontally and 10 grids vertically, named by letters and numbers resp.";

            //Bitmap icon 32x32
            Uri urlImage = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2024\icon.png");
            BitmapImage bitmapImage = new BitmapImage(urlImage);
            pushButton.LargeImage = bitmapImage;

            return Result.Succeeded;


        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        
    }
    [Transaction(TransactionMode.Manual)]
    public class MyTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            var uiapp = commandData.Application;
            var app = uiapp.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            using(Transaction transaction = new Transaction(doc, "Create Grids"))
            {
                transaction.Start();

               
                //creating horizontal grids
                Grid[] horizontalGrids = new Grid[10];
                for (int i = 0; i < horizontalGrids.Length; i++)
                {
                   
                    XYZ startPointHorizontal = new XYZ(50, (i-5)*10, 0);
                    XYZ endPointHorizontal = new XYZ(-50, (i-5)*10, 0);
                    
                    Line horizontalLine = Line.CreateBound(startPointHorizontal, endPointHorizontal);
                    string gridName = ((char)('a' + i)).ToString();
                    if (GridNameExists(doc, gridName))
                    {
                        // handling the case with already created grids
                        TaskDialog.Show("Error", $"Grids were already created.");
                        return Result.Failed; 
                    }
                    horizontalGrids[i] = Grid.Create(doc, horizontalLine);
                }


                //setting parameters of horizontal grids
                Parameter[] horizontalGridParameters = new Parameter[horizontalGrids.Length];
                for (int i = 0; i < horizontalGrids.Length; i++)
                {
                    horizontalGridParameters[i] = horizontalGrids[i].get_Parameter(BuiltInParameter.DATUM_TEXT);
                    if(GridNameExists(doc, ('a' + i).ToString())){
                        // handling the case with already created grids

                        TaskDialog.Show("Error", $"Grids were already created.");
                        return Result.Failed;
                    }
                        horizontalGridParameters[i].Set(((char)('a'+i)).ToString());
                }


                //creating vertical grids
                Grid[] verticalGrids = new Grid[10];
                for (int i = 0; i < verticalGrids.Length; i++)
                {

                    XYZ startPointVertical = new XYZ((i - 5) * 10, 50, 0);
                    XYZ endPointVertical = new XYZ((i - 5) * 10, -50, 0);

                    Line verticalLine = Line.CreateBound(startPointVertical, endPointVertical);

                    string gridName = (i + 1).ToString();
                    if (GridNameExists(doc, gridName))
                    {
                        // handling the case with already created grids

                        TaskDialog.Show("Error", $"Grids were already created.");
                        return Result.Failed;
                    }
                    verticalGrids[i] = Grid.Create(doc, verticalLine);
                }


                //setting vertical grids' parameters
                Parameter[] verticalGridParameters = new Parameter[verticalGrids.Length];
                for(int i = 0; i < verticalGrids.Length; i++)
                {
                   verticalGridParameters[i] = verticalGrids[i].get_Parameter(BuiltInParameter.DATUM_TEXT);
                    if (GridNameExists(doc, (i + 1).ToString())){
                        // handling the case with already created grids

                        TaskDialog.Show("Error", $"Grids were already created.");
                        return Result.Failed;
                    }
                        verticalGridParameters[i].Set((i +1).ToString());

                }

                transaction.Commit();

                return Result.Succeeded;

            }

        }
        private bool GridNameExists(Document doc, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Grids);
            collector.WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                if (element.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
