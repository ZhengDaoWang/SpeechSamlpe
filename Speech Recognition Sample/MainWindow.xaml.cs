using System;
using System.Collections.Generic;
using System.IO;
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
using Baidu.Aip.Speech;
using NAudio.Wave;
using Newtonsoft.Json.Linq;

namespace SpeechRecognitionSample
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private string fileName =Environment.CurrentDirectory+ @"\SpeechRecognition";
        private string outputFilePath;
        public MainWindow()
        {
            InitializeComponent();
            InitBaiduApiConfig();
        }


        private void InitBaiduApiConfig()
        {
            this.AsrClient= new Asr(APP_ID, API_KEY, SECRET_KEY);
            this.AsrClient.Timeout = 60000;  // 修改超时时间
        }

        public void AnalysisAsrData()
        {
            if (outputFilePath == null)
            {
                MessageBox.Show("请先录好音频!");
                return;
            }
            var data = File.ReadAllBytes(outputFilePath);

            // 可选参数
            var options = new Dictionary<string, object> { {"dev_pid", 1536} };
            this.AsrClient.Timeout = 120000; // 若语音较长，建议设置更大的超时时间. ms
            var result = this.AsrClient.Recognize(data, "pcm", 16000, options);
           string targetResult= JObjectToString(result);
            this.richTextBoxRun.Text = targetResult;
        }

        private string JObjectToString(JObject jObject)
        {
            var result = jObject["result"].ToString();            
            string[] resultArray = result.Split('[', ']');
            foreach (var item in resultArray)
            {
                if (!string.IsNullOrEmpty(item.Trim()))
                {
                    var itemArray = item.Split('\"');
                    foreach (var lastResult in itemArray)
                    {
                        if (!string.IsNullOrEmpty(lastResult.Trim()))
                        {
                            return lastResult.Trim();
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        private void StartRecord()
        {
            this.WaveSource = new WaveIn();
            this.WaveSource.WaveFormat = new WaveFormat(16000, 16, 1); // 16bit,16KHz,Mono的录音格式
            this.WaveSource.DataAvailable += WaveSource_DataAvailable;
            this.WaveSource.RecordingStopped += WaveSource_RecordingStopped;
            Directory.CreateDirectory(fileName);//创建文件夹
            outputFilePath = System.IO.Path.Combine(fileName, "speechrecognition.wav");
            this.WaveFileWriter = new WaveFileWriter(outputFilePath, this.WaveSource.WaveFormat);
            this.WaveSource.StartRecording();
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        private void StopRecord()
        {
            this.WaveSource.StopRecording();

            if (this.WaveSource != null)
            {
                this.WaveSource.Dispose();
                this.WaveSource = null;
            }

            if (this.WaveFileWriter != null)
            {
                this.WaveFileWriter.Dispose();
                this.WaveFileWriter = null;
            }
        }


        /// <summary>
        /// 录音结束回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (this.WaveSource != null)
            {
                this.WaveSource.Dispose();
                this.WaveSource = null;
            }

            if (this.WaveFileWriter != null)
            {
                this.WaveFileWriter.Dispose();
                this.WaveFileWriter = null;
            }
        }


        /// <summary>
        /// 开始录音回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.WaveFileWriter != null)
            {
                this.WaveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                this.WaveFileWriter.Flush();
            }
        }


        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            StartRecord();
            this.richTextBoxRun.Text = "正在进行录音.......";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopRecord();
            MessageBox.Show("录音完成！");
        }

        private void BtnRecognition_Click(object sender, RoutedEventArgs e)
        {
            AnalysisAsrData();
        }

        #region 属性

        public WaveIn WaveSource { get; set; }

        public WaveFileWriter WaveFileWriter { get; set; }

        public Asr AsrClient { get; set; }

        #endregion

        #region 常量

        const string APP_ID = "你的 App ID";
        const string API_KEY = "你的 Api Key";
        const string SECRET_KEY = "你的 Secret Key";


        #endregion

    }
}
