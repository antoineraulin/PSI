using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Microsoft.Win32;
using Path = System.IO.Path;

namespace PSI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MyImage currentImage;
        private List<LibraryImage> images;
        public MainWindow()
        {
            InitializeComponent();
             images = new List<LibraryImage>();
            //string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            foreach (String filePath in Directory.GetFiles("../../image_library/", "*.bmp"))
            {
                LibraryImage img = new LibraryImage(filePath);
                images.Add(img);
                MenuItem item = new MenuItem();
                item.Header = img.Name;
                item.Click += new RoutedEventHandler(MenuImageLibraryOnClick);
                menuStock.Items.Add(item);
            }
            imageLibraryList.ItemsSource = images;

        }

        private void MenuImageLibraryOnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                int index = menuStock.Items.IndexOf(item);
                LibraryImage img = images[index];
                String path = Path.Combine(Directory.GetCurrentDirectory(), img.ImageUri);
                LoadImage(path);
            }
        }

        private void ImagesLibraryOnClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                LibraryImage img = (LibraryImage) item.Content;
                String path = Path.Combine(Directory.GetCurrentDirectory(), img.ImageUri);
                LoadImage(path);
            }
        }

        private void Drag_Hover_Handler(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] filenames =
                    e.Data.GetData(DataFormats.FileDrop, true) as string[];

                if (filenames.Length > 1)
                {
                    dropEnabled = false;
                }
                else  if (System.IO.Path.GetExtension(filenames[0]).ToUpperInvariant() != ".bmp" && System.IO.Path.GetExtension(filenames[0]).ToUpperInvariant() != ".BMP")
                {
                    dropEnabled = false;
                }
                

            }
            else
            {
                dropEnabled = false;
            }

            if (!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void LoadImage(string filePath)
        {
            MainMenu.Visibility = Visibility.Collapsed;
            ImageEditor.Visibility = Visibility.Visible;
            Uri fileUri = new Uri(filePath);
            currentImage = new MyImage(filePath);
            histogram.Source = null;
            Thread thread = new Thread(generateHistogram);
            thread.Start();
            MainMenu.Visibility = Visibility.Collapsed;
            ImageEditor.Visibility = Visibility.Visible;
            BitmapImage preview = new BitmapImage();
            preview.BeginInit();
            Console.WriteLine(filePath);
            preview.UriSource = fileUri;
            preview.EndInit();
            ImagePreview.Source = preview;
            BottomBarLeftLabel.Content = $"Prêt - {filePath.Split('\\').Last()}";
        }

        private void Drop_Handler(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadImage(files[0]);
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMP (*.bmp;*.BMP)|*.bmp;*.BMP";
            if (openFileDialog.ShowDialog() == true)
            {
                LoadImage(openFileDialog.FileName);
            }
                
        }

        private void generateHistogram()
        {
            MyImage histo = currentImage.Histogramme();
            histo.From_Image_To_File(@".\histo.bmp");
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Uri fileUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(),@".\histo.bmp"));
                MainMenu.Visibility = Visibility.Collapsed;
                ImageEditor.Visibility = Visibility.Visible;
                BitmapImage preview = new BitmapImage();
                preview.BeginInit();
                preview.CacheOption = BitmapCacheOption.OnLoad;
                preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                preview.UriSource = fileUri;
                preview.EndInit();
                histogram.Source = preview;
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }));
        }

        private void Miroir_OnClick(object sender, RoutedEventArgs e)
        {
            MyImage mirror = currentImage.EffetMiroir();
            mirror.From_Image_To_File(@".\tmp.bmp");
            currentImage = mirror;
            Uri fileUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), @".\tmp.bmp"));
            BitmapImage preview = new BitmapImage();
            preview.BeginInit();
            preview.CacheOption = BitmapCacheOption.OnLoad;
            preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            preview.UriSource = fileUri;
            preview.EndInit();
            ImagePreview.Source = preview;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Rotate_OnClick(object sender, RoutedEventArgs e)
        {
            RotationDialog inputDialog = new RotationDialog();
            if (inputDialog.ShowDialog() == true)
            {
                double angle;
                if (double.TryParse(inputDialog.Answer, out angle))
                {
                    MyImage rotate = currentImage.RotationV2(angle, radian:inputDialog.isRadian);
                    rotate.From_Image_To_File(@".\tmp.bmp");
                    currentImage = rotate;
                    histogram.Source = null;
                    Thread thread = new Thread(generateHistogram);
                    thread.Start();
                    Uri fileUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), @".\tmp.bmp"));
                    BitmapImage preview = new BitmapImage();
                    preview.BeginInit();
                    preview.CacheOption = BitmapCacheOption.OnLoad;
                    preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    preview.UriSource = fileUri;
                    preview.EndInit();
                    ImagePreview.Source = preview;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                
                
            }
                
        }

        private void BW_OnClick(object sender, RoutedEventArgs e)
        {
            MyImage bw = currentImage.ToBW();
            bw.From_Image_To_File(@".\tmp.bmp");
            currentImage = bw;
            histogram.Source = null;
            Thread thread = new Thread(generateHistogram);
            thread.Start();
            Uri fileUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), @".\tmp.bmp"));
            BitmapImage preview = new BitmapImage();
            preview.BeginInit();
            preview.CacheOption = BitmapCacheOption.OnLoad;
            preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            preview.UriSource = fileUri;
            preview.EndInit();
            ImagePreview.Source = preview;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Grayscale_OnClick(object sender, RoutedEventArgs e)
        {
            MyImage grayscale = currentImage.ToGrayscale();
            grayscale.From_Image_To_File(@".\tmp.bmp");
            currentImage = grayscale;
            histogram.Source = null;
            Thread thread = new Thread(generateHistogram);
            thread.Start();
            Uri fileUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), @".\tmp.bmp"));
            BitmapImage preview = new BitmapImage();
            preview.BeginInit();
            preview.CacheOption = BitmapCacheOption.OnLoad;
            preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            preview.UriSource = fileUri;
            preview.EndInit();
            ImagePreview.Source = preview;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

    }
}
