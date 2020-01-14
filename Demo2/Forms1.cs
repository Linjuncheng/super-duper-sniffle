namespace Demo
{
    class Forms1
    {
        public static Form1 f1 = new Form1();
        public static Form2 f2 = new Form2();
        public static frmExport f3 = new frmExport();

        public static bool RunBool = false;
        public static bool LoginBool = false;

        public static void ShowForm3()
        {
            try
            {
                f3.Show();
            }
            catch
            {
                f3 = new Demo.frmExport();
                f3.Show();
            }
        }

        public static void ShowForm1()
        {
            try
            {
                f1.Show();
            }
            catch
            {
                f1 = new Demo.Form1();
                f1.Show();
            }
        }
    }
}
