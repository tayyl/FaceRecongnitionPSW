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
    public class FaceRecognizerVM  : INotifyPropertyChanged
    {
        #region Variables
        FaceRecognizerModel faceRecognizer = new FaceRecognizerModel("..\\..\\CascadesXML\\haarcascade_frontalface_default.xml");
        VideoCapture videoCapture = new VideoCapture();
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
                ExecuteDelegate = x => faceRecognizer.Train = faceRecognizer.Train ? false:true
            };

            addFace = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => { if(FaceName!=null) faceRecognizer.SaveTrainingData(CroppedFace.ToBitmap(), FaceName, "..\\..\\TrainedFaces\\"); }
            };

            MainCamera = faceRecognizer.ProcessFrame(videoCapture.QueryFrame().ToImage<Bgr, byte>());
            CroppedFace = faceRecognizer.CroppedFace;
            //puting into thread for better performance
            ComponentDispatcher.ThreadIdle += (object sender, EventArgs e) => {
                MainCamera = faceRecognizer.ProcessFrame(videoCapture.QueryFrame().ToImage<Bgr, byte>());
                CroppedFace = faceRecognizer.CroppedFace;
            };
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
