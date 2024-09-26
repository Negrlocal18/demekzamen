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

namespace demekzamen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public class helper
    {
    public static Entities ent;
    public static Entities GetContext()
    {
        if (ent == null)
        {
            ent = new Entities();
        }
        return ent;
    }
        public static bool flag { get; set; }
        public static int prioritet { get; set; }

    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            frame.Content = new Page1(frame);

        }

        private void frame_LoadCompleted(object sender, NavigationEventArgs e)
        {

        }
    }
}
