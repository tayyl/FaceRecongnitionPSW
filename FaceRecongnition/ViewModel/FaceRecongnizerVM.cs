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
using FaceRecognition.Model;
using System.ComponentModel;
namespace FaceRecognition.ViewModel
{
    public class FaceRecognizerVM 
    {
        FaceRecognizerModel faceRecognizer = new FaceRecognizerModel();
        public BitmapSource MainCamera
        {
            get
            {
                return ToBitmapSource(faceRecognizer.ProcessedFrame);
            }
        }
       

        public FaceRecognizerVM()
        {
            faceRecognizer.StartCapturing();
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// Convert to ImageBox format
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
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
