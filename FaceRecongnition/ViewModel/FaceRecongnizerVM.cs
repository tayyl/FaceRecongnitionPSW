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
    */
    public class FaceRecognizerVM  : INotifyPropertyChanged
    {
        #region Variables
        FaceRecognizerModel faceRecognizer = new FaceRecognizerModel("..\\..\\CascadesXML\\haarcascade_frontalface_default.xml", "..\\..\\TrainedFaces\\");
        VideoCapture videoCapture = new VideoCapture();
        OpenFileDialog fileDialog = new OpenFileDialog();
        bool isWebcamDispatcher = true;
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
        string labelUnderCamera;
        public string LabelUnderCamera
        {
            get
            {
                return labelUnderCamera;
            }
            set
            {
                labelUnderCamera = value;
                NotifyPropertyChanged(nameof(LabelUnderCamera));
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
        #endregion
        public FaceRecognizerVM()
        {
            mainSelectorChangedCommand = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => {
                    faceRecognizer.Train = faceRecognizer.Train ? false : true;
                    if (!isWebcamDispatcher)
                        ComponentDispatcher.ThreadIdle += WebcamProcessing;
                }
            };
            addFace = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => {
                    if (FaceName != null)
                    {
                        faceRecognizer.SaveTrainingData(CroppedFace.ToBitmap(), FaceName, "..\\..\\TrainedFaces\\");
                        faceRecognizer.IsTrained=faceRecognizer.LoadTrainingData("..\\..\\TrainedFaces\\");
                    }
                }
            };
            browseFile = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    fileDialog.ShowDialog();
                    ComponentDispatcher.ThreadIdle -= WebcamProcessing;
                    MainCamera = new Image<Bgr, byte>(fileDialog.FileName);
                    isWebcamDispatcher = false;
                }
            };
            MainCamera = faceRecognizer.ProcessFrame(videoCapture.QueryFrame().ToImage<Bgr, byte>());
            CroppedFace = faceRecognizer.CroppedFace;
            //puting into thread for better performance
            ComponentDispatcher.ThreadIdle += WebcamProcessing;
        }
        void WebcamProcessing(object sender, EventArgs e)
        {
            MainCamera = faceRecognizer.ProcessFrame(videoCapture.QueryFrame().ToImage<Bgr, byte>());
            CroppedFace = faceRecognizer.CroppedFace;
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
