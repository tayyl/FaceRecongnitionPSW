﻿using System;
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
     sprawdzić czy zdjęcie jest niedoświetlone/prześwietlone

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
        bool equalizeHistogramTestChecked;
        public bool EqualizeHistogramTestChecked
        {
            get
            {
                return equalizeHistogramTestChecked;
            }
            set
            {
                equalizeHistogramTestChecked = value;
                NotifyPropertyChanged(nameof(EqualizeHistogramTestChecked));
            }
        }
        bool equalizeHistogramChecked;
        public bool EqualizeHistogramChecked
        {
            get
            {
                return equalizeHistogramChecked;
            }
            set
            {
                equalizeHistogramChecked = value;
                NotifyPropertyChanged(nameof(EqualizeHistogramChecked));
            }
        }
        bool saveRaportToFileChecked;
        public bool SaveRaportToFileChecked
        {
            get
            {
                return saveRaportToFileChecked;
            }
            set
            {
                saveRaportToFileChecked = value;
                NotifyPropertyChanged(nameof(SaveRaportToFileChecked));
            }
        }
        bool eigenRecognizerChecked;
        public bool EigenRecognizerChecked
        {
            get
            {
                return eigenRecognizerChecked;
            }
            set
            {
                eigenRecognizerChecked = value;
                NotifyPropertyChanged(nameof(EigenRecognizerChecked));
            }
        }
        bool fisherRecognizerChecked;
        public bool FisherRecognizerChecked
        {
            get
            {
                return fisherRecognizerChecked;
            }
            set
            {
                fisherRecognizerChecked = value;
                NotifyPropertyChanged(nameof(FisherRecognizerChecked));
            }
        }
        bool lbphRecognizerChecked;
        public bool LBPHRecognizerChecked
        {
            get
            {
                return lbphRecognizerChecked;
            }
            set
            {
                lbphRecognizerChecked = value;
                NotifyPropertyChanged(nameof(LBPHRecognizerChecked));
            }
        }
        bool useAllRecognizersChecked;
        public bool UseAllRecognizersChecked
        {
            get
            {
                return useAllRecognizersChecked;
            }
            set
            {
                useAllRecognizersChecked = value;
                NotifyPropertyChanged(nameof(UseAllRecognizersChecked));
            }
        }
        #endregion
        #region Commands
        ICommand equalizeHistogram;
        public ICommand EqualizeHistogram
        {
            get
            {
                return equalizeHistogram;
            }
        }
        ICommand equalizeHistogramTest;
        public ICommand EqualizeHistogramTest
        {
            get
            {
                return equalizeHistogramTest;
            }
        }
        ICommand eigenRecognizer;
        public ICommand EigenRecognizer
        {
            get
            {
                return eigenRecognizer;
            }
        }
        ICommand fisherRecognizer;
        public ICommand FisherRecognizer
        {
            get
            {
                return fisherRecognizer;
            }
        }
        ICommand lbphRecognizer;
        public ICommand LBPHRecognizer
        {
            get
            {
                return lbphRecognizer;
            }
        }
        ICommand useAllRecognizersTest;
        public ICommand UseAllRecognizersTest
        {
            get
            {
                return useAllRecognizersTest;
            }
        }
        ICommand saveRaportToFile;
        public ICommand SaveRaportToFile
        {
            get
            {
                return saveRaportToFile;
            }
        }
        ICommand saveRecognizerModel;
        public ICommand SaveRecognizerModel
        {
            get
            {
                return saveRecognizerModel;
            }
        }
        ICommand loadRecognizerModel;
        public ICommand LoadRecognizerModel
        {
            get
            {
                return loadRecognizerModel;
            }
        }
        ICommand trainLoadedXML;
        public ICommand TrainLoadedXML
        {
            get
            {
                return trainLoadedXML;
            }
        }
        ICommand loadManyImages;
        public ICommand LoadManyImages
        {
            get
            {
                return loadManyImages;
            }
        }
        ICommand runTestTxt;
        public ICommand RunTestTxt
        {
            get
            {
                return runTestTxt;
            }
        }
        ICommand createTest;
        public ICommand CreateTest
        {
            get
            {
                return createTest;
            }
        }

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
            LBPHRecognizerChecked = true;
            EigenRecognizerChecked = false;
            FisherRecognizerChecked = false;
            SaveRaportToFileChecked = false;
            EqualizeHistogramChecked = faceRecognizer.EqualizeHistogram;
            EqualizeHistogramTestChecked = faceRecognizer.EqualizeHistogramTest;

            equalizeHistogram = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    faceRecognizer.EqualizeHistogram = EqualizeHistogramChecked = !EqualizeHistogramChecked;                     
                }
            };
            equalizeHistogramTest= new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    faceRecognizer.EqualizeHistogramTest = EqualizeHistogramTestChecked = !EqualizeHistogramTestChecked;
                }
            };
            saveRaportToFile = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    SaveRaportToFileChecked = !SaveRaportToFileChecked;
                }
            };

            eigenRecognizer = new SimpleCommand
            {
                CanExecuteDelegate = x => !EigenRecognizerChecked,
                ExecuteDelegate = x => { 
                    faceRecognizer.ChangeRecognizer(FaceRecognizerModel.RecognizerType.Eigen); 
                    EigenRecognizerChecked = true; 
                    FisherRecognizerChecked = LBPHRecognizerChecked = false; 
                     
                }
            };
            fisherRecognizer = new SimpleCommand
            {
                CanExecuteDelegate = x => !FisherRecognizerChecked,
                ExecuteDelegate = x => {
                    faceRecognizer.ChangeRecognizer(FaceRecognizerModel.RecognizerType.Fisher);
                    EigenRecognizerChecked = LBPHRecognizerChecked = false;
                    FisherRecognizerChecked = true;
                  
                }
            };
            lbphRecognizer = new SimpleCommand
            {
                CanExecuteDelegate = x => !LBPHRecognizerChecked,
                ExecuteDelegate = x => {
                    faceRecognizer.ChangeRecognizer(FaceRecognizerModel.RecognizerType.LBPH);
                    EigenRecognizerChecked = FisherRecognizerChecked = false;
                  
                    LBPHRecognizerChecked = true;
                }
            };
            useAllRecognizersTest = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    faceRecognizer.UseAllRecognizersTest = UseAllRecognizersChecked = !UseAllRecognizersChecked;
                 
                 }
            };
            loadManyImages = new SimpleCommand
            {
                CanExecuteDelegate = x => faceRecognizer.XmlFilename != null,
                ExecuteDelegate = x =>
                {
                    System.Windows.Forms.FolderBrowserDialog file = new System.Windows.Forms.FolderBrowserDialog();
                    if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        faceRecognizer.LoadImagesFromDirectory(file.SelectedPath);
                        
                    }
                }
            };
            runTestTxt = new SimpleCommand
            {
                CanExecuteDelegate = x => faceRecognizer.IsTrained,
                ExecuteDelegate = x =>
                {
                    string raport="";
                    OpenFileDialog file = new OpenFileDialog();
                    file.Filter = "Pliki (*.txt)|*.txt";
                    if (file.ShowDialog() == true)
                    {
                        raport=faceRecognizer.RunTestFromTxt(file.FileName);
                        MessageBox.Show(raport);
                    }
                    if (SaveRaportToFileChecked)
                    {
                        SaveFileDialog saveRaport = new SaveFileDialog();
                        saveRaport.Filter = "Pliki (*.txt)|*.txt";
                        if(saveRaport.ShowDialog() == true)
                        {
                            System.IO.File.WriteAllText(saveRaport.FileName, raport);
                        }
                    }
                }
            };
            createTest = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    System.Windows.Forms.FolderBrowserDialog file = new System.Windows.Forms.FolderBrowserDialog();
                    if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        faceRecognizer.CreateTest(file.SelectedPath);

                    }
                }
            };
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
                        faceRecognizer.IsLoaded=faceRecognizer.LoadTrainingData(fileDialog.FileName);
                        if (faceRecognizer.IsLoaded) faceRecognizer.IsTrained = faceRecognizer.TrainLoadedXML();
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
                        faceRecognizer.IsLoaded=faceRecognizer.LoadTrainingData(fileDialog.FileName);
                        if (faceRecognizer.IsLoaded) faceRecognizer.IsTrained = faceRecognizer.TrainLoadedXML();
                    }
                }
            };
            trainLoadedXML = new SimpleCommand
            {
                CanExecuteDelegate=x=> faceRecognizer.IsLoaded,
                ExecuteDelegate = x =>
                {
                    faceRecognizer.IsTrained = faceRecognizer.TrainLoadedXML();
                }
            }; 
            loadRecognizerModel = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Filter = "Pliki (*.yml)|*.yml";
                    if (fileDialog.ShowDialog() == true)
                    {
                        faceRecognizer.LoadRecognizerModel(fileDialog.FileName);
                    }
                }
            };
            saveRecognizerModel = new SimpleCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x =>
                {
                    SaveFileDialog fileDialog = new SaveFileDialog();
                    fileDialog.Filter = "Pliki (*.yml)|*.yml";
                    if (fileDialog.ShowDialog() == true)
                    {
                        faceRecognizer.SaveRecognizerModel(fileDialog.FileName);
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
                            faceRecognizer.IsLoaded = faceRecognizer.LoadTrainingData(faceRecognizer.ImagesSavePath+faceRecognizer.XmlFilename);
                            if (faceRecognizer.IsLoaded) faceRecognizer.IsTrained = faceRecognizer.TrainLoadedXML();
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
                         croppedFaceIndex = 0;
                         

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
