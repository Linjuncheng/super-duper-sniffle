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
    public partial class login : Form
    {
        string ConString = "Data Source=WIN-0MGTRP6ACV5\\MSSQL;Initial Catalog=fyh;User ID=sa;Password=123456";

        public bool bo = false;
        public login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnection ConFyh = new SqlConnection(ConString);
            string CmdString = string.Format("select password, adminname from admin where adminname like @username and password like @password");
            SqlCommand cmdFyh = new SqlCommand(CmdString, ConFyh);
            ConFyh.Open();

            SqlParameter pr = new SqlParameter();

            pr.ParameterName = "@username";
            pr.SqlDbType = SqlDbType.VarChar;
            pr.SqlValue = textBox1.Text.Trim();

            cmdFyh.Parameters.Add(pr);

            pr = new SqlParameter();

            pr.ParameterName = "@password";
            pr.SqlDbType = SqlDbType.VarChar;
            pr.SqlValue = textBox2.Text.Trim();

            cmdFyh.Parameters.Add(pr);

            SqlDataAdapter DAp = new SqlDataAdapter();
            DataTable dt = new DataTable();

            DAp.SelectCommand = cmdFyh;

            DAp.Fill(dt);

            if (dt.Rows.Count != 0)
            {
                //forms1.runbool = true;
                //this.close();

                bo = true;

                Forms1.f1.Show();
                this.Hide();

            }
            else
            {
                MessageBox.Show("用户名密码错误");
            }
            //ConFyh.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void login_Load(object sender, EventArgs e)
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
        }
    }
}
