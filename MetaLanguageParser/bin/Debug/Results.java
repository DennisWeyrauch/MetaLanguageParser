using java.util.*;
public class Program {
	public static void main(String[] args) {
		if ((i < 15) & (i > 0)) {
			while ((++i) < 15) {
				System.out.print(i);
			}
		} else {
			do {
				System.out.print(i);
			} while (i > 0);
		}
	}
}