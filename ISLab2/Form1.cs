using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ISLab2
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			chart1.Series[0].Points.Clear();
			chart1.Series[1].Points.Clear();
			dataGridView1.Rows.Clear();
			dataGridView2.Rows.Clear();
			
			double AA = (double)numericA.Value;
			double BB = (double)numericB.Value;
			
			WriteRes(AA, BB, dataGridView1, 0);

			for (int i = 0; i < Interval.points.Length; i++)	//Находим max и min значения y
			{
				Interval.points[i] = new Point(Interval.x1 + i, AA * Math.Exp(BB * (Interval.x1 + i)));
			}
			for (int i = 0; i < Interval.points.Length; i++)
			{
				if (Interval.points[i].Y > Interval.MaxY)
				{
					Interval.MaxY = Interval.points[i].Y;
				}
				if (Interval.points[i].Y < Interval.MinY)
				{
					Interval.MinY = Interval.points[i].Y;
				}
			}

			Random random = new Random();

			// ФОРМИРОВАНИЕ НАЧАЛЬНОЙ ПОПУЛЯЦИИ
			Population population = new Population();	
			for (int i = 0; i < population.persons.Length; i++)
			{
				double a = random.NextDouble();
				double b = random.NextDouble();
				population.persons[i] = new Person(Interval.Min, Interval.Max, a, b);
			}

			//ОЦЕНИВАНИЕ ПОПУЛЯЦИИ
			double UserFitness = (double)numericF.Value;	
			population.Assessment(UserFitness);
			var result = population.Selection();

			//ВЫВОД РЕЗУЛЬТАТА
			label6.Text = "Подобранная a: ";
			label7.Text = "Подобранная b: ";
			labelA.Text = Convert.ToString(Math.Round(result.A, 4));
			labelB.Text = Convert.ToString(Math.Round(result.B, 4));
			WriteRes(result.A, result.B, dataGridView2,1);
						
		}

		void WriteRes(double A, double B, DataGridView dataGridView, int seriesNumber)	// Функция заполнения таблицы и вывода графика
		{
			for(int x = 0; x < 10; x++)
			{
				double y = A * Math.Exp(B * x);
				dataGridView.Rows.Add(x, y);
				chart1.Series[seriesNumber].Points.AddXY(x, y);
			}
		}
	}
}
