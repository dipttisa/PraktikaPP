using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace PraktikaPP
{
    /// <summary>
    /// Логика взаимодействия для LogInWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        private PractikaDB _context;
        private string _captcha;
        private int _attemptsLeft = 3;
        private bool _isBlocked = false;
        private bool _captchaShown = false; // Флаг для отслеживания отображения CAPTCHA
        private DispatcherTimer _blockTimer; // Таймер для блокировки
        private int _secondsLeft; // Количество оставшихся секунд блокировки

        public LogInWindow()
        {
            InitializeComponent();
            _context = new PractikaDB();
            LoadUsers();

            // Инициализация таймера
            _blockTimer = new DispatcherTimer();
            _blockTimer.Interval = TimeSpan.FromSeconds(1);
            _blockTimer.Tick += BlockTimer_Tick;
        }

        private void LoadUsers()
        {
            var users = _context.users.ToList();
            LoginComboBox.ItemsSource = users;
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            _captcha = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            CaptchaTextBlock.Text = _captcha;
        }

        private void RefreshCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBlocked)
            {
                MessageBox.Show($"Окно авторизации заблокировано. Осталось {_secondsLeft} секунд.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedUser = LoginComboBox.SelectedItem as users;
            if (selectedUser == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var enteredPassword = PasswordBox.Password;
            var enteredCaptcha = CaptchaTextBox.Text;

            // Проверка пароля
            if (!VerifyPassword(selectedUser.password, enteredPassword))
            {
                _attemptsLeft--;
                AttemptsTextBlock.Text = $"Осталось попыток: {_attemptsLeft}";

                if (!_captchaShown) // Если CAPTCHA еще не показано
                {
                    ShowCaptcha(); // Показываем CAPTCHA
                    _captchaShown = true; // Устанавливаем флаг
                }
                else if (enteredCaptcha != _captcha) // Если CAPTCHA уже показано, проверяем его
                {
                    MessageBox.Show("Неверный CAPTCHA.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    GenerateCaptcha();
                    return;
                }

                if (_attemptsLeft == 0)
                {
                    _isBlocked = true;
                    _secondsLeft = 30;
                    _blockTimer.Start(); // Запускаем таймер
                    MessageBox.Show($"Окно авторизации заблокировано на 30 секунд.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Неверный пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Передача id пользователя в окно навигации
                var navigationWindow = new MainWindow(selectedUser.id);
                navigationWindow.Show();
                this.Close();
            }
        }

        private bool VerifyPassword(string hashedPassword, string enteredPassword)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedEnteredPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                var hashedEnteredPasswordString = BitConverter.ToString(hashedEnteredPassword).Replace("-", "").ToLower();
                return hashedPassword == hashedEnteredPasswordString;
            }
        }

        private void ShowCaptcha()
        {
            GenerateCaptcha();
            CaptchaLabel.Visibility = Visibility.Visible;
            CaptchaTextBox.Visibility = Visibility.Visible;
            CaptchaTextBlock.Visibility = Visibility.Visible;
            RefreshCaptchaButton.Visibility = Visibility.Visible;
        }

        private void HideCaptcha()
        {
            CaptchaLabel.Visibility = Visibility.Collapsed;
            CaptchaTextBox.Visibility = Visibility.Collapsed;
            CaptchaTextBlock.Visibility = Visibility.Collapsed;
            RefreshCaptchaButton.Visibility = Visibility.Collapsed;
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            _secondsLeft--;

            if (_secondsLeft <= 0)
            {
                _blockTimer.Stop(); // Останавливаем таймер
                _isBlocked = false;
                _attemptsLeft = 3; // Сбрасываем количество попыток
                HideCaptcha(); // Скрываем CAPTCHA после разблокировки
                _captchaShown = false; // Сбрасываем флаг
            }
            else
            {
                MessageBox.Show($"Окно авторизации заблокировано. Осталось {_secondsLeft} секунд.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}