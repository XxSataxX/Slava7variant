using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DepositApp
{
    public partial class MainWindow : Window
    {
        private List<Deposit> deposits = new List<Deposit>();
        private Deposit selectedDeposit;

        public MainWindow()
        {
            InitializeComponent();
            UpdateTotalAmount();
        }

        private void AddDepositButton_Click(object sender, RoutedEventArgs e)
        {
            AddDepositPopup.Visibility = Visibility.Visible;
        }

        private void AddDepositPopupButton_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = AccountNumberTextBox.Text;
            string accountHolder = AccountHolderTextBox.Text;

            if (accountNumber == "Номер счета" || accountHolder == "Владелец счета" ||
                !decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0 ||
                !DateTime.TryParse(StartDateTextBox.Text, out DateTime startDate) ||
                !DateTime.TryParse(EndDateTextBox.Text, out DateTime endDate) || endDate <= startDate ||
                !decimal.TryParse(InterestRateTextBox.Text, out decimal interestRate) || interestRate <= 0)
            {
                MessageBox.Show("Некорректный ввод данных");
                return;
            }

            Deposit deposit = new Deposit(accountNumber, accountHolder, amount, startDate, endDate, interestRate);
            deposits.Add(deposit);
            UpdateDepositsList();
            UpdateTotalAmount();

            AddDepositPopup.Visibility = Visibility.Collapsed;
        }

        private void CancelDepositPopupButton_Click(object sender, RoutedEventArgs e)
        {
            AddDepositPopup.Visibility = Visibility.Collapsed;
        }

        private void UpdateDepositsList()
        {
            DepositsListBox.Items.Clear();
            foreach (var deposit in deposits)
            {
                var item = new ListBoxItem
                {
                    Content = $"{deposit.AccountHolder} - {deposit.Amount:C} - {deposit.EndDate:yyyy-MM-dd}",
                    Tag = deposit
                };
                if (deposit.IsExpired())
                {
                    item.Background = Brushes.Red;
                }
                else if (deposit.IsAboutToExpire())
                {
                    item.Background = Brushes.Yellow;
                }
                DepositsListBox.Items.Add(item);
            }
        }

        private void UpdateTotalAmount()
        {
            decimal totalAmount = deposits.Sum(d => d.Amount);
            TotalAmountTextBlock.Text = $"Общая сумма: {totalAmount:C}";
        }

        private void ApplyInterestButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var deposit in deposits)
            {
                deposit.ApplyInterest();
            }
            UpdateDepositsList();
            UpdateTotalAmount();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            deposits.Clear();
            UpdateDepositsList();
            UpdateTotalAmount();
        }

        private void DepositsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepositsListBox.SelectedItem is ListBoxItem selectedItem)
            {
                selectedDeposit = selectedItem.Tag as Deposit;
                SelectedDepositTextBlock.Text = $"Вклад: {selectedDeposit.AccountHolder} - {selectedDeposit.Amount:C}";
                ManageDepositPopup.Visibility = Visibility.Visible;
            }
        }

        private void AddAmountButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(ManageAmountTextBox.Text, out decimal amount) && amount > 0)
            {
                selectedDeposit.AddAmount(amount);
                UpdateDepositsList();
                UpdateTotalAmount();
                ManageDepositPopup.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Некорректная сумма");
            }
        }

        private void WithdrawAmountButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(ManageAmountTextBox.Text, out decimal amount) && amount > 0)
            {
                selectedDeposit.WithdrawAmount(amount);
                UpdateDepositsList();
                UpdateTotalAmount();
                ManageDepositPopup.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Некорректная сумма");
            }
        }

        private void CancelManageDepositPopupButton_Click(object sender, RoutedEventArgs e)
        {
            ManageDepositPopup.Visibility = Visibility.Collapsed;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Номер счета" || textBox.Text == "Владелец счета" || textBox.Text == "Сумма" ||
                textBox.Text == "Дата начала (гггг-мм-дд)" || textBox.Text == "Дата окончания (гггг-мм-дд)" || textBox.Text == "Процентная ставка (%)")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Foreground = Brushes.Gray;
                if (textBox.Name == "AccountNumberTextBox") textBox.Text = "Номер счета";
                else if (textBox.Name == "AccountHolderTextBox") textBox.Text = "Владелец счета";
                else if (textBox.Name == "AmountTextBox") textBox.Text = "Сумма";
                else if (textBox.Name == "StartDateTextBox") textBox.Text = "Дата начала (гггг-мм-дд)";
                else if (textBox.Name == "EndDateTextBox") textBox.Text = "Дата окончания (гггг-мм-дд)";
                else if (textBox.Name == "InterestRateTextBox") textBox.Text = "Процентная ставка (%)";
            }
        }
    }

    public class Deposit
    {
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal InterestRate { get; set; }

        public Deposit(string accountNumber, string accountHolder, decimal amount, DateTime startDate, DateTime endDate, decimal interestRate)
        {
            AccountNumber = accountNumber;
            AccountHolder = accountHolder;
            Amount = amount;
            StartDate = startDate;
            EndDate = endDate;
            InterestRate = interestRate;
        }

        public void AddAmount(decimal amount)
        {
            Amount += amount;
        }

        public void WithdrawAmount(decimal amount)
        {
            if (Amount >= amount)
            {
                Amount -= amount;
            }
        }

        public void ApplyInterest()
        {
            if (EndDate > DateTime.Now)
            {
                Amount += Amount * (InterestRate / 100);
            }
        }

        public bool IsExpired()
        {
            return EndDate <= DateTime.Now;
        }

        public bool IsAboutToExpire()
        {
            return EndDate <= DateTime.Now.AddDays(5) && EndDate > DateTime.Now;
        }
    }
}
