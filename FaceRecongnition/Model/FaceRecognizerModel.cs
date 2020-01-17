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
        
        public bool IsTrained { get; set; } = false;
        public string XmlFilename 
        { 
            get 
            {
                return xmlFilename;
            }
        }
        public string ImagesSavePath
        {
            get
            {
                return imagesSavePath;
            }
        }
        public Image<Gray, byte> CroppedFace { get; set; } = new Image<Gray, byte>(50, 50);
        #endregion
        #region Variables
        Image<Bgr, Byte> currentFrame; 
        Image<Gray, byte> grayFrame;
        CascadeClassifier Face;
        XmlDocument xml = new XmlDocument();
        string xmlFilename=null;
        string imagesSavePath = null;
        List<string> namesList = new List<string>();
        List<int> namesIdList = new List<int>();
        //List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<Mat> trainingImages = new List<Mat>();
        string recognizerType = "EMGU.CV.EigenFaceRecognizer";
        FaceRecognizer recognizer;
        string personLabel;
        int eigenThreshold = 2000;
        float eigenDistance = 0;


        #endregion

        public FaceRecognizerModel(string cascadeCalssifierPath)
        {
            Face= new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + cascadeCalssifierPath);
            recognizer= new EigenFaceRecognizer(80, double.PositiveInfinity);
        }
        public FaceRecognizerModel(string cascadeCalssifierPath, string loadPath)
        {
            Face = new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + cascadeCalssifierPath);
            recognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
            IsTrained = LoadTrainingData(loadPath);
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

               
                
                if (IsTrained)
                {
                    CroppedFace = currentFrame.Copy(detectedFaces[i]).Convert<Gray, byte>().Resize(100, 100, Inter.Cubic);
                    CroppedFace._EqualizeHist();
                    string name = Recognize(CroppedFace);
                    int matchValue = (int)eigenDistance;

                    //draw label for every face
                    currentFrame.Draw(name, new System.Drawing.Point(detectedFaces[i].X - 2, detectedFaces[i].Y - 2), FontFace.HersheyComplex, 1, new Bgr(0, 255, 0));
                }
                else
                {
                    CroppedFace = currentFrame.Copy(detectedFaces[i]).Convert<Gray, byte>().Resize(100, 100, Inter.Cubic);
                    CroppedFace._EqualizeHist();
                }
                currentFrame.Draw(detectedFaces[i], new Bgr(0, 0, 255), 2);
            });
            return currentFrame;
        }
        public void SaveImage(System.Drawing.Image face_data, string name)
        {
            if (xmlFilename != null)
            {
                if (File.Exists(imagesSavePath + xmlFilename))
                {
                    Random rand = new Random();
                    string facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";

                    while (File.Exists(imagesSavePath + facename))
                    {
                        facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";
                    }

                    if (!Directory.Exists(imagesSavePath))
                        Directory.CreateDirectory(imagesSavePath);


                    face_data.Save(imagesSavePath + facename, ImageFormat.Jpeg);

                    xml.Load(imagesSavePath + xmlFilename);

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
                    xml.Save(imagesSavePath + xmlFilename);
                }
            }
        }
        public bool CreateXmlFile(string savePath, string filename)
        {
                xmlFilename = filename;
                this.imagesSavePath = savePath.Substring(0, savePath.Count() - xmlFilename.Count()); ;
                FileStream FS_Face = File.OpenWrite(savePath);
                using (XmlWriter writer = XmlWriter.Create(FS_Face))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Faces_For_Training");


                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                FS_Face.Close();

                return true;
            
        }
        public bool LoadTrainingData(string loadPath)
        {
            if (File.Exists(loadPath))
            {
                xmlFilename = loadPath.Split('\\')[loadPath.Split('\\').Count()-1];
                imagesSavePath = loadPath.Substring(0, loadPath.Count() - xmlFilename.Count());
                //clearing data 
                namesList.Clear();
                namesIdList.Clear();
                trainingImages.Clear();

                FileStream filestream = File.OpenRead(loadPath);
                byte[] xmlBytes = new byte[filestream.Length];
                filestream.Read(xmlBytes, 0, (int)filestream.Length);
                filestream.Close();

                MemoryStream xmlStream = new MemoryStream(xmlBytes);

                using (XmlReader xmlreader = XmlTextReader.Create(xmlStream))
                {
                    while (xmlreader.Read())
                    {
                        if (xmlreader.IsStartElement())
                        {
                            switch (xmlreader.Name)
                            {
                                case "NAME":
                                    if (xmlreader.Read())
                                    {
                                        namesIdList.Add(namesList.Count); //0, 1, 2, 3....
                                        namesList.Add(xmlreader.Value.Trim());
                                    }
                                    break;
                                case "FILE":
                                    if (xmlreader.Read())
                                    {
                                        if (File.Exists(imagesSavePath + xmlreader.Value.Trim()))
                                        {
                                            trainingImages.Add(new Mat(imagesSavePath + xmlreader.Value.Trim(), ImreadModes.Grayscale));
                                        }
                                        else
                                        {
                                            namesIdList.Remove(namesIdList.Last());
                                            namesList.Remove(namesList.Last());
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (trainingImages.ToArray().Length != 0)
                {
                    switch (recognizerType)
                    {
                        case ("EMGU.CV.LBPHFaceRecognizer"):
                            recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 100);
                            break;
                        case ("EMGU.CV.FisherFaceRecognizer"):
                            recognizer = new FisherFaceRecognizer(0, 3500);
                            break;
                        case ("EMGU.CV.EigenFaceRecognizer"):
                        default:
                            recognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
                            break;
                    }
                    recognizer.Train(trainingImages.ToArray(), namesIdList.ToArray());
                    return true;

                }
                return false;
            }
            return false;
        }
        public string Recognize(IInputArray inputImage, int eigenThresh = -1)
        {
            FaceRecognizer.PredictionResult predictionResult = recognizer.Predict(inputImage);

            if(predictionResult.Label== -1)
            {
                personLabel = "Unknown";
                eigenDistance = 0;
                return personLabel;
            }
            else
            {
                personLabel = namesList[predictionResult.Label];
                eigenDistance = (float)predictionResult.Distance;
                if (eigenThresh > -1) eigenThreshold = eigenThresh;

                
                if (recognizerType== "EMGU.CV.EigenFaceRecognizer")
                    if (eigenDistance > eigenThreshold) return personLabel;
                    else return "Unknown";
                else
                    return personLabel; // only eigenRecognizer uses eigendistance and threshold
                

            }
        }
    }
}
