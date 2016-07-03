import java.util.*;


public class Program {
	public static void main(String[] args) {
		String enemy1;
		String enemy2;
		int dist1;
		int dist2;
		while (true) {
			enemy1 = System.in.readln();
			dist1 = System.in.readln();
			enemy2 = System.in.readln();
			dist2 = System.in.readln();
			if (dist1 < dist2) {
				System.out.println(enemy1);
			} else {
				System.out.println(enemy2);
			}
		}
	}
	

}
