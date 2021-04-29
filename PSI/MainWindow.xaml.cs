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

        private List<String> history = new List<string>();
        private MyImage currentImage;
        private List<LibraryImage> images;
        private String currentFilePath;
        public MainWindow()
        {
            InitializeComponent();
            /*images = new List<LibraryImage>();
            //string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            foreach (String filePath in Directory.GetFiles("..\\..\\image_library\\", "*.bmp"))
            {
                LibraryImage img = new LibraryImage(filePath);
                images.Add(img);
                MenuItem item = new MenuItem();
                item.Header = img.Name;
                item.Click += new RoutedEventHandler(MenuImageLibraryOnClick);
                menuStock.Items.Add(item);
            }
            imageLibraryList.ItemsSource = images;

            this.titlebar.MouseLeftButtonDown +=
                new MouseButtonEventHandler(title_MouseLeftButtonDown);*/

            MyImage.GenerateQRCode("PROJET PSI ESILV");
            Environment.Exit(0);
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
            currentFilePath = filePath;
            SetBottomBarState(ready:true);
            //BottomBarLeftLabel.Content = $"Prêt - {filePath.Split('\\').Last()} - {currentImage.height}x{currentImage.width}";
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
            try
            {
                MyImage histo = currentImage.Histogramme();
                histo.From_Image_To_File(@".\histo.bmp");
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Uri fileUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), @".\histo.bmp"));
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
            catch (Exception eee)
            {

            }
        }

        private void Miroir_OnClick(object sender, RoutedEventArgs e)
        {
            SetBottomBarState(ready: false, action:"Miroir vertical...");
            UpdateHistory();
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
            SetBottomBarState(ready: true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Rotate_OnClick(object sender, RoutedEventArgs e)
        {
            RotationDialog inputDialog = new RotationDialog();
            if (inputDialog.ShowDialog() == true)
            {
                if (double.TryParse(inputDialog.Answer, out double angle))
                {
                    RunMyImageOperation("Rotation", () => currentImage.RotationV2(angle, radian: inputDialog.isRadian));
                }
                
                
            }
                
        }

        private void Scale_OnClick(object sender, RoutedEventArgs e)
        {
            ScaleDialog inputDialog = new ScaleDialog();
            if (inputDialog.ShowDialog() == true)
            {
                if (int.TryParse(inputDialog.Height, out int height) && int.TryParse(inputDialog.Width, out int width))
                {
                    RunMyImageOperation("Redimensionner", () => currentImage.ResizeAntialiasing(width, height));
                }
            }

        }

        private void BW_OnClick(object sender, RoutedEventArgs e)
        {
            RunMyImageOperation("Noir et Blanc", currentImage.ToBW);
        }

        private void Grayscale_OnClick(object sender, RoutedEventArgs e)
        {
            RunMyImageOperation("Nuance de gris", currentImage.ToGrayscale);
        }

        private void Blur_OnClick(object sender, RoutedEventArgs e)
        {
            double[,] kernel = currentImage.MatriceKernel(2);
            RunMyImageOperation("Flou Gaussien", () => currentImage.calculateKernel(kernel));
        }

        private void Edge_OnClick(object sender, RoutedEventArgs e)
        {
            double[,] kernelx =
            {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            };
            double[,] kernely = {
                {-1, -2, -1},
                {0, 0, 0},
                { 1, 2, 1}
            };
            RunMyImageOperation("Détection des contours", () => currentImage.calculateKernel(kernelx, kernely));
        }

        private void Sharp_OnClick(object sender, RoutedEventArgs e)
        {
            double[,] kernel = currentImage.MatriceKernel(3);
            RunMyImageOperation("Renforcement des bords", () => currentImage.calculateKernel(kernel));
        }

        private void Stegano_Encode_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMP (*.bmp;*.BMP)|*.bmp;*.BMP";
            openFileDialog.Title = "Dissimuler une image dans l'image actuelle";
            if (openFileDialog.ShowDialog() == true)
            {
                MyImage toHide = new MyImage(openFileDialog.FileName);
                RunMyImageOperation("Stéganographie - Encode", () => currentImage.SteganographyEncode(toHide));
            }
            
        }

        private void Stegano_Decode_OnClick(object sender, RoutedEventArgs e)
        {
            RunMyImageOperation("Stéganographie - Decode", () => currentImage.SteganographyDecode());


        }

        private void Fractal_OnClick(object sender, RoutedEventArgs e)
        {

            FractalDialog inputDialog = new FractalDialog();
            currentFilePath = "./fractale.bmp";
            if (inputDialog.ShowDialog() == true)
            {
                MainMenu.Visibility = Visibility.Collapsed;
                ImageEditor.Visibility = Visibility.Visible;
                if (int.TryParse(inputDialog.Height, out int height) &&
                    int.TryParse(inputDialog.Width, out int width) &&
                    int.TryParse(inputDialog.Iterations, out int iterations))
                {
                    if(inputDialog.isMandelbrot) RunMyImageOperation("Génération fractale", () => MyImage.MandelBrot(iterations, width, height));
                    else RunMyImageOperation("Génération fractale", () => MyImage.Julia(iterations, width, height));
                }
                    
            }

        }

        private void Undo_OnClick(object sender, RoutedEventArgs e)
        {
            CtrlZ();
        }

        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void CommandBinding_Executed_2(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        private void title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void UpdateHistory()
        {
            int historySize = history.Count;
            if (historySize == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    File.Move($".\\history{i}.bmp", $".\\history{i-1}.bmp");
                }
                File.Delete($".\\history-1.bmp");
                history.Remove($".\\history3.bmp");
            }
            historySize = history.Count;
            currentImage.From_Image_To_File($".\\history{historySize}.bmp");
            history.Add($".\\history{historySize}.bmp");
            historySize = history.Count;
            Console.WriteLine(historySize);
            undo.IsEnabled = history.Count != 0;
            BitmapImage preview = new BitmapImage();
            preview.BeginInit();
            preview.CacheOption = BitmapCacheOption.OnLoad;
            preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            preview.UriSource = new Uri($"../../assets/undo{historySize}.png", UriKind.Relative);
            preview.EndInit();
            undoImage.Source = preview;
        }

        private void CtrlZ()
        {
            int historySize = history.Count;
            if (history.Count == 0) return;
            Uri fileUri = new Uri(history[historySize - 1], UriKind.Relative);
            BitmapImage preview = new BitmapImage();
            preview.BeginInit();
            preview.CacheOption = BitmapCacheOption.OnLoad;
            preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            preview.UriSource = fileUri;
            preview.EndInit();
            ImagePreview.Source = preview;
            currentImage = new MyImage(history[historySize - 1]);
            File.Delete($".\\history{historySize-1}.bmp");
            history.Remove($".\\history{historySize-1}.bmp");
            undo.IsEnabled = history.Count != 0;
            preview = new BitmapImage();
            preview.BeginInit();
            preview.CacheOption = BitmapCacheOption.OnLoad;
            preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            preview.UriSource = new Uri($"../../assets/undo{history.Count}.png", UriKind.Relative);
            preview.EndInit();
            undoImage.Source = preview;
            SetBottomBarState(ready:true);
        }



        private void SetBottomBarState(bool ready, String action= "Prêt")
        {
            
                BottomBar.Background = ready ? Brushes.DodgerBlue : Brushes.OrangeRed;
                BottomBarLeftLabel.Content =
                    $"{action} - {currentFilePath.Split('\\').Last()} - {currentImage.height}x{currentImage.width}";
            
        }

        private void RunMyImageOperation(String operationTitle, Func<MyImage> operation)
        {
            // On met a jour la barre du bas avec une couleur orange et un texte qui indique qu'une opération sur l'image est en cours.
            SetBottomBarState(ready: false, action: $"{operationTitle}...");
            // Sauvegarde de l'état actuel dans l'historique pour pouvoir revenir en arrière ultérieurement.
            UpdateHistory();
            // Pour ne pas empêcher la manipulation de l'UI pendant le calcul on réalise l'opération sur un thread à part.
            Thread backgroundThread = new Thread(() =>
            {
                MyImage tmp = operation();
                tmp.From_Image_To_File(@".\tmp.bmp");
                currentImage = tmp;
                this.Dispatcher.BeginInvoke(new Action(() => { histogram.Source = null; }));
                Thread thread = new Thread(generateHistogram);
                thread.Start();
                Uri fileUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), @".\tmp.bmp"));

                this.Dispatcher.BeginInvoke(new Action(() => {
                    BitmapImage preview = new BitmapImage();
                    preview.BeginInit();
                    preview.CacheOption = BitmapCacheOption.OnLoad;
                    preview.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    preview.UriSource = fileUri;
                    preview.EndInit();
                    ImagePreview.Source = preview;
                    SetBottomBarState(ready: true);
                }));

                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
            backgroundThread.Start();
        }
    }
}
