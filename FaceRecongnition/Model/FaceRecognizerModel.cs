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
namespace FaceRecognition.Model
{
    public class FaceRecognizerModel
    {
        #region Variables
        Image<Bgr, Byte> currentFrame; 
        Image<Gray, byte> grayFrame = null; 

        VideoCapture videoCapture; 

        public CascadeClassifier Face = new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + "..\\..\\CascadesXML\\haarcascade_frontalface_default.xml");
        #endregion

        #region Properties
        private Image<Bgr, byte> processedFrame;
        public Image<Bgr,byte> ProcessedFrame => processedFrame;
        #endregion

        #region Public Methods
        //maybe change later to give user choice of camera
        /// <summary>
        /// Start capturing video from default webcam
        /// </summary>
        public void StartCapturing()
        {
            videoCapture = new VideoCapture();

            //working in thread to increase performance
           // if (videoCapture != null) videoCapture.Dispose();
        }
        /// <summary>
        /// Parallel processing current frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProcessFrame(object sender, EventArgs e)
        {
            currentFrame = videoCapture.QueryFrame().ToImage<Bgr, byte>();
            currentFrame.Resize(320, 240, Inter.Cubic); //resizing so can work with smaller picture for better performance

            System.Drawing.Rectangle[] detectedFaces = Face.DetectMultiScale(grayFrame, 1.2, 10, new System.Drawing.Size(50, 50), System.Drawing.Size.Empty);//found parameters for good performance and quality
            //parallel.For = better performance; processing every face in thread
            Parallel.For(0, detectedFaces.Length, i =>
            {
                try
                {
                    //cropping image to have only face, not whole head
                    detectedFaces[i].X += (int)(detectedFaces[i].Height * 0.15);
                    detectedFaces[i].Y += (int)(detectedFaces[i].Width * 0.22);
                    detectedFaces[i].Height -= (int)(detectedFaces[i].Height * 0.3);
                    detectedFaces[i].Width -= (int)(detectedFaces[i].Width * 0.35);

                    //draw the red frame around face which has been detected in the 0th (gray) 
                    currentFrame.Draw(detectedFaces[i], new Bgr(255,0,0), 2);

                }
                catch
                {
                    //if got some error just skip it
                }
            });
            processedFrame = currentFrame;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
