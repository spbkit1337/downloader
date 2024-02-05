using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using YoutubeDLSharp; // Подключаем библиотеку YoutubeDLSharp
using YoutubeDLSharp.Options;
using System.IO;
using System.Reflection.Emit;
using YoutubeDLSharp.Metadata;
using System.Reflection;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.NetworkInformation;

namespace downloader
{
    public partial class Form1 : Form
    {

        private string pathSave; //переменная пути сохранения
        private string link; //ссылка на ролик



        public Form1()
        {
            InitializeComponent();



        }

        //Загрузка формы
        private void Form1_Load(object sender, EventArgs e)
        {          
            label4.Text = ""; // путь сохранения
            label6.Text = ""; // Статус
            label7.Text = ""; // Прогресс в %
            label8.Text = ""; //название ролика

        }





        //Путь сохранения видосиков
        private void pictureBox1_Click(object sender, EventArgs e)
        {            
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pathSave = dialog.SelectedPath;
                    label4.Text = pathSave.ToString();
                }

            }
        }


        //Кнопка добавления ссылок на ролики в listview
        private async void  button2_Click(object sender, EventArgs e)
        {
            string link = textBox1.Text;

            if (textBox1.Text != "")
            {


                // создаем новый экземпляр YoutubeDL
                var ytdl = new YoutubeDL();
                var res = await ytdl.RunVideoDataFetch(link);

                // получаем инфу о ролике
                VideoData video = res.Data;
                string title = video.Title;




                // Создаем новый элемент для списка, который будет содержать текст из TextBox1 и title
                var newItem = new ListViewItem(new[] { textBox1.Text, title, "Ожидает скачивания" });

                // Добавляем элемент в ListView
                listView1.Items.Add(newItem);

                textBox1.Text = ""; //обнуляю текст после добавления  
            }
            else
            {
                MessageBox.Show("Ты забыл добавить ссылку", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }



        //Кнопка скачивания из списка listview
        private async void button1_Click(object sender, EventArgs e)
        {
            if (pathSave == null)
            {
                MessageBox.Show("Выберите папку для сохранения видео", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("Добавьте ссылки на видео для скачивания", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Создаем новый экземпляр YoutubeDL
            var ytdl = new YoutubeDL();

            // Путь сохранения
            ytdl.OutputFolder = pathSave;

            // Получаем общее количество роликов для учета прогресса
            int totalVideos = listView1.Items.Count;

            // Очищаем progress bar и устанавливаем значение Text для label7 на 0%
            progressBar1.Value = 0;
            label7.Text = "0%";

            for (int i = 0; i < totalVideos; i++)
            {
                // Обновляем текст в label8 на имя скачиваемого ролика
                label8.Text = listView1.Items[i].SubItems[1].Text;

                // Обновляем текст в label6 перед началом скачивания
                label6.Text = "В процессе";

                // Получаем ссылку на видео из текстового поля элемента списка
                string videoUrl = listView1.Items[i].Text;

                // обработчик прогресса с обратным вызовом, который обновляет панель прогресса
                var progress = new Progress<DownloadProgress>(p =>
                {
                    // Обновляем значение progress bar
                    double overallProgress = (i + (p.Progress * 100)) / totalVideos;
                    progressBar1.Value = (int)overallProgress;

                    // Обновляем значение Text для label7
                    label7.Text = $"{progressBar1.Value}%";
                    // Обновляем значение статуса в колонке columnHeader3
                    listView1.Items[i].SubItems[2].Text = "В процессе";
                });

                string selectedQuality = comboBox1.SelectedItem?.ToString(); // Получаем выбранное качество из comboBox1

                string formatParameter = ""; // Устанавливаем по умолчанию

                // Устанавливаем параметр формата в зависимости от выбранного качества
                if (selectedQuality == "720p")
                {
                    formatParameter = "(bestvideo[width<=720])+bestaudio";
                }
                else if (selectedQuality == "1080p")
                {
                    formatParameter = "(bestvideo[width<=1080])+bestaudio";
                }
                else if (selectedQuality == "480p")
                {
                    formatParameter = "(bestvideo[width<=480])+bestaudio";
                }
                else if (selectedQuality == "1440p")
                {
                    formatParameter = "(bestvideo[width<=1440])+bestaudio";
                }
                // Если не выбрано разрешение то будет скачиваться в наилучшем доступном качестве

                // Начинаем скачивание видео
                var result = await ytdl.RunVideoDownload(videoUrl, formatParameter, progress: progress);

                if (result.Success)
                {
                    // Получаем путь скачанного файла
                    string downloadedFilePath = result.Data;

                    // Обновляем значение статуса в колонке columnHeader3
                    listView1.Items[i].SubItems[2].Text = "Скачан";


                    // Обновляем текст в label6
                    label6.Text = "Готово";
                }
                else
                {
                    // В случае ошибки
                    label6.Text = "Ошибка";
                    MessageBox.Show($"Произошла ошибка при скачивании видео: {result.ErrorOutput}");
                }
            }

            // Обновляем текст в label6 после завершения всех скачиваний
            label6.Text = "Готово";

            label8.Text = "Всё скачалось";
        }





        //Удалить выбранное
        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].Remove();
            }
        }

        //Очистить список
        private void button3_Click(object sender, EventArgs e)
        {
                listView1.Items.Clear(); 
        }

        //Поддерживаемые сайты
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md");
        }
    }
}
