using System;
using System.IO;
using System.Text;
public class Program {
	public static void main(string[] args) {
		String enemy1;
		String enemy2;
		int dist1;
		int dist2;
		while (true) {
			enemy1 = System.Console.ReadLine();
			dist1 = System.Console.ReadLine();
			enemy2 = System.Console.ReadLine();
			dist2 = System.Console.ReadLine();
			if (dist1 < dist2) {
				System.Console.WriteLine(enemy1);
			} else {
				System.Console.WriteLine(enemy2);
			}
		}
	}
	

}
