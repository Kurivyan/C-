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
using System.Collections;
using System.Text.Json;

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

	abstract class V1Data : IEnumerable<DataItem> {
		public string Key {
			get;
			set;
		}
		public DateTime Date {
			get;
			set;
		}

		public abstract int xLength {
			get;
		}

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

		public abstract IEnumerator<DataItem> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
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
		public override IEnumerator<DataItem> GetEnumerator() {
			return new MyListEnumerator(this.MyList);
		}

		public class MyListEnumerator : IEnumerator<DataItem> {
			private List<DataItem> _list;
			private int _position = -1;
			public MyListEnumerator(List<DataItem> list) {
				_list = list;
			}
			public bool MoveNext() {
				_position++;
				return _position < _list.Count;
			}
			public void Reset() {
				_position = -1;
			}
			public DataItem Current {
				get {
					return _list[_position];
				}
			}
			object IEnumerator.Current {
				get {
					return Current;
				}
			}
			public void Dispose() {}
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
	
		public override IEnumerator<DataItem> GetEnumerator() {
			return new MyArrayEnumerator(this.MyArray, this.MyArray2);
		}

		public class MyArrayEnumerator : IEnumerator<DataItem> {
			private double[] _array;
			private System.Numerics.Complex[] _array2;
			private int _position = -1;
			public MyArrayEnumerator(double[] array, System.Numerics.Complex[] array2) {
				_array = array;
				_array2 = array2;
			}
			public bool MoveNext() {
				_position++;
				return _position < _array.Length;
			}
			public void Reset() {
				_position = -1;
			}
			public DataItem Current {
				get {
					return new DataItem(_array[_position], _array2[_position * 2], _array2[_position * 2 + 1]);
				}
			}
			object IEnumerator.Current {
				get {
					return Current;
				}
			}
			public void Dispose() {}
		}
	
		public bool Save(string filename){
			try{
				using (StreamWriter sw = new StreamWriter(filename)){
					var options = new JsonSerializerOptions();
					sw.WriteLine(this.Key);
					sw.WriteLine(this.Date);

					foreach(var i in this.MyArray){
						sw.Write(i + "\t");
					}
					sw.Write("\n");
					foreach(var i in this.MyArray2){
						sw.Write(i + "\t");
					}
				}
				return true;
			} catch (Exception e){
				Console.WriteLine(e.Message);
				return false;
			}
		}

		public static bool Load(string filename, ref V1DataArray ptr){
			try{
				using (StreamReader sr = new StreamReader(filename)){
					if (ptr != null)
					{
						ptr.Key = sr.ReadLine()!;
						ptr.Date = DateTime.Parse(sr.ReadLine()!);

						string[] xValues = sr.ReadLine()!.Split('\t', StringSplitOptions.RemoveEmptyEntries);
						ptr.MyArray = new double[xValues.Length];
						for (int i = 0; i < xValues.Length; i++)
						{
							ptr.MyArray[i] = double.Parse(xValues[i]);
						}

						string[] complex_cord = sr.ReadLine()!.Split('\t', StringSplitOptions.RemoveEmptyEntries);
						ptr.MyArray2 = new System.Numerics.Complex[complex_cord.Length];
						for(int i = 0; i < complex_cord.Length; i++){
							string[] numbers = complex_cord[i].Trim('<', '>').Split(';');
							ptr.MyArray2[i] = new System.Numerics.Complex(double.Parse(numbers[0]), double.Parse(numbers[1]));
						}
					}
				}
				return true;
			} catch (Exception e){
				Console.WriteLine(e.Message);
				ptr.MyArray = Array.Empty<double>();
				ptr.MyArray2 = Array.Empty<System.Numerics.Complex>();
				return false;
			}
		}
	}
	class V1MainCollection : List<V1Data>, IEnumerable<V1Data> {
		
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
					rand_data[j] = rand.Next(1, 10);
				this.Add(new V1DataArray($"key{rand.Next(1, 100)}", DateTime.Now));
			}

			for(int i = 0; i < nL; i++){
				int length_m_list = rand.Next(2, 5);
				double[] rand_data = new double[length_m_list];
				for(int j = 0; j < length_m_list; j++)
					rand_data[j] = rand.Next(1, 10);
				this.Add(new V1DataList($"key{rand.Next(1, 100)}", DateTime.Now));
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
		IEnumerator<V1Data> IEnumerable<V1Data>.GetEnumerator() {
			return new V1MainCollectionEnumerator(this);
		}


		public IEnumerator<DataItem> GetEnumerator() {
			foreach (var data in this) {
				foreach (var item in data) {
					yield return item;
				}
			}
		}

		public class V1MainCollectionEnumerator : IEnumerator<V1Data> {
			private V1MainCollection _collection;
			private int _position = -1;
			public V1MainCollectionEnumerator(V1MainCollection collection) {
				_collection = collection;
			}
			public bool MoveNext(){
				_position++;
				return _position < _collection.Count;
			}
			public void Reset(){
				_position = -1;
			}
			public V1Data Current{
				get{
					return _collection[_position];
				}
			}
			object IEnumerator.Current {
				get{
					return Current;
				}
			}
			public void Dispose(){}
		}

		public double FindMaxValue {
			get {
				int count = 0;
				foreach (var data in this)
				{
					if (data.xLength != 0)
					{
						count++;
					}
				}
				return (count == 0 || this.Count == 0) ? -1 : this.OfType<V1Data>()
					.SelectMany(data => data)
					.Max(item => item.Y1.Magnitude);
			}
		}
		public IEnumerable<double>? ascending_querry {
			get {
				int count = 0;
				foreach (var data in this)
				{
					if (data.xLength != 0)
					{
						count++;
					}
				}
				return (count == 0 || this.Count == 0) ? null : this.OfType<V1Data>()
					.SelectMany(data => data)
					.GroupBy(item => item.X)
					.Where(group => group.Count() > 1)
					.Select(group => group.Key)
					.OrderBy(x => x);
			}
		}
	}

	class Program {
		static void Main(string[] args) {
			//TestSaveLoad();
			//TestV1MainCollection();

			V1MainCollection new_obj = new V1MainCollection(3, 4);
			foreach(var item in new_obj){
				Console.WriteLine(item.ToLongString("F2"));
			}
		}

		static void TestSaveLoad() {
			V1DataArray dataArray = new V1DataArray("testKey", DateTime.Now, new double[] { 1.0, 2.0, 3.0 }, V1DataArray.F);
			string filename = "testData.txt";
			if (dataArray.Save(filename)) {
				Console.WriteLine("Data saved successfully.");
			} else {
				Console.WriteLine("Failed to save data.");
			}

			V1DataArray loadedDataArray = new V1DataArray("dummyKey", DateTime.Now);
			if (V1DataArray.Load(filename, ref loadedDataArray)) {
				Console.WriteLine("Data loaded successfully.");
				Console.WriteLine(loadedDataArray.ToLongString("F2"));
			} else {
				Console.WriteLine("Failed to load data.");
			}
		}

		static void TestV1MainCollection() {
			V1MainCollection collection = new V1MainCollection(0, 0);
			Console.WriteLine("Collection:");
			Console.WriteLine(collection.ToLongString("F2"));

			Console.WriteLine($"Max Value: {collection.FindMaxValue}");
			var ascendingQuery = collection.ascending_querry;
			if (ascendingQuery != null) {
				Console.WriteLine("Ascending Query:");
				foreach (var x in ascendingQuery) {
					Console.WriteLine(x);
				}
			} else {
				Console.WriteLine("No elements in ascending query.");
			}
		}
	}
}
