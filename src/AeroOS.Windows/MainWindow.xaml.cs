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
        private TextBlock _dashboardClockLabel;
        private TextBlock _dashboardDateLabel;
        private bool _isLoadingApps;

        private Grid _pageHost;
        private TextBlock _pageHeading;
        private TextBlock _pageDescription;
        private TextBox _appsSearch;
        private StackPanel _appsResults;
        private string _currentPage = "Home";
        private readonly Dictionary<string, Button> _navigationButtons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);

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
                if (string.Equals(_currentPage, "Apps", StringComparison.OrdinalIgnoreCase))
                    RenderAppsPage();
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
                        + "Graphics: " + snapshot.GpuName + Environment.NewLine
                        + snapshot.Architecture + "  •  " + snapshot.LogicalProcessors + " logical processors"
                        + (snapshot.TotalMemoryGb > 0 ? Environment.NewLine + snapshot.TotalMemoryGb.ToString("0.0") + " GB RAM total" : string.Empty)
                        + (snapshot.AvailableMemoryGb > 0 ? "  •  " + snapshot.AvailableMemoryGb.ToString("0.0") + " GB available" : string.Empty)
                        + (snapshot.TotalSystemDriveGb > 0 ? Environment.NewLine + "System drive: " + snapshot.FreeSystemDriveGb.ToString("0.0") + " GB free of " + snapshot.TotalSystemDriveGb.ToString("0.0") + " GB" : string.Empty);
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
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Space)
            {
                if (_startOverlay != null && _startOverlay.Visibility == Visibility.Visible)
                    _startOverlay.Visibility = Visibility.Collapsed;
                else
                    ShowStart();
                e.Handled = true;
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.OemComma)
            {
                ShowSettings();
                e.Handled = true;
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D1)
            {
                NavigateTo("Home");
                e.Handled = true;
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D2)
            {
                NavigateTo("Apps");
                e.Handled = true;
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D3)
            {
                NavigateTo("Files");
                e.Handled = true;
                return;
            }
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
                new GradientStop(ColorFrom("#FF07101F"), 0.00),
                new GradientStop(ColorFrom("#FF101D35"), 0.42),
                new GradientStop(ColorFrom("#FF171B3A"), 0.75),
                new GradientStop(ColorFrom("#FF071728"), 1.00)
            };
            ShellRoot.Background = new LinearGradientBrush(backdropStops, new Point(0, 0), new Point(1, 1));

            if (!_settings.PerformanceMode && !string.IsNullOrWhiteSpace(_settings.WallpaperPath) && File.Exists(_settings.WallpaperPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 1500;
                    bitmap.UriSource = new Uri(_settings.WallpaperPath, UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ShellRoot.Children.Add(new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.UniformToFill,
                        Opacity = 0.24,
                        IsHitTestVisible = false
                    });
                }
                catch { }
            }

            // Static gradients provide an acrylic-like atmosphere without expensive live blur.
            AddAmbientGlow(760, 680, "#765BFF", 0.28, HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(-260, -290, 0, 0));
            AddAmbientGlow(760, 690, "#20C4D8", 0.20, HorizontalAlignment.Right, VerticalAlignment.Bottom, new Thickness(0, 0, -285, -300));
            AddAmbientGlow(470, 470, "#A56BFF", 0.13, HorizontalAlignment.Right, VerticalAlignment.Top, new Thickness(0, -230, -170, 0));

            var layout = new Grid { Margin = new Thickness(28, 20, 28, 18) };
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
            var header = GlassCard(new Thickness(19, 9, 18, 9), 20);
            Grid.SetRow(header, 0);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var brand = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var mark = new Border
            {
                Width = 42,
                Height = 42,
                CornerRadius = new CornerRadius(14),
                Background = AccentFillBrush(),
                BorderBrush = BrushFrom("#70FFFFFF"),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = "✦",
                    Foreground = Brushes.White,
                    FontSize = 23,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            brand.Children.Add(mark);

            var brandText = new StackPanel { Margin = new Thickness(12, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            brandText.Children.Add(MakeText("AEROSHELL", 15, "Text", FontWeights.Bold));
            brandText.Children.Add(MakeText("WINDOWS 12-INSPIRED DESKTOP", 8, "Muted", FontWeights.SemiBold));
            brand.Children.Add(brandText);
            grid.Children.Add(brand);

            var center = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            center.Children.Add(MakeText("PERSONAL WORKSPACE", 9, "Muted", FontWeights.Bold));
            Grid.SetColumn(center, 1);
            grid.Children.Add(center);

            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            var clockBlock = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
            _clockLabel = MakeText("", 14, "Text", FontWeights.SemiBold);
            _clockLabel.HorizontalAlignment = HorizontalAlignment.Right;
            _dateLabel = MakeText("", 8, "Muted", FontWeights.Medium);
            _dateLabel.HorizontalAlignment = HorizontalAlignment.Right;
            clockBlock.Children.Add(_clockLabel);
            clockBlock.Children.Add(_dateLabel);
            actions.Children.Add(clockBlock);
            actions.Children.Add(MakeButton("⚙  Personalize", delegate { ShowSettings(); }, false, 10, 38));
            var close = MakeButton("×", delegate { Close(); }, false, 17, 38, 40);
            close.ToolTip = "Close AeroShell and return to Windows";
            actions.Children.Add(close);
            Grid.SetColumn(actions, 2);
            grid.Children.Add(actions);

            header.Child = grid;
            layout.Children.Add(header);
        }


        private void BuildDesktop(Grid layout)
        {
            var body = new Grid { Margin = new Thickness(0, 17, 0, 16) };
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(202) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(body, 1);
            layout.Children.Add(body);

            var sidebar = GlassCard(new Thickness(14, 17, 14, 14), 22);
            sidebar.Margin = new Thickness(0, 0, 15, 0);
            var sideScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            var sideStack = new StackPanel();
            var brand = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 1, 0, 21) };
            var logo = new Border
            {
                Width = 35,
                Height = 35,
                CornerRadius = new CornerRadius(12),
                Background = AccentFillBrush(),
                Child = new TextBlock
                {
                    Text = "✦",
                    Foreground = Brushes.White,
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            brand.Children.Add(logo);
            var brandName = new StackPanel { Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            brandName.Children.Add(MakeText("AEROSHELL", 12, "Text", FontWeights.Bold));
            brandName.Children.Add(MakeText("DESKTOP", 8, "Muted", FontWeights.SemiBold));
            brand.Children.Add(brandName);
            sideStack.Children.Add(brand);

            sideStack.Children.Add(MakeText("WORKSPACE", 8, "Muted", FontWeights.Bold, new Thickness(8, 0, 0, 8)));
            _navigationButtons.Clear();
            sideStack.Children.Add(NavigationButton("Home", "⌂", "Home"));
            sideStack.Children.Add(NavigationButton("Apps", "▦", "All apps"));
            sideStack.Children.Add(NavigationButton("Files", "▤", "Files & folders"));
            sideStack.Children.Add(NavigationButton("Settings", "⚙", "Personalization"));

            sideStack.Children.Add(new Border { Height = 1, Background = LineBrush(), Margin = new Thickness(7, 17, 7, 13) });
            sideStack.Children.Add(MakeText("QUICK PLACES", 8, "Muted", FontWeights.Bold, new Thickness(8, 0, 0, 7)));
            sideStack.Children.Add(PlaceActionButton("↓", "Downloads", delegate
            {
                OpenFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
            }));
            sideStack.Children.Add(PlaceActionButton("▣", "This PC", delegate { OpenFolder("shell:MyComputerFolder"); }));
            sideStack.Children.Add(PlaceActionButton("♲", "Recycle Bin", delegate { OpenFolder("shell:RecycleBinFolder"); }));

            sideStack.Children.Add(new Border { Height = 1, Background = LineBrush(), Margin = new Thickness(7, 16, 7, 12) });
            sideStack.Children.Add(MakeText("WINDOWS TOOLS", 8, "Muted", FontWeights.Bold, new Thickness(8, 0, 0, 7)));
            sideStack.Children.Add(PlaceActionButton("⚙", "Windows Settings", OpenWindowsSettings));
            sideStack.Children.Add(PlaceActionButton("▥", "Task Manager", delegate { LaunchBuiltin("@taskmanager"); }));
            sideStack.Children.Add(new Border { Height = 1, Background = LineBrush(), Margin = new Thickness(7, 16, 7, 12) });
            var safetyNote = GlassCard(new Thickness(10, 10, 10, 10), 14);
            safetyNote.Background = BrushFrom("#168B7CF6");
            var safetyStack = new StackPanel();
            safetyStack.Children.Add(MakeText("WINDOWS 10 STAYS SAFE", 8, "Accent", FontWeights.Bold));
            safetyStack.Children.Add(MakeText("AeroShell is a launcher window. Exit any time to return to your normal desktop.", 8, "Muted", FontWeights.Normal, new Thickness(0, 4, 0, 0)));
            safetyNote.Child = safetyStack;
            sideStack.Children.Add(safetyNote);

            sideScroll.Content = sideStack;
            sidebar.Child = sideScroll;
            body.Children.Add(sidebar);

            var main = new Grid();
            main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(main, 1);
            body.Children.Add(main);

            var pageHeader = new Grid { Margin = new Thickness(5, 1, 5, 13) };
            pageHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pageHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var pageLabels = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _pageHeading = MakeText("Home", 23, "Text", FontWeights.SemiBold);
            _pageDescription = MakeText("Your desktop at a glance.", 9, "Muted", FontWeights.Normal, new Thickness(1, 2, 0, 0));
            pageLabels.Children.Add(_pageHeading);
            pageLabels.Children.Add(_pageDescription);
            pageHeader.Children.Add(pageLabels);
            var headerActions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var dateCard = GlassCard(new Thickness(12, 7, 12, 7), 13);
            var dateStack = new StackPanel();
            _clockLabel = MakeText("", 12, "Text", FontWeights.SemiBold);
            _dateLabel = MakeText("", 8, "Muted");
            dateStack.Children.Add(_clockLabel);
            dateStack.Children.Add(_dateLabel);
            dateCard.Child = dateStack;
            headerActions.Children.Add(dateCard);
            var closeButton = MakeButton("×", delegate { Close(); }, false, 17, 38, 40);
            closeButton.ToolTip = "Exit AeroShell and return to Windows";
            closeButton.Margin = new Thickness(8, 0, 0, 0);
            headerActions.Children.Add(closeButton);
            Grid.SetColumn(headerActions, 1);
            pageHeader.Children.Add(headerActions);
            main.Children.Add(pageHeader);

            var pageCard = GlassCard(new Thickness(17, 16, 17, 15), 21);
            pageCard.Margin = new Thickness(0, 0, 0, 2);
            _pageHost = new Grid();
            pageCard.Child = _pageHost;
            Grid.SetRow(pageCard, 1);
            main.Children.Add(pageCard);

            NavigateTo("Home");
        }

        private Button NavigationButton(string page, string glyph, string label)
        {
            var button = MakeButton(glyph + "    " + label, delegate
            {
                if (string.Equals(page, "Settings", StringComparison.OrdinalIgnoreCase))
                    ShowSettings();
                else
                    NavigateTo(page);
            }, false, 10, 40);
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            button.Padding = new Thickness(12, 4, 8, 4);
            button.Margin = new Thickness(0, 2, 0, 2);
            button.BorderBrush = Brushes.Transparent;
            _navigationButtons[page] = button;
            return button;
        }

        private void RefreshNavigationStyles()
        {
            foreach (var entry in _navigationButtons)
            {
                var selected = string.Equals(entry.Key, _currentPage, StringComparison.OrdinalIgnoreCase);
                entry.Value.Background = selected ? BrushFrom("#488B7CF6") : BrushFrom("#0FFFFFFF");
                entry.Value.BorderBrush = selected ? BrushFrom("#778B7CF6") : Brushes.Transparent;
                entry.Value.Foreground = TextBrush();
            }
        }

        private void NavigateTo(string page)
        {
            if (_pageHost == null) return;
            _currentPage = page ?? "Home";
            if (_startOverlay != null) _startOverlay.Visibility = Visibility.Collapsed;
            if (_settingsOverlay != null) _settingsOverlay.Visibility = Visibility.Collapsed;

            _pageHost.Children.Clear();
            UIElement content;
            switch (_currentPage)
            {
                case "Apps":
                    _pageHeading.Text = "All apps";
                    _pageDescription.Text = "Search, browse and pin applications installed on Windows.";
                    content = BuildAppsPage();
                    break;
                case "Files":
                    _pageHeading.Text = "Files & folders";
                    _pageDescription.Text = "Open your Windows libraries and common locations.";
                    content = BuildFilesPage();
                    break;
                default:
                    _currentPage = "Home";
                    _pageHeading.Text = "Home";
                    _pageDescription.Text = "Your desktop at a glance.";
                    content = BuildHomePage();
                    break;
            }

            _pageHost.Children.Add(content);
            RefreshNavigationStyles();
        }

        private UIElement BuildHomePage()
        {
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            var stack = new StackPanel();

            var hero = GlassCard(new Thickness(22, 20, 22, 19), 19);
            hero.Background = BrushFrom("#242A4375");
            var heroGrid = new Grid();
            heroGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            heroGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var welcome = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            welcome.Children.Add(MakeText(GetGreeting() + ", welcome back.", 24, "Text", FontWeights.SemiBold));
            welcome.Children.Add(MakeText("A calmer, clearer space for your Windows apps and files.", 10, "Muted", FontWeights.Normal, new Thickness(1, 4, 0, 0)));
            heroGrid.Children.Add(welcome);
            var heroAction = MakeButton("Browse apps  →", delegate { NavigateTo("Apps"); }, true, 10, 40);
            heroAction.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(heroAction, 1);
            heroGrid.Children.Add(heroAction);
            hero.Child = heroGrid;
            stack.Children.Add(hero);

            var searchCard = GlassCard(new Thickness(13, 3, 13, 3), 15);
            searchCard.Height = 47;
            searchCard.Margin = new Thickness(0, 13, 0, 17);
            var searchGrid = new Grid();
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            searchGrid.Children.Add(MakeText("⌕", 22, "Accent"));
            _desktopSearch = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = TextBrush(),
                CaretBrush = AccentBrush(),
                FontSize = 12,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                ToolTip = "Type an app name, then press Enter"
            };
            _desktopSearch.TextChanged += DesktopSearch_TextChanged;
            _desktopSearch.PreviewKeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    NavigateTo("Apps");
                    if (_appsSearch != null)
                    {
                        _appsSearch.Text = _desktopSearch.Text;
                        _appsSearch.Focus();
                    }
                    e.Handled = true;
                }
            };
            Grid.SetColumn(_desktopSearch, 1);
            searchGrid.Children.Add(_desktopSearch);
            searchCard.Child = searchGrid;
            stack.Children.Add(searchCard);

            stack.Children.Add(MakeText("QUICK LAUNCH", 9, "Muted", FontWeights.Bold, new Thickness(2, 0, 0, 9)));
            var tiles = new WrapPanel { Margin = new Thickness(-5, 0, -5, 8) };
            tiles.Children.Add(CreateTile("▤", "File Explorer", "Browse files and folders", delegate { LaunchBuiltin("@explorer"); }));
            tiles.Children.Add(CreateTile("◎", "Browser", "Open your default browser", delegate { LaunchBuiltin("@browser"); }));
            tiles.Children.Add(CreateTile("▦", "Calculator", "Quick calculations", delegate { LaunchBuiltin("@calculator"); }));
            tiles.Children.Add(CreateTile("▧", "Paint", "Create and edit images", delegate { LaunchBuiltin("@paint"); }));
            tiles.Children.Add(CreateTile("▤", "Notepad", "Write a quick note", delegate { LaunchBuiltin("@notepad"); }));
            tiles.Children.Add(CreateTile("⚙", "Windows Settings", "Configure Windows", OpenWindowsSettings));
            stack.Children.Add(tiles);

            var lowerGrid = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            lowerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            lowerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var recentCard = GlassCard(new Thickness(15, 13, 15, 13), 17);
            recentCard.Margin = new Thickness(0, 0, 7, 0);
            var recentStack = new StackPanel();
            recentStack.Children.Add(MakeText("RECENTLY OPENED", 9, "Muted", FontWeights.Bold, new Thickness(0, 0, 0, 9)));
            var all = _builtInApps.Concat(_applications).ToList();
            var recent = _settings.RecentPaths
                .Select(path => all.FirstOrDefault(app => string.Equals(app.Path, path, StringComparison.OrdinalIgnoreCase)))
                .Where(app => app != null)
                .GroupBy(app => app.Path, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Take(4)
                .ToList();
            if (recent.Count == 0)
                recentStack.Children.Add(MakeText("Apps you open will appear here.", 9, "Muted"));
            else
                foreach (var app in recent)
                    recentStack.Children.Add(PlaceActionButton("•", app.Name, delegate { LaunchEntry(app); }));
            recentCard.Child = recentStack;
            lowerGrid.Children.Add(recentCard);

            var deviceCard = GlassCard(new Thickness(15, 13, 15, 13), 17);
            deviceCard.Margin = new Thickness(7, 0, 0, 0);
            var deviceStack = new StackPanel();
            deviceStack.Children.Add(MakeText("THIS DEVICE", 9, "Muted", FontWeights.Bold, new Thickness(0, 0, 0, 9)));
            _deviceSummary = MakeText("Reading device information…", 9, "Muted");
            _deviceSummary.TextWrapping = TextWrapping.Wrap;
            deviceStack.Children.Add(_deviceSummary);
            deviceCard.Child = deviceStack;
            Grid.SetColumn(deviceCard, 1);
            lowerGrid.Children.Add(deviceCard);
            stack.Children.Add(lowerGrid);

            scroll.Content = stack;
            return scroll;
        }

        private UIElement BuildAppsPage()
        {
            var stack = new DockPanel();
            var top = new StackPanel();
            var searchCard = GlassCard(new Thickness(13, 2, 13, 2), 14);
            searchCard.Height = 46;
            var searchGrid = new Grid();
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            searchGrid.Children.Add(MakeText("⌕", 22, "Accent"));
            _appsSearch = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = TextBrush(),
                CaretBrush = AccentBrush(),
                FontSize = 12,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                ToolTip = "Search apps by name or category"
            };
            _appsSearch.TextChanged += delegate { RenderAppsPage(); };
            Grid.SetColumn(_appsSearch, 1);
            searchGrid.Children.Add(_appsSearch);
            searchCard.Child = searchGrid;
            top.Children.Add(searchCard);
            top.Children.Add(MakeText("PINNED WINDOWS TOOLS", 8, "Muted", FontWeights.Bold, new Thickness(2, 13, 0, 7)));
            var pinned = new WrapPanel();
            pinned.Children.Add(StartPinned("▤", "Explorer", delegate { LaunchBuiltin("@explorer"); }));
            pinned.Children.Add(StartPinned("◎", "Browser", delegate { LaunchBuiltin("@browser"); }));
            pinned.Children.Add(StartPinned("⚙", "Settings", OpenWindowsSettings));
            pinned.Children.Add(StartPinned("▦", "Calculator", delegate { LaunchBuiltin("@calculator"); }));
            top.Children.Add(pinned);
            DockPanel.SetDock(top, Dock.Top);
            stack.Children.Add(top);

            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 11, 0, 0)
            };
            _appsResults = new StackPanel();
            scroll.Content = _appsResults;
            stack.Children.Add(scroll);
            RenderAppsPage();
            return stack;
        }

        private void RenderAppsPage()
        {
            if (_appsResults == null) return;
            _appsResults.Children.Clear();
            var query = _appsSearch == null ? string.Empty : (_appsSearch.Text ?? string.Empty).Trim();
            var all = _builtInApps.Concat(_applications)
                .GroupBy(app => app.Path, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Where(app => query.Length == 0
                    || app.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || (app.Category ?? string.Empty).IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .OrderByDescending(app => query.Length > 0 && app.Name.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
                .ThenBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            _appsResults.Children.Add(MakeText(all.Count + (all.Count == 1 ? " application" : " applications"), 9, "Muted", FontWeights.Bold, new Thickness(2, 0, 0, 8)));
            if (all.Count == 0)
            {
                var empty = GlassCard(new Thickness(18, 17, 18, 17), 15);
                empty.Child = MakeText(_isLoadingApps ? "Loading Start Menu applications…" : "No matching applications. Try another search.", 10, "Muted");
                _appsResults.Children.Add(empty);
                return;
            }

            var wrap = new WrapPanel();
            foreach (var app in all.Take(120))
            {
                var card = GlassCard(new Thickness(12, 11, 12, 10), 15);
                card.Width = 190;
                card.Height = 108;
                card.Margin = new Thickness(4);
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var labels = new StackPanel();
                labels.Children.Add(MakeText(app.Name, 10, "Text", FontWeights.SemiBold));
                labels.Children.Add(MakeText(app.Category ?? "Windows application", 8, "Muted", FontWeights.Normal, new Thickness(0, 3, 0, 0)));
                grid.Children.Add(labels);
                var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                var pin = MakeButton(IsPinned(app) ? "★" : "＋", delegate { TogglePin(app); }, false, 10, 30, 34);
                pin.ToolTip = IsPinned(app) ? "Unpin this app" : "Pin this app";
                var open = MakeButton("Open  →", delegate { LaunchEntry(app); }, true, 9, 30);
                actions.Children.Add(pin);
                actions.Children.Add(open);
                Grid.SetRow(actions, 1);
                grid.Children.Add(actions);
                card.Child = grid;
                wrap.Children.Add(card);
            }
            _appsResults.Children.Add(wrap);
            if (all.Count > 120)
                _appsResults.Children.Add(MakeText("Showing the first 120 matches. Narrow your search to find other apps.", 8, "Muted", FontWeights.Normal, new Thickness(4, 8, 0, 0)));
        }

        private UIElement BuildFilesPage()
        {
            var stack = new StackPanel();
            stack.Children.Add(MakeText("OPEN A LOCATION", 9, "Muted", FontWeights.Bold, new Thickness(2, 0, 0, 9)));
            var tiles = new WrapPanel();
            tiles.Children.Add(CreateTile("⌂", "Home", "Your user folder", delegate { OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)); }));
            tiles.Children.Add(CreateTile("▤", "Documents", "Your documents", delegate { OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); }));
            tiles.Children.Add(CreateTile("↓", "Downloads", "Downloaded files", delegate { OpenFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")); }));
            tiles.Children.Add(CreateTile("▧", "Pictures", "Your pictures", delegate { OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)); }));
            tiles.Children.Add(CreateTile("♫", "Music", "Your music library", delegate { OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)); }));
            tiles.Children.Add(CreateTile("▣", "This PC", "Drives and devices", delegate { OpenFolder("shell:MyComputerFolder"); }));
            tiles.Children.Add(CreateTile("♲", "Recycle Bin", "Deleted items", delegate { OpenFolder("shell:RecycleBinFolder"); }));
            stack.Children.Add(tiles);

            var note = GlassCard(new Thickness(15, 13, 15, 13), 16);
            note.Margin = new Thickness(0, 12, 0, 0);
            note.Child = MakeText("These shortcuts open real Windows folders in File Explorer. AeroShell does not copy, move or modify your files when you open a location.", 9, "Muted");
            stack.Children.Add(note);
            return new ScrollViewer
            {
                Content = stack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }

        private void BuildTaskbar(Grid layout)
        {
            var taskbar = GlassCard(new Thickness(13, 9, 13, 9), 24);
            taskbar.Width = Math.Max(560, Math.Min(760, SystemParameters.WorkArea.Width - 40));
            taskbar.Height = 62;
            taskbar.HorizontalAlignment = HorizontalAlignment.Center;
            taskbar.VerticalAlignment = VerticalAlignment.Center;
            taskbar.Margin = new Thickness(0, 0, 0, 3);
            Grid.SetRow(taskbar, 2);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var start = MakeButton("⊞  Start", ShowStart, true, 11, 43, 104);
            start.Margin = new Thickness(0, 0, 10, 0);
            grid.Children.Add(start);

            var dock = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            dock.Children.Add(DockButton("▤", "File Explorer", delegate { LaunchBuiltin("@explorer"); }));
            dock.Children.Add(DockButton("◎", "Browser", delegate { LaunchBuiltin("@browser"); }));
            dock.Children.Add(DockButton("▦", "Calculator", delegate { LaunchBuiltin("@calculator"); }));
            dock.Children.Add(DockButton("▧", "Paint", delegate { LaunchBuiltin("@paint"); }));
            dock.Children.Add(DockButton("▤", "Notepad", delegate { LaunchBuiltin("@notepad"); }));
            dock.Children.Add(DockButton("▦", "All apps", delegate { NavigateTo("Apps"); }));
            dock.Children.Add(DockButton("⚙", "Windows Settings", OpenWindowsSettings));
            Grid.SetColumn(dock, 1);
            grid.Children.Add(dock);

            var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var status = MakeText(_settings.PerformanceMode ? "LOW EFFECTS" : "AEROSHELL", 8, "Muted", FontWeights.Bold);
            status.Margin = new Thickness(0, 0, 8, 0);
            actions.Children.Add(status);
            var exit = MakeButton("⏻", delegate { Close(); }, false, 16, 43, 43);
            exit.ToolTip = "Exit AeroShell and return to Windows";
            exit.Margin = new Thickness(0);
            actions.Children.Add(exit);
            Grid.SetColumn(actions, 2);
            grid.Children.Add(actions);

            taskbar.Child = grid;
            layout.Children.Add(taskbar);
        }

        private Grid BuildStartOverlay()
        {
            var overlay = new Grid
            {
                Background = BrushFrom("#99050A16"),
                Visibility = Visibility.Collapsed
            };
            overlay.MouseDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (ReferenceEquals(e.OriginalSource, overlay)) overlay.Visibility = Visibility.Collapsed;
            };

            var card = GlassCard(new Thickness(25, 23, 25, 20), 25);
            card.Width = Math.Max(340, Math.Min(720, SystemParameters.WorkArea.Width - 48));
            card.Height = Math.Max(330, Math.Min(575, SystemParameters.WorkArea.Height - 100));
            card.HorizontalAlignment = HorizontalAlignment.Center;
            card.VerticalAlignment = VerticalAlignment.Center;
            card.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs e) { e.Handled = true; };

            var stack = new DockPanel();
            var top = new StackPanel();
            var titleRow = new Grid();
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var title = new StackPanel();
            title.Children.Add(MakeText("Start", 26, "Text", FontWeights.SemiBold));
            title.Children.Add(MakeText("Your apps, tools and recent activity.", 10, "Muted", FontWeights.Normal, new Thickness(0, 2, 0, 0)));
            titleRow.Children.Add(title);
            var close = MakeButton("×", delegate { overlay.Visibility = Visibility.Collapsed; }, false, 16, 36, 40);
            Grid.SetColumn(close, 1);
            titleRow.Children.Add(close);
            top.Children.Add(titleRow);

            var search = new TextBox
            {
                Height = 46,
                Margin = new Thickness(0, 17, 0, 0),
                Padding = new Thickness(15, 9, 12, 9),
                FontSize = 13,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                Background = BrushFrom("#22FFFFFF"),
                Foreground = TextBrush(),
                BorderBrush = LineBrush(),
                BorderThickness = new Thickness(1),
                CaretBrush = AccentBrush(),
                ToolTip = "Search installed Start Menu apps"
            };
            search.TextChanged += StartSearch_TextChanged;
            search.PreviewKeyDown += StartSearch_PreviewKeyDown;
            _startSearch = search;
            top.Children.Add(search);

            top.Children.Add(MakeText("PINNED", 9, "Muted", FontWeights.Bold, new Thickness(2, 17, 0, 8)));
            var pinned = new WrapPanel { Margin = new Thickness(-4, 0, -4, 8) };
            pinned.Children.Add(StartPinned("▤", "Explorer", delegate { LaunchBuiltin("@explorer"); }));
            pinned.Children.Add(StartPinned("◎", "Browser", delegate { LaunchBuiltin("@browser"); }));
            pinned.Children.Add(StartPinned("⚙", "Settings", OpenWindowsSettings));
            pinned.Children.Add(StartPinned("▦", "Calculator", delegate { LaunchBuiltin("@calculator"); }));
            pinned.Children.Add(StartPinned("▧", "Paint", delegate { LaunchBuiltin("@paint"); }));
            pinned.Children.Add(StartPinned("▤", "Notepad", delegate { LaunchBuiltin("@notepad"); }));
            top.Children.Add(pinned);
            DockPanel.SetDock(top, Dock.Top);
            stack.Children.Add(top);

            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 5, 0, 0)
            };
            _startResults = new StackPanel();
            scroll.Content = _startResults;
            stack.Children.Add(scroll);

            var footer = MakeText("AeroShell launches the Windows apps already installed on this PC.", 9, "Muted", FontWeights.Normal, new Thickness(1, 9, 0, 0));
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
                Background = BrushFrom("#AA050A16"),
                Visibility = Visibility.Collapsed
            };
            overlay.MouseDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (ReferenceEquals(e.OriginalSource, overlay)) overlay.Visibility = Visibility.Collapsed;
            };

            var card = GlassCard(new Thickness(25, 23, 25, 22), 25);
            card.Width = Math.Max(340, Math.Min(700, SystemParameters.WorkArea.Width - 48));
            card.HorizontalAlignment = HorizontalAlignment.Center;
            card.VerticalAlignment = VerticalAlignment.Center;
            card.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs e) { e.Handled = true; };

            var stack = new StackPanel();
            var titleRow = new Grid();
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var heading = new StackPanel();
            heading.Children.Add(MakeText("Personalize AeroShell", 24, "Text", FontWeights.SemiBold));
            heading.Children.Add(MakeText("Tune the look and feel of your desktop workspace.", 10, "Muted", FontWeights.Normal, new Thickness(0, 3, 0, 0)));
            titleRow.Children.Add(heading);
            var close = MakeButton("×", delegate { overlay.Visibility = Visibility.Collapsed; }, false, 16, 36, 40);
            Grid.SetColumn(close, 1);
            titleRow.Children.Add(close);
            stack.Children.Add(titleRow);
            stack.Children.Add(new Border { Height = 1, Background = LineBrush(), Margin = new Thickness(0, 16, 0, 15) });

            stack.Children.Add(SettingCard("Appearance", "Switch between a deep night workspace and a brighter glass surface.",
                MakeButton(_settings.Theme == "Dark" ? "Switch to Light" : "Switch to Dark", delegate
                {
                    _settings.Theme = _settings.Theme == "Dark" ? "Light" : "Dark";
                    SaveSettings();
                    BuildShell();
                    ShowSettings();
                }, false, 10, 38)));

            var accentRow = new WrapPanel { Margin = new Thickness(0, 9, 0, 9) };
            accentRow.Children.Add(MakeText("ACCENT COLOUR", 9, "Muted", FontWeights.Bold, new Thickness(2, 8, 12, 0)));
            accentRow.Children.Add(AccentChoice("#8B7CF6", "Violet"));
            accentRow.Children.Add(AccentChoice("#4C8DFF", "Blue"));
            accentRow.Children.Add(AccentChoice("#29B6C8", "Teal"));
            accentRow.Children.Add(AccentChoice("#E879A8", "Rose"));
            stack.Children.Add(SettingCard("Accent", "Choose the highlight colour used by the Start menu, buttons and desktop details.", accentRow));

            var performance = MakeButton(_settings.PerformanceMode ? "✓  Reduced effects: ON" : "○  Reduced effects: OFF",
                delegate
                {
                    _settings.PerformanceMode = !_settings.PerformanceMode;
                    SaveSettings();
                    BuildShell();
                    ShowSettings();
                }, false, 10, 38);
            stack.Children.Add(SettingCard("Performance", "Recommended ON for older graphics hardware. It avoids loading the optional wallpaper.", performance));

            var wallpaperRow = new StackPanel { Orientation = Orientation.Horizontal };
            wallpaperRow.Children.Add(MakeButton("Choose wallpaper", delegate { ChooseWallpaper_Click(this, new RoutedEventArgs()); }, false, 10, 38));
            var clearWallpaper = MakeButton("Remove wallpaper", delegate
            {
                _settings.WallpaperPath = string.Empty;
                SaveSettings();
                BuildShell();
                ShowSettings();
            }, false, 10, 38);
            clearWallpaper.Margin = new Thickness(8, 0, 0, 0);
            wallpaperRow.Children.Add(clearWallpaper);
            stack.Children.Add(SettingCard("Desktop background", "This wallpaper affects AeroShell only. Your Windows wallpaper is not changed.", wallpaperRow));

            var note = GlassCard(new Thickness(13, 12, 13, 12), 15);
            note.Margin = new Thickness(0, 8, 0, 0);
            note.Child = MakeText("AeroShell is a Windows 12-inspired launcher, not a replacement operating system. Close it at any time to return to Windows.", 9, "Muted");
            stack.Children.Add(note);

            var bottom = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 15, 0, 0) };
            bottom.Children.Add(MakeButton("Open Windows Settings", OpenWindowsSettings, false, 9, 36));
            var done = MakeButton("Done", delegate { overlay.Visibility = Visibility.Collapsed; }, true, 10, 36);
            done.Margin = new Thickness(8, 0, 0, 0);
            bottom.Children.Add(done);
            stack.Children.Add(bottom);

            card.Child = stack;
            var settingsScroll = new ScrollViewer
            {
                Content = card,
                Width = card.Width,
                Height = Math.Max(300, Math.Min(570, SystemParameters.WorkArea.Height - 42)),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            overlay.Children.Add(settingsScroll);
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
            var button = MakeButton(glyph + "    " + label, action, false, 10, 37);
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            button.Padding = new Thickness(11, 4, 8, 4);
            button.Margin = new Thickness(0, 2, 0, 2);
            button.Background = BrushFrom("#16FFFFFF");
            button.BorderBrush = BrushFrom("#20FFFFFF");
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
            var button = MakeButton(string.Empty, action, false, 10, 112);
            button.Width = 158;
            button.Height = 112;
            button.Margin = new Thickness(6);
            button.Padding = new Thickness(12, 10, 12, 9);
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            button.VerticalContentAlignment = VerticalAlignment.Center;
            button.ToolTip = subtitle;

            var stack = new StackPanel();
            var icon = new Border
            {
                Width = 38,
                Height = 38,
                CornerRadius = new CornerRadius(13),
                Background = BrushFrom("#318B7CF6"),
                BorderBrush = BrushFrom("#458B7CF6"),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = glyph,
                    Foreground = AccentBrush(),
                    FontSize = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            stack.Children.Add(icon);
            stack.Children.Add(MakeText(title, 10, "Text", FontWeights.SemiBold, new Thickness(0, 8, 0, 0)));
            stack.Children.Add(MakeText(subtitle, 8, "Muted", FontWeights.Normal, new Thickness(0, 2, 0, 0)));
            button.Content = stack;
            return button;
        }

        private Button StartPinned(string glyph, string label, Action action)
        {
            var button = MakeButton(string.Empty, action, false, 9, 70);
            button.Width = 91;
            button.Height = 70;
            button.Margin = new Thickness(4);
            button.Padding = new Thickness(5);
            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            stack.Children.Add(MakeText(glyph, 22, "Accent", FontWeights.Normal));
            stack.Children.Add(MakeText(label, 9, "Text", FontWeights.SemiBold));
            button.Content = stack;
            return button;
        }

        private Button DockButton(string glyph, string label, Action action)
        {
            var button = MakeButton(glyph, action, false, 20, 43, 43);
            button.Width = 43;
            button.Height = 43;
            button.Margin = new Thickness(2, 0, 2, 0);
            button.Padding = new Thickness(2);
            button.Background = Brushes.Transparent;
            button.BorderBrush = Brushes.Transparent;
            button.Foreground = TextBrush();
            button.ToolTip = label;
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
                SnapsToDevicePixels = true,
                Effect = null
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
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                FocusVisualStyle = null,
            };

            ToolTipService.SetShowDuration(button, 4000);
            var template = new ControlTemplate(typeof(Button));
            var frame = new FrameworkElementFactory(typeof(Border));
            frame.Name = "PART_ButtonFrame";
            frame.SetValue(Border.CornerRadiusProperty, new CornerRadius(primary ? 15 : 13));
            frame.SetValue(Border.SnapsToDevicePixelsProperty, true);
            frame.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            frame.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            frame.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, new TemplateBindingExtension(Control.HorizontalContentAlignmentProperty));
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, new TemplateBindingExtension(Control.VerticalContentAlignmentProperty));
            content.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Control.PaddingProperty));
            frame.AppendChild(content);
            template.VisualTree = frame;
            var hover = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(UIElement.OpacityProperty, 0.88, "PART_ButtonFrame"));
            template.Triggers.Add(hover);
            button.Template = template;

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
            var needle = (filter ?? string.Empty).Trim();

            var pinned = _settings.PinnedPaths
                .Select(path => all.FirstOrDefault(app => string.Equals(app.Path, path, StringComparison.OrdinalIgnoreCase)))
                .Where(app => app != null)
                .Where(app => needle.Length == 0
                    || app.Name.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || (app.Category ?? string.Empty).IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .ToList();

            if (pinned.Count > 0)
            {
                _startResults.Children.Add(MakeText("YOUR PINS", 9, "Muted", FontWeights.Bold, new Thickness(3, 10, 0, 7)));
                foreach (var app in pinned) AddAppRow(app, true);
            }

            if (needle.Length == 0)
            {
                var recent = _settings.RecentPaths
                    .Select(path => all.FirstOrDefault(app => string.Equals(app.Path, path, StringComparison.OrdinalIgnoreCase)))
                    .Where(app => app != null)
                    .GroupBy(app => app.Path, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .Take(5)
                    .ToList();

                if (recent.Count > 0)
                {
                    _startResults.Children.Add(MakeText("RECENTLY OPENED", 9, "Muted", FontWeights.Bold, new Thickness(3, 13, 0, 7)));
                    foreach (var app in recent) AddAppRow(app, IsPinned(app));
                }
            }

            _startResults.Children.Add(MakeText("INSTALLED APPLICATIONS", 9, "Muted", FontWeights.Bold, new Thickness(3, 13, 0, 7)));
            var matches = all
                .Where(app => needle.Length == 0
                    || app.Name.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || (app.Category ?? string.Empty).IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .OrderByDescending(app => needle.Length > 0 && app.Name.StartsWith(needle, StringComparison.CurrentCultureIgnoreCase))
                .ThenBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase)
                .Take(50)
                .ToList();

            if (matches.Count == 0)
            {
                _startResults.Children.Add(MakeText(_isLoadingApps ? "Scanning Start Menu shortcuts…" : "No matching apps found.", 11, "Muted", FontWeights.Normal, new Thickness(5, 12, 5, 12)));
                return;
            }

            foreach (var app in matches) AddAppRow(app, IsPinned(app));
        }

        private void AddAppRow(AppEntry app, bool pinned)
        {
            var row = new Grid { Height = 38, Margin = new Thickness(0, 1, 0, 1) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var open = new Button
            {
                Height = 36,
                Padding = new Thickness(10, 4, 10, 4),
                Background = Brushes.Transparent,
                Foreground = TextBrush(),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Cursor = Cursors.Hand,
                FocusVisualStyle = null,
                Content = "◦    " + app.Name + "     ·     " + app.Category,
                ToolTip = "Open " + app.Name
            };
            open.Click += delegate
            {
                _startOverlay.Visibility = Visibility.Collapsed;
                LaunchEntry(app);
            };
            row.Children.Add(open);

            var pin = MakeButton(pinned ? "★" : "＋", delegate { TogglePin(app); }, false, 11, 32, 38);
            pin.ToolTip = pinned ? "Unpin " + app.Name : "Pin " + app.Name;
            pin.Margin = new Thickness(2, 2, 1, 2);
            Grid.SetColumn(pin, 1);
            row.Children.Add(pin);
            _startResults.Children.Add(row);
        }

        private bool IsPinned(AppEntry app)
        {
            return app != null && _settings.PinnedPaths.Any(path => string.Equals(path, app.Path, StringComparison.OrdinalIgnoreCase));
        }

        private void TogglePin(AppEntry app)
        {
            if (app == null || string.IsNullOrWhiteSpace(app.Path)) return;
            var existing = _settings.PinnedPaths.FindIndex(path => string.Equals(path, app.Path, StringComparison.OrdinalIgnoreCase));
            if (existing >= 0)
                _settings.PinnedPaths.RemoveAt(existing);
            else
            {
                if (_settings.PinnedPaths.Count >= 12)
                {
                    MessageBox.Show(this, "AeroOS supports up to 12 custom pins. Remove one before adding another.", "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                _settings.PinnedPaths.Insert(0, app.Path);
            }
            SaveSettings();
            RenderStartResults(_startSearch == null ? string.Empty : _startSearch.Text);
        }

        private void RememberRecent(AppEntry app)
        {
            if (app == null || string.IsNullOrWhiteSpace(app.Path)) return;
            _settings.RecentPaths.RemoveAll(path => string.Equals(path, app.Path, StringComparison.OrdinalIgnoreCase));
            _settings.RecentPaths.Insert(0, app.Path);
            if (_settings.RecentPaths.Count > 8)
                _settings.RecentPaths.RemoveRange(8, _settings.RecentPaths.Count - 8);
            SaveSettings();
            if (_startOverlay != null && _startOverlay.Visibility == Visibility.Visible)
                RenderStartResults(_startSearch == null ? string.Empty : _startSearch.Text);
        }

        private void StartSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var query = _startSearch == null ? string.Empty : _startSearch.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                e.Handled = true;
                return;
            }

            var app = GetMatchingApps(query).FirstOrDefault();
            if (app != null)
            {
                _startOverlay.Visibility = Visibility.Collapsed;
                LaunchEntry(app);
            }
            e.Handled = true;
        }

        private IEnumerable<AppEntry> GetMatchingApps(string filter)
        {
            var all = _builtInApps.Concat(_applications).ToList();
            var needle = (filter ?? string.Empty).Trim();
            return all
                .Where(app => needle.Length == 0
                    || app.Name.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0
                    || (app.Category ?? string.Empty).IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .OrderByDescending(app => needle.Length > 0 && app.Name.StartsWith(needle, StringComparison.CurrentCultureIgnoreCase))
                .ThenBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase);
        }

        private void DesktopSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_desktopSearch == null) return;
            if (string.IsNullOrWhiteSpace(_desktopSearch.Text))
            {
                if (_startOverlay != null && _startOverlay.Visibility == Visibility.Visible && _startSearch != null)
                    _startSearch.Text = string.Empty;
                return;
            }
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
                RememberRecent(app);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Windows could not open " + app.Name + "." + Environment.NewLine + ex.Message, "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool LaunchBuiltin(string key)
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
                    default: return false;
                }
                var builtIn = _builtInApps.FirstOrDefault(app => string.Equals(app.Path, key, StringComparison.Ordinal));
                if (builtIn != null) RememberRecent(builtIn);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Windows could not open that app. It may not be available on this PC." + Environment.NewLine + ex.Message, "AeroOS", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
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
            var now = DateTime.Now;
            if (_clockLabel != null) _clockLabel.Text = now.ToString("h:mm tt");
            if (_dateLabel != null) _dateLabel.Text = now.ToString("ddd, dd MMM yyyy");
            if (_dashboardClockLabel != null) _dashboardClockLabel.Text = now.ToString("h:mm tt");
            if (_dashboardDateLabel != null) _dashboardDateLabel.Text = now.ToString("dddd, dd MMMM yyyy");
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
                stops.Add(new GradientStop(ColorFrom("#E9EFFFFF"), 0.48));
                stops.Add(new GradientStop(ColorFrom("#D7F6FAFF"), 1.00));
            }
            else
            {
                stops.Add(new GradientStop(ColorFrom("#E02B3B59"), 0.00));
                stops.Add(new GradientStop(ColorFrom("#C821304E"), 0.52));
                stops.Add(new GradientStop(ColorFrom("#B51B2943"), 1.00));
            }
            var brush = new LinearGradientBrush(stops, new Point(0, 0), new Point(1, 1));
            brush.Freeze();
            return brush;
        }

        private Brush GlassEdgeBrush()
        {
            var stops = new GradientStopCollection
            {
                new GradientStop(ColorFrom(_settings.Theme == "Light" ? "#DFFFFFFF" : "#A8FFFFFF"), 0.00),
                new GradientStop(ColorFrom(_settings.Theme == "Light" ? "#8295A9C0" : "#526F8DBB"), 0.48),
                new GradientStop(ColorFrom(_settings.Theme == "Light" ? "#B8FFFFFF" : "#5D8B7CF6"), 1.00)
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

            ShellRoot.Children.Add(new System.Windows.Shapes.Ellipse
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