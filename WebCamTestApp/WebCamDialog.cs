﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectX.Capture;
using WebCam;

namespace WebCamSample
{
    public partial class WebCamDialog : Form
    {
        WebCam.WebCam m_webCam = new WebCam.WebCam();
        Bitmap m_bmp = null;
        AutoResetEvent m_evtBmpReady = new AutoResetEvent(false);
        long m_lDuration = 0;

        delegate void fnHandleSnap(Bitmap bmp);

        public WebCamDialog()
        {
            InitializeComponent();
            m_webCam.OnSnapshot += m_webCam_OnSnapshot;
        }

        private void m_webCam_OnSnapshot(object sender, ImageArgs e)
        {
            m_bmp = e.Image;
            m_evtBmpReady.Set();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Filter filter in m_webCam.VideoInputDevices)
            {
                ListViewItem lvi = new ListViewItem(filter.Name);
                lvi.Tag = filter;
                lvi.Selected = true;

                listView1.Items.Add(lvi);
            }

            listView1.Items.Add(new ListViewItem("Video File"));
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Filter filter = listView1.SelectedItems[0].Tag as Filter;

            if (filter == null)
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    m_lDuration = m_webCam.Open(filter, pictureBox1, openFileDialog1.FileName);
                    btnStep.Enabled = true;
                    btnPlay.Enabled = true;
                    btnStop.Enabled = true;
                    lblEnd.Visible = true;
                }
                else
                {
                    btnStep.Enabled = false;
                    btnPlay.Enabled = false;
                    btnStop.Enabled = false;
                    lblEnd.Visible = false;
                }
            }
            else
            {
                m_webCam.Open(filter, pictureBox1, null);
                btnStep.Enabled = false;
                btnPlay.Enabled = false;
                btnStop.Enabled = false;
                lblEnd.Visible = false;
            }
        }

        private void timerUI_Tick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                btnConnect.Enabled = !m_webCam.IsConnected;
            else
                btnConnect.Enabled = false;

            btnDisconnect.Enabled = m_webCam.IsConnected;
            btnSnap.Enabled = m_webCam.IsConnected;

            if (m_evtBmpReady.WaitOne(0))
                pictureBox2.Image = m_bmp;

            if (m_webCam.IsAtEnd)
            {
                lblEnd.Visible = true;
                lblEnd.ForeColor = Color.Red;
                lblEnd.Text = "AT END";
            }
            else
            {
                double dfPct = m_webCam.CompletionPercent;
                if (dfPct > 0)
                {
                    lblEnd.Visible = true;
                    lblEnd.ForeColor = Color.Green;
                    lblEnd.Text = dfPct.ToString("P");
                }
                else
                {
                    lblEnd.Visible = false;
                }
            }
        }

        private void btnSnap_Click(object sender, EventArgs e)
        {
            m_webCam.GetImage();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            m_webCam.Close();
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            m_webCam.Step(1);
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            m_webCam.Play();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            m_webCam.Stop();
        }
    }
}
