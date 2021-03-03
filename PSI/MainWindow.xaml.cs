using System;
using System.Collections.Generic;
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
using Microsoft.Win32;

namespace PSI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<LibraryImage> images = new List<LibraryImage>();
            images.Add(new LibraryImage("image_library/fox.bmp","fox.bmp","96x54"));
            images.Add(new LibraryImage("image_library/Test.bmp", "Test.bmp", "20x20"));
            images.Add(new LibraryImage("image_library/smoltriangle.bmp", "smoltriangle.bmp", "10x8"));
            images.Add(new LibraryImage("image_library/w3c_home.bmp", "w3c_home.bmp", "73x48"));
            images.Add(new LibraryImage("image_library/sharp.bmp", "sharp.bmp", "750x500"));
            imageLibraryList.ItemsSource = images;

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

        private void Drop_Handler(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                MainMenu.Visibility = Visibility.Collapsed;
                ImageEditor.Visibility = Visibility.Visible;
                Uri fileUri = new Uri(files[0]);
                MainMenu.Visibility = Visibility.Collapsed;
                ImageEditor.Visibility = Visibility.Visible;
                BitmapImage preview = new BitmapImage();
                preview.BeginInit();
                Console.WriteLine(files[0]);
                preview.UriSource = fileUri;
                preview.EndInit();
                ImagePreview.Source = preview;
                BottomBarLeftLabel.Content = $"Prêt - {files[0].Split('\\').Last()}";
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMP (*.bmp;*.BMP)|*.bmp;*.BMP";
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                MainMenu.Visibility = Visibility.Collapsed;
                ImageEditor.Visibility = Visibility.Visible;
                BitmapImage preview = new BitmapImage();
                preview.BeginInit();
                Console.WriteLine(openFileDialog.FileName);
                preview.UriSource = fileUri;
                preview.EndInit();
                ImagePreview.Source = preview;
                BottomBarLeftLabel.Content = $"Prêt - {openFileDialog.FileName.Split('\\').Last()}";
            }
                
        }
    }
}
