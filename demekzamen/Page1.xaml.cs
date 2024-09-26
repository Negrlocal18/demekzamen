using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    public partial class Page1 : Page
    {
        private int start = 0;
        private int fullCount = 0;
        private int order = 0; // Переменная для хранения текущего порядка сортировки
        private Frame fr; // Добавлена переменная для хранения ссылки на Frame
        private string fnd = string.Empty; // Переменная для хранения поискового запроса
        private int iag = 0; // Переменная для хранения типа агента

        public void Load()
        {
            try
            {
                var ag = helper.GetContext().Agent
                    .Where(Agent => string.IsNullOrEmpty(fnd) ||
                                  Agent.Title.Contains(fnd) ||
                                  Agent.Phone.Contains(fnd) ||
                                  Agent.Email.Contains(fnd));

                if (iag > 0)
                {
                    ag = ag.Where(Agent => Agent.AgentTypeID == iag);
                }

                fullCount = ag.Count();
                full.Text = fullCount.ToString();
                int ost = fullCount % 10;
                int pag = (fullCount - ost) / 10;
                if (ost > 0) pag++;
                pagin.Children.Clear();
                for (int i = 0; i < pag; i++)
                {
                    Button myButton = new Button();
                    myButton.Height = 30;
                    myButton.Content = i + 1;
                    myButton.Width = 20;
                    myButton.HorizontalAlignment = HorizontalAlignment.Center;
                    myButton.Tag = i;
                    myButton.Click += new RoutedEventHandler(paginButto_Click);
                    pagin.Children.Add(myButton);
                }

                switch (order)
                {
                    case 0:
                        agentGrid.ItemsSource = ag.OrderBy(Agent => Agent.ID).Skip(start * 10).Take(10).ToList();
                        break;
                    case 1:
                        agentGrid.ItemsSource = ag.OrderBy(Agent => Agent.Title).Skip(start * 10).Take(10).ToList();
                        break;
                    case 2:
                        agentGrid.ItemsSource = ag.OrderByDescending(Agent => Agent.Title).Skip(start * 10).Take(10).ToList();
                        break;
                    case 3:
                        agentGrid.ItemsSource = ag.OrderBy(Agent => Agent.Priority).Skip(start * 10).Take(10).ToList();
                        break;
                    case 4:
                        agentGrid.ItemsSource = ag.OrderByDescending(Agent => Agent.Priority).Skip(start * 10).Take(10).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
                return;
            }

            turnButton();
        }

        private void turnButton()
        {
            if (start == 0) { back.IsEnabled = false; }
            else { back.IsEnabled = true; };
            if ((start + 1) * 10 > fullCount) { forward.IsEnabled = false; }
            else { forward.IsEnabled = true; };
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            start--;
            Load();
        }

        private void forward_Click(object sender, RoutedEventArgs e)
        {
            start++;
            Load();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            fnd = ((TextBox)sender).Text;
            Load();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            order = Convert.ToInt32(selectedItem.Tag.ToString());
            Load();
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            AgentType selectedItem = (AgentType)comboBox.SelectedItem;
            if (selectedItem != null)
            {
                Console.WriteLine("Выбранный элемент: " + selectedItem.Title);
                iag = selectedItem.ID;
                Load();
            }
        }

        private void paginButto_Click(object sender, RoutedEventArgs e)
        {
            start = Convert.ToInt32(((Button)sender).Tag.ToString());
            Load();
        }

        public Page1(Frame frame)
        {
            this.fr = frame;
            InitializeComponent();
            List<AgentType> agents = new List<AgentType> { };
            agents = helper.GetContext().AgentType.ToList();
            agents.Add(new AgentType { Title = "Все типы" });
            Type.ItemsSource = agents.OrderBy(AgentType => AgentType.ID);
            Load();
        }

        private void agentGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Agent agent = (Agent)e.Row.DataContext;
            if (agent != null)
            {
                int sum = 0;
                double fsum = 0;
                foreach (ProductSale ps in agent.ProductSale)
                {
                    List<ProductMaterial> mtr = new List<ProductMaterial> { };
                    mtr = helper.GetContext().ProductMaterial.Where(ProductMaterial => ProductMaterial.ProductID == ps.ProductID).ToList();
                    foreach (ProductMaterial mt in mtr)
                    {
                        double f = decimal.ToDouble(mt.Material.Cost);
                        fsum += f * (double)mt.Count;
                    }
                    fsum = fsum * ps.ProductCount;
                    if (ps.SaleDate.AddDays(365).CompareTo(DateTime.Today) > 0)
                        sum += ps.ProductCount;
                }
                agent.sale = sum;
                agent.percent = 0;
                if (fsum >= 10000 && fsum < 50000) agent.percent = 5;
                if (fsum >= 50000 && fsum < 150000) agent.percent = 10;
                if (fsum >= 150000 && fsum < 500000) agent.percent = 20;
                if (fsum >= 500000) agent.percent = 25;
                if (agent.percent == 25)
                {
                    SolidColorBrush hb = new SolidColorBrush(Colors.LightGreen);
                    e.Row.Background = hb;
                }
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            if (agentGrid.SelectedItems.Count > 0)
            {
                int prt = 0;
                foreach (Agent agent in agentGrid.SelectedItems)
                {
                    if (agent.Priority > prt) prt = agent.Priority;
                }
                Window2 dlg = new Window2(prt);
                helper.prioritet = prt;
                helper.flag = false;
                dlg.ShowDialog();
                if (helper.flag)
                {
                    foreach (Agent agent in agentGrid.SelectedItems)
                    {
                        agent.Priority = helper.prioritet;
                        helper.GetContext().Entry(agent).State = EntityState.Modified;
                    }
                    helper.GetContext().SaveChanges();
                    Load();
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (agentGrid.SelectedItems.Count > 0)
            {
                foreach (Agent agent in agentGrid.SelectedItems)
                {
                    try
                    {
                        helper.GetContext().Agent.Remove(agent);
                        helper.GetContext().SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting agent: " + ex.Message);
                    }
                }
                Load();
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            fr.Content = new Page3(null);
        }

        private void agentGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (agentGrid.SelectedItems.Count > 0)
            {
                Agent agent = agentGrid.SelectedItems as Agent;

                if (agent != null)
                {
                    fr.Content = new Page3(agent);
                }
            }
        }

        private void revButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}