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
using System.Windows.Threading;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;

namespace FaceRecongnition
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture capture;
        private CascadeClassifier haarCascade;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ComponentDispatcher.ThreadIdle += ProcessFrame;
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            capture = new VideoCapture();
            haarCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");       
            ComponentDispatcher.ThreadIdle += ProcessFrame;
            
        }
        void timer_Tick(object sender, EventArgs e)
        {

        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            Image<Bgr, Byte> currentFrame = capture.QueryFrame().ToImage<Bgr, byte>();

            if (currentFrame != null)
            {
                Image<Gray, Byte> grayFrame = currentFrame.Convert<Gray, Byte>();

                System.Drawing.Rectangle[] detectedFaces = haarCascade.DetectMultiScale(grayFrame);

                foreach (var face in detectedFaces)
                    currentFrame.Draw(face, new Bgr(0, double.MaxValue, 0), 3);

                imageBox1.Source = ToBitmapSource(currentFrame);

            }
        }
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        
        public static BitmapSource ToBitmapSource(Emgu.CV.Image<Bgr,byte> image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap  

                BitmapSource bs = System.Windows.Interop
                  .Imaging.CreateBitmapSourceFromHBitmap(
                  ptr,
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap  
                return bs;
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ComponentDispatcher.ThreadIdle -= ProcessFrame;
        }
    }
}
