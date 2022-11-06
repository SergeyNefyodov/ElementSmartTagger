using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static System.Math;

namespace SmartTag
{
    /// <summary>
    /// Логика взаимодействия для TagForm.xaml
    /// </summary>
    /// 
    
    public partial class TagForm : Window
    {
        public TagForm(ExternalCommandData commandData)
        {
            InitializeComponent();
            SmartTagViewModel vm = new SmartTagViewModel(commandData);
            Mvvm = vm;
            vm.CloseRequest += (s, e) => this.Close();
            DataContext = vm;            
        }
        internal SmartTagViewModel Mvvm { get; set; }
        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            DuctTags.ItemsSource = (((sender as System.Windows.Controls.ComboBox).Parent as System.Windows.Controls.Grid).Parent as TagForm).Mvvm.symbolsTags;            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
           Close();
        }

    }
}
