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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;

using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using System.ComponentModel;

namespace FaceRecognition.Model
{
    public class FaceRecognizerModel 
    {
       
        #region Variables
        Image<Bgr, Byte> currentFrame; 
        Image<Gray, byte> grayFrame;
        CascadeClassifier Face;
        
        #endregion

        public FaceRecognizerModel(string cascadeCalssifierPath)
        {
            Face= new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + cascadeCalssifierPath);
        }
       
        public Image<Bgr,byte> ProcessFrame(Image<Bgr, byte> videoCapture)
        {
            currentFrame = videoCapture;
            currentFrame.Resize(320, 240, Inter.Cubic); //resizing so can work with smaller picture for better performance

            grayFrame = currentFrame.Convert<Gray, Byte>();
            System.Drawing.Rectangle[] detectedFaces = Face.DetectMultiScale(grayFrame, 1.2, 10, new System.Drawing.Size(50, 50), System.Drawing.Size.Empty);//found parameters for good performance and quality
            //parallel.For = better performance; processing every face in thread
            Parallel.For(0, detectedFaces.Length, i =>
            {
                try
                {
                    //cropping image to have only face, not whole head
                    detectedFaces[i].X += (int)(detectedFaces[i].Height * 0.15);
                    detectedFaces[i].Y += (int)(detectedFaces[i].Width * 0.20);
                    detectedFaces[i].Height -= (int)(detectedFaces[i].Height * 0.2);
                    detectedFaces[i].Width -= (int)(detectedFaces[i].Width * 0.35);

                    //draw the red frame around face which has been detected in the 0th (gray) 
                    currentFrame.Draw(detectedFaces[i], new Bgr(0,0,255), 2);

                }
                catch
                {
                    //if got some error just skip it
                }
            });
            return currentFrame;
        }

    }
}
