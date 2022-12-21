using System;
using System.Drawing;
using System.Windows.Forms;

namespace FloatingHorizon
{
    public partial class Form1 : Form
    {
        Horizon horizon;
        Graphics graphics;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.BackColor = Color.White;
            graphics = pictureBox1.CreateGraphics();
            horizon = new Horizon(pictureBox1.Width, pictureBox1.Height, graphics);
            string[] functions = { "z=sin(x^2)+cos(y^2)" ,
                "z=cos(cos(y)-sin(x))",
                "z=cos(sqrt(x^2+y^2))",
                "z=sin(x+y)",
                "z=e^sin(sqrt(x^2+y^2))",
                "z=x^2-y^2",
                "z=x^2+y^2" };
            comboBox1.Items.AddRange(functions);
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetUpHorizon();
            horizon.Draw();
        }
        public void SetUpHorizon()
        {
            horizon.xMin = double.Parse(textBox1.Text);
            horizon.xMax = double.Parse(textBox2.Text);
            horizon.yMin = double.Parse(textBox3.Text);
            horizon.yMax = double.Parse(textBox4.Text);
            horizon.xStep = double.Parse(textBox5.Text);
            horizon.yStep = double.Parse(textBox6.Text);
            horizon.func = GetFunctionValue;
        }
        public double GetFunctionValue(double x, double y)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: return Math.Sin(x * x) + Math.Cos(y * y);
                case 1: return Math.Cos(Math.Cos(y) - Math.Sin(x));
                case 2: return Math.Cos(Math.Sqrt(x * x + y * y));
                case 3: return Math.Sin(x + y);
                case 4: return Math.Exp(Math.Sin(Math.Sqrt(x * x + y * y)));
                case 5: return x * x - y * y;
                case 6: return x * x + y * y;
            }
            return double.NaN;
        }
    }

}