using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AeroOS.Windows.Models;
using AeroOS.Windows.Services;
using Microsoft.Win32;

namespace AeroOS.Windows
{
    public partial class MainWindow : Window
    {
        private readonly PreferencesService _preferences = new PreferencesService();
        private readonly ApplicationCatalog _catalog = new ApplicationCatalog();
        private readonly SystemInfoService _systemInfo = new SystemInfoService();
        private AeroSettings _settings;
        private List<AppEntry> _applications = new List<AppEntry>();
        private SystemSnapshot _snapshot;
        private string _currentPage = "Home";
        private string _searchTerm = string.Empty;
        private readonly DispatcherTimer _clockTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            _settings = _preferences.Load();
            ApplyTheme();
            ApplyWallpaper();
            UpdateClock();
            _clockTimer.Interval = TimeSpan.FromMinutes(1);
            _clockTimer.Tick += delegate { UpdateClock(); };
            _clockTimer.Start();
            Closed += delegate { _clockTimer.Stop(); };
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RenderHome();
            try
            {
                _applications = await Task.Run(() => _catalog.Discover());
                if (_currentPage == "Home") RenderHome();
                else if (_currentPage == "Apps") RenderApplications();
            }
            catch (Exception ex)
            {
                ShowMessage("Application discovery", "AeroOS could not read one or more Start Menu folders. Windows itself is unaffected. " + ex.Message);
            }

            try { _snapshot = await Task.Run(() => _systemInfo.ReadSnapshot()); }
            catch { _snapshot = null; }
            if (_currentPage == "Home") RenderHome();
        }

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            Navigate(Convert.ToString(button.Tag));
        }

        private void Navigate(string page)
        {
            _currentPage = page;
            switch (page)
            {
                case "Home": RenderHome(); break;
                case "Apps": RenderApplications(); break;
                case "Personalization": RenderPersonalization(); break;
                case "ControlCenter": RenderControlCenter(); break;
                case "About": RenderAbout(); break;
                default: RenderHome(); break;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox == null) return;
            _searchTerm = SearchBox.Text ?? string.Empty;
            if (_currentPage != "Apps") _currentPage = "Apps";
            RenderApplications();
        }

        private void WindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenSetting("ms-settings:", null);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Navigate("About");
        }

        private void OpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            SafeStart("explorer.exe", null);
        }

        private void OpenTaskManager_Click(object sender, RoutedEventArgs e)
        {
            SafeStart("taskmgr.exe", null);
        }

        private void RefreshSystem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;
            Task.Run(() => _systemInfo.ReadSnapshot()).ContinueWith(task =>
            {
                Dispatcher.Invoke(() =>
                {
                    _snapshot = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
                    if (button != null) button.IsEnabled = true;
                    RenderHome();
                });
            });
        }

        private void RenderHome()
        {
            ContentPanel.Children.Clear();
            AddHeading("Welcome to AeroOS", "A lightweight Windows desktop companion. Your normal Windows desktop, applications, drivers and security remain underneath.");

            var hero = new Border
            {
                Background = GetBrush("CardBrush"),
                BorderBrush = GetBrush("LineBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(24),
                Margin = new Thickness(0, 0, 0, 22)
            };
            var heroStack = new StackPanel();
            heroStack.Children.Add(Text("A calmer workspace for your Windows PC", 22, true, "TextBrush"));
            heroStack.Children.Add(Text("Open your real Windows apps, reach native settings and check basic hardware information — without replacing the Windows shell.", 13, false, "MutedBrush", 0, 7));
            var heroButtons = new WrapPanel { Margin = new Thickness(0, 17, 0, 0) };
            heroButtons.Children.Add(ActionButton("Open File Explorer", OpenExplorer_Click, true));
            heroButtons.Children.Add(ActionButton("Browse applications", delegate { SearchBox.Text = string.Empty; Navigate("Apps"); }, false));
            heroButtons.Children.Add(ActionButton("Windows Settings", delegate { OpenSetting("ms-settings:", null); }, false));
            heroStack.Children.Add(heroButtons);
            hero.Child = heroStack;
            ContentPanel.Children.Add(hero);

            AddSectionTitle("System overview", "Read-only information reported by Windows");
            var metrics = new WrapPanel { Margin = new Thickness(0, 0, 0, 20) };
            metrics.Children.Add(MetricCard("Installed apps", _applications.Count.ToString(), "Start Menu shortcuts discovered"));
            metrics.Children.Add(MetricCard("Memory", _snapshot == null || _snapshot.TotalMemoryGb <= 0 ? "Checking…" : _snapshot.TotalMemoryGb.ToString("0.0") + " GB", _snapshot == null || _snapshot.AvailableMemoryGb <= 0 ? "Available memory not read" : _snapshot.AvailableMemoryGb.ToString("0.0") + " GB currently available"));
            metrics.Children.Add(MetricCard("System drive", _snapshot == null || _snapshot.TotalSystemDriveGb <= 0 ? "Checking…" : _snapshot.FreeSystemDriveGb.ToString("0.0") + " GB free", _snapshot == null || _snapshot.TotalSystemDriveGb <= 0 ? "Drive information not read" : "of " + _snapshot.TotalSystemDriveGb.ToString("0.0") + " GB"));
            ContentPanel.Children.Add(metrics);

            var detail = new Border { Background = GetBrush("PanelBrush"), BorderBrush = GetBrush("LineBrush"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(14), Padding = new Thickness(18), Margin = new Thickness(0, 0, 0, 22) };
            var detailStack = new StackPanel();
            detailStack.Children.Add(Text("Hardware snapshot", 15, true, "TextBrush"));
            detailStack.Children.Add(Text(_snapshot == null ? "System information is still loading or could not be queried." :
                "CPU: " + _snapshot.CpuName + "\nGPU: " + _snapshot.GpuName + "\nWindows: " + _snapshot.OperatingSystem + "\nArchitecture: " + _snapshot.Architecture + "\nLogical processors: " + _snapshot.LogicalProcessors,
                12, false, "MutedBrush", 0, 9));
            var detailButtons = new WrapPanel();
            detailButtons.Children.Add(ActionButton("Refresh information", RefreshSystem_Click, false));
            detailButtons.Children.Add(ActionButton("Open Task Manager", OpenTaskManager_Click, false));
            detailButtons.Children.Add(ActionButton("Control Center", delegate { Navigate("ControlCenter"); }, false));
            detailStack.Children.Add(detailButtons);
            detail.Child = detailStack;
            ContentPanel.Children.Add(detail);

            AddSectionTitle("Pinned applications", "Your shortcuts are discovered from the Windows Start Menu");
            var pinned = _applications.Where(x => _settings.PinnedPaths.Contains(x.Path, StringComparer.OrdinalIgnoreCase)).ToList();
            if (pinned.Count == 0)
                ContentPanel.Children.Add(Text("No pinned apps yet. Open Applications and select Pin on a shortcut.", 12, false, "MutedBrush", 0, 8));
            else
            {
                var row = new WrapPanel();
                foreach (var app in pinned.Take(8)) row.Children.Add(AppTile(app, true));
                ContentPanel.Children.Add(row);
            }

            AddSectionTitle("Recently opened", "Shortcuts you launched from AeroOS");
            var recent = _settings.RecentPaths.Select(path => _applications.FirstOrDefault(app => string.Equals(app.Path, path, StringComparison.OrdinalIgnoreCase)))
                .Where(app => app != null).Take(6).ToList();
            if (recent.Count == 0)
                ContentPanel.Children.Add(Text("Your recently launched shortcuts will appear here.", 11, false, "MutedBrush", 0, 12));
            else
            {
                var recentRow = new WrapPanel();
                foreach (var app in recent) recentRow.Children.Add(AppTile(app, true));
                ContentPanel.Children.Add(recentRow);
            }

            AddSectionTitle("Quick access", "These open real Windows tools");
            var quick = new WrapPanel();
            quick.Children.Add(QuickCard("File Explorer", "Browse your actual drives and files", delegate { SafeStart("explorer.exe", null); }));
            quick.Children.Add(QuickCard("Task Manager", "Inspect processes and performance", delegate { SafeStart("taskmgr.exe", null); }));
            quick.Children.Add(QuickCard("Windows Update", "Open native update settings", delegate { OpenSetting("ms-settings:windowsupdate", "" ); }));
            ContentPanel.Children.Add(quick);
        }

        private void RenderApplications()
        {
            ContentPanel.Children.Clear();
            AddHeading("Applications", "Applications are read from your user and all-users Windows Start Menu shortcut folders. AeroOS does not install or remove programs.");
            if (!string.IsNullOrWhiteSpace(_searchTerm))
                ContentPanel.Children.Add(Text("Search results for: " + _searchTerm, 12, true, "MutedBrush", 0, 12));

            var visible = _applications.Where(x => string.IsNullOrWhiteSpace(_searchTerm) ||
                x.Name.IndexOf(_searchTerm, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                x.Category.IndexOf(_searchTerm, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();

            if (visible.Count == 0)
            {
                ContentPanel.Children.Add(InfoCard("No matching Start Menu shortcuts", _applications.Count == 0 ? "No .lnk shortcuts were discovered. Store apps that do not expose Start Menu shortcuts may not appear here." : "Try a different search term or clear the search field."));
                return;
            }

            ContentPanel.Children.Add(Text(visible.Count + " shortcut(s)", 11, false, "MutedBrush", 0, 9));
            foreach (var group in visible.GroupBy(x => x.Category).OrderBy(x => x.Key, StringComparer.CurrentCultureIgnoreCase))
            {
                AddSectionTitle(group.Key, group.Count() + " app shortcut(s)");
                var grid = new WrapPanel();
                foreach (var app in group) grid.Children.Add(AppTile(app, false));
                ContentPanel.Children.Add(grid);
            }
        }

        private void RenderPersonalization()
        {
            ContentPanel.Children.Clear();
            AddHeading("Personalization", "Changes on this page affect only the AeroOS application window. They do not change the Windows system theme or wallpaper.");

            var themeCard = CreateSectionCard("Appearance", "Choose how the AeroOS window is displayed.");
            var themeRow = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            themeRow.Children.Add(ChoiceButton("Dark theme", delegate { _settings.Theme = "Dark"; SaveSettings(); ApplyTheme(); RenderPersonalization(); }));
            themeRow.Children.Add(ChoiceButton("Light theme", delegate { _settings.Theme = "Light"; SaveSettings(); ApplyTheme(); RenderPersonalization(); }));
            ((StackPanel)themeCard.Child).Children.Add(themeRow);
            ((StackPanel)themeCard.Child).Children.Add(Text("Accent color", 12, true, "TextBrush", 0, 15));
            var accent = new ComboBox { Width = 190, Height = 34, Margin = new Thickness(0, 7, 0, 5), Background = GetBrush("CardBrush"), Foreground = GetBrush("TextBrush"), BorderBrush = GetBrush("LineBrush"), Padding = new Thickness(7) };
            accent.Items.Add(new ComboBoxItem { Content = "Violet", Tag = "#8B7CF6" });
            accent.Items.Add(new ComboBoxItem { Content = "Blue", Tag = "#3984E8" });
            accent.Items.Add(new ComboBoxItem { Content = "Teal", Tag = "#20A99A" });
            accent.Items.Add(new ComboBoxItem { Content = "Rose", Tag = "#D76C9A" });
            foreach (ComboBoxItem item in accent.Items) if (Convert.ToString(item.Tag) == _settings.AccentColor) accent.SelectedItem = item;
            accent.SelectionChanged += delegate
            {
                var item = accent.SelectedItem as ComboBoxItem;
                if (item != null) { _settings.AccentColor = Convert.ToString(item.Tag); SaveSettings(); ApplyTheme(); }
            };
            ((StackPanel)themeCard.Child).Children.Add(accent);
            ContentPanel.Children.Add(themeCard);

            var performanceCard = CreateSectionCard("Performance mode", "Reduces decorative UI work inside AeroOS only. It does not disable Windows services or change system-wide performance settings.");
            var performance = new CheckBox { Content = "Use minimal visual effects", IsChecked = _settings.PerformanceMode, Foreground = GetBrush("TextBrush"), Margin = new Thickness(0, 13, 0, 0), FontSize = 13 };
            performance.Checked += delegate { _settings.PerformanceMode = true; SaveSettings(); ApplyWallpaper(); };
            performance.Unchecked += delegate { _settings.PerformanceMode = false; SaveSettings(); ApplyWallpaper(); };
            ((StackPanel)performanceCard.Child).Children.Add(performance);
            ContentPanel.Children.Add(performanceCard);

            var wallpaperCard = CreateSectionCard("AeroOS workspace background", "Select an image used behind this app's content area. Your Windows desktop wallpaper is not touched.");
            var wallpaperRow = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            wallpaperRow.Children.Add(ActionButton("Choose image…", ChooseWallpaper_Click, true));
            wallpaperRow.Children.Add(ActionButton("Clear image", delegate { _settings.WallpaperPath = string.Empty; SaveSettings(); ApplyWallpaper(); RenderPersonalization(); }, false));
            ((StackPanel)wallpaperCard.Child).Children.Add(wallpaperRow);
            ((StackPanel)wallpaperCard.Child).Children.Add(Text(string.IsNullOrWhiteSpace(_settings.WallpaperPath) ? "No workspace image selected." : Path.GetFileName(_settings.WallpaperPath), 11, false, "MutedBrush", 0, 10));
            ContentPanel.Children.Add(wallpaperCard);

            var resetCard = CreateSectionCard("Reset AeroOS preferences", "Restores AeroOS's own theme, accent, pins and recent-app list. It does not reset Windows.");
            ((StackPanel)resetCard.Child).Children.Add(ActionButton("Reset AeroOS preferences", delegate
            {
                var answer = MessageBox.Show("Reset only AeroOS preferences? Windows settings, apps and files will not be changed.", "Reset AeroOS preferences", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    _settings = AeroSettings.CreateDefault();
                    SaveSettings(); ApplyTheme(); ApplyWallpaper(); RenderPersonalization();
                }
            }, false));
            ContentPanel.Children.Add(resetCard);
        }

        private void RenderControlCenter()
        {
            ContentPanel.Children.Clear();
            AddHeading("Control Center", "Shortcuts open native Windows settings pages or built-in Windows tools. AeroOS does not imitate system controls it cannot manage.");
            var grid = new WrapPanel();
            grid.Children.Add(QuickCard("Display", "Resolution, scaling and multiple displays", delegate { OpenSetting("ms-settings:display", "desk.cpl"); }));
            grid.Children.Add(QuickCard("Sound", "Output and input devices", delegate { OpenSetting("ms-settings:sound", "mmsys.cpl"); }));
            grid.Children.Add(QuickCard("Wi-Fi & network", "Network and internet settings", delegate { OpenSetting("ms-settings:network-status", "ncpa.cpl"); }));
            grid.Children.Add(QuickCard("Bluetooth", "Bluetooth device settings", delegate { OpenSetting("ms-settings:bluetooth", "bthprops.cpl"); }));
            grid.Children.Add(QuickCard("Power & battery", "Power mode and battery settings", delegate { OpenSetting("ms-settings:powersleep", "powercfg.cpl"); }));
            grid.Children.Add(QuickCard("Storage", "Storage and free-space settings", delegate { OpenSetting("ms-settings:storagesense", "" ); }));
            grid.Children.Add(QuickCard("Personalization", "Windows themes and background settings", delegate { OpenSetting("ms-settings:personalization", "" ); }));
            grid.Children.Add(QuickCard("Windows Update", "Update status and update options", delegate { OpenSetting("ms-settings:windowsupdate", "" ); }));
            grid.Children.Add(QuickCard("Windows Security", "Security and protection dashboard", delegate { OpenSetting("ms-settings:windowsdefender", "" ); }));
            grid.Children.Add(QuickCard("Installed apps", "Apps and optional features", delegate { OpenSetting("ms-settings:appsfeatures", "appwiz.cpl"); }));
            grid.Children.Add(QuickCard("Accessibility", "Ease of Access settings", delegate { OpenSetting("ms-settings:easeofaccess", "" ); }));
            grid.Children.Add(QuickCard("About this PC", "Device and Windows information", delegate { OpenSetting("ms-settings:about", "sysdm.cpl"); }));
            ContentPanel.Children.Add(grid);
            ContentPanel.Children.Add(InfoCard("Compatibility note", "Some Windows 10 builds may not expose every ms-settings page URI. Where a known Control Panel fallback exists, AeroOS opens it. A failed shortcut does not change system configuration."));
        }

        private void RenderAbout()
        {
            ContentPanel.Children.Clear();
            AddHeading("About AeroOS Windows Edition", "A desktop companion for Windows — not a replacement operating system.");
            ContentPanel.Children.Add(InfoCard("Windows stays in charge", "AeroOS runs as a normal, resizable Windows application. It does not replace Explorer.exe, Winlogon, the Windows shell, boot files or the bootloader."));
            ContentPanel.Children.Add(InfoCard("Your files and applications", "Application discovery reads Start Menu shortcuts. Launching an item asks the normal Windows shell to open it. File Explorer remains the real Windows File Explorer."));
            ContentPanel.Children.Add(InfoCard("Performance transparency", "AeroOS does not claim to make Windows faster by changing its appearance. The optional Performance mode only reduces decorative work inside this app. Use Windows Task Manager and the included read-only diagnostic report to measure the PC."));
            ContentPanel.Children.Add(InfoCard("Privacy", "The app stores its preferences in your Local AppData folder. It does not upload hardware information, collect credentials or send telemetry."));
            ContentPanel.Children.Add(InfoCard("Recovery", "Close AeroOS like any other app. Windows Explorer and the native taskbar remain available. Uninstalling AeroOS removes only the AeroOS program folder and shortcuts created by its installer; preferences are retained unless you remove them yourself."));
        }

        private Border AppTile(AppEntry app, bool compact)
        {
            var card = new Border
            {
                Width = compact ? 190 : 218,
                MinHeight = compact ? 98 : 132,
                Margin = new Thickness(0, 0, 12, 12),
                Padding = new Thickness(14),
                Background = GetBrush("CardBrush"),
                BorderBrush = GetBrush("LineBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(13)
            };
            var stack = new StackPanel();
            stack.Children.Add(Text("▣  " + app.Name, 13, true, "TextBrush"));
            stack.Children.Add(Text(app.Category, 10, false, "MutedBrush", 0, 4));
            var buttons = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            buttons.Children.Add(ActionButton("Launch", delegate { LaunchApplication(app); }, true, 8));
            if (!compact) buttons.Children.Add(ActionButton(_settings.PinnedPaths.Contains(app.Path, StringComparer.OrdinalIgnoreCase) ? "Unpin" : "Pin", delegate { TogglePin(app); }, false, 8));
            stack.Children.Add(buttons);
            card.Child = stack;
            return card;
        }

        private Border MetricCard(string title, string value, string description)
        {
            var card = new Border { Width = 205, MinHeight = 112, Margin = new Thickness(0, 0, 12, 12), Padding = new Thickness(16), Background = GetBrush("CardBrush"), BorderBrush = GetBrush("LineBrush"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(13) };
            var stack = new StackPanel();
            stack.Children.Add(Text(title.ToUpperInvariant(), 10, true, "MutedBrush"));
            stack.Children.Add(Text(value, 23, true, "TextBrush", 0, 6));
            stack.Children.Add(Text(description, 10, false, "MutedBrush"));
            card.Child = stack;
            return card;
        }

        private Border QuickCard(string title, string description, Action action)
        {
            var card = new Border { Width = 258, MinHeight = 105, Margin = new Thickness(0, 0, 12, 12), Padding = new Thickness(16), Background = GetBrush("CardBrush"), BorderBrush = GetBrush("LineBrush"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(13) };
            var stack = new StackPanel();
            stack.Children.Add(Text(title, 14, true, "TextBrush"));
            stack.Children.Add(Text(description, 11, false, "MutedBrush", 0, 6));
            stack.Children.Add(ActionButton("Open", delegate { action(); }, false, 8));
            card.Child = stack;
            return card;
        }

        private Border CreateSectionCard(string title, string description)
        {
            var card = new Border { Background = GetBrush("PanelBrush"), BorderBrush = GetBrush("LineBrush"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(14), Padding = new Thickness(19), Margin = new Thickness(0, 0, 0, 15) };
            var stack = new StackPanel();
            stack.Children.Add(Text(title, 16, true, "TextBrush"));
            stack.Children.Add(Text(description, 11, false, "MutedBrush", 0, 5));
            card.Child = stack;
            return card;
        }

        private Border InfoCard(string title, string description)
        {
            var card = CreateSectionCard(title, description);
            return card;
        }

        private void AddHeading(string title, string subtitle)
        {
            ContentPanel.Children.Add(Text(title, 27, true, "TextBrush"));
            ContentPanel.Children.Add(Text(subtitle, 12, false, "MutedBrush", 0, 22));
        }

        private void AddSectionTitle(string title, string subtitle)
        {
            ContentPanel.Children.Add(Text(title, 17, true, "TextBrush", 0, 3));
            ContentPanel.Children.Add(Text(subtitle, 10, false, "MutedBrush", 0, 11));
        }

        private TextBlock Text(string value, double size, bool bold, string brushKey, double marginTop = 0, double marginBottom = 0)
        {
            return new TextBlock
            {
                Text = value,
                FontSize = size,
                FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = GetBrush(brushKey),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, marginTop, 0, marginBottom)
            };
        }

        private Button ActionButton(string label, RoutedEventHandler handler, bool primary, double rightMargin = 9)
        {
            var button = new Button
            {
                Content = label,
                Margin = new Thickness(0, 0, rightMargin, 7),
                Padding = new Thickness(13, 8, 13, 8),
                Background = primary ? GetBrush("AccentBrush") : GetBrush("CardBrush"),
                Foreground = primary ? Brushes.White : GetBrush("TextBrush"),
                BorderBrush = primary ? GetBrush("AccentBrush") : GetBrush("LineBrush"),
                BorderThickness = new Thickness(1),
                FontSize = 11
            };
            button.Click += handler;
            return button;
        }

        private Button ChoiceButton(string label, Action action)
        {
            var button = ActionButton(label, delegate { action(); }, false, 9);
            return button;
        }

        private Brush GetBrush(string key)
        {
            return (Brush)FindResource(key);
        }

        private void ApplyTheme()
        {
            bool dark = !string.Equals(_settings.Theme, "Light", StringComparison.OrdinalIgnoreCase);
            Resources["WindowBgBrush"] = BrushFrom(dark ? "#0B1220" : "#F3F6FB");
            Resources["PanelBrush"] = BrushFrom(dark ? "#111D30" : "#FFFFFF");
            Resources["CardBrush"] = BrushFrom(dark ? "#17263D" : "#FFFFFF");
            Resources["TextBrush"] = BrushFrom(dark ? "#F2F6FC" : "#172033");
            Resources["MutedBrush"] = BrushFrom(dark ? "#A2B2C7" : "#64748B");
            Resources["LineBrush"] = BrushFrom(dark ? "#2A3A52" : "#DCE3ED");
            Resources["AccentBrush"] = BrushFrom(_settings.AccentColor);
            Background = GetBrush("WindowBgBrush");
            ApplyWallpaper();
        }

        private static SolidColorBrush BrushFrom(string color)
        {
            try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)); }
            catch { return new SolidColorBrush(Color.FromRgb(139, 124, 246)); }
        }

        private void ApplyWallpaper()
        {
            if (ContentSurface == null) return;
            if (_settings.PerformanceMode || string.IsNullOrWhiteSpace(_settings.WallpaperPath) || !File.Exists(_settings.WallpaperPath))
            {
                ContentSurface.Background = GetBrush("WindowBgBrush");
                return;
            }
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(_settings.WallpaperPath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 1600;
                bitmap.EndInit();
                bitmap.Freeze();
                ContentSurface.Background = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill, Opacity = 0.22 };
            }
            catch
            {
                ContentSurface.Background = GetBrush("WindowBgBrush");
            }
        }

        private void ChooseWallpaper_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Choose an AeroOS workspace image", Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*", CheckFileExists = true };
            if (dialog.ShowDialog(this) == true)
            {
                _settings.WallpaperPath = dialog.FileName;
                SaveSettings();
                ApplyWallpaper();
                RenderPersonalization();
            }
        }

        private void TogglePin(AppEntry app)
        {
            var match = _settings.PinnedPaths.FirstOrDefault(x => string.Equals(x, app.Path, StringComparison.OrdinalIgnoreCase));
            if (match == null) _settings.PinnedPaths.Add(app.Path);
            else _settings.PinnedPaths.Remove(match);
            SaveSettings();
            RenderApplications();
        }

        private void LaunchApplication(AppEntry app)
        {
            try
            {
                Process.Start(new ProcessStartInfo(app.Path) { UseShellExecute = true });
                _settings.RecentPaths.RemoveAll(x => string.Equals(x, app.Path, StringComparison.OrdinalIgnoreCase));
                _settings.RecentPaths.Insert(0, app.Path);
                if (_settings.RecentPaths.Count > 8) _settings.RecentPaths.RemoveRange(8, _settings.RecentPaths.Count - 8);
                SaveSettings();
            }
            catch (Exception ex)
            {
                ShowMessage("Could not launch " + app.Name, "Windows could not open this shortcut. The shortcut may be broken or its target may have been moved.\n\n" + ex.Message);
            }
        }

        private void OpenSetting(string uri, string fallback)
        {
            try { SettingsLauncher.Open(uri, fallback); }
            catch (Exception ex) { ShowMessage("Windows settings could not be opened", ex.Message); }
        }

        private void SafeStart(string fileName, string arguments)
        {
            try { Process.Start(new ProcessStartInfo(fileName, arguments ?? string.Empty) { UseShellExecute = true }); }
            catch (Exception ex) { ShowMessage("Could not open " + fileName, ex.Message); }
        }

        private void UpdateClock()
        {
            if (FooterClock != null) FooterClock.Text = DateTime.Now.ToString("ddd, dd MMM  •  h:mm tt");
        }

        private void SaveSettings()
        {
            try { _preferences.Save(_settings); }
            catch (Exception ex) { ShowMessage("Could not save AeroOS preferences", "AeroOS could not save its own settings file. Windows settings were not changed.\n\n" + ex.Message); }
        }

        private void ShowMessage(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

