using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartTag
{
    /// <summary>
    /// Логика взаимодействия для UserControl2.xaml
    /// </summary>

    public class ReportInfo
    {
        public enum ElemPartType
        {
            Pipe,
            Elbow,
            Valve,
            Equipment
        }
        public ReportInfo(string level, string system, string name, int diameter, double length, double Q, double dp)
        {
            this.Level = level;
            this.System = system;
            Name = name;
            Diameter = diameter;
            Length = length;
            this.Q = Q;
            dP = dp;
        }
        public string Location { get; set; } = "Расположение";
        public string Level { get; set; } = "Уровень";
        public string System { get; set; } = "П1";
        public ElemPartType PartType { get; set; } = ElemPartType.Pipe;
        public string Name { get; set; } = "Отвод стальной";
        public int Diameter { get; set; } = 100;
        public double Length { get; set; } = 1;
        public double Q { get; set; } = 0.5;
        public double dP { get; set; } = 0.02;
        public double dP_l
        {
            get
            { return dP / Length; }
        }
        public double dPT { get; set; } = 12.45;
        public string Note { get; set; } = string.Empty;


    }
    public partial class MagicForm : Window
    {
        public MagicForm()
        {
            InitializeComponent();
        }
        private void grid_Loaded(object sender, RoutedEventArgs e)
        {
            grid_supply.Columns[0].Header = "Расположение";
            grid_supply.Columns[1].Header = "Уровень";
            grid_supply.Columns[2].Header = "Система";
            grid_supply.Columns[3].Header = "Элемент";
            grid_supply.Columns[4].Header = "Наименование";
            grid_supply.Columns[5].Header = "Диаметр, мм";
            grid_supply.Columns[6].Header = "Длина, м";
            grid_supply.Columns[7].Header = "Расход, м³/ч";
            grid_supply.Columns[8].Header = "dP, Па";
            grid_supply.Columns[9].Header = "dP/L, Па/м";
            grid_supply.Columns[10].Header = "P полное";
            grid_supply.Columns[11].Header = "Примечание";
            grid_return.Columns[0].Header = "Расположение";
            grid_return.Columns[1].Header = "Уровень";
            grid_return.Columns[2].Header = "Система";
            grid_return.Columns[3].Header = "Элемент";
            grid_return.Columns[4].Header = "Наименование";
            grid_return.Columns[5].Header = "Диаметр, мм";
            grid_return.Columns[6].Header = "Длина, м";
            grid_return.Columns[7].Header = "Расход, м³/ч";
            grid_return.Columns[8].Header = "dP, Па";
            grid_return.Columns[9].Header = "dP/L, Па/м";
            grid_return.Columns[10].Header = "P полное";
            grid_return.Columns[11].Header = "Примечание";



        }

        private void SupplyFlag_Checked(object sender, RoutedEventArgs e)
        {

            try
            {
                if (ReturnFlag.IsChecked != null && ReturnFlag.IsChecked == true)
                {
                    grid_supply.Visibility = Visibility.Hidden;
                    grid_return.Visibility = Visibility.Visible;
                }
                else if (ReturnFlag.IsChecked != null)
                {
                    grid_return.Visibility = Visibility.Hidden;
                    grid_supply.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }

        private void ReturnFlag_Checked(object sender, RoutedEventArgs e)
        {

            try
            {
                if (SupplyFlag.IsChecked != null && SupplyFlag.IsChecked == false)
                {
                    grid_supply.Visibility = Visibility.Hidden;
                    grid_return.Visibility = Visibility.Visible;
                }
                else if (SupplyFlag.IsChecked != null)
                {
                    grid_return.Visibility = Visibility.Hidden;
                    grid_supply.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }
    }

}
