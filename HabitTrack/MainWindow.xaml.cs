using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace HabitTrack
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        // 在读取或写入文件之前，执行下面这行代码
        private string _habitTrackFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HabitTrack"
        );
        private string taskSavePath => System.IO.Path.Combine(_habitTrackFolder, "habittrack_tasks.txt");
        private string daySavePath => System.IO.Path.Combine(_habitTrackFolder, "habittrack_day.txt");

        private int _dayCount = 0;
        private Random _rand = new Random();
        private bool _finalStage = false;

        public MainWindow()
        {
            InitializeComponent();

            // 确保 HabitTrack 文件夹存在
            if (!Directory.Exists(_habitTrackFolder))
                Directory.CreateDirectory(_habitTrackFolder);

            // 读取天数
            if (File.Exists(daySavePath))
                int.TryParse(File.ReadAllText(daySavePath), out _dayCount);

            // 加载任务
            LoadTasks();

            // 添加任务按钮
            AddTaskButton.Click += (s, e) =>
            {
                string task = TaskInput.Text.Trim();
                if (!string.IsNullOrEmpty(task))
                {
                    var cb = CreateTaskCheckBox(task, isSystem:false, isChecked:false);
                    TaskList.Items.Add(cb);
                    TaskInput.Clear();
                    SaveTasks();
                }
            };

            // 根据天数显示剧情
            ShowDayContent();
        }

        private void ShowDayContent()
        {
            if (_dayCount == 3)
            {
                TaskList.Items.Add("凌晨 3:33 醒来");
            }
            else if (_dayCount == 5)
            {
                TaskList.Items.Add("盯着屏幕 " + _rand.Next(10, 20) + " 分钟");
            }
            else if (_dayCount == 7)
            {
                TaskList.Items.Add("2019.06.15 — 数学挂科后向她表白，被拒绝");
                TaskList.Items.Add("她说：你还是先把数学学好吧。");
            }
            else if (_dayCount == 9)
            {
                TaskList.Items.Add("2024.11.02 — 半夜看了 4 小时 VTuber 直播");
                TaskList.Items.Add("她们不会嫌你笨，不会嘲笑你单身。");
            }
            else if (_dayCount >= 10)
            {
                EnterFinalStage();
            }
        }

        private void EnterFinalStage()
        {
            _finalStage = true;
            MainGrid.Background = Brushes.Black;
            TitleText.Text = "We only track what you forget.";
            TitleText.Foreground = Brushes.Red;

            // 修改输入框颜色
            TaskInput.Background = Brushes.Black;
            TaskInput.Foreground = Brushes.Red;
            TaskInput.BorderBrush = Brushes.Red;
            TaskInput.Visibility = Visibility.Collapsed;
            AddTaskButton.Visibility = Visibility.Collapsed;

            TaskList.Items.Clear();
            // add final-stage items as disabled checked boxes with red foreground
            TaskList.Items.Add(CreateTaskCheckBox("失败", isSystem:true, isChecked:true, disabled:true));
            TaskList.Items.Add(CreateTaskCheckBox("嫉妒", isSystem:true, isChecked:true, disabled:true));
            TaskList.Items.Add(CreateTaskCheckBox("孤独", isSystem:true, isChecked:true, disabled:true));
            TaskList.Items.Add(CreateTaskCheckBox(Environment.UserName + "...", isSystem:true, isChecked:true, disabled:true));
            //TaskList.Items.Add("[Press any key to remember her face]");
        }

        private CheckBox CreateTaskCheckBox(string text, bool isSystem = false, bool isChecked = false, bool disabled = false)
        {
            var cb = new CheckBox();
            cb.Content = text;
            cb.IsChecked = isChecked;
            cb.Margin = new Thickness(2);
            if (isSystem)
            {
                cb.FontWeight = FontWeights.Bold;
            }
            if (disabled)
            {
                cb.IsEnabled = false;
                cb.Foreground = Brushes.Red;
            }
            cb.Checked += (s, e) => SaveTasks();
            cb.Unchecked += (s, e) => SaveTasks();
            return cb;
        }

        private void ShowMeaningfulEnding()
        {
            MainGrid.Background = Brushes.Black;
            TaskList.Items.Clear();
            TitleText.Foreground = Brushes.White;
            TitleText.Text = "If you forget the pain, you forget yourself.";

            // 3 秒后淡出并关闭
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Application.Current.Shutdown();
            };
            timer.Start();
        }

        private void SaveTasks()
        {
            var lines = new List<string>();
            foreach (var item in TaskList.Items)
            {
                var cb = item as CheckBox;
                if (cb == null) continue;
                // format: TYPE::ISCHECKED::TEXT
                // TYPE: U = user, S = system
                var type = (cb.FontWeight == FontWeights.Bold) ? "S" : "U";
                var isChecked = cb.IsChecked == true ? "1" : "0";
                var text = cb.Content.ToString().Replace("::", "--");
                lines.Add(string.Format("{0}::{1}::{2}", type, isChecked, text));
            }
            File.WriteAllLines(taskSavePath, lines);
        }

        private void LoadTasks()
        {
            if (File.Exists(taskSavePath))
            {
                var tasks = File.ReadAllLines(taskSavePath);
                foreach (var line in tasks)
                {
                    // expected format: TYPE::ISCHECKED::TEXT
                    var parts = line.Split(new string[] {"::"}, StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        var type = parts[0];
                        var isChecked = parts[1] == "1";
                        var text = string.Join("::", parts, 2, parts.Length - 2);
                        var isSystem = type == "S";
                        TaskList.Items.Add(CreateTaskCheckBox(text, isSystem, isChecked));
                    }
                    else
                    {
                        // fallback: add raw text
                        TaskList.Items.Add(CreateTaskCheckBox(line, false, false));
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveTasks();
            _dayCount++;
            File.WriteAllText(daySavePath, _dayCount.ToString());
        }

        private void DeleteTask_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TaskList.SelectedItem != null)
            {
                TaskList.Items.Remove(TaskList.SelectedItem);
                SaveTasks();
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_finalStage)
            {
                ShowMeaningfulEnding();
            }
            else if (e.Key == Key.Delete && TaskList.SelectedItem != null)
            {
                TaskList.Items.Remove(TaskList.SelectedItem);
                SaveTasks();
                e.Handled = true;
            }
        }
    }

}