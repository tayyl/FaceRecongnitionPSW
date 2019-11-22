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

        Image<Gray, Byte> grayFrame;
        System.Drawing.Rectangle[] detectedFaces;

        Image<Bgr, Byte> currentFrame;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            capture = new VideoCapture();
            haarCascade = new CascadeClassifier("..\\..\\..\\haarcascade_frontalface_default.xml");
            ComponentDispatcher.ThreadIdle += ProcessFrame;
          
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            currentFrame = capture.QueryFrame().ToImage<Bgr, byte>();

            grayFrame = currentFrame.Convert<Gray, Byte>();
            detectedFaces = haarCascade.DetectMultiScale(grayFrame, 1.2, minSize: new System.Drawing.Size(30, 30));

            Parallel.For(0,detectedFaces.Length, i => {
                //cropping face frame, to draw only face not whole head
                detectedFaces[i].X += (int)(detectedFaces[i].Height * 0.15);
                detectedFaces[i].Y += (int)(detectedFaces[i].Width * 0.22);
                detectedFaces[i].Height -= (int)(detectedFaces[i].Height * 0.3);
                detectedFaces[i].Width -= (int)(detectedFaces[i].Width * 0.35);

                currentFrame.Draw(detectedFaces[i], new Bgr(0, 255, 0), 3);
            });

            imageBox1.Source = ToBitmapSource(currentFrame);

        }
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(Emgu.CV.Image<Bgr, byte> image)
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


    }
}
