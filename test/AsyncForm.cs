//using System;
//using System.Drawing;
//using System.Net.Http;
//using System.Threading;
//using System.Windows.Forms;

//namespace test
//{
//    class AsyncForm : Form
//    {
//        Label label;
//        Button button;

//        public AsyncForm()
//        {
//            label = new Label {Location = new Point(10,20),Text = "Length"};
//            button = new Button {Location = new Point(10, 50), Text = "Click"};
//            button.Click += DisplayWebSiteLength;
//            AutoSize = true;
//            Controls.Add(label);
//            Controls.Add(button);

//        }

//        async void DisplayWebSiteLength(object sender, EventArgs e)
//        {
//            label.Text = "Fetching...";
//            using (HttpClient client = new HttpClient())
//            {
//                var task = client.GetStringAsync("https://www.baidu.com/");
//                label.Text = task.Result.Length.ToString();
//                Thread.Sleep(5000);
//                label.Text = task.Result;
//            }
//        }


//        public void Test()
//        {
//            Application.Run(new AsyncForm());
//        }

//    }
//}
