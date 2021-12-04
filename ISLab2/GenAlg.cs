using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ISLab2
{
	class Point // Класс точек
	{
		public double X;
		public double Y;
		public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }


	class Person // Класс особей
	{
		public double A;
		public double B;
		public BitArray GenA;	// Битовая строка для кодирования гена a
		public BitArray GenB;	// Битовая строка для кодирования гена b
		public double Fitness;	// Значение функции приспособленности (целевой функции)
	    
		public Person(double min, double max, double a, double b) // Конструктор, принимающий декодированные параметры (для формирования начальной популяции)
		{
			A = a;
			int EncodedA = (int)Math.Round((a - min) * (Math.Pow(2, Interval.GenRank) - 1) / (max - min));
			GenA = new BitArray(new int[] { EncodedA });
			GenA.Length = Interval.GenRank;
			B = b;
			int EncodedB = (int)Math.Round((b - min) * (Math.Pow(2, Interval.GenRank) - 1) / (max - min));
			GenB = new BitArray(new int[] { EncodedB });
			GenB.Length = Interval.GenRank;
			FitnessFunc();
		}

		public Person(BitArray genA, BitArray genB) // Конструктор, принимающий закодированные параметры (для смены родителей потомками)
		{
			A = Interval.DecodedGen(genA);
			B = Interval.DecodedGen(genB);
			GenA = genA;
			GenB = genB;
			FitnessFunc();
		}
		
		public void FitnessFunc()	// Функция приспособленности (целевая)
		{
			double Sum = 0;
			Point[] pointsP = new Point[Interval.x2 - Interval.x1 + 1];
			for (int i = 0; i < pointsP.Length; i++)
			{
				pointsP[i] = new Point(Interval.x1 + i, A * Math.Exp(B * (Interval.x1 + i)));
				Sum += Math.Pow(Interval.points[i].Y - pointsP[i].Y, 2);
			}
		
			Fitness = Math.Sqrt(Sum / pointsP.Length) / Math.Abs(Interval.MaxY  - Interval.MinY);
		}
		
		public BitArray BuildChromosome() // Функция склеивания генов в хромосому
		{
			bool[] chromosome = new bool[Interval.GenRank * 2];
			GenA.CopyTo(chromosome, 0);
			GenB.CopyTo(chromosome, Interval.GenRank);
			return new BitArray(chromosome);
		}

	}


	class Interval
	{
		public const int GenRank = 8;	// Разрядность параметров хромосомы (длина битовой строки)
		public const double Min = 0;	// Условные концы интервала значений a и b
		public const double Max = 1;
		public static int x1 = 0;		// Условные концы интервала значений x
		public static int x2 = 9;
		public static double MinY = 100000;	// Условные концы интервала значений x
		public static double MaxY = 0;

		public static Point[] points = new Point[x2 - x1 + 1];
		public static double DecodedGen(BitArray gen)
		{
			int[] array = new int[1];
			gen.CopyTo(array, 0);
			return (array[0] * (Max - Min)) / (Math.Pow(2, GenRank) - 1) + Min;
		}
	}


	class Population
	{
		static int n = 30;   // Размер (количество особей) популяции

		public Person[] persons;
		public Population()
		{
			persons = new Person[n];
		}
		
		public Person Selection() // Функция поиска самой приспособленной особи
		{
			double MinFitness = persons[0].Fitness;
			int PersonIndex = 0;
			for (int i = 1; i < persons.Length; i++)
			{
				if (persons[i].Fitness < MinFitness)
				{
					PersonIndex = i;
					MinFitness = persons[PersonIndex].Fitness;
				}
			}
			return persons[PersonIndex];
		}

		public void Assessment(double UserFitness)		// Функция оценки популяции
		{
			Random random = new Random();
			
			while (Selection().Fitness > UserFitness)	//СЕЛЕКЦИЯ
			{
				Person[] parent = new Person[persons.Length];
				
				for (int i = 0; i < parent.Length; i++)
				{
					int Index1 = random.Next(n);
					var applicant = persons[Index1];	// Выбор двух случайных особей из популяции
					int Index2 = random.Next(n);
					while (Index2 == Index1)
					{
						Index2 = random.Next(n);
					}
					var applicant2 = persons[Index2];
					if (applicant.Fitness > applicant2.Fitness)		// Отбор одной из них для селекции
					{
						parent[i] = applicant2;
					}
					else
					{
						parent[i] = applicant;
					}
				}
				
				for (int i = 0; i < parent.Length; i += 2)		//СКРЕЩИВАНИЕ
				{
					BitArray chromosome1 = parent[i].BuildChromosome();
					BitArray chromosome2 = parent[i + 1].BuildChromosome();
					
					int CrossoverBreak = random.Next(chromosome1.Length - 1) + 1;	// Определяем случайную точку разрыва
					BitArray child1 = new BitArray(Interval.GenRank * 2);
					BitArray child2 = new BitArray(Interval.GenRank * 2);

					for (int j = 0; j < CrossoverBreak; j++)	// Переносим хромосомы родителей в потомков до т.разрыва
					{
						child1.Set(j, chromosome1[j]);
						child2.Set(j, chromosome2[j]);
					}

					for (int k = CrossoverBreak; k < Interval.GenRank * 2; k++)	// После т.разрыва меняем родителей местами
					{
						child1.Set(k, chromosome2[k]);
						child2.Set(k, chromosome1[k]);
						
						Mutation(child1);	//МУТАЦИЯ
						Mutation(child2);
					}

					BitArray GenAChild1 = new BitArray(Interval.GenRank);
					BitArray GenBChild1 = new BitArray(Interval.GenRank);
					BitArray GenAChild2 = new BitArray(Interval.GenRank);
					BitArray GenBChild2 = new BitArray(Interval.GenRank);
					
					for (int y = 0; y < Interval.GenRank; y++) // Разбиваем хромосомы каждого потомка на два гена
					{
						GenAChild1.Set(y, child1[y]);
						GenBChild1.Set(y, child1[y + Interval.GenRank]);
						GenAChild2.Set(y, child2[y]);
						GenBChild2.Set(y, child2[y + Interval.GenRank]);
					}
					
					persons[i] = new Person(GenAChild1, GenBChild1);    // Заменяем в популяции родителей потомками
					persons[i + 1] = new Person(GenAChild2, GenBChild2);
				}
			}
		}
		
		public void Mutation(BitArray child)	// Функция генерации мутации
		{
			Random random = new Random();
			double Pm = Math.Pow(child.Length, -1); // Вероятность мутации (30^(-1) = 1/30 = 0,033)

			for (int i = 0; i < Interval.GenRank * 2; i++)
			{
				double rand = random.Next(100)/100;
				if (Pm > rand)
				{
					child.Set(i, !child[i]);
				}
			}
		}
	}
}
