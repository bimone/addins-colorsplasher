using Autodesk.Revit.DB;

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using Autodesk.Revit.ApplicationServices;
using System.Text.RegularExpressions;
using ColorSplasher.ViewModels;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace ColorSplasher.Views
{
    /// <summary>
    /// Interaction logic for ColorSplasherWindows.xaml
    /// </summary>

    public partial class ColorSplasherWindow : Window
    {
        readonly Random _random = new Random();
        readonly Document _doc;
        private ElementId _solidLineElementId;
        private ElementId _solidFillElementId;

        private readonly List<string> _excludedCategories = new List<string>()
        {
            "<Room separation>",
            "Cameras",
            "Curtain wall grids",
            "Elevations",
            "Grids",
            "Model Groups",
            "Property Line segments",
            "Section Boxes",
            "Shaft openings",
            "Structural beam systems",
            "Views",
            "Structural opening cut",
            "Structural trusses",
            "<Space separation>",
            "Duct systems",
            "Lines",
            "Piping systems",
            "Matchline",
            "Center line",
            "Curtain Roof Grids",
            "Rectangular Straight wall opening"
        };


        public ColorSplasherWindow(Document doc)
        {
            //====================================We use the same language as Revit=========================================
            Autodesk.Revit.ApplicationServices.Application application = doc.Application;
            LanguageType lang = application.Language;
            if (lang.ToString().Contains("French"))
            {
                var cultureInfo = new CultureInfo("fr-FR");
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = cultureInfo;
            }
            //====================================================================================================================================================
            InitializeComponent();
            var mainViewModel = new MainViewModel();
            // this creates an instance of the ViewModel
            DataContext = mainViewModel; // this sets the newly created ViewModel as the DataContext for the View

            _doc = doc;
            FillCategoryList();
            Title = "Color Splasher" + " " + Assembly.GetAssembly(GetType()).GetName().Version.ToString();
        }


        /// <summary>
        /// Elements coloring
        /// </summary>
        /// 
        public class ParametersWithType {
            public string Name { get; set; }
            public string Type { get; set; }
        }
        
        public class BuiltInCategories {
            public string Name { get; set; }
            public Category Category { get; set; }
            public List<ParametersWithType> Params { get; set; }
        }

        public class ParamValues {
            public string Value { get; set; }
            public bool Visible { get; set; }
            public Brush Colour { get; set; }
        }

        public class NaturalSortComparer<T> : IComparer<string>, IDisposable {
            private bool isAscending;
            public NaturalSortComparer(bool inAscendingOrder = true) {
                isAscending = inAscendingOrder;
            }
            public int Compare(string x, string y) {
                throw new NotImplementedException();
            }
            int IComparer<string>.Compare(string x, string y) {
                if (x == y)
                    return 0;

                string[] x1, y1;

                if (x.StartsWith("*"))
                    return -1;

                if (y.StartsWith("*"))
                    return 1;

                if (!table.TryGetValue(x, out x1)) {
                    x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
                    table.Add(x, x1);
                }

                if (!table.TryGetValue(y, out y1)) {
                    y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
                    table.Add(y, y1);
                }

                int returnVal;

                for (var i = 0; i < x1.Length && i < y1.Length; i++) {
                    if (x1[i] != y1[i]) {
                        returnVal = PartCompare(x1[i], y1[i]);
                        return isAscending ? returnVal : -returnVal;
                    }
                }

                if (y1.Length > x1.Length) {
                    returnVal = 1;
                } else if (x1.Length > y1.Length) {
                    returnVal = -1;
                } else {
                    returnVal = 0;
                }

                return isAscending ? returnVal : -returnVal;
            }
            private static int PartCompare(string left, string right) {
                int x, y;
                if (!int.TryParse(left, out x))
                    return left.CompareTo(right);

                if (!int.TryParse(right, out y))
                    return left.CompareTo(right);

                return x.CompareTo(y);
            }
            private Dictionary<string, string[]> table = new Dictionary<string, string[]>();
            public void Dispose() {
                table.Clear();
                table = null;
            }
        }

        private OverrideGraphicSettings ogs_standart = new OverrideGraphicSettings();
        private List<BuiltInCategories> bic = new List<BuiltInCategories>();

        private void FillCategoryList()
        {
            var collector = GetCollector();
            foreach (var e in collector)
            {
                if (null != e.Category)
                { //if it is categories
                    if (bic.All(x => x.Name != e.Category.Name))
                    { //creating list of categories which had been used into active model without duplicating
                        var eltParams = new List<ParametersWithType>();
                        foreach (Parameter p in e.Parameters)
                        {
                            if (p.Definition.Name != "Category")
                            {
                                if (eltParams.All(n => n.Name != p.Definition.Name))
                                    eltParams.Add(new ParametersWithType {Type = "d", Name = p.Definition.Name});
                            }
                        }
                        var et = e.Document.GetElement(e.GetTypeId());
                        if (et != null)
                        {
                            foreach (Parameter p in et.Parameters)
                            {
                                if (p.Definition.Name != "Category")
                                {
                                    if (eltParams.All(n => n.Name != p.Definition.Name))
                                    eltParams.Add(new ParametersWithType {Type = "t", Name = p.Definition.Name});
                                }
                            }
                        }

                        eltParams.Add(new ParametersWithType { Type = "m", Name = "Type" });
                        eltParams = eltParams.OrderBy(n => n.Name).ToList();
                        bic.Add(new BuiltInCategories
                        {
                            Name = e.Category.Name,
                            Category = e.Category,
                            Params = eltParams
                        });
                    }
                }
            }


            bic = bic.OrderBy(b => b.Name).ToList(); //sort category list
            lbCategory.ItemsSource = bic;
        }

        private void lbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) {
                lbColor.ItemsSource = null;
                lbProperties.ItemsSource = null;
            if (lbCategory.SelectedItems.Count>0)
            {
                var p = new List<ParametersWithType>();// bic[lbCategory.SelectedIndex].Params; //Fill parameters list from selected category
                foreach (var selectedItem in lbCategory.SelectedItems)
                {
                    p.AddRange(((BuiltInCategories)selectedItem).Params.Where(pa=> p.All(pl => pl.Name != pa.Name)));
                }
                lbProperties.ItemsSource = p;
            }
        }

        private void lbProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbProperties.SelectedItem == null)
                return;

            try
            {
                lbColor.ItemsSource = null;
                var usedcols = new List<Brush>();
                var paramssel = new List<ParamValues>(); //get all values from selected parameters
                var collector = GetCollector();
                var type = "";
                var selectedProperty = ((ParametersWithType) lbProperties.SelectedItem);

                var categoryNotHasParameter = lbCategory.SelectedItems.Cast<BuiltInCategories>().Where(c => !c.Params.Any(p => p.Name == selectedProperty.Name));
                foreach (var cat in categoryNotHasParameter)
                {
                    var t = false;
                    Brush brush = null;
                    while (!t)
                    {
                        brush = PickBrush();
                        if (!usedcols.Any(b => b.Equals(brush)))
                        {
                            usedcols.Add(brush);
                            break;
                        }
                    }
                    paramssel.Add(new ParamValues
                    {
                        Value = "*" + cat.Name + " (N/A)",
                        Visible = true,
                        Colour = brush
                    });
                }

                foreach (var elt in collector)
                {
                    if (null != elt.Category)
                    {
                        if (lbCategory.SelectedItems.Cast<BuiltInCategories>().Any(p => p.Name == elt.Category.Name))
                        {
                            var category = bic.FirstOrDefault(c => c.Name == elt.Category.Name);
                            var catParameter = category.Params.FirstOrDefault(p => p.Name == selectedProperty.Name);
                            if (catParameter == null)
                                continue;

                            type = catParameter.Type;
                            if (type == "d")
                            {
                                foreach (Parameter p in elt.Parameters)
                                {
                                    var s = "";
                                    if (selectedProperty.Name == p.Definition.Name)
                                    { 
                                        s = ParametersStorage(p);
                                        if (
                                            paramssel.All(
                                                x =>
                                                    x.Value !=
                                                    (string.IsNullOrEmpty(s) ? "*" + elt.Category.Name + " (N/A)" : s)))
                                        {
                                            //create random brush
                                            var t = false;
                                            Brush brush = null;
                                            while (!t)
                                            {
                                                brush = PickBrush();
                                                if (!usedcols.Any(b => b.Equals(brush)))
                                                {
                                                    usedcols.Add(brush);
                                                    break;
                                                }
                                            }
                                            paramssel.Add(new ParamValues
                                            {
                                                Value = string.IsNullOrEmpty(s) ? "*" + elt.Category.Name + " (N/A)" : s,
                                                Visible = true,
                                                Colour = brush
                                            });
                                        }
                                        // create color of each string
                                    }
                                }
                            }
                            else if (type == "t")
                            {
                                var et = elt.Document.GetElement(elt.GetTypeId());
                                foreach (Parameter p in et.Parameters)
                                {
                                    var s = "";
                                    if (selectedProperty.Name == p.Definition.Name)
                                    { 
                                        s = ParametersStorage(p);
                                        if (
                                            paramssel.All(
                                                x =>
                                                    x.Value !=
                                                    (string.IsNullOrEmpty(s) ? "*" + elt.Category.Name + " (N/A)" : s)))
                                        {
                                            //create random brush
                                            var t = false;
                                            Brush brush = null;
                                            while (!t)
                                            {
                                                brush = PickBrush();
                                                if (!usedcols.Any(b => b.Equals(brush)))
                                                {
                                                    usedcols.Add(brush);
                                                    break;
                                                }
                                            }
                                            paramssel.Add(new ParamValues
                                            {
                                                Value = string.IsNullOrEmpty(s) ? "*" + elt.Category.Name + " (N/A)" : s,
                                                Visible = true,
                                                Colour = brush
                                            });
                                        }
                                        // create color of each string
                                    }
                                }
                            }
                        }
                    }
                }
                if (type == "m")
                {
                    var cat = bic[lbCategory.SelectedIndex].Category;
                    var col = new FilteredElementCollector(_doc, _doc.ActiveView.Id);
                    var types = col.OfCategory((BuiltInCategory) cat.Id.IntegerValue).WhereElementIsNotElementType();
                    var lst = new List<string>();
                    foreach (var tp in types)
                    {
                        if (null != tp.Category)
                        {
                            var s = tp.Name;
                            if (!lst.Contains(s))
                                if (paramssel.All(x => x.Value != (string.IsNullOrEmpty(s) ? tp.Category.Name + " (N/A)" : s)))
                                {
                                    //create random brush
                                    var t = false;
                                    Brush brush = null;
                                    while (!t)
                                    {
                                        brush = PickBrush();
                                        if (!usedcols.Any(b => b.Equals(brush)))
                                        {
                                            usedcols.Add(brush);
                                            break;
                                        }
                                    }
                                    paramssel.Add(new ParamValues { Value = string.IsNullOrEmpty(s) ? tp.Category.Name + " (N/A)" : s, Visible = true, Colour = brush });
                                        // create color of each string
                                }
                        }
                    }
                }



                try
                {
                    paramssel = paramssel.OrderBy(x => x.Value, new NaturalSortComparer<string>()).ToList();
                    var i = 0;
                    foreach (var pos in paramssel)
                    {
                        if (null != ((ParamValues) pos).Value)
                            if (((ParamValues) pos).Value[0] == '-')
                                i++;
                    }
                    if (i != 0)
                    {
                        var paramssel1 = paramssel.Take(paramssel.Count).Reverse().Take(i).ToList();
                        paramssel.RemoveRange(paramssel.Count - i, i);
                        paramssel.InsertRange(0, paramssel1);
                    }
                }
                catch
                {
                    paramssel = paramssel.OrderBy(x => x.Value, new NaturalSortComparer<string>()).ToList();
                }
                lbColor.ItemsSource = paramssel.OrderBy(x => x.Value, new NaturalSortComparer<string>()).ToList();
            }
            catch
            {
                
            }
        }

        private void btnApplySet_Click(object sender, RoutedEventArgs e)
        {
            if(lbColor.Items == null || lbColor.Items.Count == 0)
                return;

            /*Nouveau code pour initialiser la constante - Ajouté le 11 sept 2019 par MP*/
            FilteredElementCollector fElementCollector = new FilteredElementCollector(_doc).OfClass(typeof(FillPatternElement));
            foreach (FillPatternElement fRow in fElementCollector)
            {
                if(fRow.GetFillPattern().IsSolidFill)
                {
                    _solidFillElementId = fRow.Id;
                    break;
                }
            }
            //Ancien code pour initialiser la constante
            /*_solidFillElementId =
                (from FillPatternElement fl in new FilteredElementCollector(_doc).OfClass(typeof(FillPatternElement))
                 where fl.Name.Equals("Solid fill") || fl.Name.Equals("<Solid fill>")
                 select fl.Id).FirstOrDefault();*/

            _solidLineElementId = new ElementId(-1);

            using (var t = new Transaction(_doc, "Set Element Override"))
            {
                t.Start();
                var collector = GetCollector();
                foreach (var elt in collector)
                {
                    if (null != elt.Category)
                    {
                        if (lbCategory.SelectedValue != null)
                        {
                            foreach (var cat in lbCategory.SelectedItems.Cast<BuiltInCategories>().Where(c => c.Name == elt.Category.Name))
                            {
                                var param =
                                    cat.Params.FirstOrDefault(
                                        p => p.Name == lbProperties.SelectedValue.ToString());
                                if (param != null)
                                {
                                    var type = param.Type;
                                    if (type == "d")
                                        foreach (Parameter p in elt.Parameters)
                                        {
                                            if (p.Definition.Name == param.Name)
                                            {
                                                var s = ParametersStorage(p);
                                                if (SetColor(elt, s))
                                                    break;
                                            }
                                        }
                                    else if (type == "t")
                                    {
                                        var et = elt.Document.GetElement(elt.GetTypeId());
                                        foreach (Parameter p in et.Parameters)
                                        {
                                            if (p.Definition.Name == param.Name)
                                            {
                                                var s = ParametersStorage(p);
                                                if(SetColor(elt, s))
                                                    break;
                                            }
                                        }
                                    }
                                    else if (type == "m")
                                    {

                                        var et = elt.Document.GetElement(elt.GetTypeId());
                                        var s = et.Name;
                                        if (SetColor(elt, s))
                                            break;

                                    }
                                }
                                else
                                {
                                    var s = "*" + cat.Name + " (N/A)";
                                    if (SetColor(elt, s))
                                        break;
                                }
                            }
                        }
                    }
                }

                t.Commit();
            }
        }

        private bool SetColor(Element elt, string s)
        {
            foreach (ParamValues lbi in lbColor.Items)
            {
                if (s == (string.IsNullOrEmpty(lbi.Value) ? "*" + elt.Category.Name + " (N/A)" : lbi.Value))
                {
                    var clr =
                        (Color)
                            ColorConverter.ConvertFromString(
                                lbi.Colour.ToString());
                    var ogs = new OverrideGraphicSettings();
                    //var elementOgs = _doc.ActiveView.GetElementOverrides(elt.Id);

                    //get the random color and override active view
                    var color = new Autodesk.Revit.DB.Color(clr.R, clr.G, clr.B);

      
                    ogs.SetProjectionLineColor(color);
                    //Ajout par MP le 6sept2019
                        ogs.SetSurfaceForegroundPatternColor(color);
                        ogs.SetCutForegroundPatternColor(color);
                    if (_solidFillElementId != null)
                    {
                            ogs.SetSurfaceForegroundPatternId(_solidFillElementId);
                            ogs.SetCutForegroundPatternId(_solidFillElementId);
                    }


                    if (_solidLineElementId != null)
                    {
                        ogs.SetProjectionLinePatternId(_solidLineElementId);
                    }

                    _doc.ActiveView.SetElementOverrides(elt.Id, ogs);
                    return true;
                }
            }

            return false;
        }

        private IList<Element> GetCollector()
        {
            return new FilteredElementCollector(_doc, _doc.ActiveView.Id).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .ToElements().Where(c=> _excludedCategories.All(ec=> c.Category !=null && !ec.Equals(c.Category.Name,StringComparison.InvariantCultureIgnoreCase))).ToList();
        }
        private string ParametersStorage(Parameter p) {
            var s = "";
            switch (p.StorageType) {
                case StorageType.Double:
                    s = p.AsValueString();
                    break;

                case StorageType.ElementId:
                    var id = p.AsElementId();
                    if (id.IntegerValue >= 0) 
                        s = _doc.GetElement(id).Name;
                    else {
                        s = id.IntegerValue.ToString();
                        if (s == "-1")
                            s = "None";
                    }
                    break;

                case StorageType.Integer:
                    if (SpecTypeId.Boolean.YesNo == p.Definition.GetDataType())
                    {
                        if (p.AsInteger() == 0) 
                            s = "False";
                         else 
                            s = "True";
                    } else 
                        s = p.AsValueString();
                    break;

                case StorageType.String:
                    s = p.AsString();
                    break;

                default:
                    s = "Unexposed parameter.";
                    break;
            }
            return s;
        }

        private void btnClearSet_Click(object sender, RoutedEventArgs e)
        {
            //var filter = GetFilter();
            var collector = GetCollector();
                //new FilteredElementCollector(_doc).WherePasses(filter)
                //    .UnionWith(new FilteredElementCollector(_doc).WhereElementIsNotElementType());
            using (var t = new Transaction(_doc, "Set Element Override"))
            {
                t.Start();
                foreach (var elt in collector)
                {
                    _doc.ActiveView.SetElementOverrides(elt.Id, ogs_standart); // apply standart override
                }
                t.Commit();
            }
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private SolidColorBrush colorPicker() {
            var colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            if(colorDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            return new SolidColorBrush(Color.FromRgb(
                                       colorDialog.Color.R,
                                       colorDialog.Color.G,
                                       colorDialog.Color.B
                                       ));
            return null;
        }
        private void lbColor_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (lbColor.SelectedIndex != -1) {
                var paramssel = lbColor.ItemsSource as List<ParamValues>;
                var color = colorPicker();
                if (paramssel != null && color != null)
                {
                    paramssel[lbColor.SelectedIndex].Colour = color;
                    lbColor.ItemsSource = null;
                    lbColor.ItemsSource = paramssel;
                }
            }
        }

        private Brush PickBrush() {
            Brush result = Brushes.Transparent;
            var colorval = "";
            try
            {
                
                //Type brushesType = typeof(Brushes);
                //PropertyInfo[] properties = brushesType.GetProperties();
                colorval = "#" + String.Format("{0:X}", _random.Next(0xFFFFFF)).PadLeft(6, '0');
                var converter = new BrushConverter();
                result = (Brush)converter.ConvertFromString(colorval);
            }
            catch (Exception e)
            {
            }
            return result;
        }
        private void btnSvColorSchema_Click(object sender, RoutedEventArgs e) {
            var paramssel = lbColor.ItemsSource as List<ParamValues>;
            if (paramssel != null) {
                var colorSchmsFolder = Path.GetDirectoryName(typeof(ColorSplasherWindow).Assembly.Location) + "\\ColorSchemes";
                if (!Directory.Exists(colorSchmsFolder))
                    Directory.CreateDirectory(colorSchmsFolder);
                var sfd = new SaveFileDialog();
                sfd.InitialDirectory = colorSchmsFolder;
                sfd.Filter = "Color scheme files (*.csch)|*.csch|All Files (*.*)|*.*";
                sfd.FilterIndex = 1;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    using (var writetext = new StreamWriter(sfd.FileName)) {
                        foreach (var par in paramssel)
                            writetext.WriteLine(par.Value + ":" + par.Colour);
                    }
                }
            }
            else
            { 
                MessageBox.Show(Resource.MsgBox_PleaseSelectParameterFirst, Resource.MsgBox_SelectAParameter, MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
        }
        private void btnLdColorSchema_Click(object sender, RoutedEventArgs e) {
            var paramssel = lbColor.ItemsSource as List<ParamValues>;
            if (paramssel != null)
            {
                var ofd = new OpenFileDialog();
                ofd.InitialDirectory = Path.GetDirectoryName(typeof(ColorSplasherWindow).Assembly.Location) + "\\ColorSchemes";
                ofd.Filter = "Color scheme files (*.csch)|*.csch|All Files (*.*)|*.*";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var colorList = new List<Brush>();
                    using (var reader = new StreamReader(ofd.FileName))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var dt = line.Split(':');
                            for (var i = 0; i < paramssel.Count; i++)
                            {
                                if (paramssel[i].Value == dt[0])
                                {
                                    var bc = new BrushConverter();
                                    paramssel[i].Colour = (Brush)bc.ConvertFrom(dt[1]);
                                }
                            }
                        }
                    }
                    lbColor.ItemsSource = null;
                    lbColor.ItemsSource = paramssel;
                }
            }
            else
            {
                MessageBox.Show(Resource.MsgBox_PleaseSelectParameterFirst, Resource.MsgBox_SelectAParameter, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void btnRefreshColors_Click(object sender, RoutedEventArgs e) {
            var paramssel = lbColor.ItemsSource as List<ParamValues>;
            if (paramssel != null) {
                var usedcols = new List<Brush>();
                var t = false;
                for (var i = 0; i < paramssel.Count; i++) {
                    Brush brush = null;
                    while (!t) {
                        brush = PickBrush();
                        if (!usedcols.Contains(brush)) {
                            usedcols.Add(brush);
                            break;
                        }
                    }
                    paramssel[i].Colour = brush;
                }
                lbColor.ItemsSource = null;
                lbColor.ItemsSource = paramssel;
            }
        }
        private void btnRainbowColors_Click(object sender, RoutedEventArgs e) {
            var paramssel = lbColor.ItemsSource as List<ParamValues>;
            if (paramssel != null)
            {
                var clrStart = (Color)ColorConverter.ConvertFromString(paramssel[0].Colour.ToString());
                var clrEnd = (Color)ColorConverter.ConvertFromString(paramssel[paramssel.Count - 1].Colour.ToString());
                var clr1 = System.Drawing.Color.FromArgb(clrStart.R, clrStart.G, clrStart.B);
                var clr2 = System.Drawing.Color.FromArgb(clrEnd.R, clrEnd.G, clrEnd.B);
                var clrs = GetGradientColors(clr1, clr2, paramssel.Count);

                if ((paramssel != null) & (clrs != null))
                {
                    for (var i = 0; i < paramssel.Count - 1; i++)
                        paramssel[i].Colour = new SolidColorBrush(Color.FromRgb(
                                 clrs[i].R,
                                 clrs[i].G,
                                 clrs[i].B
                                 ));
                }
                lbColor.ItemsSource = null;
                lbColor.ItemsSource = paramssel;
            }
        }
        public static List<System.Drawing.Color> GetGradientColors(System.Drawing.Color start, System.Drawing.Color end, int steps) {
            return GetGradientColors(start, end, steps, 0, steps);
        }
        public static List<System.Drawing.Color> GetGradientColors(System.Drawing.Color start, System.Drawing.Color end, int steps, int firstStep, int lastStep) {
            var colorList = new List<System.Drawing.Color>();
            if (steps <= 0 || firstStep < 0 || lastStep > steps)
                return colorList;

            double aStep = (double)(end.A - start.A) / steps;
            double rStep = (double)(end.R - start.R) / steps;
            double gStep = (double)(end.G - start.G) / steps;
            double bStep = (double)(end.B - start.B) / steps;

            for (var i = firstStep; i < lastStep; i++) {
                var a = start.A + (int)(aStep * i);
                var r = start.R + (int)(rStep * i);
                var g = start.G + (int)(gStep * i);
                var b = start.B + (int)(bStep * i);
                colorList.Add(System.Drawing.Color.FromArgb(a, r, g, b));
            }
            return colorList;
        }
      
    }
}
