namespace Winforms_App_Template
{
    public partial class Main_Form : Form
    {
        public Main_Form()
        {
            InitializeComponent();
            timer1.Tick += Timer1_Tick;
        }

        private void Timer1_Tick(object? sender, EventArgs e)
        {
            progressBar1.PerformStep();
            label1.Text = progressBar1.Value.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

    }
}
 