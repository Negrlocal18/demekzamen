using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    using myAgent = demekzamen.Agent;
    using myAgentType = demekzamen.AgentType;
    using myProduct = demekzamen.Product;
    using myProductSale = demekzamen.ProductSale;

    public partial class Page3 : Page
    {
        private myAgent agent;
        private int curSelPr = 0; // Объявление переменной для хранения идентификатора выбранного продукта
        private int curTypAg = 0; // Объявление переменной для хранения типа агента

        public Page3(myAgent ag)
        {
            InitializeComponent();
            try
            {
                Type.ItemsSource = helper.GetContext().AgentType.ToList();
                product.ItemsSource = helper.GetContext().Product.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
            if (ag != null)
            {
                agent = ag;
                Type.SelectedItem = ag.AgentType;
                this.Title.Text = ag.Title;
                this.Adress.Text = ag.Address;
                this.Inn.Text = ag.INN;
                this.Kpp.Text = ag.KPP;
                this.Director.Text = ag.DirectorName;
                this.Phone.Text = ag.Phone;
                this.Prioritet.Text = ag.Priority.ToString();
                historyGrid.ItemsSource = helper.GetContext().ProductSale.Where(ProductSale => ProductSale.AgentID == ag.ID).ToList();
            }
            else
            {
                agent = new myAgent();
                btnDelAg.IsEnabled = false;
                btnWritHi.IsEnabled = false;
                btnDelHi.IsEnabled = false;
            }
            this.DataContext = agent;
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            myAgentType selectedItem = (myAgentType)comboBox.SelectedItem;
            if (selectedItem != null)
            {
                Console.WriteLine("Выбранный элемент: " + selectedItem.Title);
                curTypAg = selectedItem.ID;
            }
        }

        private void mask_TextChanged(object sender, TextChangedEventArgs e)
        {
            string fnd = ((TextBox)sender).Text;
            try
            {
                product.ItemsSource = helper.GetContext().Product.Where(Product => Product.Title.Contains(fnd)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering products: " + ex.Message);
            }
        }

        private void product_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curSelPr = ((myProduct)product.SelectedItem).ID;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int cnt = 0;
            try
            {
                cnt = Convert.ToInt32(count.Text);
            }
            catch
            {
                MessageBox.Show("Invalid count");
                return;
            }
            DateTime? dt = ((DatePicker)date).SelectedDate;
            if (curSelPr > 0 && dt.HasValue && cnt > 0)
            {
                myProductSale pr = new myProductSale();
                pr.AgentID = agent.ID;
                pr.ProductID = curSelPr;
                pr.SaleDate = dt.Value;
                pr.ProductCount = cnt;
                try
                {
                    helper.GetContext().ProductSale.Add(pr);
                    helper.GetContext().SaveChanges();
                    historyGrid.ItemsSource = helper.GetContext().ProductSale.Where(ProductSale => ProductSale.AgentID == agent.ID).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding product sale: " + ex.Message);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (curTypAg == 0) return;
            if (this.Title.Text == "") return;
            if (!(new Regex(@"\d{10}|\d{12}")).IsMatch(this.Inn.Text)) return;
            if (!(new Regex(@"\d{4}[\dA-Z][\dA-Z]\d{3}")).IsMatch(this.Kpp.Text)) return;
            if (!(new Regex(@"^\+?\d{0,2}\-?\d{3}\-?\d{3}\-?\d{4}")).IsMatch(this.Phone.Text)) return;
            if ((this.Email.Text != "") && (!(new Regex(@"(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)")).IsMatch(this.Email.Text))) return;
            agent.Title = this.Title.Text;
            agent.AgentTypeID = curTypAg;
            agent.Address = this.Adress.Text;
            agent.INN = this.Inn.Text;
            agent.KPP = this.Kpp.Text;
            agent.Phone = this.Phone.Text;
            agent.DirectorName = this.Director.Text;
            agent.Email = this.Email.Text; // Обновление свойства Email
            try
            {
                agent.Priority = Convert.ToInt32(this.Prioritet.Text);
            }
            catch
            {
                return;
            }
            try
            {
                if (agent.ID > 0)
                {
                    helper.GetContext().Entry(agent).State = EntityState.Modified;
                    helper.GetContext().SaveChanges();
                    MessageBox.Show("Обновление информации об агенте завершено");
                }
                else
                {
                    helper.GetContext().Agent.Add(agent);
                    helper.GetContext().SaveChanges();
                    MessageBox.Show("Добавление информации об агенте завершено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving agent: " + ex.Message);
            }
            btnDelAg.IsEnabled = true;
            btnWritHi.IsEnabled = true;
            btnDelHi.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (agent.ID > 0)
            {
                try
                {
                    foreach (Shop shop in agent.Shop)
                    {
                        helper.GetContext().Shop.Remove(shop);
                    }
                    foreach (AgentPriorityHistory apr in agent.AgentPriorityHistory)
                    {
                        helper.GetContext().AgentPriorityHistory.Remove(apr);
                    }
                    helper.GetContext().Agent.Remove(agent);
                    helper.GetContext().SaveChanges();
                    MessageBox.Show("Удаление информации об агенте завершено");
                    this.NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting agent: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Нет данных для удаления");
            }
        }



        private void historyGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработка события выбора строки в DataGrid
            // Добавьте необходимую логику здесь
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < historyGrid.SelectedItems.Count; i++)
            {
                myProductSale prs = historyGrid.SelectedItems[i] as myProductSale;
                if (prs != null)
                {
                    helper.GetContext().ProductSale.Remove(prs);
                }
            }
            try
            {
                helper.GetContext().SaveChanges();
                historyGrid.ItemsSource = helper.GetContext().ProductSale.Where(ProductSale => ProductSale.AgentID == agent.ID).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting product sale: " + ex.Message);
            }
        }
    }
}