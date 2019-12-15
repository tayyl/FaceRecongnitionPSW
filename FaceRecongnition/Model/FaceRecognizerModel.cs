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
using System.IO;
using System.Drawing.Imaging;
using System.Xml;

namespace FaceRecognition.Model
{
    public class FaceRecognizerModel 
    {
        #region Attributes
        public bool Train { get; set; } = true;
        public Image<Gray, byte> CroppedFace { get; set; } = new Image<Gray, byte>(50, 50);
        #endregion
        #region Variables
        Image<Bgr, Byte> currentFrame; 
        Image<Gray, byte> grayFrame;
        CascadeClassifier Face;
        XmlDocument xml = new XmlDocument();
        
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
                    //cropping image to have only face, not whole head
                    detectedFaces[i].X += (int)(detectedFaces[i].Height * 0.15);
                    detectedFaces[i].Y += (int)(detectedFaces[i].Width * 0.20);
                    detectedFaces[i].Height -= (int)(detectedFaces[i].Height * 0.2);
                    detectedFaces[i].Width -= (int)(detectedFaces[i].Width * 0.35);

                    //draw the red frame around face which has been detected in the 0th (gray) 
                if (Train)
                {
                    CroppedFace = currentFrame.Copy(detectedFaces[i]).Convert<Gray, byte>().Resize(100, 100, Inter.Cubic);
                    CroppedFace._EqualizeHist();
                }
                currentFrame.Draw(detectedFaces[i], new Bgr(0, 0, 255), 2);
            });
            return currentFrame;
        }
        public void SaveTrainingData(System.Drawing.Image face_data, string name,string savePath)
        {
           Random rand = new Random();
           string facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";
         
            while(File.Exists(savePath + facename))
            {
                facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";
            }        

             if (!Directory.Exists(savePath))            
                Directory.CreateDirectory(savePath);
               
                
            face_data.Save(savePath + facename, ImageFormat.Jpeg);

            if (File.Exists(savePath+"TrainedLabels.xml"))
            {
                xml.Load(savePath+"TrainedLabels.xml");

                //Get the root element
                XmlElement root = xml.DocumentElement;

                XmlElement face_D = xml.CreateElement("FACE");
                XmlElement name_D = xml.CreateElement("NAME");
                XmlElement file_D = xml.CreateElement("FILE");

                //Adding name of face and name of file
                name_D.InnerText = name;
                file_D.InnerText = facename;

                //Constructing element for person
                face_D.AppendChild(name_D);
                face_D.AppendChild(file_D);
                //Adding the new person in the end 
                root.AppendChild(face_D);

                //Save the xmlment
                xml.Save(savePath+"TrainedLabels.xml");
            }
            else
            {
                FileStream FS_Face = File.OpenWrite(savePath+"TrainedLabels.xml");
                using (XmlWriter writer = XmlWriter.Create(FS_Face))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Faces_For_Training");

                    writer.WriteStartElement("FACE");
                    writer.WriteElementString("NAME", name);
                    writer.WriteElementString("FILE", facename);
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                FS_Face.Close();
            }               

        }

    }
}
