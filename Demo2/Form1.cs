using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using libzkfpcsharp;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Sample;
using System.Data.SqlClient;
using Config;

namespace Demo
{
    public partial class Form1 : Form
    {
        
        private bool dvNO = false;
        private bool dvOp = false;

        string ConString ;

        IntPtr mDevHandle = IntPtr.Zero;
        IntPtr mDBHandle = IntPtr.Zero;
        IntPtr FormHandle = IntPtr.Zero;
        bool bIsTimeToDie = false;
        bool IsRegister = false;
        bool bIdentify = true;
        byte[] FPBuffer;
        int RegisterCount = 0;
        const int REGISTER_FINGER_COUNT = 3;

        byte[][] RegTmps = new byte[3][];
        byte[] RegTmp = new byte[2048];
        byte[] CapTmp = new byte[2048];
        int cbCapTmp = 2048;
        int cbRegTmp = 0;
        int iFid = 1;

        private int mfpWidth = 0;
        private int mfpHeight = 0;

        const int MESSAGE_CAPTURED_OK = 0x0400 + 6;

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]

        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public Form1()
        {
            InitializeComponent();
        }


        private void bnInit_Click(object sender, EventArgs e)
        {
            cmbIdx.Items.Clear();
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                int nCount = zkfp2.GetDeviceCount();
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        cmbIdx.Items.Add(i.ToString());
                    }
                    cmbIdx.SelectedIndex = 0;
                    bnInit.Enabled = false;
                    bnFree.Enabled = true;
                    bnOpen.Enabled = true;
                }
                else
                {
                    zkfp2.Terminate();
                    MessageBox.Show("No device connected!");
                }
            }
            else
            {
                MessageBox.Show("Initialize fail, ret=" + ret + " !");
            }
        }

        private void bnFree_Click(object sender, EventArgs e)
        {
            zkfp2.Terminate();
            cbRegTmp = 0;
            bnInit.Enabled = true;
            bnFree.Enabled = false;
            bnOpen.Enabled = false;
            bnClose.Enabled = false;
            bnEnroll.Enabled = false;
            bnVerify.Enabled = false;
            bnIdentify.Enabled = false;
        }

        private void bnOpen_Click(object sender, EventArgs e)
        {
            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(cmbIdx.SelectedIndex)))
            {
                MessageBox.Show("OpenDevice fail");
                return;
            }
            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
            {
                MessageBox.Show("Init DB fail");
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }
            bnInit.Enabled = false;
            bnFree.Enabled = true;
            bnOpen.Enabled = false;
            bnClose.Enabled = true;
            bnEnroll.Enabled = true;
            bnVerify.Enabled = true;
            bnIdentify.Enabled = true;
            RegisterCount = 0;
            cbRegTmp = 0;
            iFid = 1;
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

            FPBuffer = new byte[mfpWidth*mfpHeight];

            Thread captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            bIsTimeToDie = false;
            dvtxt.Text = "Open succ";
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
                Thread.Sleep(200);
            }
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case MESSAGE_CAPTURED_OK:
                    {
                        MemoryStream ms = new MemoryStream();
                        BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms);
                        Bitmap bmp = new Bitmap(ms);
                        this.picFPImg.Image = bmp;

                        if (IsRegister)
                        {
                            int ret = zkfp.ZKFP_ERR_OK;
                            int fid = 0, score = 0;
                            ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                            if (zkfp.ZKFP_ERR_OK == ret)
                            {
                                dvtxt.Text = "该指纹已注册，指纹id为 " + fid + "!";
                                return;
                            }
                            if (RegisterCount > 0 && zkfp2.DBMatch(mDBHandle, CapTmp, RegTmps[RegisterCount - 1]) <= 0)
                            {
                                dvtxt.Text = "请用同一根手指按3次进行注册！";
                                return;
                            }
                            Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
                            String strBase64 = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
                            byte[] blob = zkfp2.Base64ToBlob(strBase64);
                            RegisterCount++;
                            if (RegisterCount >= REGISTER_FINGER_COUNT)
                            {
                                RegisterCount = 0;
                                if (zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
                                       zkfp.ZKFP_ERR_OK == (ret = zkfp2.DBAdd(mDBHandle, iFid, RegTmp)))
                                {
                                    iFid++;
                                    dvtxt.Text = "注册成功！";
                                    //--------------指纹模板插入数据库---------------
                                    String hys = zkfp2.BlobToBase64(RegTmp,cbRegTmp);
                                    try
                                    {
                                        int userid=0;

                                        if (dataGridView2.SelectedRows.Count > 0)
                                        {
                                            userid = (int)dataGridView2.SelectedRows[0].Cells["Fg_Column5"].Value;

                                            SqlConnection ConFyh = new SqlConnection(ConString);
                                            string str_hys = "insert into fingerprint (fpTemplate,amitabha) values (@hys, @userid)";
                                            
                                            SqlCommand cmdFyh = new SqlCommand(str_hys, ConFyh);
                                            SqlParameter parn = new SqlParameter("@hys", SqlDbType.Text);
                                            parn.Value = hys;
                                            cmdFyh.Parameters.Add(parn);

                                            parn = new SqlParameter("@userid", SqlDbType.Int);
                                            parn.Value = userid;
                                            cmdFyh.Parameters.Add(parn);

                                            ConFyh.Open();
                                            cmdFyh.ExecuteNonQuery();
                                            cmdFyh.Dispose();
                                            ConFyh.Close();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                                else
                                {
                                    dvtxt.Text = "注册失败, 错误代码=" + ret;
                                }
                                IsRegister = false;
                                return;
                            }
                            else
                            {
                                dvtxt.Text = "你需要按 " + (REGISTER_FINGER_COUNT - RegisterCount) + " 次指纹";
                            }
                        }
                        else
                        {
                            if (cbRegTmp <= 0)
                            {
                                dvtxt.Text = "请先注册你的指纹!";
                                return;
                            }
                            if (bIdentify)
                            {
                                int ret = zkfp.ZKFP_ERR_OK;
                                int fid = 0, score = 0;
                                ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                                if (zkfp.ZKFP_ERR_OK == ret)
                                {
                                    dvtxt.Text = "Identify succ, fid= " + fid + ",score=" + score + "!";
                                    return;
                                }
                                else
                                {
                                    dvtxt.Text = "Identify fail, ret= " + ret;
                                    return;
                                }
                            }
                            else
                            {
                                int ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);
                                if (0 < ret)
                                {
                                    dvtxt.Text = "Match finger succ, score=" + ret + "!";
                                    return;
                                }
                                else
                                {
                                    dvtxt.Text = "Match finger fail, ret= " + ret;
                                    return;
                                }
                            }
                        }
                    }
                    break;

                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
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

            FormHandle = this.Handle;
        }
        
        private void bnClose_Click(object sender, EventArgs e)
        {
            bIsTimeToDie = true;
            RegisterCount = 0;
            Thread.Sleep(1000);
            zkfp2.CloseDevice(mDevHandle);
            bnInit.Enabled = false;
            bnFree.Enabled = true;
            bnOpen.Enabled = true;
            bnClose.Enabled = false;
            bnEnroll.Enabled = false;
            bnVerify.Enabled = false;
            bnIdentify.Enabled = false;
        }

        private void bnEnroll_Click(object sender, EventArgs e)
        {
            
            if (!IsRegister)
            {
                if (dataGridView2.SelectedRows.Count <= 0) return;
                string id = dataGridView2.SelectedRows[0].Cells["Fg_Column5"].Value.ToString();
                IsRegister = true;
                RegisterCount = 0;
                cbRegTmp = 0;
                dvtxt.Text = "请按3次手指!";
            }
        }

        private void bnIdentify_Click(object sender, EventArgs e)
        {
            if (!bIdentify)
            {
                bIdentify = true;
                dvtxt.Text = "请按一下指纹!";
            }
        }

        private void bnVerify_Click(object sender, EventArgs e)
        {
            if (bIdentify)
            {
                bIdentify = false;
                dvtxt.Text = "Please press your finger!";
            }
        }

        private void button1_Click(object sender, EventArgs e)//查询所有
        {
            try
            {
                string str_time1 = dateTimePicker1.Value.ToString();
                string str_time2 = dateTimePicker2.Value.ToString();
                //创建数据库连接并打开
                SqlConnection ConFyh = new SqlConnection(ConString);
                string CmdString = $"select name, job_title, department, amitabha, coding, sign_time, resign_time = case when resign_time = 1 then '是' else '否' end from sign where sign_time >= '{str_time1}' and sign_time <= '{str_time2}'";
                SqlCommand cmdFyh = new SqlCommand(CmdString, ConFyh);
                ConFyh.Open();
                DataTable dt = new DataTable();
                if (cmdFyh.ExecuteScalar() == null)
                {
                    DataTable dt_clear = (DataTable)dataGridView1.DataSource;
                    if (dt_clear != null)
                    {
                        dt_clear.Rows.Clear();
                        DG_Time.DataSource = dt_clear;
                    }
                    MessageBox.Show("未查询到数据!");
                    return;
                }
                SqlDataAdapter DAp = new SqlDataAdapter();
                
                DAp.SelectCommand = cmdFyh;
                DAp.Fill(dt);
                dataGridView1.DataSource = dt;
                ConFyh.Close();
            }
            //返回异常
            catch (Exception ex)
            {
                
            }
        }

        private void button4_Click(object sender, EventArgs e)//查询单个
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("请输入姓名!");
                return;
            }
            string str_time1 = dateTimePicker1.Value.ToString();
            string str_time2 = dateTimePicker2.Value.ToString();
            string name = textBox1.Text;
            SqlConnection conn = new SqlConnection(ConString);
            string selectOne = $"select name, job_title, department, amitabha, coding, sign_time, resign_time = case when resign_time = 1 then '是' else '否' end from sign where name = '{name}' and sign_time >= '{str_time1}' and sign_time <= '{str_time2}'";
            SqlCommand cmdFyh = new SqlCommand(selectOne, conn);
            conn.Open();
            if (cmdFyh.ExecuteScalar() == null)
            {
                DataTable dt_clear = (DataTable)dataGridView1.DataSource;
                if (dt_clear != null)
                {
                    dt_clear.Rows.Clear();
                    DG_Time.DataSource = dt_clear;
                }
                MessageBox.Show("未查询到数据!");
                return;
            }
            SqlDataAdapter DAp = new SqlDataAdapter();
            DataTable dt = new DataTable();
            DAp.SelectCommand = cmdFyh;
            DAp.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }

        private void Init()
        {
            cmbIdx.Items.Clear();
            int ret = zkfperrdef.ZKFP_ERR_OK;
            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                int nCount = zkfp2.GetDeviceCount();
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        cmbIdx.Items.Add(i.ToString());
                    }
                    cmbIdx.SelectedIndex = 0;
                    bnInit.Enabled = false;
                    bnFree.Enabled = true;
                    bnOpen.Enabled = true;
                    dvNO = true;
                }
                else
                {
                    zkfp2.Terminate();
                    dvtxt.Text = "没有连接任何设备";
                }
            }
            else
            {
                dvtxt.Text = "初始化失败";
            }
        }

        private void Open()
        {
            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(cmbIdx.SelectedIndex)))
            {
                dvtxt.Text = "打开设备失败";
                return;
            }
            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
            {
                dvtxt.Text = "初始化数据库失败";
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }

            bnInit.Enabled = false;
            bnFree.Enabled = true;
            bnOpen.Enabled = false;
            bnClose.Enabled = true;
            bnEnroll.Enabled = true;
            bnVerify.Enabled = true;
            bnIdentify.Enabled = true;
            RegisterCount = 0;
            cbRegTmp = 0;
            iFid = 1;
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
            dvtxt.Text = "打开成功";
            dvtxt.Text = "设备已连接！";
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
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (textBox_name.Text == "")
                {
                    SqlConnection conn = new SqlConnection(ConString);
                    string selectAll = string.Format("select name, job_title, department, coding, amitabha from employees");
                    SqlCommand cmdFyh = new SqlCommand(selectAll, conn);
                    conn.Open();
                    SqlDataAdapter DAp = new SqlDataAdapter();
                    DataTable dt = new DataTable();
                    DAp.SelectCommand = cmdFyh;
                    DAp.Fill(dt);
                    dataGridView2.DataSource = dt;
                }
                else
                {
                    string name = textBox_name.Text;
                    SqlConnection conn = new SqlConnection(ConString);
                    string selectOne = string.Format("select name, job_title, department, coding, amitabha from employees where name ='") + name + "'";
                    SqlCommand cmdFyh = new SqlCommand(selectOne, conn);
                    conn.Open();
                    SqlDataAdapter DAp = new SqlDataAdapter();
                    DataTable dt = new DataTable();
                    DAp.SelectCommand = cmdFyh;
                    DAp.Fill(dt);
                    dataGridView2.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool conflict(string bc_b, string bc_e, string jc_b, string jc_e)
        {
            string selectAll = "select * from timehorizon";
            SqlConnection conn = new SqlConnection(ConString);
            SqlCommand select_cmd = new SqlCommand(selectAll, conn);
            conn.Open();
            var reader = select_cmd.ExecuteReader();
            while (reader.Read())
            {
                TimeSpan bc_begin = DateTime.Parse(reader["bc_begin"].ToString()).TimeOfDay;//数据库里的报餐开始
                TimeSpan bc_end = DateTime.Parse(reader["bc_end"].ToString()).TimeOfDay;//数据库里的报餐结束
                TimeSpan jc_begin = DateTime.Parse(reader["jc_begin"].ToString()).TimeOfDay;//数据库里的就餐开始
                TimeSpan jc_end = DateTime.Parse(reader["jc_end"].ToString()).TimeOfDay;//数据库里的就餐结束
                TimeSpan conf_bb = DateTime.Parse(bc_b).TimeOfDay;//传进来的报餐开始
                TimeSpan conf_be = DateTime.Parse(bc_e).TimeOfDay;//传进来的报餐 结束
                TimeSpan conf_jb = DateTime.Parse(jc_b).TimeOfDay;//传进来的就餐开始
                TimeSpan conf_je = DateTime.Parse(jc_e).TimeOfDay;//传进来的就餐结束
                if ((conf_bb <= bc_begin && conf_be >= bc_end) || (conf_jb <= jc_begin && conf_je >= jc_end))
                {
                    return false;
                }
                if ((conf_be < bc_end && conf_be >= bc_begin) || (conf_jb < jc_end && conf_je >= jc_begin))
                {
                    return false;
                }
                if ((conf_bb <= bc_end && conf_bb >= bc_begin) || (conf_jb <= jc_end && conf_jb >= jc_begin))
                {
                    return false;
                }
            }
            return true;
        }

        private void button6_Click(object sender, EventArgs e)//保存
        {

            if (this.textBox_title.Text == "")
            {
                MessageBox.Show("请填写就餐类型");
                return;
            }
            string bc_begin = nud_1.Value.ToString() + ":" + nud_2.Value.ToString();
            string bc_end = nud_3.Value.ToString() + ":" + nud_4.Value.ToString();
            string jc_begin = nud_5.Value.ToString() + ":" + nud_6.Value.ToString();
            string jc_end = nud_7.Value.ToString() + ":" + nud_8.Value.ToString();
            if (conflict(bc_begin, bc_end, jc_begin, jc_end)){
                SqlConnection conn = new SqlConnection(ConString);
                string insertTime = "insert into timehorizon (title,bc_begin,bc_end,jc_begin,jc_end) values (@title,@bc_begin,@bc_end,@jc_begin,@jc_end)";
                conn.Open();
                SqlCommand insert_cmd = new SqlCommand(insertTime, conn);
                insert_cmd.Parameters.AddRange(
                    new SqlParameter[] {
                new SqlParameter("@title",SqlDbType.VarChar,50) {Value = textBox_title.Text },
                new SqlParameter("@bc_begin",SqlDbType.Time) {Value =  bc_begin},
                new SqlParameter("@bc_end",SqlDbType.Time) {Value =  bc_end},
                new SqlParameter("@jc_begin",SqlDbType.Time) {Value = jc_begin},
                new SqlParameter("@jc_end",SqlDbType.Time) {Value = jc_end}
                    });
                if (insert_cmd.ExecuteNonQuery() > 0) Select_AllTime_Click(sender, e);
                insert_cmd.Dispose();
                conn.Close();
            }else
            {
                MessageBox.Show("时间冲突!");
            }

        }

        private void Select_AllTime_Click(object sender, EventArgs e)
        {
            string selectAll = "select * from timehorizon";
            SqlConnection conn = new SqlConnection(ConString);
            SqlCommand cmdFyh = new SqlCommand(selectAll, conn);
            conn.Open();
            SqlDataAdapter DAp = new SqlDataAdapter();
            DataTable dt = new DataTable();
            DAp.SelectCommand = cmdFyh;
            DAp.Fill(dt);
            DG_Time.DataSource = dt;
        }

        private void DG_Time_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            this.textBox_title.Text = DG_Time.SelectedRows[0].Cells["time_title"].Value.ToString();

            this.nud_1.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_bc_begin"].Value.ToString().Substring(0,2));
            this.nud_2.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_bc_begin"].Value.ToString().Substring(3, 2));
            this.nud_3.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_bc_end"].Value.ToString().Substring(0, 2));
            this.nud_4.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_bc_end"].Value.ToString().Substring(3, 2));

            this.nud_5.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_jc_begin"].Value.ToString().Substring(0, 2));
            this.nud_6.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_jc_begin"].Value.ToString().Substring(3, 2));
            this.nud_7.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_jc_end"].Value.ToString().Substring(0, 2));
            this.nud_8.Value = Convert.ToInt32(DG_Time.SelectedRows[0].Cells["time_jc_end"].Value.ToString().Substring(3, 2));
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            this.textBox_title.Text = "";

            this.nud_1.Value = 0;
            this.nud_2.Value = 0;
            this.nud_3.Value = 0;
            this.nud_4.Value = 0;
            this.nud_5.Value = 0;
            this.nud_6.Value = 0;
            this.nud_7.Value = 0;
            this.nud_8.Value = 0;

            DataTable dt_clear = (DataTable)DG_Time.DataSource;
            if (dt_clear != null)
            {
                dt_clear.Rows.Clear();
                DG_Time.DataSource = dt_clear;
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.textBox3.Text == "")
            {
                MessageBox.Show("请填写姓名!");
                return;
            }
            string name = textBox3.Text;
            SqlConnection conn = new SqlConnection(ConString);
            string selectOne = string.Format($"select name, job_title, department, coding, amitabha from employees where name = '{name}'");
            SqlCommand cmdFyh = new SqlCommand(selectOne, conn);
            conn.Open();
            if (cmdFyh.ExecuteScalar() != null)
            {
                SqlDataAdapter DAp = new SqlDataAdapter();
                DataTable dt = new DataTable();
                DAp.SelectCommand = cmdFyh;
                DAp.Fill(dt);
                dataGridView_buka.DataSource = dt;
            }
            else
            {
                MessageBox.Show("查无此人!");
            }
            conn.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView_buka.SelectedRows.Count <= 0)
            {
                MessageBox.Show("请选择一行信息!");
                return;
            }
            Form3 f3 = new Form3();
            f3.Userid = Convert.ToInt32(dataGridView_buka.SelectedRows[0].Cells["amitabha"].Value);
            f3.Show();
        }

        private void button6_Click_2(object sender, EventArgs e)
        {
            try
            {
                string id = DG_Time.SelectedRows[0].Cells["time_id"].Value.ToString();
                string selectAll = "delete from timehorizon where id = " + id;
                SqlConnection conn = new SqlConnection(ConString);
                SqlCommand cmdFyh = new SqlCommand(selectAll, conn);
                conn.Open();
                cmdFyh.ExecuteNonQuery();
                Select_AllTime_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("请先选择需要删除的行");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            frmExport fe = new frmExport();
            fe.t1 = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            fe.t2 = dateTimePicker2.Value.ToString("yyyy-MM-dd");

            //dataGridView1.Update();
            fe.Show();
            //dataGridView1.EndEdit();
        }
    }//
}
