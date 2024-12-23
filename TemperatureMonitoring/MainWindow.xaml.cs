using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace TemperatureMonitoring
{
    public partial class MainWindow : Window
    {
        private DateTime startTime;
        private List<int> temperatures;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Загрузка данных из файла
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var (start, temps) = LoadDataFromFile(openFileDialog.FileName);
                    startTime = start;
                    temperatures = temps;

                    MessageBox.Show("Данные загружены успешно!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}");
                }
            }
        }

        // Проверка температурных условий
        private void Check_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Чтение параметров из полей ввода
                int Tmax = int.Parse(TmaxInput.Text);
                int T1 = int.Parse(T1Input.Text);
                int Tmin = int.Parse(TminInput.Text);
                int T2 = int.Parse(T2Input.Text);

                var condition = new ProductCondition
                {
                    Tmax = Tmax,
                    T1 = T1,
                    Tmin = Tmin,
                    T2 = T2
                };

                var violations = CheckTemperatureCompliance(startTime, temperatures, condition);
                if (violations.Count == 0)
                {
                    ReportOutput.Text = "Нарушений не обнаружено. Все условия соблюдены.";
                }
                else
                {
                    ReportOutput.Text = "Обнаружены нарушения:\n";
                    foreach (var violation in violations)
                    {
                        ReportOutput.Text += $"{violation.ViolationTime:dd.MM.yyyy HH:mm} - " +
                                             $"Требуется: {violation.RequiredTemperature}, " +
                                             $"Фактически: {violation.ActualTemperature}, " +
                                             $"Отклонение: {violation.Deviation}\n";
                    }

                    // Сохранение отчета в файл
                    SaveReport("TemperatureViolationsReport.txt", violations);
                    MessageBox.Show("Отчет сохранен как 'TemperatureViolationsReport.txt'");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Загрузка данных из файла
        private (DateTime, List<int>) LoadDataFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            DateTime startTime = DateTime.ParseExact(lines[0], "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            List<int> temperatures = lines[1].Split(' ').Select(int.Parse).ToList();
            return (startTime, temperatures);
        }

        // Проверка соблюдения условий
        private List<TemperatureViolation> CheckTemperatureCompliance(DateTime startTime, List<int> temperatures, ProductCondition condition)
        {
            List<TemperatureViolation> violations = new List<TemperatureViolation>();
            int timeAboveMax = 0, timeBelowMin = 0;

            for (int i = 0; i < temperatures.Count; i++)
            {
                DateTime currentTime = startTime.AddMinutes(i * 10);
                int temperature = temperatures[i];

                if (temperature > condition.Tmax)
                {
                    timeAboveMax += 10;
                    if (timeAboveMax > condition.T1)
                    {
                        violations.Add(new TemperatureViolation
                        {
                            ViolationTime = currentTime,
                            RequiredTemperature = condition.Tmax,
                            ActualTemperature = temperature,
                            Deviation = temperature - condition.Tmax
                        });
                    }
                }
                else
                {
                    timeAboveMax = 0; // Сброс времени превышения
                }

                if (temperature < condition.Tmin)
                {
                    timeBelowMin += 10;
                    if (timeBelowMin > condition.T2)
                    {
                        violations.Add(new TemperatureViolation
                        {
                            ViolationTime = currentTime,
                            RequiredTemperature = condition.Tmin,
                            ActualTemperature = temperature,
                            Deviation = condition.Tmin - temperature
                        });
                    }
                }
                else
                {
                    timeBelowMin = 0; // Сброс времени ниже минимальной температуры
                }
            }

            return violations;
        }

        // Сохранение отчета в файл
        private void SaveReport(string filePath, List<TemperatureViolation> violations)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var violation in violations)
                {
                    writer.WriteLine($"{violation.ViolationTime:dd.MM.yyyy HH:mm} - " +
                                     $"Требуется: {violation.RequiredTemperature}, " +
                                     $"Фактически: {violation.ActualTemperature}, " +
                                     $"Отклонение: {violation.Deviation}");
                }
            }
        }
    }
}