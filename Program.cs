using System.Collections.Generic;
using System;
using System.Reflection;
using System.Collections.Frozen;
using System.Xml.Schema;
using System.IO.Pipes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using Microsoft.VisualBasic;
using System.Data;
using System.ComponentModel;

namespace Programm {
	struct DataItem {
		public double X {
			get;
			set;
		}
		public System.Numerics.Complex Y1 {
			get;
			set;
		}
		public System.Numerics.Complex Y2 {
			get;
			set;
		}
		public DataItem(double x, System.Numerics.Complex y1, System.Numerics.Complex y2) {
			X = x;
			Y1 = y1;
			Y2 = y2;
		}
		public string ToString(string format) {
			return $"Узел {X.ToString(format)}, Вектор {Y1.Real.ToString(format)} + {Y1.Imaginary.ToString(format)}i : {Y2.Real.ToString(format)} + {Y2.Imaginary.ToString(format)}i";
		}
		public override string ToString() {
			return $"Узел {X.ToString()}, Вектор {Y1.ToString()} : {Y2.ToString()}";
		}
	}

	abstract class V1Data {
		public string Key {
			get;
			set;
		}
		public DateTime Date {
			get;
			set;
		}

		private int _xLength;
		public abstract int xLength {
			get;
		}

		private(double, double) _MinMaxDifference;
		public abstract(double, double) MinMaxDifference {
			get;
		}

		public abstract string ToLongString(string format);
		public override string ToString() {
			return $"Key : {Key} | Date : {Date}\n";
		}

		public V1Data(string key, DateTime date) {
			Key = key;
			Date = date;
		}
	}

	class V1DataList: V1Data {
		public List < DataItem > MyList {
			get;
			set;
		}

		public override int xLength {
			get {
				return MyList.Count;
			}
		}

		public V1DataList(string key, DateTime date): base(key, date) {
			MyList = new List < DataItem > ();
		}


		public static FDI F_int = (double x) => { // целочисленные значения
			var rand = new System.Random();
			return (new System.Numerics.Complex(rand.Next(1, 10), rand.Next(1, 10)), new System.Numerics.Complex(rand.Next(1, 10), rand.Next(1, 10)));
		};
		public static FDI F = (double x) => { // вещественные значения
			var rand = new System.Random();
			return (new System.Numerics.Complex(rand.NextDouble() * 10, rand.NextDouble() * 10), new System.Numerics.Complex(rand.NextDouble() * 10, rand.NextDouble() * 10));
		};
		public V1DataList(string key, DateTime date, double[] x, FDI F): base(key, date) {
			double[] x1 = x.Distinct().ToArray();
			MyList = new List < DataItem > ();
			for (int i = 0; i < x1.Length; i++) {
				foreach(var node in MyList!) {
					if (node.X == x1[i])
						continue;
				}

				DataItem ptr = new DataItem(x1[i], 0, 0);
				(System.Numerics.Complex, System.Numerics.Complex) temp = F(x1[i]);
				ptr.Y1 = temp.Item1;
				ptr.Y2 = temp.Item2;
				MyList.Add(ptr);
			}
		}

		public override(double, double) MinMaxDifference {
			get {
				double min = Double.MaxValue;
				double max = Double.MinValue;
				foreach(var pear in MyList) {
					double difference = Math.Abs(System.Numerics.Complex.Abs(pear.Y1) - System.Numerics.Complex.Abs(pear.Y2));
					if (difference > max) {
						max = difference;
					}
					if (difference < min) {
						min = difference;
					}
				}
				return (min, max);
			}
		}

		public override string ToString() {
			return "ArrayList : \n\t" + base.ToString() + $"\n\tCount : {xLength} \n";
		}

		public override string ToLongString(string format) {
			string return_str = String.Empty;
			int i = 1;
			foreach(var node in MyList) {
				return_str += $"\n\t Node {i} : {node.X.ToString(format)} | {node.Y1.ToString(format)} : {node.Y2.ToString(format)}\n";
				i++;
			}
			return ToString() + return_str;
		}

		public static explicit operator V1DataArray(V1DataList source){
			V1DataArray res_arr = new V1DataArray(source.Key, source.Date);
			res_arr.MyArray = new double[source.xLength];
			res_arr.MyArray2 = new System.Numerics.Complex[source.xLength * 2];
			int iterator = 0;
			foreach(var node in source.MyList){
				res_arr.MyArray[iterator / 2] = node.X;
				res_arr.MyArray2[iterator] = node.Y1;
				res_arr.MyArray2[iterator + 1] = node.Y2;
				iterator += 2;
			}
			return res_arr;
		}
		public delegate(System.Numerics.Complex, System.Numerics.Complex) FDI(double x);
		public IEnumerator < DataItem > GetEnumerator() {
			return MyList.GetEnumerator();
		}
	}

	class V1DataArray: V1Data {
		public double[] MyArray {
			get;
			set;
		}
		public System.Numerics.Complex[] MyArray2 {
			get;
			set;
		}

		public V1DataArray(string key, DateTime date): base(key, date) {
			MyArray = Array.Empty < double > ();
			MyArray2 = Array.Empty < System.Numerics.Complex > ();
		}


		public static FValues F_int = (double x) => {
			var rand = new System.Random();
			return (new System.Numerics.Complex(rand.Next(1, 10), rand.Next(1, 10)), new System.Numerics.Complex(rand.Next(1, 10), rand.Next(1, 10)));
		};
		public static FValues F = (double x) => {
			var rand = new System.Random();
			return (new System.Numerics.Complex(rand.NextDouble() * 10, rand.NextDouble() * 10), new System.Numerics.Complex(rand.NextDouble() * 10, rand.NextDouble() * 10));
		};
		public V1DataArray(string key, DateTime data, double[] x, FValues F): base(key, data) {
			double[] x1 = x.Distinct().ToArray();
			MyArray = new double[x1.Length];
			MyArray2 = new System.Numerics.Complex[x1.Length * 2];

			Array.Copy(x1, MyArray, x1.Length);
			for (var i = 0; i < MyArray2.Length; i += 2) {
				MyArray2[i] = F(x1[i / 2]).Item1;
				MyArray2[i + 1] = F(x1[i / 2]).Item2;
			}
		}

		public DataItem ? this[int item] {
			get {
				if (item >= MyArray.Length) {
					return null;
				}
				return new DataItem(MyArray[item], MyArray2[item * 2], MyArray2[item * 2 + 1]);
			}
		}

		public override int xLength {
			get {
				return MyArray.Length;
			}
		}

		public override(double, double) MinMaxDifference {
			get {
				double min = Double.MaxValue;
				double max = Double.MinValue;
				for (var i = 0; i < MyArray2.Length; i += 2) {
					double difference = Math.Abs(System.Numerics.Complex.Abs(MyArray2[i]) - System.Numerics.Complex.Abs(MyArray2[i + 1]));
					if (difference > max)
						max = difference;
					if (difference < min)
						min = difference;
				}
				return (min, max);
			}
		}

		public override string ToString() {
			return "V1DataArray : \n\t" + base.ToString() + "\n";
		}
		public override string ToLongString(string format) {
			string ret_str = String.Empty;
			for(var i = 0; i < MyArray2.Length; i += 2){
				ret_str += $"\tNode {MyArray[i/2].ToString(format)} : {MyArray2[i].ToString(format)} , {MyArray2[i+1].ToString(format)}\n";
			}
			return this.ToString() + ret_str;
		}
		public delegate(System.Numerics.Complex, System.Numerics.Complex) FValues(double x);
	}

	class V1MainCollection : List<V1Data> {
		
		public V1Data ? this[string str]{
			get {
				foreach(var item in this){
					if (str == item.Key)
						return item;
				}
				return null;
			}
		}
		public new bool Add(V1Data v1Data){
			if(this.Count == 0)
				base.Add(v1Data);
			foreach(var item in this){
				if (item.Key == v1Data.Key || item.Date == v1Data.Date)
					return false;
				base.Add(v1Data);
				return true;
			}
			return false;
		}

		public V1MainCollection(int nA, int nL){
			System.Random rand = new Random();
			for(int i = 0; i < nA; i++){
				int length_m_arr = rand.Next(2, 5);
				double[] rand_data = new double[length_m_arr];
				for(int j = 0; j < length_m_arr; j++)
					rand_data[j] = rand.NextDouble() * 10;
				this.Add(new V1DataArray($"key{rand.Next(1, 100)}", DateTime.Now, rand_data, V1DataArray.F_int));
			}

			for(int i = 0; i < nL; i++){
				int length_m_list = rand.Next(2, 5);
				double[] rand_data = new double[length_m_list];
				for(int j = 0; j < length_m_list; j++)
					rand_data[j] = rand.NextDouble() * 10;
				this.Add(new V1DataList($"key{rand.Next(1, 100)}", DateTime.Now, rand_data, V1DataList.F_int));
			}
		}

		public string ToLongString(string format){
			string ret_str = String.Empty;
			foreach(var item in this){
				ret_str += item.ToLongString(format) + "\n";
			}
			return ret_str;
		}

		public override string ToString() {
			string ret_str = String.Empty;
			foreach(var item in this){
				ret_str += item.ToString() + "\n";
			}
			return ret_str;
		}
	}

	class Program {
		static void Main(string[] args) {
				V1DataList new_list = new V1DataList("test", DateTime.Now, [1, 2, 2, 2 , 2], V1DataList.F_int);
				System.Console.WriteLine(new_list.ToLongString("F2")); 
			}
		}
	}