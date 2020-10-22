using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

namespace IntegrationML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string curFile = "";
        //sets the data path
        ///static readonly string _dataPath = System.IO.Path.Combine
        //(Environment.CurrentDirectory, "Data", "yelp_labelled.txt");
        static string _dataPath;
        public void Analyze(object sender, System.EventArgs e)
        {
            if (curFile == "" || Text.Text == "")
            {
                addAnalysis("Pick a file or create data in the box before running tests");
            }
            else
            {
                Save();
                if (curFile == "")
                {
                    return;
                }
                MLContext mlContext = new MLContext();
                AnalysisBlock.Text = "";
                TrainTestData splitDataView = LoadData(mlContext);
                ITransformer model = BuildAndTrainModel(mlContext, splitDataView.TrainSet);
                //splitDataView.TestSet is splitTestSet
                //Evaluate(mlContext, model, splitDataView.TestSet);
                addAnalysis("=============== Evaluating Model accuracy with Test data===============");
                IDataView predictions = model.Transform(splitDataView.TestSet);
                try
                {
                    CalibratedBinaryClassificationMetrics metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");


                    Console.WriteLine();//display metrics
                    addAnalysis("Model quality metrics evaluation");
                    addAnalysis("--------------------------------");
                    addAnalysis($"Accuracy: {metrics.Accuracy:P2}");
                    addAnalysis($"Auc: {metrics.AreaUnderRocCurve:P2}");//close to 1 is better
                    addAnalysis($"F1Score: {metrics.F1Score:P2}");
                    addAnalysis("== End of model evaluation ==");
                }
                catch (Exception)
                {
                    AnalysisBlock.Text = "There was an error reading this project." + Environment.NewLine
                                        +"Data should be formatted as:"+Environment.NewLine+
                                        "Any text    0 or 1" + Environment.NewLine+Environment.NewLine+
                    "Example: "+Environment.NewLine+ "This restaurant gave me an existential crisis    0";
                }
            }
            //UseModelWithSingleItem(mlContext, model);

            //UseModelWithBatchItems(mlContext, model);

        }

        /*The BuildAndTrainModel() method executes the following tasks:
Extracts and transforms the data.
Trains the model.
Predicts sentiment based on test data.
Returns the model.
         * 
         */
        public static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView splitTrainSet)
        {
            var estimator = mlContext.Transforms.Text.FeaturizeText
                (outputColumnName: "Features", inputColumnName: nameof(SentimentData.SentimentText))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression
                (labelColumnName: "Label", featureColumnName: "Features"));

            Console.WriteLine("== Create and Train the Model ==");
            var model = estimator.Fit(splitTrainSet);
            Console.WriteLine("=============== End of training ===============");
            Console.WriteLine();
            return model;
        }
        /*
        The Evaluate() method executes the following tasks:
Loads the test dataset.
Creates the BinaryClassification evaluator.
Evaluates the model and creates metrics.
Displays the metrics.
        public static void Evaluate(MLContext mlContext, ITransformer model, IDataView splitTestSet)
        {
            addAnalysis("=============== Evaluating Model accuracy with Test data===============");
            IDataView predictions = model.Transform(splitTestSet);
            CalibratedBinaryClassificationMetrics metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");
            Console.WriteLine();//display metrics
            addAnalysis("Model quality metrics evaluation");
            addAnalysis("--------------------------------");
            addAnalysis($"Accuracy: {metrics.Accuracy:P2}");
            addAnalysis($"Auc: {metrics.AreaUnderRocCurve:P2}");//close to 1 is better
            addAnalysis($"F1Score: {metrics.F1Score:P2}");
            addAnalysis("=============== End of model evaluation ===============");
        }*/

        public void addAnalysis(String text)
        {
            text += Environment.NewLine;
            AnalysisBlock.Text += text;
        }

        //predict test data outcome
        /*The UseModelWithSingleItem() method executes the following tasks:
Creates a single comment of test data.
Predicts sentiment based on test data.
Combines test data and predictions for reporting.
Displays the predicted results.*/
        public void predict(object sender, System.EventArgs e)
        {
            if(predictionText.Text == "")
            {
                
                predictionText.Text = "Enter some text here before pressing predict";
                return;
            }
            if(Text.Text == "")
            {
                predictionText.Text = "The project is currently empty.  Put some data in before predicting";
            }
            //can use prediction engine pool for multithread environments
            Save();
            if(curFile == "")
            {
                 
                return;
            }
            MLContext mlContext = new MLContext();
            AnalysisBlock.Text = "";
            TrainTestData splitDataView = LoadData(mlContext);
            ITransformer model = BuildAndTrainModel(mlContext, splitDataView.TrainSet);
            //splitDataView.TestSet is splitTestSet
            //Evaluate(mlContext, model, splitDataView.TestSet);
            addAnalysis("== Evaluating Model accuracy with Test data ==");
            IDataView predictions = model.Transform(splitDataView.TestSet);
            try
            {
                CalibratedBinaryClassificationMetrics metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");

                PredictionEngine<SentimentData, SentimentPrediction> predictionFunction =
                    mlContext.Model.CreatePredictionEngine
                    <SentimentData, SentimentPrediction>(model);

                SentimentData sampleStatement = new SentimentData
                {
                    SentimentText = predictionText.Text
                };

                //predict makes a prediction on a single row of data
                var resultPrediction = predictionFunction.Predict(sampleStatement);

                //display sentiment text and prediction
                //Console.WriteLine();
                addAnalysis("== Prediction Test of model with a single sample and test dataset ==");

                //Console.WriteLine();
                addAnalysis($"Sentiment: {resultPrediction.SentimentText} | Prediction: {(Convert.ToBoolean(resultPrediction.Prediction) ? "Positive" : "Negative")} | Probability: {resultPrediction.Probability} ");

                addAnalysis("== End of Predictions ==");
                //Console.WriteLine(); ;
                correctLabel.Content = "Was this prediction correct?";
                predictionText.Text += " " + (Convert.ToBoolean(resultPrediction.Prediction) ? "1" : "0");
                predictionText.IsReadOnly = true;
                predictButton.Visibility = Visibility.Hidden;
                correctLabel.Visibility = Visibility.Visible;
                yesButton.Visibility = Visibility.Visible;
                noButton.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                AnalysisBlock.Text = "There was an error reading this project." + Environment.NewLine
                                    + "Data should be formatted as:" + Environment.NewLine +
                                    "Any text    0 or 1" + Environment.NewLine + Environment.NewLine +
                "Example: " + Environment.NewLine + "This restaurant gave me an existential crisis    0";
            }
        }

        public void addPrediction(object sender, System.EventArgs e)
        {
            Text.Text += Environment.NewLine+ predictionText.Text;
            Save();
            correctLabel.Content = "Training data added to file";
            predictButton.Visibility = Visibility.Visible;
            yesButton.Visibility = Visibility.Hidden;
            noButton.Visibility = Visibility.Hidden;
            predictionText.IsReadOnly = false;

        }
        //this will still add the data but will reverse the sentiment prediction
        public void addReverse(object sender, System.EventArgs e)
        {
            string predText = predictionText.Text;
            if (predText.EndsWith("1"))
            {
                Console.WriteLine("Flipping to 0");
                predText = predText.Remove(predText.Length - 1, 1) + "0";
            }
            else if (predText.EndsWith("0"))
            {
                Console.WriteLine("Flipping to 1");
                predText = predText.Remove(predText.Length - 1, 1) + "1";
                Console.WriteLine(predText);
            }
            Text.Text += Environment.NewLine + predText;
            Save();
            correctLabel.Content = "Sentiment reversed, training data added to file";
            predictButton.Visibility = Visibility.Visible;
            yesButton.Visibility = Visibility.Hidden;
            noButton.Visibility = Visibility.Hidden;
            predictionText.IsReadOnly = false;
        }


        //same thing as singleItem but for multiple entries
        public static void UseModelWithBatchItems(MLContext mlContext, ITransformer model)
        {
            IEnumerable<SentimentData> sentiments = new[]
            {
                new SentimentData
                {
                SentimentText = "This was a horrible meal"
                },
                new SentimentData
                {
                    SentimentText = "I love this spaghetti."
                }
            };
            //Use the model to predict the comment data sentiment using the Transform() method:
            IDataView batchComments = mlContext.Data.LoadFromEnumerable(sentiments);

            IDataView predictions = model.Transform(batchComments);

            // Use model to predict whether comment data is Positive (1) or Negative (0).
            IEnumerable<SentimentPrediction> predictedResults =
                mlContext.Data.CreateEnumerable<SentimentPrediction>
                (predictions, reuseRowObject: false);

            Console.WriteLine();

            Console.WriteLine("== Prediction Test of loaded model with multiple samples ==");

            foreach (SentimentPrediction prediction in predictedResults)
            {
                Console.WriteLine($"Sentiment: {prediction.SentimentText} | Prediction: {(Convert.ToBoolean(prediction.Prediction) ? "Positive" : "Negative")} | Probability: {prediction.Probability} ");
            }
            Console.WriteLine("== End of predictions ==");
        }

        /*The LoadData() method executes the following tasks:
        Loads the data.
        Splits the loaded dataset into train and test datasets.
        Returns the split train and test datasets.*/
        public static TrainTestData LoadData(MLContext mLContext)
        {
            IDataView dataView = mLContext.Data.LoadFromTextFile
                <SentimentData>(_dataPath, hasHeader: false);
            TrainTestData splitDataView = mLContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            return splitDataView;
        }

        TextDocument doc = new TextDocument();

        //saveas
        public void SaveAsHandler(object sender, System.EventArgs e)
        {
            SaveAs();
        }
        public void SaveAs()
        {
            using (System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog())
            {

                fileDialog.Filter = "txt files (*.txt)|*.txt";
                fileDialog.InitialDirectory = "c://bin/Debug/Data";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    String content = Text.Text;
                    //var filestream = fileDialog.OpenFile();
                    doc.Save(fileDialog.FileName, content);
                    doc.SavedRecently = true;
                    SaveButton.Foreground = Brushes.Gray;
                }
            }
        }

        //save
        public void SaveHandler(object sender, System.EventArgs e)
        {
            Save();
        }
        public void Save()
        {
            if(doc.SavedRecently == true & doc.BeenSaved == false)
            {

            }
            else if (doc.SavedRecently == true)
            {

            }
            else if (doc.BeenSaved == true)
            {
                doc.Save(doc.Name, Text.Text);
                SaveButton.Foreground = Brushes.Gray;
                doc.SavedRecently = true;
            }
            else
            {
                SaveAs();
            }
        }

        //I think new just needs to reset the document, wont need a dialog
        //unless someone forgot to save
        public void NewHandler(object sender, System.EventArgs e)
        {
            if (doc.SavedRecently == false)
            {
                NewSaveAsk.IsOpen = true;
            }
            else
            {
                New();
            }
        }

        private void New()
        {
            Text.Text = "";
            doc.Name = "";
            doc.SavedRecently = false;
            doc.BeenSaved = false;
            doc.Text = "";
            curFile = "New Project";
            ProjectNameLabel.Content = curFile;
        }
        private void NewWithSave()
        {
            Save();
            Text.Text = "";
            doc.Name = "";
            doc.SavedRecently = false;
            doc.BeenSaved = false;
            doc.Text = "";
            curFile = "New Project";
            ProjectNameLabel.Content = curFile;
        }
        

        private void textChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            
            if (doc.SavedRecently == true)
            {
                SaveButton.Foreground = Brushes.Black;
            }
            doc.SavedRecently = false;
        }



        private void OpenHandler(object sender, RoutedEventArgs e)
        {
            if (doc.SavedRecently == false)
            {
                OpenSaveAsk.IsOpen = true;
            }
            else
            {
                Open();
            }

        }

        private void Open()
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {

                openFileDialog.Filter = "txt files (*.txt)|*.txt";
                openFileDialog.InitialDirectory = "c://bin/Debug/Data";

                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    doc.Open(openFileDialog.FileName);
                }
                Text.Text = doc.Text;
                _dataPath = System.IO.Path.Combine
                (Environment.CurrentDirectory, "Data", openFileDialog.FileName);
                curFile = openFileDialog.FileName;
                ProjectNameLabel.Content = curFile;
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void About(object sender, RoutedEventArgs e)
        {
            AboutWindow.IsOpen = true;
        }

        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            AboutWindow.IsOpen = false;
        }

        private void NewSaveAskSave(object sender, RoutedEventArgs e)
        {
            NewSaveAsk.IsOpen = false;
            NewWithSave();
        }

        private void NewSaveAskNoSave(object sender, RoutedEventArgs e)
        {

            NewSaveAsk.IsOpen = false;
            New();
        }

        private void OpenSaveAskSave(object sender, RoutedEventArgs e)
        {
            OpenSaveAsk.IsOpen = false;
            Save();
            Open();
        }

        private void OpenSaveAskNoSave(object sender, RoutedEventArgs e)
        {

            OpenSaveAsk.IsOpen = false;
            Open();
        }
    }
}
