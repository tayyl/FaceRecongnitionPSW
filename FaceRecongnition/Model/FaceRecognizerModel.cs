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
        public enum RecognizerType { Eigen = 0, Fisher = 1, LBPH = 2 };
        #region Attributes
        public RecognizerType CurrentRecognizer = RecognizerType.LBPH;
        public int CroppedFacesCount { get; private set; } = 0;
        public bool UseAllRecognizersTest { get; set; } = false;
        public bool EqualizeHistogramTest { get; set; } = false;
        public bool EqualizeHistogram { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public bool IsTrained{ get; set; } = false;
        public string XmlFilename { get; private set; } = null;
        public string ImagesSavePath { get; private set; } = null;
        public Image<Bgr, byte> CurrentFrame { get; private set; }
        public Image<Gray, byte> CroppedFace { get; private set; } = new Image<Gray, byte>(50, 50);
        #endregion
        #region Variables         
        Image<Gray, byte> emptyImage=new Image<Gray,byte>(50,50);
        Image<Gray, byte> grayFrame;
        CascadeClassifier Face;
        XmlDocument xml = new XmlDocument();
        List<string> namesList = new List<string>();
        List<int> namesIdList = new List<int>();
        List<Mat> trainingImages = new List<Mat>();
        FaceRecognizer recognizer = null;
        #endregion
        public FaceRecognizerModel(string cascadeCalssifierPath)
        {
            Face= new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + cascadeCalssifierPath);
            recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 400);
        }
        public FaceRecognizerModel(string cascadeCalssifierPath, string loadPath)
        {
            Face = new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + cascadeCalssifierPath);
            IsLoaded = LoadTrainingData(loadPath);
            if (IsLoaded) IsTrained=TrainLoadedXML();

            recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 400);
        }
        public void ChangeRecognizer(RecognizerType type)
        {
            CurrentRecognizer = type;
            switch (type)
            {
                case RecognizerType.Eigen:
                    recognizer = new EigenFaceRecognizer(trainingImages.Count(), double.MaxValue);
                    break;
                case RecognizerType.Fisher:
                    recognizer = new FisherFaceRecognizer(trainingImages.Count(), double.MaxValue);
                    break;
                case RecognizerType.LBPH:
                    recognizer = new LBPHFaceRecognizer(1,8,8,8,400);
                    break;
            }
        }
        public List<Image<Gray,byte>> ProcessFrame(Image<Bgr, byte> videoCapture)
        {
            List<Image<Gray, byte>> CroppedFaces = new List<Image<Gray, byte>>(); 
            CurrentFrame = videoCapture;
            CurrentFrame.Resize(320, 240, Inter.Cubic); //resizing so can work with smaller picture for better performance

            grayFrame = CurrentFrame.Convert<Gray, Byte>();
            System.Drawing.Rectangle[] detectedFaces = Face.DetectMultiScale(grayFrame, 1.2, 10, new System.Drawing.Size(50, 50), System.Drawing.Size.Empty);//found parameters for good performance and quality
            //parallel.For = better performance; processing every face in thread
            Parallel.For(0, detectedFaces.Length, i =>
            {
                    //cropping image to have only face, not whole head
                    detectedFaces[i].X += (int)(detectedFaces[i].Height * 0.15);
                    detectedFaces[i].Y += (int)(detectedFaces[i].Width * 0.20);
                    detectedFaces[i].Height -= (int)(detectedFaces[i].Height * 0.2);
                    detectedFaces[i].Width -= (int)(detectedFaces[i].Width * 0.35);
                Image<Gray, byte> tmp = new Image<Gray, byte>(50, 50);
                tmp = grayFrame.Copy(detectedFaces[i]).Resize(100, 100, Inter.Cubic);

                tmp._EqualizeHist();
                CroppedFaces.Add(tmp);
                if (IsTrained)
                {
                    string name = Recognize(tmp,recognizer);
                    //draw label for every face
                    CurrentFrame.Draw(name, new System.Drawing.Point(detectedFaces[i].X - 2, detectedFaces[i].Y - 2), FontFace.HersheyComplex, 1, new Bgr(0, 255, 0));
                }

                CurrentFrame.Draw(detectedFaces[i], new Bgr(0, 0, 255), 2);
            });
            if (CroppedFaces.Count == 0)
            {
                CroppedFaces.Add(emptyImage);
            }
            CroppedFacesCount = CroppedFaces.Count;
            return CroppedFaces;
        }
        Image<Gray, byte> processImage(Image<Gray,byte> img)
        {
            img.Resize(320, 240, Inter.Cubic);
            System.Drawing.Rectangle[] detectedFaces = Face.DetectMultiScale(img, 1.2, 10, new System.Drawing.Size(50, 50), System.Drawing.Size.Empty);//found parameters for good performance and quality

          //  img._EqualizeHist();
            if (detectedFaces.Length == 1)
            {
                detectedFaces[0].X += (int)(detectedFaces[0].Height * 0.15);
                detectedFaces[0].Y += (int)(detectedFaces[0].Width * 0.20);
                detectedFaces[0].Height -= (int)(detectedFaces[0].Height * 0.2);
                detectedFaces[0].Width -= (int)(detectedFaces[0].Width * 0.35);
                return img.Copy(detectedFaces[0]).Resize(100, 100, Inter.Cubic);
                
            }
            else
                return null;

        }
        public void SaveImage(System.Drawing.Image face_data, string name)
        {
            if (XmlFilename != null)
            {
                if (File.Exists(ImagesSavePath + XmlFilename))
                {
                    Random rand = new Random();
                    string facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";

                    while (File.Exists(ImagesSavePath + facename))
                    {
                        facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";
                    }

                    if (!Directory.Exists(ImagesSavePath))
                        Directory.CreateDirectory(ImagesSavePath);


                    face_data.Save(ImagesSavePath + facename, ImageFormat.Jpeg);

                    xml.Load(ImagesSavePath + XmlFilename);

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
                    xml.Save(ImagesSavePath + XmlFilename);

                }
            }
        }
        public bool CreateXmlFile(string savePath, string filename)
        {
                XmlFilename = filename;
                this.ImagesSavePath = savePath.Substring(0, savePath.Count() - XmlFilename.Count()); ;
                FileStream FS_Face = File.OpenWrite(savePath);
                using (XmlWriter writer = XmlWriter.Create(FS_Face))
                {
                    writer.WriteStartDocument();
                    writer.WriteElementString("Faces_For_Training","");

                    
                    //writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                FS_Face.Close();

                return true;
            
        }
        public bool LoadTrainingData(string loadPath)
        {
            if (File.Exists(loadPath))
            {
                XmlFilename = loadPath.Split('\\')[loadPath.Split('\\').Count()-1];
                ImagesSavePath = loadPath.Substring(0, loadPath.Count() - XmlFilename.Count());
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
                                        if (File.Exists(ImagesSavePath + xmlreader.Value.Trim()))
                                        {
                                            trainingImages.Add(new Mat(ImagesSavePath + xmlreader.Value.Trim(), ImreadModes.Grayscale));
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
                return true;
            }
            return false;
        }
        public bool LoadRecognizerModel(string loadPath)
        {
            if (File.Exists(loadPath))
            {
                if (recognizer == null)
                recognizer.Read(loadPath);
                return true;
            }
            return false;
        }
        public void SaveRecognizerModel(string savePath)
        {
            recognizer.Write(savePath);

        }
        public bool TrainLoadedXML()
        {

            if (trainingImages.ToArray().Length != 0)
            {
               
                recognizer.Train(trainingImages.ToArray(), namesIdList.ToArray());
                return true;

            }
            return false;
        }
        public void LoadImagesFromDirectory(string loadPath)
        {
           string[] imagesList = Directory.GetFiles(loadPath);
           foreach (string imagePath in imagesList)
                {
                    Image<Gray, byte> detectedFace = null;
                    try
                    {
                        detectedFace = processImage(new Image<Gray, byte>(imagePath));
                        if (detectedFace != null)
                        {
                            SaveImage(detectedFace.Bitmap, GetFaceLabel(imagePath));
                            detectedFace.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                }
            
            
            IsLoaded = LoadTrainingData(ImagesSavePath+XmlFilename);
        }
        string GetFaceLabel(string filePath)
        {
            string fileName = filePath.Split('\\')[filePath.Split('\\').Count() - 1];
            string[] splittedFileName = fileName.Split('_');
            string label = "";
            for (int i = 0; i < splittedFileName.Count() - 1; i++)
            {
                label += splittedFileName[i];
                if (i + 1 < splittedFileName.Count() - 1)
                    label += "_";
            }
            return label;
        }
        public string RunTestFromTxt(string testPath)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(testPath);
            string line;
            int amountDetectedFaces = 0, allPictures=0;
            int[] recognizedAll= { 0, 0, 0 };
            FaceRecognizer[] testRecognizers = { null, null, null };
            testRecognizers[(int)CurrentRecognizer] = recognizer;
            string outputRaport = "";
            if (UseAllRecognizersTest)
            {
                for(int i=0; i<3; i++)
                {
                    if(testRecognizers[i] == null)
                    {
                        switch (i)
                        {
                            case (int)RecognizerType.Eigen:
                                testRecognizers[i] = new EigenFaceRecognizer(trainingImages.Count, double.MaxValue);
                                break;
                            case (int)RecognizerType.Fisher:
                                testRecognizers[i] = new FisherFaceRecognizer(trainingImages.Count, double.MaxValue);
                                break;
                            case (int)RecognizerType.LBPH:
                                testRecognizers[i] = new FisherFaceRecognizer(trainingImages.Count, double.MaxValue);
                                break;
                        }
                        testRecognizers[i].Train(trainingImages.ToArray(), namesIdList.ToArray());
                    }
                }
            }

            while ((line = file.ReadLine()) != null)
            {
                string[] fileAndLabel=line.Split('\t');
                string recognizedFace;

                Image<Gray,byte> testImageprocessed = processImage( new Image<Gray, byte>(fileAndLabel[0]));

                if (testImageprocessed != null)
                {
                    if(EqualizeHistogramTest)
                        testImageprocessed._EqualizeHist();
                   
                    for(int i=0; i<3; i++)
                     {
                        if (testRecognizers[i] != null) {
                            recognizedFace = Recognize(testImageprocessed, testRecognizers[i]);
                            if(recognizedFace == fileAndLabel[1])
                                 recognizedAll[i]++;
                        }
                    }           
                    amountDetectedFaces++;                    
                    
                    testImageprocessed.Dispose();
                }
                allPictures++;
            }

            file.Close();
            outputRaport +="Tested DataSet: "+ XmlFilename;
            outputRaport += "\nTesting pictures: " + allPictures.ToString();
            outputRaport += "\nPictures with detected faces: " + amountDetectedFaces.ToString();
            outputRaport += "\n\nRecognized faces for: ";
            for(int i=0; i<3; i++)
            {
                switch (i)
                {
                    case (int)RecognizerType.Eigen:
                        outputRaport += "\nEigenFaces: ";
                        break;
                    case (int)RecognizerType.Fisher:
                        outputRaport += "\nFisherFaces: ";
                        break;
                    case (int)RecognizerType.LBPH:
                        outputRaport += "\nEigenFaces: ";
                        break;
                }
                outputRaport += recognizedAll[i] + ", accuracy: " + ((double)recognizedAll[i] / amountDetectedFaces * 100).ToString("0.##") + "%";
            }
            return outputRaport;
        }
        public void CreateTest(string filesPath)
        {
            string[] imagesList = Directory.GetFiles(filesPath);
            string testFileContent = "";
            foreach (string imagePath in imagesList)
            {
                testFileContent += imagePath + "\t" + GetFaceLabel(imagePath) + "\n";    
            }
            string destinationPath = filesPath + "\\TestFile_" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")+".txt";
            System.IO.File.WriteAllText(destinationPath, testFileContent);
        }
        public string Recognize(IInputArray inputImage, FaceRecognizer r)
        {
            FaceRecognizer.PredictionResult prediction = r.Predict(inputImage);

            string label;
          
                if (prediction.Label == -1)
                {
                    label="Unknown";
                }
                else
                {
                    label = namesList[prediction.Label];  
                }
            

            return label;
        }
    }
}
