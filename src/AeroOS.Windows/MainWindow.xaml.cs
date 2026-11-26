using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        private readonly DispatcherTimer _clockTimer = new DispatcherTimer();
        private AeroSettings _settings;
        private List<AppEntry> _applications = new List<AppEntry>();
        private List<AppEntry> _builtInApps = new List<AppEntry>();
        private TextBlock _clockLabel;
        private TextBlock _dateLabel;
        private TextBlock _deviceSummary;
        private TextBox _startSearch;
        private StackPanel _startResults;
        private Grid _startOverlay;
        private Grid _settingsOverlay;
        private TextBox _desktopSearch;
        private bool _isLoadingApps;

        public MainWindow()
        {
            InitializeComponent();
            _settings = _preferences.Load();
            _settings.Normalize();
            _builtInApps = CreateBuiltInApps();
            BuildShell();

            _clockTimer.Interval = TimeSpan.FromMinutes(1);
            _clockTimer.Tick += delegate { UpdateClock(); };
            _clockTimer.Start();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoadingApps = true;
            RenderStartResults(string.Empty);
            try
            {
                _applications = await Task.Run(() => _catalog.Discover());
                RenderStartResults(_startSearch == null ? string.Empty : _startSearch.Text);
            }
            catch
            {
                if (_startResults != null)
                {
                    _startResults.Children.Clear();
                    _startResults.Children.Add(MakeText("Installed app discovery is unavailable. Windows apps are unaffected.", 12, "Muted"));
                }
            }
            finally
            {
                _isLoadingApps = false;
            }

            try
            {
                var snapshot = await Task.Run(() => _systemInfo.ReadSnapshot());
                if (_deviceSummary != null)
                {
                    _deviceSummary.Text = snapshot.CpuName + Environment.NewLine
                        + snapshot.Architecture + "  •  " + snapshot.LogicalProcessors + " logical processors"
                        + (snapshot.TotalMemoryGb > 0 ? Environment.NewLine + snapshot.TotalMemoryGb.ToString("0.0") + " GB memory" : string.Empty);
                }
            }
            catch
            {
                if (_deviceSummary != null) _deviceSummary.Text = "Windows device information is unavailable.";
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _clockTimer.Stop();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_startOverlay != null && _startOverlay.Visibility == Visibility.Visible)
                {
                    _startOverlay.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
                else if (_settingsOverlay != null && _settingsOverlay.Visibility == Visibility.Visible)
                {
                    _settingsOverlay.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
            }
        }

        private void BuildShell()
        {
            ShellRoot.Children.Clear();
            var backdropStops = new GradientStopCollection
            {
                new GradientStop(ColorFrom("#FF080E1C"), 0.00),
                new GradientStop(ColorFrom("#FF101A32"), 0.38),
                new GradientStop(ColorFrom("#FF171A38"), 0.72),
                new GradientStop(ColorFrom("#FF0A1729"), 1.00)
            };
            ShellRoot.Background = new LinearGradientBrush(backdropStops, new Point(0, 0), new Point(1, 1));

            if (!_settings.PerformanceMode && !string.IsNullOrWhiteSpace(_settings.WallpaperPath) && File.Exists(_settings.WallpaperPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 1600;
                    bitmap.UriSource = new Uri(_settings.WallpaperPath, UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ShellRoot.Children.Add(new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.UniformToFill,
                        Opacity = 0.28,
                        IsHitTestVisible = false
                    });
                }
                catch { }
            }

            // Static ambient light gives the glass surfaces depth without continuous animation or blur effects.
            AddAmbientGlow(720, 650, "#735CFF", 0.34, HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(-240, -250, 0, 0));
            AddAmbientGlow(700, 640, "#22B8D6", 0.25, HorizontalAlignment.Right, VerticalAlignment.Bottom, new Thickness(0, 0, -245, -250));
            AddAmbientGlow(460, 460, "#CB6DFF", 0.16, HorizontalAlignment.Right, VerticalAlignment.Top, new Thickness(0, -220, -175, 0));

            var layout = new Grid { Margin = new Thickness(26, 18, 26, 18) };
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(66) });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(76) });
            BuildHeader(layout);
            BuildDesktop(layout);
            BuildTaskbar(layout);
            ShellRoot.Children.Add(layout);

            _startOverlay = BuildStartOverlay();
            _settingsOverlay = BuildSettingsOverlay();
            ShellRoot.Children.Add(_startOverlay);
            ShellRoot.Children.Add(_settingsOverlay);
            Panel.SetZIndex(_startOverlay, 50);
            Panel.SetZIndex(_settingsOverlay, 60);
            UpdateClock();
        }

        private void BuildHeader(Grid layout)
        {
            var header = GlassCard(new Thickness(18, 10, 18, 10), 18);
            Grid.SetRow(header, 0);
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var brand = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var mark = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(13),
                Background = AccentBrush(),
                Child = new TextBlock
                {
                    Text = "A",
                    Foreground = Brushes.White,
                    FontSize = 25,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            brand.Children.Add(mark);
            var brandText = new StackPanel { Margin = new Thickness(11, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            brandText.Children.Add(MakeText("AEROOS", 15, "Text", FontWeights.Bold));
            brandText.Children.Add(MakeText("WINDOWS EDITION", 8, "Muted", FontWeights.SemiBold));
            brand.Children.Add(brandText);
            grid.Children.Add(brand);

            var center = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            center.Children.Add(MakeText("PERSONAL DESKTOP", 10, "Muted", FontWeights.SemiBold));
            Grid.SetColumn(center, 1);
            grid.Children.Add(center);

            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            var clockBlock = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 18, 0) };
            _clockLabel = MakeText("", 15, "Text", FontWeights.SemiBold);
            _clockLabel.HorizontalAlignment = HorizontalAlignment.Right;
            _dateLabel = MakeText("", 9, "Muted");
            _dateLabel.HorizontalAlignment = HorizontalAlignment.Right;
            clockBlock.Children.Add(_clockLabel);
            clockBlock.Children.Add(_dateLabel);
            actions.Children.Add(clockBlock);
            actions.Children.Add(MakeButton("⚙  Settings", delegate { ShowSettings(); }, false, 12, 34));
            actions.Children.Add(MakeButton("✕", delegate { Close(); }, false, 12, 34, 42));
            Grid.SetColumn(actions, 2);
            grid.Children.Add(actions);

            header.Child = grid;
            layout.Children.Add(header);
        }

        private void BuildDesktop(Grid layout)
        {
            var body = new Grid { Margin = new Thickness(0, 17, 0, 17) };
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(205) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(248) });
            Grid.SetRow(body, 1);
            layout.Children.Add(body);

            var sidebar = GlassCard(new Thickness(15, 18, 15, 16), 20);
            var sideStack = new StackPanel();
            sideStack.Children.Add(MakeText("YOUR PLACES", 9, "Muted", FontWeights.Bold, new Thickness(9, 2, 0, 11)));
            sideStack.Children.Add(PlaceButton("⌂", "Home", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
            sideStack.Children.Add(PlaceButton("▤", "Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            sideStack.Children.Add(PlaceButton("↓", "Downloads", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")));
            sideStack.Children.Add(PlaceButton("▧", "Pictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));
            sideStack.Children.Add(PlaceButton("♫", "Music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)));
            sideStack.Children.Add(PlaceButton("▣", "This PC", "shell:MyComputerFolder"));
            sideStack.Children.Add(PlaceButton("♲", "Recycle Bin", "shell:RecycleBinFolder"));
            var sideDivider = new Border { Height = 1, Background = LineBrush(), Margin = new Thickness(6, 15, 6, 13) };
            sideStack.Children.Add(sideDivider);
            sideStack.Children.Add(MakeText("WINDOWS TOOLS", 9, "Muted", FontWeights.Bold, new Thickness(9, 0, 0, 9)));
            sideStack.Children.Add(PlaceActionButton("▦", "All applications", ShowStart));
            sideStack.Children.Add(PlaceActionButton("⚙", "Windows Settings", OpenWindowsSettings));
            sideStack.Children.Add(PlaceActionButton("▥", "Task Manager", delegate { LaunchBuiltin("@taskmanager"); }));
            sidebar.Child = sideStack;
            body.Children.Add(sidebar);

            var center = new StackPanel { Margin = new Thickness(20, 4, 20, 4) };
            Grid.SetColumn(center, 1);
            body.Children.Add(center);

            var greeting = GetGreeting();
            center.Children.Add(MakeText(greeting + ", welcome back.", 29, "Text", FontWeights.SemiBold, new Thickness(2, 4, 0, 1)));
            center.Children.Add(MakeText("A beautiful, focused space for the things you do on Windows.", 12, "Muted", FontWeights.Normal, new Thickness(3, 0, 0, 19)));

            var searchRow = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            searchRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            searchRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var searchBorder = GlassCard(new Thickness(15, 0, 12, 0), 14);
            searchBorder.Height = 48;
            var searchGrid = new Grid();
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            searchGrid.Children.Add(MakeText("⌕", 25, "Muted", FontWeights.Normal));
            _desktopSearch = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = TextBrush(),
                CaretBrush = AccentBrush(),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                ToolTip = "Search installed Start Menu applications and Windows tools"
            };
            _desktopSearch.TextChanged += DesktopSearch_TextChanged;
            Grid.SetColumn(_desktopSearch, 1);
            searchGrid.Children.Add(_desktopSearch);
            searchBorder.Child = searchGrid;
            searchRow.Children.Add(searchBorder);
            var searchButton = MakeButton("Search apps", delegate
            {
                ShowStart();
                if (_startSearch != null) _startSearch.Text = _desktopSearch == null ? string.Empty : _desktopSearch.Text;
            }, true, 12, 48);
            searchButton.Margin = new Thickness(10, 0, 0, 0);
            Grid.SetColumn(searchButton, 1);
            searchRow.Children.Add(searchButton);
            center.Children.Add(searchRow);

            var quickHeader = new Grid { Margin = new Thickness(2, 0, 0, 9) };
            quickHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            quickHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            quickHeader.Children.Add(MakeText("QUICK LAUNCH", 10, "Muted", FontWeights.Bold));
            var allApps = MakeText("VIEW ALL  →", 9, "Accent", FontWeights.Bold);
            allApps.Cursor = Cursors.Hand;
            allApps.MouseLeftButtonUp += delegate { ShowStart(); };
            Grid.SetColumn(allApps, 1);
            quickHeader.Children.Add(allApps);
            center.Children.Add(quickHeader);

            var tiles = new WrapPanel { Margin = new Thickness(-5, 0, -5, 8) };
            tiles.Children.Add(CreateTile("▤", "File Explorer", "Your files and folders", delegate { LaunchBuiltin("@explorer"); }));
            tiles.Children.Add(CreateTile("◎", "Web browser", "Open your default browser", delegate { LaunchBuiltin("@browser"); }));
            tiles.Children.Add(CreateTile("⚙", "Settings", "Windows and AeroOS settings", OpenWindowsSettings));
            tiles.Children.Add(CreateTile("▦", "Calculator", "Quick calculations", delegate { LaunchBuiltin("@calculator"); }));
            tiles.Children.Add(CreateTile("▧", "Paint", "Create and edit images", delegate { LaunchBuiltin("@paint"); }));
            tiles.Children.Add(CreateTile("▤", "Notepad", "Write a quick note", delegate { LaunchBuiltin("@notepad"); }));
            center.Children.Add(tiles);

            var lower = GlassCard(new Thickness(17, 13, 17, 13), 16);
            lower.Margin = new Thickness(0, 9, 0, 0);
            var lowerGrid = new Grid();
            lowerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            lowerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var lowerText = new StackPanel();
            lowerText.Children.Add(MakeText("AeroOS stays in its own window", 13, "Text", FontWeights.SemiBold));
            lowerText.Children.Add(MakeText("Windows remains underneath. Close AeroOS at any time to return to the normal desktop.", 10, "Muted"));
            lowerGrid.Children.Add(lowerText);
            var safeBadge = MakeText("SAFE SESSION", 9, "Accent", FontWeights.Bold);
            safeBadge.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(safeBadge, 1);
            lowerGrid.Children.Add(safeBadge);
            lower.Child = lowerGrid;
            center.Children.Add(lower);

            var right = new StackPanel();
            Grid.SetColumn(right, 2);
            body.Children.Add(right);

            var timeCard = GlassCard(new Thickness(18, 18, 18, 18), 20);
            timeCard.Margin = new Thickness(0, 0, 0, 13);
            var timeStack = new StackPanel();
            timeStack.Children.Add(MakeText("YOUR MOMENT", 9, "Muted", FontWeights.Bold, new Thickness(0, 0, 0, 13)));
            var largeTime = MakeText(DateTime.Now.ToString("h:mm"), 34, "Text", FontWeights.Light);
            timeStack.Children.Add(largeTime);
            timeStack.Children.Add(MakeText(DateTime.Now.ToString("dddd, dd MMMM yyyy"), 10, "Muted", FontWeights.Normal, new Thickness(0, 0, 0, 12)));
            var timeDivider = new Border { Height = 1, Background = LineBrush(), Margin = new Thickness(0, 1, 0, 11) };
            timeStack.Children.Add(timeDivider);
            timeStack.Children.Add(MakeText("A little more focus. A little less clutter.", 11, "Muted"));
            timeCard.Child = timeStack;
            right.Children.Add(timeCard);

            var toolsCard = GlassCard(new Thickness(16, 16, 16, 16), 20);
            toolsCard.Margin = new Thickness(0, 0, 0, 13);
            var toolsStack = new StackPanel();
            toolsStack.Children.Add(MakeText("CONTROL CENTER", 9, "Muted", FontWeights.Bold, new Thickness(0, 0, 0, 12)));
            toolsStack.Children.Add(QuickActionButton("Network & internet", delegate { OpenSetting("ms-settings:network-status", null); }));
            toolsStack.Children.Add(QuickActionButton("Display settings", delegate { OpenSetting("ms-settings:display", null); }));
            toolsStack.Children.Add(QuickActionButton("Sound settings", delegate { OpenSetting("ms-settings:sound", null); }));
            toolsStack.Children.Add(QuickActionButton("Bluetooth settings", delegate { OpenSetting("ms-settings:bluetooth", null); }));
            toolsCard.Child = toolsStack;
            right.Children.Add(toolsCard);

            var deviceCard = GlassCard(new Thickness(16, 14, 16, 14), 18);
            var deviceStack = new StackPanel();
            deviceStack.Children.Add(MakeText("THIS DEVICE", 9, "Muted", FontWeights.Bold, new Thickness(0, 0, 0, 8)));
            _deviceSummary = MakeText("Reading basic Windows hardware information…", 10, "Muted");
            _deviceSummary.TextWrapping = TextWrapping.Wrap;
            deviceStack.Children.Add(_deviceSummary);
            deviceCard.Child = deviceStack;
            right.Children.Add(deviceCard);
        }

        private void BuildTaskbar(Grid layout)
        {
            var taskbar = GlassCard(new Thickness(12, 8, 12, 8), 22);
            Grid.SetRow(taskbar, 2);
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var start = MakeButton("◈  Start", ShowStart, true, 12, 48);
            start.Width = 110;
            grid.Children.Add(start);

            var dock = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            dock.Children.Add(DockButton("▤", "File Explorer", delegate { LaunchBuiltin("@explorer"); }));
            dock.Children.Add(DockButton("◎", "Browser", delegate { LaunchBuiltin("@browser"); }));
            dock.Children.Add(DockButton("▦", "Calculator", delegate { LaunchBuiltin("@calculator"); }));
            dock.Children.Add(DockButton("▧", "Paint", delegate { LaunchBuiltin("@paint"); }));
            dock.Children.Add(DockButton("⚙", "Windows Settings", OpenWindowsSettings));
            dock.Children.Add(DockButton("▥", "Task Manager", delegate { LaunchBuiltin("@taskmanager"); }));
            Grid.SetColumn(dock, 1);
            grid.Children.Add(dock);

            var status = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
            status.Children.Add(MakeText(_settings.PerformanceMode ? "◌  LOW EFFECTS" : "◌  GLASS MODE", 9, "Muted", FontWeights.SemiBold));
            var exit = MakeButton("Exit AeroOS  ×", delegate { Close(); }, false, 10, 38);
            exit.Margin = new Thickness(15, 0, 0, 0);
            status.Children.Add(exit);
            Grid.SetColumn(status, 2);
            grid.Children.Add(status);

            taskbar.Child = grid;
            layout.Children.Add(taskbar);
        }

        private Grid BuildStartOverlay()
        {
            var overlay = new Grid
            {
                Background = BrushFrom("#99050A18"),
                Visibility = Visibility.Collapsed
            };
            overlay.MouseDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (ReferenceEquals(e.OriginalSource, overlay)) overlay.Visibility = Visibility.Collapsed;
            };
            var card = GlassCard(new Thickness(24, 22, 24, 20), 22);
            card.Width = 630;
            card.Height = 550;
            card.HorizontalAlignment = HorizontalAlignment.Left;
            card.VerticalAlignment = VerticalAlignment.Bottom;
            card.Margin = new Thickness(26, 0, 0, 105);
            card.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs e) { e.Handled = true; };
            var stack = new DockPanel();

            var top = new StackPanel();
            var titleRow = new Grid();
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleRow.Children.Add(MakeText("Start", 24, "Text", FontWeights.SemiBold));
            var close = MakeButton("✕", delegate { overlay.Visibility = Visibility.Collapsed; }, false, 10, 32, 38);
            Grid.SetColumn(close, 1);
            titleRow.Children.Add(close);
            top.Children.Add(titleRow);
            top.Children.Add(MakeText("Find your apps and open Windows tools.", 11, "Muted", FontWeights.Normal, new Thickness(0, 2, 0, 14)));

            var search = new TextBox
            {
                Height = 42,
                Padding = new Thickness(13, 9, 13, 9),
                FontSize = 12,
                Background = BrushFrom("#33FFFFFF"),
                Foreground = TextBrush(),
                BorderBrush = LineBrush(),
                CaretBrush = AccentBrush(),
                ToolTip = "Search installed Start Menu apps"
            };
            search.TextChanged += StartSearch_TextChanged;
            _startSearch = search;
            top.Children.Add(search);
            top.Children.Add(MakeText("PINNED WINDOWS TOOLS", 9, "Muted", FontWeights.Bold, new Thickness(2, 17, 0, 9)));
            var pinned = new WrapPanel { Margin = new Thickness(-4, 0, -4, 9) };
            pinned.Children.Add(StartPinned("▤", "Explorer", delegate { LaunchBuiltin("@explorer"); }));
            pinned.Children.Add(StartPinned("◎", "Browser", delegate { LaunchBuiltin("@browser"); }));
            pinned.Children.Add(StartPinned("⚙", "Settings", OpenWindowsSettings));
            pinned.Children.Add(StartPinned("▦", "Calculator", delegate { LaunchBuiltin("@calculator"); }));
            pinned.Children.Add(StartPinned("▧", "Paint", delegate { LaunchBuiltin("@paint"); }));
            pinned.Children.Add(StartPinned("▤", "Notepad", delegate { LaunchBuiltin("@notepad"); }));
            top.Children.Add(pinned);
            top.Children.Add(MakeText("INSTALLED APPLICATIONS", 9, "Muted", FontWeights.Bold, new Thickness(2, 5, 0, 8)));
            DockPanel.SetDock(top, Dock.Top);
            stack.Children.Add(top);

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            _startResults = new StackPanel();
            scroll.Content = _startResults;
            stack.Children.Add(scroll);
            var footer = MakeText("AeroOS opens apps through Windows. It does not install or modify them.", 9, "Muted", FontWeights.Normal, new Thickness(1, 8, 0, 0));
            DockPanel.SetDock(footer, Dock.Bottom);
            stack.Children.Add(footer);

            card.Child = stack;
            overlay.Children.Add(card);
            return overlay;
        }

        private Grid BuildSettingsOverlay()
        {
            var overlay = new Grid
            {
                Background = BrushFrom("#AA050A18"),
                Visibility = Visibility.Collapsed
            };
            overlay.MouseDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (ReferenceEquals(e.OriginalSource, overlay)) overlay.Visibility = Visibility.Collapsed;
            };
            var card = GlassCard(new Thickness(27, 24, 27, 24), 23);
            card.Width = 650;
            card.Height = 500;
            card.HorizontalAlignment = HorizontalAlignment.Center;
            card.VerticalAlignment = VerticalAlignment.Center;
            card.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs e) { e.Handled = true; };
            var stack = new StackPanel();

            var titleRow = new Grid();
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleRow.Children.Add(MakeText("Personalize AeroOS", 23, "Text", FontWeights.SemiBold));
            var close = MakeButton("✕", delegate { overlay.Visibility = Visibility.Collapsed; }, false, 10, 34, 42);
            Grid.SetColumn(close, 1);
            titleRow.Children.Add(close);
            stack.Children.Add(titleRow);
            stack.Children.Add(MakeText("A lightweight desktop experience, with settings kept inside AeroOS.", 11, "Muted", FontWeights.Normal, new Thickness(0, 3, 0, 20)));

            stack.Children.Add(SettingCard("Appearance", "Switch between a deep glass look and a brighter workspace.",
                MakeButton(_settings.Theme == "Dark" ? "Switch to Light" : "Switch to Dark", delegate
                {
                    _settings.Theme = _settings.Theme == "Dark" ? "Light" : "Dark";
                    SaveSettings();
                    BuildShell();
                    ShowSettings();
                }, false, 11, 38)));

            var accentRow = new WrapPanel { Margin = new Thickness(0, 8, 0, 8) };
            accentRow.Children.Add(MakeText("ACCENT COLOUR", 9, "Muted", FontWeights.Bold, new Thickness(2, 7, 12, 0)));
            accentRow.Children.Add(AccentChoice("#8B7CF6", "Violet"));
            accentRow.Children.Add(AccentChoice("#4C8DFF", "Blue"));
            accentRow.Children.Add(AccentChoice("#29B6C8", "Teal"));
            accentRow.Children.Add(AccentChoice("#E879A8", "Rose"));
            stack.Children.Add(accentRow);

            var performance = MakeButton(_settings.PerformanceMode ? "✓  Reduced effects: ON" : "◯  Reduced effects: OFF",
                delegate
                {
                    _settings.PerformanceMode = !_settings.PerformanceMode;
                    SaveSettings();
                    BuildShell();
                    ShowSettings();
                }, false, 11, 40);
            stack.Children.Add(SettingCard("Performance", "Reduced effects avoid the custom background image and keep the interface static for older graphics hardware.", performance));

            var wallpaperRow = new StackPanel { Orientation = Orientation.Horizontal };
            wallpaperRow.Children.Add(MakeButton("Choose wallpaper", delegate { ChooseWallpaper_Click(this, new RoutedEventArgs()); }, false, 11, 38));
            var clearWallpaper = MakeButton("Remove wallpaper", delegate
            {
                _settings.WallpaperPath = string.Empty;
                SaveSettings();
                BuildShell();
                ShowSettings();
            }, false, 11, 38);
            clearWallpaper.Margin = new Thickness(8, 0, 0, 0);
            wallpaperRow.Children.Add(clearWallpaper);
            stack.Children.Add(SettingCard("Workspace background", "Wallpaper is displayed only inside AeroOS. Your Windows wallpaper is never changed.", wallpaperRow));

            var bottom = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 17, 0, 0) };
            bottom.Children.Add(MakeButton("Open Windows Settings", OpenWindowsSettings, false, 10, 36));
            var closeBottom = MakeButton("Done", delegate { overlay.Visibility = Visibility.Collapsed; }, true, 10, 36);
            closeBottom.Margin = new Thickness(8, 0, 0, 0);
            bottom.Children.Add(closeBottom);
            stack.Children.Add(bottom);

            card.Child = stack;
            overlay.Children.Add(card);
            return overlay;
        }

        private Border SettingCard(string title, string description, UIElement control)
        {
            var card = GlassCard(new Thickness(14, 12, 14, 12), 15);
            card.Margin = new Thickness(0, 0, 0, 9);
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var textStack = new StackPanel();
            textStack.Children.Add(MakeText(title, 12, "Text", FontWeights.SemiBold));
            textStack.Children.Add(MakeText(description, 9, "Muted", FontWeights.Normal, new Thickness(0, 3, 12, 0)));
            grid.Children.Add(textStack);
            Grid.SetColumn(control, 1);
            var frameworkControl = control as FrameworkElement;
            if (frameworkControl != null) frameworkControl.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(control);
            card.Child = grid;
            return card;
        }

        private Button AccentChoice(string color, string label)
        {
            var button = MakeButton("● " + label, delegate
            {
                _settings.AccentColor = color;
                SaveSettings();
                BuildShell();
                ShowSettings();
            }, false, 9, 32);
            button.Margin = new Thickness(3, 0, 3, 0);
            button.Foreground = BrushFrom(color);
            return button;
        }

        private Button PlaceButton(string glyph, string label, string path)
        {
            return PlaceActionButton(glyph, label, delegate { OpenFolder(path); });
        }

        private Button PlaceActionButton(string glyph, string label, Action action)
        {
            var button = new Button
            {
                Content = glyph + "    " + label,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Height = 36,
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(10, 4, 7, 4),
                Background = Brushes.Transparent,
                Foreground = TextBrush(),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 11,
                Cursor = Cursors.Hand,
                FocusVisualStyle = null
            };
            button.Click += delegate { action(); };
            return button;
        }

        private Button QuickActionButton(string label, Action action)
        {
            var button = MakeButton("›   " + label, delegate { action(); }, false, 10, 35);
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            button.Margin = new Thickness(0, 2, 0, 2);
            button.Background = BrushFrom("#1CFFFFFF");
            return button;
        }

        private Button CreateTile(string glyph, string title, string subtitle, Action action)
        {
            var button = new Button
            {
                Width = 145,
                Height = 116,
                Margin = new Thickness(5),
                Padding = new Thickness(12, 11, 12, 10),
                Background = GlassBrush(),
                BorderBrush = LineBrush(),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FocusVisualStyle = null,
                ToolTip = subtitle
            };
            var stack = new StackPanel();
            stack.Children.Add(MakeText(glyph, 24, "Accent", FontWeights.Normal, new Thickness(0, 0, 0, 9)));
            stack.Children.Add(MakeText(title, 11, "Text", FontWeights.SemiBold));
            stack.Children.Add(MakeText(subtitle, 8, "Muted", FontWeights.Normal, new Thickness(0, 3, 0, 0)));
            button.Content = stack;
            button.Click += delegate { action(); };
            return button;
        }

        private Button StartPinned(string glyph, string label, Action action)
        {
            var button = new Button
            {
                Width = 88,
                Height = 70,
                Margin = new Thickness(4),
                Background = GlassBrush(),
                BorderBrush = LineBrush(),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FocusVisualStyle = null
            };
            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            stack.Children.Add(MakeText(glyph, 22, "Accent", FontWeights.Normal));
            stack.Children.Add(MakeText(label, 9, "Text", FontWeights.SemiBold));
            button.Content = stack;
            button.Click += delegate { action(); };
            return button;
        }

        private Button DockButton(string glyph, string label, Action action)
        {
            var button = new Button
            {
                Width = 46,
                Height = 46,
                Margin = new Thickness(3, 0, 3, 0),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = TextBrush(),
                FontSize = 21,
                Content = glyph,
                ToolTip = label,
                Cursor = Cursors.Hand,
                FocusVisualStyle = null
            };
            button.Click += delegate { action(); };
            return button;
        }

        private Border GlassCard(Thickness padding, double radius)
        {
            return new Border
            {
                Background = GlassBrush(),
                BorderBrush = GlassEdgeBrush(),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(radius),
                Padding = padding,
                SnapsToDevicePixels = true
            };
        }

        private Button MakeButton(string label, Action action, bool primary, double fontSize, double height, double width = double.NaN)
        {
            var button = new Button
            {
                Content = label,
                Height = height,
                MinWidth = double.IsNaN(width) ? 0 : width,
                Padding = new Thickness(14, 5, 14, 5),
                Margin = new Thickness(4, 0, 4, 0),
                Background = primary ? AccentFillBrush() : GlassBrush(),
                Foreground = primary ? Brushes.White : TextBrush(),
                BorderBrush = primary ? AccentEdgeBrush() : GlassEdgeBrush(),
                BorderThickness = new Thickness(1),
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                FocusVisualStyle = null
            };
            button.Click += delegate { action(); };
            return button;
        }

        private TextBlock MakeText(string text, double size, string tone, FontWeight? weight = null, Thickness? margin = null)
        {
            Brush brush = tone == "Text" ? TextBrush()
                : tone == "Accent" ? AccentBrush()
                : tone == "Muted" ? MutedBrush()
                : TextBrush();
            return new TextBlock
            {
                Text = text,
                FontSize = size,
                FontWeight = weight ?? FontWeights.Normal,
                Foreground = brush,
                Margin = margin ?? new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.None,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private void RenderStartResults(string filter)
        {
            if (_startResults == null) return;
            _startResults.Children.Clear();
            var all = _builtInApps.Concat(_applications).ToList();
            var matches = all.Where(x => string.IsNullOrWhiteSpace(filter)
                || x.Name.IndexOf(filter.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0
                || (x.Category ?? string.Empty).IndexOf(filter.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .Take(40)
                .ToList();

            if (matches.Count == 0)
            {
                _startResults.Children.Add(MakeText(_isLoadingApps ? "Scanning Start Menu shortcuts…" : "No matching apps found.", 11, "Muted", FontWeights.Normal, new Thickness(5, 12, 5, 12)));
                return;
            }

            foreach (var app in matches)
            {
                var button = new Button
                {
                    Height = 37,
                    Margin = new Thickness(0, 1, 0, 1),
                    Padding = new Thickness(10, 4, 10, 4),
                    Background = Brushes.Transparent,
                    Foreground = TextBrush(),
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Cursor = Cursors.Hand,
                    FocusVisualStyle = null
                };
                button.Content = "◦    " + app.Name + "     ·     " + app.Category;
                button.Click += delegate
                {
                    _startOverlay.Visibility = Visibility.Collapsed;
                    LaunchEntry(app);
                };
                _startResults.Children.Add(button);
            }
        }

        private void DesktopSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_desktopSearch == null || string.IsNullOrWhiteSpace(_desktopSearch.Text)) return;
            ShowStart();
            if (_startSearch != null)
            {
                _startSearch.Text = _desktopSearch.Text;
                _startSearch.CaretIndex = _startSearch.Text.Length;
            }
        }

        private void StartSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderStartResults(_startSearch == null ? string.Empty : _startSearch.Text);
        }

        private void ShowStart()
        {
            if (_settingsOverlay != null) _settingsOverlay.Visibility = Visibility.Collapsed;
            if (_startOverlay == null) return;
            _startOverlay.Visibility = Visibility.Visible;
            RenderStartResults(_startSearch == null ? string.Empty : _startSearch.Text);
            if (_startSearch != null) _startSearch.Focus();
        }

        private void ShowSettings()
        {
            if (_startOverlay != null) _startOverlay.Visibility = Visibility.Collapsed;
            if (_settingsOverlay != null) _settingsOverlay.Visibility = Visibility.Visible;
        }

        private void OpenWindowsSettings()
        {
            OpenSetting("ms-settings:", null);
        }

        private void OpenSetting(string uri, string fallback)
        {
            try
            {
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(fallback))
                    Process.Start(new ProcessStartInfo("control.exe", fallback) { UseShellExecute = true });
                else
                    MessageBox.Show(this, "Windows could not open that Settings page. You can still use the normal Windows Settings app.", "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenFolder(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return;
                Process.Start(new ProcessStartInfo("explorer.exe", path.StartsWith("shell:", StringComparison.OrdinalIgnoreCase) ? path : "\"" + path + "\"")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Windows could not open this location." + Environment.NewLine + ex.Message, "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LaunchEntry(AppEntry app)
        {
            if (app == null || string.IsNullOrWhiteSpace(app.Path)) return;
            if (app.Path.StartsWith("@", StringComparison.Ordinal))
            {
                LaunchBuiltin(app.Path);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(app.Path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Windows could not open " + app.Name + "." + Environment.NewLine + ex.Message, "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LaunchBuiltin(string key)
        {
            try
            {
                switch (key)
                {
                    case "@explorer": Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true }); break;
                    case "@browser": Process.Start(new ProcessStartInfo("https://www.bing.com") { UseShellExecute = true }); break;
                    case "@settings": OpenWindowsSettings(); break;
                    case "@calculator": Process.Start(new ProcessStartInfo("calc.exe") { UseShellExecute = true }); break;
                    case "@paint": Process.Start(new ProcessStartInfo("mspaint.exe") { UseShellExecute = true }); break;
                    case "@notepad": Process.Start(new ProcessStartInfo("notepad.exe") { UseShellExecute = true }); break;
                    case "@taskmanager": Process.Start(new ProcessStartInfo("taskmgr.exe") { UseShellExecute = true }); break;
                    case "@control": Process.Start(new ProcessStartInfo("control.exe") { UseShellExecute = true }); break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Windows could not open that app. It may not be available on this PC." + Environment.NewLine + ex.Message, "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private List<AppEntry> CreateBuiltInApps()
        {
            return new List<AppEntry>
            {
                new AppEntry { Name = "File Explorer", Path = "@explorer", Category = "Windows tools" },
                new AppEntry { Name = "Web browser", Path = "@browser", Category = "Internet" },
                new AppEntry { Name = "Windows Settings", Path = "@settings", Category = "Windows tools" },
                new AppEntry { Name = "Calculator", Path = "@calculator", Category = "Accessories" },
                new AppEntry { Name = "Paint", Path = "@paint", Category = "Accessories" },
                new AppEntry { Name = "Notepad", Path = "@notepad", Category = "Accessories" },
                new AppEntry { Name = "Task Manager", Path = "@taskmanager", Category = "System tools" },
                new AppEntry { Name = "Control Panel", Path = "@control", Category = "System tools" }
            };
        }

        private void ChooseWallpaper_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Choose a wallpaper for AeroOS only",
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*",
                CheckFileExists = true
            };
            if (dialog.ShowDialog(this) == true)
            {
                _settings.WallpaperPath = dialog.FileName;
                SaveSettings();
                BuildShell();
                ShowSettings();
            }
        }

        private void SaveSettings()
        {
            try { _preferences.Save(_settings); }
            catch (Exception ex)
            {
                MessageBox.Show(this, "AeroOS could not save its own preferences. Windows settings were not changed." + Environment.NewLine + ex.Message, "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateClock()
        {
            if (_clockLabel != null) _clockLabel.Text = DateTime.Now.ToString("h:mm tt");
            if (_dateLabel != null) _dateLabel.Text = DateTime.Now.ToString("ddd, dd MMM yyyy");
        }

        private static string GetGreeting()
        {
            var hour = DateTime.Now.Hour;
            if (hour < 12) return "Good morning";
            if (hour < 18) return "Good afternoon";
            return "Good evening";
        }

        private Brush TextBrush()
        {
            return _settings.Theme == "Light" ? BrushFrom("#182238") : BrushFrom("#F4F7FF");
        }

        private Brush MutedBrush()
        {
            return _settings.Theme == "Light" ? BrushFrom("#68758C") : BrushFrom("#AAB8D0");
        }

        private Brush AccentBrush()
        {
            return BrushFrom(string.IsNullOrWhiteSpace(_settings.AccentColor) ? "#8B7CF6" : _settings.AccentColor);
        }

        private Brush LineBrush()
        {
            return _settings.Theme == "Light" ? BrushFrom("#667C8DA6") : BrushFrom("#3FFFFFFF");
        }

        private Brush GlassBrush()
        {
            var stops = new GradientStopCollection();
            if (_settings.Theme == "Light")
            {
                stops.Add(new GradientStop(ColorFrom("#F4FFFFFF"), 0.00));
                stops.Add(new GradientStop(ColorFrom("#E4EAF5FF"), 0.42));
                stops.Add(new GradientStop(ColorFrom("#D6FFFFFF"), 1.00));
            }
            else
            {
                stops.Add(new GradientStop(ColorFrom("#D8384A6B"), 0.00));
                stops.Add(new GradientStop(ColorFrom("#C51D2B49"), 0.48));
                stops.Add(new GradientStop(ColorFrom("#B8142038"), 1.00));
            }

            var brush = new LinearGradientBrush(stops, new Point(0, 0), new Point(1, 1));
            brush.Freeze();
            return brush;
        }

        private Brush GlassEdgeBrush()
        {
            var stops = new GradientStopCollection
            {
                new GradientStop(ColorFrom(_settings.Theme == "Light" ? "#E6FFFFFF" : "#B6FFFFFF"), 0.00),
                new GradientStop(ColorFrom(_settings.Theme == "Light" ? "#829BAFC8" : "#5A90A6D0"), 0.48),
                new GradientStop(ColorFrom(_settings.Theme == "Light" ? "#A4FFFFFF" : "#624B5FC5"), 1.00)
            };
            var brush = new LinearGradientBrush(stops, new Point(0, 0), new Point(1, 1));
            brush.Freeze();
            return brush;
        }

        private Brush AccentFillBrush()
        {
            var baseColor = ColorFrom(_settings.AccentColor);
            var stops = new GradientStopCollection
            {
                new GradientStop(AdjustColor(baseColor, 0.18), 0.00),
                new GradientStop(baseColor, 0.48),
                new GradientStop(AdjustColor(baseColor, -0.16), 1.00)
            };
            var brush = new LinearGradientBrush(stops, new Point(0, 0), new Point(1, 1));
            brush.Freeze();
            return brush;
        }

        private Brush AccentEdgeBrush()
        {
            var baseColor = ColorFrom(_settings.AccentColor);
            var stops = new GradientStopCollection
            {
                new GradientStop(AdjustColor(baseColor, 0.34), 0.00),
                new GradientStop(AdjustColor(baseColor, -0.04), 1.00)
            };
            var brush = new LinearGradientBrush(stops, new Point(0, 0), new Point(1, 1));
            brush.Freeze();
            return brush;
        }

        private void AddAmbientGlow(double width, double height, string colorHex, double opacity,
            HorizontalAlignment horizontal, VerticalAlignment vertical, Thickness margin)
        {
            var tint = ColorFrom(colorHex);
            var glowStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(0x8A, tint.R, tint.G, tint.B), 0.00),
                new GradientStop(Color.FromArgb(0x35, tint.R, tint.G, tint.B), 0.48),
                new GradientStop(Color.FromArgb(0x00, tint.R, tint.G, tint.B), 1.00)
            };
            var radial = new RadialGradientBrush(glowStops)
            {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.42, 0.38),
                RadiusX = 0.5,
                RadiusY = 0.5
            };
            radial.Freeze();

            ShellRoot.Children.Add(new Ellipse
            {
                Width = width,
                Height = height,
                Fill = radial,
                Opacity = opacity,
                HorizontalAlignment = horizontal,
                VerticalAlignment = vertical,
                Margin = margin,
                IsHitTestVisible = false,
                SnapsToDevicePixels = true
            });
        }

        private static Color AdjustColor(Color color, double amount)
        {
            byte Adjust(byte channel)
            {
                var value = channel + (amount * 255.0);
                return (byte)Math.Max(0, Math.Min(255, value));
            }

            return Color.FromArgb(color.A, Adjust(color.R), Adjust(color.G), Adjust(color.B));
        }

        private static Brush BrushFrom(string color)
        {
            try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)); }
            catch { return new SolidColorBrush(Color.FromRgb(139, 124, 246)); }
        }

        private static Color ColorFrom(string color)
        {
            try { return (Color)ColorConverter.ConvertFromString(color); }
            catch { return Color.FromRgb(16, 23, 44); }
        }
    }
}