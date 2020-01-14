using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Config;

namespace Demo
{
    public partial class Form3 : Form
    {
        string ConString;
        public Form3()
        {
            InitializeComponent();
        }

        public int Userid { get; set; }

        private void Form3_Load(object sender, EventArgs e)
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

            string str = "select * from timehorizon";
            SqlConnection conn = new SqlConnection(ConString);
            System.Data.DataTable dt = new DataTable();
            SqlCommand cmdFyh = new SqlCommand(str, conn);
            conn.Open();
            SqlDataAdapter DAp = new SqlDataAdapter();
            DAp.SelectCommand = cmdFyh;
            DAp.Fill(dt);
            comboBox1.DataSource = dt;
            comboBox1.ValueMember = "bc_begin";
            comboBox1.DisplayMember = "title";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TimeSpan ts = (TimeSpan)comboBox1.SelectedValue;
            string name = "";
            string job_title = "";
            string department = "";
            string coding = "";
            string title = "";
            bool resign_time = true;
            DateTime sign_time = DateTime.Parse($"{monthCalendar1.SelectionStart.ToShortDateString()} {ts.ToString()}" );
            DataTable dt1 = new DataTable();

            string chachong = $"select * from sign where amitabha = {Userid} and sign_time = '{sign_time}' and resign_time ='true'";
            SqlCommand cmd_chachong = SqlConn(chachong);
            SqlDataAdapter dap1 = new SqlDataAdapter();
            dap1.SelectCommand = cmd_chachong;
            dap1.Fill(dt1); 
            if (dt1.Rows.Count > 0)
            {
                return;
            }
            else
            {
                string selectyg_id = string.Format("select name, job_title, department, coding from employees where amitabha ='") + Userid + "'";
                SqlCommand cmd1 = SqlConn(selectyg_id);
                SqlConnection ConFyh = new SqlConnection(ConString);
                var reader = cmd1.ExecuteReader();

                while (reader.Read())
                {
                    name = (string)reader["name"];
                    job_title = (string)reader["job_title"];
                    department = (string)reader["department"];
                    coding = (string)reader["coding"];
                }

                string str_hys = "insert into sign (amitabha,sign_time,resign_time,name,job_title,department,coding) values (@Userid, @sign_time, @resign_time, @name, @job_title, @department, @coding)";
                SqlCommand cmdFyh = new SqlCommand(str_hys, ConFyh);

                SqlParameter parn = new SqlParameter("@Userid", SqlDbType.Int);
                parn.Value = Userid;
                cmdFyh.Parameters.Add(parn);

                parn = new SqlParameter("@sign_time", SqlDbType.DateTime);
                parn.Value = sign_time;
                cmdFyh.Parameters.Add(parn);

                parn = new SqlParameter("@resign_time", SqlDbType.Bit);
                parn.Value = resign_time;
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

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            label1.Text = monthCalendar1.SelectionStart.ToString("yyyy年MM月dd日")+ DateTime.Now.TimeOfDay;
        }
        private SqlCommand SqlConn(string sqlStr)
        {
            SqlConnection conn = new SqlConnection(ConString);
            string selectStr = sqlStr;
            SqlCommand cmd = new SqlCommand(selectStr, conn);
            conn.Open();
            return cmd;
        }
    }
}
