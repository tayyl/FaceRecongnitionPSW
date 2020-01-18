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
using Microsoft.Win32;

namespace FaceRecognition.ViewModel
{
    /*
     Przygotować dane testowe
     określić wydajność i efektywność     
     miary wydajności 
     ilość klatek na sekundę

    czyli przygotować test

     spróbować uzyc pozostalych dwoch recognizerow, aby zwiekszyc celnosc
    */
    public class FaceRecognizerVM  : INotifyPropertyChanged
    {
        #region Variables
        enum Tab { Webcam = 0, File = 1 };
        Tab TabContainer = Tab.File;
        FaceRecognizerModel faceRecognizer = new FaceRecognizerModel("..\\..\\CascadesXML\\haarcascade_frontalface_default.xml");
        VideoCapture videoCapture = new VideoCapture();
        Image<Bgr, byte> fileWithFacesImageTmp= null;
        List<Image<Gray, byte>> croppedFacesTmp = new List<Image<Gray, byte>>();
        List<Image<Gray, byte>> croppedFacesFileTmp = new List<Image<Gray, byte>>();
        int croppedFaceIndex = 0;
        #endregion
        #region Attributes
        Image<Bgr, byte> mainCamera;
        public Image<Bgr,byte> MainCamera
        {
            get
            {
                return mainCamera;
            }
            set
            {
                mainCamera = value;
                NotifyPropertyChanged(nameof(MainCamera));
            }
        }
        Image<Gray, byte> croppedFace;
        public Image<Gray, byte> CroppedFace
        {
            get
            {
                return croppedFace;
            }
            set
            {
                croppedFace = value;
                NotifyPropertyChanged(nameof(CroppedFace));
            }
        }
        Image<Gray, byte> croppedFaceFile;
        public Image<Gray, byte> CroppedFaceFile
        {
            get
            {
                return croppedFaceFile;
            }
            set
            {
                croppedFaceFile = value;
                NotifyPropertyChanged(nameof(CroppedFaceFile));
            }
        }
        string faceName;
        public string FaceName
        {
            get
            {
                return faceName; 
            }
            set
            {
                faceName = value;
                NotifyPropertyChanged(nameof(FaceName));
            }
        }
        string cameraButtonText;
        public string CameraButtonText
        {
            get
            {
                return cameraButtonText;

            }
            set
            {
                cameraButtonText = value;
                NotifyPropertyChanged(nameof(CameraButtonText));
            }
        }
        #endregion
        #region Commands
        ICommand browseFile;
        public ICommand BrowseFile
        {
            get
            {
                return browseFile;
            }
        }
        ICommand addFace;
        public ICommand AddFace
        {
            get
            {
                return addFace;
            }
        }
        ICommand mainSelectorChangedCommand;
        public ICommand MainSelectorChangedCommand
        {
            get
            {
                return mainSelectorChangedCommand;
            }
        }
        ICommand stopStartCamera;
        public ICommand StopStartCamera
        {
            get
            {
                return stopStartCamera;
                
            }
        }
        ICommand openXML;
        public ICommand OpenXML {
            get
            {
                return openXML;
            }
        }
        ICommand createXML;
        public ICommand CreateXML
        {
            get
            {
                return createXML;
            }
        }
        ICommand nextCroppedFace;
        public ICommand NextCroppedFace
        {
            get
            {
                return nextCroppedFace;
            }
        }
        ICommand backCroppedFace;
        public ICommand BackCroppedFace
        {
            get
            {
                return backCroppedFace;
            }
        }
        #endregion
        public FaceRecognizerVM()
        {
            ComponentDispatcher.ThreadIdle += WebcamProcessing;
            mainSelectorChangedCommand = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => {
                    TabContainer = TabContainer==Tab.Webcam ? Tab.File : Tab.Webcam;
                    if (TabContainer == Tab.File)
                    {
                        if (!(fileWithFacesImageTmp == null))
                        {
                            ComponentDispatcher.ThreadIdle -= WebcamProcessing;
                            croppedFacesTmp = faceRecognizer.ProcessFrame(fileWithFacesImageTmp.Copy());
                            CroppedFace = croppedFacesTmp[croppedFaceIndex];
                            MainCamera = faceRecognizer.CurrentFrame;
                            CroppedFaceFile = CroppedFace;
                        }
                    }
                    else
                    {
                        croppedFaceIndex = 0;
                        if(!(fileWithFacesImageTmp==null)) 
                        ComponentDispatcher.ThreadIdle += WebcamProcessing;
                    }
                }
            };
            openXML = new SimpleCommand
            {
                CanExecuteDelegate=x=>true,
                ExecuteDelegate = x =>
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Filter = "Pliki (*.xml)|*.xml";
                    if(fileDialog.ShowDialog()== true)
                    {
                        faceRecognizer.IsTrained=faceRecognizer.LoadTrainingData(fileDialog.FileName);
                    }
                }
            };
            createXML = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    SaveFileDialog fileDialog = new SaveFileDialog();
                    fileDialog.Filter = "Pliki (*.xml)|*.xml";
                    if (fileDialog.ShowDialog() == true)
                    {
                        faceRecognizer.CreateXmlFile(fileDialog.FileName, fileDialog.SafeFileName);
                        faceRecognizer.IsTrained=faceRecognizer.LoadTrainingData(fileDialog.FileName);
                    }
                }
            };
            addFace = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => {
                    if (FaceName != null)
                    {
                        if (faceRecognizer.XmlFilename != null)
                        {
                            faceRecognizer.SaveImage(CroppedFace.ToBitmap(), FaceName);
                            faceRecognizer.IsTrained = faceRecognizer.LoadTrainingData(faceRecognizer.ImagesSavePath+faceRecognizer.XmlFilename);
                            if (TabContainer == Tab.File)
                            {
                                croppedFacesTmp = faceRecognizer.ProcessFrame(fileWithFacesImageTmp.Copy());
                                CroppedFace = croppedFacesTmp[croppedFaceIndex];
                                MainCamera = faceRecognizer.CurrentFrame;
                                CroppedFaceFile = CroppedFace;
                            }
                        }
                        else
                        {
                            createXML.Execute(null);
                            addFace.Execute(null);
                        }
                    }
                }
            };
            browseFile = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    if (fileDialog.ShowDialog() == true)
                    {
                        fileWithFacesImageTmp = new Image<Bgr, byte>(fileDialog.FileName);

                        ComponentDispatcher.ThreadIdle -= WebcamProcessing;

                        croppedFacesTmp = faceRecognizer.ProcessFrame(fileWithFacesImageTmp.Copy());
                        CroppedFace = croppedFacesTmp[croppedFaceIndex];
                        MainCamera = faceRecognizer.CurrentFrame;
                        CroppedFaceFile = CroppedFace;

                    }
                }
            };
            stopStartCamera = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                 {
                     if (CameraButtonText == "STOP CAMERA")
                     {
                         ComponentDispatcher.ThreadIdle -= WebcamProcessing;
                       //  videoCapture.Stop();
                         CameraButtonText = "START CAMERA";
                     }
                     else
                     {
                         // videoCapture.Start();
                         croppedFacesTmp = faceRecognizer.ProcessFrame(videoCapture.QueryFrame().ToImage<Bgr, byte>());
                         if (croppedFacesTmp.Count < croppedFaceIndex) croppedFaceIndex = 0;
                         

                         ComponentDispatcher.ThreadIdle += WebcamProcessing;
                         CameraButtonText = "STOP CAMERA";
                     }
                 }
            };
            nextCroppedFace = new SimpleCommand
            {
                CanExecuteDelegate = x => croppedFaceIndex < faceRecognizer.CroppedFacesCount-1,
                ExecuteDelegate = x => { 
                    croppedFaceIndex++;
                    if (TabContainer == Tab.Webcam)
                        CroppedFace = croppedFacesTmp[croppedFaceIndex];
                    else
                        CroppedFaceFile = croppedFacesTmp[croppedFaceIndex];
                }
            };
            backCroppedFace = new SimpleCommand
            {
                CanExecuteDelegate = x => croppedFaceIndex >0,
                ExecuteDelegate = x => { 
                    croppedFaceIndex--;
                    if (TabContainer == Tab.Webcam)
                        CroppedFace = croppedFacesTmp[croppedFaceIndex];
                    else
                        CroppedFaceFile = croppedFacesTmp[croppedFaceIndex];
                }
            };
            cameraButtonText = "STOP CAMERA";
            croppedFacesTmp = faceRecognizer.ProcessFrame(videoCapture.QueryFrame().ToImage<Bgr, byte>());
            CroppedFace = croppedFacesTmp[croppedFaceIndex];
            MainCamera = faceRecognizer.CurrentFrame;
            croppedFaceFile = new Image<Gray, byte>(50,50);
        }
        void WebcamProcessing(object sender, EventArgs e)
        {
            croppedFacesTmp = faceRecognizer.ProcessFrame(videoCapture.QueryFrame().ToImage<Bgr, byte>());
            CroppedFace = croppedFacesTmp[croppedFaceIndex];
            MainCamera = faceRecognizer.CurrentFrame;
        }
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName = null)
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
