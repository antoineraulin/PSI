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
using Microsoft.Win32;
using Path = System.IO.Path;

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
            //string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            foreach (String filePath in Directory.GetFiles("../../image_library/", "*.bmp"))
            {
                images.Add(new LibraryImage(filePath));

            }
            imageLibraryList.ItemsSource = images;
        }

        private void ImagesLibraryOnClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
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
    }
}
