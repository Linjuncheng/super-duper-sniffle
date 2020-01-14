using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    public partial class frmExport : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public string t1 { get; set; }

        public string t2 { get; set; }

        public frmExport()
        {
            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            InitializeComponent();


            //利用反射设置DataGridView的双缓冲
            Type dgvType = this.dataGridView1.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);
        }

        private void InitHeaders()
        {
            for (int day = 1; day <= 37; day++)
            {
                this.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            }
            //写入表头
            this.dataGridView1.Rows.Add();
            this.dataGridView1.Rows[0].Cells[0].Value = "编码";
            this.dataGridView1.Rows[0].Cells[1].Value = "内部编码";
            this.dataGridView1.Rows[0].Cells[2].Value = "名称";
            this.dataGridView1.Rows[0].Cells[3].Value = "报餐统计";
            this.dataGridView1.Rows[0].Cells[4].Value = "就餐统计";
            this.dataGridView1.Rows[0].Cells[5].Value = "报餐就餐差";
            //写入表头日期
            for (int day = 1; day <= 31; day++)
            {
                this.dataGridView1.Rows[0].Cells[day + 5].Value = day;
            }
        }

        private IList<Attend> QueryAttendsFromDatabase()
        {
            string ic = "fyh";
            string ConString = "Data Source=192.168.88.96;Initial Catalog=fyh;Persist Security Info=True;User ID=test;Password=13579";
            try
            {
                using (SqlConnection ConFyh = new SqlConnection(ConString))
                {
                    string sql = string.Format("select a.[amitabha], a.[sign_time], a.[name], a.[coding], count(b.[name]) from [fyh].[dbo].[sign] as a left join [fyh].[dbo].[baocan] as b on a.name = b.name where (a.sign_time between " + "'" + t1 + "' " + "and " + "'" + t2 + "') and " +"(b.sign between " + "'" + t1 + "' and " + "'" + t2 + "')"  + "group by a.[amitabha], a.[sign_time], a.[name], a.[coding]");
                    SqlDataAdapter DAp = new SqlDataAdapter(sql, ConFyh);
                    MessageBox.Show(sql);
                    DataTable dt = new DataTable();
                    DAp.Fill(dt);
                    return dt.Rows.OfType<DataRow>()
                        .Select(x => new Attend(
                            Convert.ToInt32(x[0]),
                            Convert.ToDateTime(x[1]),
                            Convert.ToString(x[2]),
                            Convert.ToString(x[3]),
                            Convert.ToInt32(x[4])
                            )).ToList();
                }
            }
            //返回异常
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new List<Attend>();
            }
        }

        private void frmExport_Load(object sender, EventArgs e)
        {
            this.InitHeaders();
            //测试数据
            //var allAttend = new List<Attend>()
            //{
            //    new Attend("0001",new DateTime(2020,1,1)),
            //    new Attend("0001",new DateTime(2020,1,1)),
            //    new Attend("0002",new DateTime(2020,1,2)),
            //    new Attend("0002",new DateTime(2020,1,2)),
            //    new Attend("0001",new DateTime(2020,1,2))
            //};

            var allAttend = QueryAttendsFromDatabase();
            var ids = allAttend.Select(x => x.Id).Distinct().ToList();
            var idss = allAttend.Select(x => x.Name).Distinct().ToList();
            var idsss = allAttend.Select(x => x.Name).GroupBy(x => x).ToList();
            var idssss = allAttend.Select(x => x.UserID).Distinct().ToList();
            var idsssss = allAttend.Select(x => x.Name_1).ToList();
            if (idss.Count == 0)
            {
                MessageBox.Show("数据为空，请重新选择日期", "提示", MessageBoxButtons.OK);
                //Forms1.f3.Close();
            }
            else
            {
                for (int index = 0; index < idss.Count; index++)
                {
                    
                    try
                    {
                        string id = ids[index];
                        string name = idss[index];
                        int cname = idsss[index].Count();
                        int userid = idssss[index];
                        int cname_1 = idsssss[index];
                        var dates = allAttend.Where(x => x.Name == name).Select(x => x.Date);
                        this.dataGridView1.Rows.Add();
                        int row = index + 1;
                        dataGridView1.Update();
                        this.dataGridView1.Rows[row].Cells[0].Value = id;
                        this.dataGridView1.Rows[row].Cells[1].Value = userid;
                        this.dataGridView1.Rows[row].Cells[2].Value = name;
                        this.dataGridView1.Rows[row].Cells[3].Value = cname_1;
                        this.dataGridView1.Rows[row].Cells[4].Value = cname;
                        for (int day = 1; day <= 31; day++)
                        {
                            int cnt = dates.Count(x => x.Day == day);
                            this.dataGridView1.Rows[row].Cells[day + 5].Value = cnt;
                        }
                        dataGridView1.EndEdit();
                    }
                    catch
                    (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
            }
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public class Attend
        {
            public Attend(int userID, DateTime date, string name, string id, int name_1)
            {
                this.Id = id;
                this.Date = date;
                this.Name = name;
                this.UserID = userID;
                this.Name_1 = name_1;
            }

            public string Name { get; set; }
            public int Name_1 { get; set; }
            public DateTime Date { get; set; }
            public int UserID { get; set; }
            public string Id { get; set; }
        }

        private void 导出数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            if (path.ShowDialog() == DialogResult.OK)
            {
                string txtPath = path.SelectedPath;
                string year = DateTime.Now.ToString("yyyyMMddHHmmss");
                //通过IO创建文件myExcel
                FileInfo file = new FileInfo(txtPath + "/" + $"统计报表{year}.xlsx");

                //创建ExcelPackage对象，这个对象是面对工作簿的，就是里面的所有
                using (ExcelPackage myExcelPackage = new ExcelPackage(file))
                {
                    //创建ExcelWorkSheet对象，这个对象就是面对表的，是工作簿中单个表
                    ExcelWorksheet worksheet = myExcelPackage.Workbook.Worksheets.Add(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    //坐标1，1赋值A1就是相当于在Excel中的A1位置赋值了一个A1字符串。

                    worksheet.Cells[1, 1].Value = "导出日期：" + DateTime.Now.ToString("yyyy-MM-dd");
                    worksheet.Cells.Style.ShrinkToFit = true;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1].Value = Convert.ToString(dataGridView1[j, i].Value);
                        }
                        myExcelPackage.Save();

                    }
                    MessageBox.Show("导出完成");
                }
                //save方法就保存我们这个对象，他就会去执行我们刚刚赋值的那些东西
            }
        }
    }
}

