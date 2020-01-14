using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using libzkfpcsharp;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Sample;
using System.Data.SqlClient;
using System.Speech.Synthesis;
using Config;
//using SpeechLib;

namespace Demo
{
    public partial class Form2 : Form
    {
        int index = 0;
        string codings = "";
        string code = "";

        IntPtr mDevHandle = IntPtr.Zero;
        IntPtr mDBHandle = IntPtr.Zero;
        IntPtr FormHandle = IntPtr.Zero;

        bool bIsTimeToDie = false;
        byte[][] RegTmps = new byte[3][];
        byte[] FPBuffer;
        byte[] CapTmp = new byte[2048];
        byte[] RegTmp = new byte[2048];
        int cbRegTmp = 0;
        int RegisterCount = 0;
        int cbCapTmp = 2048;

        private int mfpWidth = 0;
        private int mfpHeight = 0;

        private bool dvNO = false;
        private bool dvOp = false;

        string ConString ;
        
        private System.Windows.Forms.Timer Timer;
        private System.Windows.Forms.Timer Timer2;

        const int MESSAGE_CAPTURED_OK = 0x0400 + 6;

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]

        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Config.ConfigMode m = new ConfigMode(); string i = "";
            m = (ConfigMode)Config.Config.read(out i);
            SqlConnectionStringBuilder ssb = new SqlConnectionStringBuilder();
            ssb.DataSource = $"{m.sqlconfig.IP},{m.sqlconfig.Port}";
            ssb.IntegratedSecurity = false;
            ssb.InitialCatalog = m.sqlconfig.DB;
            ssb.UserID = m.sqlconfig.UserID;
            ssb.Password = m.sqlconfig.password;
            ConString = ssb.ToString();
            
            this.FormBorderStyle = FormBorderStyle.None;     //设置窗体为无边框样式
            this.WindowState = FormWindowState.Maximized;    //最大化窗体 
            this.TopMost = true;

            Timer = new System.Windows.Forms.Timer();
            Timer.Interval = 1000;
            Timer.Tick += new EventHandler(timer_on);
            Timer.Start();

            Timer2 = new System.Windows.Forms.Timer();
            Timer2.Interval = 3000;
            Timer2.Tick += new EventHandler(timer_on2);
            Timer2.Start();

            FormHandle = this.Handle;
        }

        private void timer_on(object sender, EventArgs e)
        {
            this.label_time.Text = DateTime.Now.ToString("yyyy年MM月dd日 dddd");
            this.label_time2.Text = DateTime.Now.ToString("tt HH:mm:ss");
            textBox1.Focus();
        }

        private void timer_on2(object sender, EventArgs e)
        {
            this.name_lb.Text = " ";
            this.department_lb.Text = " ";
            this.job_title_lb.Text = " ";
            this.hint.Text = " ";
        }

        private void Init()
        {
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                int nCount = zkfp2.GetDeviceCount();
                if (nCount > 0)
                {
                    index = 0;
                    dvNO = true;
                }
                else
                {
                    zkfp2.Terminate();
                    //dvtxt.Text = "没有连接任何设备";
                }
            }
            else
            {
                //dvtxt.Text = "初始化失败";
            }
        }
    
        private void Open()
        {
            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(index)))
            {
                //dvtxt.Text = "打开设备失败";
                return;
            }
            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
            {
                //dvtxt.Text = "初始化数据库失败";
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }
            RegisterCount = 0;
            cbRegTmp = 0;
            for (int i = 0; i < 3; i++)
            {
                RegTmps[i] = new byte[2048];
            }
            byte[] paramValue = new byte[4];
            int size = 4;
            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);

            size = 4;
            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);

            FPBuffer = new byte[mfpWidth * mfpHeight];

            Thread captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            bIsTimeToDie = false;
            dvOp = true;
        }

        private void DoCapture()
        {
            while (!bIsTimeToDie)
            {
                cbCapTmp = 2048;
                int ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer, CapTmp, ref cbCapTmp);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    SendMessage(FormHandle, MESSAGE_CAPTURED_OK, IntPtr.Zero, IntPtr.Zero);
                } 
                System.Threading.Thread.Sleep(200);
            }
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case MESSAGE_CAPTURED_OK:
                    {
                        Timer2.Enabled = false;
                        Timer2.Enabled = true;

                        bool aon = false;
                        int i = 0;
                        DateTime t2 = DateTime.Now;
                        DataTable dt1 = new DataTable();

                        string sqlstr1 = "select bc_begin,bc_end from timehorizon";
                        DataTable dt_pd = new DataTable();
                        SqlCommand bc_cmd = SqlConn(sqlstr1);
                        SqlDataAdapter dap = new SqlDataAdapter();
                        dap.SelectCommand = bc_cmd;
                        dap.Fill(dt_pd);

                        TimeSpan Time_Begin = new TimeSpan();
                        TimeSpan Time_End = new TimeSpan();
                        TimeSpan ts_now = DateTime.Now.TimeOfDay;
                        for (i = 0; i<dt_pd.Rows.Count; i++)
                        { 
                            if (ts_now >= (TimeSpan)dt_pd.Rows[i]["bc_begin"] && ts_now <= (TimeSpan)dt_pd.Rows[i]["bc_end"])
                            {
                                Time_Begin = (TimeSpan)dt_pd.Rows[i]["bc_begin"];
                                Time_End = (TimeSpan)dt_pd.Rows[i]["bc_end"];
                                aon = true;
                                break;
                            }
                        }
                        int ret = zkfp.ZKFP_ERR_OK;
                        int fid = 0, score = 0;
                        ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                        if (zkfp.ZKFP_ERR_OK != ret)
                        {
                            this.name_lb.Text = " ";
                            this.department_lb.Text = " ";
                            this.job_title_lb.Text = " ";
                            hint.Text = "未找到指纹数据";
                            return;
                        }
                        if (aon)
                        {
                            if (zkfp.ZKFP_ERR_OK == ret)
                            {
                                string selectyg_id = string.Format("select amitabha from fingerprint where fingerprint_id ='") + fid + "'";
                                SqlCommand cmd = SqlConn(selectyg_id);
                                string yg_id = cmd.ExecuteScalar().ToString();
                                
                                string selectyg_xx = string.Format("select name, job_title, department, coding from employees where amitabha ='") + yg_id + "'";
                                SqlCommand cmd1 = SqlConn(selectyg_xx);
                                var reader = cmd1.ExecuteReader();
                                
                                DateTime dt_begin = new DateTime(t2.Year, t2.Month, t2.Day, Time_Begin.Hours, Time_Begin.Minutes, Time_Begin.Seconds);
                                DateTime dt_end = new DateTime(t2.Year, t2.Month, t2.Day, Time_End.Hours, Time_End.Minutes, Time_End.Seconds);
                                string s1 = dt_begin.ToString();
                                string s2 = dt_end.ToString();

                                string bc_str = $"select sign from baocan where amitabha = {yg_id}  and sign >= '{s1}' and sign <= '{s2}'";
                                SqlCommand cmd_bc = SqlConn(bc_str);
                                SqlDataAdapter dap1 = new SqlDataAdapter();
                                dap1.SelectCommand = cmd_bc;
                                dap1.Fill(dt1);

                                string name = "";
                                string job_title = "";
                                string department = "";
                                string coding = "";
                                string title = "";
                                DateTime dt = DateTime.Now;
                                while (reader.Read())
                                {
                                    name = (string)reader["name"];
                                    job_title = (string)reader["job_title"];
                                    department = (string)reader["department"];
                                    coding = (string)reader["coding"];
                                    name_lb.Text = name;
                                    job_title_lb.Text = job_title;
                                    department_lb.Text = department;
                                    hint.Text = DateTime.Now.ToString("yyyy年MM月dd日 dddd") + DateTime.Now.ToString(" tt HH:mm:ss ") + "报餐成功";
                                    //SpVoice sp = new SpVoice();
                                    //sp.Speak("报餐成功");
                                }

                                //判断是否重复报餐
                                if (dt1.Rows.Count > 0)
                                {
                                    return;
                                }
                                else
                                {
                                    string str1 = Time_Begin.ToString();
                                    string str2 = Time_End.ToString();
                                    SqlCommand cmd_title = SqlConn($"select title from timehorizon where bc_begin ='{Time_Begin}' and bc_end ='{Time_End}'");
                                    title = cmd_title.ExecuteScalar().ToString();

                                    SqlConnection ConFyh = new SqlConnection(ConString);
                                    string str_hys = "insert into baocan (amitabha,sign,name,job_title,department,coding,title) values (@yg_id, @dt, @name, @job_title, @department, @coding, @title)";
                                    SqlCommand cmdFyh = new SqlCommand(str_hys, ConFyh);

                                    SqlParameter parn = new SqlParameter("@yg_id", SqlDbType.Int);
                                    parn.Value = yg_id;
                                    cmdFyh.Parameters.Add(parn);

                                    parn = new SqlParameter("@dt", SqlDbType.DateTime);
                                    parn.Value = dt;
                                    cmdFyh.Parameters.Add(parn);

                                    parn = new SqlParameter("@name", SqlDbType.VarChar);
                                    parn.Value = name;
                                    cmdFyh.Parameters.Add(parn);

                                    parn = new SqlParameter("@job_title", SqlDbType.VarChar);
                                    parn.Value = job_title;
                                    cmdFyh.Parameters.Add(parn);

                                    parn = new SqlParameter("@department", SqlDbType.VarChar);
                                    parn.Value = department;
                                    cmdFyh.Parameters.Add(parn);

                                    parn = new SqlParameter("@coding", SqlDbType.VarChar);
                                    parn.Value = coding;
                                    cmdFyh.Parameters.Add(parn);

                                    parn = new SqlParameter("@title", SqlDbType.VarChar);
                                    parn.Value = title;
                                    cmdFyh.Parameters.Add(parn);

                                    ConFyh.Open();
                                    cmdFyh.ExecuteNonQuery();
                                    cmdFyh.Dispose();
                                    ConFyh.Close();
                                }
                                return;
                            }
                        }
                        else
                        {
                            hint.Text="未在报餐时间";
                            //SpVoice sp = new SpVoice();
                            //sp.Speak("未在报餐时间");
                            return;
                        }
                    }
                    break;

                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!dvNO)
            {
                Init();
            }
            if (!dvOp)
            {
                Open();
                selectAll();
            }
        }

        private void selectAll()
        {
            string selectfp = "select fingerprint_id,fpTemplate from fingerprint";
            SqlCommand cmd = SqlConn(selectfp);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = (int)reader["fingerprint_id"];
                string fp = (string)reader["fpTemplate"];
                byte[] blob = zkfp2.Base64ToBlob(fp);
                zkfp2.DBAdd(mDBHandle, id, blob);
            }
        }

        private SqlCommand SqlConn(string sqlStr)
        {
            SqlConnection conn = new SqlConnection(ConString);
            string selectStr = sqlStr;
            SqlCommand cmd = new SqlCommand(selectStr, conn);
            conn.Open();
            return cmd;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.Equals(textBox1.Text.Trim(), codings))
                {
                    code=textBox1.Text.Trim();

                    Timer2.Enabled = false;
                    Timer2.Enabled = true;

                    bool aon = false;
                    int i = 0;
                    DateTime t2 = DateTime.Now;
                    DataTable dt1 = new DataTable();

                    string sqlstr1 = "select bc_begin,bc_end from timehorizon";
                    DataTable dt_pd = new DataTable();
                    SqlCommand bc_cmd = SqlConn(sqlstr1);
                    SqlDataAdapter dap = new SqlDataAdapter();
                    dap.SelectCommand = bc_cmd;
                    dap.Fill(dt_pd);

                    TimeSpan Time_Begin = new TimeSpan();
                    TimeSpan Time_End = new TimeSpan();
                    TimeSpan ts_now = DateTime.Now.TimeOfDay;
                    for (i = 0; i < dt_pd.Rows.Count; i++)
                    {
                        if (ts_now >= (TimeSpan)dt_pd.Rows[i]["bc_begin"] && ts_now <= (TimeSpan)dt_pd.Rows[i]["bc_end"])
                        {
                            Time_Begin = (TimeSpan)dt_pd.Rows[i]["bc_begin"];
                            Time_End = (TimeSpan)dt_pd.Rows[i]["bc_end"];
                            aon = true;
                            break;
                        }
                    }
                    string selectyg_id = string.Format("select amitabha from employees where coding ='") + code + "'";
                    SqlCommand cmd = SqlConn(selectyg_id);
                    if (cmd.ExecuteScalar() == null)
                    {
                        this.name_lb.Text = " ";
                        this.department_lb.Text = " ";
                        this.job_title_lb.Text = " ";
                        hint.Text = "无效条码";
                        textBox1.Clear();
                        return;
                    }
                    if (aon)
                    {
                        string yg_id = cmd.ExecuteScalar().ToString();

                        string selectyg_xx = string.Format("select name, job_title, department, coding from employees where amitabha ='") + yg_id + "'";
                        SqlCommand cmd1 = SqlConn(selectyg_xx);
                        var reader = cmd1.ExecuteReader();

                        DateTime dt_begin = new DateTime(t2.Year, t2.Month, t2.Day, Time_Begin.Hours, Time_Begin.Minutes, Time_Begin.Seconds);
                        DateTime dt_end = new DateTime(t2.Year, t2.Month, t2.Day, Time_End.Hours, Time_End.Minutes, Time_End.Seconds);
                        string s1 = dt_begin.ToString();
                        string s2 = dt_end.ToString();
                        string bc_str = $"select sign from baocan where amitabha = {yg_id}  and sign >= '{s1}' and sign <= '{s2}'";
                        SqlCommand cmd_bc = SqlConn(bc_str);
                        SqlDataAdapter dap1 = new SqlDataAdapter();
                        dap1.SelectCommand = cmd_bc;
                        dap1.Fill(dt1);

                        string name = "";
                        string job_title = "";
                        string department = "";
                        string coding = "";
                        string title = "";
                        DateTime dt = DateTime.Now;
                        while (reader.Read())
                        {
                            name = (string)reader["name"];
                            job_title = (string)reader["job_title"];
                            department = (string)reader["department"];
                            coding = (string)reader["coding"];
                            name_lb.Text = name;
                            job_title_lb.Text = job_title;
                            department_lb.Text = department;
                            hint.Text = DateTime.Now.ToString("yyyy年MM月dd日 dddd") + DateTime.Now.ToString(" tt HH:mm:ss ") + "报餐成功";
                            //SpVoice sp = new SpVoice();
                            //sp.Speak("报餐成功");
                        }

                        //判断是否重复报餐
                        if (dt1.Rows.Count > 0)
                        {
                            textBox1.Clear();
                            return;
                        }
                        else
                        {
                            string str1 = Time_Begin.ToString();
                            string str2 = Time_End.ToString();
                            SqlCommand cmd_title = SqlConn($"select title from timehorizon where bc_begin ='{Time_Begin}' and bc_end ='{Time_End}'");
                            title = cmd_title.ExecuteScalar().ToString();

                            SqlConnection ConFyh = new SqlConnection(ConString);
                            string str_hys = "insert into baocan (amitabha,sign,name,job_title,department,coding,title) values (@yg_id, @dt, @name, @job_title, @department, @coding, @title)";
                            SqlCommand cmdFyh = new SqlCommand(str_hys, ConFyh);

                            SqlParameter parn = new SqlParameter("@yg_id", SqlDbType.Int);
                            parn.Value = yg_id;
                            cmdFyh.Parameters.Add(parn);

                            parn = new SqlParameter("@dt", SqlDbType.DateTime);
                            parn.Value = dt;
                            cmdFyh.Parameters.Add(parn);

                            parn = new SqlParameter("@name", SqlDbType.VarChar);
                            parn.Value = name;
                            cmdFyh.Parameters.Add(parn);

                            parn = new SqlParameter("@job_title", SqlDbType.VarChar);
                            parn.Value = job_title;
                            cmdFyh.Parameters.Add(parn);

                            parn = new SqlParameter("@department", SqlDbType.VarChar);
                            parn.Value = department;
                            cmdFyh.Parameters.Add(parn);

                            parn = new SqlParameter("@coding", SqlDbType.VarChar);
                            parn.Value = coding;
                            cmdFyh.Parameters.Add(parn);

                            parn = new SqlParameter("@title", SqlDbType.VarChar);
                            parn.Value = title;
                            cmdFyh.Parameters.Add(parn);

                            ConFyh.Open();
                            cmdFyh.ExecuteNonQuery();
                            cmdFyh.Dispose();
                            ConFyh.Close();
                        }
                    }
                    else
                    {
                        hint.Text = "未在报餐时间";
                        //SpVoice sp = new SpVoice();
                        //sp.Speak("未在报餐时间");
                    }
                }
                textBox1.Clear();
            }     
        }
    }//
}//

