using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.System.Diagnostics;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionTest
{
    public sealed partial class MainPage : Page
    {
        ContainerVisual visual;
        Compositor compositor;
        string drawTime;
        int targetDrawCount = 0;

        static Random rnd = new Random();
        static int size = 6;
        static int columnGrid = 100;
        Vector2 sizeVector = new Vector2(size);

        public MainPage()
        {
            this.InitializeComponent();

            Init();
        }

        private void Init()
        {
            ObjectInput.Text = 100.ToString();
            GridSizeInput.Text = 6.ToString();
            ProcessMemoryUsageReport memoryUsageLaunch = ProcessDiagnosticInfo.GetForCurrentProcess().MemoryUsage.GetReport();
            MemoryUsage.Text = "Memory usage for draw (working set): " + (memoryUsageLaunch.WorkingSetSizeInBytes / 1024).ToString() + "kb";
            RenderedText.Text = "Objects rendered: 0/10000";

            compositor = Window.Current.Compositor;
            visual = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(DrawArea, visual);
        }

        private async void Draw_Click(object sender, RoutedEventArgs e)
        {
            VisualStatus.Text = "Creating visuals...";
            PercentRenderedBar.Value = 0;
            targetDrawCount = columnGrid * columnGrid;
            RenderedText.Text = $"Objects rendered: 0/{targetDrawCount}";

            await Task.Run(() =>
            {
                visual.Children.RemoveAll();

                Debug.WriteLine("Creating visuals...");
                Stopwatch drawTimeBench = Stopwatch.StartNew();

                for (int intT = 0; intT < columnGrid; intT++)
                {
                    for (int incR = 0; incR < columnGrid; incR++)
                    {
                        SpriteVisual b = compositor.CreateSpriteVisual();
                        b.Size = sizeVector;
                        b.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xff, Convert.ToByte(rnd.Next(0, 256)), Convert.ToByte(rnd.Next(0, 256)), Convert.ToByte(rnd.Next(0, 256))));
                        b.Offset = new Vector3(intT * size, incR * size, 0);
                        visual.Children.InsertAtBottom(b);
                    }
                }

                drawTimeBench.Stop();

                Debug.WriteLine("Visuals created in: " + drawTimeBench.Elapsed.ToString("mm\\:ss\\.fff"));
                drawTime = drawTimeBench.Elapsed.ToString("mm\\:ss\\.fff");
            });
            ProcessMemoryUsageReport memoryUsageRun = ProcessDiagnosticInfo.GetForCurrentProcess().MemoryUsage.GetReport();
            MemoryUsage.Text = "Memory usage for draw (working set): " + (memoryUsageRun.WorkingSetSizeInBytes / 1024).ToString() + "kb";
            VisualStatus.Text = "Visuals created in: " + drawTime;
            RenderedText.Text = $"Objects rendered: {targetDrawCount}/{targetDrawCount}";
            PercentRenderedBar.Value = 100;
        }

        private void ObjectInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                columnGrid = Convert.ToInt32(ObjectInput.Text);
                targetDrawCount = columnGrid * columnGrid;
                RenderedText.Text = $"Objects rendered: 0/{targetDrawCount}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void GridSizeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                size = Convert.ToInt32(GridSizeInput.Text);
                sizeVector = new Vector2(size);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void ClearVisuals_Click(object sender, RoutedEventArgs e)
        {
            Draw.IsEnabled = false;
            visual.Children.RemoveAll();
            PercentRenderedBar.Value = 0;
            VisualStatus.Text = "Ready";
            RenderedText.Text = $"Objects rendered: 0/{targetDrawCount}";
            Draw.IsEnabled = true;
        }
    }
}
