using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FaceRecognition.Converters
{
    public class ToBitmapSourceConverter : IValueConverter
    {
       // [DllImport("gdi32")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Emgu.CV.Image<Bgr, byte> myValue=(Emgu.CV.Image<Bgr, byte>)value;
            using (System.Drawing.Bitmap source = myValue.Bitmap)
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

        private void DeleteObject(IntPtr ptr)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
