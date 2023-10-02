using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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


namespace CurrencyConverter_DB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection conn = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter adapter = new SqlDataAdapter();

        private int CurrencyID = 0;
        private double FromAmount = 0;
        private double ToAmount = 0;


        public MainWindow()
        {
            InitializeComponent();

            BindCurrency();

            GetData();
        }

        public void myConn()
        {
            String Conn = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            conn = new SqlConnection(Conn);

            conn.Open();
        }

        private void BindCurrency()
        {

            myConn();

            DataTable dt = new DataTable();
            cmd = new SqlCommand("select Id, CurrencyName from Currency_Master", conn);

            cmd.CommandType = CommandType.Text;

            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);

            DataRow newrow = dt.NewRow();
            newrow["Id"] = 0;
            newrow["CurrencyName"] = "--SELECT--";

            dt.Rows.InsertAt(newrow, 0);

            if (dt != null && dt.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dt.DefaultView;


                cmbToCurrency.ItemsSource = dt.DefaultView;
            }

            conn.Close();


            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }



            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                ConvertedValue = double.Parse(txtCurrency.Text);

                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");

            }
            else
            {
                ConvertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) *
                    double.Parse(txtCurrency.Text) /
                    double.Parse(cmbToCurrency.SelectedValue.ToString()));

                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Clearcontrols();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {


            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

        }

        private void Clearcontrols()
        {
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0)
            {
                cmbFromCurrency.SelectedIndex = 0;

            }
            if (cmbToCurrency.Items.Count > 0)
            {
                cmbToCurrency.SelectedIndex = 0;
                lblCurrency.Content = "";
                txtCurrency.Focus();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AmountTxtBox.Text == null || AmountTxtBox.Text.Trim() == "") {
                    MessageBox.Show("Please Enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    AmountTxtBox.Focus();
                    return;





                } else if ((CurrencyTxtBox.Text == null || CurrencyTxtBox.Text.Trim() == "")) {

                    MessageBox.Show("Please Enter Currency name", "Information", MessageBoxButton.OK, MessageBoxImage.Error);

                    CurrencyTxtBox.Focus();
                    return;
                } else
                {
                    if (CurrencyID > 0)
                    {
                        if (MessageBox.Show("Are you sure you want to update?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            myConn();

                            DataTable dt = new DataTable();

                            cmd = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", CurrencyID);
                            cmd.Parameters.AddWithValue("@Amount", AmountTxtBox.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", CurrencyTxtBox.Text);
                            cmd.ExecuteNonQuery();
                            conn.Close();

                            MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                    } else
                    {
                        if (MessageBox.Show("Are you sure you want to save?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            myConn();
                            cmd = new SqlCommand("INSERT INTO Currency_Master(Amount,CurrencyName) VALUES(@Amount, @CurrencyName)", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", AmountTxtBox.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", CurrencyTxtBox.Text);
                            cmd.ExecuteNonQuery();
                            conn.Close();
                            MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    ClearMaster();
                }



            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearMaster()
        {
            try
            {
                AmountTxtBox.Text = string.Empty;
                CurrencyTxtBox.Text = string.Empty;
                Save.Content = "Save";
                GetData();
                CurrencyID = 0;
                BindCurrency();
                AmountTxtBox.Focus();

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GetData()
        {
            myConn();
            DataTable dt = new DataTable();
            cmd = new SqlCommand("SELECT * FROM Currency_Master", conn);
            cmd.CommandType = CommandType.Text;
            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dt.DefaultView;

            }
            else
            {
                dgvCurrency.ItemsSource = null;
            }
            conn.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) {

            try
            {
                ClearMaster();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid grid = (DataGrid)sender;
                DataRowView row_selected = grid.CurrentItem as DataRowView;
                if (row_selected != null)
                {
                    if(dgvCurrency.Items.Count > 0)
                    {
                        if(grid.SelectedCells.Count > 0)
                        {
                            CurrencyID = Int32.Parse(row_selected["Id"].ToString());
                            if (grid.SelectedCells[0].Column.DisplayIndex == 0)
                            {
                                AmountTxtBox.Text = row_selected["Amount"].ToString();
                                CurrencyTxtBox.Text = row_selected["CurrencyName"].ToString();
                                Save.Content = "Update";
                            }
                            if (grid.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                if(MessageBox.Show("Are you sure you want to delete?","Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes){

                                    myConn();
                                    DataTable dt = new DataTable();
                                    cmd = new SqlCommand("DELETE FROM Currency_Master Where Id = @Id", conn);
                                    cmd.Parameters.AddWithValue("@Id", CurrencyID);
                                    cmd.ExecuteNonQuery();
                                    conn.Close();

                                    MessageBox.Show("Data deleted successfully","Information",MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearMaster();
                                }
                            }
                        }
                    }
                }


            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
