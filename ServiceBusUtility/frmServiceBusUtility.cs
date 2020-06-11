using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using ServiceBusUtility.Servicebus;
using ServiceBusUtility.Util;

namespace ServiceBusUtility
{
    public partial class frmServiceBusUtility : Form
    {
        public static IConfigurationRoot Configuration { get; set; }

        private int _previousLength = 0;

        private string _endPoint;
        private string _topic;
        private string _labelFilter;
        private string _json;

        public frmServiceBusUtility()
        {
            InitializeComponent();

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            cmbEnvironment.Items.Add("Dev");
            cmbEnvironment.Items.Add("QA");
            cmbEnvironment.Items.Add("Homol");
            
            txtQuantity.Text = "1";
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = $"Start: {DateTime.Now.ToString("HH:mm:ss")}";

            var _stopWatch = new Stopwatch();

            dynamic message = Json.DeserializeObject(rtbJson.Text);

            var sb = new ServiceBus()
            {
                endPoint = txtEndpoint.Text,
                topic = txtTopic.Text,
                labelFilter = txtLabelFilter.Text
            };

            var quantity = Convert.ToInt32(txtQuantity.Text);

            toolStripProgressBar2.Value = 0;
            toolStripProgressBar2.Minimum = 0;
            toolStripProgressBar2.Maximum = quantity;

            int sended = 0;

            _stopWatch.Start();

            var t1 = Task.Factory.StartNew(() =>
                Parallel.For(0, quantity, new ParallelOptions { MaxDegreeOfParallelism = 4 }, count =>
                {
                    sb.SendMessages(message);

                    this.Invoke(new Action(() => sended++));
                    this.Invoke(new Action(() => toolStripStatusLabel10.Text = $"Quantity Processed: {sended}"));
                    this.Invoke(new Action(() => toolStripProgressBar2.Value++));
                })
            );

            Task.Factory.ContinueWhenAll(new[] { t1 }, tasks => {
                _stopWatch.Stop();
                toolStripStatusLabel4.Text = $"Total: {_stopWatch.Elapsed.ToString().Substring(0, 8)}";
                toolStripStatusLabel3.Text = $"End: {DateTime.Now.ToString("HH:mm:ss")}";
            });
        }
        private void cmbEnvironment_TextChanged(object sender, EventArgs e)
        {
            if (cmbEnvironment.SelectedItem != null)
            {
                _endPoint = Configuration.GetSection(cmbEnvironment.SelectedItem.ToString())["EndPoint"];
                _topic = Configuration.GetSection(cmbEnvironment.SelectedItem.ToString())["Topic"];
                _labelFilter = Configuration.GetSection(cmbEnvironment.SelectedItem.ToString())["LabelFilter"];
                _json = Configuration.GetSection(cmbEnvironment.SelectedItem.ToString())["Json"];

                txtEndpoint.Text = _endPoint;
                txtTopic.Text = _topic;
                txtLabelFilter.Text = _labelFilter;
                rtbJson.Text = Json.FormatJson(_json);
            }
        }
        private void rtbJson_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                rtbJson.Text = Json.FormatJson(rtbJson.Text);
            }
        }
    }
}
